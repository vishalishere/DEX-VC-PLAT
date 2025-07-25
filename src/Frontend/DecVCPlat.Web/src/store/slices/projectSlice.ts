// © 2024 DecVCPlat. All rights reserved.

import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';

export interface Project {
  id: string;
  title: string;
  description: string;
  fundingGoal: number;
  currentFunding: number;
  founderId: string;
  founderName: string;
  category: string;
  status: 'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Funded' | 'Completed';
  tags: string[];
  documents: ProjectDocument[];
  milestones: ProjectMilestone[];
  votes: ProjectVote[];
  createdAt: string;
  updatedAt: string;
  submittedAt?: string;
  approvedAt?: string;
  fundingDeadline?: string;
  imageUrl?: string;
  websiteUrl?: string;
  githubUrl?: string;
  pitchDeckUrl?: string;
}

export interface ProjectDocument {
  id: string;
  fileName: string;
  fileUrl: string;
  fileType: string;
  description?: string;
  uploadedAt: string;
}

export interface ProjectMilestone {
  id: string;
  projectId: string;
  title: string;
  description: string;
  targetAmount: number;
  targetDate: string;
  isCompleted: boolean;
  completedAt?: string;
  evidenceUrl?: string;
}

export interface ProjectVote {
  id: string;
  projectId: string;
  userId: string;
  userName: string;
  voteType: 'Approve' | 'Reject';
  stakedTokens: number;
  comment?: string;
  createdAt: string;
}

