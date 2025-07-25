/**
 * DecVCPlat Full System Integration Validation
 * Comprehensive end-to-end validation of frontend-backend integration
 */

const axios = require('axios');
const WebSocket = require('ws');
const { ethers } = require('ethers');

class DecVCPlatSystemValidator {
  constructor() {
    this.apiBaseUrl = process.env.API_BASE_URL || 'https://api.decvcplat.com';
    this.frontendUrl = process.env.FRONTEND_URL || 'https://decvcplat.com';
    this.authToken = null;
    this.testUser = {
      email: 'test@decvcplat.com',
      password: 'TestPassword123!',
      firstName: 'Integration',
      lastName: 'Test',
      role: 'Founder'
    };
  }

  // Core Authentication Flow Validation
  async validateAuthenticationFlow() {
    console.log('🔐 Validating Authentication Flow...');
    
    try {
      // 1. User Registration
      const registrationResponse = await axios.post(`${this.apiBaseUrl}/api/auth/register`, {
        ...this.testUser,
        hasGdprConsent: true
      });
      
      if (registrationResponse.status !== 201) {
        throw new Error(`Registration failed: ${registrationResponse.status}`);
      }
      
      console.log('✅ User Registration: SUCCESS');

      // 2. User Login
      const loginResponse = await axios.post(`${this.apiBaseUrl}/api/auth/login`, {
        email: this.testUser.email,
        password: this.testUser.password
      });
      
      if (loginResponse.status !== 200 || !loginResponse.data.token) {
        throw new Error('Login failed or no token received');
      }
      
      this.authToken = loginResponse.data.token;
      console.log('✅ User Login: SUCCESS');

      // 3. Token Validation
      const profileResponse = await axios.get(`${this.apiBaseUrl}/api/auth/profile`, {
        headers: { Authorization: `Bearer ${this.authToken}` }
      });
      
      if (profileResponse.status !== 200) {
        throw new Error('Token validation failed');
      }
      
      console.log('✅ Token Validation: SUCCESS');
      return true;
    } catch (error) {
      console.error('❌ Authentication Flow Failed:', error.message);
      return false;
    }
  }

  // Project Management API Validation
  async validateProjectManagement() {
    console.log('📋 Validating Project Management APIs...');
    
    try {
      const headers = { Authorization: `Bearer ${this.authToken}` };
      
      // 1. Create Project
      const projectData = {
        title: 'Integration Test Project',
        description: 'This is a test project for integration validation',
        category: 'Technology',
        fundingGoal: 100000,
        milestones: [
          {
            title: 'Phase 1',
            description: 'Initial development',
            fundingAmount: 50000,
            dueDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString()
          }
        ]
      };
      
      const createResponse = await axios.post(`${this.apiBaseUrl}/api/projects`, projectData, { headers });
      
      if (createResponse.status !== 201) {
        throw new Error(`Project creation failed: ${createResponse.status}`);
      }
      
      const projectId = createResponse.data.id;
      console.log('✅ Project Creation: SUCCESS');

      // 2. Get Project Details
      const getResponse = await axios.get(`${this.apiBaseUrl}/api/projects/${projectId}`, { headers });
      
      if (getResponse.status !== 200) {
        throw new Error('Project retrieval failed');
      }
      
      console.log('✅ Project Retrieval: SUCCESS');

      // 3. Update Project
      const updateData = { description: 'Updated description for integration test' };
      const updateResponse = await axios.put(`${this.apiBaseUrl}/api/projects/${projectId}`, updateData, { headers });
      
      if (updateResponse.status !== 200) {
        throw new Error('Project update failed');
      }
      
      console.log('✅ Project Update: SUCCESS');

      // 4. List Projects
      const listResponse = await axios.get(`${this.apiBaseUrl}/api/projects`, { headers });
      
      if (listResponse.status !== 200 || !Array.isArray(listResponse.data.projects)) {
        throw new Error('Project listing failed');
      }
      
      console.log('✅ Project Listing: SUCCESS');
      return projectId;
    } catch (error) {
      console.error('❌ Project Management Failed:', error.message);
      return null;
    }
  }

