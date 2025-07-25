// © 2024 DecVCPlat. All rights reserved.

declare global {
  interface Window {
    ethereum?: any;
  }
}

export interface DecVCPlatWalletConnectionResult {
  decvcplatWalletAddress: string;
  decvcplatNetworkId: number;
  decvcplatNetworkName: string;
  decvcplatWalletProvider: string;
}

export interface DecVCPlatTokenContractInfo {
  decvcplatTokenAddress: string;
  decvcplatTokenSymbol: string;
  decvcplatTokenDecimals: number;
  decvcplatTokenName: string;
}

export interface DecVCPlatTransactionResult {
  decvcplatTxHash: string;
  decvcplatTxStatus: 'pending' | 'confirmed' | 'failed';
  decvcplatBlockNumber?: number;
  decvcplatGasUsed?: string;
}

class DecVCPlatWalletService {
  private decvcplatWalletProvider: any = null;
  private decvcplatConnectedAddress: string | null = null;
  private decvcplatCurrentNetwork: number | null = null;

  // DecVCPlat supported networks configuration
  private decvcplatSupportedNetworks = {
    1: { decvcplatName: 'Ethereum Mainnet', decvcplatRpcUrl: 'https://mainnet.infura.io/v3/decvcplat' },
    11155111: { decvcplatName: 'Sepolia Testnet', decvcplatRpcUrl: 'https://sepolia.infura.io/v3/decvcplat' },
    137: { decvcplatName: 'Polygon Mainnet', decvcplatRpcUrl: 'https://polygon-rpc.com' },
  };

  // DecVCPlat token contract addresses
  private decvcplatTokenContracts: { [networkId: number]: DecVCPlatTokenContractInfo } = {
    1: {
      decvcplatTokenAddress: '0xDecVCPlat1234567890123456789012345678901234567890',
      decvcplatTokenSymbol: 'DVCP',
      decvcplatTokenDecimals: 18,
      decvcplatTokenName: 'DecVCPlat Token',
    },
    11155111: {
      decvcplatTokenAddress: '0xDecVCPlatTestnet1234567890123456789012345678901234567890',
      decvcplatTokenSymbol: 'DVCP',
      decvcplatTokenDecimals: 18,
      decvcplatTokenName: 'DecVCPlat Token (Testnet)',
    },
  };

  async detectDecVCPlatWalletProvider(): Promise<boolean> {
    if (typeof window !== 'undefined' && window.ethereum) {
      this.decvcplatWalletProvider = window.ethereum;
      return true;
    }
    return false;
  }

  async connectDecVCPlatWallet(): Promise<DecVCPlatWalletConnectionResult> {
    if (!this.decvcplatWalletProvider) {
      const decvcplatProviderDetected = await this.detectDecVCPlatWalletProvider();
      if (!decvcplatProviderDetected) {
        throw new Error('DecVCPlat requires MetaMask or compatible wallet extension');
      }
    }

    try {
      const decvcplatAccounts = await this.decvcplatWalletProvider.request({
        method: 'eth_requestAccounts',
      });

      if (decvcplatAccounts.length === 0) {
        throw new Error('DecVCPlat wallet connection declined by user');
      }

      this.decvcplatConnectedAddress = decvcplatAccounts[0];

      const decvcplatNetworkId = await this.getCurrentDecVCPlatNetwork();
      const decvcplatNetworkInfo = this.decvcplatSupportedNetworks[decvcplatNetworkId as keyof typeof this.decvcplatSupportedNetworks];

      return {
        decvcplatWalletAddress: this.decvcplatConnectedAddress || '',
        decvcplatNetworkId,
        decvcplatNetworkName: decvcplatNetworkInfo?.decvcplatName || 'Unknown Network',
        decvcplatWalletProvider: this.getDecVCPlatWalletProviderName(),
      };
    } catch (decvcplatError) {
      console.error('DecVCPlat wallet connection error:', decvcplatError);
      throw new Error(`DecVCPlat wallet connection failed: ${decvcplatError}`);
    }
  }

  async getCurrentDecVCPlatNetwork(): Promise<number> {
    if (!this.decvcplatWalletProvider) {
      throw new Error('DecVCPlat wallet provider not initialized');
    }

    const decvcplatNetworkId = await this.decvcplatWalletProvider.request({
      method: 'eth_chainId',
    });

    this.decvcplatCurrentNetwork = parseInt(decvcplatNetworkId, 16);
    return this.decvcplatCurrentNetwork;
  }

