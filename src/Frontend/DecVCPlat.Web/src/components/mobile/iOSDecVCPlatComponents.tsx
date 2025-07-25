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
  Divider,
  Avatar,
  Chip
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
  ExitToApp,
  ChevronRight,
  ArrowBackIos
} from '@mui/icons-material';

// iOS-specific styled components with native design patterns
const iOSDecVCPlatCard = styled(Card)(({ theme }) => ({
  borderRadius: theme.spacing(1.5),
  margin: theme.spacing(1),
  backgroundColor: theme.palette.mode === 'dark' ? '#1c1c1e' : '#ffffff',
  border: 'none',
  boxShadow: theme.palette.mode === 'dark' 
    ? '0 1px 3px rgba(0,0,0,0.3)' 
    : '0 1px 3px rgba(0,0,0,0.1)',
  '&:active': {
    transform: 'scale(0.98)',
    transition: 'all 0.1s ease-in-out'
  }
}));

const iOSDecVCPlatFAB = styled(Fab)(({ theme }) => ({
  position: 'fixed',
  bottom: theme.spacing(12),
  right: theme.spacing(2),
  backgroundColor: theme.palette.primary.main,
  color: theme.palette.primary.contrastText,
  boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
  '&:active': {
    transform: 'scale(0.95)',
    transition: 'all 0.1s ease-in-out'
  }
}));

const iOSDecVCPlatAppBar = styled(AppBar)(({ theme }) => ({
  backgroundColor: theme.palette.mode === 'dark' ? '#000000' : '#f2f2f7',
  color: theme.palette.mode === 'dark' ? '#ffffff' : '#000000',
  boxShadow: 'none',
  borderBottom: `0.5px solid ${theme.palette.mode === 'dark' ? '#38383a' : '#c6c6c8'}`,
  '& .MuiToolbar-root': {
    minHeight: 44,
    paddingLeft: theme.spacing(2),
    paddingRight: theme.spacing(2)
  }
}));

const iOSDecVCPlatBottomNav = styled(BottomNavigation)(({ theme }) => ({
  position: 'fixed',
  bottom: 0,
  left: 0,
  right: 0,
  backgroundColor: theme.palette.mode === 'dark' ? '#000000' : '#f2f2f7',
  borderTop: `0.5px solid ${theme.palette.mode === 'dark' ? '#38383a' : '#c6c6c8'}`,
  zIndex: 1000,
  height: 83, // iOS safe area height
  paddingBottom: 20,
  '& .MuiBottomNavigationAction-root': {
    minWidth: 'auto',
    '&.Mui-selected': {
      color: theme.palette.primary.main,
      '& .MuiBottomNavigationAction-label': {
        fontSize: '10px',
        fontWeight: 600
      }
    },
    '& .MuiBottomNavigationAction-label': {
      fontSize: '10px',
      fontWeight: 400
    }
  }
}));

const iOSDecVCPlatListItem = styled(ListItem)(({ theme }) => ({
  backgroundColor: theme.palette.mode === 'dark' ? '#1c1c1e' : '#ffffff',
  borderBottom: `0.5px solid ${theme.palette.mode === 'dark' ? '#38383a' : '#c6c6c8'}`,
  padding: theme.spacing(2),
  '&:active': {
    backgroundColor: theme.palette.mode === 'dark' ? '#2c2c2e' : '#f2f2f7',
    transition: 'all 0.1s ease-in-out'
  },
  '&:last-child': {
    borderBottom: 'none'
  }
}));

interface iOSDecVCPlatMobileHeaderProps {
  title: string;
  onMenuClick: () => void;
  showBackButton?: boolean;
  onBackClick?: () => void;
  rightAction?: React.ReactNode;
}

