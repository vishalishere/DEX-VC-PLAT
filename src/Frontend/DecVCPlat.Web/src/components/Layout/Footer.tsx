// © 2024 DecVCPlat. All rights reserved.

import React from 'react';
import {
  Box,
  Container,
  Grid,
  Typography,
  Link,
  Divider,
  useTheme,
} from '@mui/material';

const Footer: React.FC = () => {
  const theme = useTheme();
  const currentYear = new Date().getFullYear();

  return (
    <Box
      component="footer"
      sx={{
        bgcolor: 'background.paper',
        borderTop: 1,
        borderColor: 'divider',
        mt: 'auto',
        py: 4,
      }}
    >
      <Container maxWidth="lg">
        <Grid container spacing={4}>
          <Grid item xs={12} sm={6} md={3}>
            <Box
              sx={{
                width: 48,
                height: 48,
                borderRadius: 2,
                background: theme.custom.gradients.primary,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'white',
                fontWeight: 'bold',
                fontSize: '1.5rem',
                mb: 2,
              }}
            >
              D
            </Box>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              DecVCPlat
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Decentralized Venture Capital Platform empowering innovation through blockchain governance.
            </Typography>
          </Grid>

          <Grid item xs={12} sm={6} md={3}>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              Platform
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Link href="/projects" color="text.secondary" underline="hover">
                Projects
              </Link>
              <Link href="/voting" color="text.secondary" underline="hover">
                Voting
              </Link>
              <Link href="/funding" color="text.secondary" underline="hover">
                Funding
              </Link>
            </Box>
          </Grid>

          <Grid item xs={12} sm={6} md={3}>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              Resources
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Link href="/docs" color="text.secondary" underline="hover">
                Documentation
              </Link>
              <Link href="/api" color="text.secondary" underline="hover">
                API Reference
              </Link>
              <Link href="/help" color="text.secondary" underline="hover">
                Help Center
              </Link>
            </Box>
          </Grid>

          <Grid item xs={12} sm={6} md={3}>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 600 }}>
              Legal
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Link href="/terms" color="text.secondary" underline="hover">
                Terms of Service
              </Link>
              <Link href="/privacy" color="text.secondary" underline="hover">
                Privacy Policy
              </Link>
              <Link href="/gdpr" color="text.secondary" underline="hover">
                GDPR Compliance
              </Link>
            </Box>
          </Grid>
        </Grid>

        <Divider sx={{ my: 3 }} />

        <Box
          sx={{
            display: 'flex',
            flexDirection: { xs: 'column', sm: 'row' },
            justifyContent: 'space-between',
            alignItems: 'center',
            gap: 2,
          }}
        >
          <Typography variant="body2" color="text.secondary">
            © {currentYear} DecVCPlat. All rights reserved.
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Built for the decentralized future
          </Typography>
        </Box>
      </Container>
    </Box>
  );
};

export default Footer;
