using System;
using System.Numerics;
using System.Threading.Tasks;
using DecVCPlat.Common.Blockchain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DecVCPlat.Funding.Services
{
    /// <summary>
    /// Service for interacting with the DecVCPlat token smart contract
    /// </summary>
    public class TokenContractService : ITokenContractService
    {
        private readonly IBlockchainService _blockchainService;
        private readonly ILogger<TokenContractService> _logger;
        private readonly BlockchainSettings _settings;

        public TokenContractService(
            IBlockchainService blockchainService,
            IOptions<BlockchainSettings> settings,
            ILogger<TokenContractService> logger)
        {
            _blockchainService = blockchainService ?? throw new ArgumentNullException(nameof(blockchainService));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<decimal> GetTokenBalanceAsync(string address)
        {
            try
            {
                var balance = await _blockchainService.CallContractFunctionAsync<BigInteger>(
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "balanceOf",
                    address);
                
                // Convert from wei (10^18) to token units
                return (decimal)balance / 1_000_000_000_000_000_000m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token balance for address {Address}", address);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> CreateProjectAsync(string adminPrivateKey, long projectId, string ownerAddress, decimal fundingGoal, int durationDays)
        {
            try
            {
                // Convert funding goal to wei (multiply by 10^18)
                var fundingGoalWei = new BigInteger(fundingGoal * 1_000_000_000_000_000_000m);
                
                var transactionHash = await _blockchainService.SendContractTransactionAsync(
                    adminPrivateKey,
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "createProject",
                    new BigInteger(projectId),
                    ownerAddress,
                    fundingGoalWei,
                    new BigInteger(durationDays));
                
                _logger.LogInformation("Project {ProjectId} created on blockchain with transaction {TransactionHash}", projectId, transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project {ProjectId} on blockchain", projectId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> FundProjectAsync(string userPrivateKey, long projectId, decimal amount)
        {
            try
            {
                // Convert amount to wei (multiply by 10^18)
                var amountWei = new BigInteger(amount * 1_000_000_000_000_000_000m);
                
                var transactionHash = await _blockchainService.SendContractTransactionAsync(
                    userPrivateKey,
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "fundProject",
                    new BigInteger(projectId),
                    amountWei);
                
                _logger.LogInformation("Project {ProjectId} funded with {Amount} tokens, transaction {TransactionHash}", projectId, amount, transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error funding project {ProjectId} with {Amount} tokens", projectId, amount);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> CreateVotingSessionAsync(string adminPrivateKey, long votingSessionId, long projectId, int durationDays)
        {
            try
            {
                var transactionHash = await _blockchainService.SendContractTransactionAsync(
                    adminPrivateKey,
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "createVotingSession",
                    new BigInteger(votingSessionId),
                    new BigInteger(projectId),
                    new BigInteger(durationDays));
                
                _logger.LogInformation("Voting session {VotingSessionId} created for project {ProjectId}, transaction {TransactionHash}", 
                    votingSessionId, projectId, transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voting session {VotingSessionId} for project {ProjectId}", votingSessionId, projectId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> VoteAsync(string userPrivateKey, long votingSessionId, bool inFavor)
        {
            try
            {
                var transactionHash = await _blockchainService.SendContractTransactionAsync(
                    userPrivateKey,
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "vote",
                    new BigInteger(votingSessionId),
                    inFavor);
                
                _logger.LogInformation("Vote cast for voting session {VotingSessionId}, in favor: {InFavor}, transaction {TransactionHash}", 
                    votingSessionId, inFavor, transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error casting vote for voting session {VotingSessionId}", votingSessionId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> FinalizeVotingSessionAsync(string adminPrivateKey, long votingSessionId)
        {
            try
            {
                var transactionHash = await _blockchainService.SendContractTransactionAsync(
                    adminPrivateKey,
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "finalizeVotingSession",
                    new BigInteger(votingSessionId));
                
                _logger.LogInformation("Voting session {VotingSessionId} finalized, transaction {TransactionHash}", 
                    votingSessionId, transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing voting session {VotingSessionId}", votingSessionId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> ReleaseFundsAsync(string adminPrivateKey, long projectId, long votingSessionId)
        {
            try
            {
                var transactionHash = await _blockchainService.SendContractTransactionAsync(
                    adminPrivateKey,
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "releaseFunds",
                    new BigInteger(projectId),
                    new BigInteger(votingSessionId));
                
                _logger.LogInformation("Funds released for project {ProjectId} after voting session {VotingSessionId}, transaction {TransactionHash}", 
                    projectId, votingSessionId, transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing funds for project {ProjectId} after voting session {VotingSessionId}", 
                    projectId, votingSessionId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(decimal FundingGoal, decimal CurrentFunding, DateTime Deadline, bool Funded, bool FundsReleased)> 
            GetProjectDetailsAsync(long projectId)
        {
            try
            {
                var projectDetails = await _blockchainService.CallContractFunctionAsync<
                    (BigInteger Id, string Owner, BigInteger FundingGoal, BigInteger CurrentFunding, BigInteger Deadline, bool Funded, bool FundsReleased)>(
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "projects",
                    new BigInteger(projectId));
                
                // Convert wei amounts to token units
                var fundingGoal = (decimal)projectDetails.FundingGoal / 1_000_000_000_000_000_000m;
                var currentFunding = (decimal)projectDetails.CurrentFunding / 1_000_000_000_000_000_000m;
                
                // Convert Unix timestamp to DateTime
                var deadline = DateTimeOffset.FromUnixTimeSeconds((long)projectDetails.Deadline).DateTime;
                
                return (fundingGoal, currentFunding, deadline, projectDetails.Funded, projectDetails.FundsReleased);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project details for project {ProjectId}", projectId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(long ProjectId, DateTime StartTime, DateTime EndTime, decimal YesVotes, decimal NoVotes, bool Finalized)> 
            GetVotingSessionDetailsAsync(long votingSessionId)
        {
            try
            {
                var sessionDetails = await _blockchainService.CallContractFunctionAsync<
                    (BigInteger Id, BigInteger ProjectId, BigInteger StartTime, BigInteger EndTime, BigInteger YesVotes, BigInteger NoVotes, bool Finalized)>(
                    _settings.ContractAddress,
                    _settings.ContractAbi,
                    "votingSessions",
                    new BigInteger(votingSessionId));
                
                // Convert vote counts from wei to token units
                var yesVotes = (decimal)sessionDetails.YesVotes / 1_000_000_000_000_000_000m;
                var noVotes = (decimal)sessionDetails.NoVotes / 1_000_000_000_000_000_000m;
                
                // Convert Unix timestamps to DateTime
                var startTime = DateTimeOffset.FromUnixTimeSeconds((long)sessionDetails.StartTime).DateTime;
                var endTime = DateTimeOffset.FromUnixTimeSeconds((long)sessionDetails.EndTime).DateTime;
                
                return ((long)sessionDetails.ProjectId, startTime, endTime, yesVotes, noVotes, sessionDetails.Finalized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voting session details for session {VotingSessionId}", votingSessionId);
                throw;
            }
        }
    }
}
