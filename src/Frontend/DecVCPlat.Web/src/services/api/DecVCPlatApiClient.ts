// © 2024 DecVCPlat. All rights reserved.

import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

class DecVCPlatApiClientService {
  private decvcplatApiInstance: AxiosInstance;
  private decvcplatBaseUrl: string;
  private decvcplatAuthToken: string | null = null;

  constructor() {
    this.decvcplatBaseUrl = process.env.REACT_APP_DECVCPLAT_API_URL || 'http://localhost:5000';
    this.decvcplatApiInstance = axios.create({
      baseURL: this.decvcplatBaseUrl,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
        'X-DecVCPlat-Client': 'Web-Frontend',
      },
    });

    this.initializeDecVCPlatRequestInterceptor();
    this.initializeDecVCPlatResponseInterceptor();
  }

  private initializeDecVCPlatRequestInterceptor(): void {
    this.decvcplatApiInstance.interceptors.request.use(
      (decvcplatConfig) => {
        if (this.decvcplatAuthToken) {
          decvcplatConfig.headers.Authorization = `Bearer ${this.decvcplatAuthToken}`;
        }
        console.log(`DecVCPlat API Request: ${decvcplatConfig.method?.toUpperCase()} ${decvcplatConfig.url}`);
        return decvcplatConfig;
      },
      (decvcplatError) => {
        console.error('DecVCPlat API Request Error:', decvcplatError);
        return Promise.reject(decvcplatError);
      }
    );
  }

  private initializeDecVCPlatResponseInterceptor(): void {
    this.decvcplatApiInstance.interceptors.response.use(
      (decvcplatResponse: AxiosResponse) => {
        console.log(`DecVCPlat API Response: ${decvcplatResponse.status} ${decvcplatResponse.config.url}`);
        return decvcplatResponse;
      },
      (decvcplatError) => {
        console.error('DecVCPlat API Response Error:', decvcplatError);
        
        if (decvcplatError.response?.status === 401) {
          this.clearDecVCPlatAuthentication();
          window.location.href = '/login';
        }
        
        return Promise.reject(decvcplatError);
      }
    );
  }

  setDecVCPlatAuthenticationToken(decvcplatToken: string): void {
    this.decvcplatAuthToken = decvcplatToken;
  }

  clearDecVCPlatAuthentication(): void {
    this.decvcplatAuthToken = null;
    localStorage.removeItem('decvcplat_token');
    localStorage.removeItem('decvcplat_user');
  }

  // DecVCPlat Authentication API Methods
  async authenticateDecVCPlatUser(decvcplatCredentials: { email: string; password: string }): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/auth/login', decvcplatCredentials);
    return decvcplatResponse.data;
  }

  async registerDecVCPlatUser(decvcplatUserData: { 
    userName: string; 
    email: string; 
    fullName: string; 
    password: string; 
    role: string; 
  }): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/auth/register', decvcplatUserData);
    return decvcplatResponse.data;
  }

  async refreshDecVCPlatToken(): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/auth/refresh');
    return decvcplatResponse.data;
  }

  async updateDecVCPlatUserProfile(decvcplatProfileData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.put('/auth/profile', decvcplatProfileData);
    return decvcplatResponse.data;
  }

  // DecVCPlat Project API Methods
  async fetchDecVCPlatProjects(decvcplatQueryParams?: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.get('/projects', { params: decvcplatQueryParams });
    return decvcplatResponse.data;
  }

  async fetchDecVCPlatProjectById(decvcplatProjectId: string): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.get(`/projects/${decvcplatProjectId}`);
    return decvcplatResponse.data;
  }

  async createDecVCPlatProject(decvcplatProjectData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/projects', decvcplatProjectData);
    return decvcplatResponse.data;
  }

  async updateDecVCPlatProject(decvcplatProjectId: string, decvcplatUpdateData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.put(`/projects/${decvcplatProjectId}`, decvcplatUpdateData);
    return decvcplatResponse.data;
  }

  async voteOnDecVCPlatProject(decvcplatProjectId: string, decvcplatVoteData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post(`/projects/${decvcplatProjectId}/vote`, decvcplatVoteData);
    return decvcplatResponse.data;
  }

  // DecVCPlat Voting API Methods
  async fetchDecVCPlatProposals(decvcplatQueryParams?: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.get('/voting/proposals', { params: decvcplatQueryParams });
    return decvcplatResponse.data;
  }

  async fetchDecVCPlatProposalById(decvcplatProposalId: string): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.get(`/voting/proposals/${decvcplatProposalId}`);
    return decvcplatResponse.data;
  }

  async createDecVCPlatProposal(decvcplatProposalData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/voting/proposals', decvcplatProposalData);
    return decvcplatResponse.data;
  }

  async stakeDecVCPlatTokens(decvcplatStakeData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/voting/stake', decvcplatStakeData);
    return decvcplatResponse.data;
  }

  async castDecVCPlatVote(decvcplatVoteData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/voting/vote', decvcplatVoteData);
    return decvcplatResponse.data;
  }

  // DecVCPlat Funding API Methods
  async fetchDecVCPlatFundingTranches(decvcplatQueryParams?: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.get('/funding/tranches', { params: decvcplatQueryParams });
    return decvcplatResponse.data;
  }

  async createDecVCPlatFundingTranche(decvcplatTrancheData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/funding/tranches', decvcplatTrancheData);
    return decvcplatResponse.data;
  }

  async releaseDecVCPlatFunds(decvcplatReleaseData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/funding/release', decvcplatReleaseData);
    return decvcplatResponse.data;
  }

  async approveDecVCPlatFundingRelease(decvcplatApprovalData: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/funding/approve', decvcplatApprovalData);
    return decvcplatResponse.data;
  }

  // DecVCPlat Notification API Methods
  async fetchDecVCPlatNotifications(decvcplatQueryParams?: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.get('/notifications', { params: decvcplatQueryParams });
    return decvcplatResponse.data;
  }

  async markDecVCPlatNotificationsAsRead(decvcplatNotificationIds: string[]): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.post('/notifications/mark-read', { notificationIds: decvcplatNotificationIds });
    return decvcplatResponse.data;
  }

  async updateDecVCPlatNotificationPreferences(decvcplatPreferences: any): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.put('/notifications/preferences', decvcplatPreferences);
    return decvcplatResponse.data;
  }

  // DecVCPlat Generic API Method
  async makeDecVCPlatApiCall(decvcplatMethod: 'GET' | 'POST' | 'PUT' | 'DELETE', decvcplatEndpoint: string, decvcplatData?: any, decvcplatConfig?: AxiosRequestConfig): Promise<any> {
    const decvcplatResponse = await this.decvcplatApiInstance.request({
      method: decvcplatMethod,
      url: decvcplatEndpoint,
      data: decvcplatData,
      ...decvcplatConfig,
    });
    return decvcplatResponse.data;
  }
}

export const decvcplatApiService = new DecVCPlatApiClientService();
export default decvcplatApiService;
