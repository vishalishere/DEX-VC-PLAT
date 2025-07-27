// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline, Box, Typography } from '@mui/material';
import { QueryClient, QueryClientProvider } from 'react-query';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { HelmetProvider } from 'react-helmet-async';

// Create a theme
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
    background: {
      default: '#f5f5f5',
    },
  },
});

// Create a simple store
const store = configureStore({
  reducer: {
    // Add reducers here
  },
});

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
                {/* Header */}
                <Box
                  component="header"
                  sx={{
                    p: 2,
                    bgcolor: 'primary.main',
                    color: 'white',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    position: 'sticky',
                    top: 0,
                    zIndex: 1100,
                    boxShadow: 2
                  }}
                >
                  <Typography variant="h6" sx={{ fontWeight: 'bold', display: 'flex', alignItems: 'center' }}>
                    DecVCPlat
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 3 }}>
                    <Box 
                      component="a" 
                      href="/"
                      sx={{ 
                        color: 'white', 
                        textDecoration: 'none',
                        '&:hover': { textDecoration: 'underline' }
                      }}
                    >
                      Home
                    </Box>
                    <Box 
                      component="a" 
                      href="/dashboard"
                      sx={{ 
                        color: 'white', 
                        textDecoration: 'none',
                        '&:hover': { textDecoration: 'underline' }
                      }}
                    >
                      Dashboard
                    </Box>
                    <Box 
                      component="a" 
                      href="/projects"
                      sx={{ 
                        color: 'white', 
                        textDecoration: 'none',
                        '&:hover': { textDecoration: 'underline' }
                      }}
                    >
                      Projects
                    </Box>
                    <Box 
                      component="a" 
                      href="/voting"
                      sx={{ 
                        color: 'white', 
                        textDecoration: 'none',
                        '&:hover': { textDecoration: 'underline' }
                      }}
                    >
                      Voting
                    </Box>
                  </Box>
                </Box>
                
                <Box
                  component="main"
                  sx={{
                    flex: 1,
                    display: 'flex',
                    flexDirection: 'column',
                    p: 3,
                  }}
                >
                  <Routes>
                    {/* Public Routes */}
                    <Route path="/" element={
                      <Box sx={{ textAlign: 'center', mt: 4 }}>
                        <Typography variant="h3" gutterBottom>Welcome to DecVCPlat</Typography>
                        <Typography variant="h5" color="text.secondary" gutterBottom>
                          Decentralized Venture Capital Platform
                        </Typography>
                        <Typography variant="body1" paragraph>
                          Connect with founders, investors, and industry luminaries in a decentralized ecosystem.
                        </Typography>
                        <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center', gap: 2 }}>
                          <Box 
                            sx={{ 
                              p: 3, 
                              bgcolor: 'background.paper', 
                              borderRadius: 2,
                              boxShadow: 1,
                              width: 250,
                              textAlign: 'center'
                            }}
                          >
                            <Typography variant="h6" gutterBottom>Projects</Typography>
                            <Typography variant="body2">
                              Browse innovative projects seeking funding and support.
                            </Typography>
                          </Box>
                          <Box 
                            sx={{ 
                              p: 3, 
                              bgcolor: 'background.paper', 
                              borderRadius: 2,
                              boxShadow: 1,
                              width: 250,
                              textAlign: 'center'
                            }}
                          >
                            <Typography variant="h6" gutterBottom>Voting</Typography>
                            <Typography variant="body2">
                              Participate in transparent governance and decision-making.
                            </Typography>
                          </Box>
                          <Box 
                            sx={{ 
                              p: 3, 
                              bgcolor: 'background.paper', 
                              borderRadius: 2,
                              boxShadow: 1,
                              width: 250,
                              textAlign: 'center'
                            }}
                          >
                            <Typography variant="h6" gutterBottom>Funding</Typography>
                            <Typography variant="body2">
                              Access decentralized funding opportunities for your projects.
                            </Typography>
                          </Box>
                        </Box>
                      </Box>
                    } />
                    
                    <Route path="/dashboard" element={
                      <Box sx={{ p: 4 }}>
                        <Typography variant="h4" gutterBottom>DecVCPlat Dashboard</Typography>
                        <Typography variant="body1" color="text.secondary" paragraph>
                          Welcome to the DecVCPlat platform dashboard.
                        </Typography>
                        
                        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 3, mt: 4 }}>
                          <Box sx={{ p: 3, bgcolor: 'background.paper', borderRadius: 2, boxShadow: 1 }}>
                            <Typography variant="h6" gutterBottom>Active Projects</Typography>
                            <Typography variant="h4" color="primary">12</Typography>
                            <Typography variant="body2" color="text.secondary">Projects currently seeking funding</Typography>
                          </Box>
                          
                          <Box sx={{ p: 3, bgcolor: 'background.paper', borderRadius: 2, boxShadow: 1 }}>
                            <Typography variant="h6" gutterBottom>Active Votes</Typography>
                            <Typography variant="h4" color="primary">5</Typography>
                            <Typography variant="body2" color="text.secondary">Governance proposals open for voting</Typography>
                          </Box>
                          
                          <Box sx={{ p: 3, bgcolor: 'background.paper', borderRadius: 2, boxShadow: 1 }}>
                            <Typography variant="h6" gutterBottom>Total Funds</Typography>
                            <Typography variant="h4" color="primary">$2.4M</Typography>
                            <Typography variant="body2" color="text.secondary">Available for investment</Typography>
                          </Box>
                          
                          <Box sx={{ p: 3, bgcolor: 'background.paper', borderRadius: 2, boxShadow: 1 }}>
                            <Typography variant="h6" gutterBottom>Platform Users</Typography>
                            <Typography variant="h4" color="primary">1,250</Typography>
                            <Typography variant="body2" color="text.secondary">Active platform participants</Typography>
                          </Box>
                        </Box>
                        
                        <Box sx={{ mt: 6 }}>
                          <Typography variant="h5" gutterBottom>Recent Activity</Typography>
                          <Box sx={{ bgcolor: 'background.paper', borderRadius: 2, boxShadow: 1, overflow: 'hidden' }}>
                            {[1, 2, 3, 4, 5].map((item) => (
                              <Box key={item} sx={{ p: 2, borderBottom: '1px solid', borderColor: 'divider', '&:last-child': { borderBottom: 'none' } }}>
                                <Typography variant="subtitle1">Activity Item {item}</Typography>
                                <Typography variant="body2" color="text.secondary">Lorem ipsum dolor sit amet, consectetur adipiscing elit.</Typography>
                              </Box>
                            ))}
                          </Box>
                        </Box>
                      </Box>
                    } />
                    
                    <Route path="/projects" element={
                      <Box sx={{ p: 4 }}>
                        <Typography variant="h4" gutterBottom>Projects</Typography>
                        <Typography variant="body1" paragraph>
                          Browse innovative projects seeking funding and support on the DecVCPlat platform.
                        </Typography>
                        
                        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: 3, mt: 4 }}>
                          {[1, 2, 3, 4, 5, 6].map((project) => (
                            <Box 
                              key={project}
                              sx={{ 
                                bgcolor: 'background.paper', 
                                borderRadius: 2, 
                                boxShadow: 1,
                                overflow: 'hidden',
                                transition: 'transform 0.3s, box-shadow 0.3s',
                                '&:hover': {
                                  transform: 'translateY(-4px)',
                                  boxShadow: 3
                                }
                              }}
                            >
                              <Box 
                                sx={{ 
                                  height: 140, 
                                  bgcolor: `hsl(${project * 60}, 70%, 65%)`,
                                  display: 'flex',
                                  alignItems: 'center',
                                  justifyContent: 'center'
                                }}
                              >
                                <Typography variant="h6" color="white">Project {project}</Typography>
                              </Box>
                              <Box sx={{ p: 2 }}>
                                <Typography variant="h6">Project Title {project}</Typography>
                                <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                                  A brief description of this innovative project and what it aims to accomplish in the decentralized ecosystem.
                                </Typography>
                                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 2 }}>
                                  <Typography variant="body2" fontWeight="bold" color="primary">
                                    Funding: ${(project * 50000).toLocaleString()}
                                  </Typography>
                                  <Box 
                                    component="a"
                                    href={`/projects/${project}`}
                                    sx={{ 
                                      textDecoration: 'none',
                                      color: 'primary.main',
                                      fontWeight: 'medium',
                                      '&:hover': { textDecoration: 'underline' }
                                    }}
                                  >
                                    View Details
                                  </Box>
                                </Box>
                              </Box>
                            </Box>
                          ))}
                        </Box>
                      </Box>
                    } />
                    
                    <Route path="/voting" element={
                      <Box sx={{ p: 4 }}>
                        <Typography variant="h4" gutterBottom>Voting</Typography>
                        <Typography variant="body1" paragraph>
                          Participate in transparent governance and decision-making on the DecVCPlat platform.
                        </Typography>
                        
                        <Box sx={{ bgcolor: 'background.paper', borderRadius: 2, boxShadow: 1, overflow: 'hidden', mt: 4 }}>
                          {[1, 2, 3, 4, 5].map((vote) => (
                            <Box key={vote} sx={{ p: 3, borderBottom: '1px solid', borderColor: 'divider', '&:last-child': { borderBottom: 'none' } }}>
                              <Typography variant="h6">Governance Proposal #{vote}</Typography>
                              <Typography variant="body2" color="text.secondary" paragraph>
                                This proposal aims to improve the platform by implementing new features and enhancing existing functionality.
                              </Typography>
                              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 2 }}>
                                <Box>
                                  <Typography variant="body2">Status: <Box component="span" sx={{ color: 'success.main', fontWeight: 'medium' }}>Active</Box></Typography>
                                  <Typography variant="body2">Ends in: {vote + 2} days</Typography>
                                </Box>
                                <Box sx={{ display: 'flex', gap: 1 }}>
                                  <Box component="button" sx={{ px: 2, py: 1, bgcolor: 'success.main', color: 'white', border: 'none', borderRadius: 1, cursor: 'pointer' }}>Vote Yes</Box>
                                  <Box component="button" sx={{ px: 2, py: 1, bgcolor: 'error.main', color: 'white', border: 'none', borderRadius: 1, cursor: 'pointer' }}>Vote No</Box>
                                </Box>
                              </Box>
                            </Box>
                          ))}
                        </Box>
                      </Box>
                    } />
                  </Routes>
                </Box>
                
                {/* Footer would go here */}
                <Box
                  component="footer"
                  sx={{
                    p: 2,
                    bgcolor: 'grey.200',
                    textAlign: 'center',
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    © 2024 DecVCPlat. All rights reserved.
                  </Typography>
                </Box>
              </Box>
            </Router>
          </ThemeProvider>
        </QueryClientProvider>
      </Provider>
    </HelmetProvider>
  );
};

export default App;
