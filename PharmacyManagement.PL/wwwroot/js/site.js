const pmsTranslations = {
    en: {
        'brand.short': 'Pharmacy MS',
        'nav.dashboard': 'Dashboard',
        'nav.medicines': 'Medicines',
        'nav.newSale': 'New Sale (POS)',
        'nav.salesHistory': 'Sales History',
        'nav.customers': 'Customers',
        'nav.payments': 'Payments',
        'nav.management': 'Management',
        'nav.currentShift': 'Current Shift',
        'nav.shiftHistory': 'Shift History',
        'nav.inventory': 'Inventory',
        'nav.categories': 'Categories',
        'nav.suppliers': 'Suppliers',
        'nav.stock': 'Stock',
        'nav.purchases': 'Purchases',
        'nav.invoices': 'Invoices',
        'nav.returns': 'Returns',
        'nav.purchaseReturns': 'Purchase Returns',
        'nav.salesReturns': 'Sales Returns',
        'nav.reports': 'Reports',
        'nav.profit': 'Profit',
        'nav.topSelling': 'Top Selling',
        'nav.outOfStock': 'Out of Stock',
        'nav.admin': 'Admin',
        'nav.users': 'Users',
        'nav.auditLogs': 'Audit Logs',
        'nav.loginHistory': 'Login History',
        'search.placeholder': 'Search medicines, suppliers...',
        'user.staff': 'Pharmacy Staff',
        'action.logout': 'Logout',
        'login.continue': 'Sign in to continue',
        'login.signIn': 'Sign In',
        'login.demoAccounts': 'Demo accounts:',
        'pager.previous': 'Previous',
        'pager.next': 'Next',
        'pager.showing': 'Showing {from}-{to} of {total}'
    },
    ar: {
        'brand.short': 'نظام الصيدلية',
        'nav.dashboard': 'لوحة التحكم',
        'nav.medicines': 'الأدوية',
        'nav.newSale': 'بيع جديد',
        'nav.salesHistory': 'سجل المبيعات',
        'nav.customers': 'العملاء',
        'nav.payments': 'المدفوعات',
        'nav.management': 'الإدارة',
        'nav.currentShift': 'الوردية الحالية',
        'nav.shiftHistory': 'سجل الورديات',
        'nav.inventory': 'المخزون',
        'nav.categories': 'التصنيفات',
        'nav.suppliers': 'الموردون',
        'nav.stock': 'الرصيد',
        'nav.purchases': 'المشتريات',
        'nav.invoices': 'الفواتير',
        'nav.returns': 'المرتجعات',
        'nav.purchaseReturns': 'مرتجعات الشراء',
        'nav.salesReturns': 'مرتجعات البيع',
        'nav.reports': 'التقارير',
        'nav.profit': 'الأرباح',
        'nav.topSelling': 'الأكثر مبيعا',
        'nav.outOfStock': 'نفد المخزون',
        'nav.admin': 'المسؤول',
        'nav.users': 'المستخدمون',
        'nav.auditLogs': 'سجل التدقيق',
        'nav.loginHistory': 'سجل الدخول',
        'search.placeholder': 'ابحث عن أدوية أو موردين...',
        'user.staff': 'فريق الصيدلية',
        'action.logout': 'تسجيل الخروج',
        'login.continue': 'سجل الدخول للمتابعة',
        'login.signIn': 'تسجيل الدخول',
        'login.demoAccounts': 'حسابات تجريبية:',
        'pager.previous': 'السابق',
        'pager.next': 'التالي',
        'pager.showing': 'عرض {from}-{to} من {total}'
    }
};

function getPmsLanguage() {
    return localStorage.getItem('pms-lang') || 'en';
}

function getPmsTheme() {
    return localStorage.getItem('pms-theme') || 'light';
}

