// © 2024 DecVCPlat. All rights reserved.

import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';

export interface VotingProposal {
  id: string;
  title: string;
  description: string;
  proposerId: string;
  proposerName: string;
  projectId?: string;
  proposalType: 'ProjectApproval' | 'FundingRelease' | 'Governance' | 'MilestoneApproval';
  status: 'Active' | 'Passed' | 'Failed' | 'Executed' | 'Cancelled';
  startTime: string;
  endTime: string;
  quorumThreshold: number;
  approvalThreshold: number;
  totalStaked: number;
  forVotes: number;
  againstVotes: number;
  abstainVotes: number;
  executionData?: string;
  createdAt: string;
  updatedAt: string;
}

export interface TokenStake {
  id: string;
  userId: string;
  proposalId: string;
  amount: number;
  lockPeriod: number;
  stakedAt: string;
  unstakedAt?: string;
  rewardsClaimed: number;
  isActive: boolean;
}

export interface Vote {
  id: string;
  proposalId: string;
  userId: string;
  userName: string;
  choice: 'For' | 'Against' | 'Abstain';
  votingPower: number;
  stakedAmount: number;
  comment?: string;
  timestamp: string;
  delegatedTo?: string;
  delegatedFrom?: string;
}

export interface UserVotingStats {
  totalStaked: number;
  totalVotingPower: number;
  activeProposals: number;
  votesCast: number;
  proposalsCreated: number;
  rewardsEarned: number;
  delegatedPower: number;
  receivedPower: number;
}

