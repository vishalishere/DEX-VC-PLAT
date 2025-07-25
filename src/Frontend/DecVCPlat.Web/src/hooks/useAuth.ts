// © 2024 DecVCPlat. All rights reserved.

import { useEffect, useState } from 'react';
import { useAppSelector, useAppDispatch } from './redux';
import { setUser, setToken } from '../store/slices/authSlice';
import jwt_decode from 'jwt-decode';

interface DecodedToken {
  user_id: string;
  email: string;
  role: string;
  wallet_verified: boolean;
  exp: number;
}

export const useAuth = () => {
  const dispatch = useAppDispatch();
  const { user, token, isAuthenticated, isLoading, error } = useAppSelector(state => state.auth);
  const [isInitializing, setIsInitializing] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const savedToken = localStorage.getItem('decvcplat_token');
        const savedUser = localStorage.getItem('decvcplat_user');

        if (savedToken && savedUser) {
          try {
            const decodedToken = jwt_decode<DecodedToken>(savedToken);
            
            // Check if token is expired
            if (decodedToken.exp * 1000 > Date.now()) {
              dispatch(setToken(savedToken));
              dispatch(setUser(JSON.parse(savedUser)));
            } else {
              // Token expired, clean up
              localStorage.removeItem('decvcplat_token');
              localStorage.removeItem('decvcplat_user');
            }
          } catch (decodeError) {
            // Invalid token, clean up
            localStorage.removeItem('decvcplat_token');
            localStorage.removeItem('decvcplat_user');
          }
        }
      } catch (initError) {
        console.error('DecVCPlat auth initialization error:', initError);
      } finally {
        setIsInitializing(false);
      }
    };

    initializeAuth();
  }, [dispatch]);

  // Save token and user to localStorage when they change
  useEffect(() => {
    if (token) {
      localStorage.setItem('decvcplat_token', token);
    } else {
      localStorage.removeItem('decvcplat_token');
    }
  }, [token]);

  useEffect(() => {
    if (user) {
      localStorage.setItem('decvcplat_user', JSON.stringify(user));
    } else {
      localStorage.removeItem('decvcplat_user');
    }
  }, [user]);

  const hasRole = (requiredRole: string | string[]): boolean => {
    if (!user) return false;
    
    if (Array.isArray(requiredRole)) {
      return requiredRole.includes(user.role);
    }
    
    return user.role === requiredRole;
  };

  const isWalletVerified = (): boolean => {
    return user?.isWalletVerified || false;
  };

  const getUserDisplayName = (): string => {
    return user?.fullName || user?.userName || 'DecVCPlat User';
  };

  const getUserRole = (): string => {
    return user?.role || 'Unknown';
  };

  const canCreateProjects = (): boolean => {
    return hasRole('Founder');
  };

  const canVoteOnProposals = (): boolean => {
    return hasRole(['Investor', 'Luminary']);
  };

  const canCreateProposals = (): boolean => {
    return hasRole('Luminary');
  };

  const canApproveFunding = (): boolean => {
    return hasRole('Luminary');
  };

  const canManageProjects = (): boolean => {
    return hasRole(['Founder', 'Luminary']);
  };

  return {
    // State
    user,
    token,
    isAuthenticated,
    isLoading: isLoading || isInitializing,
    error,

    // Helper functions
    hasRole,
    isWalletVerified,
    getUserDisplayName,
    getUserRole,

    // Permission checks
    canCreateProjects,
    canVoteOnProposals,
    canCreateProposals,
    canApproveFunding,
    canManageProjects,
  };
};
