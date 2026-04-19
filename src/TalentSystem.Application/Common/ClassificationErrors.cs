namespace TalentSystem.Application.Common;

public static class ClassificationErrors
{
    public const string ClassificationNotFound = "Classification.ClassificationNotFound";

    public const string ClassificationAlreadyExists = "Classification.ClassificationAlreadyExists";

    public const string CycleArchivedCannotClassify = "Classification.CycleArchivedCannotClassify";

    public const string TalentScoreRequired = "Classification.TalentScoreRequired";

    public const string RuleSetNotFound = "Classification.RuleSetNotFound";

    public const string RuleSetVersionDuplicate = "Classification.RuleSetVersionDuplicate";

    public const string RuleSetMustBeInactiveToActivate = "Classification.RuleSetMustBeInactiveToActivate";
}
