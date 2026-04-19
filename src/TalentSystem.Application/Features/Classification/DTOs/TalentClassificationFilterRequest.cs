using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Classification.DTOs;

public sealed class TalentClassificationFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? EmployeeId { get; set; }

    public Guid? PerformanceCycleId { get; set; }

    public NineBoxCode? NineBoxCode { get; set; }

    public bool? IsHighPotential { get; set; }

    public bool? IsHighPerformer { get; set; }

    public PerformanceBand? PerformanceBand { get; set; }

    public PotentialBand? PotentialBand { get; set; }
}