export const iOSDecVCPlatMobileHeader: React.FC<iOSDecVCPlatMobileHeaderProps> = ({
  title,
  onMenuClick,
  showBackButton = false,
  onBackClick,
  rightAction
}) => {
  return (
    <iOSDecVCPlatAppBar position="sticky">
      <Toolbar>
        <Box display="flex" alignItems="center" width="100%">
          <Box width="60px">
            {showBackButton ? (
              <IconButton
                edge="start"
                color="inherit"
                aria-label="back"
                onClick={onBackClick}
                size="small"
              >
                <ArrowBackIos fontSize="small" />
              </IconButton>
            ) : (
              <IconButton
                edge="start"
                color="inherit"
                aria-label="menu"
                onClick={onMenuClick}
                size="small"
              >
                <Menu fontSize="small" />
              </IconButton>
            )}
          </Box>
          <Box flexGrow={1} textAlign="center">
            <Typography 
              variant="h6" 
              component="div" 
              sx={{ 
                fontWeight: 600,
                fontSize: '17px',
                letterSpacing: '-0.4px'
              }}
            >
              {title}
            </Typography>
          </Box>
          <Box width="60px" display="flex" justifyContent="flex-end">
            {rightAction}
          </Box>
        </Box>
      </Toolbar>
    </iOSDecVCPlatAppBar>
  );
};

interface iOSDecVCPlatProjectCardProps {
  project: {
    id: string;
    title: string;
    description: string;
    funding: number;
    goal: number;
    status: string;
    category?: string;
  };
  onPress: (projectId: string) => void;
}

export const iOSDecVCPlatProjectCard: React.FC<iOSDecVCPlatProjectCardProps> = ({
  project,
  onPress
}) => {
  const theme = useTheme();
  const progress = (project.funding / project.goal) * 100;

  return (
    <iOSDecVCPlatCard onClick={() => onPress(project.id)}>
      <CardContent sx={{ padding: '16px !important' }}>
        <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
          <Typography 
            variant="h6" 
            component="div" 
            fontWeight={600}
            fontSize="17px"
            letterSpacing="-0.4px"
          >
            {project.title}
          </Typography>
          <ChevronRight 
            sx={{ 
              color: theme.palette.mode === 'dark' ? '#8e8e93' : '#c7c7cc',
              fontSize: '16px'
            }} 
          />
        </Box>
        
        {project.category && (
          <Chip
            label={project.category}
            size="small"
            sx={{
              backgroundColor: theme.palette.mode === 'dark' ? '#2c2c2e' : '#f2f2f7',
              color: theme.palette.primary.main,
              fontSize: '12px',
              height: '24px',
              mb: 1
            }}
          />
        )}
        
        <Typography 
          variant="body2" 
          color="text.secondary" 
          sx={{ 
            mb: 2,
            fontSize: '15px',
            lineHeight: 1.3,
            color: theme.palette.mode === 'dark' ? '#8e8e93' : '#8e8e93'
          }}
        >
          {project.description}
        </Typography>
        
        <Box sx={{ mb: 2 }}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
            <Typography 
              variant="body2" 
              fontWeight={600}
              fontSize="15px"
              color={theme.palette.mode === 'dark' ? '#ffffff' : '#000000'}
            >
              ${project.funding.toLocaleString()}
            </Typography>
            <Typography 
              variant="body2" 
              fontSize="13px"
              color={theme.palette.mode === 'dark' ? '#8e8e93' : '#8e8e93'}
            >
              of ${project.goal.toLocaleString()}
            </Typography>
          </Box>
          
          {/* iOS-style progress bar */}
          <Box
            sx={{
              width: '100%',
              height: 4,
              backgroundColor: theme.palette.mode === 'dark' ? '#2c2c2e' : '#e5e5ea',
              borderRadius: 2,
              overflow: 'hidden'
            }}
          >
            <Box
              sx={{
                width: `${Math.min(progress, 100)}%`,
                height: '100%',
                backgroundColor: theme.palette.primary.main,
                borderRadius: 2,
                transition: 'width 0.3s ease-out'
              }}
            />
          </Box>
        </Box>
        
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography
            variant="caption"
            sx={{
              px: 1.5,
              py: 0.5,
              borderRadius: 1,
              backgroundColor: theme.palette.mode === 'dark' ? '#2c2c2e' : '#f2f2f7',
              color: theme.palette.mode === 'dark' ? '#ffffff' : '#000000',
              fontWeight: 500,
              fontSize: '12px'
            }}
          >
            {project.status}
          </Typography>
          <Typography 
            variant="body2" 
            color="primary" 
            fontWeight={400}
            fontSize="15px"
          >
            {Math.round(progress)}%
          </Typography>
        </Box>
      </CardContent>
    </iOSDecVCPlatCard>
  );
};

