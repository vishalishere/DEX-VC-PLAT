import React, { useEffect, useState } from 'react';
import {
  Box,
  Grid,
  Paper,
  Typography,
  Card,
  CardContent,
  IconButton,
  Chip,
  LinearProgress,
  Avatar,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Divider,
  Button,
  useTheme
} from '@mui/material';
import {
  TrendingUp,
  AccountBalanceWallet,
  Assessment,
  Notifications,
  Business,
  HowToVote,
  AttachMoney,
  People,
  MoreVert
} from '@mui/icons-material';
import { useAppSelector, useAppDispatch } from '../hooks/redux';
import { useAuth } from '../hooks/useAuth';
import { fetchDecVCPlatUserStatistics, fetchDecVCPlatRecentActivity } from '../store/slices/authSlice';
import { fetchDecVCPlatProjects } from '../store/slices/projectSlice';
import { fetchDecVCPlatProposals } from '../store/slices/votingSlice';
import { LoadingSpinner } from '../components/LoadingSpinner';

interface DecVCPlatStatCard {
  title: string;
  value: string;
  change: string;
  icon: React.ReactNode;
  color: 'primary' | 'secondary' | 'success' | 'warning' | 'error';
}

interface DecVCPlatActivityItem {
  id: string;
  type: 'project' | 'vote' | 'funding' | 'notification';
  title: string;
  description: string;
  timestamp: string;
  avatar?: string;
  status?: string;
}

