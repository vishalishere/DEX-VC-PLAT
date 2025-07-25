// © 2024 DecVCPlat. All rights reserved.

import React, { useEffect, useState } from 'react';
import { Container, Grid, Box, Typography, Button, Card, CardContent, TextField, Dialog, DialogTitle, DialogContent, DialogActions, useTheme, Paper, Chip, List, ListItem, ListItemText, ListItemAvatar, Avatar, Divider } from '@mui/material';
import { AccountBalanceWallet, Send, CallReceived, TrendingUp, Security, Refresh, SwapHoriz } from '@mui/icons-material';
import { useAuth } from '../../hooks/useAuth';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { establishWalletConnection, disconnectWallet, stakeDecVCPlatTokens, unstakeDecVCPlatTokens, fetchDecVCPlatTransactionHistory, switchDecVCPlatNetwork } from '../../store/slices/walletSlice';
import LoadingSpinner from '../../components/Common/LoadingSpinner';
import { toast } from 'react-hot-toast';
import { Helmet } from 'react-helmet-async';

const WalletPage: React.FC = () => {
  const decvcplatTheme = useTheme();
  const decvcplatAuth = useAuth();
  const decvcplatDispatch = useAppDispatch();
  
  const { 
    decvcplatWalletConnected,
    decvcplatWalletAddress,
    decvcplatTokenBalance,
    decvcplatEthBalance,
    decvcplatStakedBalance,
    decvcplatTransactionHistory,
    decvcplatNetworkInfo,
    decvcplatLoading 
  } = useAppSelector(state => state.decvcplatWallet);
  
  const [decvcplatStakeDialogOpen, setDecVCPlatStakeDialogOpen] = useState(false);
  const [decvcplatStakeAmount, setDecVCPlatStakeAmount] = useState('');
  const [decvcplatUnstakeDialogOpen, setDecVCPlatUnstakeDialogOpen] = useState(false);
  const [decvcplatUnstakeAmount, setDecVCPlatUnstakeAmount] = useState('');

  useEffect(() => {
    if (decvcplatWalletConnected) {
      decvcplatDispatch(fetchDecVCPlatTransactionHistory());
    }
  }, [decvcplatDispatch, decvcplatWalletConnected]);

  const handleDecVCPlatWalletConnect = async () => {
    try {
      const decvcplatResult = await decvcplatDispatch(establishWalletConnection('MetaMask'));
      if (establishWalletConnection.fulfilled.match(decvcplatResult)) {
        toast.success('DecVCPlat wallet connected successfully');
      }
    } catch (decvcplatError) {
      toast.error('Failed to connect DecVCPlat wallet');
    }
  };

  const handleDecVCPlatWalletDisconnect = () => {
    decvcplatDispatch(disconnectWallet());
    toast.success('DecVCPlat wallet disconnected');
  };

  const handleDecVCPlatTokenStake = async () => {
    if (!decvcplatStakeAmount || parseFloat(decvcplatStakeAmount) <= 0) {
      toast.error('Please enter a valid DecVCPlat stake amount');
      return;
    }

    try {
      const decvcplatStakeData = {
        decvcplatAmount: parseFloat(decvcplatStakeAmount),
        decvcplatDuration: 30, // 30 days
      };

      const decvcplatResult = await decvcplatDispatch(stakeDecVCPlatTokens(decvcplatStakeData));
      if (stakeDecVCPlatTokens.fulfilled.match(decvcplatResult)) {
        toast.success('DecVCPlat tokens staked successfully');
        setDecVCPlatStakeDialogOpen(false);
        setDecVCPlatStakeAmount('');
      }
    } catch (decvcplatError) {
      toast.error('Failed to stake DecVCPlat tokens');
    }
  };

  const handleDecVCPlatTokenUnstake = async () => {
    if (!decvcplatUnstakeAmount || parseFloat(decvcplatUnstakeAmount) <= 0) {
      toast.error('Please enter a valid DecVCPlat unstake amount');
      return;
    }

    try {
      const decvcplatUnstakeData = {
        decvcplatAmount: parseFloat(decvcplatUnstakeAmount),
      };

      const decvcplatResult = await decvcplatDispatch(unstakeDecVCPlatTokens(decvcplatUnstakeData));
      if (unstakeDecVCPlatTokens.fulfilled.match(decvcplatResult)) {
        toast.success('DecVCPlat tokens unstaked successfully');
        setDecVCPlatUnstakeDialogOpen(false);
        setDecVCPlatUnstakeAmount('');
      }
    } catch (decvcplatError) {
      toast.error('Failed to unstake DecVCPlat tokens');
    }
  };

  const handleDecVCPlatNetworkSwitch = async (decvcplatNetworkId: number) => {
    try {
      const decvcplatResult = await decvcplatDispatch(switchDecVCPlatNetwork(decvcplatNetworkId));
      if (switchDecVCPlatNetwork.fulfilled.match(decvcplatResult)) {
        toast.success('DecVCPlat network switched successfully');
      }
    } catch (decvcplatError) {
      toast.error('Failed to switch DecVCPlat network');
    }
  };

  const formatDecVCPlatAddress = (decvcplatAddress: string): string => {
    return `${decvcplatAddress.slice(0, 6)}...${decvcplatAddress.slice(-4)}`;
  };

  const formatDecVCPlatBalance = (decvcplatBalance: number): string => {
    return decvcplatBalance.toLocaleString(undefined, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 6,
    });
  };

  const getDecVCPlatTransactionIcon = (decvcplatTxType: string) => {
    switch (decvcplatTxType) {
      case 'Stake': return <Security color="primary" />;
      case 'Unstake': return <CallReceived color="secondary" />;
      case 'Vote': return <TrendingUp color="success" />;
      case 'Transfer': return <Send color="info" />;
      default: return <SwapHoriz color="action" />;
    }
  };

  if (decvcplatLoading) {
    return <LoadingSpinner message="Loading DecVCPlat wallet..." />;
  }

  return (
    <>
      <Helmet>
        <title>DecVCPlat Wallet - Manage Your Assets</title>
        <meta name="description" content="Manage your DecVCPlat tokens, stake for governance, and view transaction history on the decentralized platform." />
      </Helmet>

      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" fontWeight={600} gutterBottom>
            DecVCPlat Wallet
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage your DecVCPlat tokens, participate in governance, and track your blockchain activity
          </Typography>
        </Box>

        {!decvcplatWalletConnected ? (
          <Paper elevation={1} sx={{ p: 6, textAlign: 'center' }}>
            <AccountBalanceWallet sx={{ fontSize: 64, color: 'primary.main', mb: 2 }} />
            <Typography variant="h5" fontWeight={600} gutterBottom>
              Connect Your DecVCPlat Wallet
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
              Connect your MetaMask wallet to manage DecVCPlat tokens and participate in governance
            </Typography>
            <Button
              variant="contained"
              size="large"
              startIcon={<AccountBalanceWallet />}
              onClick={handleDecVCPlatWalletConnect}
              sx={{
                px: 4,
                py: 1.5,
                background: decvcplatTheme.custom.gradients.primary,
                '&:hover': { opacity: 0.9 },
              }}
            >
              Connect DecVCPlat Wallet
            </Button>
          </Paper>
        ) : (
          <Grid container spacing={3}>
            {/* DecVCPlat Wallet Overview */}
            <Grid item xs={12} md={8}>
              <Card elevation={1} sx={{ mb: 3 }}>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
                    <Box>
                      <Typography variant="h6" fontWeight={600} gutterBottom>
                        DecVCPlat Wallet Overview
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {formatDecVCPlatAddress(decvcplatWalletAddress)} • {decvcplatNetworkInfo.decvcplatNetworkName}
                      </Typography>
                    </Box>
                    <Button
                      variant="outlined"
                      startIcon={<Refresh />}
                      onClick={() => decvcplatDispatch(fetchDecVCPlatTransactionHistory())}
                      size="small"
                    >
                      Refresh
                    </Button>
                  </Box>

                  <Grid container spacing={3}>
                    <Grid item xs={12} sm={4}>
                      <Paper elevation={0} sx={{ p: 3, bgcolor: 'primary.light', color: 'primary.contrastText' }}>
                        <Typography variant="body2" fontWeight={600}>
                          DVCP Balance
                        </Typography>
                        <Typography variant="h4" fontWeight={700}>
                          {formatDecVCPlatBalance(decvcplatTokenBalance)}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={12} sm={4}>
                      <Paper elevation={0} sx={{ p: 3, bgcolor: 'success.light', color: 'success.contrastText' }}>
                        <Typography variant="body2" fontWeight={600}>
                          Staked DVCP
                        </Typography>
                        <Typography variant="h4" fontWeight={700}>
                          {formatDecVCPlatBalance(decvcplatStakedBalance)}
                        </Typography>
                      </Paper>
                    </Grid>
                    <Grid item xs={12} sm={4}>
                      <Paper elevation={0} sx={{ p: 3, bgcolor: 'secondary.light', color: 'secondary.contrastText' }}>
                        <Typography variant="body2" fontWeight={600}>
                          ETH Balance
                        </Typography>
                        <Typography variant="h4" fontWeight={700}>
                          {formatDecVCPlatBalance(decvcplatEthBalance)}
                        </Typography>
                      </Paper>
                    </Grid>
                  </Grid>

                  <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
                    <Button
                      variant="contained"
                      startIcon={<Security />}
                      onClick={() => setDecVCPlatStakeDialogOpen(true)}
                      disabled={decvcplatTokenBalance === 0}
                    >
                      Stake DecVCPlat Tokens
                    </Button>
                    <Button
                      variant="outlined"
                      startIcon={<CallReceived />}
                      onClick={() => setDecVCPlatUnstakeDialogOpen(true)}
                      disabled={decvcplatStakedBalance === 0}
                    >
                      Unstake Tokens
                    </Button>
                  </Box>
                </CardContent>
              </Card>

              {/* DecVCPlat Transaction History */}
              <Card elevation={1}>
                <CardContent>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    DecVCPlat Transaction History
                  </Typography>
                  {decvcplatTransactionHistory.length === 0 ? (
                    <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                      No DecVCPlat transactions found
                    </Typography>
                  ) : (
                    <List sx={{ p: 0 }}>
                      {decvcplatTransactionHistory.slice(0, 10).map((decvcplatTx, decvcplatIndex) => (
                        <React.Fragment key={decvcplatTx.decvcplatTransactionHash}>
                          <ListItem sx={{ px: 0 }}>
                            <ListItemAvatar>
                              <Avatar sx={{ bgcolor: 'primary.light' }}>
                                {getDecVCPlatTransactionIcon(decvcplatTx.decvcplatTransactionType)}
                              </Avatar>
                            </ListItemAvatar>
                            <ListItemText
                              primary={`${decvcplatTx.decvcplatTransactionType}: ${formatDecVCPlatBalance(decvcplatTx.decvcplatAmount)} DVCP`}
                              secondary={`${new Date(decvcplatTx.decvcplatTimestamp).toLocaleString()} • ${formatDecVCPlatAddress(decvcplatTx.decvcplatTransactionHash)}`}
                            />
                            <Chip
                              label={decvcplatTx.decvcplatStatus}
                              color={decvcplatTx.decvcplatStatus === 'Confirmed' ? 'success' : 'warning'}
                              size="small"
                            />
                          </ListItem>
                          {decvcplatIndex < decvcplatTransactionHistory.length - 1 && <Divider />}
                        </React.Fragment>
                      ))}
                    </List>
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* DecVCPlat Wallet Actions */}
            <Grid item xs={12} md={4}>
              <Card elevation={1} sx={{ mb: 3 }}>
                <CardContent>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    DecVCPlat Network
                  </Typography>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Current: {decvcplatNetworkInfo.decvcplatNetworkName}
                  </Typography>
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mt: 2 }}>
                    <Button
                      variant={decvcplatNetworkInfo.decvcplatNetworkId === 1 ? 'contained' : 'outlined'}
                      size="small"
                      onClick={() => handleDecVCPlatNetworkSwitch(1)}
                      disabled={decvcplatNetworkInfo.decvcplatNetworkId === 1}
                    >
                      Ethereum Mainnet
                    </Button>
                    <Button
                      variant={decvcplatNetworkInfo.decvcplatNetworkId === 11155111 ? 'contained' : 'outlined'}
                      size="small"
                      onClick={() => handleDecVCPlatNetworkSwitch(11155111)}
                      disabled={decvcplatNetworkInfo.decvcplatNetworkId === 11155111}
                    >
                      Sepolia Testnet
                    </Button>
                  </Box>
                </CardContent>
              </Card>

              <Card elevation={1}>
                <CardContent>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    DecVCPlat Wallet Actions
                  </Typography>
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    <Button
                      variant="outlined"
                      fullWidth
                      startIcon={<AccountBalanceWallet />}
                      onClick={handleDecVCPlatWalletDisconnect}
                    >
                      Disconnect Wallet
                    </Button>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        )}

        {/* DecVCPlat Stake Dialog */}
        <Dialog open={decvcplatStakeDialogOpen} onClose={() => setDecVCPlatStakeDialogOpen(false)} maxWidth="sm" fullWidth>
          <DialogTitle>Stake DecVCPlat Tokens</DialogTitle>
          <DialogContent>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Available Balance: {formatDecVCPlatBalance(decvcplatTokenBalance)} DVCP
            </Typography>
            <TextField
              fullWidth
              label="Amount to Stake"
              type="number"
              value={decvcplatStakeAmount}
              onChange={(e) => setDecVCPlatStakeAmount(e.target.value)}
              inputProps={{ min: 0, max: decvcplatTokenBalance }}
              sx={{ mt: 2 }}
            />
            <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
              Staked tokens earn rewards and provide voting power in DecVCPlat governance
            </Typography>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDecVCPlatStakeDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleDecVCPlatTokenStake} variant="contained">
              Stake DecVCPlat Tokens
            </Button>
          </DialogActions>
        </Dialog>

        {/* DecVCPlat Unstake Dialog */}
        <Dialog open={decvcplatUnstakeDialogOpen} onClose={() => setDecVCPlatUnstakeDialogOpen(false)} maxWidth="sm" fullWidth>
          <DialogTitle>Unstake DecVCPlat Tokens</DialogTitle>
          <DialogContent>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Staked Balance: {formatDecVCPlatBalance(decvcplatStakedBalance)} DVCP
            </Typography>
            <TextField
              fullWidth
              label="Amount to Unstake"
              type="number"
              value={decvcplatUnstakeAmount}
              onChange={(e) => setDecVCPlatUnstakeAmount(e.target.value)}
              inputProps={{ min: 0, max: decvcplatStakedBalance }}
              sx={{ mt: 2 }}
            />
            <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
              Unstaking may have a cooldown period before tokens are available
            </Typography>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDecVCPlatUnstakeDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleDecVCPlatTokenUnstake} variant="contained">
              Unstake DecVCPlat Tokens
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </>
  );
};

export default WalletPage;
