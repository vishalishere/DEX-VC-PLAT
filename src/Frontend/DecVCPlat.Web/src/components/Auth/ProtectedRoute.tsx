// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { Box, Typography, Paper } from '@mui/material';
import LoadingSpinner from '../Common/LoadingSpinner';

interface ProtectedRouteProps {
  children: React.ReactNode;
  roles?: string[];
  requireWalletVerification?: boolean;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  roles,
  requireWalletVerification = false,
}) => {
  const { isAuthenticated, isLoading, user, hasRole, isWalletVerified } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <LoadingSpinner message="Verifying authentication..." />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && !hasRole(roles)) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '60vh',
          p: 3,
        }}
      >
        <Paper
          elevation={1}
          sx={{
            p: 4,
            textAlign: 'center',
            maxWidth: 400,
          }}
        >
          <Typography variant="h5" color="error" gutterBottom>
            Access Denied
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
            Your current role ({user?.role}) does not have permission to access this page.
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Required roles: {roles.join(', ')}
          </Typography>
        </Paper>
      </Box>
    );
  }

  if (requireWalletVerification && !isWalletVerified()) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '60vh',
          p: 3,
        }}
      >
        <Paper
          elevation={1}
          sx={{
            p: 4,
            textAlign: 'center',
            maxWidth: 400,
          }}
        >
          <Typography variant="h5" color="warning.main" gutterBottom>
            Wallet Verification Required
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Please connect and verify your wallet to access this feature.
          </Typography>
        </Paper>
      </Box>
    );
  }

  return <>{children}</>;
};

export default ProtectedRoute;
