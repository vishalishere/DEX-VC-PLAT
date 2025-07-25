// © 2024 DecVCPlat. All rights reserved.

import React, { useState } from 'react';
import {
  Box,
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Link,
  Divider,
  Alert,
  InputAdornment,
  IconButton,
  useTheme,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Email,
  Lock,
  Google,
  GitHub,
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate, useLocation } from 'react-router-dom';
import { useFormik } from 'formik';
import * as yup from 'yup';
import { useAppDispatch } from '../../hooks/redux';
import { loginUser } from '../../store/slices/authSlice';
import LoadingSpinner from '../../components/Common/LoadingSpinner';
import { toast } from 'react-hot-toast';
import { Helmet } from 'react-helmet-async';

const validationSchema = yup.object({
  email: yup
    .string()
    .email('Enter a valid email address')
    .required('Email is required'),
  password: yup
    .string()
    .min(8, 'Password should be at least 8 characters')
    .required('Password is required'),
});

const LoginPage: React.FC = () => {
  const theme = useTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const dispatch = useAppDispatch();
  
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const from = (location.state as any)?.from?.pathname || '/dashboard';

  const formik = useFormik({
    initialValues: {
      email: '',
      password: '',
    },
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      setIsLoading(true);
      setError(null);
      
      try {
        const result = await dispatch(loginUser(values));
        
        if (loginUser.fulfilled.match(result)) {
          toast.success('Welcome to DecVCPlat!');
          navigate(from, { replace: true });
        } else {
          const errorMessage = result.payload as string || 'Login failed';
          setError(errorMessage);
          toast.error(errorMessage);
        }
      } catch (err) {
        const errorMessage = 'An unexpected error occurred';
        setError(errorMessage);
        toast.error(errorMessage);
      } finally {
        setIsLoading(false);
      }
    },
  });

  const handleTogglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const handleDemoLogin = (role: 'Founder' | 'Investor' | 'Luminary') => {
    const demoCredentials = {
      Founder: { email: 'founder@decvcplat.com', password: 'demo12345' },
      Investor: { email: 'investor@decvcplat.com', password: 'demo12345' },
      Luminary: { email: 'luminary@decvcplat.com', password: 'demo12345' },
    };

    formik.setValues(demoCredentials[role]);
  };

  return (
    <>
      <Helmet>
        <title>Login - DecVCPlat</title>
        <meta name="description" content="Sign in to your DecVCPlat account to access the decentralized venture capital platform." />
      </Helmet>

      <Container maxWidth="sm">
        <Box
          sx={{
            minHeight: '100vh',
            display: 'flex',
            alignItems: 'center',
            py: 4,
          }}
        >
          <Paper
            elevation={3}
            sx={{
              width: '100%',
              p: 4,
              borderRadius: 3,
              background: theme.palette.background.paper,
            }}
          >
            {/* Header */}
            <Box sx={{ textAlign: 'center', mb: 4 }}>
              <Box
                sx={{
                  width: 64,
                  height: 64,
                  borderRadius: 3,
                  background: theme.custom.gradients.primary,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                  fontWeight: 'bold',
                  fontSize: '2rem',
                  mx: 'auto',
                  mb: 2,
                }}
              >
                D
              </Box>
              <Typography variant="h4" fontWeight={600} gutterBottom>
                Welcome Back
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Sign in to your DecVCPlat account
              </Typography>
            </Box>

            {/* Error Alert */}
            {error && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {error}
              </Alert>
            )}

            {/* Demo Login Buttons */}
            <Box sx={{ mb: 3 }}>
              <Typography variant="body2" color="text.secondary" textAlign="center" sx={{ mb: 2 }}>
                Quick Demo Login:
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', justifyContent: 'center' }}>
                <Button
                  size="small"
                  variant="outlined"
                  onClick={() => handleDemoLogin('Founder')}
                  sx={{ textTransform: 'none' }}
                >
                  Founder
                </Button>
                <Button
                  size="small"
                  variant="outlined"
                  onClick={() => handleDemoLogin('Investor')}
                  sx={{ textTransform: 'none' }}
                >
                  Investor
                </Button>
                <Button
                  size="small"
                  variant="outlined"
                  onClick={() => handleDemoLogin('Luminary')}
                  sx={{ textTransform: 'none' }}
                >
                  Luminary
                </Button>
              </Box>
            </Box>

            <Divider sx={{ mb: 3 }}>or sign in with email</Divider>

            {/* Login Form */}
            <form onSubmit={formik.handleSubmit}>
              <TextField
                fullWidth
                id="email"
                name="email"
                label="Email Address"
                type="email"
                value={formik.values.email}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={formik.touched.email && Boolean(formik.errors.email)}
                helperText={formik.touched.email && formik.errors.email}
                disabled={isLoading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Email color="action" />
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 3 }}
              />

              <TextField
                fullWidth
                id="password"
                name="password"
                label="Password"
                type={showPassword ? 'text' : 'password'}
                value={formik.values.password}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={formik.touched.password && Boolean(formik.errors.password)}
                helperText={formik.touched.password && formik.errors.password}
                disabled={isLoading}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Lock color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={handleTogglePasswordVisibility}
                        edge="end"
                        disabled={isLoading}
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 3 }}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={isLoading}
                sx={{
                  mb: 2,
                  py: 1.5,
                  background: theme.custom.gradients.primary,
                  '&:hover': {
                    background: theme.custom.gradients.primary,
                    opacity: 0.9,
                  },
                }}
              >
                {isLoading ? <LoadingSpinner size={24} message="" centered={false} /> : 'Sign In'}
              </Button>

              <Box sx={{ textAlign: 'center', mb: 3 }}>
                <Link
                  component={RouterLink}
                  to="/forgot-password"
                  variant="body2"
                  color="primary"
                  underline="hover"
                >
                  Forgot your password?
                </Link>
              </Box>
            </form>

            {/* Social Login */}
            <Divider sx={{ mb: 3 }}>or continue with</Divider>

            <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Google />}
                disabled={isLoading}
                sx={{ textTransform: 'none' }}
              >
                Google
              </Button>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<GitHub />}
                disabled={isLoading}
                sx={{ textTransform: 'none' }}
              >
                GitHub
              </Button>
            </Box>

            {/* Sign Up Link */}
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                Don't have an account?{' '}
                <Link
                  component={RouterLink}
                  to="/register"
                  color="primary"
                  underline="hover"
                  fontWeight={500}
                >
                  Create one here
                </Link>
              </Typography>
            </Box>

            {/* Terms */}
            <Box sx={{ textAlign: 'center', mt: 3 }}>
              <Typography variant="caption" color="text.secondary">
                By signing in, you agree to our{' '}
                <Link href="/terms" color="primary" underline="hover">
                  Terms of Service
                </Link>{' '}
                and{' '}
                <Link href="/privacy" color="primary" underline="hover">
                  Privacy Policy
                </Link>
              </Typography>
            </Box>
          </Paper>
        </Box>
      </Container>
    </>
  );
};

export default LoginPage;
