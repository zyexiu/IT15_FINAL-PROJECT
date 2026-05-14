// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
	const THEME_KEY = 'snackflow-theme';
	const DARK_ICON = '<path d="M21 12.8A9 9 0 1 1 11.2 3 7 7 0 0 0 21 12.8z"/>';
	const LIGHT_ICON = '<circle cx="12" cy="12" r="4"/><path d="M12 2v2.5M12 19.5V22M4.2 4.2l1.8 1.8M18 18l1.8 1.8M2 12h2.5M19.5 12H22M4.2 19.8l1.8-1.8M18 6l1.8-1.8"/>';

	function readStoredTheme() {
		try {
			return localStorage.getItem(THEME_KEY);
		} catch {
			return null;
		}
	}

	function storeTheme(theme) {
		try {
			localStorage.setItem(THEME_KEY, theme);
		} catch {
			// ignore storage failures and keep the current session theme only
		}
	}

	function getPreferredTheme() {
		const storedTheme = readStoredTheme();
		if (storedTheme === 'light' || storedTheme === 'dark') {
			return storedTheme;
		}

		return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
			? 'dark'
			: 'light';
	}

	function syncThemeControls(theme) {
		const isDark = theme === 'dark';

		document.querySelectorAll('[data-theme-toggle]').forEach(button => {
			const label = button.querySelector('[data-theme-label]');
			const icon = button.querySelector('[data-theme-icon]');

			button.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
			button.setAttribute('title', isDark ? 'Switch to light mode' : 'Switch to dark mode');

			if (label) {
				label.textContent = isDark ? 'Light mode' : 'Dark mode';
			}

			if (icon) {
				icon.innerHTML = isDark ? LIGHT_ICON : DARK_ICON;
			}
		});
	}

	function applyTheme(theme) {
		const resolvedTheme = theme === 'dark' ? 'dark' : 'light';
		document.documentElement.dataset.theme = resolvedTheme;
		document.documentElement.style.colorScheme = resolvedTheme;
		storeTheme(resolvedTheme);
		syncThemeControls(resolvedTheme);
	}

	function toggleTheme() {
		const currentTheme = document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light';
		applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
	}

	function initTheme() {
		applyTheme(getPreferredTheme());

		document.querySelectorAll('[data-theme-toggle]').forEach(button => {
			button.addEventListener('click', toggleTheme);
		});
	}

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', initTheme, { once: true });
	} else {
		initTheme();
	}

	window.SnackFlowTheme = {
		applyTheme,
		getPreferredTheme,
		toggleTheme
	};
})();