  async switchDecVCPlatNetwork(decvcplatTargetNetworkId: number): Promise<void> {
    if (!this.decvcplatWalletProvider) {
      throw new Error('DecVCPlat wallet provider not initialized');
    }

    const decvcplatNetworkHex = `0x${decvcplatTargetNetworkId.toString(16)}`;

    try {
      await this.decvcplatWalletProvider.request({
        method: 'wallet_switchEthereumChain',
        params: [{ chainId: decvcplatNetworkHex }],
      });

      this.decvcplatCurrentNetwork = decvcplatTargetNetworkId;
    } catch (decvcplatError: any) {
      if (decvcplatError.code === 4902) {
        throw new Error('DecVCPlat network not added to wallet');
      }
      throw new Error(`DecVCPlat network switch failed: ${decvcplatError.message}`);
    }
  }

  async getDecVCPlatWalletBalance(decvcplatWalletAddress?: string): Promise<string> {
    if (!this.decvcplatWalletProvider) {
      throw new Error('DecVCPlat wallet provider not initialized');
    }

    const decvcplatTargetAddress = decvcplatWalletAddress || this.decvcplatConnectedAddress;
    if (!decvcplatTargetAddress) {
      throw new Error('DecVCPlat wallet address not provided');
    }

    try {
      const decvcplatBalanceWei = await this.decvcplatWalletProvider.request({
        method: 'eth_getBalance',
        params: [decvcplatTargetAddress, 'latest'],
      });

      // Convert Wei to Ether (simplified implementation)
      const decvcplatBalanceEther = (parseInt(decvcplatBalanceWei, 16) / Math.pow(10, 18)).toString();
      return decvcplatBalanceEther;
    } catch (decvcplatError) {
      console.error('DecVCPlat balance retrieval error:', decvcplatError);
      throw new Error(`DecVCPlat balance retrieval failed: ${decvcplatError}`);
    }
  }

  async getDecVCPlatTokenBalance(decvcplatWalletAddress?: string): Promise<string> {
    if (!this.decvcplatCurrentNetwork || !this.decvcplatTokenContracts[this.decvcplatCurrentNetwork]) {
      throw new Error('DecVCPlat token not available on current network');
    }

    const decvcplatTargetAddress = decvcplatWalletAddress || this.decvcplatConnectedAddress;
    if (!decvcplatTargetAddress) {
      throw new Error('DecVCPlat wallet address not provided');
    }

    const decvcplatTokenContract = this.decvcplatTokenContracts[this.decvcplatCurrentNetwork];

    try {
      // Simplified ERC20 balanceOf call (would need proper Web3 implementation)
      const decvcplatBalanceData = `0x70a08231000000000000000000000000${decvcplatTargetAddress.slice(2)}`;
      
      const decvcplatBalanceResult = await this.decvcplatWalletProvider.request({
        method: 'eth_call',
        params: [{
          to: decvcplatTokenContract.decvcplatTokenAddress,
          data: decvcplatBalanceData,
        }, 'latest'],
      });

      // Convert hex result to decimal (simplified)
      const decvcplatBalanceValue = parseInt(decvcplatBalanceResult, 16) / Math.pow(10, decvcplatTokenContract.decvcplatTokenDecimals);
      return decvcplatBalanceValue.toString();
    } catch (decvcplatError) {
      console.error('DecVCPlat token balance retrieval error:', decvcplatError);
      return '0';
    }
  }

  async stakeDecVCPlatTokens(decvcplatStakeAmount: string, decvcplatStakingContractAddress: string): Promise<DecVCPlatTransactionResult> {
    if (!this.decvcplatWalletProvider || !this.decvcplatConnectedAddress) {
      throw new Error('DecVCPlat wallet not connected');
    }

    try {
      // Simplified staking transaction (would need proper contract interaction)
      const decvcplatStakeData = this.encodeDecVCPlatStakeTransaction(decvcplatStakeAmount);

      const decvcplatTxParams = {
        from: this.decvcplatConnectedAddress,
        to: decvcplatStakingContractAddress,
        data: decvcplatStakeData,
        gas: '0x76c0', // 30400 gas
      };

      const decvcplatTxHash = await this.decvcplatWalletProvider.request({
        method: 'eth_sendTransaction',
        params: [decvcplatTxParams],
      });

      return {
        decvcplatTxHash,
        decvcplatTxStatus: 'pending',
      };
    } catch (decvcplatError) {
      console.error('DecVCPlat token staking error:', decvcplatError);
      throw new Error(`DecVCPlat token staking failed: ${decvcplatError}`);
    }
  }

