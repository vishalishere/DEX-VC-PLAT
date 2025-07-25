// © 2024 DecVCPlat. All rights reserved.

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Paper,
  Grid,
  TextField,
  Button,
  IconButton,
  Chip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  InputAdornment,
  Stepper,
  Step,
  StepLabel,
  Card,
  CardContent,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Alert,
  Divider,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Add as AddIcon,
  Delete as DeleteIcon,
  Upload as UploadIcon,
  AttachMoney as MoneyIcon,
  Timeline as TimelineIcon,
  Description as DescriptionIcon,
} from '@mui/icons-material';
import { useAppDispatch } from '../../hooks/redux';
import { useAuth } from '../../hooks/useAuth';
import { createProject } from '../../store/slices/projectSlice';
import toast from 'react-hot-toast';
import { Helmet } from 'react-helmet-async';

interface DecVCPlatMilestone {
  title: string;
  description: string;
  dueDate: string;
  fundingAmount: number;
}

interface DecVCPlatProjectData {
  title: string;
  description: string;
  category: string;
  fundingGoal: number;
  duration: number;
  tags: string[];
  milestones: DecVCPlatMilestone[];
  documents: File[];
}

const DecVCPlatCreateProjectPage: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAuth();

  const [decvcplatActiveStep, setDecVCPlatActiveStep] = useState(0);
  const [decvcplatNewTag, setDecVCPlatNewTag] = useState('');
  const [decvcplatIsSubmitting, setDecVCPlatIsSubmitting] = useState(false);

  const [decvcplatProjectData, setDecVCPlatProjectData] = useState<DecVCPlatProjectData>({
    title: '',
    description: '',
    category: '',
    fundingGoal: 0,
    duration: 12,
    tags: [],
    milestones: [
      {
        title: 'Project Initiation',
        description: 'Initial project setup and team formation',
        dueDate: '',
        fundingAmount: 0,
      }
    ],
    documents: [],
  });

  const decvcplatStepLabels = [
    'Basic Information',
    'Funding & Timeline',
    'Milestones',
    'Documents & Review'
  ];

  const decvcplatCategories = [
    'Technology',
    'Healthcare',
    'Fintech',
    'Education',
    'Sustainability',
    'Gaming',
    'AI/ML',
    'Blockchain',
    'IoT',
    'Other'
  ];

  const handleDecVCPlatInputChange = (field: keyof DecVCPlatProjectData, value: any) => {
    setDecVCPlatProjectData(prev => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleDecVCPlatAddTag = () => {
    if (decvcplatNewTag.trim() && !decvcplatProjectData.tags.includes(decvcplatNewTag.trim())) {
      handleDecVCPlatInputChange('tags', [...decvcplatProjectData.tags, decvcplatNewTag.trim()]);
      setDecVCPlatNewTag('');
    }
  };

  const handleDecVCPlatRemoveTag = (tagToRemove: string) => {
    handleDecVCPlatInputChange('tags', decvcplatProjectData.tags.filter(tag => tag !== tagToRemove));
  };

  const handleDecVCPlatMilestoneChange = (index: number, field: keyof DecVCPlatMilestone, value: any) => {
    const updatedMilestones = decvcplatProjectData.milestones.map((milestone, i) =>
      i === index ? { ...milestone, [field]: value } : milestone
    );
    handleDecVCPlatInputChange('milestones', updatedMilestones);
  };

  const handleDecVCPlatAddMilestone = () => {
    const newMilestone: DecVCPlatMilestone = {
      title: '',
      description: '',
      dueDate: '',
      fundingAmount: 0,
    };
    handleDecVCPlatInputChange('milestones', [...decvcplatProjectData.milestones, newMilestone]);
  };

  const handleDecVCPlatRemoveMilestone = (index: number) => {
    if (decvcplatProjectData.milestones.length > 1) {
      const updatedMilestones = decvcplatProjectData.milestones.filter((_, i) => i !== index);
      handleDecVCPlatInputChange('milestones', updatedMilestones);
    }
  };

  const handleDecVCPlatFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (files) {
      const newFiles = Array.from(files);
      handleDecVCPlatInputChange('documents', [...decvcplatProjectData.documents, ...newFiles]);
    }
  };

  const handleDecVCPlatRemoveDocument = (index: number) => {
    const updatedDocuments = decvcplatProjectData.documents.filter((_, i) => i !== index);
    handleDecVCPlatInputChange('documents', updatedDocuments);
  };

  const handleDecVCPlatNext = () => {
    if (decvcplatActiveStep < decvcplatStepLabels.length - 1) {
      setDecVCPlatActiveStep(prev => prev + 1);
    }
  };

  const handleDecVCPlatBack = () => {
    if (decvcplatActiveStep > 0) {
      setDecVCPlatActiveStep(prev => prev - 1);
    }
  };

  const validateDecVCPlatCurrentStep = () => {
    switch (decvcplatActiveStep) {
      case 0:
        return decvcplatProjectData.title.trim() && 
               decvcplatProjectData.description.trim() && 
               decvcplatProjectData.category;
      case 1:
        return decvcplatProjectData.fundingGoal > 0 && 
               decvcplatProjectData.duration > 0;
      case 2:
        return decvcplatProjectData.milestones.every(m => 
          m.title.trim() && m.description.trim() && m.dueDate && m.fundingAmount >= 0
        );
      case 3:
        return true; // Documents are optional
      default:
        return false;
    }
  };

  const handleDecVCPlatSubmit = async () => {
    if (!user) {
      toast.error('You must be logged in to create a project');
      return;
    }

    if (user.role !== 'Founder') {
      toast.error('Only Founders can create projects');
      return;
    }

    setDecVCPlatIsSubmitting(true);

    try {
      const projectSubmissionData = {
        ...decvcplatProjectData,
        founderId: user.id,
        founderName: user.displayName,
        status: 'Pending',
        submittedAt: new Date().toISOString(),
      };

      await dispatch(createProject(projectSubmissionData)).unwrap();
      
      toast.success('Project created successfully! It will be reviewed by the community.');
      navigate('/projects');
    } catch (error: any) {
      toast.error(error.message || 'Failed to create project');
    } finally {
      setDecVCPlatIsSubmitting(false);
    }
  };

  const renderDecVCPlatStepContent = () => {
    switch (decvcplatActiveStep) {
      case 0:
        return (
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Project Title"
                value={decvcplatProjectData.title}
                onChange={(e) => handleDecVCPlatInputChange('title', e.target.value)}
                placeholder="Enter a compelling project title"
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={4}
                label="Project Description"
                value={decvcplatProjectData.description}
                onChange={(e) => handleDecVCPlatInputChange('description', e.target.value)}
                placeholder="Describe your project, its goals, and potential impact"
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth required>
                <InputLabel>Category</InputLabel>
                <Select
                  value={decvcplatProjectData.category}
                  onChange={(e) => handleDecVCPlatInputChange('category', e.target.value)}
                  label="Category"
                >
                  {decvcplatCategories.map((category) => (
                    <MenuItem key={category} value={category}>
                      {category}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Project Tags
                </Typography>
                <Box display="flex" gap={1} mb={2} flexWrap="wrap">
                  {decvcplatProjectData.tags.map((tag, index) => (
                    <Chip
                      key={index}
                      label={tag}
                      onDelete={() => handleDecVCPlatRemoveTag(tag)}
                      color="primary"
                      variant="outlined"
                    />
                  ))}
                </Box>
                <Box display="flex" gap={1}>
                  <TextField
                    size="small"
                    placeholder="Add a tag"
                    value={decvcplatNewTag}
                    onChange={(e) => setDecVCPlatNewTag(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && handleDecVCPlatAddTag()}
                  />
                  <Button
                    variant="outlined"
                    onClick={handleDecVCPlatAddTag}
                    disabled={!decvcplatNewTag.trim()}
                  >
                    Add Tag
                  </Button>
                </Box>
              </Box>
            </Grid>
          </Grid>
        );

      case 1:
        return (
          <Grid container spacing={3}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Funding Goal"
                value={decvcplatProjectData.fundingGoal}
                onChange={(e) => handleDecVCPlatInputChange('fundingGoal', Number(e.target.value))}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                  endAdornment: <InputAdornment position="end">ETH</InputAdornment>,
                }}
                placeholder="0"
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Project Duration"
                value={decvcplatProjectData.duration}
                onChange={(e) => handleDecVCPlatInputChange('duration', Number(e.target.value))}
                InputProps={{
                  endAdornment: <InputAdornment position="end">months</InputAdornment>,
                }}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <Alert severity="info">
                <Typography variant="body2">
                  <strong>Funding Information:</strong> Your funding goal will be released in tranches based on milestone completion. 
                  The DecVCPlat community will vote on milestone approvals before funds are released.
                </Typography>
              </Alert>
            </Grid>
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Funding Breakdown Preview
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Based on your milestones, funding will be distributed as follows:
                  </Typography>
                  {/* This will be calculated from milestones in the next step */}
                  <Typography variant="body2" sx={{ mt: 1 }}>
                    Detailed breakdown will be shown after defining milestones.
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        );

      case 2:
        return (
          <Box>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
              <Typography variant="h6">Project Milestones</Typography>
              <Button
                variant="outlined"
                startIcon={<AddIcon />}
                onClick={handleDecVCPlatAddMilestone}
              >
                Add Milestone
              </Button>
            </Box>
            
            <Alert severity="info" sx={{ mb: 3 }}>
              Define clear, measurable milestones for your project. Each milestone should have specific deliverables 
              and a corresponding funding amount that will be released upon community approval.
            </Alert>

            {decvcplatProjectData.milestones.map((milestone, index) => (
              <Card key={index} sx={{ mb: 2 }}>
                <CardContent>
                  <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                    <Typography variant="subtitle1">
                      Milestone {index + 1}
                    </Typography>
                    {decvcplatProjectData.milestones.length > 1 && (
                      <IconButton
                        onClick={() => handleDecVCPlatRemoveMilestone(index)}
                        color="error"
                        size="small"
                      >
                        <DeleteIcon />
                      </IconButton>
                    )}
                  </Box>
                  
                  <Grid container spacing={2}>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        label="Milestone Title"
                        value={milestone.title}
                        onChange={(e) => handleDecVCPlatMilestoneChange(index, 'title', e.target.value)}
                        required
                      />
                    </Grid>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        type="date"
                        label="Due Date"
                        value={milestone.dueDate}
                        onChange={(e) => handleDecVCPlatMilestoneChange(index, 'dueDate', e.target.value)}
                        InputLabelProps={{ shrink: true }}
                        required
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <TextField
                        fullWidth
                        multiline
                        rows={2}
                        label="Milestone Description"
                        value={milestone.description}
                        onChange={(e) => handleDecVCPlatMilestoneChange(index, 'description', e.target.value)}
                        placeholder="Describe what will be delivered in this milestone"
                        required
                      />
                    </Grid>
                    <Grid item xs={12} sm={6}>
                      <TextField
                        fullWidth
                        type="number"
                        label="Funding Amount"
                        value={milestone.fundingAmount}
                        onChange={(e) => handleDecVCPlatMilestoneChange(index, 'fundingAmount', Number(e.target.value))}
                        InputProps={{
                          startAdornment: <InputAdornment position="start">$</InputAdornment>,
                          endAdornment: <InputAdornment position="end">ETH</InputAdornment>,
                        }}
                        required
                      />
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            ))}

            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Milestone Summary
                </Typography>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Total Milestone Funding: ${decvcplatProjectData.milestones.reduce((sum, m) => sum + m.fundingAmount, 0).toLocaleString()} ETH
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Funding Goal: ${decvcplatProjectData.fundingGoal.toLocaleString()} ETH
                </Typography>
                {decvcplatProjectData.milestones.reduce((sum, m) => sum + m.fundingAmount, 0) !== decvcplatProjectData.fundingGoal && (
                  <Alert severity="warning" sx={{ mt: 2 }}>
                    The sum of milestone funding amounts should equal your total funding goal.
                  </Alert>
                )}
              </CardContent>
            </Card>
          </Box>
        );

      case 3:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Project Documents
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Upload supporting documents such as business plans, technical specifications, 
              market research, or prototypes to strengthen your project proposal.
            </Typography>

            <Card sx={{ mb: 3, p: 3, textAlign: 'center', border: '2px dashed', borderColor: 'divider' }}>
              <input
                accept=".pdf,.doc,.docx,.ppt,.pptx,.txt,.png,.jpg,.jpeg"
                style={{ display: 'none' }}
                id="decvcplat-file-upload"
                multiple
                type="file"
                onChange={handleDecVCPlatFileUpload}
              />
              <label htmlFor="decvcplat-file-upload">
                <Button
                  component="span"
                  variant="outlined"
                  startIcon={<UploadIcon />}
                  size="large"
                >
                  Upload Documents
                </Button>
              </label>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
                Supported formats: PDF, DOC, DOCX, PPT, PPTX, TXT, PNG, JPG, JPEG
              </Typography>
            </Card>

            {decvcplatProjectData.documents.length > 0 && (
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Uploaded Documents
                  </Typography>
                  <List>
                    {decvcplatProjectData.documents.map((file, index) => (
                      <ListItem key={index} divider>
                        <ListItemText
                          primary={file.name}
                          secondary={`${(file.size / 1024).toFixed(1)} KB`}
                        />
                        <ListItemSecondaryAction>
                          <IconButton
                            edge="end"
                            onClick={() => handleDecVCPlatRemoveDocument(index)}
                            color="error"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </ListItemSecondaryAction>
                      </ListItem>
                    ))}
                  </List>
                </CardContent>
              </Card>
            )}

            <Divider sx={{ my: 3 }} />

            {/* Project Review Summary */}
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Project Review Summary
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6}>
                    <Typography variant="subtitle2">Title:</Typography>
                    <Typography variant="body2" sx={{ mb: 1 }}>{decvcplatProjectData.title}</Typography>
                    
                    <Typography variant="subtitle2">Category:</Typography>
                    <Typography variant="body2" sx={{ mb: 1 }}>{decvcplatProjectData.category}</Typography>
                    
                    <Typography variant="subtitle2">Funding Goal:</Typography>
                    <Typography variant="body2">${decvcplatProjectData.fundingGoal.toLocaleString()} ETH</Typography>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <Typography variant="subtitle2">Duration:</Typography>
                    <Typography variant="body2" sx={{ mb: 1 }}>{decvcplatProjectData.duration} months</Typography>
                    
                    <Typography variant="subtitle2">Milestones:</Typography>
                    <Typography variant="body2" sx={{ mb: 1 }}>{decvcplatProjectData.milestones.length} milestones</Typography>
                    
                    <Typography variant="subtitle2">Documents:</Typography>
                    <Typography variant="body2">{decvcplatProjectData.documents.length} files uploaded</Typography>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Box>
        );

      default:
        return null;
    }
  };

  return (
    <>
      <Helmet>
        <title>Create New Project - DecVCPlat</title>
        <meta name="description" content="Submit a new project proposal to the DecVCPlat community" />
      </Helmet>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box display="flex" alignItems="center" mb={4}>
          <IconButton onClick={() => navigate('/projects')} sx={{ mr: 2 }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h4" component="h1">
            Create New Project
          </Typography>
        </Box>

        {/* Stepper */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Stepper activeStep={decvcplatActiveStep} alternativeLabel>
            {decvcplatStepLabels.map((label, index) => (
              <Step key={label}>
                <StepLabel>{label}</StepLabel>
              </Step>
            ))}
          </Stepper>
        </Paper>

        {/* Step Content */}
        <Paper sx={{ p: 4, mb: 3 }}>
          {renderDecVCPlatStepContent()}
        </Paper>

        {/* Navigation Buttons */}
        <Box display="flex" justifyContent="space-between">
          <Button
            variant="outlined"
            onClick={handleDecVCPlatBack}
            disabled={decvcplatActiveStep === 0}
          >
            Back
          </Button>
          
          <Box>
            {decvcplatActiveStep < decvcplatStepLabels.length - 1 ? (
              <Button
                variant="contained"
                onClick={handleDecVCPlatNext}
                disabled={!validateDecVCPlatCurrentStep()}
              >
                Next
              </Button>
            ) : (
              <Button
                variant="contained"
                onClick={handleDecVCPlatSubmit}
                disabled={!validateDecVCPlatCurrentStep() || decvcplatIsSubmitting}
              >
                {decvcplatIsSubmitting ? 'Creating Project...' : 'Submit Project'}
              </Button>
            )}
          </Box>
        </Box>
      </Container>
    </>
  );
};

export default DecVCPlatCreateProjectPage;
