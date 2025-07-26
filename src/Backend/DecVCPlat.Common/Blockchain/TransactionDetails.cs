using System;
using System.Numerics;

namespace DecVCPlat.Common.Blockchain
{
    /// <summary>
    /// Represents details of a blockchain transaction
    /// </summary>
    public class TransactionDetails
    {
        /// <summary>
        /// The transaction hash
        /// </summary>
        public string TransactionHash { get; set; }
        
        /// <summary>
        /// The block number where the transaction was included
        /// </summary>
        public BigInteger BlockNumber { get; set; }
        
        /// <summary>
        /// The timestamp of the block
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// The sender address
        /// </summary>
        public string From { get; set; }
        
        /// <summary>
        /// The recipient address
        /// </summary>
        public string To { get; set; }
        
        /// <summary>
        /// The transaction value in wei
        /// </summary>
        public BigInteger Value { get; set; }
        
        /// <summary>
        /// The gas used by the transaction
        /// </summary>
        public BigInteger GasUsed { get; set; }
        
        /// <summary>
        /// The gas price in wei
        /// </summary>
        public BigInteger GasPrice { get; set; }
        
        /// <summary>
        /// The transaction data (input)
        /// </summary>
        public string Data { get; set; }
        
        /// <summary>
        /// The transaction status (1 = success, 0 = failure)
        /// </summary>
        public int Status { get; set; }
        
        /// <summary>
        /// Number of confirmations for the transaction
        /// </summary>
        public BigInteger Confirmations { get; set; }
        
        /// <summary>
        /// Whether the transaction was successful
        /// </summary>
        public bool IsSuccessful => Status == 1;
    }
}