  async voteOnDecVCPlatProposal(decvcplatProposalId: string, decvcplatVoteChoice: number, decvcplatGovernanceContractAddress: string): Promise<DecVCPlatTransactionResult> {
    if (!this.decvcplatWalletProvider || !this.decvcplatConnectedAddress) {
      throw new Error('DecVCPlat wallet not connected');
    }

    try {
      // Simplified voting transaction (would need proper contract interaction)
      const decvcplatVoteData = this.encodeDecVCPlatVoteTransaction(decvcplatProposalId, decvcplatVoteChoice);

      const decvcplatTxParams = {
        from: this.decvcplatConnectedAddress,
        to: decvcplatGovernanceContractAddress,
        data: decvcplatVoteData,
        gas: '0x9c40', // 40000 gas
      };

      const decvcplatTxHash = await this.decvcplatWalletProvider.request({
        method: 'eth_sendTransaction',
        params: [decvcplatTxParams],
      });

      return {
        decvcplatTxHash,
        decvcplatTxStatus: 'pending',
      };
    } catch (decvcplatError) {
      console.error('DecVCPlat proposal voting error:', decvcplatError);
      throw new Error(`DecVCPlat proposal voting failed: ${decvcplatError}`);
    }
  }

  async getDecVCPlatTransactionStatus(decvcplatTxHash: string): Promise<DecVCPlatTransactionResult> {
    if (!this.decvcplatWalletProvider) {
      throw new Error('DecVCPlat wallet provider not initialized');
    }

    try {
      const decvcplatTxReceipt = await this.decvcplatWalletProvider.request({
        method: 'eth_getTransactionReceipt',
        params: [decvcplatTxHash],
      });

      if (!decvcplatTxReceipt) {
        return {
          decvcplatTxHash,
          decvcplatTxStatus: 'pending',
        };
      }

      return {
        decvcplatTxHash,
        decvcplatTxStatus: decvcplatTxReceipt.status === '0x1' ? 'confirmed' : 'failed',
        decvcplatBlockNumber: parseInt(decvcplatTxReceipt.blockNumber, 16),
        decvcplatGasUsed: parseInt(decvcplatTxReceipt.gasUsed, 16).toString(),
      };
    } catch (decvcplatError) {
      console.error('DecVCPlat transaction status error:', decvcplatError);
      return {
        decvcplatTxHash,
        decvcplatTxStatus: 'failed',
      };
    }
  }

  disconnectDecVCPlatWallet(): void {
    this.decvcplatConnectedAddress = null;
    this.decvcplatCurrentNetwork = null;
    this.decvcplatWalletProvider = null;
  }

  getDecVCPlatConnectedAddress(): string | null {
    return this.decvcplatConnectedAddress;
  }

  isDecVCPlatWalletConnected(): boolean {
    return this.decvcplatConnectedAddress !== null;
  }

  getDecVCPlatCurrentNetwork(): number | null {
    return this.decvcplatCurrentNetwork;
  }

  private getDecVCPlatWalletProviderName(): string {
    if (this.decvcplatWalletProvider?.isMetaMask) return 'MetaMask';
    if (this.decvcplatWalletProvider?.isCoinbaseWallet) return 'Coinbase Wallet';
    if (this.decvcplatWalletProvider?.isWalletConnect) return 'WalletConnect';
    return 'Unknown Wallet';
  }

  private encodeDecVCPlatStakeTransaction(decvcplatAmount: string): string {
    // Simplified encoding (would need proper ABI encoding)
    return `0xa694fc3a000000000000000000000000000000000000000000000000${decvcplatAmount}`;
  }

  private encodeDecVCPlatVoteTransaction(decvcplatProposalId: string, decvcplatVoteChoice: number): string {
    // Simplified encoding (would need proper ABI encoding)
    return `0x56781388${decvcplatProposalId.padStart(64, '0')}${decvcplatVoteChoice.toString(16).padStart(64, '0')}`;
  }
}

export const decvcplatWalletService = new DecVCPlatWalletService();
export default decvcplatWalletService;
