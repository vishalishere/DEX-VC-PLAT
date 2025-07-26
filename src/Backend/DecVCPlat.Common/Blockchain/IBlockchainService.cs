using System.Numerics;
using System.Threading.Tasks;

namespace DecVCPlat.Common.Blockchain
{
    /// <summary>
    /// Interface for blockchain interaction services
    /// </summary>
    public interface IBlockchainService
    {
        /// <summary>
        /// Gets the current block number from the blockchain
        /// </summary>
        /// <returns>The current block number</returns>
        Task<BigInteger> GetBlockNumberAsync();
        
        /// <summary>
        /// Gets the balance of an account
        /// </summary>
        /// <param name="address">The Ethereum address</param>
        /// <returns>The account balance in wei</returns>
        Task<BigInteger> GetBalanceAsync(string address);
        
        /// <summary>
        /// Verifies a transaction on the blockchain
        /// </summary>
        /// <param name="transactionHash">The transaction hash to verify</param>
        /// <returns>True if the transaction exists and is confirmed, false otherwise</returns>
        Task<bool> VerifyTransactionAsync(string transactionHash);
        
        /// <summary>
        /// Gets transaction details from the blockchain
        /// </summary>
        /// <param name="transactionHash">The transaction hash</param>
        /// <returns>Transaction details</returns>
        Task<TransactionDetails> GetTransactionDetailsAsync(string transactionHash);
        
        /// <summary>
        /// Estimates the gas required for a transaction
        /// </summary>
        /// <param name="fromAddress">The sender address</param>
        /// <param name="toAddress">The recipient address</param>
        /// <param name="value">The amount to send in wei</param>
        /// <param name="data">Optional transaction data</param>
        /// <returns>The estimated gas amount</returns>
        Task<BigInteger> EstimateGasAsync(string fromAddress, string toAddress, BigInteger value, string data = null);
        
        /// <summary>
        /// Sends a transaction to the blockchain
        /// </summary>
        /// <param name="privateKey">The private key of the sender</param>
        /// <param name="toAddress">The recipient address</param>
        /// <param name="value">The amount to send in wei</param>
        /// <param name="data">Optional transaction data</param>
        /// <returns>The transaction hash</returns>
        Task<string> SendTransactionAsync(string privateKey, string toAddress, BigInteger value, string data = null);
        
        /// <summary>
        /// Deploys a smart contract to the blockchain
        /// </summary>
        /// <param name="privateKey">The private key of the deployer</param>
        /// <param name="abi">The contract ABI</param>
        /// <param name="bytecode">The contract bytecode</param>
        /// <param name="constructorParams">Optional constructor parameters</param>
        /// <returns>The deployed contract address</returns>
        Task<string> DeployContractAsync(string privateKey, string abi, string bytecode, params object[] constructorParams);
        
        /// <summary>
        /// Calls a read-only function on a smart contract
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="contractAddress">The contract address</param>
        /// <param name="abi">The contract ABI</param>
        /// <param name="functionName">The function name to call</param>
        /// <param name="parameters">Function parameters</param>
        /// <returns>The function result</returns>
        Task<T> CallContractFunctionAsync<T>(string contractAddress, string abi, string functionName, params object[] parameters);
        
        /// <summary>
        /// Sends a transaction to a smart contract function
        /// </summary>
        /// <param name="privateKey">The private key of the sender</param>
        /// <param name="contractAddress">The contract address</param>
        /// <param name="abi">The contract ABI</param>
        /// <param name="functionName">The function name to call</param>
        /// <param name="parameters">Function parameters</param>
        /// <returns>The transaction hash</returns>
        Task<string> SendContractTransactionAsync(string privateKey, string contractAddress, string abi, string functionName, params object[] parameters);
    }
}
