// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import {
  Card,
  CardContent,
  CardActions,
  Typography,
  Button,
  Chip,
  Avatar,
  Box,
  LinearProgress,
  useTheme,
  Divider,
} from '@mui/material';
import {
  HowToVote,
  Person,
  AccessTime,
  TrendingUp,
  CheckCircle,
  Cancel,
  RemoveCircle,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { VotingProposal } from '../../store/slices/votingSlice';

interface DecVCPlatProposalCardProps {
  decvcplatProposal: VotingProposal;
  decvcplatShowVoteActions?: boolean;
  decvcplatCompactView?: boolean;
  onDecVCPlatVote?: (proposalId: string, voteChoice: 'For' | 'Against' | 'Abstain') => void;
}

const ProposalCard: React.FC<DecVCPlatProposalCardProps> = ({
  decvcplatProposal,
  decvcplatShowVoteActions = true,
  decvcplatCompactView = false,
  onDecVCPlatVote,
}) => {
  const decvcplatTheme = useTheme();

  const calculateDecVCPlatVotingProgress = () => {
    const decvcplatTotalVotes = decvcplatProposal.forVotes + decvcplatProposal.againstVotes + decvcplatProposal.abstainVotes;
    if (decvcplatTotalVotes === 0) return { forPercent: 0, againstPercent: 0, abstainPercent: 0 };

    return {
      forPercent: (decvcplatProposal.forVotes / decvcplatTotalVotes) * 100,
      againstPercent: (decvcplatProposal.againstVotes / decvcplatTotalVotes) * 100,
      abstainPercent: (decvcplatProposal.abstainVotes / decvcplatTotalVotes) * 100,
    };
  };

  const getDecVCPlatProposalStatusColor = (): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (decvcplatProposal.status) {
      case 'Active': return 'primary';
      case 'Passed': return 'success';
      case 'Failed': return 'error';
      case 'Executed': return 'info';
      case 'Cancelled': return 'warning';
      default: return 'default';
    }
  };

  const getDecVCPlatProposalTypeColor = (): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (decvcplatProposal.proposalType) {
      case 'ProjectApproval': return 'primary';
      case 'FundingRelease': return 'success';
      case 'Governance': return 'secondary';
      case 'MilestoneApproval': return 'info';
      default: return 'default';
    }
  };

  const formatDecVCPlatTokenAmount = (decvcplatAmount: number): string => {
    if (decvcplatAmount >= 1000000) {
      return `${(decvcplatAmount / 1000000).toFixed(1)}M`;
    } else if (decvcplatAmount >= 1000) {
      return `${(decvcplatAmount / 1000).toFixed(1)}K`;
    }
    return decvcplatAmount.toString();
  };

  const calculateDecVCPlatTimeRemaining = (): string => {
    const decvcplatEndTime = new Date(decvcplatProposal.endTime);
    const decvcplatNow = new Date();
    const decvcplatTimeDiff = decvcplatEndTime.getTime() - decvcplatNow.getTime();

    if (decvcplatTimeDiff <= 0) return 'Voting Ended';

    const decvcplatDays = Math.floor(decvcplatTimeDiff / (1000 * 60 * 60 * 24));
    const decvcplatHours = Math.floor((decvcplatTimeDiff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

    if (decvcplatDays > 0) return `${decvcplatDays}d ${decvcplatHours}h remaining`;
    return `${decvcplatHours}h remaining`;
  };

  const decvcplatVotingProgress = calculateDecVCPlatVotingProgress();
  const decvcplatTimeRemaining = calculateDecVCPlatTimeRemaining();

  return (
    <Card
      elevation={1}
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        transition: 'all 0.3s ease',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: decvcplatTheme.custom.shadows.card,
        },
      }}
    >
      <CardContent sx={{ flexGrow: 1, pb: decvcplatCompactView ? 2 : 3 }}>
        {/* DecVCPlat Proposal Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
          <Typography
            variant={decvcplatCompactView ? 'h6' : 'h5'}
            component="h3"
            fontWeight={600}
            sx={{
              flex: 1,
              mr: 1,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
            }}
          >
            {decvcplatProposal.title}
          </Typography>
          <Chip
            label={decvcplatProposal.status}
            color={getDecVCPlatProposalStatusColor()}
            size="small"
            variant="outlined"
          />
        </Box>

        {/* DecVCPlat Proposal Description */}
        {!decvcplatCompactView && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mb: 2,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 3,
              WebkitBoxOrient: 'vertical',
            }}
          >
            {decvcplatProposal.description}
          </Typography>
        )}

        {/* DecVCPlat Proposer Information */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Avatar sx={{ width: 32, height: 32, mr: 1, bgcolor: 'secondary.main' }}>
              <Person fontSize="small" />
            </Avatar>
            <Box>
              <Typography variant="body2" fontWeight={500}>
                {decvcplatProposal.proposerName}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                DecVCPlat Proposer
              </Typography>
            </Box>
          </Box>
          <Chip
            label={decvcplatProposal.proposalType}
            color={getDecVCPlatProposalTypeColor()}
            size="small"
            variant="filled"
          />
        </Box>

        {/* DecVCPlat Voting Progress */}
        <Box sx={{ mb: 2 }}>
          <Typography variant="body2" fontWeight={600} gutterBottom>
            DecVCPlat Voting Results
          </Typography>
          
          {/* For Votes */}
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <CheckCircle fontSize="small" sx={{ color: 'success.main', mr: 1 }} />
            <Typography variant="body2" sx={{ minWidth: 60 }}>
              For: {decvcplatVotingProgress.forPercent.toFixed(1)}%
            </Typography>
            <LinearProgress
              variant="determinate"
              value={decvcplatVotingProgress.forPercent}
              sx={{
                flexGrow: 1,
                ml: 2,
                height: 4,
                borderRadius: 2,
                backgroundColor: 'grey.200',
                '& .MuiLinearProgress-bar': {
                  borderRadius: 2,
                  backgroundColor: decvcplatTheme.palette.success.main,
                },
              }}
            />
          </Box>

          {/* Against Votes */}
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <Cancel fontSize="small" sx={{ color: 'error.main', mr: 1 }} />
            <Typography variant="body2" sx={{ minWidth: 60 }}>
              Against: {decvcplatVotingProgress.againstPercent.toFixed(1)}%
            </Typography>
            <LinearProgress
              variant="determinate"
              value={decvcplatVotingProgress.againstPercent}
              sx={{
                flexGrow: 1,
                ml: 2,
                height: 4,
                borderRadius: 2,
                backgroundColor: 'grey.200',
                '& .MuiLinearProgress-bar': {
                  borderRadius: 2,
                  backgroundColor: decvcplatTheme.palette.error.main,
                },
              }}
            />
          </Box>

          {/* Abstain Votes */}
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <RemoveCircle fontSize="small" sx={{ color: 'grey.500', mr: 1 }} />
            <Typography variant="body2" sx={{ minWidth: 60 }}>
              Abstain: {decvcplatVotingProgress.abstainPercent.toFixed(1)}%
            </Typography>
            <LinearProgress
              variant="determinate"
              value={decvcplatVotingProgress.abstainPercent}
              sx={{
                flexGrow: 1,
                ml: 2,
                height: 4,
                borderRadius: 2,
                backgroundColor: 'grey.200',
                '& .MuiLinearProgress-bar': {
                  borderRadius: 2,
                  backgroundColor: decvcplatTheme.palette.grey[500],
                },
              }}
            />
          </Box>

          {/* DecVCPlat Voting Stats */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="caption" color="text.secondary">
              Total Staked: {formatDecVCPlatTokenAmount(decvcplatProposal.totalStaked)} DVCP
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <AccessTime fontSize="small" sx={{ color: 'warning.main', mr: 0.5 }} />
              <Typography variant="caption" color="text.secondary">
                {decvcplatTimeRemaining}
              </Typography>
            </Box>
          </Box>
        </Box>

        {/* DecVCPlat Proposal Thresholds */}
        {!decvcplatCompactView && (
          <>
            <Divider sx={{ my: 2 }} />
            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="caption" color="text.secondary" display="block">
                  Quorum Required
                </Typography>
                <Typography variant="body2" fontWeight={600}>
                  {formatDecVCPlatTokenAmount(decvcplatProposal.quorumThreshold)} DVCP
                </Typography>
              </Box>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="caption" color="text.secondary" display="block">
                  Approval Threshold
                </Typography>
                <Typography variant="body2" fontWeight={600}>
                  {decvcplatProposal.approvalThreshold}%
                </Typography>
              </Box>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="caption" color="text.secondary" display="block">
                  Current Participation
                </Typography>
                <Typography variant="body2" fontWeight={600} color={
                  decvcplatProposal.totalStaked >= decvcplatProposal.quorumThreshold ? 'success.main' : 'warning.main'
                }>
                  {((decvcplatProposal.totalStaked / decvcplatProposal.quorumThreshold) * 100).toFixed(1)}%
                </Typography>
              </Box>
            </Box>
          </>
        )}
      </CardContent>

      {/* DecVCPlat Vote Actions */}
      {decvcplatShowVoteActions && decvcplatProposal.status === 'Active' && (
        <CardActions sx={{ p: 2, pt: 0 }}>
          <Box sx={{ display: 'flex', gap: 1, width: '100%' }}>
            <Button
              variant="contained"
              color="success"
              size="small"
              startIcon={<CheckCircle />}
              onClick={() => onDecVCPlatVote?.(decvcplatProposal.id, 'For')}
              sx={{ flex: 1, textTransform: 'none' }}
            >
              Vote For
            </Button>
            <Button
              variant="contained"
              color="error"
              size="small"
              startIcon={<Cancel />}
              onClick={() => onDecVCPlatVote?.(decvcplatProposal.id, 'Against')}
              sx={{ flex: 1, textTransform: 'none' }}
            >
              Vote Against
            </Button>
            <Button
              variant="outlined"
              color="inherit"
              size="small"
              startIcon={<RemoveCircle />}
              onClick={() => onDecVCPlatVote?.(decvcplatProposal.id, 'Abstain')}
              sx={{ flex: 1, textTransform: 'none' }}
            >
              Abstain
            </Button>
          </Box>
        </CardActions>
      )}

      {/* DecVCPlat View Details Action */}
      {!decvcplatShowVoteActions && (
        <CardActions sx={{ p: 2, pt: 0 }}>
          <Button
            component={RouterLink}
            to={`/voting/${decvcplatProposal.id}`}
            variant="outlined"
            size="small"
            fullWidth
            startIcon={<HowToVote />}
          >
            View DecVCPlat Proposal Details
          </Button>
        </CardActions>
      )}
    </Card>
  );
};

export default ProposalCard;
