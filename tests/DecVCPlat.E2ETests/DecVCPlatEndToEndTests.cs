// © 2024 DecVCPlat. All rights reserved.

using Microsoft.Playwright;
using Xunit;

namespace DecVCPlat.E2ETests;

public class DecVCPlatEndToEndTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page?.CloseAsync()!;
        await _context?.CloseAsync()!;
        await _browser?.CloseAsync()!;
        _playwright?.Dispose();
    }

    [Fact]
    public async Task DecVCPlat_UserRegistration_CompleteFlow_Success()
    {
        // Navigate to registration page
        await _page.GotoAsync("http://localhost:3000/register");
        
        // Wait for page to load
        await _page.WaitForSelectorAsync("[data-testid=registration-form]", new PageWaitForSelectorOptions
        {
            Timeout = 5000
        });

        // Fill registration form
        await _page.FillAsync("[data-testid=email-input]", "testuser@decvcplat.com");
        await _page.FillAsync("[data-testid=password-input]", "DecVCPlat@123");
        await _page.FillAsync("[data-testid=confirm-password-input]", "DecVCPlat@123");
        await _page.FillAsync("[data-testid=first-name-input]", "Test");
        await _page.FillAsync("[data-testid=last-name-input]", "User");
        await _page.SelectOptionAsync("[data-testid=role-select]", "Founder");
        
        // Accept GDPR consent
        await _page.CheckAsync("[data-testid=gdpr-consent-checkbox]");
        
        // Submit registration
        await _page.ClickAsync("[data-testid=register-button]");
        
        // Wait for redirect to dashboard
        await _page.WaitForURLAsync("**/dashboard", new PageWaitForURLOptions
        {
            Timeout = 10000
        });

        // Verify successful registration
        var welcomeMessage = await _page.TextContentAsync("[data-testid=welcome-message]");
        Assert.Contains("Welcome to DecVCPlat", welcomeMessage);
    }

    [Fact]
    public async Task DecVCPlat_UserLogin_ValidCredentials_Success()
    {
        // Navigate to login page
        await _page.GotoAsync("http://localhost:3000/login");
        
        // Wait for login form
        await _page.WaitForSelectorAsync("[data-testid=login-form]");

        // Fill login credentials
        await _page.FillAsync("[data-testid=email-input]", "founder@decvcplat.com");
        await _page.FillAsync("[data-testid=password-input]", "DecVCPlat@123");
        
        // Submit login
        await _page.ClickAsync("[data-testid=login-button]");
        
        // Wait for redirect to dashboard
        await _page.WaitForURLAsync("**/dashboard");

        // Verify successful login
        var userProfile = await _page.TextContentAsync("[data-testid=user-profile]");
        Assert.Contains("founder@decvcplat.com", userProfile);
    }

    [Fact]
    public async Task DecVCPlat_ProjectCreation_FounderRole_Success()
    {
        // Login as founder first
        await LoginAsFounder();
        
        // Navigate to create project page
        await _page.ClickAsync("[data-testid=create-project-link]");
        await _page.WaitForURLAsync("**/projects/create");

        // Fill project details - Step 1: Basic Information
        await _page.FillAsync("[data-testid=project-title-input]", "E2E Test Project");
        await _page.FillAsync("[data-testid=project-description-input]", "End-to-end test project for DecVCPlat");
        await _page.SelectOptionAsync("[data-testid=project-category-select]", "Technology");
        await _page.FillAsync("[data-testid=project-tags-input]", "blockchain,fintech");
        
        // Next step
        await _page.ClickAsync("[data-testid=next-step-button]");

        // Step 2: Funding and Timeline
        await _page.FillAsync("[data-testid=funding-goal-input]", "100000");
        await _page.FillAsync("[data-testid=project-duration-input]", "12");
        
        // Next step
        await _page.ClickAsync("[data-testid=next-step-button]");

        // Step 3: Milestones
        await _page.ClickAsync("[data-testid=add-milestone-button]");
        await _page.FillAsync("[data-testid=milestone-title-input-0]", "MVP Development");
        await _page.FillAsync("[data-testid=milestone-description-input-0]", "Develop minimum viable product");
        await _page.FillAsync("[data-testid=milestone-funding-input-0]", "50");
        
        // Next step
        await _page.ClickAsync("[data-testid=next-step-button]");

        // Step 4: Skip documents for now
        await _page.ClickAsync("[data-testid=next-step-button]");

        // Step 5: Review and Submit
        await _page.ClickAsync("[data-testid=submit-project-button]");
        
        // Wait for success message
        await _page.WaitForSelectorAsync("[data-testid=project-created-success]");
        
        // Verify project creation
        var successMessage = await _page.TextContentAsync("[data-testid=project-created-success]");
        Assert.Contains("Project created successfully", successMessage);
    }

    [Fact]
    public async Task DecVCPlat_ProjectVoting_InvestorRole_Success()
    {
        // Login as investor
        await LoginAsInvestor();
        
        // Navigate to voting page
        await _page.ClickAsync("[data-testid=voting-link]");
        await _page.WaitForURLAsync("**/voting");

        // Find first project proposal
        await _page.WaitForSelectorAsync("[data-testid=proposal-card]");
        
        // Click on first proposal
        await _page.ClickAsync("[data-testid=proposal-card]:first-child [data-testid=vote-button]");
        
        // Wait for voting dialog
        await _page.WaitForSelectorAsync("[data-testid=voting-dialog]");
        
        // Select approve vote
        await _page.ClickAsync("[data-testid=approve-vote-button]");
        
        // Add comments
        await _page.FillAsync("[data-testid=vote-comments-input]", "Excellent project with strong potential");
        
        // Submit vote
        await _page.ClickAsync("[data-testid=submit-vote-button]");
        
        // Wait for success confirmation
        await _page.WaitForSelectorAsync("[data-testid=vote-success-message]");
        
        // Verify voting success
        var voteMessage = await _page.TextContentAsync("[data-testid=vote-success-message]");
        Assert.Contains("Vote submitted successfully", voteMessage);
    }

    [Fact]
    public async Task DecVCPlat_WalletConnection_MetaMask_Success()
    {
        // Login first
        await LoginAsFounder();
        
        // Navigate to wallet page
        await _page.ClickAsync("[data-testid=wallet-link]");
        await _page.WaitForURLAsync("**/wallet");

        // Mock MetaMask connection (in real E2E, this would interact with actual MetaMask extension)
        await _page.EvaluateAsync(@"
            window.ethereum = {
                request: async ({ method }) => {
                    if (method === 'eth_requestAccounts') {
                        return ['0x1234567890123456789012345678901234567890'];
                    }
                    if (method === 'eth_getBalance') {
                        return '0x16345785d8a0000'; // 0.1 ETH in wei
                    }
                    return null;
                },
                selectedAddress: '0x1234567890123456789012345678901234567890'
            };
        ");

        // Click connect wallet button
        await _page.ClickAsync("[data-testid=connect-wallet-button]");
        
        // Wait for wallet connection
        await _page.WaitForSelectorAsync("[data-testid=wallet-connected]");
        
        // Verify wallet connection
        var walletAddress = await _page.TextContentAsync("[data-testid=wallet-address]");
        Assert.Contains("0x1234567890123456789012345678901234567890", walletAddress);
    }

    [Fact]
    public async Task DecVCPlat_NotificationSystem_RealTime_Success()
    {
        // Login as founder
        await LoginAsFounder();
        
        // Navigate to notifications page
        await _page.ClickAsync("[data-testid=notifications-link]");
        await _page.WaitForURLAsync("**/notifications");

        // Check for notification indicator
        var notificationCount = await _page.TextContentAsync("[data-testid=notification-count]");
        Assert.NotNull(notificationCount);

        // Click on first notification
        await _page.ClickAsync("[data-testid=notification-item]:first-child");
        
        // Verify notification details
        var notificationTitle = await _page.TextContentAsync("[data-testid=notification-title]");
        Assert.NotEmpty(notificationTitle);
    }

    [Fact]
    public async Task DecVCPlat_ProfileManagement_UpdateSettings_Success()
    {
        // Login first
        await LoginAsFounder();
        
        // Navigate to profile page
        await _page.ClickAsync("[data-testid=profile-link]");
        await _page.WaitForURLAsync("**/profile");

        // Update profile information
        await _page.FillAsync("[data-testid=first-name-input]", "Updated");
        await _page.FillAsync("[data-testid=last-name-input]", "Name");
        
        // Update notification preferences
        await _page.ClickAsync("[data-testid=notification-preferences-tab]");
        await _page.CheckAsync("[data-testid=email-notifications-checkbox]");
        
        // Save changes
        await _page.ClickAsync("[data-testid=save-profile-button]");
        
        // Wait for success message
        await _page.WaitForSelectorAsync("[data-testid=profile-updated-success]");
        
        // Verify profile update
        var successMessage = await _page.TextContentAsync("[data-testid=profile-updated-success]");
        Assert.Contains("Profile updated successfully", successMessage);
    }

    [Fact]
    public async Task DecVCPlat_ProjectSearch_FilterAndSort_Success()
    {
        // Login as investor
        await LoginAsInvestor();
        
        // Navigate to projects page
        await _page.ClickAsync("[data-testid=projects-link]");
        await _page.WaitForURLAsync("**/projects");

        // Use search functionality
        await _page.FillAsync("[data-testid=project-search-input]", "blockchain");
        await _page.ClickAsync("[data-testid=search-button]");
        
        // Wait for search results
        await _page.WaitForSelectorAsync("[data-testid=search-results]");
        
        // Apply category filter
        await _page.SelectOptionAsync("[data-testid=category-filter]", "Technology");
        
        // Apply sort order
        await _page.SelectOptionAsync("[data-testid=sort-select]", "funding-desc");
        
        // Verify filtered results
        var projectCards = await _page.QuerySelectorAllAsync("[data-testid=project-card]");
        Assert.True(projectCards.Count > 0);
    }

    // Helper methods
    private async Task LoginAsFounder()
    {
        await _page.GotoAsync("http://localhost:3000/login");
        await _page.FillAsync("[data-testid=email-input]", "founder@decvcplat.com");
        await _page.FillAsync("[data-testid=password-input]", "DecVCPlat@123");
        await _page.ClickAsync("[data-testid=login-button]");
        await _page.WaitForURLAsync("**/dashboard");
    }

    private async Task LoginAsInvestor()
    {
        await _page.GotoAsync("http://localhost:3000/login");
        await _page.FillAsync("[data-testid=email-input]", "investor@decvcplat.com");
        await _page.FillAsync("[data-testid=password-input]", "DecVCPlat@123");
        await _page.ClickAsync("[data-testid=login-button]");
        await _page.WaitForURLAsync("**/dashboard");
    }
}
