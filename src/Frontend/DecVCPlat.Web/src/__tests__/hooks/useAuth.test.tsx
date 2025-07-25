import React from 'react';
import { renderHook, act } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { useAuth } from '../../hooks/useAuth';
import authReducer from '../../store/slices/authSlice';

// Mock store setup
const createMockStore = (initialState = {}) => {
  return configureStore({
    reducer: {
      auth: authReducer,
    },
    preloadedState: {
      auth: {
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
        ...initialState,
      },
    },
  });
};

const createWrapper = (store: any) => {
  return ({ children }: { children: React.ReactNode }) => (
    <Provider store={store}>{children}</Provider>
  );
};

describe('useAuth', () => {
  it('returns initial unauthenticated state', () => {
    const store = createMockStore();
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBe(null);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBe(null);
  });

  it('returns authenticated state when user is logged in', () => {
    const mockUser = {
      id: '1',
      email: 'test@decvcplat.com',
      firstName: 'John',
      lastName: 'Doe',
      role: 'Founder',
      walletAddress: '0x1234567890123456789012345678901234567890',
      hasGdprConsent: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z'
    };

    const store = createMockStore({
      user: mockUser,
      isAuthenticated: true,
    });
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user).toEqual(mockUser);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBe(null);
  });

  it('provides role-based permission checks', () => {
    const mockFounder = {
      id: '1',
      email: 'founder@decvcplat.com',
      firstName: 'Jane',
      lastName: 'Founder',
      role: 'Founder',
      walletAddress: '0x1234567890123456789012345678901234567890',
      hasGdprConsent: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z'
    };

    const store = createMockStore({
      user: mockFounder,
      isAuthenticated: true,
    });
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.hasRole('Founder')).toBe(true);
    expect(result.current.hasRole('Investor')).toBe(false);
    expect(result.current.hasRole('Luminary')).toBe(false);
  });

  it('provides wallet connection status', () => {
    const mockUserWithWallet = {
      id: '1',
      email: 'user@decvcplat.com',
      firstName: 'Bob',
      lastName: 'User',
      role: 'Investor',
      walletAddress: '0x1234567890123456789012345678901234567890',
      hasGdprConsent: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z'
    };

    const store = createMockStore({
      user: mockUserWithWallet,
      isAuthenticated: true,
    });
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.hasWalletConnected()).toBe(true);
  });

  it('handles user without wallet', () => {
    const mockUserWithoutWallet = {
      id: '1',
      email: 'user@decvcplat.com',
      firstName: 'Alice',
      lastName: 'User',
      role: 'Luminary',
      walletAddress: null,
      hasGdprConsent: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z'
    };

    const store = createMockStore({
      user: mockUserWithoutWallet,
      isAuthenticated: true,
    });
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.hasWalletConnected()).toBe(false);
  });

  it('provides GDPR consent status', () => {
    const mockUserWithoutConsent = {
      id: '1',
      email: 'user@decvcplat.com',
      firstName: 'Charlie',
      lastName: 'User',
      role: 'Investor',
      walletAddress: '0x1234567890123456789012345678901234567890',
      hasGdprConsent: false,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z'
    };

    const store = createMockStore({
      user: mockUserWithoutConsent,
      isAuthenticated: true,
    });
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.hasGdprConsent()).toBe(false);
  });

  it('returns false for permission checks when not authenticated', () => {
    const store = createMockStore();
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.hasRole('Founder')).toBe(false);
    expect(result.current.hasWalletConnected()).toBe(false);
    expect(result.current.hasGdprConsent()).toBe(false);
  });

  it('handles loading state', () => {
    const store = createMockStore({
      isLoading: true,
    });
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.isAuthenticated).toBe(false);
  });

  it('handles error state', () => {
    const mockError = 'Authentication failed';
    const store = createMockStore({
      error: mockError,
    });
    const wrapper = createWrapper(store);

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.error).toBe(mockError);
    expect(result.current.isAuthenticated).toBe(false);
  });
});
