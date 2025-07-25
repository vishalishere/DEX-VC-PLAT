import React from 'react';
import { render, screen } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import { theme } from '../../theme/theme';

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

describe('LoadingSpinner', () => {
  it('renders with default message', () => {
    renderWithTheme(<LoadingSpinner />);
    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('renders with custom message', () => {
    const customMessage = 'Loading DecVCPlat data...';
    renderWithTheme(<LoadingSpinner message={customMessage} />);
    expect(screen.getByText(customMessage)).toBeInTheDocument();
  });

  it('renders centered by default', () => {
    renderWithTheme(<LoadingSpinner />);
    const container = screen.getByRole('progressbar').closest('div');
    expect(container).toHaveStyle({
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center'
    });
  });

  it('renders non-centered when specified', () => {
    renderWithTheme(<LoadingSpinner centered={false} />);
    const container = screen.getByRole('progressbar').closest('div');
    expect(container).not.toHaveStyle({
      justifyContent: 'center',
      alignItems: 'center'
    });
  });

  it('applies custom size', () => {
    renderWithTheme(<LoadingSpinner size={60} />);
    const progressbar = screen.getByRole('progressbar');
    expect(progressbar).toHaveStyle({
      width: '60px',
      height: '60px'
    });
  });

  it('has correct accessibility attributes', () => {
    renderWithTheme(<LoadingSpinner message="Loading projects" />);
    const progressbar = screen.getByRole('progressbar');
    expect(progressbar).toHaveAttribute('aria-label', 'Loading projects');
  });
});
