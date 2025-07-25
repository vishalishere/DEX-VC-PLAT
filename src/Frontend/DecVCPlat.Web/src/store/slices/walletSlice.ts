// © 2024 DecVCPlat. All rights reserved.

import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';

export interface DecVCPlatWallet {
  address: string;
  walletType: 'MetaMask' | 'WalletConnect' | 'Coinbase' | 'DecVCPlat';
  networkId: number;
  connected: boolean;
  ethBalance: string;
  decvcplatTokenBalance: string;
  networkDisplayName: string;
}

export interface DecVCPlatTokenInfo {
  tokenSymbol: string;
  tokenName: string;
  contractAddress: string;
  userBalance: string;
  tokenDecimals: number;
  tokenIconUrl?: string;
  usdEquivalent?: number;
}

export interface DecVCPlatTransaction {
  transactionHash: string;
  fromAddress: string;
  toAddress: string;
  amountTransferred: string;
  gasConsumed: string;
  gasCostInGwei: string;
  blockTimestamp: string;
  confirmationStatus: 'pending' | 'confirmed' | 'failed';
  transactionCategory: 'stake' | 'unstake' | 'vote' | 'transfer' | 'funding' | 'reward';
  transactionNote?: string;
  blockHeight?: number;
}

export interface DecVCPlatStakingData {
  currentlyStakedAmount: string;
  availableForStaking: string;
  earnedStakingRewards: string;
  unstakingWaitPeriod: number;
  nextRewardDistribution?: string;
  annualPercentageYield: number;
}

interface DecVCPlatWalletState {
  walletConnection: DecVCPlatWallet | null;
  userTokenBalances: DecVCPlatTokenInfo[];
  userTransactionHistory: DecVCPlatTransaction[];
  userStakingData: DecVCPlatStakingData | null;
  connectingToWallet: boolean;
  fetchingBalances: boolean;
  fetchingTransactionHistory: boolean;
  processingStakeRequest: boolean;
  processingUnstakeRequest: boolean;
  walletErrorMessage: string | null;
  supportedNetworks: Array<{
    networkId: number;
    networkName: string;
    rpcEndpoint: string;
    blockExplorerUrl: string;
  }>;
}

const decvcplatWalletInitialState: DecVCPlatWalletState = {
  walletConnection: null,
  userTokenBalances: [],
  userTransactionHistory: [],
  userStakingData: null,
  connectingToWallet: false,
  fetchingBalances: false,
  fetchingTransactionHistory: false,
  processingStakeRequest: false,
  processingUnstakeRequest: false,
  walletErrorMessage: null,
  supportedNetworks: [
    {
      networkId: 1,
      networkName: 'Ethereum Mainnet',
      rpcEndpoint: 'https://mainnet.infura.io/v3/decvcplat',
      blockExplorerUrl: 'https://etherscan.io',
    },
    {
      networkId: 11155111,
      networkName: 'Sepolia Testnet',
      rpcEndpoint: 'https://sepolia.infura.io/v3/decvcplat',
      blockExplorerUrl: 'https://sepolia.etherscan.io',
    },
  ],
};

// DecVCPlat-specific async operations
export const establishWalletConnection = createAsyncThunk(
  'decvcplatWallet/establishWalletConnection',
  async (walletProvider: 'MetaMask' | 'WalletConnect' | 'Coinbase', { rejectWithValue }) => {
    try {
      const decvcplatWalletConnection: DecVCPlatWallet = {
        address: '0xDecVCPlat1234567890123456789012345678',
        walletType: walletProvider,
        networkId: 1,
        connected: true,
        ethBalance: '2.1234',
        decvcplatTokenBalance: '75000.0',
        networkDisplayName: 'Ethereum Mainnet',
      };

      localStorage.setItem('decvcplat_wallet_state', JSON.stringify(decvcplatWalletConnection));
      return decvcplatWalletConnection;
    } catch (decvcplatError: any) {
      return rejectWithValue(decvcplatError.message || 'DecVCPlat wallet connection failed');
    }
  }
);