  // Voting System Validation
  async validateVotingSystem(projectId) {
    console.log('🗳️ Validating Voting System...');
    
    try {
      const headers = { Authorization: `Bearer ${this.authToken}` };
      
      // 1. Create Voting Proposal
      const proposalData = {
        title: 'Test Proposal',
        description: 'Integration test voting proposal',
        proposalType: 'ProjectApproval',
        projectId: projectId,
        votingEndDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString()
      };
      
      const createProposalResponse = await axios.post(`${this.apiBaseUrl}/api/voting/proposals`, proposalData, { headers });
      
      if (createProposalResponse.status !== 201) {
        throw new Error(`Proposal creation failed: ${createProposalResponse.status}`);
      }
      
      const proposalId = createProposalResponse.data.id;
      console.log('✅ Proposal Creation: SUCCESS');

      // 2. Stake Tokens
      const stakeData = {
        amount: 100,
        proposalId: proposalId
      };
      
      const stakeResponse = await axios.post(`${this.apiBaseUrl}/api/voting/stake`, stakeData, { headers });
      
      if (stakeResponse.status !== 200) {
        throw new Error('Token staking failed');
      }
      
      console.log('✅ Token Staking: SUCCESS');

      // 3. Cast Vote
      const voteData = {
        proposalId: proposalId,
        voteType: 'For',
        votingPower: 100
      };
      
      const voteResponse = await axios.post(`${this.apiBaseUrl}/api/voting/vote`, voteData, { headers });
      
      if (voteResponse.status !== 200) {
        throw new Error('Vote casting failed');
      }
      
      console.log('✅ Vote Casting: SUCCESS');

      // 4. Get Proposal Details
      const proposalResponse = await axios.get(`${this.apiBaseUrl}/api/voting/proposals/${proposalId}`, { headers });
      
      if (proposalResponse.status !== 200) {
        throw new Error('Proposal retrieval failed');
      }
      
      console.log('✅ Proposal Retrieval: SUCCESS');
      return proposalId;
    } catch (error) {
      console.error('❌ Voting System Failed:', error.message);
      return null;
    }
  }

  // Real-time SignalR Notifications Validation
  async validateSignalRNotifications() {
    console.log('📡 Validating SignalR Real-time Notifications...');
    
    return new Promise((resolve) => {
      try {
        const signalRUrl = `${this.apiBaseUrl.replace('https', 'wss')}/notificationHub`;
        const ws = new WebSocket(signalRUrl, {
          headers: { Authorization: `Bearer ${this.authToken}` }
        });
        
        let notificationReceived = false;
        
        ws.on('open', () => {
          console.log('✅ SignalR Connection: SUCCESS');
          
          // Send test notification request
          ws.send(JSON.stringify({
            target: 'SendNotification',
            arguments: [{
              title: 'Integration Test',
              message: 'Testing real-time notifications',
              type: 'info'
            }]
          }));
        });
        
        ws.on('message', (data) => {
          try {
            const message = JSON.parse(data.toString());
            if (message.target === 'ReceiveNotification') {
              notificationReceived = true;
              console.log('✅ Real-time Notification: SUCCESS');
              ws.close();
              resolve(true);
            }
          } catch (error) {
            console.error('Error parsing SignalR message:', error);
          }
        });
        
        ws.on('error', (error) => {
          console.error('❌ SignalR Connection Failed:', error.message);
          resolve(false);
        });
        
        // Timeout after 10 seconds
        setTimeout(() => {
          if (!notificationReceived) {
            console.error('❌ SignalR Notification Timeout');
            ws.close();
            resolve(false);
          }
        }, 10000);
        
      } catch (error) {
        console.error('❌ SignalR Validation Failed:', error.message);
        resolve(false);
      }
    });
  }

  // Blockchain Wallet Integration Validation
  async validateBlockchainIntegration() {
    console.log('⛓️ Validating Blockchain Integration...');
    
    try {
      const headers = { Authorization: `Bearer ${this.authToken}` };
      
      // 1. Connect Wallet (Simulated)
      const walletAddress = '0x1234567890123456789012345678901234567890';
      const walletResponse = await axios.post(`${this.apiBaseUrl}/api/auth/wallet`, {
        walletAddress: walletAddress
      }, { headers });
      
      if (walletResponse.status !== 200) {
        throw new Error('Wallet connection failed');
      }
      
      console.log('✅ Wallet Connection: SUCCESS');

      // 2. Get Token Balance
      const balanceResponse = await axios.get(`${this.apiBaseUrl}/api/wallet/balance`, { headers });
      
      if (balanceResponse.status !== 200) {
        throw new Error('Token balance retrieval failed');
      }
      
      console.log('✅ Token Balance Retrieval: SUCCESS');

      // 3. Transaction History
      const historyResponse = await axios.get(`${this.apiBaseUrl}/api/wallet/transactions`, { headers });
      
      if (historyResponse.status !== 200) {
        throw new Error('Transaction history retrieval failed');
      }
      
      console.log('✅ Transaction History: SUCCESS');
      return true;
    } catch (error) {
      console.error('❌ Blockchain Integration Failed:', error.message);
      return false;
    }
  }

