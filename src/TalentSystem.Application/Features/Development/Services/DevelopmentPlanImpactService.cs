using System.Text;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;
using TalentSystem.Domain.Potential;
using TalentSystem.Domain.Scoring;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Services;

public sealed class DevelopmentPlanImpactService : IDevelopmentPlanImpactService
{
    private readonly TalentDbContext _db;

    public DevelopmentPlanImpactService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<DevelopmentPlanImpactSnapshotDto>>> ListAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.DevelopmentPlans.AsNoTracking().AnyAsync(x => x.Id == planId, cancellationToken))
        {
            return Result<IReadOnlyList<DevelopmentPlanImpactSnapshotDto>>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        var rows = await _db.DevelopmentPlanImpactSnapshots.AsNoTracking()
            .Where(x => x.DevelopmentPlanId == planId)
            .OrderBy(x => x.Phase)
            .Select(x => new DevelopmentPlanImpactSnapshotDto
            {
                Id = x.Id,
                DevelopmentPlanId = x.DevelopmentPlanId,
                Phase = x.Phase,
                RecordedOnUtc = x.RecordedOnUtc,
                SummaryNotes = x.SummaryNotes,
                MetricScore = x.MetricScore,
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<DevelopmentPlanImpactSnapshotDto>>.Ok(rows);
    }

    public async Task<Result<DevelopmentPlanImpactSnapshotDto>> ComputeAndPersistAsync(
        Guid planId,
        DevelopmentImpactPhase phase,
        CancellationToken cancellationToken = default)
    {
        var plan = await _db.DevelopmentPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == planId, cancellationToken);

        if (plan is null)
        {
            return Result<DevelopmentPlanImpactSnapshotDto>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        var employee = await _db.Employees.AsNoTracking()
            .Where(x => x.Id == plan.EmployeeId)
            .Select(x => new { x.FullNameAr, x.FullNameEn })
            .FirstAsync(cancellationToken);

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .Where(x => x.Id == plan.PerformanceCycleId)
            .Select(x => new { x.NameAr, x.NameEn })
            .FirstAsync(cancellationToken);

        var ts = await _db.TalentScores.AsNoTracking()
            .Where(x =>
                x.EmployeeId == plan.EmployeeId &&
                x.PerformanceCycleId == plan.PerformanceCycleId &&
                x.RecordStatus != RecordStatus.Deleted)
            .OrderByDescending(x => x.CalculatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var ev = await _db.PerformanceEvaluations.AsNoTracking()
            .Where(x =>
                x.EmployeeId == plan.EmployeeId &&
                x.PerformanceCycleId == plan.PerformanceCycleId &&
                x.RecordStatus != RecordStatus.Deleted)
            .OrderByDescending(x => x.EvaluatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var pa = await _db.PotentialAssessments.AsNoTracking()
            .Where(x =>
                x.EmployeeId == plan.EmployeeId &&
                x.PerformanceCycleId == plan.PerformanceCycleId &&
                x.Status == PotentialAssessmentStatus.Finalized &&
                x.RecordStatus != RecordStatus.Deleted)
            .OrderByDescending(x => x.AssessedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var metric = ResolveCompositeMetric(ts, ev, pa);
        var summary = BuildArabicSummary(phase, employee.FullNameAr, cycle.NameAr, ts, ev, pa);

        var recorded = DateTime.UtcNow;

        var existing = await _db.DevelopmentPlanImpactSnapshots
            .FirstOrDefaultAsync(
                x => x.DevelopmentPlanId == planId && x.Phase == phase,
                cancellationToken);

        if (existing is null)
        {
            existing = new DevelopmentPlanImpactSnapshot
            {
                DevelopmentPlanId = planId,
                Phase = phase,
                RecordedOnUtc = recorded,
                SummaryNotes = summary,
                MetricScore = metric,
            };
            _db.DevelopmentPlanImpactSnapshots.Add(existing);
        }
        else
        {
            existing.RecordedOnUtc = recorded;
            existing.SummaryNotes = summary;
            existing.MetricScore = metric;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanImpactSnapshotDto>.Ok(new DevelopmentPlanImpactSnapshotDto
        {
            Id = existing.Id,
            DevelopmentPlanId = existing.DevelopmentPlanId,
            Phase = existing.Phase,
            RecordedOnUtc = existing.RecordedOnUtc,
            SummaryNotes = existing.SummaryNotes,
            MetricScore = existing.MetricScore,
        });
    }

    private static decimal? ResolveCompositeMetric(
        TalentScore? ts,
        PerformanceEvaluation? ev,
        PotentialAssessment? pa)
    {
        if (ts is not null)
        {
            return Math.Round(ts.FinalScore, 2, MidpointRounding.AwayFromZero);
        }

        var parts = new List<decimal>();
        if (ev is not null)
        {
            parts.Add(ev.OverallScore);
        }

        if (pa is not null)
        {
            parts.Add(pa.OverallPotentialScore);
        }

        if (parts.Count == 0)
        {
            return null;
        }

        return Math.Round(parts.Average(), 2, MidpointRounding.AwayFromZero);
    }

    private static string BuildArabicSummary(
        DevelopmentImpactPhase phase,
        string employeeNameAr,
        string cycleNameAr,
        TalentScore? ts,
        PerformanceEvaluation? ev,
        PotentialAssessment? pa)
    {
        var sb = new StringBuilder();
        sb.AppendLine(phase == DevelopmentImpactPhase.Before
            ? "لقطة أساس (قبل): تُحسب تلقائياً من بيانات النظام عند تفعيل الخطة."
            : "لقطة إكمال (بعد): تُحسب تلقائياً من بيانات النظام عند إكمال الخطة.");
        sb.AppendLine($"الموظف: {employeeNameAr} — الدورة: {cycleNameAr}");
        sb.AppendLine();

        if (ts is not null)
        {
            sb.AppendLine("— درجة المواهب المحسوبة —");
            sb.AppendLine($"النهائي: {ts.FinalScore:0.##} | أداء: {ts.PerformanceScore:0.##} | إمكانات: {ts.PotentialScore:0.##}");
            sb.AppendLine($"الأوزان (أداء/إمكانات): {ts.PerformanceWeight:0.##} / {ts.PotentialWeight:0.##} — الإصدار: {ts.CalculationVersion}");
            sb.AppendLine($"تاريخ الحساب: {ts.CalculatedOnUtc:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine();
        }

        if (ev is not null)
        {
            sb.AppendLine("— تقييم الأداء (آخر سجل للدورة) —");
            sb.AppendLine($"الدرجة الإجمالية: {ev.OverallScore:0.##} | الحالة: {ev.Status}");
            if (ev.EvaluatedOnUtc is { } evDt)
            {
                sb.AppendLine($"تاريخ التقييم: {evDt:yyyy-MM-dd HH:mm} UTC");
            }

            sb.AppendLine();
        }

        if (pa is not null)
        {
            sb.AppendLine("— تقييم الإمكانات (معتمد) —");
            sb.AppendLine(
                $"الإجمالي: {pa.OverallPotentialScore:0.##} | المستوى: {pa.PotentialLevel} | الحالة: {pa.Status}");
            if (pa.AssessedOnUtc is { } paDt)
            {
                sb.AppendLine($"تاريخ التقييم: {paDt:yyyy-MM-dd HH:mm} UTC");
            }

            sb.AppendLine();
        }

        if (ts is null && ev is null && pa is null)
        {
            sb.AppendLine(
                "لا توجد حالياً درجة مواهب أو تقييم أداء أو تقييم إمكانات معتمد مرتبط بهذه الدورة في النظام؛ لذلك المؤشر المركّب قد يكون فارغاً.");
        }

        return sb.ToString().Trim();
    }
}
