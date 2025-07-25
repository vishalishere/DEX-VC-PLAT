// SPDX-License-Identifier: MIT
// © 2024 DecVCPlat. All rights reserved.

pragma solidity ^0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Burnable.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

/**
 * @title DecVCPlatToken
 * @dev ERC20 token for DecVCPlat - Decentralized Venture Capital Platform
 * Features:
 * - Standard ERC20 functionality
 * - Burnable tokens for deflationary mechanism
 * - Pausable for emergency situations
 * - Owner controls for platform management
 * - Staking rewards for governance participation
 * - Anti-reentrancy protection
 */
contract DecVCPlatToken is ERC20, ERC20Burnable, Pausable, Ownable, ReentrancyGuard {
    
    // Token configuration
    uint256 public constant INITIAL_SUPPLY = 1_000_000_000 * 10**18; // 1 billion tokens
    uint256 public constant MAX_SUPPLY = 10_000_000_000 * 10**18; // 10 billion max supply
    
    // Staking configuration
    uint256 public stakingRewardRate = 50; // 5% annual reward (basis points)
    uint256 public constant REWARD_RATE_PRECISION = 1000;
    uint256 public constant SECONDS_PER_YEAR = 365 * 24 * 60 * 60;
    
    // Minimum staking period (7 days)
    uint256 public constant MIN_STAKING_PERIOD = 7 days;
    
    // Staking data
    struct Stake {
        uint256 amount;
        uint256 stakingTime;
        uint256 lastRewardTime;
        bool isActive;
    }
    
    // Mappings
    mapping(address => Stake) public stakes;
    mapping(address => uint256) public totalStaked;
    mapping(address => bool) public authorizedMinters;
    
    // Events
    event TokensStaked(address indexed user, uint256 amount);
    event TokensUnstaked(address indexed user, uint256 amount, uint256 rewards);
    event RewardsDistributed(address indexed user, uint256 amount);
    event StakingRewardRateUpdated(uint256 oldRate, uint256 newRate);
    event MinterAuthorized(address indexed minter);
    event MinterRevoked(address indexed minter);
    
    // Total staking statistics
    uint256 public totalStakedTokens;
    uint256 public totalRewardsDistributed;
    
    constructor(address initialOwner) ERC20("DecVCPlat Token", "DVCP") {
        _transferOwnership(initialOwner);
        _mint(initialOwner, INITIAL_SUPPLY);
        
        // Authorize the initial owner as a minter
        authorizedMinters[initialOwner] = true;
        emit MinterAuthorized(initialOwner);
    }
    
    /**
     * @dev Modifier to check if caller is authorized minter
     */
    modifier onlyMinter() {
        require(authorizedMinters[msg.sender], "DecVCPlat: caller is not authorized minter");
        _;
    }
    
    /**
     * @dev Mint new tokens (only authorized minters)
     * @param to Address to mint tokens to
     * @param amount Amount of tokens to mint
     */
    function mint(address to, uint256 amount) external onlyMinter whenNotPaused {
        require(totalSupply() + amount <= MAX_SUPPLY, "DecVCPlat: would exceed max supply");
        _mint(to, amount);
    }
    
    /**
     * @dev Authorize a new minter
     * @param minter Address to authorize as minter
     */
    function authorizeMinter(address minter) external onlyOwner {
        require(minter != address(0), "DecVCPlat: minter cannot be zero address");
        authorizedMinters[minter] = true;
        emit MinterAuthorized(minter);
    }
    
    /**
     * @dev Revoke minter authorization
     * @param minter Address to revoke minter status from
     */
    function revokeMinter(address minter) external onlyOwner {
        authorizedMinters[minter] = false;
        emit MinterRevoked(minter);
    }
    
    /**
     * @dev Stake tokens for governance and rewards
     * @param amount Amount of tokens to stake
     */
    function stakeTokens(uint256 amount) external nonReentrant whenNotPaused {
        require(amount > 0, "DecVCPlat: cannot stake 0 tokens");
        require(balanceOf(msg.sender) >= amount, "DecVCPlat: insufficient balance");
        
        // If user already has an active stake, claim rewards first
        if (stakes[msg.sender].isActive) {
            _claimStakingRewards(msg.sender);
        }
        
        // Transfer tokens from user to contract
        _transfer(msg.sender, address(this), amount);
        
        // Update or create stake
        if (stakes[msg.sender].isActive) {
            stakes[msg.sender].amount += amount;
        } else {
            stakes[msg.sender] = Stake({
                amount: amount,
                stakingTime: block.timestamp,
                lastRewardTime: block.timestamp,
                isActive: true
            });
        }
        
        totalStaked[msg.sender] += amount;
        totalStakedTokens += amount;
        
        emit TokensStaked(msg.sender, amount);
    }
    
    /**
     * @dev Unstake tokens and claim rewards
     * @param amount Amount of tokens to unstake
     */
    function unstakeTokens(uint256 amount) external nonReentrant whenNotPaused {
        Stake storage userStake = stakes[msg.sender];
        require(userStake.isActive, "DecVCPlat: no active stake");
        require(amount > 0, "DecVCPlat: cannot unstake 0 tokens");
        require(userStake.amount >= amount, "DecVCPlat: insufficient staked amount");
        require(
            block.timestamp >= userStake.stakingTime + MIN_STAKING_PERIOD,
            "DecVCPlat: minimum staking period not met"
        );
        
        // Calculate and distribute rewards
        uint256 rewards = _calculateStakingRewards(msg.sender);
        if (rewards > 0) {
            _mint(msg.sender, rewards);
            totalRewardsDistributed += rewards;
            emit RewardsDistributed(msg.sender, rewards);
        }
        
        // Update stake
        userStake.amount -= amount;
        userStake.lastRewardTime = block.timestamp;
        totalStaked[msg.sender] -= amount;
        totalStakedTokens -= amount;
        
        // If all tokens unstaked, deactivate stake
        if (userStake.amount == 0) {
            userStake.isActive = false;
        }
        
        // Transfer tokens back to user
        _transfer(address(this), msg.sender, amount);
        
        emit TokensUnstaked(msg.sender, amount, rewards);
    }
    
    /**
     * @dev Claim staking rewards without unstaking
     */
    function claimStakingRewards() external nonReentrant whenNotPaused {
        require(stakes[msg.sender].isActive, "DecVCPlat: no active stake");
        _claimStakingRewards(msg.sender);
    }
    
    /**
     * @dev Internal function to claim staking rewards
     * @param user Address of the user claiming rewards
     */
    function _claimStakingRewards(address user) internal {
        uint256 rewards = _calculateStakingRewards(user);
        if (rewards > 0) {
            stakes[user].lastRewardTime = block.timestamp;
            _mint(user, rewards);
            totalRewardsDistributed += rewards;
            emit RewardsDistributed(user, rewards);
        }
    }
    
    /**
     * @dev Calculate pending staking rewards for a user
     * @param user Address of the user
     * @return rewards Pending rewards amount
     */
    function _calculateStakingRewards(address user) internal view returns (uint256) {
        Stake storage userStake = stakes[user];
        if (!userStake.isActive || userStake.amount == 0) {
            return 0;
        }
        
        uint256 stakingDuration = block.timestamp - userStake.lastRewardTime;
        uint256 annualReward = (userStake.amount * stakingRewardRate) / REWARD_RATE_PRECISION;
        uint256 rewards = (annualReward * stakingDuration) / SECONDS_PER_YEAR;
        
        return rewards;
    }
    
    /**
     * @dev Get pending staking rewards for a user (external view)
     * @param user Address of the user
     * @return Pending rewards amount
     */
    function getPendingRewards(address user) external view returns (uint256) {
        return _calculateStakingRewards(user);
    }
    
    /**
     * @dev Get staking information for a user
     * @param user Address of the user
     * @return amount Staked amount
     * @return stakingTime When tokens were first staked
     * @return lastRewardTime When rewards were last claimed
     * @return isActive Whether the stake is active
     * @return pendingRewards Current pending rewards
     */
    function getStakeInfo(address user) external view returns (
        uint256 amount,
        uint256 stakingTime,
        uint256 lastRewardTime,
        bool isActive,
        uint256 pendingRewards
    ) {
        Stake storage userStake = stakes[user];
        return (
            userStake.amount,
            userStake.stakingTime,
            userStake.lastRewardTime,
            userStake.isActive,
            _calculateStakingRewards(user)
        );
    }
    
    /**
     * @dev Update staking reward rate (only owner)
     * @param newRate New reward rate in basis points
     */
    function updateStakingRewardRate(uint256 newRate) external onlyOwner {
        require(newRate <= 200, "DecVCPlat: reward rate too high"); // Max 20% annual
        uint256 oldRate = stakingRewardRate;
        stakingRewardRate = newRate;
        emit StakingRewardRateUpdated(oldRate, newRate);
    }
    
    /**
     * @dev Pause the contract (only owner)
     */
    function pause() external onlyOwner {
        _pause();
    }
    
    /**
     * @dev Unpause the contract (only owner)
     */
    function unpause() external onlyOwner {
        _unpause();
    }
    
    /**
     * @dev Override transfer function to include pause functionality
     */
    function _beforeTokenTransfer(
        address from,
        address to,
        uint256 amount
    ) internal override whenNotPaused {
        super._beforeTokenTransfer(from, to, amount);
    }
    
    /**
     * @dev Get voting power based on staked tokens
     * @param user Address of the user
     * @return Voting power (staked tokens)
     */
    function getVotingPower(address user) external view returns (uint256) {
        return totalStaked[user];
    }
    
    /**
     * @dev Emergency function to recover accidentally sent tokens
     * @param token Address of the token to recover
     * @param amount Amount to recover
     */
    function emergencyRecoverToken(address token, uint256 amount) external onlyOwner {
        require(token != address(this), "DecVCPlat: cannot recover DVCP tokens");
        IERC20(token).transfer(owner(), amount);
    }
}