export const terminateWalletConnection = createAsyncThunk(
  'decvcplatWallet/terminateWalletConnection',
  async (_, { rejectWithValue }) => {
    try {
      localStorage.removeItem('decvcplat_wallet_state');
      return null;
    } catch (decvcplatError: any) {
      return rejectWithValue(decvcplatError.message || 'DecVCPlat wallet disconnection failed');
    }
  }
);

export const retrieveTokenBalances = createAsyncThunk(
  'decvcplatWallet/retrieveTokenBalances',
  async (_, { getState, rejectWithValue }) => {
    try {
      const currentState = getState() as { wallet: DecVCPlatWalletState };
      if (!currentState.wallet.walletConnection) {
        throw new Error('DecVCPlat wallet not connected');
      }

      const decvcplatTokenBalances: DecVCPlatTokenInfo[] = [
        {
          tokenSymbol: 'ETH',
          tokenName: 'Ethereum',
          contractAddress: '0x0000000000000000000000000000000000000000',
          userBalance: '2.1234',
          tokenDecimals: 18,
          usdEquivalent: 4800.56,
        },
        {
          tokenSymbol: 'DVCP',
          tokenName: 'DecVCPlat Token',
          contractAddress: '0xDecVCPlatToken1111111111111111111111111',
          userBalance: '75000.0',
          tokenDecimals: 18,
          usdEquivalent: 37500.0,
        },
        {
          tokenSymbol: 'USDC',
          tokenName: 'USD Coin',
          contractAddress: '0xDecVCPlatUSDC2222222222222222222222222',
          userBalance: '5000.0',
          tokenDecimals: 6,
          usdEquivalent: 5000.0,
        },
      ];

      return decvcplatTokenBalances;
    } catch (decvcplatError: any) {
      return rejectWithValue(decvcplatError.message || 'DecVCPlat balance retrieval failed');
    }
  }
);

export const retrieveTransactionHistory = createAsyncThunk(
  'decvcplatWallet/retrieveTransactionHistory',
  async (_, { getState, rejectWithValue }) => {
    try {
      const currentState = getState() as { wallet: DecVCPlatWalletState };
      if (!currentState.wallet.walletConnection) {
        throw new Error('DecVCPlat wallet not connected');
      }

      const decvcplatTransactionHistory: DecVCPlatTransaction[] = [
        {
          transactionHash: '0xDecVCPlat123abc456def789ghi012jkl345mno678pqr901stu234vwx567yz',
          fromAddress: '0xDecVCPlat1234567890123456789012345678',
          toAddress: '0xDecVCPlatStaking9876543210987654321098765432',
          amountTransferred: '10000.0',
          gasConsumed: '45000',
          gasCostInGwei: '25',
          blockTimestamp: '2024-02-10T14:30:00Z',
          confirmationStatus: 'confirmed',
          transactionCategory: 'stake',
          transactionNote: 'DecVCPlat governance token staking',
          blockHeight: 19456789,
        },
        {
          transactionHash: '0xDecVCPlat456def789ghi012jkl345mno678pqr901stu234vwx567yz123abc',
          fromAddress: '0xDecVCPlatRewards9876543210987654321098765432',
          toAddress: '0xDecVCPlat1234567890123456789012345678',
          amountTransferred: '750.0',
          gasConsumed: '21000',
          gasCostInGwei: '20',
          blockTimestamp: '2024-02-09T10:15:00Z',
          confirmationStatus: 'confirmed',
          transactionCategory: 'reward',
          transactionNote: 'DecVCPlat staking rewards distribution',
          blockHeight: 19456700,
        },
      ];

      return decvcplatTransactionHistory;
    } catch (decvcplatError: any) {
      return rejectWithValue(decvcplatError.message || 'DecVCPlat transaction history retrieval failed');
    }
  }
);

