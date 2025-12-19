namespace SRMS.Domain.Identity.Enums;

public enum UserProfileStatus
{
    PendingAccountTypeSelection,
    PendingProfileCompletion,
    PendingApproval,
    Active,
    Rejected,
    Suspended
}
