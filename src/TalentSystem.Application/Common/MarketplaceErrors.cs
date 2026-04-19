namespace TalentSystem.Application.Common;

public static class MarketplaceErrors
{
    public const string OpportunityNotFound = "Marketplace.OpportunityNotFound";

    public const string OpportunityReadOnly = "Marketplace.OpportunityReadOnly";

    public const string OpportunityNotOpen = "Marketplace.OpportunityNotOpen";

    public const string InvalidOpportunityStatusTransition = "Marketplace.InvalidOpportunityStatusTransition";

    public const string ApplicationNotFound = "Marketplace.ApplicationNotFound";

    public const string ApplicationReadOnly = "Marketplace.ApplicationReadOnly";

    public const string ApplicationDuplicate = "Marketplace.ApplicationDuplicate";

    public const string MaxApplicantsReached = "Marketplace.MaxApplicantsReached";

    public const string ApplicantAlreadyInTargetRole = "Marketplace.ApplicantAlreadyInTargetRole";

    public const string InvalidApplicationStatusTransition = "Marketplace.InvalidApplicationStatusTransition";
}
