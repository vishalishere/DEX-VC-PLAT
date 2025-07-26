using System;
using System.Threading.Tasks;

namespace DecVCPlat.Funding.Services
{
    /// <summary>
    /// Interface for interacting with the DecVCPlat token smart contract
    /// </summary>
    public interface ITokenContractService
    {
        /// <summary>
        /// Gets the token balance for an address
        /// </summary>
        /// <param name="address">The Ethereum address</param>
        /// <returns>The token balance</returns>
        Task<decimal> GetTokenBalanceAsync(string address);
        
        /// <summary>
        /// Creates a new project on the blockchain
        /// </summary>
        /// <param name="adminPrivateKey">Private key of the admin account</param>
        /// <param name="projectId">Unique identifier for the project</param>
        /// <param name="ownerAddress">Ethereum address of the project owner</param>
        /// <param name="fundingGoal">Target amount of tokens to raise</param>
        /// <param name="durationDays">Number of days the funding will be open</param>
        /// <returns>The transaction hash</returns>
        Task<string> CreateProjectAsync(string adminPrivateKey, long projectId, string ownerAddress, decimal fundingGoal, int durationDays);
        
        /// <summary>
        /// Funds a project with tokens
        /// </summary>
        /// <param name="userPrivateKey">Private key of the funder</param>
        /// <param name="projectId">ID of the project to fund</param>
        /// <param name="amount">Amount of tokens to contribute</param>
        /// <returns>The transaction hash</returns>
        Task<string> FundProjectAsync(string userPrivateKey, long projectId, decimal amount);
        
        /// <summary>
        /// Creates a new voting session for a project
        /// </summary>
        /// <param name="adminPrivateKey">Private key of the admin account</param>
        /// <param name="votingSessionId">Unique identifier for the voting session</param>
        /// <param name="projectId">ID of the project this voting is for</param>
        /// <param name="durationDays">Number of days the voting will be open</param>
        /// <returns>The transaction hash</returns>
        Task<string> CreateVotingSessionAsync(string adminPrivateKey, long votingSessionId, long projectId, int durationDays);
        
        /// <summary>
        /// Casts a vote in a voting session
        /// </summary>
        /// <param name="userPrivateKey">Private key of the voter</param>
        /// <param name="votingSessionId">ID of the voting session</param>
        /// <param name="inFavor">Whether the vote is in favor (true) or against (false)</param>
        /// <returns>The transaction hash</returns>
        Task<string> VoteAsync(string userPrivateKey, long votingSessionId, bool inFavor);
        
        /// <summary>
        /// Finalizes a voting session after it ends
        /// </summary>
        /// <param name="adminPrivateKey">Private key of the admin account</param>
        /// <param name="votingSessionId">ID of the voting session to finalize</param>
        /// <returns>The transaction hash</returns>
        Task<string> FinalizeVotingSessionAsync(string adminPrivateKey, long votingSessionId);
        
        /// <summary>
        /// Releases funds to project owner if project is funded and approved by voting
        /// </summary>
        /// <param name="adminPrivateKey">Private key of the admin account</param>
        /// <param name="projectId">ID of the project</param>
        /// <param name="votingSessionId">ID of the associated voting session</param>
        /// <returns>The transaction hash</returns>
        Task<string> ReleaseFundsAsync(string adminPrivateKey, long projectId, long votingSessionId);
        
        /// <summary>
        /// Gets project details from the blockchain
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <returns>Project details</returns>
        Task<(decimal FundingGoal, decimal CurrentFunding, DateTime Deadline, bool Funded, bool FundsReleased)> 
            GetProjectDetailsAsync(long projectId);
        
        /// <summary>
        /// Gets voting session details from the blockchain
        /// </summary>
        /// <param name="votingSessionId">ID of the voting session</param>
        /// <returns>Voting session details</returns>
        Task<(long ProjectId, DateTime StartTime, DateTime EndTime, decimal YesVotes, decimal NoVotes, bool Finalized)> 
            GetVotingSessionDetailsAsync(long votingSessionId);
    }
}
