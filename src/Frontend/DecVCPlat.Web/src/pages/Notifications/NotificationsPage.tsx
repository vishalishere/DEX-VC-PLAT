// © 2024 DecVCPlat. All rights reserved.

import React, { useEffect, useState } from 'react';
import { Container, Box, Typography, Button, TextField, FormControl, InputLabel, Select, MenuItem, Tabs, Tab, Pagination, useTheme, Paper, Divider } from '@mui/material';
import { MarkEmailRead, Delete, Settings, Search, FilterList, Notifications } from '@mui/icons-material';
import { useAuth } from '../../hooks/useAuth';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { fetchNotifications, markNotificationsAsRead, archiveNotifications, setFilters, setPagination } from '../../store/slices/notificationSlice';
import NotificationItem from '../../components/Notification/NotificationItem';
import LoadingSpinner from '../../components/Common/LoadingSpinner';
import { toast } from 'react-hot-toast';
import { Helmet } from 'react-helmet-async';

interface DecVCPlatTabPanelProps {
  children?: React.ReactNode;
  decvcplatTabIndex: number;
  decvcplatCurrentValue: number;
}

function DecVCPlatTabPanel({ children, decvcplatCurrentValue, decvcplatTabIndex }: DecVCPlatTabPanelProps) {
  return (
    <div hidden={decvcplatCurrentValue !== decvcplatTabIndex}>
      {decvcplatCurrentValue === decvcplatTabIndex && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

const NotificationsPage: React.FC = () => {
  const decvcplatTheme = useTheme();
  const decvcplatAuth = useAuth();
  const decvcplatDispatch = useAppDispatch();
  
  const { 
    notifications: decvcplatNotificationList, 
    isLoading: decvcplatNotificationsLoading,
    unreadCount: decvcplatUnreadCount,
    filters: decvcplatCurrentFilters, 
    pagination: decvcplatPaginationState 
  } = useAppSelector(state => state.notifications);
  
  const [decvcplatActiveTab, setDecVCPlatActiveTab] = useState(0);
  const [decvcplatSearchInput, setDecVCPlatSearchInput] = useState('');
  const [decvcplatShowFilters, setDecVCPlatShowFilters] = useState(false);
  const [decvcplatSelectedNotifications, setDecVCPlatSelectedNotifications] = useState<string[]>([]);

  useEffect(() => {
    const decvcplatFilterStatus = decvcplatActiveTab === 0 ? 'All' : decvcplatActiveTab === 1 ? 'Unread' : 'Read';
    
    decvcplatDispatch(fetchNotifications({
      page: decvcplatPaginationState.page,
      pageSize: decvcplatPaginationState.pageSize,
      status: decvcplatFilterStatus,
      type: decvcplatCurrentFilters.type,
      priority: decvcplatCurrentFilters.priority,
    }));
  }, [decvcplatDispatch, decvcplatActiveTab, decvcplatPaginationState.page, decvcplatCurrentFilters]);

  const handleDecVCPlatTabChange = (_: React.SyntheticEvent, decvcplatNewValue: number) => {
    setDecVCPlatActiveTab(decvcplatNewValue);
    decvcplatDispatch(setPagination({ page: 1 }));
  };

  const handleDecVCPlatSearch = () => {
    decvcplatDispatch(setFilters({ search: decvcplatSearchInput }));
    decvcplatDispatch(setPagination({ page: 1 }));
  };

  const handleDecVCPlatFilterChange = (decvcplatFilterType: string, decvcplatFilterValue: string) => {
    decvcplatDispatch(setFilters({ [decvcplatFilterType]: decvcplatFilterValue }));
    decvcplatDispatch(setPagination({ page: 1 }));
  };

  const handleDecVCPlatPageChange = (_: React.ChangeEvent<unknown>, decvcplatNewPage: number) => {
    decvcplatDispatch(setPagination({ page: decvcplatNewPage }));
  };

  const handleDecVCPlatMarkAsRead = async (decvcplatNotificationId: string) => {
    try {
      const decvcplatResult = await decvcplatDispatch(markNotificationsAsRead([decvcplatNotificationId]));
      if (markNotificationsAsRead.fulfilled.match(decvcplatResult)) {
        toast.success('DecVCPlat notification marked as read');
      }
    } catch (decvcplatError) {
      toast.error('Failed to mark DecVCPlat notification as read');
    }
  };

  const handleDecVCPlatDismissNotification = async (decvcplatNotificationId: string) => {
    try {
      const decvcplatResult = await decvcplatDispatch(archiveNotifications([decvcplatNotificationId]));
      if (archiveNotifications.fulfilled.match(decvcplatResult)) {
        toast.success('DecVCPlat notification dismissed');
      }
    } catch (decvcplatError) {
      toast.error('Failed to dismiss DecVCPlat notification');
    }
  };

  const handleDecVCPlatMarkAllAsRead = async () => {
    const decvcplatUnreadIds = decvcplatNotificationList
      .filter(n => !n.isRead)
      .map(n => n.id);

    if (decvcplatUnreadIds.length === 0) {
      toast.info('No unread DecVCPlat notifications to mark');
      return;
    }

    try {
      const decvcplatResult = await decvcplatDispatch(markNotificationsAsRead(decvcplatUnreadIds));
      if (markNotificationsAsRead.fulfilled.match(decvcplatResult)) {
        toast.success(`Marked ${decvcplatUnreadIds.length} DecVCPlat notifications as read`);
      }
    } catch (decvcplatError) {
      toast.error('Failed to mark all DecVCPlat notifications as read');
    }
  };

  const getDecVCPlatFilteredNotifications = () => {
    return decvcplatNotificationList.filter(decvcplatNotification => {
      const decvcplatMatchesSearch = !decvcplatSearchInput || 
        decvcplatNotification.title.toLowerCase().includes(decvcplatSearchInput.toLowerCase()) ||
        decvcplatNotification.message.toLowerCase().includes(decvcplatSearchInput.toLowerCase());

      return decvcplatMatchesSearch;
    });
  };

  if (decvcplatNotificationsLoading) {
    return <LoadingSpinner message="Loading your DecVCPlat notifications..." />;
  }

  const decvcplatFilteredNotifications = getDecVCPlatFilteredNotifications();

  return (
    <>
      <Helmet>
        <title>DecVCPlat Notifications - Stay Updated</title>
        <meta name="description" content="Manage your DecVCPlat notifications, stay informed about project updates, voting results, and platform announcements." />
      </Helmet>

      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}>
          <Box>
            <Typography variant="h4" fontWeight={600} gutterBottom>
              DecVCPlat Notifications
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Stay updated with your DecVCPlat activities and platform announcements
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', gap: 2 }}>
            <Button
              variant="outlined"
              startIcon={<MarkEmailRead />}
              onClick={handleDecVCPlatMarkAllAsRead}
              disabled={decvcplatUnreadCount === 0}
            >
              Mark All Read ({decvcplatUnreadCount})
            </Button>
            <Button
              variant="outlined"
              startIcon={<Settings />}
            >
              Notification Settings
            </Button>
          </Box>
        </Box>

        <Paper elevation={1} sx={{ mb: 4 }}>
          <Tabs value={decvcplatActiveTab} onChange={handleDecVCPlatTabChange} sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tab 
              label={`All Notifications (${decvcplatPaginationState.totalCount})`} 
              icon={<Notifications />} 
              iconPosition="start" 
            />
            <Tab 
              label={`Unread (${decvcplatUnreadCount})`} 
              icon={<Notifications color={decvcplatUnreadCount > 0 ? 'error' : 'disabled'} />} 
              iconPosition="start" 
            />
            <Tab 
              label="Read" 
              icon={<MarkEmailRead />} 
              iconPosition="start" 
            />
          </Tabs>

          <Box sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: decvcplatShowFilters ? 3 : 0 }}>
              <TextField
                placeholder="Search DecVCPlat notifications..."
                value={decvcplatSearchInput}
                onChange={(e) => setDecVCPlatSearchInput(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleDecVCPlatSearch()}
                InputProps={{
                  startAdornment: <Search color="action" sx={{ mr: 1 }} />,
                }}
                size="small"
                sx={{ flex: 1 }}
              />
              <Button
                variant="contained"
                onClick={handleDecVCPlatSearch}
                size="small"
                sx={{ minWidth: 100 }}
              >
                Search
              </Button>
              <Button
                variant="outlined"
                startIcon={<FilterList />}
                onClick={() => setDecVCPlatShowFilters(!decvcplatShowFilters)}
                size="small"
              >
                Filters
              </Button>
            </Box>

            {decvcplatShowFilters && (
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <FormControl size="small" sx={{ minWidth: 150 }}>
                  <InputLabel>Notification Type</InputLabel>
                  <Select
                    value={decvcplatCurrentFilters.type}
                    label="Notification Type"
                    onChange={(e) => handleDecVCPlatFilterChange('type', e.target.value)}
                  >
                    <MenuItem value="All">All Types</MenuItem>
                    <MenuItem value="ProjectUpdate">Project Update</MenuItem>
                    <MenuItem value="VotingResult">Voting Result</MenuItem>
                    <MenuItem value="FundingRelease">Funding Release</MenuItem>
                    <MenuItem value="MilestoneComplete">Milestone Complete</MenuItem>
                    <MenuItem value="System">System</MenuItem>
                  </Select>
                </FormControl>
                <FormControl size="small" sx={{ minWidth: 120 }}>
                  <InputLabel>Priority</InputLabel>
                  <Select
                    value={decvcplatCurrentFilters.priority}
                    label="Priority"
                    onChange={(e) => handleDecVCPlatFilterChange('priority', e.target.value)}
                  >
                    <MenuItem value="All">All Priorities</MenuItem>
                    <MenuItem value="Critical">Critical</MenuItem>
                    <MenuItem value="High">High</MenuItem>
                    <MenuItem value="Normal">Normal</MenuItem>
                    <MenuItem value="Low">Low</MenuItem>
                  </Select>
                </FormControl>
              </Box>
            )}
          </Box>
        </Paper>

        <DecVCPlatTabPanel decvcplatCurrentValue={decvcplatActiveTab} decvcplatTabIndex={0}>
          {decvcplatFilteredNotifications.length === 0 ? (
            <Paper elevation={1} sx={{ p: 6, textAlign: 'center' }}>
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No DecVCPlat notifications found
              </Typography>
              <Typography variant="body2" color="text.secondary">
                You're all caught up! New DecVCPlat notifications will appear here as they arrive.
              </Typography>
            </Paper>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {decvcplatFilteredNotifications.map((decvcplatNotification, decvcplatIndex) => (
                <React.Fragment key={decvcplatNotification.id}>
                  <NotificationItem
                    decvcplatNotification={decvcplatNotification}
                    decvcplatShowActions={true}
                    decvcplatCompactMode={false}
                    onDecVCPlatMarkAsRead={handleDecVCPlatMarkAsRead}
                    onDecVCPlatDismiss={handleDecVCPlatDismissNotification}
                  />
                  {decvcplatIndex < decvcplatFilteredNotifications.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </Box>
          )}
        </DecVCPlatTabPanel>

        <DecVCPlatTabPanel decvcplatCurrentValue={decvcplatActiveTab} decvcplatTabIndex={1}>
          {decvcplatFilteredNotifications.filter(n => !n.isRead).length === 0 ? (
            <Paper elevation={1} sx={{ p: 6, textAlign: 'center' }}>
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No unread DecVCPlat notifications
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Great! You've read all your DecVCPlat notifications. Check back later for updates.
              </Typography>
            </Paper>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {decvcplatFilteredNotifications.filter(n => !n.isRead).map((decvcplatNotification, decvcplatIndex, decvcplatUnreadArray) => (
                <React.Fragment key={decvcplatNotification.id}>
                  <NotificationItem
                    decvcplatNotification={decvcplatNotification}
                    decvcplatShowActions={true}
                    decvcplatCompactMode={false}
                    onDecVCPlatMarkAsRead={handleDecVCPlatMarkAsRead}
                    onDecVCPlatDismiss={handleDecVCPlatDismissNotification}
                  />
                  {decvcplatIndex < decvcplatUnreadArray.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </Box>
          )}
        </DecVCPlatTabPanel>

        <DecVCPlatTabPanel decvcplatCurrentValue={decvcplatActiveTab} decvcplatTabIndex={2}>
          {decvcplatFilteredNotifications.filter(n => n.isRead).length === 0 ? (
            <Paper elevation={1} sx={{ p: 6, textAlign: 'center' }}>
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No read DecVCPlat notifications
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Read DecVCPlat notifications will appear here for your reference.
              </Typography>
            </Paper>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {decvcplatFilteredNotifications.filter(n => n.isRead).map((decvcplatNotification, decvcplatIndex, decvcplatReadArray) => (
                <React.Fragment key={decvcplatNotification.id}>
                  <NotificationItem
                    decvcplatNotification={decvcplatNotification}
                    decvcplatShowActions={true}
                    decvcplatCompactMode={false}
                    onDecVCPlatMarkAsRead={handleDecVCPlatMarkAsRead}
                    onDecVCPlatDismiss={handleDecVCPlatDismissNotification}
                  />
                  {decvcplatIndex < decvcplatReadArray.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </Box>
          )}
        </DecVCPlatTabPanel>

        {decvcplatPaginationState.totalPages > 1 && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <Pagination
              count={decvcplatPaginationState.totalPages}
              page={decvcplatPaginationState.page}
              onChange={handleDecVCPlatPageChange}
              color="primary"
              size="large"
              showFirstButton
              showLastButton
            />
          </Box>
        )}
      </Container>
    </>
  );
};

export default NotificationsPage;
