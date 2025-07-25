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
} from '@mui/material';
import {
  Person,
  TrendingUp,
  Schedule,
  CheckCircle,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { Project } from '../../store/slices/projectSlice';

interface DecVCPlatProjectCardProps {
  decvcplatProject: Project;
  decvcplatShowActions?: boolean;
  decvcplatCompactMode?: boolean;
}

const ProjectCard: React.FC<DecVCPlatProjectCardProps> = ({
  decvcplatProject,
  decvcplatShowActions = true,
  decvcplatCompactMode = false,
}) => {
  const decvcplatTheme = useTheme();

  const calculateDecVCPlatFundingProgress = (): number => {
    if (decvcplatProject.fundingGoal === 0) return 0;
    return Math.min((decvcplatProject.currentFunding / decvcplatProject.fundingGoal) * 100, 100);
  };

  const getDecVCPlatStatusColor = (): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (decvcplatProject.status) {
      case 'Draft': return 'default';
      case 'Submitted': return 'info';
      case 'UnderReview': return 'warning';
      case 'Approved': return 'success';
      case 'Rejected': return 'error';
      case 'Funded': return 'primary';
      case 'Completed': return 'success';
      default: return 'default';
    }
  };

  const formatDecVCPlatCurrency = (decvcplatAmount: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(decvcplatAmount);
  };

  const decvcplatFundingProgress = calculateDecVCPlatFundingProgress();

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
      <CardContent sx={{ flexGrow: 1, pb: decvcplatCompactMode ? 2 : 3 }}>
        {/* DecVCPlat Project Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
          <Typography
            variant={decvcplatCompactMode ? 'h6' : 'h5'}
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
            {decvcplatProject.title}
          </Typography>
          <Chip
            label={decvcplatProject.status}
            color={getDecVCPlatStatusColor()}
            size="small"
            variant="outlined"
          />
        </Box>

        {/* DecVCPlat Project Description */}
        {!decvcplatCompactMode && (
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
            {decvcplatProject.description}
          </Typography>
        )}

        {/* DecVCPlat Founder Information */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <Avatar sx={{ width: 32, height: 32, mr: 1, bgcolor: 'primary.main' }}>
            <Person fontSize="small" />
          </Avatar>
          <Box>
            <Typography variant="body2" fontWeight={500}>
              {decvcplatProject.founderName}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              DecVCPlat Founder
            </Typography>
          </Box>
        </Box>

        {/* DecVCPlat Project Category and Tags */}
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
          <Chip
            label={decvcplatProject.category}
            color="primary"
            size="small"
            variant="outlined"
          />
          {decvcplatProject.tags.slice(0, decvcplatCompactMode ? 2 : 3).map((decvcplatTag, decvcplatTagIndex) => (
            <Chip
              key={decvcplatTagIndex}
              label={decvcplatTag}
              size="small"
              variant="outlined"
              sx={{ fontSize: '0.7rem' }}
            />
          ))}
          {decvcplatProject.tags.length > (decvcplatCompactMode ? 2 : 3) && (
            <Chip
              label={`+${decvcplatProject.tags.length - (decvcplatCompactMode ? 2 : 3)}`}
              size="small"
              variant="outlined"
              sx={{ fontSize: '0.7rem' }}
            />
          )}
        </Box>

        {/* DecVCPlat Funding Progress */}
        <Box sx={{ mb: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="body2" color="text.secondary">
              DecVCPlat Funding Progress
            </Typography>
            <Typography variant="body2" fontWeight={600}>
              {decvcplatFundingProgress.toFixed(1)}%
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={decvcplatFundingProgress}
            sx={{
              height: 6,
              borderRadius: 3,
              backgroundColor: 'grey.200',
              '& .MuiLinearProgress-bar': {
                borderRadius: 3,
                background: decvcplatTheme.custom.gradients.success,
              },
            }}
          />
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
            <Typography variant="caption" color="text.secondary">
              {formatDecVCPlatCurrency(decvcplatProject.currentFunding)} raised
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {formatDecVCPlatCurrency(decvcplatProject.fundingGoal)} goal
            </Typography>
          </Box>
        </Box>

        {/* DecVCPlat Project Stats */}
        {!decvcplatCompactMode && (
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <TrendingUp fontSize="small" color="success" sx={{ mr: 0.5 }} />
              <Typography variant="caption" color="text.secondary">
                {decvcplatProject.votes.length} votes
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <CheckCircle fontSize="small" color="info" sx={{ mr: 0.5 }} />
              <Typography variant="caption" color="text.secondary">
                {decvcplatProject.milestones.filter(m => m.isCompleted).length}/{decvcplatProject.milestones.length} milestones
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Schedule fontSize="small" color="action" sx={{ mr: 0.5 }} />
              <Typography variant="caption" color="text.secondary">
                {new Date(decvcplatProject.createdAt).toLocaleDateString()}
              </Typography>
            </Box>
          </Box>
        )}
      </CardContent>

      {/* DecVCPlat Card Actions */}
      {decvcplatShowActions && (
        <CardActions sx={{ p: 2, pt: 0 }}>
          <Button
            component={RouterLink}
            to={`/projects/${decvcplatProject.id}`}
            variant="outlined"
            size="small"
            fullWidth
          >
            View DecVCPlat Project
          </Button>
        </CardActions>
      )}
    </Card>
  );
};

export default ProjectCard;