interface ProjectsState {
  projects: Project[];
  currentProject: Project | null;
  userProjects: Project[];
  isLoading: boolean;
  isCreating: boolean;
  isUpdating: boolean;
  error: string | null;
  filters: {
    status: string;
    category: string;
    search: string;
  };
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

const initialState: ProjectsState = {
  projects: [],
  currentProject: null,
  userProjects: [],
  isLoading: false,
  isCreating: false,
  isUpdating: false,
  error: null,
  filters: {
    status: 'All',
    category: 'All',
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
export const fetchProjects = createAsyncThunk(
  'projects/fetchProjects',
  async (params: { page?: number; pageSize?: number; status?: string; category?: string; search?: string }, { rejectWithValue }) => {
    try {
      // Mock implementation - replace with actual API call
      const mockProjects: Project[] = [
        {
          id: '1',
          title: 'AI-Powered Healthcare Platform',
          description: 'Revolutionary AI platform for early disease detection and personalized treatment recommendations.',
          fundingGoal: 1000000,
          currentFunding: 250000,
          founderId: 'founder1',
          founderName: 'John Smith',
          category: 'Healthcare',
          status: 'Approved',
          tags: ['AI', 'Healthcare', 'Machine Learning'],
          documents: [],
          milestones: [],
          votes: [],
          createdAt: '2024-01-15T10:00:00Z',
          updatedAt: '2024-01-20T15:30:00Z',
        },
        {
          id: '2',
          title: 'Sustainable Energy Storage',
          description: 'Next-generation battery technology for renewable energy storage solutions.',
          fundingGoal: 2000000,
          currentFunding: 500000,
          founderId: 'founder2',
          founderName: 'Sarah Johnson',
          category: 'Energy',
          status: 'Funded',
          tags: ['Energy', 'Sustainability', 'Battery Tech'],
          documents: [],
          milestones: [],
          votes: [],
          createdAt: '2024-01-10T08:00:00Z',
          updatedAt: '2024-01-25T12:00:00Z',
        },
      ];

      return {
        projects: mockProjects,
        totalCount: mockProjects.length,
        totalPages: Math.ceil(mockProjects.length / (params.pageSize || 10)),
      };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch projects');
    }
  }
);

export const fetchProjectById = createAsyncThunk(
  'projects/fetchProjectById',
  async (projectId: string, { rejectWithValue }) => {
    try {
      // Mock implementation - replace with actual API call
      const mockProject: Project = {
        id: projectId,
        title: 'AI-Powered Healthcare Platform',
        description: 'Revolutionary AI platform for early disease detection and personalized treatment recommendations.',
        fundingGoal: 1000000,
        currentFunding: 250000,
        founderId: 'founder1',
        founderName: 'John Smith',
        category: 'Healthcare',
        status: 'Approved',
        tags: ['AI', 'Healthcare', 'Machine Learning'],
        documents: [],
        milestones: [
          {
            id: '1',
            projectId,
            title: 'MVP Development',
            description: 'Complete minimum viable product',
            targetAmount: 300000,
            targetDate: '2024-06-01T00:00:00Z',
            isCompleted: false,
          },
        ],
        votes: [],
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-20T15:30:00Z',
      };

      return mockProject;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch project');
    }
  }
);

export const createProject = createAsyncThunk(
  'projects/createProject',
  async (projectData: Partial<Project>, { rejectWithValue }) => {
    try {
      // Mock implementation - replace with actual API call
      const newProject: Project = {
        id: Date.now().toString(),
        title: projectData.title || '',
        description: projectData.description || '',
        fundingGoal: projectData.fundingGoal || 0,
        currentFunding: 0,
        founderId: 'current-user',
        founderName: 'Current User',
        category: projectData.category || '',
        status: 'Draft',
        tags: projectData.tags || [],
        documents: [],
        milestones: [],
        votes: [],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };

      return newProject;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create project');
    }
  }
);

export const updateProject = createAsyncThunk(
  'projects/updateProject',
  async ({ projectId, updates }: { projectId: string; updates: Partial<Project> }, { rejectWithValue }) => {
    try {
      // Mock implementation - replace with actual API call
      return { projectId, updates };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update project');
    }
  }
);

export const voteOnProject = createAsyncThunk(
  'projects/voteOnProject',
  async (voteData: { projectId: string; voteType: 'Approve' | 'Reject'; stakedTokens: number; comment?: string }, { rejectWithValue }) => {
    try {
      // Mock implementation - replace with actual API call
      const newVote: ProjectVote = {
        id: Date.now().toString(),
        projectId: voteData.projectId,
        userId: 'current-user',
        userName: 'Current User',
        voteType: voteData.voteType,
        stakedTokens: voteData.stakedTokens,
        comment: voteData.comment,
        createdAt: new Date().toISOString(),
      };

      return newVote;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to vote on project');
    }
  }
);

const projectSlice = createSlice({
  name: 'projects',
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
    clearCurrentProject: (state) => {
      state.currentProject = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch Projects
    builder
      .addCase(fetchProjects.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchProjects.fulfilled, (state, action) => {
        state.isLoading = false;
        state.projects = action.payload.projects;
        state.pagination.totalCount = action.payload.totalCount;
        state.pagination.totalPages = action.payload.totalPages;
      })
      .addCase(fetchProjects.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch Project by ID
    builder
      .addCase(fetchProjectById.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchProjectById.fulfilled, (state, action) => {
        state.isLoading = false;
        state.currentProject = action.payload;
      })
      .addCase(fetchProjectById.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create Project
    builder
      .addCase(createProject.pending, (state) => {
        state.isCreating = true;
        state.error = null;
      })
      .addCase(createProject.fulfilled, (state, action) => {
        state.isCreating = false;
        state.projects.unshift(action.payload);
        state.userProjects.unshift(action.payload);
      })
      .addCase(createProject.rejected, (state, action) => {
        state.isCreating = false;
        state.error = action.payload as string;
      });

    // Update Project
    builder
      .addCase(updateProject.pending, (state) => {
        state.isUpdating = true;
        state.error = null;
      })
      .addCase(updateProject.fulfilled, (state, action) => {
        state.isUpdating = false;
        const { projectId, updates } = action.payload;
        
        // Update in projects array
        const projectIndex = state.projects.findIndex(p => p.id === projectId);
        if (projectIndex !== -1) {
          state.projects[projectIndex] = { ...state.projects[projectIndex], ...updates };
        }
        
        // Update current project if it's the same
        if (state.currentProject?.id === projectId) {
          state.currentProject = { ...state.currentProject, ...updates };
        }
      })
      .addCase(updateProject.rejected, (state, action) => {
        state.isUpdating = false;
        state.error = action.payload as string;
      });

    // Vote on Project
    builder
      .addCase(voteOnProject.fulfilled, (state, action) => {
        const vote = action.payload;
        
        // Add vote to current project
        if (state.currentProject && state.currentProject.id === vote.projectId) {
          state.currentProject.votes.push(vote);
        }
        
        // Add vote to projects array
        const projectIndex = state.projects.findIndex(p => p.id === vote.projectId);
        if (projectIndex !== -1) {
          state.projects[projectIndex].votes.push(vote);
        }
      });
  },
});

export const { clearError, setFilters, setPagination, clearCurrentProject } = projectSlice.actions;
export default projectSlice.reducer;
