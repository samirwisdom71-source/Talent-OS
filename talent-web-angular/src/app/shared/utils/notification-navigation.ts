import { NotificationListItemDto } from '../models/notification.models';

export interface NotificationNavigationTarget {
  commands: any[];
  queryParams?: Record<string, string>;
}

export function resolveNotificationNavigation(
  row: Pick<NotificationListItemDto, 'relatedEntityId' | 'relatedEntityType'>,
): NotificationNavigationTarget | null {
  const id = row.relatedEntityId ?? undefined;
  const type = (row.relatedEntityType ?? '').trim();

  if (!type) {
    return null;
  }

  switch (type) {
    case 'ApprovalRequest':
      return id ? { commands: ['/approvals', id] } : { commands: ['/approvals'] };
    case 'DevelopmentPlan':
      return id ? { commands: ['/development', id] } : { commands: ['/development'] };
    case 'DevelopmentPlanItem':
    case 'DevelopmentPlanItemPath':
      return {
        commands: ['/development'],
        queryParams: id ? { focusId: id, focusType: type } : undefined,
      };
    case 'MarketplaceOpportunity':
      return id ? { commands: ['/marketplace', id] } : { commands: ['/marketplace'] };
    case 'OpportunityApplication':
      return { commands: ['/marketplace'] };
    case 'SuccessionPlan':
      return id ? { commands: ['/succession/plans', id] } : { commands: ['/succession/plans'] };
    case 'Employee':
      return id ? { commands: ['/employees', id] } : { commands: ['/employees'] };
    case 'PerformanceCycle':
      return { commands: ['/talent/performance/cycles'] };
    case 'PerformanceGoal':
      return { commands: ['/talent/performance/goals'] };
    case 'PerformanceEvaluation':
      return { commands: ['/talent/performance/evaluations'] };
    case 'PotentialAssessment':
      return { commands: ['/talent/potential'] };
    case 'TalentScore':
      return { commands: ['/talent/scores'] };
    case 'TalentClassification':
      return { commands: ['/talent/classifications'] };
    case 'ScoringPolicy':
      return { commands: ['/scoring-policies'] };
    case 'CriticalPosition':
      return { commands: ['/succession/critical-positions'] };
    case 'SuccessorCandidate':
      return { commands: ['/succession/successor-candidates'] };
    case 'OrganizationUnit':
      return { commands: ['/organization-units'] };
    case 'Position':
      return { commands: ['/positions'] };
    case 'JobGrade':
      return { commands: ['/job-grades'] };
    case 'CompetencyCategory':
      return { commands: ['/competency-categories'] };
    case 'Competency':
      return { commands: ['/competencies'] };
    case 'CompetencyLevel':
      return { commands: ['/competency-levels'] };
    case 'JobCompetencyRequirement':
      return { commands: ['/job-competency-requirements'] };
    case 'Role':
      return { commands: ['/roles'] };
    case 'User':
      return { commands: ['/users'] };
    case 'ClassificationRuleSet':
      return { commands: ['/settings/classification-rule-sets'] };
    default:
      return { commands: ['/dashboard'] };
  }
}