export const retrieveStakingData = createAsyncThunk(
  'decvcplatWallet/retrieveStakingData',
  async (_, { getState, rejectWithValue }) => {
    try {
      const currentState = getState() as { wallet: DecVCPlatWalletState };
      if (!currentState.wallet.walletConnection) {
        throw new Error('DecVCPlat wallet not connected');
      }

      const decvcplatStakingData: DecVCPlatStakingData = {
        currentlyStakedAmount: '50000.0',
        availableForStaking: '25000.0',
        earnedStakingRewards: '2750.0',
        unstakingWaitPeriod: 7,
        nextRewardDistribution: '2024-02-17T10:00:00Z',
        annualPercentageYield: 15.0,
      };

      return decvcplatStakingData;
    } catch (decvcplatError: any) {
      return rejectWithValue(decvcplatError.message || 'DecVCPlat staking data retrieval failed');
    }
  }
);

export const executeTokenStaking = createAsyncThunk(
  'decvcplatWallet/executeTokenStaking',
  async (stakingAmount: string, { getState, rejectWithValue }) => {
    try {
      const currentState = getState() as { wallet: DecVCPlatWalletState };
      if (!currentState.wallet.walletConnection) {
        throw new Error('DecVCPlat wallet not connected');
      }

      const decvcplatStakingTransaction: DecVCPlatTransaction = {
        transactionHash: '0xDecVCPlatStake' + Math.random().toString(16).substring(2, 50),
        fromAddress: currentState.wallet.walletConnection.address,
        toAddress: '0xDecVCPlatStaking9876543210987654321098765432',
        amountTransferred: stakingAmount,
        gasConsumed: '0',
        gasCostInGwei: '0',
        blockTimestamp: new Date().toISOString(),
        confirmationStatus: 'pending',
        transactionCategory: 'stake',
        transactionNote: `DecVCPlat ${stakingAmount} DVCP tokens staked for governance`,
      };

      return decvcplatStakingTransaction;
    } catch (decvcplatError: any) {
      return rejectWithValue(decvcplatError.message || 'DecVCPlat token staking failed');
    }
  }
);

export const executeTokenUnstaking = createAsyncThunk(
  'decvcplatWallet/executeTokenUnstaking',
  async (unstakingAmount: string, { getState, rejectWithValue }) => {
    try {
      const currentState = getState() as { wallet: DecVCPlatWalletState };
      if (!currentState.wallet.walletConnection) {
        throw new Error('DecVCPlat wallet not connected');
      }

      const decvcplatUnstakingTransaction: DecVCPlatTransaction = {
        transactionHash: '0xDecVCPlatUnstake' + Math.random().toString(16).substring(2, 48),
        fromAddress: '0xDecVCPlatStaking9876543210987654321098765432',
        toAddress: currentState.wallet.walletConnection.address,
        amountTransferred: unstakingAmount,
        gasConsumed: '0',
        gasCostInGwei: '0',
        blockTimestamp: new Date().toISOString(),
        confirmationStatus: 'pending',
        transactionCategory: 'unstake',
        transactionNote: `DecVCPlat ${unstakingAmount} DVCP tokens unstaked from governance`,
      };

      return decvcplatUnstakingTransaction;
    } catch (decvcplatError: any) {
      return rejectWithValue(decvcplatError.message || 'DecVCPlat token unstaking failed');
    }
  }
);