interface iOSDecVCPlatBottomNavigationProps {
  currentTab: number;
  onTabChange: (newValue: number) => void;
}

export const iOSDecVCPlatBottomNavigation: React.FC<iOSDecVCPlatBottomNavigationProps> = ({
  currentTab,
  onTabChange
}) => {
  return (
    <iOSDecVCPlatBottomNav
      value={currentTab}
      onChange={(event, newValue) => onTabChange(newValue)}
      showLabels
    >
      <BottomNavigationAction
        label="Home"
        icon={<Home fontSize="small" />}
      />
      <BottomNavigationAction
        label="Projects"
        icon={<Business fontSize="small" />}
      />
      <BottomNavigationAction
        label="Vote"
        icon={<HowToVote fontSize="small" />}
      />
      <BottomNavigationAction
        label="Wallet"
        icon={<AccountBalanceWallet fontSize="small" />}
      />
      <BottomNavigationAction
        label="Alerts"
        icon={<Notifications fontSize="small" />}
      />
    </iOSDecVCPlatBottomNav>
  );
};

interface iOSDecVCPlatNavigationDrawerProps {
  open: boolean;
  onClose: () => void;
  user?: {
    firstName: string;
    lastName: string;
    email: string;
    role: string;
    avatar?: string;
  };
}

