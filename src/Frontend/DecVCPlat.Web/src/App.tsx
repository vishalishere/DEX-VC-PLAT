// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { CssBaseline, Box, Typography } from '@mui/material';
import { QueryClient, QueryClientProvider } from 'react-query';
import { Provider } from 'react-redux';
import { HelmetProvider } from 'react-helmet-async';
import { Toaster } from 'react-hot-toast';

// Theme and Store
import { lightTheme, darkTheme } from './theme/theme';
import { store } from './store/store';

// Components
import Header from './components/Layout/Header';
import Footer from './components/Layout/Footer';
import LoadingSpinner from './components/Common/LoadingSpinner';

// Pages
import HomePage from './pages/HomePage';
import LoginPage from './pages/Auth/LoginPage';
import RegisterPage from './pages/Auth/RegisterPage';
// import DashboardPage from './pages/Dashboard/DashboardPage'; // TODO: Complete dashboard implementation
import ProjectsPage from './pages/Projects/ProjectsPage';
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage';
import CreateProjectPage from './pages/Projects/CreateProjectPage';
import DecVCPlatMainControlCenter from './pages/DecVCPlatMainControlCenter';
import VotingPage from './pages/Voting/VotingPage';
import ProfilePage from './pages/Profile/ProfilePage';
import NotificationsPage from './pages/Notifications/NotificationsPage';
import WalletPage from './pages/Wallet/WalletPage';

// Hooks
import { useAppSelector } from './hooks/redux';
import { useAuth } from './hooks/useAuth';

// Routes
import ProtectedRoute from './components/Auth/ProtectedRoute';
import PublicRoute from './components/Auth/PublicRoute';

// Create a client for React Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 3,
      retryDelay: attemptIndex => Math.min(1000 * 2 ** attemptIndex, 30000),
      staleTime: 5 * 60 * 1000, // 5 minutes
      cacheTime: 10 * 60 * 1000, // 10 minutes
    },
  },
});

const App: React.FC = () => {
  const { isDarkMode } = useAppSelector(state => state.theme);
  const { isLoading } = useAuth();

  const theme = isDarkMode ? darkTheme : lightTheme;

  if (isLoading) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Box
          display="flex"
          justifyContent="center"
          alignItems="center"
          minHeight="100vh"
          bgcolor="background.default"
        >
          <LoadingSpinner size={60} />
        </Box>
      </ThemeProvider>
    );
  }

  return (
    <HelmetProvider>
      <Provider store={store}>
        <QueryClientProvider client={queryClient}>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            <Router>
              <Box
                sx={{
                  display: 'flex',
                  flexDirection: 'column',
                  minHeight: '100vh',
                  bgcolor: 'background.default',
                }}
              >
                <Header />
                
                <Box
                  component="main"
                  sx={{
                    flex: 1,
                    display: 'flex',
                    flexDirection: 'column',
                    pt: { xs: 8, sm: 9 }, // Account for fixed header
                  }}
                >
                  <Routes>
                    {/* Public Routes */}
                    <Route path="/" element={<HomePage />} />
                    
                    <Route
                      path="/dashboard"
                      element={<DecVCPlatMainControlCenter />}
                    />
                    
                    <Route
                      path="/login"
                      element={
                        <PublicRoute>
                          <LoginPage />
                        </PublicRoute>
                      }
                    />
                    
                    <Route
                      path="/register"
                      element={
                        <PublicRoute>
                          <RegisterPage />
                        </PublicRoute>
                      }
                    />
                    
                    {/* Protected Routes */}
                    <Route
                      path="/dashboard"
                      element={
                        <ProtectedRoute>
                          <Box sx={{ p: 4, textAlign: 'center' }}>
                            <Typography variant="h4" gutterBottom>Dashboard Coming Soon</Typography>
                            <Typography variant="body1" color="text.secondary">
                              The DecVCPlat dashboard is currently under development.
                            </Typography>
                          </Box>
                        </ProtectedRoute>
                      }
                    />
                    
                    <Route
                      path="/projects"
                      element={
                        <ProtectedRoute>
                          <ProjectsPage />
                        </ProtectedRoute>
                      }
                    />
                    
                    <Route
                      path="/projects/:id"
                      element={
                        <ProtectedRoute>
                          <ProjectDetailsPage />
                        </ProtectedRoute>
                      }
                    />
                    
                    <Route
                      path="/projects/create"
                      element={
                        <ProtectedRoute roles={['Founder']}>
                          <CreateProjectPage />
                        </ProtectedRoute>
                      }
                    />
                    
                    <Route
                      path="/voting"
                      element={
                        <ProtectedRoute roles={['Investor', 'Luminary']}>
                          <VotingPage />
                        </ProtectedRoute>
                      }
                    />
                    
                    <Route
                      path="/profile"
                      element={
                        <ProtectedRoute>
                          <Box sx={{ p: 4, textAlign: 'center' }}>
                            <Typography variant="h4" gutterBottom>Profile Coming Soon</Typography>
                            <Typography variant="body1" color="text.secondary">
                              User profile management is currently under development.
                            </Typography>
                          </Box>
                        </ProtectedRoute>
                      }
                    />
                    
                    <Route
                      path="/notifications"
                      element={
                        <ProtectedRoute>
                          <NotificationsPage />
                        </ProtectedRoute>
                      }
                    />
                    
                    <Route
                      path="/wallet"
                      element={
                        <ProtectedRoute>
                          <WalletPage />
                        </ProtectedRoute>
                      }
                    />
                    
                    {/* 404 Page */}
                    <Route 
                      path="*" 
                      element={
                        <Box
                          display="flex"
                          flexDirection="column"
                          alignItems="center"
                          justifyContent="center"
                          minHeight="60vh"
                          textAlign="center"
                          p={3}
                        >
                          <h1>404 - Page Not Found</h1>
                          <p>The page you're looking for doesn't exist.</p>
                        </Box>
                      } 
                    />
                  </Routes>
                </Box>
                
                <Footer />
              </Box>
            </Router>
            
            {/* Global Toast Notifications */}
            <Toaster
              position="top-right"
              toastOptions={{
                duration: 4000,
                style: {
                  background: theme.palette.background.paper,
                  color: theme.palette.text.primary,
                  border: `1px solid ${theme.palette.divider}`,
                },
                success: {
                  iconTheme: {
                    primary: theme.palette.success.main,
                    secondary: theme.palette.success.contrastText,
                  },
                },
                error: {
                  iconTheme: {
                    primary: theme.palette.error.main,
                    secondary: theme.palette.error.contrastText,
                  },
                },
              }}
            />
          </ThemeProvider>
        </QueryClientProvider>
      </Provider>
    </HelmetProvider>
  );
};

export default App;
