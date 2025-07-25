// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import { Box, CircularProgress, Typography, useTheme } from '@mui/material';

interface LoadingSpinnerProps {
  size?: number;
  message?: string;
  centered?: boolean;
  minHeight?: string | number;
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 40,
  message = 'Loading...',
  centered = true,
  minHeight = '200px',
}) => {
  const theme = useTheme();

  const content = (
    <>
      <CircularProgress
        size={size}
        sx={{
          color: theme.palette.primary.main,
          mb: message ? 2 : 0,
        }}
      />
      {message && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ textAlign: 'center' }}
        >
          {message}
        </Typography>
      )}
    </>
  );

  if (!centered) {
    return <>{content}</>;
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight,
        p: 2,
      }}
    >
      {content}
    </Box>
  );
};

export default LoadingSpinner;