export const iOSDecVCPlatNavigationDrawer: React.FC<iOSDecVCPlatNavigationDrawerProps> = ({
  open,
  onClose,
  user
}) => {
  const theme = useTheme();

  const menuItems = [
    { label: 'Dashboard', icon: <Home fontSize="small" />, path: '/dashboard' },
    { label: 'Projects', icon: <Business fontSize="small" />, path: '/projects' },
    { label: 'Voting', icon: <HowToVote fontSize="small" />, path: '/voting' },
    { label: 'Wallet', icon: <AccountBalanceWallet fontSize="small" />, path: '/wallet' },
    { label: 'Profile', icon: <Person fontSize="small" />, path: '/profile' },
    { label: 'Settings', icon: <Settings fontSize="small" />, path: '/settings' }
  ];

  return (
    <SwipeableDrawer
      anchor="left"
      open={open}
      onClose={onClose}
      onOpen={() => {}}
      PaperProps={{
        sx: {
          width: 320,
          backgroundColor: theme.palette.mode === 'dark' ? '#000000' : '#f2f2f7'
        }
      }}
    >
      {/* iOS-style header */}
      <Box 
        sx={{ 
          p: 3, 
          pb: 2,
          backgroundColor: theme.palette.mode === 'dark' ? '#1c1c1e' : '#ffffff',
          borderBottom: `0.5px solid ${theme.palette.mode === 'dark' ? '#38383a' : '#c6c6c8'}`
        }}
      >
        <Box display="flex" alignItems="center" mb={2}>
          <Avatar
            sx={{ 
              width: 60, 
              height: 60, 
              mr: 2,
              backgroundColor: theme.palette.primary.main,
              fontSize: '24px',
              fontWeight: 600
            }}
            src={user?.avatar}
          >
            {user?.firstName?.[0]}{user?.lastName?.[0]}
          </Avatar>
          <Box>
            <Typography 
              variant="h6" 
              fontWeight={600}
              fontSize="17px"
              letterSpacing="-0.4px"
            >
              DecVCPlat
            </Typography>
            {user && (
              <Typography 
                variant="body2" 
                color="text.secondary"
                fontSize="13px"
              >
                {user.firstName} {user.lastName}
              </Typography>
            )}
          </Box>
        </Box>
        
        {user && (
          <Box 
            sx={{ 
              p: 1.5,
              backgroundColor: theme.palette.mode === 'dark' ? '#2c2c2e' : '#f2f2f7',
              borderRadius: 1
            }}
          >
            <Typography 
              variant="caption" 
              color="primary"
              fontSize="12px"
              fontWeight={600}
            >
              {user.role.toUpperCase()}
            </Typography>
            <Typography 
              variant="caption" 
              color="text.secondary"
              display="block"
              fontSize="11px"
            >
              {user.email}
            </Typography>
          </Box>
        )}
      </Box>

      {/* Menu items */}
      <List sx={{ p: 0 }}>
        {menuItems.map((item, index) => (
          <iOSDecVCPlatListItem 
            button 
            key={index}
            onClick={() => {
              window.location.href = item.path;
              onClose();
            }}
          >
            <ListItemIcon 
              sx={{ 
                color: theme.palette.primary.main,
                minWidth: 36
              }}
            >
              {item.icon}
            </ListItemIcon>
            <ListItemText 
              primary={item.label}
              primaryTypographyProps={{ 
                fontWeight: 400,
                fontSize: '17px',
                letterSpacing: '-0.4px'
              }}
            />
            <ChevronRight 
              sx={{ 
                color: theme.palette.mode === 'dark' ? '#8e8e93' : '#c7c7cc',
                fontSize: '16px'
              }} 
            />
          </iOSDecVCPlatListItem>
        ))}
        
        <Box sx={{ mt: 2 }}>
          <iOSDecVCPlatListItem 
            button
            onClick={() => {
              // Handle logout
              onClose();
            }}
            sx={{
              '&:active': {
                backgroundColor: theme.palette.error.light
              }
            }}
          >
            <ListItemIcon sx={{ color: theme.palette.error.main, minWidth: 36 }}>
              <ExitToApp fontSize="small" />
            </ListItemIcon>
            <ListItemText 
              primary="Sign Out"
              primaryTypographyProps={{ 
                fontWeight: 400,
                fontSize: '17px',
                letterSpacing: '-0.4px',
                color: theme.palette.error.main
              }}
            />
          </iOSDecVCPlatListItem>
        </Box>
      </List>
    </SwipeableDrawer>
  );
};

export const iOSDecVCPlatCreateFAB: React.FC<{ onClick: () => void }> = ({ onClick }) => {
  return (
    <iOSDecVCPlatFAB onClick={onClick} aria-label="create">
      <Add />
    </iOSDecVCPlatFAB>
  );
};

// iOS-specific layout wrapper
interface iOSDecVCPlatLayoutProps {
  children: React.ReactNode;
  title: string;
  showFAB?: boolean;
  onFABClick?: () => void;
  currentTab?: number;
  onTabChange?: (newValue: number) => void;
  rightAction?: React.ReactNode;
}

export const iOSDecVCPlatLayout: React.FC<iOSDecVCPlatLayoutProps> = ({
  children,
  title,
  showFAB = false,
  onFABClick,
  currentTab = 0,
  onTabChange,
  rightAction
}) => {
  const [drawerOpen, setDrawerOpen] = React.useState(false);

  return (
    <Box sx={{ pb: 10 }}>
      <iOSDecVCPlatMobileHeader
        title={title}
        onMenuClick={() => setDrawerOpen(true)}
        rightAction={rightAction}
      />
      
      <iOSDecVCPlatNavigationDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
      />
      
      <Box sx={{ backgroundColor: '#f2f2f7', minHeight: 'calc(100vh - 127px)' }}>
        <Box sx={{ p: 2 }}>
          {children}
        </Box>
      </Box>
      
      {showFAB && onFABClick && (
        <iOSDecVCPlatCreateFAB onClick={onFABClick} />
      )}
      
      {onTabChange && (
        <iOSDecVCPlatBottomNavigation
          currentTab={currentTab}
          onTabChange={onTabChange}
        />
      )}
    </Box>
  );
};

export default iOSDecVCPlatLayout;
