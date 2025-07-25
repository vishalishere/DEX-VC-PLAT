import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  IconButton,
  Fab,
  useTheme,
  styled,
  AppBar,
  Toolbar,
  BottomNavigation,
  BottomNavigationAction,
  SwipeableDrawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider
} from '@mui/material';
import {
  Add,
  Home,
  AccountBalanceWallet,
  HowToVote,
  Notifications,
  Business,
  Menu,
  Person,
  Settings,
  ExitToApp
} from '@mui/icons-material';

// Android Material Design 3 styled components
const AndroidDecVCPlatCard = styled(Card)(({ theme }) => ({
  borderRadius: theme.spacing(2),
  elevation: 2,
  margin: theme.spacing(1),
  backgroundColor: theme.palette.mode === 'dark' ? '#2d3748' : '#ffffff',
  border: `1px solid ${theme.palette.mode === 'dark' ? '#4a5568' : '#e2e8f0'}`,
  '&:hover': {
    elevation: 4,
    transform: 'translateY(-2px)',
    transition: 'all 0.2s ease-in-out'
  }
}));

const AndroidDecVCPlatFAB = styled(Fab)(({ theme }) => ({
  position: 'fixed',
  bottom: theme.spacing(10),
  right: theme.spacing(2),
  backgroundColor: theme.palette.primary.main,
  color: theme.palette.primary.contrastText,
  '&:hover': {
    backgroundColor: theme.palette.primary.dark,
    transform: 'scale(1.1)',
    transition: 'all 0.2s ease-in-out'
  }
}));

const AndroidDecVCPlatAppBar = styled(AppBar)(({ theme }) => ({
  backgroundColor: theme.palette.mode === 'dark' ? '#1a202c' : theme.palette.primary.main,
  boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
  borderBottom: `1px solid ${theme.palette.mode === 'dark' ? '#4a5568' : 'transparent'}`
}));

const AndroidDecVCPlatBottomNav = styled(BottomNavigation)(({ theme }) => ({
  position: 'fixed',
  bottom: 0,
  left: 0,
  right: 0,
  backgroundColor: theme.palette.mode === 'dark' ? '#2d3748' : '#ffffff',
  borderTop: `1px solid ${theme.palette.mode === 'dark' ? '#4a5568' : '#e2e8f0'}`,
  zIndex: 1000,
  '& .Mui-selected': {
    color: theme.palette.primary.main,
    '& .MuiBottomNavigationAction-label': {
      fontSize: '0.75rem',
      fontWeight: 600
    }
  }
}));

interface AndroidDecVCPlatMobileHeaderProps {
  title: string;
  onMenuClick: () => void;
  showBackButton?: boolean;
  onBackClick?: () => void;
}

export const AndroidDecVCPlatMobileHeader: React.FC<AndroidDecVCPlatMobileHeaderProps> = ({
  title,
  onMenuClick,
  showBackButton = false,
  onBackClick
}) => {
  return (
    <AndroidDecVCPlatAppBar position="sticky">
      <Toolbar>
        <IconButton
          edge="start"
          color="inherit"
          aria-label={showBackButton ? 'back' : 'menu'}
          onClick={showBackButton ? onBackClick : onMenuClick}
        >
          {showBackButton ? <ExitToApp style={{ transform: 'rotate(180deg)' }} /> : <Menu />}
        </IconButton>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 600 }}>
          {title}
        </Typography>
      </Toolbar>
    </AndroidDecVCPlatAppBar>
  );
};

interface AndroidDecVCPlatProjectCardProps {
  project: {
    id: string;
    title: string;
    description: string;
    funding: number;
    goal: number;
    status: string;
  };
  onPress: (projectId: string) => void;
}

export const AndroidDecVCPlatProjectCard: React.FC<AndroidDecVCPlatProjectCardProps> = ({
  project,
  onPress
}) => {
  const theme = useTheme();
  const progress = (project.funding / project.goal) * 100;

  return (
    <AndroidDecVCPlatCard onClick={() => onPress(project.id)}>
      <CardContent>
        <Typography variant="h6" component="div" gutterBottom fontWeight={600}>
          {project.title}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {project.description}
        </Typography>
        <Box sx={{ mb: 2 }}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
            <Typography variant="body2" fontWeight={500}>
              ${project.funding.toLocaleString()} raised
            </Typography>
            <Typography variant="body2" color="text.secondary">
              of ${project.goal.toLocaleString()}
            </Typography>
          </Box>
          <Box
            sx={{
              width: '100%',
              height: 8,
              backgroundColor: theme.palette.mode === 'dark' ? '#4a5568' : '#e2e8f0',
              borderRadius: 4,
              overflow: 'hidden'
            }}
          >
            <Box
              sx={{
                width: `${Math.min(progress, 100)}%`,
                height: '100%',
                backgroundColor: theme.palette.success.main,
                borderRadius: 4,
                transition: 'width 0.5s ease-in-out'
              }}
            />
          </Box>
        </Box>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography
            variant="caption"
            sx={{
              px: 1,
              py: 0.5,
              borderRadius: 1,
              backgroundColor: theme.palette.mode === 'dark' ? '#4a5568' : '#f7fafc',
              color: theme.palette.mode === 'dark' ? '#e2e8f0' : '#2d3748',
              fontWeight: 500
            }}
          >
            {project.status}
          </Typography>
          <Button size="small" variant="outlined" color="primary">
            View Details
          </Button>
        </Box>
      </CardContent>
    </AndroidDecVCPlatCard>
  );
};

