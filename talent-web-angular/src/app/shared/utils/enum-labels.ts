/** Arabic-first labels with English hint for mixed audiences. */
export type UiLang = 'ar' | 'en';

const pick = (lang: UiLang, ar: string, en: string) => (lang === 'ar' ? ar : en);

function mapNum(lang: UiLang, m: Record<number, { ar: string; en: string }>, v: number | null | undefined): string {
  if (v === null || v === undefined) return pick(lang, '—', '—');
  const row = m[v];
  return row ? pick(lang, row.ar, row.en) : pick(lang, `قيمة ${v}`, `Value ${v}`);
}

const APPROVAL_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مسودة', en: 'Draft' },
  2: { ar: 'مُرسَل', en: 'Submitted' },
  3: { ar: 'قيد المراجعة', en: 'In review' },
  4: { ar: 'مقبول', en: 'Approved' },
  5: { ar: 'مرفوض', en: 'Rejected' },
  6: { ar: 'ملغى', en: 'Cancelled' },
};

const INSIGHT_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'نشط', en: 'Active' },
  2: { ar: 'مُهمَل', en: 'Dismissed' },
  3: { ar: 'مؤرشف', en: 'Archived' },
};

const RECOMMENDATION_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'نشط', en: 'Active' },
  2: { ar: 'مقبول', en: 'Accepted' },
  3: { ar: 'مُهمَل', en: 'Dismissed' },
  4: { ar: 'مؤرشف', en: 'Archived' },
};

const DEV_ITEM_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'لم يبدأ', en: 'Not started' },
  2: { ar: 'قيد التنفيذ', en: 'In progress' },
  3: { ar: 'مكتمل', en: 'Completed' },
  4: { ar: 'ملغى', en: 'Cancelled' },
};

const OPP_APP_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مُقدَّم', en: 'Submitted' },
  2: { ar: 'قيد المراجعة', en: 'Under review' },
  3: { ar: 'قائمة مختصرة', en: 'Shortlisted' },
  4: { ar: 'مقبول', en: 'Accepted' },
  5: { ar: 'مرفوض', en: 'Rejected' },
  6: { ar: 'مسحوب', en: 'Withdrawn' },
};

const READINESS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'جاهز الآن', en: 'Ready now' },
  2: { ar: 'جاهز قريباً', en: 'Ready soon' },
  3: { ar: 'جاهز لاحقاً', en: 'Ready later' },
};

const NINE_BOX: Record<number, { ar: string; en: string }> = {
  1: { ar: 'صندوق 1', en: 'Box 1' },
  2: { ar: 'صندوق 2', en: 'Box 2' },
  3: { ar: 'صندوق 3', en: 'Box 3' },
  4: { ar: 'صندوق 4', en: 'Box 4' },
  5: { ar: 'صندوق 5', en: 'Box 5' },
  6: { ar: 'صندوق 6', en: 'Box 6' },
  7: { ar: 'صندوق 7', en: 'Box 7' },
  8: { ar: 'صندوق 8', en: 'Box 8' },
  9: { ar: 'صندوق 9 (قائد استراتيجي)', en: 'Box 9 (Strategic leader)' },
};

const PERF_CYCLE_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مسودة', en: 'Draft' },
  2: { ar: 'نشط', en: 'Active' },
  3: { ar: 'مغلق', en: 'Closed' },
  4: { ar: 'مؤرشف', en: 'Archived' },
};

const MARKETPLACE_OPP_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مسودة', en: 'Draft' },
  2: { ar: 'مفتوح', en: 'Open' },
  3: { ar: 'مغلق', en: 'Closed' },
  4: { ar: 'ملغى', en: 'Cancelled' },
  5: { ar: 'مؤرشف', en: 'Archived' },
};

const DEV_PLAN_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مسودة', en: 'Draft' },
  2: { ar: 'نشط', en: 'Active' },
  3: { ar: 'مكتمل', en: 'Completed' },
  4: { ar: 'ملغى', en: 'Cancelled' },
};

const SUCCESSION_PLAN_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مسودة', en: 'Draft' },
  2: { ar: 'نشط', en: 'Active' },
  3: { ar: 'مغلق', en: 'Closed' },
  4: { ar: 'مؤرشف', en: 'Archived' },
};

const POTENTIAL_ASSESSMENT_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مسودة', en: 'Draft' },
  2: { ar: 'مُعتمد', en: 'Finalized' },
  3: { ar: 'ملغى', en: 'Cancelled' },
};

const POTENTIAL_LEVEL: Record<number, { ar: string; en: string }> = {
  1: { ar: 'منخفض', en: 'Low' },
  2: { ar: 'متوسط', en: 'Medium' },
  3: { ar: 'مرتفع', en: 'High' },
};

const APPROVAL_ACTION_TYPE: Record<number, { ar: string; en: string }> = {
  1: { ar: 'إرسال', en: 'Submit' },
  2: { ar: 'تعيين', en: 'Assign' },
  3: { ar: 'بدء مراجعة', en: 'Start review' },
  4: { ar: 'موافقة', en: 'Approve' },
  5: { ar: 'رفض', en: 'Reject' },
  6: { ar: 'إلغاء', en: 'Cancel' },
  7: { ar: 'إعادة تعيين', en: 'Reassign' },
};

const DEV_ITEM_TYPE: Record<number, { ar: string; en: string }> = {
  1: { ar: 'تدريب', en: 'Training' },
  2: { ar: 'تدريب على رأس العمل', en: 'Coaching' },
  3: { ar: 'إرشاد', en: 'Mentoring' },
  4: { ar: 'مهمة موسّعة', en: 'Stretch assignment' },
  5: { ar: 'تدوير وظيفي', en: 'Job rotation' },
  6: { ar: 'شهادة', en: 'Certification' },
  7: { ar: 'تعلّم ذاتي', en: 'Self-learning' },
  8: { ar: 'أخرى', en: 'Other' },
};

