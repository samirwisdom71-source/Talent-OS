import { CommonModule, DOCUMENT } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { HeaderComponent } from './sections/header.component';
import { HeroComponent } from './sections/hero.component';
import { ValueStripComponent } from './sections/value-strip.component';
import { ModulesGridComponent } from './sections/modules-grid.component';
import { HowItWorksComponent } from './sections/how-it-works.component';
import { OutcomesComponent } from './sections/outcomes.component';
import { FinalCtaComponent } from './sections/final-cta.component';
import { FooterComponent } from './sections/footer.component';

type Lang = 'en' | 'ar';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [
    CommonModule,
    HeaderComponent,
    HeroComponent,
    ValueStripComponent,
    ModulesGridComponent,
    HowItWorksComponent,
    OutcomesComponent,
    FinalCtaComponent,
    FooterComponent,
  ],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
})
export class LandingPageComponent {
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  readonly lang = signal<Lang>('ar');
  readonly copy = computed(() => (this.lang() === 'en' ? EN : AR));
  readonly heroData = computed(() => ({
    ...this.copy().hero,
    primary: this.auth.isAuthenticated()
      ? this.lang() === 'ar'
        ? 'لوحة التحكم'
        : 'Dashboard'
      : this.lang() === 'ar'
        ? 'تسجيل الدخول'
        : 'Sign In',
  }));

  switchLang(next: Lang): void {
    this.lang.set(next);
  }

