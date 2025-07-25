// © 2024 DecVCPlat. All rights reserved.

import React, { useEffect, useState } from 'react';
import { Container, Grid, Box, Typography, Button, TextField, FormControl, InputLabel, Select, MenuItem, Pagination, useTheme, Paper } from '@mui/material';
import { Add, FilterList, Search } from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { fetchProjects, setFilters, setPagination } from '../../store/slices/projectSlice';
import ProjectCard from '../../components/Project/ProjectCard';
import LoadingSpinner from '../../components/Common/LoadingSpinner';
import { Helmet } from 'react-helmet-async';

const ProjectsPage: React.FC = () => {
  const decvcplatTheme = useTheme();
  const decvcplatAuth = useAuth();
  const decvcplatDispatch = useAppDispatch();
  
  const { 
    projects: decvcplatProjectList, 
    isLoading: decvcplatProjectsLoading, 
    filters: decvcplatCurrentFilters, 
    pagination: decvcplatPaginationState 
  } = useAppSelector(state => state.projects);
  
  const [decvcplatSearchInput, setDecVCPlatSearchInput] = useState(decvcplatCurrentFilters.search);
  const [decvcplatShowFilters, setDecVCPlatShowFilters] = useState(false);

  useEffect(() => {
    decvcplatDispatch(fetchProjects({
      page: decvcplatPaginationState.page,
      pageSize: decvcplatPaginationState.pageSize,
      status: decvcplatCurrentFilters.status,
      category: decvcplatCurrentFilters.category,
      search: decvcplatCurrentFilters.search,
    }));
  }, [decvcplatDispatch, decvcplatPaginationState.page, decvcplatCurrentFilters]);

  const handleDecVCPlatSearch = () => {
    decvcplatDispatch(setFilters({ search: decvcplatSearchInput }));
    decvcplatDispatch(setPagination({ page: 1 }));
  };

  const handleDecVCPlatFilterChange = (decvcplatFilterType: string, decvcplatFilterValue: string) => {
    decvcplatDispatch(setFilters({ [decvcplatFilterType]: decvcplatFilterValue }));
    decvcplatDispatch(setPagination({ page: 1 }));
  };

  const handleDecVCPlatPageChange = (_: React.ChangeEvent<unknown>, decvcplatNewPage: number) => {
    decvcplatDispatch(setPagination({ page: decvcplatNewPage }));
  };

  const getDecVCPlatPageTitle = (): string => {
    switch (decvcplatAuth.user?.role) {
      case 'Founder':
        return 'My DecVCPlat Projects';
      case 'Investor':
        return 'Explore DecVCPlat Projects';
      case 'Luminary':
        return 'Curate DecVCPlat Projects';
      default:
        return 'DecVCPlat Projects';
    }
  };

  const getDecVCPlatPageDescription = (): string => {
    switch (decvcplatAuth.user?.role) {
      case 'Founder':
        return 'Manage your DecVCPlat venture capital projects and track funding progress';
      case 'Investor':
        return 'Discover innovative DecVCPlat projects to support and invest in';
      case 'Luminary':
        return 'Review and curate high-quality DecVCPlat projects for the community';
      default:
        return 'Explore innovative projects on the DecVCPlat platform';
    }
  };

  if (decvcplatProjectsLoading) {
    return <LoadingSpinner message="Loading DecVCPlat projects..." />;
  }

  return (
    <>
      <Helmet>
        <title>{getDecVCPlatPageTitle()} - DecVCPlat</title>
        <meta name="description" content={getDecVCPlatPageDescription()} />
      </Helmet>

      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}>
          <Box>
            <Typography variant="h4" fontWeight={600} gutterBottom>
              {getDecVCPlatPageTitle()}
            </Typography>
            <Typography variant="body1" color="text.secondary">
              {getDecVCPlatPageDescription()}
            </Typography>
          </Box>

          {decvcplatAuth.canCreateProjects() && (
            <Button
              component={RouterLink}
              to="/projects/create"
              variant="contained"
              startIcon={<Add />}
              sx={{
                background: decvcplatTheme.custom.gradients.primary,
                '&:hover': { opacity: 0.9 },
              }}
            >
              Create DecVCPlat Project
            </Button>
          )}
        </Box>

        <Paper elevation={1} sx={{ p: 3, mb: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: decvcplatShowFilters ? 3 : 0 }}>
            <TextField
              placeholder="Search DecVCPlat projects..."
              value={decvcplatSearchInput}
              onChange={(e) => setDecVCPlatSearchInput(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleDecVCPlatSearch()}
              InputProps={{
                startAdornment: <Search color="action" sx={{ mr: 1 }} />,
              }}
              sx={{ flex: 1 }}
            />
            <Button
              variant="contained"
              onClick={handleDecVCPlatSearch}
              sx={{ minWidth: 120 }}
            >
              Search
            </Button>
            <Button
              variant="outlined"
              startIcon={<FilterList />}
              onClick={() => setDecVCPlatShowFilters(!decvcplatShowFilters)}
            >
              Filters
            </Button>
          </Box>

          {decvcplatShowFilters && (
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>DecVCPlat Status</InputLabel>
                  <Select
                    value={decvcplatCurrentFilters.status}
                    label="DecVCPlat Status"
                    onChange={(e) => handleDecVCPlatFilterChange('status', e.target.value)}
                  >
                    <MenuItem value="All">All Statuses</MenuItem>
                    <MenuItem value="Draft">Draft</MenuItem>
                    <MenuItem value="Submitted">Submitted</MenuItem>
                    <MenuItem value="UnderReview">Under Review</MenuItem>
                    <MenuItem value="Approved">Approved</MenuItem>
                    <MenuItem value="Funded">Funded</MenuItem>
                    <MenuItem value="Completed">Completed</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>DecVCPlat Category</InputLabel>
                  <Select
                    value={decvcplatCurrentFilters.category}
                    label="DecVCPlat Category"
                    onChange={(e) => handleDecVCPlatFilterChange('category', e.target.value)}
                  >
                    <MenuItem value="All">All Categories</MenuItem>
                    <MenuItem value="Healthcare">Healthcare</MenuItem>
                    <MenuItem value="Energy">Energy</MenuItem>
                    <MenuItem value="Technology">Technology</MenuItem>
                    <MenuItem value="Finance">Finance</MenuItem>
                    <MenuItem value="Education">Education</MenuItem>
                    <MenuItem value="Environment">Environment</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          )}
        </Paper>

        {decvcplatProjectList.length === 0 ? (
          <Paper elevation={1} sx={{ p: 6, textAlign: 'center' }}>
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No DecVCPlat projects found
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              {decvcplatAuth.canCreateProjects() 
                ? "Start by creating your first DecVCPlat project to attract investors."
                : "Check back later for new DecVCPlat projects to explore and support."
              }
            </Typography>
            {decvcplatAuth.canCreateProjects() && (
              <Button
                component={RouterLink}
                to="/projects/create"
                variant="contained"
                startIcon={<Add />}
              >
                Create Your First DecVCPlat Project
              </Button>
            )}
          </Paper>
        ) : (
          <>
            <Grid container spacing={3} sx={{ mb: 4 }}>
              {decvcplatProjectList.map((decvcplatProject) => (
                <Grid item xs={12} sm={6} lg={4} key={decvcplatProject.id}>
                  <ProjectCard 
                    decvcplatProject={decvcplatProject}
                    decvcplatShowActions={true}
                    decvcplatCompactMode={false}
                  />
                </Grid>
              ))}
            </Grid>

            {decvcplatPaginationState.totalPages > 1 && (
              <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <Pagination
                  count={decvcplatPaginationState.totalPages}
                  page={decvcplatPaginationState.page}
                  onChange={handleDecVCPlatPageChange}
                  color="primary"
                  size="large"
                  showFirstButton
                  showLastButton
                />
              </Box>
            )}

            <Box sx={{ textAlign: 'center', mt: 4 }}>
              <Typography variant="body2" color="text.secondary">
                Showing {decvcplatProjectList.length} of {decvcplatPaginationState.totalCount} DecVCPlat projects
              </Typography>
            </Box>
          </>
        )}
      </Container>
    </>
  );
};

export default ProjectsPage;
