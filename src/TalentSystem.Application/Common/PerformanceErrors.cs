namespace TalentSystem.Application.Common;

public static class PerformanceErrors
{
    public const string CycleNotFound = "Performance.CycleNotFound";

    public const string CycleReadOnly = "Performance.CycleReadOnly";

    public const string CycleMustBeDraftToActivate = "Performance.CycleMustBeDraftToActivate";

    public const string CycleMustBeActiveToClose = "Performance.CycleMustBeActiveToClose";

    public const string CycleInvalidDateRange = "Performance.CycleInvalidDateRange";

    public const string EmployeeNotFound = "Performance.EmployeeNotFound";

    public const string GoalNotFound = "Performance.GoalNotFound";

    public const string GoalReadOnly = "Performance.GoalReadOnly";

    public const string GoalCycleNotOpen = "Performance.GoalCycleNotOpen";

    public const string GoalWeightExceeded = "Performance.GoalWeightExceeded";

    public const string EvaluationNotFound = "Performance.EvaluationNotFound";

    public const string EvaluationDuplicate = "Performance.EvaluationDuplicate";

    public const string EvaluationCycleClosed = "Performance.EvaluationCycleClosed";

    public const string EvaluationReadOnly = "Performance.EvaluationReadOnly";
}
