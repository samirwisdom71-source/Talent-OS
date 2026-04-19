namespace TalentSystem.Application.Common;

public static class CompetencyErrors
{
    public const string CategoryNotFound = "Competency.CategoryNotFound";

    public const string CompetencyNotFound = "Competency.NotFound";

    public const string DuplicateCompetencyCode = "Competency.DuplicateCode";

    public const string CompetencyCategoryMissing = "Competency.CategoryMissing";

    public const string LevelNotFound = "CompetencyLevel.NotFound";

    public const string DuplicateCompetencyLevelNumericValue = "CompetencyLevel.DuplicateNumericValue";

    public const string JobRequirementNotFound = "JobCompetencyRequirement.NotFound";

    public const string DuplicateJobRequirement = "JobCompetencyRequirement.DuplicatePositionCompetency";

    public const string JobRequirementPositionNotFound = "JobCompetencyRequirement.PositionNotFound";

    public const string JobRequirementCompetencyNotFound = "JobCompetencyRequirement.CompetencyNotFound";

    public const string JobRequirementRequiredLevelNotFound = "JobCompetencyRequirement.RequiredLevelNotFound";
}
