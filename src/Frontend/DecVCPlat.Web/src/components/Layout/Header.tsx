// © 2024 DecVCPlat. All rights reserved.

import React, { useState } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Box,
  Avatar,
  Menu,
  MenuItem,
  Badge,
  useTheme,
  useMediaQuery,
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
} from '@mui/material';
import {
  Menu as MenuIcon,
  AccountCircle,
  Notifications,
  DarkMode,
  LightMode,
  Dashboard,
  Work,
  HowToVote,
  AccountBalanceWallet,
  Settings,
  Logout,
  Home,
} from '@mui/icons-material';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { logout } from '../../store/slices/authSlice';
import { toggleDarkMode } from '../../store/slices/themeSlice';

const Header: React.FC = () => {
  const theme = useTheme();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  
  const { user, isAuthenticated } = useAppSelector(state => state.auth);
  const { isDarkMode } = useAppSelector(state => state.theme);
  
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [notificationCount] = useState(3); // Mock notification count

  const handleProfileMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    dispatch(logout());
    handleMenuClose();
    navigate('/');
  };

  const handleThemeToggle = () => {
    dispatch(toggleDarkMode());
  };

  const toggleMobileMenu = () => {
    setMobileMenuOpen(!mobileMenuOpen);
  };

  const navigationItems = [
    { label: 'Home', path: '/', icon: <Home />, public: true },
    { label: 'Dashboard', path: '/dashboard', icon: <Dashboard />, roles: ['Founder', 'Investor', 'Luminary'] },
    { label: 'Projects', path: '/projects', icon: <Work />, roles: ['Founder', 'Investor', 'Luminary'] },
    { label: 'Voting', path: '/voting', icon: <HowToVote />, roles: ['Investor', 'Luminary'] },
  ];

  const filteredNavItems = navigationItems.filter(item => 
    item.public || (isAuthenticated && (!item.roles || item.roles.includes(user?.role || '')))
  );

  const mobileMenu = (
    <Drawer
      anchor="left"
      open={mobileMenuOpen}
      onClose={toggleMobileMenu}
      sx={{
        '& .MuiDrawer-paper': {
          width: 280,
          bgcolor: 'background.paper',
        },
      }}
    >
      <Box sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box
          sx={{
            width: 40,
            height: 40,
            borderRadius: 2,
            background: theme.custom.gradients.primary,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'white',
            fontWeight: 'bold',
            fontSize: '1.2rem',
          }}
        >
          D
        </Box>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          DecVCPlat
        </Typography>
      </Box>
      
      <Divider />
      
      <List>
        {filteredNavItems.map((item) => (
          <ListItem
            button
            key={item.path}
            component={Link}
            to={item.path}
            onClick={toggleMobileMenu}
          >
            <ListItemIcon>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} />
          </ListItem>
        ))}
        
        {isAuthenticated && (
          <>
            <Divider sx={{ my: 1 }} />
            <ListItem button onClick={handleThemeToggle}>
              <ListItemIcon>
                {isDarkMode ? <LightMode /> : <DarkMode />}
              </ListItemIcon>
              <ListItemText primary={isDarkMode ? 'Light Mode' : 'Dark Mode'} />
            </ListItem>
            <ListItem button component={Link} to="/profile" onClick={toggleMobileMenu}>
              <ListItemIcon><Settings /></ListItemIcon>
              <ListItemText primary="Profile" />
            </ListItem>
            <ListItem button onClick={() => { handleLogout(); toggleMobileMenu(); }}>
              <ListItemIcon><Logout /></ListItemIcon>
              <ListItemText primary="Logout" />
            </ListItem>
          </>
        )}
      </List>
    </Drawer>
  );

  return (
    <>
      <AppBar 
        position="fixed" 
        elevation={1}
        sx={{ 
          bgcolor: 'background.paper',
          color: 'text.primary',
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <Toolbar>
          {/* Mobile Menu Button */}
          {isMobile && (
            <IconButton
              edge="start"
              color="inherit"
              aria-label="menu"
              onClick={toggleMobileMenu}
              sx={{ mr: 2 }}
            >
              <MenuIcon />
            </IconButton>
          )}

          {/* Logo */}
          <Box
            component={Link}
            to="/"
            sx={{
              display: 'flex',
              alignItems: 'center',
              textDecoration: 'none',
              color: 'inherit',
            }}
          >
            <Box
              sx={{
                width: 40,
                height: 40,
                borderRadius: 2,
                background: theme.custom.gradients.primary,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'white',
                fontWeight: 'bold',
                fontSize: '1.2rem',
                mr: 1,
              }}
            >
              D
            </Box>
            <Typography
              variant="h6"
              sx={{
                fontWeight: 600,
                display: { xs: 'none', sm: 'block' },
              }}
            >
              DecVCPlat
            </Typography>
          </Box>

          {/* Desktop Navigation */}
          {!isMobile && (
            <Box sx={{ flexGrow: 1, display: 'flex', ml: 4, gap: 1 }}>
              {filteredNavItems.map((item) => (
                <Button
                  key={item.path}
                  component={Link}
                  to={item.path}
                  color="inherit"
                  sx={{
                    textTransform: 'none',
                    fontWeight: 500,
                    px: 2,
                    '&:hover': {
                      bgcolor: 'action.hover',
                    },
                  }}
                >
                  {item.label}
                </Button>
              ))}
            </Box>
          )}

          <Box sx={{ flexGrow: 1 }} />

          {/* Right side actions */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {/* Theme Toggle */}
            <IconButton color="inherit" onClick={handleThemeToggle}>
              {isDarkMode ? <LightMode /> : <DarkMode />}
            </IconButton>

            {isAuthenticated ? (
              <>
                {/* Notifications */}
                <IconButton
                  color="inherit"
                  component={Link}
                  to="/notifications"
                >
                  <Badge badgeContent={notificationCount} color="error">
                    <Notifications />
                  </Badge>
                </IconButton>

                {/* Wallet */}
                {user?.walletAddress && (
                  <IconButton color="inherit">
                    <AccountBalanceWallet />
                  </IconButton>
                )}

                {/* Profile Menu */}
                <IconButton
                  edge="end"
                  aria-label="account of current user"
                  aria-controls="profile-menu"
                  aria-haspopup="true"
                  onClick={handleProfileMenuOpen}
                  color="inherit"
                >
                  {user?.profilePictureUrl ? (
                    <Avatar
                      src={user.profilePictureUrl}
                      alt={user.fullName}
                      sx={{ width: 32, height: 32 }}
                    />
                  ) : (
                    <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
                      {user?.fullName?.[0]?.toUpperCase() || 'U'}
                    </Avatar>
                  )}
                </IconButton>

                {/* Profile Dropdown Menu */}
                <Menu
                  id="profile-menu"
                  anchorEl={anchorEl}
                  keepMounted
                  open={Boolean(anchorEl)}
                  onClose={handleMenuClose}
                  transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                  anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
                >
                  <MenuItem onClick={handleMenuClose}>
                    <Box>
                      <Typography variant="body1" fontWeight={500}>
                        {user?.fullName}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {user?.email}
                      </Typography>
                      <Typography
                        variant="caption"
                        sx={{
                          px: 1,
                          py: 0.5,
                          bgcolor: 'primary.main',
                          color: 'primary.contrastText',
                          borderRadius: 1,
                          fontSize: '0.75rem',
                        }}
                      >
                        {user?.role}
                      </Typography>
                    </Box>
                  </MenuItem>
                  <Divider />
                  <MenuItem component={Link} to="/dashboard" onClick={handleMenuClose}>
                    <Dashboard sx={{ mr: 1 }} /> Dashboard
                  </MenuItem>
                  <MenuItem component={Link} to="/profile" onClick={handleMenuClose}>
                    <Settings sx={{ mr: 1 }} /> Profile
                  </MenuItem>
                  <Divider />
                  <MenuItem onClick={handleLogout}>
                    <Logout sx={{ mr: 1 }} /> Logout
                  </MenuItem>
                </Menu>
              </>
            ) : (
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  component={Link}
                  to="/login"
                  color="inherit"
                  sx={{ textTransform: 'none' }}
                >
                  Login
                </Button>
                <Button
                  component={Link}
                  to="/register"
                  variant="contained"
                  sx={{ textTransform: 'none' }}
                >
                  Sign Up
                </Button>
              </Box>
            )}
          </Box>
        </Toolbar>
      </AppBar>

      {/* Mobile Drawer */}
      {mobileMenu}
    </>
  );
};

export default Header;
