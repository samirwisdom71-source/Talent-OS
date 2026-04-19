import { DOCUMENT } from '@angular/common';
import { Injectable, computed, inject, signal } from '@angular/core';

export type AppLang = 'ar' | 'en';

const DICT: Record<AppLang, Record<string, string>> = {
  ar: {
    'nav.dashboard': 'لوحة التحكم',
    'nav.employees': 'الموظفون',
    'nav.performance': 'الأداء',
    'nav.potential': 'الإمكانات',
    'nav.nineBox': 'مصفوفة الصناديق التسعة',
    'nav.succession': 'التعاقب',
    'nav.development': 'خطط التطوير',
    'nav.marketplace': 'السوق الداخلي',
    'nav.analytics': 'التحليلات التنفيذية',
    'nav.intelligence': 'الذكاء المؤسسي',
    'nav.insights': 'الرؤى',
    'nav.recommendations': 'التوصيات',
    'nav.approvals': 'الموافقات',
    'nav.notifications': 'الإشعارات',
    'nav.notificationTemplates': 'قوالب الإشعارات',
    'nav.settings': 'الإعدادات',
    'app.title': 'نظام المواهب',
    'app.subtitle': 'Talent OS',
    'login.title': 'تسجيل الدخول',
    'login.subtitle': 'أدخل بياناتك للوصول إلى لوحة المواهب.',
    'login.email': 'البريد الإلكتروني',
    'login.password': 'كلمة المرور',
    'login.submit': 'دخول',
    'login.validation': 'يرجى إدخال بريد إلكتروني وكلمة مرور صالحة.',
    'login.lang': 'English',
    'topbar.logout': 'خروج',
    'common.loading': 'جاري التحميل…',
    'common.retry': 'إعادة المحاولة',
    'common.empty': 'لا توجد بيانات',
    'common.error': 'حدث خطأ',
    'common.view': 'عرض',
    'common.actions': 'إجراءات',
    'common.status': 'الحالة',
    'common.search': 'بحث',
    'common.page': 'صفحة',
    'common.of': 'من',
  },
  en: {
    'nav.dashboard': 'Dashboard',
    'nav.employees': 'Employees',
    'nav.performance': 'Performance',
    'nav.potential': 'Potential',
    'nav.nineBox': '9-Box',
    'nav.succession': 'Succession',
    'nav.development': 'Development',
    'nav.marketplace': 'Marketplace',
    'nav.analytics': 'Executive analytics',
    'nav.intelligence': 'Intelligence',
    'nav.insights': 'Insights',
    'nav.recommendations': 'Recommendations',
    'nav.approvals': 'Approvals',
    'nav.notifications': 'Notifications',
    'nav.notificationTemplates': 'Notification templates',
    'nav.settings': 'Settings',
    'app.title': 'Talent OS',
    'app.subtitle': 'Enterprise talent',
    'login.title': 'Sign in',
    'login.subtitle': 'Enter your credentials to open the talent workspace.',
    'login.email': 'Email',
    'login.password': 'Password',
    'login.submit': 'Sign in',
    'login.validation': 'Please enter a valid email and password.',
    'login.lang': 'العربية',
    'topbar.logout': 'Log out',
    'common.loading': 'Loading…',
    'common.retry': 'Retry',
    'common.empty': 'Nothing to show yet',
    'common.error': 'Something went wrong',
    'common.view': 'View',
    'common.actions': 'Actions',
    'common.status': 'Status',
    'common.search': 'Search',
    'common.page': 'Page',
    'common.of': 'of',
  },
};

@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly document = inject(DOCUMENT);
  readonly lang = signal<AppLang>('ar');

  readonly isRtl = computed(() => this.lang() === 'ar');

  constructor() {
    this.applyDom();
  }

  t(key: string): string {
    return DICT[this.lang()][key] ?? key;
  }

  toggleLang(): void {
    this.lang.update((l) => (l === 'ar' ? 'en' : 'ar'));
    this.applyDom();
  }

  private applyDom(): void {
    const html = this.document.documentElement;
    const l = this.lang();
    html.lang = l;
    html.dir = l === 'ar' ? 'rtl' : 'ltr';
  }
}
