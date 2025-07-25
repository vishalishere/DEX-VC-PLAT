// © 2024 DecVCPlat. All rights reserved.

import React, { useState } from 'react';
import { Box, Container, Paper, TextField, Button, Typography, Link, Alert, FormControl, InputLabel, Select, MenuItem, useTheme } from '@mui/material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../../hooks/redux';
import { registerUser } from '../../store/slices/authSlice';
import { toast } from 'react-hot-toast';
import { Helmet } from 'react-helmet-async';

interface DecVCPlatRegistrationForm {
  decvcplatUsername: string;
  decvcplatEmail: string;
  decvcplatFullName: string;
  decvcplatPassword: string;
  decvcplatPasswordConfirm: string;
  decvcplatUserRole: string;
  decvcplatTermsAccepted: boolean;
  decvcplatPrivacyAccepted: boolean;
}

const RegisterPage: React.FC = () => {
  const decvcplatTheme = useTheme();
  const decvcplatNavigator = useNavigate();
  const decvcplatDispatcher = useAppDispatch();
  
  const [decvcplatFormState, setDecVCPlatFormState] = useState<DecVCPlatRegistrationForm>({
    decvcplatUsername: '',
    decvcplatEmail: '',
    decvcplatFullName: '',
    decvcplatPassword: '',
    decvcplatPasswordConfirm: '',
    decvcplatUserRole: '',
    decvcplatTermsAccepted: false,
    decvcplatPrivacyAccepted: false,
  });
  
  const [decvcplatValidationErrors, setDecVCPlatValidationErrors] = useState<Partial<DecVCPlatRegistrationForm>>({});
  const [decvcplatProcessingState, setDecVCPlatProcessingState] = useState(false);
  const [decvcplatErrorMessage, setDecVCPlatErrorMessage] = useState<string | null>(null);

  const validateDecVCPlatRegistrationData = (): boolean => {
    const decvcplatErrors: Partial<DecVCPlatRegistrationForm> = {};
    
    if (!decvcplatFormState.decvcplatUsername || decvcplatFormState.decvcplatUsername.length < 3) {
      decvcplatErrors.decvcplatUsername = 'DecVCPlat username requires minimum 3 characters';
    }
    
    if (!decvcplatFormState.decvcplatEmail || !decvcplatFormState.decvcplatEmail.includes('@')) {
      decvcplatErrors.decvcplatEmail = 'DecVCPlat valid email address required';
    }
    
    if (!decvcplatFormState.decvcplatFullName || decvcplatFormState.decvcplatFullName.length < 2) {
      decvcplatErrors.decvcplatFullName = 'DecVCPlat full name required';
    }
    
    if (!decvcplatFormState.decvcplatPassword || decvcplatFormState.decvcplatPassword.length < 8) {
      decvcplatErrors.decvcplatPassword = 'DecVCPlat password requires minimum 8 characters';
    }
    
    if (decvcplatFormState.decvcplatPassword !== decvcplatFormState.decvcplatPasswordConfirm) {
      decvcplatErrors.decvcplatPasswordConfirm = 'DecVCPlat password confirmation must match';
    }
    
    if (!decvcplatFormState.decvcplatUserRole) {
      decvcplatErrors.decvcplatUserRole = 'DecVCPlat role selection required';
    }
    
    if (!decvcplatFormState.decvcplatTermsAccepted) {
      decvcplatErrors.decvcplatTermsAccepted = true;
    }
    
    if (!decvcplatFormState.decvcplatPrivacyAccepted) {
      decvcplatErrors.decvcplatPrivacyAccepted = true;
    }
    
    setDecVCPlatValidationErrors(decvcplatErrors);
    return Object.keys(decvcplatErrors).length === 0;
  };

  const handleDecVCPlatFormFieldUpdate = (decvcplatFieldName: keyof DecVCPlatRegistrationForm, decvcplatFieldValue: string | boolean) => {
    setDecVCPlatFormState(decvcplatPreviousState => ({
      ...decvcplatPreviousState,
      [decvcplatFieldName]: decvcplatFieldValue
    }));
    
    if (decvcplatValidationErrors[decvcplatFieldName]) {
      setDecVCPlatValidationErrors(decvcplatPreviousErrors => ({
        ...decvcplatPreviousErrors,
        [decvcplatFieldName]: undefined
      }));
    }
  };

  const processDecVCPlatRegistration = async (decvcplatEvent: React.FormEvent) => {
    decvcplatEvent.preventDefault();
    
    if (!validateDecVCPlatRegistrationData()) {
      return;
    }
    
    setDecVCPlatProcessingState(true);
    setDecVCPlatErrorMessage(null);
    
    try {
      const decvcplatRegistrationPayload = {
        userName: decvcplatFormState.decvcplatUsername,
        email: decvcplatFormState.decvcplatEmail,
        fullName: decvcplatFormState.decvcplatFullName,
        password: decvcplatFormState.decvcplatPassword,
        role: decvcplatFormState.decvcplatUserRole as 'Founder' | 'Investor' | 'Luminary',
      };

      const decvcplatRegistrationResult = await decvcplatDispatcher(registerUser(decvcplatRegistrationPayload));
      
      if (registerUser.fulfilled.match(decvcplatRegistrationResult)) {
        toast.success('DecVCPlat account creation successful! Welcome to the platform.');
        decvcplatNavigator('/dashboard', { replace: true });
      } else {
        const decvcplatFailureMessage = decvcplatRegistrationResult.payload as string || 'DecVCPlat registration process failed';
        setDecVCPlatErrorMessage(decvcplatFailureMessage);
        toast.error(decvcplatFailureMessage);
      }
    } catch (decvcplatException) {
      const decvcplatExceptionMessage = 'DecVCPlat registration encountered unexpected system error';
      setDecVCPlatErrorMessage(decvcplatExceptionMessage);
      toast.error(decvcplatExceptionMessage);
    } finally {
      setDecVCPlatProcessingState(false);
    }
  };

  const getDecVCPlatRoleExplanation = (decvcplatSelectedRole: string): string => {
    switch (decvcplatSelectedRole) {
      case 'Founder':
        return 'DecVCPlat Founder Role: Submit innovative projects, manage development milestones, receive venture capital funding';
      case 'Investor':
        return 'DecVCPlat Investor Role: Stake governance tokens, participate in proposal voting, support promising innovations';
      case 'Luminary':
        return 'DecVCPlat Luminary Role: Curate premium projects, create governance proposals, guide platform strategic direction';
      default:
        return 'Select your DecVCPlat participant role for detailed explanation';
    }
  };

  return (
    <>
      <Helmet>
        <title>DecVCPlat Account Creation Portal</title>
        <meta name="description" content="Create your DecVCPlat account for decentralized venture capital platform access as Founder, Investor, or Luminary participant." />
      </Helmet>

      <Container maxWidth="md">
        <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', py: 4 }}>
          <Paper elevation={3} sx={{ width: '100%', p: 4, borderRadius: 3, background: decvcplatTheme.palette.background.paper }}>
            
            <Box sx={{ textAlign: 'center', mb: 4 }}>
              <Box sx={{
                width: 64, height: 64, borderRadius: 3, background: decvcplatTheme.custom.gradients.primary,
                display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white',
                fontWeight: 'bold', fontSize: '2rem', mx: 'auto', mb: 2,
              }}>D</Box>
              <Typography variant="h4" fontWeight={600} gutterBottom>DecVCPlat Registration</Typography>
              <Typography variant="body1" color="text.secondary">
                Join the DecVCPlat decentralized venture capital ecosystem
              </Typography>
            </Box>

            {decvcplatErrorMessage && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {decvcplatErrorMessage}
              </Alert>
            )}

            <form onSubmit={processDecVCPlatRegistration}>
              <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3, mb: 3 }}>
                <TextField
                  fullWidth
                  label="DecVCPlat Username"
                  value={decvcplatFormState.decvcplatUsername}
                  onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatUsername', e.target.value)}
                  error={Boolean(decvcplatValidationErrors.decvcplatUsername)}
                  helperText={decvcplatValidationErrors.decvcplatUsername}
                  disabled={decvcplatProcessingState}
                />
                <TextField
                  fullWidth
                  label="Complete Full Name"
                  value={decvcplatFormState.decvcplatFullName}
                  onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatFullName', e.target.value)}
                  error={Boolean(decvcplatValidationErrors.decvcplatFullName)}
                  helperText={decvcplatValidationErrors.decvcplatFullName}
                  disabled={decvcplatProcessingState}
                />
              </Box>

              <TextField
                fullWidth
                label="Email Address"
                type="email"
                value={decvcplatFormState.decvcplatEmail}
                onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatEmail', e.target.value)}
                error={Boolean(decvcplatValidationErrors.decvcplatEmail)}
                helperText={decvcplatValidationErrors.decvcplatEmail}
                disabled={decvcplatProcessingState}
                sx={{ mb: 3 }}
              />

              <FormControl fullWidth sx={{ mb: 3 }}>
                <InputLabel>DecVCPlat Participant Role</InputLabel>
                <Select
                  value={decvcplatFormState.decvcplatUserRole}
                  label="DecVCPlat Participant Role"
                  onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatUserRole', e.target.value)}
                  error={Boolean(decvcplatValidationErrors.decvcplatUserRole)}
                  disabled={decvcplatProcessingState}
                >
                  <MenuItem value="Founder">Founder</MenuItem>
                  <MenuItem value="Investor">Investor</MenuItem>
                  <MenuItem value="Luminary">Luminary</MenuItem>
                </Select>
                {decvcplatFormState.decvcplatUserRole && (
                  <Typography variant="caption" color="text.secondary" sx={{ mt: 1 }}>
                    {getDecVCPlatRoleExplanation(decvcplatFormState.decvcplatUserRole)}
                  </Typography>
                )}
              </FormControl>

              <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3, mb: 3 }}>
                <TextField
                  fullWidth
                  label="Secure Password"
                  type="password"
                  value={decvcplatFormState.decvcplatPassword}
                  onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatPassword', e.target.value)}
                  error={Boolean(decvcplatValidationErrors.decvcplatPassword)}
                  helperText={decvcplatValidationErrors.decvcplatPassword}
                  disabled={decvcplatProcessingState}
                />
                <TextField
                  fullWidth
                  label="Confirm Secure Password"
                  type="password"
                  value={decvcplatFormState.decvcplatPasswordConfirm}
                  onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatPasswordConfirm', e.target.value)}
                  error={Boolean(decvcplatValidationErrors.decvcplatPasswordConfirm)}
                  helperText={decvcplatValidationErrors.decvcplatPasswordConfirm}
                  disabled={decvcplatProcessingState}
                />
              </Box>

              <Box sx={{ mb: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <input
                    type="checkbox"
                    checked={decvcplatFormState.decvcplatTermsAccepted}
                    onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatTermsAccepted', e.target.checked)}
                    disabled={decvcplatProcessingState}
                    style={{ marginRight: 8 }}
                  />
                  <Typography variant="body2">
                    Accept DecVCPlat{' '}
                    <Link href="/terms" color="primary" underline="hover">Terms of Service</Link>
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <input
                    type="checkbox"
                    checked={decvcplatFormState.decvcplatPrivacyAccepted}
                    onChange={(e) => handleDecVCPlatFormFieldUpdate('decvcplatPrivacyAccepted', e.target.checked)}
                    disabled={decvcplatProcessingState}
                    style={{ marginRight: 8 }}
                  />
                  <Typography variant="body2">
                    Accept DecVCPlat{' '}
                    <Link href="/privacy" color="primary" underline="hover">Privacy Policy</Link>{' '}
                    and GDPR compliance
                  </Typography>
                </Box>
              </Box>

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={decvcplatProcessingState}
                sx={{
                  mb: 2, py: 1.5,
                  background: decvcplatTheme.custom.gradients.primary,
                  '&:hover': { background: decvcplatTheme.custom.gradients.primary, opacity: 0.9 },
                }}
              >
                {decvcplatProcessingState ? 'Creating DecVCPlat Account...' : 'Create DecVCPlat Account'}
              </Button>
            </form>

            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                Already registered with DecVCPlat?{' '}
                <Link component={RouterLink} to="/login" color="primary" underline="hover" fontWeight={500}>
                  Access your DecVCPlat account
                </Link>
              </Typography>
            </Box>
          </Paper>
        </Box>
      </Container>
    </>
  );
};

export default RegisterPage;