const DEV_SOURCE_TYPE: Record<number, { ar: string; en: string }> = {
  1: { ar: 'يدوي', en: 'Manual' },
  2: { ar: 'فجوة كفاءات', en: 'Competency gap' },
  3: { ar: 'تعاقب', en: 'Succession' },
  4: { ar: 'أداء', en: 'Performance' },
  5: { ar: 'إمكانات', en: 'Potential' },
};

const OPPORTUNITY_TYPE: Record<number, { ar: string; en: string }> = {
  1: { ar: 'مشروع', en: 'Project' },
  2: { ar: 'فريق مهام', en: 'Task force' },
  3: { ar: 'مهمة موسّعة', en: 'Stretch assignment' },
  4: { ar: 'تدوير وظيفي', en: 'Job rotation' },
  5: { ar: 'دور داخلي', en: 'Internal role' },
  6: { ar: 'تكليف مؤقت', en: 'Temporary assignment' },
  7: { ar: 'لجنة', en: 'Committee' },
  8: { ar: 'أخرى', en: 'Other' },
};

const LOW_MED_HIGH: Record<number, { ar: string; en: string }> = {
  1: { ar: 'منخفض', en: 'Low' },
  2: { ar: 'متوسط', en: 'Medium' },
  3: { ar: 'مرتفع', en: 'High' },
};

const RECORD_STATUS: Record<number, { ar: string; en: string }> = {
  1: { ar: 'نشط', en: 'Active' },
  2: { ar: 'مؤرشف', en: 'Archived' },
  3: { ar: 'محذوف', en: 'Deleted' },
};

const NOTIFICATION_CHANNEL: Record<number, { ar: string; en: string }> = {
  1: { ar: 'داخل التطبيق', en: 'In-app' },
  2: { ar: 'بريد', en: 'Email' },
  3: { ar: 'الاثنان', en: 'Both' },
};

const NOTIFICATION_TYPE: Record<number, { ar: string; en: string }> = {
  1: { ar: 'تعيين موافقة', en: 'Approval assigned' },
  2: { ar: 'إرسال موافقة', en: 'Approval submitted' },
  3: { ar: 'موافقة مقبولة', en: 'Approval approved' },
  4: { ar: 'موافقة مرفوضة', en: 'Approval rejected' },
  10: { ar: 'طلب سوق (مُقدَّم)', en: 'Marketplace application submitted' },
  11: { ar: 'طلب سوق (مقبول)', en: 'Marketplace application accepted' },
  12: { ar: 'طلب سوق (مرفوض)', en: 'Marketplace application rejected' },
  20: { ar: 'خطة تطوير مفعّلة', en: 'Development plan activated' },
  21: { ar: 'خطة تطوير مكتملة', en: 'Development plan completed' },
  99: { ar: 'عام', en: 'General' },
};

export const EnumLabels = {
  approvalStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, APPROVAL_STATUS, v),
  insightStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, INSIGHT_STATUS, v),
  recommendationStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, RECOMMENDATION_STATUS, v),
  developmentItemStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, DEV_ITEM_STATUS, v),
  opportunityApplicationStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, OPP_APP_STATUS, v),
  readinessLevel: (lang: UiLang, v: number | null | undefined) => mapNum(lang, READINESS, v),
  nineBoxCode: (lang: UiLang, v: number | null | undefined) => mapNum(lang, NINE_BOX, v),
  performanceCycleStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, PERF_CYCLE_STATUS, v),
  marketplaceOpportunityStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, MARKETPLACE_OPP_STATUS, v),
  developmentPlanStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, DEV_PLAN_STATUS, v),
  successionPlanStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, SUCCESSION_PLAN_STATUS, v),
  potentialAssessmentStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, POTENTIAL_ASSESSMENT_STATUS, v),
  potentialLevel: (lang: UiLang, v: number | null | undefined) => mapNum(lang, POTENTIAL_LEVEL, v),
  approvalActionType: (lang: UiLang, v: number | null | undefined) => mapNum(lang, APPROVAL_ACTION_TYPE, v),
  developmentItemType: (lang: UiLang, v: number | null | undefined) => mapNum(lang, DEV_ITEM_TYPE, v),
  developmentSourceType: (lang: UiLang, v: number | null | undefined) => mapNum(lang, DEV_SOURCE_TYPE, v),
  opportunityType: (lang: UiLang, v: number | null | undefined) => mapNum(lang, OPPORTUNITY_TYPE, v),
  criticalityLevel: (lang: UiLang, v: number | null | undefined) => mapNum(lang, LOW_MED_HIGH, v),
  successionRiskLevel: (lang: UiLang, v: number | null | undefined) => mapNum(lang, LOW_MED_HIGH, v),
  recordStatus: (lang: UiLang, v: number | null | undefined) => mapNum(lang, RECORD_STATUS, v),
  notificationChannel: (lang: UiLang, v: number | null | undefined) => mapNum(lang, NOTIFICATION_CHANNEL, v),
  notificationType: (lang: UiLang, v: number | null | undefined) => mapNum(lang, NOTIFICATION_TYPE, v),
};

export function shortGuid(id: string | null | undefined): string {
  if (!id) return '—';
  if (id.length <= 12) return id;
  return `${id.slice(0, 8)}…${id.slice(-4)}`;
}
