(() => {
    const storageKey = 'techasset-theme';
    const root = document.documentElement;
    const toggle = document.getElementById('themeToggle');

    const applyTheme = (theme) => {
        const safeTheme = theme === 'dark' ? 'dark' : 'light';
        root.setAttribute('data-bs-theme', safeTheme);
        localStorage.setItem(storageKey, safeTheme);
        if (toggle) {
            toggle.setAttribute('aria-pressed', safeTheme === 'dark' ? 'true' : 'false');
        }
        window.dispatchEvent(new CustomEvent('techasset:themechanged', { detail: { theme: safeTheme } }));
    };

    applyTheme(localStorage.getItem(storageKey) || 'light');

    if (toggle) {
        toggle.addEventListener('click', () => {
            const currentTheme = root.getAttribute('data-bs-theme') === 'dark' ? 'dark' : 'light';
            applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
        });
    }
})();