export const DecVCPlatMainControlCenter: React.FC = () => {
  const theme = useTheme();
  const dispatch = useAppDispatch();
  const { user, isAuthenticated } = useAuth();
  
  const [isLoadingStats, setIsLoadingStats] = useState(true);
  const [decvcplatStats, setDecvcplatStats] = useState<DecVCPlatStatCard[]>([]);
  const [recentDecVCPlatActivity, setRecentDecVCPlatActivity] = useState<DecVCPlatActivityItem[]>([]);

  const projects = useAppSelector((state) => state.project.projects);
  const proposals = useAppSelector((state) => state.voting.proposals);
  const notifications = useAppSelector((state) => state.notification.notifications);

  useEffect(() => {
    if (isAuthenticated && user) {
      loadDecVCPlatControlCenterData();
    }
  }, [isAuthenticated, user]);

  const loadDecVCPlatControlCenterData = async () => {
    try {
      setIsLoadingStats(true);
      
      // Load user-specific data
      await Promise.all([
        dispatch(fetchDecVCPlatUserStatistics()),
        dispatch(fetchDecVCPlatRecentActivity()),
        dispatch(fetchDecVCPlatProjects({ page: 1, limit: 5 })),
        dispatch(fetchDecVCPlatProposals({ page: 1, limit: 5 }))
      ]);

      // Generate role-specific statistics
      const userRoleStats = generateDecVCPlatRoleStats();
      setDecvcplatStats(userRoleStats);
      
      // Generate recent activity
      const activityData = generateDecVCPlatActivity();
      setRecentDecVCPlatActivity(activityData);
      
    } catch (error) {
      console.error('DecVCPlat Control Center loading error:', error);
    } finally {
      setIsLoadingStats(false);
    }
  };

  const generateDecVCPlatRoleStats = (): DecVCPlatStatCard[] => {
    const baseStats: DecVCPlatStatCard[] = [
      {
        title: 'Active Projects',
        value: projects.length.toString(),
        change: '+12%',
        icon: <Business />,
        color: 'primary'
      },
      {
        title: 'Wallet Balance',
        value: '1,250 DVCP',
        change: '+5.2%',
        icon: <AccountBalanceWallet />,
        color: 'success'
      },
      {
        title: 'Notifications',
        value: notifications.filter(n => !n.isRead).length.toString(),
        change: 'New',
        icon: <Notifications />,
        color: 'warning'
      }
    ];

    // Add role-specific stats
    if (user?.role === 'Founder') {
      baseStats.push({
        title: 'Projects Created',
        value: '8',
        change: '+2 this month',
        icon: <TrendingUp />,
        color: 'secondary'
      });
    } else if (user?.role === 'Investor') {
      baseStats.push({
        title: 'Investments',
        value: '$125,000',
        change: '+8.5%',
        icon: <AttachMoney />,
        color: 'success'
      });
    } else if (user?.role === 'Luminary') {
      baseStats.push({
        title: 'Votes Cast',
        value: '42',
        change: '+15 this week',
        icon: <HowToVote />,
        color: 'primary'
      });
    }

    return baseStats;
  };

  const generateDecVCPlatActivity = (): DecVCPlatActivityItem[] => {
    const activities: DecVCPlatActivityItem[] = [
      {
        id: '1',
        type: 'project',
        title: 'New Project Submission',
        description: 'EcoTech Solutions submitted for review',
        timestamp: '2 hours ago',
        status: 'pending'
      },
      {
        id: '2',
        type: 'vote',
        title: 'Voting Completed',
        description: 'AI Healthcare proposal approved with 78% support',
        timestamp: '4 hours ago',
        status: 'approved'
      },
      {
        id: '3',
        type: 'funding',
        title: 'Tranche Released',
        description: '$50,000 released to GreenEnergy project',
        timestamp: '1 day ago',
        status: 'completed'
      },
      {
        id: '4',
        type: 'notification',
        title: 'Milestone Achieved',
        description: 'BlockChain Analytics reached Phase 2',
        timestamp: '2 days ago',
        status: 'info'
      }
    ];

    return activities;
  };

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'project': return <Business />;
      case 'vote': return <HowToVote />;
      case 'funding': return <AttachMoney />;
      case 'notification': return <Notifications />;
      default: return <Assessment />;
    }
  };

  const getStatusColor = (status?: string) => {
    switch (status) {
      case 'approved': return 'success';
      case 'pending': return 'warning';
      case 'rejected': return 'error';
      case 'completed': return 'success';
      default: return 'default';
    }
  };

  if (!isAuthenticated) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <Typography variant="h6">Please log in to access your DecVCPlat Control Center</Typography>
      </Box>
    );
  }

  if (isLoadingStats) {
    return <LoadingSpinner message="Loading DecVCPlat Control Center..." />;
  }

  return (
    <Box sx={{ flexGrow: 1, p: 3 }}>
      {/* Welcome Header */}
      <Box mb={4}>
        <Typography variant="h4" gutterBottom>
          Welcome to DecVCPlat, {user?.firstName || 'User'}
        </Typography>
        <Typography variant="subtitle1" color="text.secondary">
          Your {user?.role} Control Center - Manage your decentralized venture capital activities
        </Typography>
      </Box>

      {/* Statistics Cards Grid */}
      <Grid container spacing={3} mb={4}>
        {decvcplatStats.map((stat, index) => (
          <Grid item xs={12} sm={6} md={3} key={index}>
            <Card elevation={2}>
              <CardContent>
                <Box display="flex" alignItems="center" justifyContent="space-between">
                  <Box>
                    <Typography color="text.secondary" gutterBottom variant="body2">
                      {stat.title}
                    </Typography>
                    <Typography variant="h5" component="div">
                      {stat.value}
                    </Typography>
                    <Chip 
                      label={stat.change} 
                      color={stat.color} 
                      size="small" 
                      sx={{ mt: 1 }}
                    />
                  </Box>
                  <Box 
                    sx={{ 
                      color: theme.palette[stat.color].main,
                      fontSize: '2rem'
                    }}
                  >
                    {stat.icon}
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={3}>
        {/* Recent Projects */}
        <Grid item xs={12} md={6}>
          <Paper elevation={2} sx={{ p: 3, height: '400px' }}>
            <Box display="flex" alignItems="center" justifyContent="space-between" mb={2}>
              <Typography variant="h6">Recent Projects</Typography>
              <IconButton size="small">
                <MoreVert />
              </IconButton>
            </Box>
            <List>
              {projects.slice(0, 4).map((project, index) => (
                <React.Fragment key={project.id}>
                  <ListItem>
                    <ListItemAvatar>
                      <Avatar sx={{ bgcolor: theme.palette.primary.main }}>
                        <Business />
                      </Avatar>
                    </ListItemAvatar>
                    <ListItemText
                      primary={project.title}
                      secondary={
                        <Box>
                          <Typography variant="body2" color="text.secondary">
                            ${project.fundingGoal?.toLocaleString()} goal
                          </Typography>
                          <LinearProgress 
                            variant="determinate" 
                            value={(project.currentFunding || 0) / (project.fundingGoal || 1) * 100}
                            sx={{ mt: 1, width: '100%' }}
                          />
                        </Box>
                      }
                    />
                    <Chip 
                      label={project.status} 
                      color={getStatusColor(project.status)} 
                      size="small" 
                    />
                  </ListItem>
                  {index < 3 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          </Paper>
        </Grid>

        {/* Recent Activity */}
        <Grid item xs={12} md={6}>
          <Paper elevation={2} sx={{ p: 3, height: '400px' }}>
            <Box display="flex" alignItems="center" justifyContent="space-between" mb={2}>
              <Typography variant="h6">Recent Activity</Typography>
              <Button size="small" color="primary">
                View All
              </Button>
            </Box>
            <List>
              {recentDecVCPlatActivity.map((activity, index) => (
                <React.Fragment key={activity.id}>
                  <ListItem>
                    <ListItemAvatar>
                      <Avatar sx={{ bgcolor: theme.palette.secondary.main }}>
                        {getActivityIcon(activity.type)}
                      </Avatar>
                    </ListItemAvatar>
                    <ListItemText
                      primary={activity.title}
                      secondary={
                        <Box>
                          <Typography variant="body2" color="text.secondary">
                            {activity.description}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {activity.timestamp}
                          </Typography>
                        </Box>
                      }
                    />
                    {activity.status && (
                      <Chip 
                        label={activity.status} 
                        color={getStatusColor(activity.status)} 
                        size="small" 
                      />
                    )}
                  </ListItem>
                  {index < recentDecVCPlatActivity.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          </Paper>
        </Grid>
      </Grid>

      {/* Quick Actions */}
      <Grid container spacing={2} mt={2}>
        <Grid item xs={12} sm={6} md={3}>
          <Button
            variant="contained"
            color="primary"
            fullWidth
            startIcon={<Business />}
            onClick={() => window.location.href = '/projects/create'}
          >
            Create Project
          </Button>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Button
            variant="outlined"
            color="primary"
            fullWidth
            startIcon={<HowToVote />}
            onClick={() => window.location.href = '/voting'}
          >
            View Proposals
          </Button>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Button
            variant="outlined"
            color="secondary"
            fullWidth
            startIcon={<AccountBalanceWallet />}
            onClick={() => window.location.href = '/wallet'}
          >
            Manage Wallet
          </Button>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Button
            variant="outlined"
            color="info"
            fullWidth
            startIcon={<Assessment />}
            onClick={() => window.location.href = '/profile'}
          >
            View Profile
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
};

export default DecVCPlatMainControlCenter;