interface AndroidDecVCPlatBottomNavigationProps {
  currentTab: number;
  onTabChange: (newValue: number) => void;
}

export const AndroidDecVCPlatBottomNavigation: React.FC<AndroidDecVCPlatBottomNavigationProps> = ({
  currentTab,
  onTabChange
}) => {
  return (
    <AndroidDecVCPlatBottomNav
      value={currentTab}
      onChange={(event, newValue) => onTabChange(newValue)}
      showLabels
    >
      <BottomNavigationAction
        label="Home"
        icon={<Home />}
      />
      <BottomNavigationAction
        label="Projects"
        icon={<Business />}
      />
      <BottomNavigationAction
        label="Voting"
        icon={<HowToVote />}
      />
      <BottomNavigationAction
        label="Wallet"
        icon={<AccountBalanceWallet />}
      />
      <BottomNavigationAction
        label="Notifications"
        icon={<Notifications />}
      />
    </AndroidDecVCPlatBottomNav>
  );
};

interface AndroidDecVCPlatNavigationDrawerProps {
  open: boolean;
  onClose: () => void;
  user?: {
    firstName: string;
    lastName: string;
    email: string;
    role: string;
  };
}

export const AndroidDecVCPlatNavigationDrawer: React.FC<AndroidDecVCPlatNavigationDrawerProps> = ({
  open,
  onClose,
  user
}) => {
  const theme = useTheme();

  const menuItems = [
    { label: 'Dashboard', icon: <Home />, path: '/dashboard' },
    { label: 'Projects', icon: <Business />, path: '/projects' },
    { label: 'Voting', icon: <HowToVote />, path: '/voting' },
    { label: 'Wallet', icon: <AccountBalanceWallet />, path: '/wallet' },
    { label: 'Profile', icon: <Person />, path: '/profile' },
    { label: 'Settings', icon: <Settings />, path: '/settings' }
  ];

  return (
    <SwipeableDrawer
      anchor="left"
      open={open}
      onClose={onClose}
      onOpen={() => {}}
      PaperProps={{
        sx: {
          width: 280,
          backgroundColor: theme.palette.mode === 'dark' ? '#2d3748' : '#ffffff'
        }
      }}
    >
      <Box sx={{ p: 3, backgroundColor: theme.palette.primary.main, color: 'white' }}>
        <Typography variant="h6" fontWeight={600}>
          DecVCPlat
        </Typography>
        {user && (
          <Box mt={2}>
            <Typography variant="body2" sx={{ opacity: 0.9 }}>
              {user.firstName} {user.lastName}
            </Typography>
            <Typography variant="caption" sx={{ opacity: 0.7 }}>
              {user.role} • {user.email}
            </Typography>
          </Box>
        )}
      </Box>
      <List>
        {menuItems.map((item, index) => (
          <ListItem 
            button 
            key={index}
            onClick={() => {
              window.location.href = item.path;
              onClose();
            }}
            sx={{
              '&:hover': {
                backgroundColor: theme.palette.mode === 'dark' ? '#4a5568' : '#f7fafc'
              }
            }}
          >
            <ListItemIcon sx={{ color: theme.palette.primary.main }}>
              {item.icon}
            </ListItemIcon>
            <ListItemText 
              primary={item.label}
              primaryTypographyProps={{ fontWeight: 500 }}
            />
          </ListItem>
        ))}
        <Divider sx={{ my: 1 }} />
        <ListItem 
          button
          onClick={() => {
            // Handle logout
            onClose();
          }}
          sx={{
            '&:hover': {
              backgroundColor: theme.palette.error.light,
              '& .MuiListItemIcon-root': {
                color: theme.palette.error.main
              }
            }
          }}
        >
          <ListItemIcon>
            <ExitToApp />
          </ListItemIcon>
          <ListItemText 
            primary="Logout"
            primaryTypographyProps={{ fontWeight: 500 }}
          />
        </ListItem>
      </List>
    </SwipeableDrawer>
  );
};

export const AndroidDecVCPlatCreateFAB: React.FC<{ onClick: () => void }> = ({ onClick }) => {
  return (
    <AndroidDecVCPlatFAB onClick={onClick} aria-label="create">
      <Add />
    </AndroidDecVCPlatFAB>
  );
};

// Android-specific layout wrapper
interface AndroidDecVCPlatLayoutProps {
  children: React.ReactNode;
  title: string;
  showFAB?: boolean;
  onFABClick?: () => void;
  currentTab?: number;
  onTabChange?: (newValue: number) => void;
}

export const AndroidDecVCPlatLayout: React.FC<AndroidDecVCPlatLayoutProps> = ({
  children,
  title,
  showFAB = false,
  onFABClick,
  currentTab = 0,
  onTabChange
}) => {
  const [drawerOpen, setDrawerOpen] = React.useState(false);

  return (
    <Box sx={{ pb: 8 }}>
      <AndroidDecVCPlatMobileHeader
        title={title}
        onMenuClick={() => setDrawerOpen(true)}
      />
      
      <AndroidDecVCPlatNavigationDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
      />
      
      <Box sx={{ p: 2, minHeight: 'calc(100vh - 120px)' }}>
        {children}
      </Box>
      
      {showFAB && onFABClick && (
        <AndroidDecVCPlatCreateFAB onClick={onFABClick} />
      )}
      
      {onTabChange && (
        <AndroidDecVCPlatBottomNavigation
          currentTab={currentTab}
          onTabChange={onTabChange}
        />
      )}
    </Box>
  );
};

export default AndroidDecVCPlatLayout;
