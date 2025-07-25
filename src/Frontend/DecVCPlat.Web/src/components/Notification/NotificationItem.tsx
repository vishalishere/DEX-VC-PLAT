// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import {
  Box,
  Typography,
  Avatar,
  Chip,
  IconButton,
  Button,
  useTheme,
  alpha,
} from '@mui/material';
import {
  Notifications,
  TrendingUp,
  HowToVote,
  MonetizationOn,
  CheckCircle,
  Warning,
  Info,
  Error,
  Close,
  MarkEmailRead,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { Notification } from '../../store/slices/notificationSlice';

interface DecVCPlatNotificationItemProps {
  decvcplatNotification: Notification;
  decvcplatShowActions?: boolean;
  decvcplatCompactMode?: boolean;
  onDecVCPlatMarkAsRead?: (notificationId: string) => void;
  onDecVCPlatDismiss?: (notificationId: string) => void;
}

const NotificationItem: React.FC<DecVCPlatNotificationItemProps> = ({
  decvcplatNotification,
  decvcplatShowActions = true,
  decvcplatCompactMode = false,
  onDecVCPlatMarkAsRead,
  onDecVCPlatDismiss,
}) => {
  const decvcplatTheme = useTheme();

  const getDecVCPlatNotificationIcon = () => {
    switch (decvcplatNotification.type) {
      case 'ProjectUpdate':
        return <TrendingUp fontSize="small" />;
      case 'VotingResult':
        return <HowToVote fontSize="small" />;
      case 'FundingRelease':
        return <MonetizationOn fontSize="small" />;
      case 'MilestoneComplete':
        return <CheckCircle fontSize="small" />;
      case 'Success':
        return <CheckCircle fontSize="small" />;
      case 'Warning':
        return <Warning fontSize="small" />;
      case 'System':
        return <Info fontSize="small" />;
      default:
        return <Notifications fontSize="small" />;
    }
  };

  const getDecVCPlatNotificationColor = () => {
    switch (decvcplatNotification.type) {
      case 'ProjectUpdate':
        return decvcplatTheme.palette.primary.main;
      case 'VotingResult':
        return decvcplatTheme.palette.secondary.main;
      case 'FundingRelease':
        return decvcplatTheme.palette.success.main;
      case 'MilestoneComplete':
        return decvcplatTheme.palette.info.main;
      case 'Success':
        return decvcplatTheme.palette.success.main;
      case 'Warning':
        return decvcplatTheme.palette.warning.main;
      case 'System':
        return decvcplatTheme.palette.info.main;
      default:
        return decvcplatTheme.palette.grey[500];
    }
  };

  const getDecVCPlatPriorityColor = (): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (decvcplatNotification.priority) {
      case 'Critical':
        return 'error';
      case 'High':
        return 'warning';
      case 'Normal':
        return 'info';
      case 'Low':
        return 'default';
      default:
        return 'default';
    }
  };

  const formatDecVCPlatRelativeTime = (decvcplatTimestamp: string): string => {
    const decvcplatNotificationTime = new Date(decvcplatTimestamp);
    const decvcplatNow = new Date();
    const decvcplatTimeDiff = decvcplatNow.getTime() - decvcplatNotificationTime.getTime();

    const decvcplatMinutes = Math.floor(decvcplatTimeDiff / (1000 * 60));
    const decvcplatHours = Math.floor(decvcplatTimeDiff / (1000 * 60 * 60));
    const decvcplatDays = Math.floor(decvcplatTimeDiff / (1000 * 60 * 60 * 24));

    if (decvcplatMinutes < 1) return 'Just now';
    if (decvcplatMinutes < 60) return `${decvcplatMinutes}m ago`;
    if (decvcplatHours < 24) return `${decvcplatHours}h ago`;
    if (decvcplatDays < 7) return `${decvcplatDays}d ago`;
    return decvcplatNotificationTime.toLocaleDateString();
  };

  const decvcplatNotificationColor = getDecVCPlatNotificationColor();

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'flex-start',
        p: decvcplatCompactMode ? 2 : 3,
        backgroundColor: decvcplatNotification.isRead 
          ? 'transparent' 
          : alpha(decvcplatNotificationColor, 0.05),
        borderLeft: `4px solid ${decvcplatNotification.isRead ? 'transparent' : decvcplatNotificationColor}`,
        borderRadius: 1,
        transition: 'all 0.3s ease',
        '&:hover': {
          backgroundColor: alpha(decvcplatNotificationColor, 0.08),
        },
      }}
    >
      {/* DecVCPlat Notification Icon */}
      <Avatar
        sx={{
          width: decvcplatCompactMode ? 36 : 40,
          height: decvcplatCompactMode ? 36 : 40,
          backgroundColor: alpha(decvcplatNotificationColor, 0.1),
          color: decvcplatNotificationColor,
          mr: 2,
        }}
      >
        {getDecVCPlatNotificationIcon()}
      </Avatar>

      {/* DecVCPlat Notification Content */}
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ flex: 1, mr: 2 }}>
            <Typography
              variant={decvcplatCompactMode ? 'body2' : 'body1'}
              fontWeight={decvcplatNotification.isRead ? 'normal' : 'bold'}
              sx={{ mb: 0.5 }}
            >
              {decvcplatNotification.title}
            </Typography>
            <Typography
              variant="body2"
              color="text.secondary"
              sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                display: '-webkit-box',
                WebkitLineClamp: decvcplatCompactMode ? 2 : 3,
                WebkitBoxOrient: 'vertical',
              }}
            >
              {decvcplatNotification.message}
            </Typography>
          </Box>

          {/* DecVCPlat Priority and Actions */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {decvcplatNotification.priority !== 'Normal' && (
              <Chip
                label={decvcplatNotification.priority}
                color={getDecVCPlatPriorityColor()}
                size="small"
                variant="outlined"
              />
            )}
            {decvcplatShowActions && (
              <Box sx={{ display: 'flex', gap: 0.5 }}>
                {!decvcplatNotification.isRead && (
                  <IconButton
                    size="small"
                    onClick={() => onDecVCPlatMarkAsRead?.(decvcplatNotification.id)}
                    title="Mark as read"
                  >
                    <MarkEmailRead fontSize="small" />
                  </IconButton>
                )}
                <IconButton
                  size="small"
                  onClick={() => onDecVCPlatDismiss?.(decvcplatNotification.id)}
                  title="Dismiss notification"
                >
                  <Close fontSize="small" />
                </IconButton>
              </Box>
            )}
          </Box>
        </Box>

        {/* DecVCPlat Notification Metadata */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 2 }}>
          <Typography variant="caption" color="text.secondary">
            {formatDecVCPlatRelativeTime(decvcplatNotification.createdAt)}
          </Typography>

          {/* DecVCPlat Action Button */}
          {decvcplatNotification.actionUrl && decvcplatNotification.actionText && (
            <Button
              component={RouterLink}
              to={decvcplatNotification.actionUrl}
              variant="text"
              size="small"
              sx={{
                textTransform: 'none',
                color: decvcplatNotificationColor,
                '&:hover': {
                  backgroundColor: alpha(decvcplatNotificationColor, 0.1),
                },
              }}
            >
              {decvcplatNotification.actionText}
            </Button>
          )}
        </Box>

        {/* DecVCPlat Read Indicator */}
        {decvcplatNotification.isRead && decvcplatNotification.readAt && (
          <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
            Read on {new Date(decvcplatNotification.readAt).toLocaleString()}
          </Typography>
        )}
      </Box>
    </Box>
  );
};

export default NotificationItem;
