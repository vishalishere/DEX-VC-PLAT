// © 2024 DecVCPlat. All rights reserved.

import React, { useEffect, useState } from 'react';
import { Container, Grid, Box, Typography, Button, TextField, FormControl, InputLabel, Select, MenuItem, Pagination, Dialog, DialogTitle, DialogContent, DialogActions, Slider, useTheme, Paper } from '@mui/material';
import { Add, FilterList, Search, HowToVote, AccountBalanceWallet } from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { fetchProposals, setFilters, setPagination, castVote } from '../../store/slices/votingSlice';
import ProposalCard from '../../components/Voting/ProposalCard';
import LoadingSpinner from '../../components/Common/LoadingSpinner';
import { toast } from 'react-hot-toast';
import { Helmet } from 'react-helmet-async';

const VotingPage: React.FC = () => {
  const decvcplatTheme = useTheme();
  const decvcplatAuth = useAuth();
  const decvcplatDispatch = useAppDispatch();
  
  const { 
    proposals: decvcplatProposalList, 
    isLoading: decvcplatVotingLoading,
    isVoting: decvcplatCastingVote,
    filters: decvcplatCurrentFilters, 
    pagination: decvcplatPaginationState 
  } = useAppSelector(state => state.voting);
  
  const [decvcplatSearchInput, setDecVCPlatSearchInput] = useState(decvcplatCurrentFilters.search);
  const [decvcplatShowFilters, setDecVCPlatShowFilters] = useState(false);
  const [decvcplatVoteDialogOpen, setDecVCPlatVoteDialogOpen] = useState(false);
  const [decvcplatSelectedProposal, setDecVCPlatSelectedProposal] = useState<string | null>(null);
  const [decvcplatSelectedVoteChoice, setDecVCPlatSelectedVoteChoice] = useState<'For' | 'Against' | 'Abstain'>('For');
  const [decvcplatStakeAmount, setDecVCPlatStakeAmount] = useState<number>(1000);
  const [decvcplatVoteComment, setDecVCPlatVoteComment] = useState('');

  useEffect(() => {
    decvcplatDispatch(fetchProposals({
      page: decvcplatPaginationState.page,
      pageSize: decvcplatPaginationState.pageSize,
      status: decvcplatCurrentFilters.status,
      type: decvcplatCurrentFilters.type,
    }));
  }, [decvcplatDispatch, decvcplatPaginationState.page, decvcplatCurrentFilters]);

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

  const handleDecVCPlatVoteClick = (decvcplatProposalId: string, decvcplatVoteChoice: 'For' | 'Against' | 'Abstain') => {
    if (!decvcplatAuth.canVoteOnProposals()) {
      toast.error('You do not have permission to vote on DecVCPlat proposals');
      return;
    }

    setDecVCPlatSelectedProposal(decvcplatProposalId);
    setDecVCPlatSelectedVoteChoice(decvcplatVoteChoice);
    setDecVCPlatVoteDialogOpen(true);
  };

  const handleDecVCPlatVoteSubmit = async () => {
    if (!decvcplatSelectedProposal) return;

    try {
      const decvcplatVoteData = {
        proposalId: decvcplatSelectedProposal,
        choice: decvcplatSelectedVoteChoice,
        stakedAmount: decvcplatStakeAmount,
        comment: decvcplatVoteComment || undefined,
      };

      const decvcplatVoteResult = await decvcplatDispatch(castVote(decvcplatVoteData));
      
      if (castVote.fulfilled.match(decvcplatVoteResult)) {
        toast.success(`Successfully voted ${decvcplatSelectedVoteChoice.toLowerCase()} on DecVCPlat proposal`);
        setDecVCPlatVoteDialogOpen(false);
        setDecVCPlatVoteComment('');
      } else {
        toast.error('Failed to cast DecVCPlat vote');
      }
    } catch (decvcplatError) {
      toast.error('Error casting DecVCPlat vote');
    }
  };

  const getDecVCPlatPageTitle = (): string => {
    switch (decvcplatAuth.user?.role) {
      case 'Investor':
        return 'DecVCPlat Voting Dashboard';
      case 'Luminary':
        return 'DecVCPlat Governance Center';
      default:
        return 'DecVCPlat Proposals';
    }
  };

  const getDecVCPlatPageDescription = (): string => {
    switch (decvcplatAuth.user?.role) {
      case 'Investor':
        return 'Stake tokens and vote on DecVCPlat proposals to shape the future of venture capital';
      case 'Luminary':
        return 'Create and manage DecVCPlat governance proposals for platform evolution';
      default:
        return 'Explore active DecVCPlat governance proposals and voting results';
    }
  };

  if (decvcplatVotingLoading) {
    return <LoadingSpinner message="Loading DecVCPlat proposals..." />;
  }

  return (
    <>
      <Helmet>
        <title>{getDecVCPlatPageTitle()} - DecVCPlat</title>
        <meta name="description" content={getDecVCPlatPageDescription()} />
      </Helmet>

      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}>
          <Box>
            <Typography variant="h4" fontWeight={600} gutterBottom>
              {getDecVCPlatPageTitle()}
            </Typography>
            <Typography variant="body1" color="text.secondary">
              {getDecVCPlatPageDescription()}
            </Typography>
          </Box>

          {decvcplatAuth.canCreateProposals() && (
            <Button
              component={RouterLink}
              to="/voting/create"
              variant="contained"
              startIcon={<Add />}
              sx={{
                background: decvcplatTheme.custom.gradients.secondary,
                '&:hover': { opacity: 0.9 },
              }}
            >
              Create DecVCPlat Proposal
            </Button>
          )}
        </Box>

        <Paper elevation={1} sx={{ p: 3, mb: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: decvcplatShowFilters ? 3 : 0 }}>
            <TextField
              placeholder="Search DecVCPlat proposals..."
              value={decvcplatSearchInput}
              onChange={(e) => setDecVCPlatSearchInput(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleDecVCPlatSearch()}
              InputProps={{
                startAdornment: <Search color="action" sx={{ mr: 1 }} />,
              }}
              sx={{ flex: 1 }}
            />
            <Button
              variant="contained"
              onClick={handleDecVCPlatSearch}
              sx={{ minWidth: 120 }}
            >
              Search
            </Button>
            <Button
              variant="outlined"
              startIcon={<FilterList />}
              onClick={() => setDecVCPlatShowFilters(!decvcplatShowFilters)}
            >
              Filters
            </Button>
          </Box>

          {decvcplatShowFilters && (
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Proposal Status</InputLabel>
                  <Select
                    value={decvcplatCurrentFilters.status}
                    label="Proposal Status"
                    onChange={(e) => handleDecVCPlatFilterChange('status', e.target.value)}
                  >
                    <MenuItem value="Active">Active Voting</MenuItem>
                    <MenuItem value="Passed">Passed</MenuItem>
                    <MenuItem value="Failed">Failed</MenuItem>
                    <MenuItem value="Executed">Executed</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Proposal Type</InputLabel>
                  <Select
                    value={decvcplatCurrentFilters.type}
                    label="Proposal Type"
                    onChange={(e) => handleDecVCPlatFilterChange('type', e.target.value)}
                  >
                    <MenuItem value="All">All Types</MenuItem>
                    <MenuItem value="ProjectApproval">Project Approval</MenuItem>
                    <MenuItem value="FundingRelease">Funding Release</MenuItem>
                    <MenuItem value="Governance">Governance</MenuItem>
                    <MenuItem value="MilestoneApproval">Milestone Approval</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          )}
        </Paper>

        {decvcplatProposalList.length === 0 ? (
          <Paper elevation={1} sx={{ p: 6, textAlign: 'center' }}>
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No DecVCPlat proposals found
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              {decvcplatAuth.canCreateProposals() 
                ? "Create the first DecVCPlat governance proposal to engage the community."
                : "Check back later for new DecVCPlat proposals to vote on."
              }
            </Typography>
            {decvcplatAuth.canCreateProposals() && (
              <Button
                component={RouterLink}
                to="/voting/create"
                variant="contained"
                startIcon={<Add />}
              >
                Create DecVCPlat Proposal
              </Button>
            )}
          </Paper>
        ) : (
          <>
            <Grid container spacing={3} sx={{ mb: 4 }}>
              {decvcplatProposalList.map((decvcplatProposal) => (
                <Grid item xs={12} lg={6} key={decvcplatProposal.id}>
                  <ProposalCard 
                    decvcplatProposal={decvcplatProposal}
                    decvcplatShowVoteActions={decvcplatAuth.canVoteOnProposals()}
                    decvcplatCompactView={false}
                    onDecVCPlatVote={handleDecVCPlatVoteClick}
                  />
                </Grid>
              ))}
            </Grid>

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
          </>
        )}

        {/* DecVCPlat Vote Dialog */}
        <Dialog 
          open={decvcplatVoteDialogOpen} 
          onClose={() => setDecVCPlatVoteDialogOpen(false)}
          maxWidth="sm"
          fullWidth
        >
          <DialogTitle>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <HowToVote sx={{ mr: 2 }} />
              Cast Your DecVCPlat Vote
            </Box>
          </DialogTitle>
          <DialogContent>
            <Typography variant="h6" gutterBottom>
              Vote Choice: <strong>{decvcplatSelectedVoteChoice}</strong>
            </Typography>
            
            <Box sx={{ mt: 3, mb: 3 }}>
              <Typography variant="body2" gutterBottom>
                Stake Amount: {decvcplatStakeAmount.toLocaleString()} DVCP
              </Typography>
              <Slider
                value={decvcplatStakeAmount}
                onChange={(_, newValue) => setDecVCPlatStakeAmount(newValue as number)}
                min={100}
                max={50000}
                step={100}
                marks={[
                  { value: 100, label: '100' },
                  { value: 10000, label: '10K' },
                  { value: 50000, label: '50K' },
                ]}
                valueLabelDisplay="auto"
              />
            </Box>

            <TextField
              fullWidth
              label="Comment (optional)"
              multiline
              rows={3}
              value={decvcplatVoteComment}
              onChange={(e) => setDecVCPlatVoteComment(e.target.value)}
              placeholder="Explain your DecVCPlat vote decision..."
              sx={{ mt: 2 }}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDecVCPlatVoteDialogOpen(false)}>
              Cancel
            </Button>
            <Button 
              onClick={handleDecVCPlatVoteSubmit}
              variant="contained"
              disabled={decvcplatCastingVote}
              startIcon={<AccountBalanceWallet />}
            >
              {decvcplatCastingVote ? 'Submitting Vote...' : 'Submit DecVCPlat Vote'}
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </>
  );
};

export default VotingPage;
