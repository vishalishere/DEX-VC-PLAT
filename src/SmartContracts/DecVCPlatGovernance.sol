// SPDX-License-Identifier: MIT
// © 2024 DecVCPlat. All rights reserved.

pragma solidity ^0.8.19;

import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "./DecVCPlatToken.sol";

/**
 * @title DecVCPlatGovernance
 * @dev Governance contract for DecVCPlat project voting and funding decisions
 * Features:
 * - Project proposal voting with token staking
 * - Milestone-based funding releases
 * - Weighted voting based on staked tokens
 * - Time-locked voting periods
 * - Anti-reentrancy protection
 * - Emergency pause functionality
 */
contract DecVCPlatGovernance is ReentrancyGuard, Ownable, Pausable {
    
    DecVCPlatToken public immutable dvcpToken;
    
    // Proposal states
    enum ProposalState {
        Pending,
        Active,
        Succeeded,
        Failed,
        Executed,
        Cancelled
    }
    
    // Vote choices
    enum VoteChoice {
        Against,
        For,
        Abstain
    }
    
    // Proposal structure
    struct Proposal {
        uint256 id;
        address proposer;
        string title;
        string description;
        uint256 fundingAmount;
        uint256 startTime;
        uint256 endTime;
        uint256 forVotes;
        uint256 againstVotes;
        uint256 abstainVotes;
        uint256 totalStaked;
        ProposalState state;
        mapping(address => Vote) votes;
        mapping(address => uint256) stakedAmounts;
        address[] voters;
        bool executed;
    }
    
    // Vote structure
    struct Vote {
        bool hasVoted;
        VoteChoice choice;
        uint256 votingPower;
        uint256 stakedAmount;
        uint256 timestamp;
    }
    
    // Milestone structure for funding releases
    struct Milestone {
        uint256 proposalId;
        string description;
        uint256 fundingAmount;
        bool completed;
        bool approved;
        uint256 approvalVotes;
        uint256 rejectionVotes;
        uint256 voteEndTime;
        mapping(address => bool) hasVoted;
    }
    
    // Configuration
    uint256 public constant VOTING_PERIOD = 7 days;
    uint256 public constant MILESTONE_VOTING_PERIOD = 3 days;
    uint256 public constant QUORUM_PERCENTAGE = 10; // 10% of total staked tokens
    uint256 public constant APPROVAL_THRESHOLD = 51; // 51% approval needed
    uint256 public constant MIN_PROPOSAL_STAKE = 1000 * 10**18; // 1000 DVCP tokens
    
    // State variables
    uint256 public proposalCount;
    uint256 public milestoneCount;
    mapping(uint256 => Proposal) public proposals;
    mapping(uint256 => Milestone) public milestones;
    mapping(address => uint256) public stakedForVoting;
    
    // Events
    event ProposalCreated(
        uint256 indexed proposalId,
        address indexed proposer,
        string title,
        uint256 fundingAmount
    );
    
    event VoteCast(
        address indexed voter,
        uint256 indexed proposalId,
        VoteChoice choice,
        uint256 votingPower,
        uint256 stakedAmount
    );
    
    event ProposalExecuted(uint256 indexed proposalId, uint256 fundingAmount);
    
    event StakeForVoting(address indexed user, uint256 amount);
    event UnstakeFromVoting(address indexed user, uint256 amount);
    
    event MilestoneCreated(
        uint256 indexed milestoneId,
        uint256 indexed proposalId,
        string description,
        uint256 fundingAmount
    );
    
    event MilestoneCompleted(uint256 indexed milestoneId);
    event MilestoneApproved(uint256 indexed milestoneId, uint256 fundingReleased);
    
    constructor(address _dvcpToken, address initialOwner) {
        dvcpToken = DecVCPlatToken(_dvcpToken);
        _transferOwnership(initialOwner);
    }
    
    /**
     * @dev Create a new project proposal
     * @param title Proposal title
     * @param description Proposal description
     * @param fundingAmount Amount of funding requested
     */
    function createProposal(
        string calldata title,
        string calldata description,
        uint256 fundingAmount
    ) external nonReentrant whenNotPaused {
        require(bytes(title).length > 0, "DecVCPlat: title cannot be empty");
        require(bytes(description).length > 0, "DecVCPlat: description cannot be empty");
        require(fundingAmount > 0, "DecVCPlat: funding amount must be positive");
        require(
            dvcpToken.balanceOf(msg.sender) >= MIN_PROPOSAL_STAKE,
            "DecVCPlat: insufficient balance for proposal"
        );
        
        // Stake tokens for proposal
        dvcpToken.transferFrom(msg.sender, address(this), MIN_PROPOSAL_STAKE);
        
        proposalCount++;
        Proposal storage newProposal = proposals[proposalCount];
        newProposal.id = proposalCount;
        newProposal.proposer = msg.sender;
        newProposal.title = title;
        newProposal.description = description;
        newProposal.fundingAmount = fundingAmount;
        newProposal.startTime = block.timestamp;
        newProposal.endTime = block.timestamp + VOTING_PERIOD;
        newProposal.state = ProposalState.Active;
        
        emit ProposalCreated(proposalCount, msg.sender, title, fundingAmount);
    }
    
    /**
     * @dev Stake tokens to participate in voting
     * @param amount Amount of tokens to stake
     */
    function stakeForVoting(uint256 amount) external nonReentrant whenNotPaused {
        require(amount > 0, "DecVCPlat: cannot stake 0 tokens");
        
        dvcpToken.transferFrom(msg.sender, address(this), amount);
        stakedForVoting[msg.sender] += amount;
        
        emit StakeForVoting(msg.sender, amount);
    }
    
    /**
     * @dev Unstake tokens from voting (after voting period ends)
     * @param amount Amount of tokens to unstake
     */
    function unstakeFromVoting(uint256 amount) external nonReentrant whenNotPaused {
        require(amount > 0, "DecVCPlat: cannot unstake 0 tokens");
        require(stakedForVoting[msg.sender] >= amount, "DecVCPlat: insufficient staked amount");
        
        stakedForVoting[msg.sender] -= amount;
        dvcpToken.transfer(msg.sender, amount);
        
        emit UnstakeFromVoting(msg.sender, amount);
    }
    
    /**
     * @dev Cast a vote on a proposal
     * @param proposalId ID of the proposal
     * @param choice Vote choice (Against, For, Abstain)
     * @param stakeAmount Amount of tokens to stake for this vote
     */
    function castVote(
        uint256 proposalId,
        VoteChoice choice,
        uint256 stakeAmount
    ) external nonReentrant whenNotPaused {
        require(proposalExists(proposalId), "DecVCPlat: proposal does not exist");
        
        Proposal storage proposal = proposals[proposalId];
        require(proposal.state == ProposalState.Active, "DecVCPlat: proposal not active");
        require(block.timestamp <= proposal.endTime, "DecVCPlat: voting period ended");
        require(!proposal.votes[msg.sender].hasVoted, "DecVCPlat: already voted");
        require(stakeAmount > 0, "DecVCPlat: must stake tokens to vote");
        require(
            stakedForVoting[msg.sender] >= stakeAmount,
            "DecVCPlat: insufficient staked tokens"
        );
        
        // Record the vote
        proposal.votes[msg.sender] = Vote({
            hasVoted: true,
            choice: choice,
            votingPower: stakeAmount,
            stakedAmount: stakeAmount,
            timestamp: block.timestamp
        });
        
        proposal.stakedAmounts[msg.sender] = stakeAmount;
        proposal.voters.push(msg.sender);
        proposal.totalStaked += stakeAmount;
        
        // Update vote tallies
        if (choice == VoteChoice.For) {
            proposal.forVotes += stakeAmount;
        } else if (choice == VoteChoice.Against) {
            proposal.againstVotes += stakeAmount;
        } else {
            proposal.abstainVotes += stakeAmount;
        }
        
        // Lock staked tokens for this proposal
        stakedForVoting[msg.sender] -= stakeAmount;
        
        emit VoteCast(msg.sender, proposalId, choice, stakeAmount, stakeAmount);
    }
    
    /**
     * @dev Execute a proposal after voting period ends
     * @param proposalId ID of the proposal to execute
     */
    function executeProposal(uint256 proposalId) external nonReentrant whenNotPaused {
        require(proposalExists(proposalId), "DecVCPlat: proposal does not exist");
        
        Proposal storage proposal = proposals[proposalId];
        require(proposal.state == ProposalState.Active, "DecVCPlat: proposal not active");
        require(block.timestamp > proposal.endTime, "DecVCPlat: voting period not ended");
        require(!proposal.executed, "DecVCPlat: proposal already executed");
        
        // Check quorum and approval
        uint256 totalVotes = proposal.forVotes + proposal.againstVotes + proposal.abstainVotes;
        uint256 quorumThreshold = (dvcpToken.totalSupply() * QUORUM_PERCENTAGE) / 100;
        
        if (totalVotes >= quorumThreshold && proposal.forVotes > proposal.againstVotes) {
            proposal.state = ProposalState.Succeeded;
            proposal.executed = true;
            
            // Release initial funding (could be integrated with funding service)
            emit ProposalExecuted(proposalId, proposal.fundingAmount);
        } else {
            proposal.state = ProposalState.Failed;
        }
        
        // Return staked tokens to voters
        _returnVoterStakes(proposalId);
    }
    
    /**
     * @dev Create a milestone for a successful proposal
     * @param proposalId ID of the parent proposal
     * @param description Milestone description
     * @param fundingAmount Funding amount to release upon milestone completion
     */
    function createMilestone(
        uint256 proposalId,
        string calldata description,
        uint256 fundingAmount
    ) external onlyOwner {
        require(proposalExists(proposalId), "DecVCPlat: proposal does not exist");
        require(proposals[proposalId].state == ProposalState.Succeeded, "DecVCPlat: proposal not successful");
        
        milestoneCount++;
        Milestone storage milestone = milestones[milestoneCount];
        milestone.proposalId = proposalId;
        milestone.description = description;
        milestone.fundingAmount = fundingAmount;
        milestone.completed = false;
        milestone.approved = false;
        
        emit MilestoneCreated(milestoneCount, proposalId, description, fundingAmount);
    }
    
    /**
     * @dev Mark milestone as completed (by project founder or owner)
     * @param milestoneId ID of the milestone
     */
    function completeMilestone(uint256 milestoneId) external {
        require(milestoneExists(milestoneId), "DecVCPlat: milestone does not exist");
        
        Milestone storage milestone = milestones[milestoneId];
        uint256 proposalId = milestone.proposalId;
        
        require(
            msg.sender == proposals[proposalId].proposer || msg.sender == owner(),
            "DecVCPlat: not authorized"
        );
        require(!milestone.completed, "DecVCPlat: milestone already completed");
        
        milestone.completed = true;
        milestone.voteEndTime = block.timestamp + MILESTONE_VOTING_PERIOD;
        
        emit MilestoneCompleted(milestoneId);
    }
    
    /**
     * @dev Vote on milestone approval
     * @param milestoneId ID of the milestone
     * @param approve Whether to approve the milestone
     */
    function voteOnMilestone(uint256 milestoneId, bool approve) external {
        require(milestoneExists(milestoneId), "DecVCPlat: milestone does not exist");
        
        Milestone storage milestone = milestones[milestoneId];
        require(milestone.completed, "DecVCPlat: milestone not completed");
        require(block.timestamp <= milestone.voteEndTime, "DecVCPlat: voting period ended");
        require(!milestone.hasVoted[msg.sender], "DecVCPlat: already voted");
        require(stakedForVoting[msg.sender] > 0, "DecVCPlat: must have staked tokens");
        
        milestone.hasVoted[msg.sender] = true;
        uint256 votingPower = stakedForVoting[msg.sender];
        
        if (approve) {
            milestone.approvalVotes += votingPower;
        } else {
            milestone.rejectionVotes += votingPower;
        }
        
        // Check if milestone is approved
        if (block.timestamp > milestone.voteEndTime || 
            milestone.approvalVotes > milestone.rejectionVotes) {
            
            if (milestone.approvalVotes > milestone.rejectionVotes) {
                milestone.approved = true;
                emit MilestoneApproved(milestoneId, milestone.fundingAmount);
            }
        }
    }
    
    /**
     * @dev Internal function to return staked tokens to voters
     * @param proposalId ID of the proposal
     */
    function _returnVoterStakes(uint256 proposalId) internal {
        Proposal storage proposal = proposals[proposalId];
        
        for (uint256 i = 0; i < proposal.voters.length; i++) {
            address voter = proposal.voters[i];
            uint256 stakedAmount = proposal.stakedAmounts[voter];
            
            if (stakedAmount > 0) {
                stakedForVoting[voter] += stakedAmount;
                proposal.stakedAmounts[voter] = 0;
            }
        }
    }
    
    /**
     * @dev Get proposal information
     * @param proposalId ID of the proposal
     */
    function getProposal(uint256 proposalId) external view returns (
        uint256 id,
        address proposer,
        string memory title,
        string memory description,
        uint256 fundingAmount,
        uint256 startTime,
        uint256 endTime,
        uint256 forVotes,
        uint256 againstVotes,
        uint256 abstainVotes,
        ProposalState state
    ) {
        require(proposalExists(proposalId), "DecVCPlat: proposal does not exist");
        
        Proposal storage proposal = proposals[proposalId];
        return (
            proposal.id,
            proposal.proposer,
            proposal.title,
            proposal.description,
            proposal.fundingAmount,
            proposal.startTime,
            proposal.endTime,
            proposal.forVotes,
            proposal.againstVotes,
            proposal.abstainVotes,
            proposal.state
        );
    }
    
    /**
     * @dev Get voting information for a user on a proposal
     * @param proposalId ID of the proposal
     * @param voter Address of the voter
     */
    function getVote(uint256 proposalId, address voter) external view returns (
        bool hasVoted,
        VoteChoice choice,
        uint256 votingPower,
        uint256 stakedAmount
    ) {
        require(proposalExists(proposalId), "DecVCPlat: proposal does not exist");
        
        Vote storage vote = proposals[proposalId].votes[voter];
        return (vote.hasVoted, vote.choice, vote.votingPower, vote.stakedAmount);
    }
    
    /**
     * @dev Check if a proposal exists
     * @param proposalId ID of the proposal
     */
    function proposalExists(uint256 proposalId) public view returns (bool) {
        return proposalId > 0 && proposalId <= proposalCount;
    }
    
    /**
     * @dev Check if a milestone exists
     * @param milestoneId ID of the milestone
     */
    function milestoneExists(uint256 milestoneId) public view returns (bool) {
        return milestoneId > 0 && milestoneId <= milestoneCount;
    }
    
    /**
     * @dev Emergency pause (only owner)
     */
    function pause() external onlyOwner {
        _pause();
    }
    
    /**
     * @dev Emergency unpause (only owner)
     */
    function unpause() external onlyOwner {
        _unpause();
    }
    
    /**
     * @dev Emergency function to recover accidentally sent tokens
     * @param token Address of the token to recover
     * @param amount Amount to recover
     */
    function emergencyRecoverToken(address token, uint256 amount) external onlyOwner {
        require(token != address(dvcpToken), "DecVCPlat: cannot recover DVCP tokens");
        IERC20(token).transfer(owner(), amount);
    }
}
