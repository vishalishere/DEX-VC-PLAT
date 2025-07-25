// © 2024 DecVCPlat. All rights reserved.

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Button,
  Chip,
  LinearProgress,
  Divider,
  Avatar,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Tab,
  Tabs,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Timeline as TimelineIcon,
  Description as DocumentIcon,
  HowToVote as VoteIcon,
  AttachMoney as FundingIcon,
  Download as DownloadIcon,
  Visibility as ViewIcon,
  ThumbUp as ApproveIcon,
  ThumbDown as RejectIcon,
  CheckCircle as CompleteIcon,
  Schedule as PendingIcon,
} from '@mui/icons-material';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { useAuth } from '../../hooks/useAuth';
import { fetchProjectById, voteOnProject } from '../../store/slices/projectSlice';
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
      id={`decvcplat-project-tabpanel-${index}`}
      aria-labelledby={`decvcplat-project-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

const DecVCPlatProjectDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAuth();
  const { currentProject, isLoading } = useAppSelector(state => state.projects);

  const [decvcplatActiveTab, setDecVCPlatActiveTab] = useState(0);
  const [decvcplatShowVoteDialog, setDecVCPlatShowVoteDialog] = useState(false);
  const [decvcplatVoteComment, setDecVCPlatVoteComment] = useState('');
  const [decvcplatVoteType, setDecVCPlatVoteType] = useState<'approve' | 'reject'>('approve');

  useEffect(() => {
    if (id) {
      dispatch(fetchProjectById(id));
    }
  }, [dispatch, id]);

  const handleDecVCPlatTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setDecVCPlatActiveTab(newValue);
  };

  const handleDecVCPlatVoteOpen = (voteType: 'approve' | 'reject') => {
    setDecVCPlatVoteType(voteType);
    setDecVCPlatShowVoteDialog(true);
  };

  const handleDecVCPlatVoteSubmit = async () => {
    if (!currentProject || !user) return;

    try {
      await dispatch(voteOnProject({
        projectId: currentProject.id,
        voteType: decvcplatVoteType,
        comment: decvcplatVoteComment,
        voterRole: user.role,
      })).unwrap();

      toast.success(`Vote ${decvcplatVoteType === 'approve' ? 'approved' : 'rejected'} successfully`);
      setDecVCPlatShowVoteDialog(false);
      setDecVCPlatVoteComment('');
    } catch (error: any) {
      toast.error(error.message || 'Failed to submit vote');
    }
  };

  const getFundingProgressPercentage = () => {
    if (!currentProject) return 0;
    return Math.min((currentProject.fundingRaised / currentProject.fundingGoal) * 100, 100);
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
      case 'approved':
      case 'completed':
        return 'success';
      case 'pending':
      case 'review':
        return 'warning';
      case 'rejected':
      case 'cancelled':
        return 'error';
      default:
        return 'default';
    }
  };

  const canUserVote = () => {
    return user && ['Investor', 'Luminary'].includes(user.role) && currentProject?.status === 'Review';
  };

  if (isLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
          <Box textAlign="center">
            <LinearProgress sx={{ width: 200, mb: 2 }} />
            <Typography>Loading project details...</Typography>
          </Box>
        </Box>
      </Container>
    );
  }

  if (!currentProject) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">
          Project not found. Please check the project ID and try again.
        </Alert>
      </Container>
    );
  }

  return (
    <>
      <Helmet>
        <title>{currentProject.title} - DecVCPlat</title>
        <meta name="description" content={currentProject.description} />
      </Helmet>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box display="flex" alignItems="center" mb={3}>
          <IconButton onClick={() => navigate('/projects')} sx={{ mr: 2 }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h4" component="h1" sx={{ flexGrow: 1 }}>
            {currentProject.title}
          </Typography>
          <Chip
            label={currentProject.status}
            color={getStatusColor(currentProject.status) as any}
            sx={{ ml: 2 }}
          />
        </Box>

        {/* Project Overview */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Grid container spacing={3}>
            <Grid item xs={12} md={8}>
              <Typography variant="h6" gutterBottom>
                Project Description
              </Typography>
              <Typography variant="body1" paragraph>
                {currentProject.description}
              </Typography>

              {/* Founder Info */}
              <Box display="flex" alignItems="center" mt={2}>
                <Avatar sx={{ mr: 2 }}>
                  {currentProject.founderName?.charAt(0).toUpperCase()}
                </Avatar>
                <Box>
                  <Typography variant="subtitle1">{currentProject.founderName}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    Project Founder
                  </Typography>
                </Box>
              </Box>

              {/* Tags */}
              <Box mt={3}>
                <Typography variant="subtitle2" gutterBottom>
                  Categories
                </Typography>
                {currentProject.tags?.map((tag, index) => (
                  <Chip
                    key={index}
                    label={tag}
                    size="small"
                    sx={{ mr: 1, mb: 1 }}
                  />
                ))}
              </Box>
            </Grid>

            <Grid item xs={12} md={4}>
              {/* Funding Progress */}
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Funding Progress
                  </Typography>
                  <Box mb={2}>
                    <Typography variant="h4" color="primary">
                      ${currentProject.fundingRaised?.toLocaleString()} ETH
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      of ${currentProject.fundingGoal?.toLocaleString()} ETH goal
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={getFundingProgressPercentage()}
                    sx={{ height: 8, borderRadius: 4, mb: 1 }}
                  />
                  <Typography variant="body2" color="text.secondary">
                    {getFundingProgressPercentage().toFixed(1)}% funded
                  </Typography>

                  {/* Voting Actions */}
                  {canUserVote() && (
                    <Box mt={3}>
                      <Typography variant="subtitle2" gutterBottom>
                        Cast Your Vote
                      </Typography>
                      <Button
                        variant="contained"
                        color="success"
                        startIcon={<ApproveIcon />}
                        fullWidth
                        sx={{ mb: 1 }}
                        onClick={() => handleDecVCPlatVoteOpen('approve')}
                      >
                        Approve Project
                      </Button>
                      <Button
                        variant="outlined"
                        color="error"
                        startIcon={<RejectIcon />}
                        fullWidth
                        onClick={() => handleDecVCPlatVoteOpen('reject')}
                      >
                        Reject Project
                      </Button>
                    </Box>
                  )}
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </Paper>

        {/* Detailed Tabs */}
        <Paper>
          <Tabs
            value={decvcplatActiveTab}
            onChange={handleDecVCPlatTabChange}
            variant="scrollable"
            scrollButtons="auto"
            sx={{ borderBottom: 1, borderColor: 'divider' }}
          >
            <Tab icon={<TimelineIcon />} label="Milestones" />
            <Tab icon={<DocumentIcon />} label="Documents" />
            <Tab icon={<VoteIcon />} label="Voting History" />
            <Tab icon={<FundingIcon />} label="Funding Details" />
          </Tabs>

          {/* Milestones Tab */}
          <TabPanel value={decvcplatActiveTab} index={0}>
            <Box sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Project Milestones
              </Typography>
              <List>
                {currentProject.milestones?.map((milestone, index) => (
                  <ListItem key={index} divider>
                    <ListItemIcon>
                      {milestone.status === 'completed' ? (
                        <CompleteIcon color="success" />
                      ) : (
                        <PendingIcon color="warning" />
                      )}
                    </ListItemIcon>
                    <ListItemText
                      primary={milestone.title}
                      secondary={
                        <>
                          <Typography variant="body2" component="span">
                            {milestone.description}
                          </Typography>
                          <br />
                          <Typography variant="caption" color="text.secondary">
                            Due: {new Date(milestone.dueDate).toLocaleDateString()} • 
                            Funding: ${milestone.fundingAmount?.toLocaleString()} ETH
                          </Typography>
                        </>
                      }
                    />
                    <Chip
                      label={milestone.status}
                      color={getStatusColor(milestone.status) as any}
                      size="small"
                    />
                  </ListItem>
                ))}
              </List>
            </Box>
          </TabPanel>

          {/* Documents Tab */}
          <TabPanel value={decvcplatActiveTab} index={1}>
            <Box sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Project Documents
              </Typography>
              <List>
                {currentProject.documents?.map((document, index) => (
                  <ListItem key={index} divider>
                    <ListItemIcon>
                      <DocumentIcon />
                    </ListItemIcon>
                    <ListItemText
                      primary={document.name}
                      secondary={
                        <>
                          <Typography variant="body2" component="span">
                            {document.description}
                          </Typography>
                          <br />
                          <Typography variant="caption" color="text.secondary">
                            Uploaded: {new Date(document.uploadedAt).toLocaleDateString()} • 
                            Size: {document.fileSize}
                          </Typography>
                        </>
                      }
                    />
                    <Box>
                      <IconButton size="small" sx={{ mr: 1 }}>
                        <ViewIcon />
                      </IconButton>
                      <IconButton size="small">
                        <DownloadIcon />
                      </IconButton>
                    </Box>
                  </ListItem>
                ))}
              </List>
            </Box>
          </TabPanel>

          {/* Voting History Tab */}
          <TabPanel value={decvcplatActiveTab} index={2}>
            <Box sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Voting History
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6} md={3}>
                  <Card>
                    <CardContent sx={{ textAlign: 'center' }}>
                      <Typography variant="h4" color="success.main">
                        {currentProject.votes?.approve || 0}
                      </Typography>
                      <Typography variant="body2">Approve Votes</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <Card>
                    <CardContent sx={{ textAlign: 'center' }}>
                      <Typography variant="h4" color="error.main">
                        {currentProject.votes?.reject || 0}
                      </Typography>
                      <Typography variant="body2">Reject Votes</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <Card>
                    <CardContent sx={{ textAlign: 'center' }}>
                      <Typography variant="h4" color="primary">
                        {currentProject.totalVotes || 0}
                      </Typography>
                      <Typography variant="body2">Total Votes</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  <Card>
                    <CardContent sx={{ textAlign: 'center' }}>
                      <Typography variant="h4" color="text.primary">
                        {currentProject.approvalPercentage || 0}%
                      </Typography>
                      <Typography variant="body2">Approval Rate</Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </Box>
          </TabPanel>

          {/* Funding Details Tab */}
          <TabPanel value={decvcplatActiveTab} index={3}>
            <Box sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Funding Breakdown
              </Typography>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Funding Summary
                      </Typography>
                      <Box mb={2}>
                        <Typography variant="body2" color="text.secondary">
                          Total Goal
                        </Typography>
                        <Typography variant="h5">
                          ${currentProject.fundingGoal?.toLocaleString()} ETH
                        </Typography>
                      </Box>
                      <Box mb={2}>
                        <Typography variant="body2" color="text.secondary">
                          Amount Raised
                        </Typography>
                        <Typography variant="h5" color="primary">
                          ${currentProject.fundingRaised?.toLocaleString()} ETH
                        </Typography>
                      </Box>
                      <Box>
                        <Typography variant="body2" color="text.secondary">
                          Funding Progress
                        </Typography>
                        <LinearProgress
                          variant="determinate"
                          value={getFundingProgressPercentage()}
                          sx={{ height: 8, borderRadius: 4, mt: 1 }}
                        />
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Tranche Information
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Funding will be released in tranches based on milestone completion.
                      </Typography>
                      <List dense>
                        {currentProject.milestones?.map((milestone, index) => (
                          <ListItem key={index}>
                            <ListItemText
                              primary={`Tranche ${index + 1}`}
                              secondary={`$${milestone.fundingAmount?.toLocaleString()} ETH - ${milestone.title}`}
                            />
                          </ListItem>
                        ))}
                      </List>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </Box>
          </TabPanel>
        </Paper>

        {/* Vote Dialog */}
        <Dialog
          open={decvcplatShowVoteDialog}
          onClose={() => setDecVCPlatShowVoteDialog(false)}
          maxWidth="sm"
          fullWidth
        >
          <DialogTitle>
            {decvcplatVoteType === 'approve' ? 'Approve Project' : 'Reject Project'}
          </DialogTitle>
          <DialogContent>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Please provide a comment explaining your vote decision.
            </Typography>
            <TextField
              autoFocus
              fullWidth
              multiline
              rows={4}
              label="Vote Comment"
              value={decvcplatVoteComment}
              onChange={(e) => setDecVCPlatVoteComment(e.target.value)}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDecVCPlatShowVoteDialog(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleDecVCPlatVoteSubmit}
              variant="contained"
              color={decvcplatVoteType === 'approve' ? 'success' : 'error'}
            >
              Submit {decvcplatVoteType === 'approve' ? 'Approval' : 'Rejection'}
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </>
  );
};

export default DecVCPlatProjectDetailsPage;