function applyPmsPreferences() {
    const lang = getPmsLanguage();
    const theme = getPmsTheme();
    const dictionary = pmsTranslations[lang] || pmsTranslations.en;

    document.documentElement.lang = lang;
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.dataset.theme = theme;

    document.querySelectorAll('[data-i18n]').forEach(element => {
        const key = element.dataset.i18n;
        if (key && dictionary[key]) element.textContent = dictionary[key];
    });

    document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
        const key = element.dataset.i18nPlaceholder;
        if (key && dictionary[key]) element.setAttribute('placeholder', dictionary[key]);
    });

    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        const isDark = theme === 'dark';
        themeToggle.innerHTML = `<i class="bi ${isDark ? 'bi-sun' : 'bi-moon-stars'}"></i>`;
        themeToggle.setAttribute('title', isDark ? 'Light mode' : 'Dark mode');
        themeToggle.setAttribute('aria-label', isDark ? 'Light mode' : 'Dark mode');
    }

    const languageToggle = document.getElementById('languageToggle');
    if (languageToggle) {
        languageToggle.textContent = lang === 'ar' ? 'EN' : 'AR';
        languageToggle.setAttribute('title', lang === 'ar' ? 'Switch to English' : 'التبديل إلى العربية');
        languageToggle.setAttribute('aria-label', lang === 'ar' ? 'Switch to English' : 'Switch to Arabic');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    applyPmsPreferences();

    document.getElementById('themeToggle')?.addEventListener('click', () => {
        localStorage.setItem('pms-theme', getPmsTheme() === 'dark' ? 'light' : 'dark');
        applyPmsPreferences();
    });

    document.getElementById('languageToggle')?.addEventListener('click', () => {
        localStorage.setItem('pms-lang', getPmsLanguage() === 'ar' ? 'en' : 'ar');
        applyPmsPreferences();
        document.dispatchEvent(new CustomEvent('pms:language-changed'));
    });

    const pageSize = 10;

    document.querySelectorAll('table.table-pharmacy').forEach((table, tableIndex) => {
        if (table.dataset.noPagination === 'true') return;

        const tbody = table.tBodies[0];
        if (!tbody) return;

        const rows = Array.from(tbody.rows);
        if (rows.length <= pageSize) return;

        let currentPage = 1;
        const totalPages = Math.ceil(rows.length / pageSize);
        const pager = document.createElement('nav');
        pager.className = 'table-pager d-flex justify-content-between align-items-center flex-wrap gap-2 mt-3';
        pager.setAttribute('aria-label', `Table ${tableIndex + 1} pagination`);

        const info = document.createElement('div');
        info.className = 'text-muted small';

        const list = document.createElement('ul');
        list.className = 'pagination pagination-sm mb-0';
        pager.append(info, list);

        const render = () => {
            const dictionary = pmsTranslations[getPmsLanguage()] || pmsTranslations.en;
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;

            rows.forEach((row, index) => {
                row.hidden = index < start || index >= end;
            });

            info.textContent = dictionary['pager.showing']
                .replace('{from}', start + 1)
                .replace('{to}', Math.min(end, rows.length))
                .replace('{total}', rows.length);
            list.replaceChildren();

            const addButton = (label, page, disabled = false, active = false) => {
                const item = document.createElement('li');
                item.className = `page-item${disabled ? ' disabled' : ''}${active ? ' active' : ''}`;

                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'page-link';
                button.textContent = label;
                button.disabled = disabled;
                button.addEventListener('click', () => {
                    currentPage = page;
                    render();
                });

                item.appendChild(button);
                list.appendChild(item);
            };

            addButton(dictionary['pager.previous'], Math.max(1, currentPage - 1), currentPage === 1);

            for (let page = 1; page <= totalPages; page++) {
                addButton(String(page), page, false, page === currentPage);
            }

            addButton(dictionary['pager.next'], Math.min(totalPages, currentPage + 1), currentPage === totalPages);
        };

        const tableShell = table.closest('.content-card') || table.parentElement;
        tableShell?.appendChild(pager);
        render();
        document.addEventListener('pms:language-changed', render);
    });
});
