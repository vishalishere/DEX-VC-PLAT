// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Burnable.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";

/**
 * @title DecVCPlatToken
 * @dev ERC20 token for the DecVCPlat platform with governance capabilities
 */
contract DecVCPlatToken is ERC20, ERC20Burnable, Pausable, AccessControl {
    bytes32 public constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    bytes32 public constant MINTER_ROLE = keccak256("MINTER_ROLE");
    bytes32 public constant PAUSER_ROLE = keccak256("PAUSER_ROLE");

    // Mapping of project IDs to their funding details
    mapping(uint256 => Project) public projects;
    
    // Mapping of voting session IDs to their details
    mapping(uint256 => VotingSession) public votingSessions;
    
    // Mapping of user address to their voting power (token balance at voting snapshot)
    mapping(address => mapping(uint256 => uint256)) public votingPower;

    // Project struct to store project funding details
    struct Project {
        uint256 id;
        address owner;
        uint256 fundingGoal;
        uint256 currentFunding;
        uint256 deadline;
        bool funded;
        bool fundsReleased;
    }
    
    // Voting session struct
    struct VotingSession {
        uint256 id;
        uint256 projectId;
        uint256 startTime;
        uint256 endTime;
        uint256 yesVotes;
        uint256 noVotes;
        bool finalized;
        mapping(address => bool) hasVoted;
    }

    // Events
    event ProjectCreated(uint256 indexed projectId, address indexed owner, uint256 fundingGoal, uint256 deadline);
    event ProjectFunded(uint256 indexed projectId, address indexed funder, uint256 amount);
    event FundsReleased(uint256 indexed projectId, address indexed recipient, uint256 amount);
    event VotingSessionCreated(uint256 indexed votingSessionId, uint256 indexed projectId, uint256 startTime, uint256 endTime);
    event VoteCast(uint256 indexed votingSessionId, address indexed voter, bool inFavor, uint256 votingPower);
    event VotingSessionFinalized(uint256 indexed votingSessionId, bool passed);

    /**
     * @dev Constructor that initializes the token with name, symbol and roles
     */
    constructor() ERC20("DecVCPlat Token", "DECVC") {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _grantRole(ADMIN_ROLE, msg.sender);
        _grantRole(MINTER_ROLE, msg.sender);
        _grantRole(PAUSER_ROLE, msg.sender);
    }

    /**
     * @dev Creates a new project with funding goal
     * @param projectId Unique identifier for the project
     * @param owner Address of the project owner
     * @param fundingGoal Target amount of tokens to raise
     * @param durationDays Number of days the funding will be open
     */
    function createProject(uint256 projectId, address owner, uint256 fundingGoal, uint256 durationDays) 
        external onlyRole(ADMIN_ROLE) 
    {
        require(projects[projectId].owner == address(0), "Project ID already exists");
        require(owner != address(0), "Invalid owner address");
        require(fundingGoal > 0, "Funding goal must be greater than zero");
        
        uint256 deadline = block.timestamp + (durationDays * 1 days);
        
        projects[projectId] = Project({
            id: projectId,
            owner: owner,
            fundingGoal: fundingGoal,
            currentFunding: 0,
            deadline: deadline,
            funded: false,
            fundsReleased: false
        });
        
        emit ProjectCreated(projectId, owner, fundingGoal, deadline);
    }

    /**
     * @dev Allows users to fund a project with tokens
     * @param projectId ID of the project to fund
     * @param amount Amount of tokens to contribute
     */
    function fundProject(uint256 projectId, uint256 amount) external whenNotPaused {
        Project storage project = projects[projectId];
        
        require(project.owner != address(0), "Project does not exist");
        require(block.timestamp <= project.deadline, "Funding period has ended");
        require(!project.funded, "Project already fully funded");
        require(amount > 0, "Amount must be greater than zero");
        require(balanceOf(msg.sender) >= amount, "Insufficient balance");
        
        // Transfer tokens from sender to contract
        _transfer(msg.sender, address(this), amount);
        
        // Update project funding
        project.currentFunding += amount;
        
        // Check if funding goal is reached
        if (project.currentFunding >= project.fundingGoal) {
            project.funded = true;
        }
        
        emit ProjectFunded(projectId, msg.sender, amount);
    }

    /**
     * @dev Creates a new voting session for a project
     * @param votingSessionId Unique identifier for the voting session
     * @param projectId ID of the project this voting is for
     * @param durationDays Number of days the voting will be open
     */
    function createVotingSession(uint256 votingSessionId, uint256 projectId, uint256 durationDays) 
        external onlyRole(ADMIN_ROLE) 
    {
        require(projects[projectId].owner != address(0), "Project does not exist");
        
        uint256 startTime = block.timestamp;
        uint256 endTime = startTime + (durationDays * 1 days);
        
        VotingSession storage session = votingSessions[votingSessionId];
        session.id = votingSessionId;
        session.projectId = projectId;
        session.startTime = startTime;
        session.endTime = endTime;
        session.yesVotes = 0;
        session.noVotes = 0;
        session.finalized = false;
        
        emit VotingSessionCreated(votingSessionId, projectId, startTime, endTime);
    }

    /**
     * @dev Allows token holders to vote in a voting session
     * @param votingSessionId ID of the voting session
     * @param inFavor Whether the vote is in favor (true) or against (false)
     */
    function vote(uint256 votingSessionId, bool inFavor) external whenNotPaused {
        VotingSession storage session = votingSessions[votingSessionId];
        
        require(session.startTime > 0, "Voting session does not exist");
        require(block.timestamp >= session.startTime, "Voting has not started yet");
        require(block.timestamp <= session.endTime, "Voting has ended");
        require(!session.hasVoted[msg.sender], "Already voted");
        
        uint256 power = balanceOf(msg.sender);
        require(power > 0, "No voting power");
        
        // Record voting power at time of vote
        votingPower[msg.sender][votingSessionId] = power;
        
        // Record vote
        if (inFavor) {
            session.yesVotes += power;
        } else {
            session.noVotes += power;
        }
        
        // Mark as voted
        session.hasVoted[msg.sender] = true;
        
        emit VoteCast(votingSessionId, msg.sender, inFavor, power);
    }

    /**
     * @dev Finalizes a voting session after it ends
     * @param votingSessionId ID of the voting session to finalize
     */
    function finalizeVotingSession(uint256 votingSessionId) external {
        VotingSession storage session = votingSessions[votingSessionId];
        
        require(session.startTime > 0, "Voting session does not exist");
        require(block.timestamp > session.endTime, "Voting still in progress");
        require(!session.finalized, "Voting already finalized");
        
        session.finalized = true;
        
        bool passed = session.yesVotes > session.noVotes;
        
        emit VotingSessionFinalized(votingSessionId, passed);
    }

    /**
     * @dev Releases funds to project owner if project is funded and approved by voting
     * @param projectId ID of the project
     * @param votingSessionId ID of the associated voting session
     */
    function releaseFunds(uint256 projectId, uint256 votingSessionId) external {
        Project storage project = projects[projectId];
        VotingSession storage session = votingSessions[votingSessionId];
        
        require(project.owner != address(0), "Project does not exist");
        require(project.funded, "Project not fully funded");
        require(!project.fundsReleased, "Funds already released");
        require(session.projectId == projectId, "Voting session not for this project");
        require(session.finalized, "Voting not finalized");
        require(session.yesVotes > session.noVotes, "Vote did not pass");
        
        uint256 amount = project.currentFunding;
        project.fundsReleased = true;
        
        // Transfer tokens from contract to project owner
        _transfer(address(this), project.owner, amount);
        
        emit FundsReleased(projectId, project.owner, amount);
    }

    /**
     * @dev Mints new tokens
     * @param to Address to mint tokens to
     * @param amount Amount of tokens to mint
     */
    function mint(address to, uint256 amount) external onlyRole(MINTER_ROLE) {
        _mint(to, amount);
    }

    /**
     * @dev Pauses token transfers
     */
    function pause() external onlyRole(PAUSER_ROLE) {
        _pause();
    }

    /**
     * @dev Unpauses token transfers
     */
    function unpause() external onlyRole(PAUSER_ROLE) {
        _unpause();
    }

    /**
     * @dev Hook that is called before any transfer of tokens
     */
    function _beforeTokenTransfer(address from, address to, uint256 amount)
        internal
        whenNotPaused
        override
    {
        super._beforeTokenTransfer(from, to, amount);
    }
}
