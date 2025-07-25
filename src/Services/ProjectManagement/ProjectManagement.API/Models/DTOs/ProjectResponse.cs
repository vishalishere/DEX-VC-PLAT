// © 2024 DecVCPlat. All rights reserved.

using ProjectManagement.API.Models.Entities;

namespace ProjectManagement.API.Models.DTOs;

public class ProjectResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid FounderId { get; set; }
    public string FounderName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal FundingGoal { get; set; }
    public decimal CurrentFunding { get; set; }
    public int VotingPower { get; set; }
    public int TotalVotes { get; set; }
    public ProjectStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? FundingDeadline { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string? RiskAssessment { get; set; }
    public string? SmartContractAddress { get; set; }
    public bool IsSmartContractDeployed { get; set; }

    // Related data
    public List<MilestoneResponse> Milestones { get; set; } = new();
    public List<VoteResponse> Votes { get; set; } = new();
    public List<DocumentResponse> Documents { get; set; } = new();
    public List<CommentResponse> Comments { get; set; } = new();
    public List<FundingResponse> FundingRounds { get; set; } = new();

    // Calculated properties
    public decimal FundingProgress => FundingGoal > 0 ? (CurrentFunding / FundingGoal) * 100 : 0;
    public int DaysRemaining => FundingDeadline.HasValue ? Math.Max(0, (int)(FundingDeadline.Value - DateTime.UtcNow).TotalDays) : 0;
    public bool IsFundingActive => Status == ProjectStatus.Approved && DateTime.UtcNow < FundingDeadline;
}

public class MilestoneResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal FundingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? CompletionNotes { get; set; }
    public string? ProofDocumentUrl { get; set; }
    public string? BlockchainTransactionHash { get; set; }
    public bool IsFundingReleased { get; set; }
}

public class VoteResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid VoterId { get; set; }
    public string VoterName { get; set; } = string.Empty;
    public VoteType VoteType { get; set; }
    public decimal StakedTokens { get; set; }
    public decimal VotingPower { get; set; }
    public string? Comments { get; set; }
    public DateTime VotedAt { get; set; }
    public string? BlockchainTransactionHash { get; set; }
    public bool IsVerifiedOnChain { get; set; }
}

public class DocumentResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
}

public class CommentResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CommentType CommentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<CommentResponse> Replies { get; set; } = new();
}

public class FundingResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid InvestorId { get; set; }
    public string InvestorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public FundingType FundingType { get; set; }
    public FundingStatus Status { get; set; }
    public DateTime FundedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? TransactionHash { get; set; }
    public string? WalletAddress { get; set; }
    public bool IsVerifiedOnChain { get; set; }
    public Guid? MilestoneId { get; set; }
    public string? Notes { get; set; }
    public decimal StakedTokens { get; set; }
    public decimal VotingPowerGranted { get; set; }
}