interface VotingState {
  proposals: VotingProposal[];
  userProposals: VotingProposal[];
  currentProposal: VotingProposal | null;
  userStakes: TokenStake[];
  userVotes: Vote[];
  userStats: UserVotingStats | null;
  isLoading: boolean;
  isStaking: boolean;
  isVoting: boolean;
  isCreatingProposal: boolean;
  error: string | null;
  filters: {
    status: string;
    type: string;
    search: string;
  };
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

const initialState: VotingState = {
  proposals: [],
  userProposals: [],
  currentProposal: null,
  userStakes: [],
  userVotes: [],
  userStats: null,
  isLoading: false,
  isStaking: false,
  isVoting: false,
  isCreatingProposal: false,
  error: null,
  filters: {
    status: 'Active',
    type: 'All',
    search: '',
  },
  pagination: {
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  },
};

// Async thunks
export const fetchProposals = createAsyncThunk(
  'voting/fetchProposals',
  async (params: { page?: number; pageSize?: number; status?: string; type?: string }, { rejectWithValue }) => {
    try {
      // Mock implementation
      const mockProposals: VotingProposal[] = [
        {
          id: '1',
          title: 'Approve AI Healthcare Platform Funding',
          description: 'Vote to approve initial funding for the AI-powered healthcare platform project.',
          proposerId: 'luminary1',
          proposerName: 'Alice Johnson',
          projectId: 'project1',
          proposalType: 'ProjectApproval',
          status: 'Active',
          startTime: '2024-02-01T10:00:00Z',
          endTime: '2024-02-08T10:00:00Z',
          quorumThreshold: 10000,
          approvalThreshold: 51,
          totalStaked: 15000,
          forVotes: 9000,
          againstVotes: 3000,
          abstainVotes: 3000,
          createdAt: '2024-02-01T10:00:00Z',
          updatedAt: '2024-02-05T14:30:00Z',
        },
        {
          id: '2',
          title: 'Release Milestone Funding - Energy Storage',
          description: 'Vote to release next tranche of funding upon completion of prototype milestone.',
          proposerId: 'luminary2',
          proposerName: 'Bob Smith',
          projectId: 'project2',
          proposalType: 'FundingRelease',
          status: 'Passed',
          startTime: '2024-01-25T10:00:00Z',
          endTime: '2024-02-01T10:00:00Z',
          quorumThreshold: 8000,
          approvalThreshold: 60,
          totalStaked: 12000,
          forVotes: 8500,
          againstVotes: 2000,
          abstainVotes: 1500,
          createdAt: '2024-01-25T10:00:00Z',
          updatedAt: '2024-02-01T12:00:00Z',
        },
      ];

      return {
        proposals: mockProposals,
        totalCount: mockProposals.length,
        totalPages: Math.ceil(mockProposals.length / (params.pageSize || 10)),
      };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch proposals');
    }
  }
);

export const fetchProposalById = createAsyncThunk(
  'voting/fetchProposalById',
  async (proposalId: string, { rejectWithValue }) => {
    try {
      // Mock implementation
      const mockProposal: VotingProposal = {
        id: proposalId,
        title: 'Approve AI Healthcare Platform Funding',
        description: 'Vote to approve initial funding for the AI-powered healthcare platform project. This proposal seeks community approval for the initial funding tranche of $300,000 to support MVP development and initial team hiring.',
        proposerId: 'luminary1',
        proposerName: 'Alice Johnson',
        projectId: 'project1',
        proposalType: 'ProjectApproval',
        status: 'Active',
        startTime: '2024-02-01T10:00:00Z',
        endTime: '2024-02-08T10:00:00Z',
        quorumThreshold: 10000,
        approvalThreshold: 51,
        totalStaked: 15000,
        forVotes: 9000,
        againstVotes: 3000,
        abstainVotes: 3000,
        createdAt: '2024-02-01T10:00:00Z',
        updatedAt: '2024-02-05T14:30:00Z',
      };

      return mockProposal;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch proposal');
    }
  }
);

export const stakeTokens = createAsyncThunk(
  'voting/stakeTokens',
  async (stakeData: { amount: number; lockPeriod?: number }, { rejectWithValue }) => {
    try {
      // Mock implementation
      const newStake: TokenStake = {
        id: Date.now().toString(),
        userId: 'current-user',
        proposalId: 'general',
        amount: stakeData.amount,
        lockPeriod: stakeData.lockPeriod || 30,
        stakedAt: new Date().toISOString(),
        rewardsClaimed: 0,
        isActive: true,
      };

      return newStake;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to stake tokens');
    }
  }
);

export const castVote = createAsyncThunk(
  'voting/castVote',
  async (voteData: {
    proposalId: string;
    choice: 'For' | 'Against' | 'Abstain';
    stakedAmount: number;
    comment?: string;
  }, { rejectWithValue }) => {
    try {
      // Mock implementation
      const newVote: Vote = {
        id: Date.now().toString(),
        proposalId: voteData.proposalId,
        userId: 'current-user',
        userName: 'Current User',
        choice: voteData.choice,
        votingPower: voteData.stakedAmount,
        stakedAmount: voteData.stakedAmount,
        comment: voteData.comment,
        timestamp: new Date().toISOString(),
      };

      return newVote;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to cast vote');
    }
  }
);

export const createProposal = createAsyncThunk(
  'voting/createProposal',
  async (proposalData: {
    title: string;
    description: string;
    proposalType: VotingProposal['proposalType'];
    projectId?: string;
    executionData?: string;
    votingPeriod?: number;
  }, { rejectWithValue }) => {
    try {
      // Mock implementation
      const endTime = new Date();
      endTime.setDate(endTime.getDate() + (proposalData.votingPeriod || 7));

      const newProposal: VotingProposal = {
        id: Date.now().toString(),
        title: proposalData.title,
        description: proposalData.description,
        proposerId: 'current-user',
        proposerName: 'Current User',
        projectId: proposalData.projectId,
        proposalType: proposalData.proposalType,
        status: 'Active',
        startTime: new Date().toISOString(),
        endTime: endTime.toISOString(),
        quorumThreshold: 10000,
        approvalThreshold: 51,
        totalStaked: 0,
        forVotes: 0,
        againstVotes: 0,
        abstainVotes: 0,
        executionData: proposalData.executionData,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };

      return newProposal;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create proposal');
    }
  }
);

export const fetchUserVotingStats = createAsyncThunk(
  'voting/fetchUserVotingStats',
  async (_, { rejectWithValue }) => {
    try {
      // Mock implementation
      const mockStats: UserVotingStats = {
        totalStaked: 50000,
        totalVotingPower: 55000,
        activeProposals: 3,
        votesCart: 12,
        proposalsCreated: 2,
        rewardsEarned: 2500,
        delegatedPower: 10000,
        receivedPower: 15000,
      };

      return mockStats;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user stats');
    }
  }
);

const votingSlice = createSlice({
  name: 'voting',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    setFilters: (state, action: PayloadAction<Partial<typeof initialState.filters>>) => {
      state.filters = { ...state.filters, ...action.payload };
    },
    setPagination: (state, action: PayloadAction<Partial<typeof initialState.pagination>>) => {
      state.pagination = { ...state.pagination, ...action.payload };
    },
    clearCurrentProposal: (state) => {
      state.currentProposal = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch Proposals
    builder
      .addCase(fetchProposals.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchProposals.fulfilled, (state, action) => {
        state.isLoading = false;
        state.proposals = action.payload.proposals;
        state.pagination.totalCount = action.payload.totalCount;
        state.pagination.totalPages = action.payload.totalPages;
      })
      .addCase(fetchProposals.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch Proposal by ID
    builder
      .addCase(fetchProposalById.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchProposalById.fulfilled, (state, action) => {
        state.isLoading = false;
        state.currentProposal = action.payload;
      })
      .addCase(fetchProposalById.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Stake Tokens
    builder
      .addCase(stakeTokens.pending, (state) => {
        state.isStaking = true;
        state.error = null;
      })
      .addCase(stakeTokens.fulfilled, (state, action) => {
        state.isStaking = false;
        state.userStakes.push(action.payload);
      })
      .addCase(stakeTokens.rejected, (state, action) => {
        state.isStaking = false;
        state.error = action.payload as string;
      });

    // Cast Vote
    builder
      .addCase(castVote.pending, (state) => {
        state.isVoting = true;
        state.error = null;
      })
      .addCase(castVote.fulfilled, (state, action) => {
        state.isVoting = false;
        state.userVotes.push(action.payload);
        
        // Update proposal vote counts if it's the current proposal
        if (state.currentProposal && state.currentProposal.id === action.payload.proposalId) {
          const vote = action.payload;
          state.currentProposal.totalStaked += vote.stakedAmount;
          
          if (vote.choice === 'For') {
            state.currentProposal.forVotes += vote.votingPower;
          } else if (vote.choice === 'Against') {
            state.currentProposal.againstVotes += vote.votingPower;
          } else {
            state.currentProposal.abstainVotes += vote.votingPower;
          }
        }
      })
      .addCase(castVote.rejected, (state, action) => {
        state.isVoting = false;
        state.error = action.payload as string;
      });

    // Create Proposal
    builder
      .addCase(createProposal.pending, (state) => {
        state.isCreatingProposal = true;
        state.error = null;
      })
      .addCase(createProposal.fulfilled, (state, action) => {
        state.isCreatingProposal = false;
        state.proposals.unshift(action.payload);
        state.userProposals.unshift(action.payload);
      })
      .addCase(createProposal.rejected, (state, action) => {
        state.isCreatingProposal = false;
        state.error = action.payload as string;
      });

    // Fetch User Voting Stats
    builder
      .addCase(fetchUserVotingStats.fulfilled, (state, action) => {
        state.userStats = action.payload;
      });
  },
});

export const { clearError, setFilters, setPagination, clearCurrentProposal } = votingSlice.actions;
export default votingSlice.reducer;
