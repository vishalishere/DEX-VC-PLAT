namespace DecVCPlat.Common.Blockchain
{
    /// <summary>
    /// Settings for blockchain integration
    /// </summary>
    public class BlockchainSettings
    {
        /// <summary>
        /// The URL of the blockchain provider (e.g., Infura, Alchemy)
        /// </summary>
        public string ProviderUrl { get; set; }
        
        /// <summary>
        /// The network ID of the blockchain (e.g., 1 for Ethereum mainnet, 5 for Goerli testnet)
        /// </summary>
        public int NetworkId { get; set; }
        
        /// <summary>
        /// The address of the smart contract used for platform operations
        /// </summary>
        public string ContractAddress { get; set; }
        
        /// <summary>
        /// The ABI (Application Binary Interface) of the smart contract
        /// </summary>
        public string ContractAbi { get; set; }
        
        /// <summary>
        /// Number of confirmations required to consider a transaction verified
        /// </summary>
        public int RequiredConfirmations { get; set; } = 12;
        
        /// <summary>
        /// Gas price strategy (e.g., "fastest", "fast", "average", "safeLow")
        /// </summary>
        public string GasPriceStrategy { get; set; } = "average";
    }
}
