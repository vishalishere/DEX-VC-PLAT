// © 2024 DecVCPlat. All rights reserved.

namespace Shared.Common.Enums;

public enum UserRole
{
    Founder = 1,
    Investor = 2,
    Luminary = 3
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    PendingVerification = 4,
    Blocked = 5
}

public enum ProjectStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5,
    Voting = 6,
    Funded = 7,
    InProgress = 8,
    Completed = 9,
    Cancelled = 10,
    Failed = 11
}

public enum MilestoneStatus
{
    Pending = 1,
    InProgress = 2,
    Submitted = 3,
    UnderReview = 4,
    Approved = 5,
    Rejected = 6,
    Completed = 7,
    Cancelled = 8
}

public enum VoteType
{
    Yes = 1,
    No = 2,
    Abstain = 3
}

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Disbursement = 3,
    Reward = 4,
    Stake = 5,
    Unstake = 6,
    Refund = 7
}
