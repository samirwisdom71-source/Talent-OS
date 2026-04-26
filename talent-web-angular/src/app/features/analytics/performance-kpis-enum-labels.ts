/** Keys in `i18n.service` — values match `TalentSystem.Domain.Enums.DevelopmentItemType`. */
const DEVELOPMENT_ITEM_TYPE_KEYS: Record<number, string> = {
  1: 'enums.developmentItemType.training',
  2: 'enums.developmentItemType.coaching',
  3: 'enums.developmentItemType.mentoring',
  4: 'enums.developmentItemType.stretchAssignment',
  5: 'enums.developmentItemType.jobRotation',
  6: 'enums.developmentItemType.certification',
  7: 'enums.developmentItemType.selfLearning',
  8: 'enums.developmentItemType.other',
};

/** Keys — values match `TalentSystem.Domain.Enums.OpportunityType`. */
const OPPORTUNITY_TYPE_KEYS: Record<number, string> = {
  1: 'enums.opportunityType.project',
  2: 'enums.opportunityType.taskForce',
  3: 'enums.opportunityType.stretchAssignment',
  4: 'enums.opportunityType.jobRotation',
  5: 'enums.opportunityType.internalRole',
  6: 'enums.opportunityType.temporaryAssignment',
  7: 'enums.opportunityType.committee',
  8: 'enums.opportunityType.other',
};

export function developmentItemTypeI18nKey(itemType: number): string {
  return DEVELOPMENT_ITEM_TYPE_KEYS[itemType] ?? 'enums.developmentItemType.unknown';
}

export function opportunityTypeI18nKey(opportunityType: number): string {
  return OPPORTUNITY_TYPE_KEYS[opportunityType] ?? 'enums.opportunityType.unknown';
}
