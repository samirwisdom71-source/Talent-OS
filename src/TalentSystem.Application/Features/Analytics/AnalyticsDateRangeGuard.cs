using FluentValidation;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics;

internal static class AnalyticsDateRangeGuard
{
    public static async Task<Result> ValidateAsync(
        IValidator<AnalyticsDateRangeFilter> validator,
        AnalyticsDateRangeFilter? filter,
        CancellationToken cancellationToken)
    {
        if (filter is null || (!filter.FromUtc.HasValue && !filter.ToUtc.HasValue))
        {
            return Result.Ok();
        }

        var v = await validator.ValidateAsync(filter, cancellationToken).ConfigureAwait(false);
        return v.IsValid
            ? Result.Ok()
            : Result.Fail(v.Errors.Select(e => e.ErrorMessage).ToList());
    }
}
