// © 2024 DecVCPlat. All rights reserved.

import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Paper,
  Grid,
  TextField,
  Button,
  Switch,
  FormControlLabel,
  Divider,
  Alert,
  Avatar,
  IconButton,
  Tab,
  Tabs,
  Card,
  CardContent,
  Chip,
} from '@mui/material';
import {
  Edit as EditIcon,
  Save as SaveIcon,
  Cancel as CancelIcon,
  PhotoCamera as PhotoCameraIcon,
  Security as SecurityIcon,
  Notifications as NotificationsIcon,
  AccountCircle as ProfileIcon,
} from '@mui/icons-material';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { useAuth } from '../../hooks/useAuth';
import { updateNotificationPreferences, fetchNotificationPreferences } from '../../store/slices/notificationSlice';
import { updateProfile } from '../../store/slices/authSlice';
import toast from 'react-hot-toast';
import { Helmet } from 'react-helmet-async';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`decvcplat-profile-tabpanel-${index}`}
      aria-labelledby={`decvcplat-profile-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

const DecVCPlatProfilePage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { user, isLoading: authLoading } = useAuth();
  const { preferences, isUpdatingPreferences } = useAppSelector(state => state.notifications);

  const [decvcplatActiveTab, setDecVCPlatActiveTab] = useState(0);
  const [decvcplatEditingProfile, setDecVCPlatEditingProfile] = useState(false);
  const [decvcplatProfileFormData, setDecVCPlatProfileFormData] = useState({
    displayName: user?.displayName || '',
    email: user?.email || '',
    bio: user?.bio || '',
    company: user?.company || '',
    website: user?.website || '',
    location: user?.location || '',
  });

  const [decvcplatNotificationPrefs, setDecVCPlatNotificationPrefs] = useState({
    emailNotifications: true,
    pushNotifications: true,
    projectUpdates: true,
    votingResults: true,
    fundingReleases: true,
    milestoneUpdates: true,
    marketingEmails: false,
    weeklyDigest: true,
  });

  useEffect(() => {
    dispatch(fetchNotificationPreferences());
  }, [dispatch]);

  useEffect(() => {
    if (preferences) {
      setDecVCPlatNotificationPrefs({
        emailNotifications: preferences.emailNotifications,
        pushNotifications: preferences.pushNotifications,
        projectUpdates: preferences.projectUpdates,
        votingResults: preferences.votingResults,
        fundingReleases: preferences.fundingReleases,
        milestoneUpdates: preferences.milestoneUpdates,
        marketingEmails: preferences.marketingEmails,
        weeklyDigest: preferences.weeklyDigest,
      });
    }
  }, [preferences]);

  const handleDecVCPlatTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setDecVCPlatActiveTab(newValue);
  };

  const handleDecVCPlatProfileEdit = () => {
    setDecVCPlatEditingProfile(true);
  };

  const handleDecVCPlatProfileSave = async () => {
    try {
      await dispatch(updateProfile(decvcplatProfileFormData)).unwrap();
      setDecVCPlatEditingProfile(false);
      toast.success('Profile updated successfully');
    } catch (error: any) {
      toast.error(error.message || 'Failed to update profile');
    }
  };

  const handleDecVCPlatProfileCancel = () => {
    setDecVCPlatProfileFormData({
      displayName: user?.displayName || '',
      email: user?.email || '',
      bio: user?.bio || '',
      company: user?.company || '',
      website: user?.website || '',
      location: user?.location || '',
    });
    setDecVCPlatEditingProfile(false);
  };

  const handleDecVCPlatInputChange = (field: string, value: string) => {
    setDecVCPlatProfileFormData(prev => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleDecVCPlatNotificationChange = (setting: string, checked: boolean) => {
    setDecVCPlatNotificationPrefs(prev => ({
      ...prev,
      [setting]: checked,
    }));
  };

  const handleDecVCPlatNotificationSave = async () => {
    try {
      await dispatch(updateNotificationPreferences(decvcplatNotificationPrefs)).unwrap();
      toast.success('Notification preferences updated successfully');
    } catch (error: any) {
      toast.error(error.message || 'Failed to update notification preferences');
    }
  };

  return (
    <>
      <Helmet>
        <title>Profile Settings - DecVCPlat</title>
        <meta name="description" content="Manage your DecVCPlat profile settings and preferences" />
      </Helmet>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Typography variant="h4" gutterBottom>
          Profile Settings
        </Typography>

        <Paper sx={{ mt: 3 }}>
          <Tabs
            value={decvcplatActiveTab}
            onChange={handleDecVCPlatTabChange}
            variant="scrollable"
            scrollButtons="auto"
            sx={{ borderBottom: 1, borderColor: 'divider' }}
          >
            <Tab icon={<ProfileIcon />} label="Profile Information" />
            <Tab icon={<NotificationsIcon />} label="Notifications" />
            <Tab icon={<SecurityIcon />} label="Security" />
          </Tabs>

          {/* Profile Information Tab */}
          <TabPanel value={decvcplatActiveTab} index={0}>
            <Box sx={{ p: 3 }}>
              <Grid container spacing={3}>
                {/* Avatar Section */}
                <Grid item xs={12} md={4}>
                  <Box display="flex" flexDirection="column" alignItems="center">
                    <Avatar
                      sx={{ width: 120, height: 120, mb: 2 }}
                      src={user?.avatar || undefined}
                    >
                      {user?.displayName?.charAt(0).toUpperCase()}
                    </Avatar>
                    <IconButton
                      component="label"
                      sx={{ mb: 2 }}
                    >
                      <PhotoCameraIcon />
                      <input
                        type="file"
                        accept="image/*"
                        hidden
                        onChange={(e) => {
                          // Handle avatar upload
                          toast.success('Avatar upload functionality will be implemented');
                        }}
                      />
                    </IconButton>
                    <Typography variant="body2" color="text.secondary" textAlign="center">
                      Click to change profile picture
                    </Typography>
                  </Box>
                </Grid>

                {/* Profile Form */}
                <Grid item xs={12} md={8}>
                  <Box>
                    <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
                      <Typography variant="h6">Personal Information</Typography>
                      {!decvcplatEditingProfile ? (
                        <Button
                          startIcon={<EditIcon />}
                          onClick={handleDecVCPlatProfileEdit}
                          variant="outlined"
                        >
                          Edit Profile
                        </Button>
                      ) : (
                        <Box>
                          <Button
                            startIcon={<SaveIcon />}
                            onClick={handleDecVCPlatProfileSave}
                            variant="contained"
                            sx={{ mr: 1 }}
                            disabled={authLoading}
                          >
                            Save
                          </Button>
                          <Button
                            startIcon={<CancelIcon />}
                            onClick={handleDecVCPlatProfileCancel}
                            variant="outlined"
                          >
                            Cancel
                          </Button>
                        </Box>
                      )}
                    </Box>

                    <Grid container spacing={2}>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Display Name"
                          value={decvcplatProfileFormData.displayName}
                          onChange={(e) => handleDecVCPlatInputChange('displayName', e.target.value)}
                          disabled={!decvcplatEditingProfile}
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Email Address"
                          type="email"
                          value={decvcplatProfileFormData.email}
                          onChange={(e) => handleDecVCPlatInputChange('email', e.target.value)}
                          disabled={!decvcplatEditingProfile}
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Bio"
                          multiline
                          rows={3}
                          value={decvcplatProfileFormData.bio}
                          onChange={(e) => handleDecVCPlatInputChange('bio', e.target.value)}
                          disabled={!decvcplatEditingProfile}
                        />
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <TextField
                          fullWidth
                          label="Company"
                          value={decvcplatProfileFormData.company}
                          onChange={(e) => handleDecVCPlatInputChange('company', e.target.value)}
                          disabled={!decvcplatEditingProfile}
                        />
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <TextField
                          fullWidth
                          label="Website"
                          value={decvcplatProfileFormData.website}
                          onChange={(e) => handleDecVCPlatInputChange('website', e.target.value)}
                          disabled={!decvcplatEditingProfile}
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Location"
                          value={decvcplatProfileFormData.location}
                          onChange={(e) => handleDecVCPlatInputChange('location', e.target.value)}
                          disabled={!decvcplatEditingProfile}
                        />
                      </Grid>
                    </Grid>

                    <Box mt={3}>
                      <Typography variant="h6" gutterBottom>
                        Role Information
                      </Typography>
                      <Chip
                        label={user?.role || 'Unknown'}
                        color="primary"
                        sx={{ mr: 1, mb: 1 }}
                      />
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                        Role: {user?.role} • Member since: {user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'N/A'}
                      </Typography>
                    </Box>
                  </Box>
                </Grid>
              </Grid>
            </Box>
          </TabPanel>

          {/* Notifications Tab */}
          <TabPanel value={decvcplatActiveTab} index={1}>
            <Box sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Notification Preferences
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Configure how you'd like to receive notifications from DecVCPlat
              </Typography>

              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        General Notifications
                      </Typography>
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.emailNotifications}
                            onChange={(e) => handleDecVCPlatNotificationChange('emailNotifications', e.target.checked)}
                          />
                        }
                        label="Email Notifications"
                      />
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.pushNotifications}
                            onChange={(e) => handleDecVCPlatNotificationChange('pushNotifications', e.target.checked)}
                          />
                        }
                        label="Push Notifications"
                      />
                    </CardContent>
                  </Card>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Platform Activities
                      </Typography>
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.projectUpdates}
                            onChange={(e) => handleDecVCPlatNotificationChange('projectUpdates', e.target.checked)}
                          />
                        }
                        label="Project Updates"
                      />
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.votingResults}
                            onChange={(e) => handleDecVCPlatNotificationChange('votingResults', e.target.checked)}
                          />
                        }
                        label="Voting Results"
                      />
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.fundingReleases}
                            onChange={(e) => handleDecVCPlatNotificationChange('fundingReleases', e.target.checked)}
                          />
                        }
                        label="Funding Releases"
                      />
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.milestoneUpdates}
                            onChange={(e) => handleDecVCPlatNotificationChange('milestoneUpdates', e.target.checked)}
                          />
                        }
                        label="Milestone Updates"
                      />
                    </CardContent>
                  </Card>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Marketing & Communication
                      </Typography>
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.marketingEmails}
                            onChange={(e) => handleDecVCPlatNotificationChange('marketingEmails', e.target.checked)}
                          />
                        }
                        label="Marketing Emails"
                      />
                      <FormControlLabel
                        control={
                          <Switch
                            checked={decvcplatNotificationPrefs.weeklyDigest}
                            onChange={(e) => handleDecVCPlatNotificationChange('weeklyDigest', e.target.checked)}
                          />
                        }
                        label="Weekly Digest"
                      />
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              <Box mt={3}>
                <Button
                  variant="contained"
                  onClick={handleDecVCPlatNotificationSave}
                  disabled={isUpdatingPreferences}
                >
                  Save Notification Preferences
                </Button>
              </Box>
            </Box>
          </TabPanel>

          {/* Security Tab */}
          <TabPanel value={decvcplatActiveTab} index={2}>
            <Box sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Security Settings
              </Typography>
              
              <Alert severity="info" sx={{ mb: 3 }}>
                Security features including two-factor authentication and password changes are coming soon.
              </Alert>

              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Account Security
                      </Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        Manage your account security settings and authentication methods.
                      </Typography>
                      <Button variant="outlined" disabled>
                        Change Password (Coming Soon)
                      </Button>
                      <Button variant="outlined" sx={{ ml: 2 }} disabled>
                        Enable 2FA (Coming Soon)
                      </Button>
                    </CardContent>
                  </Card>
                </Grid>

                <Grid item xs={12}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Wallet Security
                      </Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        Your wallet connection is secured through MetaMask and blockchain encryption.
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Connected Wallet: {user?.walletAddress ? `${user.walletAddress.substring(0, 6)}...${user.walletAddress.substring(38)}` : 'Not Connected'}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </Box>
          </TabPanel>
        </Paper>
      </Container>
    </>
  );
};

export default DecVCPlatProfilePage;
