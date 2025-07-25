// © 2024 DecVCPlat. All rights reserved.

import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';

export interface Notification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: 'ProjectUpdate' | 'VotingResult' | 'FundingRelease' | 'MilestoneComplete' | 'System' | 'Warning' | 'Success';
  priority: 'Low' | 'Normal' | 'High' | 'Critical';
  isRead: boolean;
  isArchived: boolean;
  actionUrl?: string;
  actionText?: string;
  metadata?: any;
  createdAt: string;
  readAt?: string;
  expiresAt?: string;
}

export interface NotificationPreferences {
  emailNotifications: boolean;
  pushNotifications: boolean;
  inAppNotifications: boolean;
  smsNotifications: boolean;
  projectUpdates: boolean;
  votingResults: boolean;
  fundingReleases: boolean;
  milestoneUpdates: boolean;
  systemAlerts: boolean;
  marketingEmails: boolean;
  weeklyDigest: boolean;
  instantAlerts: boolean;
  quietHoursEnabled: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
}

interface NotificationState {
  notifications: Notification[];
  unreadCount: number;
  preferences: NotificationPreferences | null;
  isLoading: boolean;
  isUpdatingPreferences: boolean;
  error: string | null;
  filters: {
    type: string;
    priority: string;
    isRead: boolean | null;
  };
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

const initialState: NotificationState = {
  notifications: [],
  unreadCount: 0,
  preferences: null,
  isLoading: false,
  isUpdatingPreferences: false,
  error: null,
  filters: {
    type: 'All',
    priority: 'All',
    isRead: null,
  },
  pagination: {
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0,
  },
};

// Async thunks
export const fetchNotifications = createAsyncThunk(
  'notifications/fetchNotifications',
  async (params: { page?: number; pageSize?: number; type?: string; isRead?: boolean }, { rejectWithValue }) => {
    try {
      // Mock implementation
      const mockNotifications: Notification[] = [
        {
          id: '1',
          userId: 'current-user',
          title: 'Project Approved for Funding',
          message: 'Your project "AI Healthcare Platform" has been approved for initial funding of $300,000.',
          type: 'ProjectUpdate',
          priority: 'High',
          isRead: false,
          isArchived: false,
          actionUrl: '/projects/1',
          actionText: 'View Project',
          createdAt: '2024-02-10T14:30:00Z',
        },
        {
          id: '2',
          userId: 'current-user',
          title: 'Voting Period Ended',
          message: 'Voting has ended for proposal "Energy Storage Funding". The proposal passed with 85% approval.',
          type: 'VotingResult',
          priority: 'Normal',
          isRead: false,
          isArchived: false,
          actionUrl: '/voting/2',
          actionText: 'View Results',
          createdAt: '2024-02-09T16:45:00Z',
        },
        {
          id: '3',
          userId: 'current-user',
          title: 'Milestone Completed',
          message: 'Milestone "MVP Development" has been completed and is awaiting community approval.',
          type: 'MilestoneComplete',
          priority: 'Normal',
          isRead: true,
          isArchived: false,
          actionUrl: '/projects/1/milestones',
          actionText: 'Review Milestone',
          createdAt: '2024-02-08T10:15:00Z',
          readAt: '2024-02-08T11:00:00Z',
        },
        {
          id: '4',
          userId: 'current-user',
          title: 'System Maintenance Scheduled',
          message: 'Scheduled maintenance will occur on February 15th from 2:00 AM to 4:00 AM UTC.',
          type: 'System',
          priority: 'Low',
          isRead: true,
          isArchived: false,
          createdAt: '2024-02-07T09:00:00Z',
          readAt: '2024-02-07T14:30:00Z',
        },
        {
          id: '5',
          userId: 'current-user',
          title: 'Funds Released Successfully',
          message: 'Tranche 2 funding of $200,000 has been released to your wallet.',
          type: 'FundingRelease',
          priority: 'High',
          isRead: false,
          isArchived: false,
          actionUrl: '/wallet',
          actionText: 'View Wallet',
          createdAt: '2024-02-06T13:20:00Z',
        },
      ];

      const unreadCount = mockNotifications.filter(n => !n.isRead).length;

      return {
        notifications: mockNotifications,
        unreadCount,
        totalCount: mockNotifications.length,
        totalPages: Math.ceil(mockNotifications.length / (params.pageSize || 20)),
      };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch notifications');
    }
  }
);

export const markAsRead = createAsyncThunk(
  'notifications/markAsRead',
  async (notificationIds: string[], { rejectWithValue }) => {
    try {
      // Mock implementation
      return { notificationIds, readAt: new Date().toISOString() };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to mark notifications as read');
    }
  }
);

export const markAllAsRead = createAsyncThunk(
  'notifications/markAllAsRead',
  async (_, { rejectWithValue }) => {
    try {
      // Mock implementation
      return { readAt: new Date().toISOString() };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to mark all notifications as read');
    }
  }
);

export const archiveNotifications = createAsyncThunk(
  'notifications/archiveNotifications',
  async (notificationIds: string[], { rejectWithValue }) => {
    try {
      // Mock implementation
      return { notificationIds };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to archive notifications');
    }
  }
);

export const fetchNotificationPreferences = createAsyncThunk(
  'notifications/fetchPreferences',
  async (_, { rejectWithValue }) => {
    try {
      // Mock implementation
      const mockPreferences: NotificationPreferences = {
        emailNotifications: true,
        pushNotifications: true,
        inAppNotifications: true,
        smsNotifications: false,
        projectUpdates: true,
        votingResults: true,
        fundingReleases: true,
        milestoneUpdates: true,
        systemAlerts: true,
        marketingEmails: false,
        weeklyDigest: true,
        instantAlerts: true,
        quietHoursEnabled: false,
        quietHoursStart: '22:00',
        quietHoursEnd: '08:00',
      };

      return mockPreferences;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch notification preferences');
    }
  }
);

export const updateNotificationPreferences = createAsyncThunk(
  'notifications/updatePreferences',
  async (preferences: Partial<NotificationPreferences>, { rejectWithValue }) => {
    try {
      // Mock implementation
      return preferences;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update notification preferences');
    }
  }
);

const notificationSlice = createSlice({
  name: 'notifications',
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
    addNotification: (state, action: PayloadAction<Notification>) => {
      state.notifications.unshift(action.payload);
      if (!action.payload.isRead) {
        state.unreadCount += 1;
      }
    },
    removeNotification: (state, action: PayloadAction<string>) => {
      const notificationIndex = state.notifications.findIndex(n => n.id === action.payload);
      if (notificationIndex !== -1) {
        const notification = state.notifications[notificationIndex];
        if (!notification.isRead) {
          state.unreadCount -= 1;
        }
        state.notifications.splice(notificationIndex, 1);
      }
    },
    clearAllNotifications: (state) => {
      state.notifications = [];
      state.unreadCount = 0;
    },
  },
  extraReducers: (builder) => {
    // Fetch Notifications
    builder
      .addCase(fetchNotifications.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchNotifications.fulfilled, (state, action) => {
        state.isLoading = false;
        state.notifications = action.payload.notifications;
        state.unreadCount = action.payload.unreadCount;
        state.pagination.totalCount = action.payload.totalCount;
        state.pagination.totalPages = action.payload.totalPages;
      })
      .addCase(fetchNotifications.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Mark as Read
    builder
      .addCase(markAsRead.fulfilled, (state, action) => {
        const { notificationIds, readAt } = action.payload;
        
        notificationIds.forEach(id => {
          const notification = state.notifications.find(n => n.id === id);
          if (notification && !notification.isRead) {
            notification.isRead = true;
            notification.readAt = readAt;
            state.unreadCount -= 1;
          }
        });
      });

    // Mark All as Read
    builder
      .addCase(markAllAsRead.fulfilled, (state, action) => {
        const { readAt } = action.payload;
        
        state.notifications.forEach(notification => {
          if (!notification.isRead) {
            notification.isRead = true;
            notification.readAt = readAt;
          }
        });
        state.unreadCount = 0;
      });

    // Archive Notifications
    builder
      .addCase(archiveNotifications.fulfilled, (state, action) => {
        const { notificationIds } = action.payload;
        
        notificationIds.forEach(id => {
          const notification = state.notifications.find(n => n.id === id);
          if (notification) {
            notification.isArchived = true;
          }
        });
      });

    // Fetch Notification Preferences
    builder
      .addCase(fetchNotificationPreferences.fulfilled, (state, action) => {
        state.preferences = action.payload;
      });

    // Update Notification Preferences
    builder
      .addCase(updateNotificationPreferences.pending, (state) => {
        state.isUpdatingPreferences = true;
        state.error = null;
      })
      .addCase(updateNotificationPreferences.fulfilled, (state, action) => {
        state.isUpdatingPreferences = false;
        if (state.preferences) {
          state.preferences = { ...state.preferences, ...action.payload };
        }
      })
      .addCase(updateNotificationPreferences.rejected, (state, action) => {
        state.isUpdatingPreferences = false;
        state.error = action.payload as string;
      });
  },
});

export const { 
  clearError, 
  setFilters, 
  setPagination, 
  addNotification, 
  removeNotification, 
  clearAllNotifications 
} = notificationSlice.actions;

// Export alias for compatibility with NotificationsPage
export const markNotificationsAsRead = markAsRead;

export default notificationSlice.reducer;