  // Frontend Asset Loading Validation
  async validateFrontendAssets() {
    console.log('🎨 Validating Frontend Assets...');
    
    try {
      // 1. Main App Loading
      const appResponse = await axios.get(this.frontendUrl);
      
      if (appResponse.status !== 200) {
        throw new Error(`Frontend app loading failed: ${appResponse.status}`);
      }
      
      console.log('✅ Frontend App Loading: SUCCESS');

      // 2. CSS Assets
      const cssRegex = /<link[^>]*href="([^"]*\.css)"[^>]*>/g;
      const cssMatches = [...appResponse.data.matchAll(cssRegex)];
      
      for (const match of cssMatches.slice(0, 3)) { // Test first 3 CSS files
        const cssUrl = match[1].startsWith('http') ? match[1] : `${this.frontendUrl}${match[1]}`;
        const cssResponse = await axios.get(cssUrl);
        
        if (cssResponse.status !== 200) {
          throw new Error(`CSS asset loading failed: ${cssUrl}`);
        }
      }
      
      console.log('✅ CSS Assets Loading: SUCCESS');

      // 3. JavaScript Assets
      const jsRegex = /<script[^>]*src="([^"]*\.js)"[^>]*>/g;
      const jsMatches = [...appResponse.data.matchAll(jsRegex)];
      
      for (const match of jsMatches.slice(0, 3)) { // Test first 3 JS files
        const jsUrl = match[1].startsWith('http') ? match[1] : `${this.frontendUrl}${match[1]}`;
        const jsResponse = await axios.get(jsUrl);
        
        if (jsResponse.status !== 200) {
          throw new Error(`JavaScript asset loading failed: ${jsUrl}`);
        }
      }
      
      console.log('✅ JavaScript Assets Loading: SUCCESS');
      return true;
    } catch (error) {
      console.error('❌ Frontend Assets Validation Failed:', error.message);
      return false;
    }
  }

  // API Health Checks
  async validateAPIHealth() {
    console.log('🏥 Validating API Health...');
    
    try {
      const services = [
        'user-management',
        'project-management',
        'voting',
        'funding',
        'notification'
      ];
      
      for (const service of services) {
        const healthResponse = await axios.get(`${this.apiBaseUrl}/api/${service}/health`);
        
        if (healthResponse.status !== 200) {
          throw new Error(`${service} health check failed`);
        }
        
        console.log(`✅ ${service} Health Check: SUCCESS`);
      }
      
      return true;
    } catch (error) {
      console.error('❌ API Health Validation Failed:', error.message);
      return false;
    }
  }

  // Run Complete System Validation
  async runCompleteValidation() {
    console.log('🚀 Starting DecVCPlat Complete System Validation...\n');
    
    const results = {
      authentication: false,
      projectManagement: false,
      voting: false,
      signalr: false,
      blockchain: false,
      frontend: false,
      apiHealth: false
    };
    
    // Run all validations
    results.authentication = await this.validateAuthenticationFlow();
    
    if (results.authentication) {
      const projectId = await this.validateProjectManagement();
      results.projectManagement = projectId !== null;
      
      if (results.projectManagement) {
        const proposalId = await this.validateVotingSystem(projectId);
        results.voting = proposalId !== null;
      }
    }
    
    results.signalr = await this.validateSignalRNotifications();
    results.blockchain = await this.validateBlockchainIntegration();
    results.frontend = await this.validateFrontendAssets();
    results.apiHealth = await this.validateAPIHealth();
    
    // Generate Final Report
    console.log('\n📊 DecVCPlat System Validation Report:');
    console.log('==========================================');
    
    const passedTests = Object.values(results).filter(result => result).length;
    const totalTests = Object.keys(results).length;
    
    Object.entries(results).forEach(([test, passed]) => {
      console.log(`${passed ? '✅' : '❌'} ${test.charAt(0).toUpperCase() + test.slice(1).replace(/([A-Z])/g, ' $1')}: ${passed ? 'PASSED' : 'FAILED'}`);
    });
    
    console.log('==========================================');
    console.log(`Overall Success Rate: ${passedTests}/${totalTests} (${Math.round(passedTests/totalTests*100)}%)`);
    
    if (passedTests === totalTests) {
      console.log('🎉 DecVCPlat System is FULLY OPERATIONAL and PRODUCTION READY!');
      return true;
    } else {
      console.log('⚠️ DecVCPlat System has issues that need to be addressed before production deployment.');
      return false;
    }
  }
}

// Execute validation if run directly
if (require.main === module) {
  const validator = new DecVCPlatSystemValidator();
  validator.runCompleteValidation()
    .then(success => {
      process.exit(success ? 0 : 1);
    })
    .catch(error => {
      console.error('Fatal validation error:', error);
      process.exit(1);
    });
}

module.exports = DecVCPlatSystemValidator;
