namespace TalentSystem.Application.Common;

public static class ScoringErrors
{
    public const string TalentScoreNotFound = "Scoring.TalentScoreNotFound";

    public const string TalentScoreAlreadyExists = "Scoring.TalentScoreAlreadyExists";

    public const string CycleArchivedCannotScore = "Scoring.CycleArchivedCannotScore";

    public const string MissingPerformanceEvaluation = "Scoring.MissingPerformanceEvaluation";

    public const string MissingPotentialAssessment = "Scoring.MissingPotentialAssessment";

    public const string SourceNotFinalized = "Scoring.SourceNotFinalized";

    public const string PolicyNotFound = "Scoring.PolicyNotFound";

    public const string PolicyVersionDuplicate = "Scoring.PolicyVersionDuplicate";

    public const string PolicyMustBeInactiveToActivate = "Scoring.PolicyMustBeInactiveToActivate";
}
