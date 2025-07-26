using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace DecVCPlat.Common.Blockchain
{
    /// <summary>
    /// Implementation of blockchain service for Ethereum networks
    /// </summary>
    public class EthereumBlockchainService : IBlockchainService
    {
        private readonly IWeb3 _web3;
        private readonly ILogger<EthereumBlockchainService> _logger;
        private readonly BlockchainSettings _settings;

        public EthereumBlockchainService(
            IOptions<BlockchainSettings> settings,
            ILogger<EthereumBlockchainService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize Web3 with the provider URL
            _web3 = new Web3(_settings.ProviderUrl);
            
            _logger.LogInformation("Ethereum blockchain service initialized with provider: {ProviderUrl}", _settings.ProviderUrl);
        }

        /// <inheritdoc />
        public async Task<BigInteger> GetBlockNumberAsync()
        {
            try
            {
                var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                return blockNumber.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting block number");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<BigInteger> GetBalanceAsync(string address)
        {
            try
            {
                var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
                return balance.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance for address {Address}", address);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> VerifyTransactionAsync(string transactionHash)
        {
            try
            {
                var transaction = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);
                
                if (transaction == null)
                {
                    return false;
                }
                
                // Check if transaction is included in a block
                if (transaction.BlockNumber == null)
                {
                    return false;
                }
                
                // Get current block number to calculate confirmations
                var currentBlock = await GetBlockNumberAsync();
                var transactionBlock = transaction.BlockNumber.Value;
                var confirmations = currentBlock - transactionBlock + 1;
                
                // Transaction is verified if it has enough confirmations
                return confirmations >= _settings.RequiredConfirmations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying transaction {TransactionHash}", transactionHash);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<TransactionDetails> GetTransactionDetailsAsync(string transactionHash)
        {
            try
            {
                var transaction = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);
                
                if (transaction == null)
                {
                    throw new Exception($"Transaction {transactionHash} not found");
                }
                
                // If transaction is pending (not in a block yet), return limited details
                if (transaction.BlockNumber == null)
                {
                    return new TransactionDetails
                    {
                        TransactionHash = transactionHash,
                        From = transaction.From,
                        To = transaction.To,
                        Value = transaction.Value.Value,
                        Data = transaction.Input,
                        GasPrice = transaction.GasPrice.Value
                    };
                }
                
                // Get receipt for additional details
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                
                // Get block to get timestamp
                var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByHash.SendRequestAsync(transaction.BlockHash);
                var timestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).DateTime;
                
                // Get current block for confirmations
                var currentBlock = await GetBlockNumberAsync();
                var confirmations = currentBlock - transaction.BlockNumber.Value;
                
                return new TransactionDetails
                {
                    TransactionHash = transactionHash,
                    BlockNumber = transaction.BlockNumber.Value,
                    Timestamp = timestamp,
                    From = transaction.From,
                    To = transaction.To,
                    Value = transaction.Value.Value,
                    GasUsed = receipt.GasUsed.Value,
                    GasPrice = transaction.GasPrice.Value,
                    Data = transaction.Input,
                    Status = receipt.Status.Value == 1 ? 1 : 0,
                    Confirmations = confirmations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction details for {TransactionHash}", transactionHash);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<BigInteger> EstimateGasAsync(string fromAddress, string toAddress, BigInteger value, string data = null)
        {
            try
            {
                var callInput = new CallInput
                {
                    From = fromAddress,
                    To = toAddress,
                    Value = new HexBigInteger(value)
                };
                
                if (!string.IsNullOrEmpty(data))
                {
                    callInput.Data = data;
                }
                
                var gas = await _web3.Eth.TransactionManager.EstimateGasAsync(callInput);
                return gas.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating gas for transaction from {FromAddress} to {ToAddress}", fromAddress, toAddress);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> SendTransactionAsync(string privateKey, string toAddress, BigInteger value, string data = null)
        {
            try
            {
                // Create account from private key
                var account = new Account(privateKey, _settings.NetworkId);
                var web3 = new Web3(account, _settings.ProviderUrl);
                
                // Prepare transaction input
                var transaction = new TransactionInput
                {
                    To = toAddress,
                    Value = new HexBigInteger(value),
                    From = account.Address
                };
                
                if (!string.IsNullOrEmpty(data))
                {
                    transaction.Data = data;
                }
                
                // Send transaction
                var transactionHash = await web3.Eth.TransactionManager.SendTransactionAsync(transaction);
                
                _logger.LogInformation("Transaction sent: {TransactionHash}", transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending transaction to {ToAddress}", toAddress);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> DeployContractAsync(string privateKey, string abi, string bytecode, params object[] constructorParams)
        {
            try
            {
                // Create account from private key
                var account = new Account(privateKey, _settings.NetworkId);
                var web3 = new Web3(account, _settings.ProviderUrl);
                
                // Deploy contract
                var deployment = new Nethereum.Contracts.DeployContract
                {
                    ByteCode = bytecode,
                    Abi = abi
                };
                
                var deploymentHandler = web3.Eth.GetContractDeploymentHandler<Nethereum.Contracts.DeployContract>();
                var transactionReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deployment, constructorParams);
                
                if (transactionReceipt.Status.Value == 0)
                {
                    throw new Exception("Contract deployment failed");
                }
                
                var contractAddress = transactionReceipt.ContractAddress;
                
                _logger.LogInformation("Contract deployed at address: {ContractAddress}", contractAddress);
                
                return contractAddress;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying contract");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<T> CallContractFunctionAsync<T>(string contractAddress, string abi, string functionName, params object[] parameters)
        {
            try
            {
                var contract = _web3.Eth.GetContract(abi, contractAddress);
                var function = contract.GetFunction(functionName);
                
                var result = await function.CallAsync<T>(parameters);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling contract function {FunctionName} at {ContractAddress}", functionName, contractAddress);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> SendContractTransactionAsync(string privateKey, string contractAddress, string abi, string functionName, params object[] parameters)
        {
            try
            {
                // Create account from private key
                var account = new Account(privateKey, _settings.NetworkId);
                var web3 = new Web3(account, _settings.ProviderUrl);
                
                // Get contract and function
                var contract = web3.Eth.GetContract(abi, contractAddress);
                var function = contract.GetFunction(functionName);
                
                // Send transaction
                var transactionHash = await function.SendTransactionAsync(account.Address, null, null, parameters);
                
                _logger.LogInformation("Contract transaction sent: {TransactionHash}", transactionHash);
                
                return transactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contract transaction for function {FunctionName} at {ContractAddress}", functionName, contractAddress);
                throw;
            }
        }
    }
}