const decvcplatWalletSlice = createSlice({
  name: 'decvcplatWallet',
  initialState: decvcplatWalletInitialState,
  reducers: {
    clearWalletError: (state) => {
      state.walletErrorMessage = null;
    },
    updateTransactionConfirmation: (state, action: PayloadAction<{ hash: string; status: 'confirmed' | 'failed'; blockHeight?: number }>) => {
      const targetTransaction = state.userTransactionHistory.find(tx => tx.transactionHash === action.payload.hash);
      if (targetTransaction) {
        targetTransaction.confirmationStatus = action.payload.status;
        if (action.payload.blockHeight) {
          targetTransaction.blockHeight = action.payload.blockHeight;
        }
      }
    },
    appendNewTransaction: (state, action: PayloadAction<DecVCPlatTransaction>) => {
      state.userTransactionHistory.unshift(action.payload);
    },
    restoreWalletFromStorage: (state) => {
      const savedDecVCPlatWallet = localStorage.getItem('decvcplat_wallet_state');
      if (savedDecVCPlatWallet) {
        try {
          state.walletConnection = JSON.parse(savedDecVCPlatWallet);
        } catch {
          localStorage.removeItem('decvcplat_wallet_state');
        }
      }
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(establishWalletConnection.pending, (state) => {
        state.connectingToWallet = true;
        state.walletErrorMessage = null;
      })
      .addCase(establishWalletConnection.fulfilled, (state, action) => {
        state.connectingToWallet = false;
        state.walletConnection = action.payload;
      })
      .addCase(establishWalletConnection.rejected, (state, action) => {
        state.connectingToWallet = false;
        state.walletErrorMessage = action.payload as string;
      });

    builder
      .addCase(terminateWalletConnection.fulfilled, (state) => {
        state.walletConnection = null;
        state.userTokenBalances = [];
        state.userTransactionHistory = [];
        state.userStakingData = null;
      });

    builder
      .addCase(retrieveTokenBalances.pending, (state) => {
        state.fetchingBalances = true;
        state.walletErrorMessage = null;
      })
      .addCase(retrieveTokenBalances.fulfilled, (state, action) => {
        state.fetchingBalances = false;
        state.userTokenBalances = action.payload;
      })
      .addCase(retrieveTokenBalances.rejected, (state, action) => {
        state.fetchingBalances = false;
        state.walletErrorMessage = action.payload as string;
      });

    builder
      .addCase(retrieveTransactionHistory.pending, (state) => {
        state.fetchingTransactionHistory = true;
        state.walletErrorMessage = null;
      })
      .addCase(retrieveTransactionHistory.fulfilled, (state, action) => {
        state.fetchingTransactionHistory = false;
        state.userTransactionHistory = action.payload;
      })
      .addCase(retrieveTransactionHistory.rejected, (state, action) => {
        state.fetchingTransactionHistory = false;
        state.walletErrorMessage = action.payload as string;
      });

    builder
      .addCase(retrieveStakingData.fulfilled, (state, action) => {
        state.userStakingData = action.payload;
      });

    builder
      .addCase(executeTokenStaking.pending, (state) => {
        state.processingStakeRequest = true;
        state.walletErrorMessage = null;
      })
      .addCase(executeTokenStaking.fulfilled, (state, action) => {
        state.processingStakeRequest = false;
        state.userTransactionHistory.unshift(action.payload);
      })
      .addCase(executeTokenStaking.rejected, (state, action) => {
        state.processingStakeRequest = false;
        state.walletErrorMessage = action.payload as string;
      });

    builder
      .addCase(executeTokenUnstaking.pending, (state) => {
        state.processingUnstakeRequest = true;
        state.walletErrorMessage = null;
      })
      .addCase(executeTokenUnstaking.fulfilled, (state, action) => {
        state.processingUnstakeRequest = false;
        state.userTransactionHistory.unshift(action.payload);
      })
      .addCase(executeTokenUnstaking.rejected, (state, action) => {
        state.processingUnstakeRequest = false;
        state.walletErrorMessage = action.payload as string;
      });
  },
});

export const { 
  clearWalletError, 
  updateTransactionConfirmation, 
  appendNewTransaction, 
  restoreWalletFromStorage 
} = decvcplatWalletSlice.actions;

// Export aliases for compatibility with WalletPage
export const disconnectWallet = terminateWalletConnection;
export const stakeDecVCPlatTokens = executeTokenStaking;
export const unstakeDecVCPlatTokens = executeTokenUnstaking;
export const fetchDecVCPlatTransactionHistory = retrieveTransactionHistory;

// Network switching function - create a basic implementation
export const switchDecVCPlatNetwork = createAsyncThunk(
  'decvcplatWallet/switchDecVCPlatNetwork',
  async (networkId: number, { rejectWithValue }) => {
    try {
      // Mock implementation for network switching
      const networkInfo = networkId === 1 
        ? { networkId: 1, networkName: 'Ethereum Mainnet' }
        : { networkId: 11155111, networkName: 'Sepolia Testnet' };
      
      return networkInfo;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Network switching failed');
    }
  }
);

export default decvcplatWalletSlice.reducer;
