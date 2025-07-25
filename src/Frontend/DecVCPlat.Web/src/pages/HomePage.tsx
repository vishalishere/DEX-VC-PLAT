// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Grid,
  Card,
  CardContent,
  useTheme,
  Paper,
} from '@mui/material';
import {
  TrendingUp,
  Security,
  Group,
  AccountBalance,
  Verified,
  Speed,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Helmet } from 'react-helmet-async';

const HomePage: React.FC = () => {
  const decvcplatTheme = useTheme();
  const { isAuthenticated, getUserRole } = useAuth();

  const decvcplatFeatures = [
    {
      decvcplatIcon: <TrendingUp fontSize="large" color="primary" />,
      decvcplatTitle: 'Innovative Project Funding',
      decvcplatDescription: 'DecVCPlat connects visionary founders with strategic investors through transparent, blockchain-powered funding mechanisms.',
    },
    {
      decvcplatIcon: <Security fontSize="large" color="primary" />,
      decvcplatTitle: 'Blockchain Governance',
      decvcplatDescription: 'DecVCPlat utilizes secure smart contracts and token-based voting for decentralized decision-making and governance.',
    },
    {
      decvcplatIcon: <Group fontSize="large" color="primary" />,
      decvcplatTitle: 'Expert Community',
      decvcplatDescription: 'DecVCPlat brings together experienced Luminaries who curate projects and guide platform evolution.',
    },
    {
      decvcplatIcon: <AccountBalance fontSize="large" color="primary" />,
      decvcplatTitle: 'Milestone-Based Releases',
      decvcplatDescription: 'DecVCPlat ensures responsible funding through milestone-based tranche releases and community oversight.',
    },
    {
      decvcplatIcon: <Verified fontSize="large" color="primary" />,
      decvcplatTitle: 'Transparent Operations',
      decvcplatDescription: 'DecVCPlat maintains full transparency with on-chain voting records and public milestone tracking.',
    },
    {
      decvcplatIcon: <Speed fontSize="large" color="primary" />,
      decvcplatTitle: 'Rapid Innovation',
      decvcplatDescription: 'DecVCPlat accelerates breakthrough innovation through efficient funding processes and expert guidance.',
    },
  ];

  const decvcplatStats = [
    { decvcplatLabel: 'Active Projects', decvcplatValue: '127' },
    { decvcplatLabel: 'Total Funding', decvcplatValue: '$12.4M' },
    { decvcplatLabel: 'Community Members', decvcplatValue: '2,847' },
    { decvcplatLabel: 'Success Rate', decvcplatValue: '89%' },
  ];

  return (
    <>
      <Helmet>
        <title>DecVCPlat - Decentralized Venture Capital Platform</title>
        <meta name="description" content="DecVCPlat revolutionizes venture capital through blockchain governance, transparent funding, and community-driven innovation." />
      </Helmet>

      {/* DecVCPlat Hero Section */}
      <Box
        sx={{
          background: `linear-gradient(135deg, ${decvcplatTheme.palette.primary.main}20 0%, ${decvcplatTheme.palette.secondary.main}10 100%)`,
          py: 8,
        }}
      >
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography
              variant="h1"
              sx={{
                fontWeight: 700,
                fontSize: { xs: '2.5rem', md: '3.5rem' },
                mb: 2,
                background: decvcplatTheme.custom.gradients.primary,
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
              }}
            >
              DecVCPlat Platform
            </Typography>
            <Typography
              variant="h4"
              color="text.secondary"
              sx={{ mb: 4, fontWeight: 400, fontSize: { xs: '1.2rem', md: '1.5rem' } }}
            >
              Revolutionary Decentralized Venture Capital Ecosystem
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mb: 4, maxWidth: 600, mx: 'auto' }}>
              DecVCPlat transforms traditional venture capital through blockchain governance, 
              transparent milestone funding, and community-driven project curation.
            </Typography>

            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
              {!isAuthenticated ? (
                <>
                  <Button
                    component={RouterLink}
                    to="/register"
                    variant="contained"
                    size="large"
                    sx={{
                      px: 4,
                      py: 1.5,
                      background: decvcplatTheme.custom.gradients.primary,
                      '&:hover': { opacity: 0.9 },
                    }}
                  >
                    Join DecVCPlat Today
                  </Button>
                  <Button
                    component={RouterLink}
                    to="/login"
                    variant="outlined"
                    size="large"
                    sx={{ px: 4, py: 1.5 }}
                  >
                    Access Your Account
                  </Button>
                </>
              ) : (
                <Button
                  component={RouterLink}
                  to="/dashboard"
                  variant="contained"
                  size="large"
                  sx={{
                    px: 4,
                    py: 1.5,
                    background: decvcplatTheme.custom.gradients.primary,
                    '&:hover': { opacity: 0.9 },
                  }}
                >
                  Access DecVCPlat Dashboard ({getUserRole()})
                </Button>
              )}
            </Box>
          </Box>

          {/* DecVCPlat Statistics */}
          <Grid container spacing={3} sx={{ mb: 6 }}>
            {decvcplatStats.map((decvcplatStat, decvcplatIndex) => (
              <Grid item xs={6} md={3} key={decvcplatIndex}>
                <Paper
                  elevation={1}
                  sx={{
                    p: 3,
                    textAlign: 'center',
                    background: decvcplatTheme.palette.background.paper,
                    backdropFilter: 'blur(10px)',
                  }}
                >
                  <Typography variant="h3" color="primary" fontWeight={700} gutterBottom>
                    {decvcplatStat.decvcplatValue}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {decvcplatStat.decvcplatLabel}
                  </Typography>
                </Paper>
              </Grid>
            ))}
          </Grid>
        </Container>
      </Box>

      {/* DecVCPlat Features Section */}
      <Container maxWidth="lg" sx={{ py: 8 }}>
        <Box sx={{ textAlign: 'center', mb: 6 }}>
          <Typography variant="h2" fontWeight={600} gutterBottom>
            DecVCPlat Core Features
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 600, mx: 'auto' }}>
            DecVCPlat combines cutting-edge blockchain technology with proven venture capital principles
            to create the future of innovation funding.
          </Typography>
        </Box>

        <Grid container spacing={4}>
          {decvcplatFeatures.map((decvcplatFeature, decvcplatIndex) => (
            <Grid item xs={12} md={6} lg={4} key={decvcplatIndex}>
              <Card
                elevation={0}
                sx={{
                  height: '100%',
                  p: 3,
                  border: 1,
                  borderColor: 'divider',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    borderColor: 'primary.main',
                    transform: 'translateY(-4px)',
                    boxShadow: decvcplatTheme.custom.shadows.card,
                  },
                }}
              >
                <CardContent sx={{ p: 0 }}>
                  <Box sx={{ mb: 2 }}>
                    {decvcplatFeature.decvcplatIcon}
                  </Box>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    {decvcplatFeature.decvcplatTitle}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {decvcplatFeature.decvcplatDescription}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>

      {/* DecVCPlat Roles Section */}
      <Box sx={{ bgcolor: 'background.paper', py: 8 }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography variant="h2" fontWeight={600} gutterBottom>
              DecVCPlat Participant Roles
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Choose your role in the DecVCPlat ecosystem and contribute to innovation funding.
            </Typography>
          </Box>

          <Grid container spacing={4}>
            <Grid item xs={12} md={4}>
              <Card elevation={1} sx={{ height: '100%', p: 4, textAlign: 'center' }}>
                <Box
                  sx={{
                    width: 80,
                    height: 80,
                    borderRadius: '50%',
                    background: decvcplatTheme.custom.gradients.primary,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                  }}
                >
                  <Typography variant="h4" color="white" fontWeight={700}>
                    F
                  </Typography>
                </Box>
                <Typography variant="h5" fontWeight={600} gutterBottom>
                  Founder
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Submit innovative projects, manage development milestones, and receive DecVCPlat venture capital funding.
                </Typography>
                <Button variant="outlined" sx={{ textTransform: 'none' }}>
                  Become DecVCPlat Founder
                </Button>
              </Card>
            </Grid>

            <Grid item xs={12} md={4}>
              <Card elevation={1} sx={{ height: '100%', p: 4, textAlign: 'center' }}>
                <Box
                  sx={{
                    width: 80,
                    height: 80,
                    borderRadius: '50%',
                    background: decvcplatTheme.custom.gradients.secondary,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                  }}
                >
                  <Typography variant="h4" color="white" fontWeight={700}>
                    I
                  </Typography>
                </Box>
                <Typography variant="h5" fontWeight={600} gutterBottom>
                  Investor
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Stake DecVCPlat tokens, vote on promising proposals, and support breakthrough innovation projects.
                </Typography>
                <Button variant="outlined" sx={{ textTransform: 'none' }}>
                  Become DecVCPlat Investor
                </Button>
              </Card>
            </Grid>

            <Grid item xs={12} md={4}>
              <Card elevation={1} sx={{ height: '100%', p: 4, textAlign: 'center' }}>
                <Box
                  sx={{
                    width: 80,
                    height: 80,
                    borderRadius: '50%',
                    background: decvcplatTheme.custom.gradients.success,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                  }}
                >
                  <Typography variant="h4" color="white" fontWeight={700}>
                    L
                  </Typography>
                </Box>
                <Typography variant="h5" fontWeight={600} gutterBottom>
                  Luminary
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Curate premium projects, create governance proposals, and guide DecVCPlat strategic direction.
                </Typography>
                <Button variant="outlined" sx={{ textTransform: 'none' }}>
                  Become DecVCPlat Luminary
                </Button>
              </Card>
            </Grid>
          </Grid>
        </Container>
      </Box>

      {/* DecVCPlat Call to Action */}
      <Container maxWidth="lg" sx={{ py: 8 }}>
        <Paper
          elevation={1}
          sx={{
            p: 6,
            textAlign: 'center',
            background: `linear-gradient(135deg, ${decvcplatTheme.palette.primary.main}10 0%, ${decvcplatTheme.palette.secondary.main}05 100%)`,
          }}
        >
          <Typography variant="h3" fontWeight={600} gutterBottom>
            Ready to Revolutionize Venture Capital?
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 4, maxWidth: 600, mx: 'auto' }}>
            Join DecVCPlat today and be part of the future of decentralized innovation funding.
            Whether you're a founder, investor, or luminary, your expertise is valuable.
          </Typography>
          <Button
            component={RouterLink}
            to={isAuthenticated ? '/dashboard' : '/register'}
            variant="contained"
            size="large"
            sx={{
              px: 6,
              py: 2,
              background: decvcplatTheme.custom.gradients.primary,
              '&:hover': { opacity: 0.9 },
            }}
          >
            {isAuthenticated ? 'Access DecVCPlat Dashboard' : 'Start Your DecVCPlat Journey'}
          </Button>
        </Paper>
      </Container>
    </>
  );
};

export default HomePage;
