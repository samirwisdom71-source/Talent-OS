using System.Text;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Intelligence;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Services;

public sealed class DevelopmentPlanSuggestionService : IDevelopmentPlanSuggestionService
{
    private readonly TalentDbContext _db;

    public DevelopmentPlanSuggestionService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DevelopmentPlanSuggestionDto>> SuggestAsync(
        SuggestDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<DevelopmentPlanSuggestionDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        if (!await _db.PerformanceCycles.AsNoTracking().AnyAsync(x => x.Id == request.PerformanceCycleId, cancellationToken))
        {
            return Result<DevelopmentPlanSuggestionDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!Enum.IsDefined(typeof(DevelopmentPlanSourceType), request.SourceType))
        {
            return Result<DevelopmentPlanSuggestionDto>.Fail("Invalid source type.");
        }

        var ctx = await LoadEmployeePlanContextAsync(request.EmployeeId, request.PerformanceCycleId, cancellationToken);

        return request.SourceType switch
        {
            DevelopmentPlanSourceType.Manual => Result<DevelopmentPlanSuggestionDto>.Ok(ManualTemplate(ctx)),
            DevelopmentPlanSourceType.CompetencyGap => Result<DevelopmentPlanSuggestionDto>.Ok(
                await SuggestCompetencyGapsAsync(ctx, cancellationToken)),
            DevelopmentPlanSourceType.Succession => Result<DevelopmentPlanSuggestionDto>.Ok(
                await SuggestSuccessionAsync(ctx, cancellationToken)),
            DevelopmentPlanSourceType.Performance => Result<DevelopmentPlanSuggestionDto>.Ok(
                await SuggestPerformanceAsync(ctx, cancellationToken)),
            DevelopmentPlanSourceType.Potential => Result<DevelopmentPlanSuggestionDto>.Ok(
                await SuggestPotentialAsync(ctx, cancellationToken)),
            _ => Result<DevelopmentPlanSuggestionDto>.Ok(ManualTemplate(ctx)),
        };
    }

    private sealed record EmployeePlanContext(
        Guid EmployeeId,
        string EmployeeNameAr,
        string EmployeeNameEn,
        string PositionTitleAr,
        string PositionTitleEn,
        string OrgUnitNameAr,
        string OrgUnitNameEn,
        Guid PerformanceCycleId,
        string CycleNameAr,
        string CycleNameEn,
        DateTime CycleStart,
        DateTime CycleEnd);

    private async Task<EmployeePlanContext> LoadEmployeePlanContextAsync(
        Guid employeeId,
        Guid cycleId,
        CancellationToken cancellationToken)
    {
        var row = await _db.Employees.AsNoTracking()
            .Where(x => x.Id == employeeId)
            .Select(x => new
            {
                x.FullNameAr,
                x.FullNameEn,
                PosAr = x.Position.TitleAr,
                PosEn = x.Position.TitleEn,
                OuAr = x.OrganizationUnit.NameAr,
                OuEn = x.OrganizationUnit.NameEn,
            })
            .FirstAsync(cancellationToken);

        var cy = await _db.PerformanceCycles.AsNoTracking()
            .Where(x => x.Id == cycleId)
            .Select(x => new { x.NameAr, x.NameEn, x.StartDate, x.EndDate })
            .FirstAsync(cancellationToken);

        return new EmployeePlanContext(
            employeeId,
            row.FullNameAr,
            row.FullNameEn,
            row.PosAr,
            row.PosEn,
            row.OuAr,
            row.OuEn,
            cycleId,
            cy.NameAr,
            cy.NameEn,
            cy.StartDate,
            cy.EndDate);
    }