  scrollTo(id: string): void {
    if (id === 'auth') {
      void this.router.navigateByUrl(this.auth.isAuthenticated() ? '/dashboard' : '/login');
      return;
    }
    this.document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}

const EN = {
  nav: { platform: 'Platform', modules: 'Modules', outcomes: 'Outcomes', how: 'How It Works', demo: 'Get Started', signIn: 'Sign In', request: 'Request Demo' },
  hero: {
    title: 'Unify Talent Decisions Across Performance, Potential, Succession, and Growth',
    subtitle: 'Talent OS helps enterprise HR leaders turn workforce data into clear talent decisions, stronger succession pipelines, and measurable development outcomes.',
    primary: 'Sign In',
    secondary: 'Explore Platform',
  },
  tags: ['Enterprise Talent Intelligence', 'Performance & Potential', 'Succession Visibility', 'Executive Dashboards', 'Governed Workflows'],
  modulesTitle: 'A Unified Platform for the Full Talent Lifecycle',
  modulesLead: 'Coordinate performance, potential, succession, development, mobility, and governance in one enterprise operating layer.',
  modules: [
    { icon: '🎯', title: 'Performance Management', description: 'Run performance cycles, goals tracking, and evaluation workflows with accountability.' },
    { icon: '🧩', title: 'Potential & Talent Classification', description: 'Use potential assessment and 9-box intelligence to classify readiness and talent segments.' },
    { icon: '🏛️', title: 'Succession Planning', description: 'Cover critical positions with stronger successor pipelines and role risk visibility.' },
    { icon: '📈', title: 'Development Planning', description: 'Align individual plans, learning actions, and progress tracking to business priorities.' },
    { icon: '🧭', title: 'Internal Talent Marketplace', description: 'Enable internal opportunities, projects, and applications to accelerate mobility and growth.' },
    { icon: '📊', title: 'Executive Analytics', description: 'Deliver real-time dashboards, strategic KPIs, and recommendations for leadership decisions.' },
    { icon: '🛡️', title: 'Workflow & Governance', description: 'Digital approvals, role-based permissions, notifications, and auditable actions at scale.' },
  ],
  howTitle: 'From Talent Data to Executive Decisions',
  steps: [
    { title: 'Capture talent data', description: 'Consolidate people, role, and performance data across business structures.' },
    { title: 'Assess performance and potential', description: 'Evaluate readiness using consistent criteria and calibrated frameworks.' },
    { title: 'Plan succession and development', description: 'Create succession coverage and targeted development pathways.' },
    { title: 'Track outcomes through executive analytics', description: 'Monitor risk, capability, and impact with real-time leadership views.' },
  ],
  outcomesEyebrow: 'Executive Outcomes',
  outcomesTitle: 'Designed for strategic talent decisions in complex organizations',
  outcomesLead: 'Talent OS enables CHROs, executives, and line leaders to make confident decisions with trusted workforce intelligence.',
  outcomes: [
    'Identify high-potential talent earlier',
    'Reduce succession risk for critical roles',
    'Align development plans with business priorities',
    'Improve visibility across complex organizational structures',
    'Accelerate internal mobility and retention',
    'Give executives real-time talent intelligence',
  ],
  finalTitle: 'Ready to turn talent data into confident leadership decisions?',
  finalSubtitle: 'Request a demo and explore how Talent OS can support your enterprise talent strategy.',
  footerDescription: 'Talent OS is a unified enterprise HR and talent intelligence platform for performance, succession, development, mobility, and governance.',
  footerLinks: { platform: 'Platform', modules: 'Modules', analytics: 'Analytics', security: 'Security', contact: 'Contact' },
  copyright: 'Copyright © 2026 Talent OS. All rights reserved.',
};

const AR = {
  nav: { platform: 'المنصة', modules: 'الوحدات', outcomes: 'النتائج', how: 'كيف تعمل', demo: 'ابدأ الآن', signIn: 'تسجيل الدخول', request: 'اطلب عرضًا تجريبيًا' },
  hero: {
    title: 'وحّد قرارات المواهب عبر الأداء والإمكانات والتعاقب والنمو',
    subtitle: 'يساعد Talent OS قادة الموارد البشرية في المؤسسات على تحويل بيانات المواهب إلى قرارات أوضح، وخطط تعاقب أقوى، ونتائج تطوير قابلة للقياس.',
    primary: 'تسجيل الدخول',
    secondary: 'استكشف المنصة',
  },
  tags: ['ذكاء المواهب المؤسسي', 'الأداء والإمكانات', 'وضوح خطط التعاقب', 'لوحات قيادية تنفيذية', 'إجراءات عمل محوكمة'],
  modulesTitle: 'منصة موحدة لإدارة دورة حياة المواهب بالكامل',
  modulesLead: 'تجمع Talent OS الأداء والإمكانات والتعاقب والتطوير والتنقل الداخلي والحوكمة ضمن طبقة تشغيلية واحدة للمؤسسات.',
  modules: [
    { icon: '🎯', title: 'إدارة الأداء', description: 'تشغيل دورات الأداء، متابعة الأهداف، وتنفيذ مسارات التقييم والمراجعة بوضوح.' },
    { icon: '🧩', title: 'الإمكانات وتصنيف المواهب', description: 'تقييم الإمكانات ومصفوفة 9-Box وتصنيف المواهب ومؤشرات الجاهزية.' },
    { icon: '🏛️', title: 'التخطيط للتعاقب', description: 'تغطية المناصب الحرجة، بناء خطوط بديلة، ومتابعة المخاطر قبل تأثيرها.' },
    { icon: '📈', title: 'خطط التطوير', description: 'خطط تطوير فردية وإجراءات تعلم وتوجيه مع متابعة التقدم والإنجاز.' },
    { icon: '🧭', title: 'سوق المواهب الداخلي', description: 'فرص داخلية ومشاريع وتقديم الموظفين لدعم النمو والتنقل الوظيفي.' },
    { icon: '📊', title: 'التحليلات التنفيذية', description: 'لوحات لحظية ورؤى وتوصيات ومؤشرات أداء لدعم القرارات القيادية.' },
    { icon: '🛡️', title: 'سير العمل والحوكمة', description: 'موافقات رقمية وصلاحيات حسب الدور وإشعارات وسجل إجراءات قابل للتدقيق.' },
  ],
  howTitle: 'من بيانات المواهب إلى قرارات تنفيذية',
  steps: [
    { title: 'جمع بيانات المواهب', description: 'توحيد بيانات الموظفين والأدوار والأداء عبر هيكل المؤسسة.' },
    { title: 'تقييم الأداء والإمكانات', description: 'تطبيق معايير تقييم موحدة تعكس الجاهزية والإمكانات بدقة.' },
    { title: 'التخطيط للتعاقب والتطوير', description: 'بناء خطط تعاقب وتطوير مرتبطة بالأدوار والأولويات الاستراتيجية.' },
    { title: 'قياس النتائج عبر التحليلات التنفيذية', description: 'متابعة الأثر والمخاطر وجودة التنفيذ من منظور قيادي لحظي.' },
  ],
  outcomesEyebrow: 'الأثر التنفيذي',
  outcomesTitle: 'منصة تدعم قرارات المواهب الاستراتيجية في المؤسسات المعقدة',
  outcomesLead: 'تمنح Talent OS القيادات رؤية موثوقة لحالة المواهب والاستعداد القيادي وخيارات الحركة والتطوير.',
  outcomes: [
    'اكتشاف المواهب عالية الإمكانات مبكرًا',
    'تقليل مخاطر التعاقب في المناصب الحرجة',
    'ربط خطط التطوير بأولويات العمل',
    'تحسين الرؤية عبر الهياكل التنظيمية المعقدة',
    'تسريع التنقل الداخلي والاحتفاظ بالمواهب',
    'تزويد القيادات بذكاء مواهب لحظي',
  ],
  finalTitle: 'هل أنت مستعد لتحويل بيانات المواهب إلى قرارات قيادية واثقة؟',
  finalSubtitle: 'اطلب عرضًا تجريبيًا واكتشف كيف يدعم Talent OS استراتيجية المواهب في مؤسستك.',
  footerDescription: 'Talent OS منصة موحدة لإدارة المواهب وذكاء الموارد البشرية للمؤسسات الكبيرة.',
  footerLinks: { platform: 'المنصة', modules: 'الوحدات', analytics: 'التحليلات', security: 'الأمان', contact: 'تواصل معنا' },
  copyright: 'جميع الحقوق محفوظة © 2026 Talent OS',
};