    private async Task<IReadOnlyList<TalentRecommendationRow>> LoadActiveRecommendationsAsync(
        Guid employeeId,
        Guid cycleId,
        CancellationToken cancellationToken)
    {
        return await _db.TalentRecommendations.AsNoTracking()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.RecordStatus != RecordStatus.Deleted &&
                x.Status == TalentRecommendationStatus.Active &&
                (x.PerformanceCycleId == null || x.PerformanceCycleId == cycleId))
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.ConfidenceScore)
            .ThenByDescending(x => x.GeneratedOnUtc)
            .Take(5)
            .Select(x => new TalentRecommendationRow(
                x.Title,
                x.Description,
                x.RecommendedAction,
                x.Priority,
                x.GeneratedOnUtc))
            .ToListAsync(cancellationToken);
    }

    private sealed record TalentRecommendationRow(
        string Title,
        string Description,
        string RecommendedAction,
        RecommendationPriority Priority,
        DateTime GeneratedOnUtc);

    private static DevelopmentPlanSuggestionDto ManualTemplate(EmployeePlanContext ctx) =>
        new()
        {
            PlanTitle = $"خطة تطوير — {ctx.EmployeeNameAr}",
            Notes = BuildContextPreamble(ctx, "أضف البنود والمسارات يدوياً، أو استخدم «اقتراح من النظام» حسب مصدر الخطة لاستخراج مسودة من بيانات المنصب والدورة والتوصيات."),
            Items = Array.Empty<DevelopmentPlanStructuredItemInputDto>(),
            Links = Array.Empty<DevelopmentPlanLinkInputDto>(),
        };

    private async Task<DevelopmentPlanSuggestionDto> SuggestCompetencyGapsAsync(
        EmployeePlanContext ctx,
        CancellationToken cancellationToken)
    {
        var positionId = await _db.Employees.AsNoTracking()
            .Where(x => x.Id == ctx.EmployeeId)
            .Select(x => x.PositionId)
            .FirstAsync(cancellationToken);

        var reqs = await _db.JobCompetencyRequirements.AsNoTracking()
            .Where(x => x.PositionId == positionId)
            .OrderBy(x => x.Competency.NameAr)
            .Take(8)
            .Select(x => new
            {
                x.CompetencyId,
                NameAr = x.Competency.NameAr,
                NameEn = x.Competency.NameEn,
                LevelName = x.RequiredLevel.Name,
                LevelValue = x.RequiredLevel.NumericValue,
            })
            .ToListAsync(cancellationToken);

        var recs = await LoadActiveRecommendationsAsync(ctx.EmployeeId, ctx.PerformanceCycleId, cancellationToken);

        var items = new List<DevelopmentPlanStructuredItemInputDto>();
        foreach (var r in reqs)
        {
            items.Add(new DevelopmentPlanStructuredItemInputDto
            {
                Title = $"سد فجوة: {r.NameAr}",
                Description =
                    $"منصب «{ctx.PositionTitleAr}» يتطلب لهذه الكفاءة مستوى {r.LevelName} (قيمة {r.LevelValue}). " +
                    $"المرجع الإنجليزي: {r.NameEn}. ركّز الأنشطة والتدريب على الوصول لهذا المستوى.",
                ItemType = DevelopmentItemType.Training,
                RelatedCompetencyId = r.CompetencyId,
                TargetDate = DateTime.UtcNow.Date.AddMonths(3),
                Paths = DefaultPaths(),
            });
        }

        if (items.Count == 0)
        {
            items.Add(new DevelopmentPlanStructuredItemInputDto
            {
                Title = "مراجعة متطلبات الكفاءات للمنصب",
                Description =
                    "لا توجد متطلبات كفاءات مسجلة لمنصب الموظف في النظام. راجع هيك الوظائف أو سجّل المتطلبات ثم أعد الاقتراح.",
                ItemType = DevelopmentItemType.Other,
                Paths = DefaultPaths(),
            });
        }

        var notes = new StringBuilder();
        notes.AppendLine(BuildContextPreamble(ctx,
            "يستند الاقتراح إلى متطلبات الكفاءات المعرفة لمنصب الموظف الحالي (مستوى مطلوب لكل كفاءة)."));
        if (reqs.Count > 0)
        {
            notes.AppendLine();
            notes.AppendLine("ملخص المتطلبات المأخوذة في البنود:");
            foreach (var r in reqs)
            {
                notes.AppendLine($"• {r.NameAr} — مستوى مطلوب: {r.LevelName}");
            }
        }

        AppendRecommendationsSection(notes, recs);

        var planTitle = recs.Count > 0
            ? $"{recs[0].Title} — خطة كفاءات ({ctx.EmployeeNameAr})"
            : $"خطة تطوير الكفاءات — {ctx.EmployeeNameAr} · {ctx.PositionTitleAr}";

        return new DevelopmentPlanSuggestionDto
        {
            PlanTitle = planTitle,
            Notes = notes.ToString().Trim(),
            Items = items,
            Links = Array.Empty<DevelopmentPlanLinkInputDto>(),
        };
    }

    private async Task<DevelopmentPlanSuggestionDto> SuggestSuccessionAsync(
        EmployeePlanContext ctx,
        CancellationToken cancellationToken)
    {
        var row = await _db.SuccessorCandidates.AsNoTracking()
            .Where(x => x.EmployeeId == ctx.EmployeeId)
            .OrderByDescending(x => x.IsPrimarySuccessor)
            .ThenBy(x => x.RankOrder)
            .Select(x => new { x.SuccessionPlanId, x.SuccessionPlan.PlanName, x.ReadinessLevel, x.Notes })
            .FirstOrDefaultAsync(cancellationToken);

        var recs = await LoadActiveRecommendationsAsync(ctx.EmployeeId, ctx.PerformanceCycleId, cancellationToken);

        var items = new List<DevelopmentPlanStructuredItemInputDto>();
        var links = new List<DevelopmentPlanLinkInputDto>();

        if (row is not null)
        {
            links.Add(new DevelopmentPlanLinkInputDto
            {
                LinkType = DevelopmentPlanLinkType.SuccessionPlan,
                LinkedEntityId = row.SuccessionPlanId,
                Notes = "ربط بخطة التعاقب",
            });

            var readiness = ReadinessLevelAr(row.ReadinessLevel);
            var desc = string.IsNullOrWhiteSpace(row.Notes)
                ? $"الموظف: {ctx.EmployeeNameAr}. خطة التعاقب: «{row.PlanName}». مستوى الجاهزية المسجل: {readiness}."
                : $"{row.Notes.Trim()} — السياق: {ctx.EmployeeNameAr}، {ctx.PositionTitleAr}، دورة {ctx.CycleNameAr}.";

            items.Add(new DevelopmentPlanStructuredItemInputDto
            {
                Title = $"جاهزية التعاقب — {row.PlanName}",
                Description = desc,
                ItemType = DevelopmentItemType.StretchAssignment,
                Paths = DefaultPaths(),
            });
        }
        else
        {
            items.Add(new DevelopmentPlanStructuredItemInputDto
            {
                Title = "تطوير مسار التعاقب",
                Description =
                    $"لا يوجد سجل مرشح تعاقب لـ {ctx.EmployeeNameAr} في النظام. يمكن ربط الخطة لاحقاً عند تسجيله كمرشح، أو استكمال تطوير المهارات العامة للمنصب الحالي ({ctx.PositionTitleAr}).",
                ItemType = DevelopmentItemType.JobRotation,
                Paths = DefaultPaths(),
            });
        }

        var notes = new StringBuilder();
        notes.AppendLine(BuildContextPreamble(ctx, "مقترح مرتبط ببيانات التعاقب والمرشحين في النظام."));
        AppendRecommendationsSection(notes, recs);

        var planTitle = recs.Count > 0
            ? $"{recs[0].Title} — تعاقب ({ctx.EmployeeNameAr})"
            : row is null
                ? $"خطة تطوير التعاقب — {ctx.EmployeeNameAr}"
                : $"خطة التعاقب — {row.PlanName} · {ctx.EmployeeNameAr}";

        return new DevelopmentPlanSuggestionDto
        {
            PlanTitle = planTitle,
            Notes = notes.ToString().Trim(),
            Items = items,
            Links = links,
        };
    }

    private async Task<DevelopmentPlanSuggestionDto> SuggestPerformanceAsync(
        EmployeePlanContext ctx,
        CancellationToken cancellationToken)
    {
        var ev = await _db.PerformanceEvaluations.AsNoTracking()
            .Where(x => x.EmployeeId == ctx.EmployeeId && x.PerformanceCycleId == ctx.PerformanceCycleId)
            .OrderByDescending(x => x.EvaluatedOnUtc)
            .Select(x => new
            {
                x.Id,
                x.OverallScore,
                x.ManagerComments,
                x.Status,
                x.EvaluatedOnUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        var recs = await LoadActiveRecommendationsAsync(ctx.EmployeeId, ctx.PerformanceCycleId, cancellationToken);

        var items = new List<DevelopmentPlanStructuredItemInputDto>();
        var links = new List<DevelopmentPlanLinkInputDto>();

        if (ev is not null)
        {
            links.Add(new DevelopmentPlanLinkInputDto
            {
                LinkType = DevelopmentPlanLinkType.PerformanceEvaluation,
                LinkedEntityId = ev.Id,
                Notes = "ربط بتقييم الأداء",
            });

            var statusLabel = ev.Status.ToString();
            var comment = string.IsNullOrWhiteSpace(ev.ManagerComments)
                ? $"لا توجد ملاحظات مدير في السجل؛ الدرجة الإجمالية: {ev.OverallScore:0.##}. حالة التقييم: {statusLabel}."
                : ev.ManagerComments.Trim();
            if (comment.Length > 1200)
            {
                comment = comment[..1200] + "…";
            }

            items.Add(new DevelopmentPlanStructuredItemInputDto
            {
                Title = $"متابعة الأداء — دورة {ctx.CycleNameAr}",
                Description =
                    $"الموظف: {ctx.EmployeeNameAr} ({ctx.PositionTitleAr}). " +
                    $"الدرجة الإجمالية: {ev.OverallScore:0.##}. تاريخ التقييم: {ev.EvaluatedOnUtc:yyyy-MM-dd}. " +
                    $"ملاحظات المدير: {comment}",
                ItemType = DevelopmentItemType.Coaching,
                Paths = DefaultPaths(),
            });
        }
        else
        {
            items.Add(new DevelopmentPlanStructuredItemInputDto
            {
                Title = "تهيئة أهداف الأداء للدورة",
                Description =
                    $"لا يوجد تقييم أداء مسجل لـ {ctx.EmployeeNameAr} ضمن دورة «{ctx.CycleNameAr}». أنشئ التقييم أولاً ثم أعد الاقتراح لربط الخطة بالنتائج الفعلية.",
                ItemType = DevelopmentItemType.Other,
                Paths = DefaultPaths(),
            });
        }

        var notes = new StringBuilder();
        notes.AppendLine(BuildContextPreamble(ctx,
            ev is not null
                ? $"يستند الاقتراح إلى أحدث تقييم أداء مسجل للدورة (درجة إجمالية {ev.OverallScore:0.##})."
                : "لا يوجد تقييم أداء بعد لهذه الدورة؛ البنود توضيحية إلى حين إنشاء التقييم."));
        AppendRecommendationsSection(notes, recs);

        var planTitle = recs.Count > 0
            ? $"{recs[0].Title} — أداء ({ctx.EmployeeNameAr})"
            : ev is not null
                ? $"خطة تطوير الأداء — {ctx.EmployeeNameAr} · درجة {ev.OverallScore:0.#}"
                : $"خطة تطوير الأداء — {ctx.EmployeeNameAr} · {ctx.CycleNameAr}";

        return new DevelopmentPlanSuggestionDto
        {
            PlanTitle = planTitle,
            Notes = notes.ToString().Trim(),
            Items = items,
            Links = links,
        };
    }

    private async Task<DevelopmentPlanSuggestionDto> SuggestPotentialAsync(
        EmployeePlanContext ctx,
        CancellationToken cancellationToken)
    {
        var assessment = await _db.PotentialAssessments.AsNoTracking()
            .Where(x =>
                x.EmployeeId == ctx.EmployeeId &&
                x.PerformanceCycleId == ctx.PerformanceCycleId &&
                x.Status == PotentialAssessmentStatus.Finalized)
            .OrderByDescending(x => x.AssessedOnUtc)
            .Select(x => new { x.Id, x.Comments, x.AssessedOnUtc })
            .FirstOrDefaultAsync(cancellationToken);

        var recs = await LoadActiveRecommendationsAsync(ctx.EmployeeId, ctx.PerformanceCycleId, cancellationToken);

        var items = new List<DevelopmentPlanStructuredItemInputDto>();
        var links = new List<DevelopmentPlanLinkInputDto>();

        if (assessment is not null)
        {
            links.Add(new DevelopmentPlanLinkInputDto
            {
                LinkType = DevelopmentPlanLinkType.PotentialAssessment,
                LinkedEntityId = assessment.Id,
            });

            var factors = await _db.PotentialAssessmentFactors.AsNoTracking()
                .Where(x => x.PotentialAssessmentId == assessment.Id)
                .OrderBy(x => x.Score)
                .Take(5)
                .Select(x => new { x.FactorName, x.Score })
                .ToListAsync(cancellationToken);

            foreach (var f in factors)
            {
                items.Add(new DevelopmentPlanStructuredItemInputDto
                {
                    Title = $"رفع إمكانات — {f.FactorName}",
                    Description =
                        $"أضعف العوامل حسب السجل: {f.FactorName} بدرجة {f.Score:0.##}. " +
                        $"الموظف: {ctx.EmployeeNameAr}، المنصب: {ctx.PositionTitleAr}. ركّز التدريب والإرشاد على هذا العامل.",
                    ItemType = DevelopmentItemType.Mentoring,
                    Paths = DefaultPaths(),
                });
            }

            if (items.Count == 0 && !string.IsNullOrWhiteSpace(assessment.Comments))
            {
                items.Add(new DevelopmentPlanStructuredItemInputDto
                {
                    Title = "متابعة تقييم الإمكانات",
                    Description = assessment.Comments.Trim(),
                    ItemType = DevelopmentItemType.SelfLearning,
                    Paths = DefaultPaths(),
                });
            }
        }

        if (items.Count == 0)
        {
            items.Add(new DevelopmentPlanStructuredItemInputDto
            {
                Title = "تطوير الإمكانات",
                Description =
                    $"لا يوجد تقييم إمكانات معتمد لـ {ctx.EmployeeNameAr} في دورة «{ctx.CycleNameAr}». أنشئ التقييم واعتمده ثم أعد الاقتراح.",
                ItemType = DevelopmentItemType.Other,
                Paths = DefaultPaths(),
            });
        }

        var notes = new StringBuilder();
        notes.AppendLine(BuildContextPreamble(ctx,
            assessment is not null
                ? $"يستند الاقتراح إلى تقييم إمكانات معتمد (تاريخ {assessment.AssessedOnUtc:yyyy-MM-dd}) مع التركيز على أضعف العوامل."
                : "لا يوجد تقييم إمكانات معتمد بعد؛ البنود توضيحية."));
        AppendRecommendationsSection(notes, recs);

        var planTitle = recs.Count > 0
            ? $"{recs[0].Title} — إمكانات ({ctx.EmployeeNameAr})"
            : $"خطة تطوير الإمكانات — {ctx.EmployeeNameAr} · {ctx.CycleNameAr}";

        return new DevelopmentPlanSuggestionDto
        {
            PlanTitle = planTitle,
            Notes = notes.ToString().Trim(),
            Items = items,
            Links = links,
        };
    }

    private static string BuildContextPreamble(EmployeePlanContext ctx, string interpretationLine)
    {
        var sb = new StringBuilder();
        sb.AppendLine("【سياق من النظام】");
        sb.AppendLine($"• الموظف: {ctx.EmployeeNameAr} ({ctx.EmployeeNameEn})");
        sb.AppendLine($"• المنصب: {ctx.PositionTitleAr} — {ctx.PositionTitleEn}");
        sb.AppendLine($"• الوحدة: {ctx.OrgUnitNameAr} — {ctx.OrgUnitNameEn}");
        sb.AppendLine(
            $"• دورة الأداء: {ctx.CycleNameAr} ({ctx.CycleStart:yyyy-MM-dd} → {ctx.CycleEnd:yyyy-MM-dd}) — {ctx.CycleNameEn}");
        sb.AppendLine();
        sb.AppendLine("【التفسير】");
        sb.AppendLine(interpretationLine);
        return sb.ToString().TrimEnd();
    }

    private static void AppendRecommendationsSection(StringBuilder notes, IReadOnlyList<TalentRecommendationRow> recs)
    {
        if (recs.Count == 0)
        {
            return;
        }

        notes.AppendLine();
        notes.AppendLine("【توصيات ذكاء المواهب (نشطة)】");
        foreach (var r in recs)
        {
            notes.AppendLine();
            notes.AppendLine($"◆ {r.Title}  [{PriorityAr(r.Priority)} · ثقة {r.GeneratedOnUtc:yyyy-MM-dd}]");
            notes.AppendLine(r.Description);
            notes.AppendLine($"الإجراء المقترح: {r.RecommendedAction}");
        }
    }

    private static string PriorityAr(RecommendationPriority p) => p switch
    {
        RecommendationPriority.Low => "منخفض",
        RecommendationPriority.Medium => "متوسط",
        RecommendationPriority.High => "مرتفع",
        RecommendationPriority.Critical => "حرج",
        _ => p.ToString(),
    };

    private static string ReadinessLevelAr(ReadinessLevel r) => r switch
    {
        ReadinessLevel.ReadyNow => "جاهز الآن",
        ReadinessLevel.ReadySoon => "جاهز قريباً",
        ReadinessLevel.ReadyLater => "جاهز لاحقاً",
        _ => r.ToString(),
    };

    private static IReadOnlyList<CreateDevelopmentPlanItemPathRequest> DefaultPaths()
    {
        var start = DateTime.UtcNow.Date;
        return new List<CreateDevelopmentPlanItemPathRequest>
        {
            new()
            {
                SortOrder = 1,
                Title = "التحضير والاتفاق",
                PlannedStartUtc = start,
                PlannedEndUtc = start.AddDays(14),
            },
            new()
            {
                SortOrder = 2,
                Title = "التنفيذ",
                PlannedStartUtc = start.AddDays(15),
                PlannedEndUtc = start.AddDays(60),
            },
            new()
            {
                SortOrder = 3,
                Title = "المراجعة وقياس الأثر",
                PlannedStartUtc = start.AddDays(61),
                PlannedEndUtc = start.AddDays(90),
            },
        };
    }
}
