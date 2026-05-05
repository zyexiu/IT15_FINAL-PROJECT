/**
 * SnackFlow MES — Landing Page JS
 * Handles: navbar scroll reveal + sticky, login dropdown, mobile nav, smooth scroll, scroll reveal
 */
(function () {
  'use strict';

  document.addEventListener('DOMContentLoaded', function () {

    /* ── Element refs ─────────────────────────────────────── */
    const navbar      = document.getElementById('sf-navbar');
    const signinBtn   = document.getElementById('sf-signin-btn');
    const loginPanel  = document.getElementById('sf-login-panel');
    const hamburger   = document.getElementById('sf-hamburger');
    const navlinks    = document.getElementById('sf-navlinks');

    if (!navbar) return; // guard

    /* ══════════════════════════════════════════════════════
       NAVBAR — scroll reveal + sticky
    ══════════════════════════════════════════════════════ */
    const SHOW_THRESHOLD   = 80;   // px scrolled before navbar appears
    const SCROLL_THRESHOLD = 120;  // px scrolled before "scrolled" style kicks in

    function updateNavbar() {
      const y = window.scrollY || window.pageYOffset;

      if (y > SHOW_THRESHOLD) {
        navbar.classList.add('sf-navbar--visible');
      } else {
        navbar.classList.remove('sf-navbar--visible');
        // also close login panel when scrolling back to top
        closeLoginPanel();
      }

      if (y > SCROLL_THRESHOLD) {
        navbar.classList.add('sf-navbar--scrolled');
      } else {
        navbar.classList.remove('sf-navbar--scrolled');
      }
    }

    updateNavbar(); // run once on load
    window.addEventListener('scroll', updateNavbar, { passive: true });

    /* ══════════════════════════════════════════════════════
       LOGIN DROPDOWN
    ══════════════════════════════════════════════════════ */
    function openLoginPanel() {
      loginPanel.classList.add('sf-login-panel--open');
      loginPanel.setAttribute('aria-hidden', 'false');
      signinBtn.setAttribute('aria-expanded', 'true');
      // focus first input
      const firstInput = loginPanel.querySelector('input');
      if (firstInput) setTimeout(() => firstInput.focus(), 50);
    }

    function closeLoginPanel() {
      loginPanel.classList.remove('sf-login-panel--open');
      loginPanel.setAttribute('aria-hidden', 'true');
      signinBtn.setAttribute('aria-expanded', 'false');
    }

    function toggleLoginPanel() {
      const isOpen = loginPanel.classList.contains('sf-login-panel--open');
      isOpen ? closeLoginPanel() : openLoginPanel();
    }

    if (signinBtn) {
      signinBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        toggleLoginPanel();
      });
    }

    // Close on outside click
    document.addEventListener('click', function (e) {
      if (
        loginPanel &&
        !loginPanel.contains(e.target) &&
        signinBtn &&
        !signinBtn.contains(e.target)
      ) {
        closeLoginPanel();
      }
    });

    // Close on Escape key
    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') {
        closeLoginPanel();
        closeMobileNav();
      }
    });

    /* ══════════════════════════════════════════════════════
       MOBILE NAV
    ══════════════════════════════════════════════════════ */
    function openMobileNav() {
      navlinks.classList.add('sf-navlinks--open');
      hamburger.classList.add('sf-hamburger--open');
      hamburger.setAttribute('aria-expanded', 'true');
    }

    function closeMobileNav() {
      navlinks.classList.remove('sf-navlinks--open');
      hamburger.classList.remove('sf-hamburger--open');
      hamburger.setAttribute('aria-expanded', 'false');
    }

    if (hamburger) {
      hamburger.addEventListener('click', function (e) {
        e.stopPropagation();
        const isOpen = navlinks.classList.contains('sf-navlinks--open');
        isOpen ? closeMobileNav() : openMobileNav();
      });
    }

    /* ══════════════════════════════════════════════════════
       SMOOTH SCROLL for anchor links
    ══════════════════════════════════════════════════════ */
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
      anchor.addEventListener('click', function (e) {
        const targetId = this.getAttribute('href');
        if (targetId === '#') return;
        const target = document.querySelector(targetId);
        if (!target) return;

        e.preventDefault();

        // Account for navbar height when visible
        const navH = navbar.classList.contains('sf-navbar--visible')
          ? navbar.offsetHeight
          : 0;
        const offset = navH + 16;
        const top = target.getBoundingClientRect().top + window.scrollY - offset;

        window.scrollTo({ top, behavior: 'smooth' });

        // Close mobile nav after clicking a link
        closeMobileNav();

        // Update active link
        document.querySelectorAll('.sf-link').forEach(l => l.classList.remove('sf-link--active'));
        this.classList.add('sf-link--active');
      });
    });

    /* ══════════════════════════════════════════════════════
       ACTIVE NAV LINK on scroll (IntersectionObserver)
    ══════════════════════════════════════════════════════ */
    const sections = document.querySelectorAll('section[id]');
    const navLinkMap = {};
    document.querySelectorAll('.sf-link[href^="#"]').forEach(function (link) {
      navLinkMap[link.getAttribute('href').slice(1)] = link;
    });

    if ('IntersectionObserver' in window && sections.length) {
      const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            Object.values(navLinkMap).forEach(l => l.classList.remove('sf-link--active'));
            const active = navLinkMap[entry.target.id];
            if (active) active.classList.add('sf-link--active');
          }
        });
      }, { rootMargin: '-40% 0px -55% 0px' });

      sections.forEach(s => observer.observe(s));
    }

    /* ══════════════════════════════════════════════════════
       SCROLL REVEAL (IntersectionObserver)
    ══════════════════════════════════════════════════════ */
    // Hero elements animate via CSS keyframes on load — exclude them from observer
    const revealEls = document.querySelectorAll('.sf-reveal:not(.sf-hero .sf-reveal)');

    if ('IntersectionObserver' in window && revealEls.length) {
      const revealObserver = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            entry.target.classList.add('sf-reveal--visible');
            revealObserver.unobserve(entry.target);
          }
        });
      }, { threshold: 0.12 });

      revealEls.forEach(el => revealObserver.observe(el));
    } else {
      // Fallback: show all immediately
      document.querySelectorAll('.sf-reveal').forEach(el => el.classList.add('sf-reveal--visible'));
    }

    /* ══════════════════════════════════════════════════════
       PASSWORD TOGGLE
    ══════════════════════════════════════════════════════ */
    const passwordToggle = document.getElementById('sf-password-toggle');
    const passwordInput  = document.getElementById('sf-password');

    if (passwordToggle && passwordInput) {
      passwordToggle.addEventListener('click', function () {
        const isPassword = passwordInput.type === 'password';
        passwordInput.type = isPassword ? 'text' : 'password';

        // Toggle eye icons
        const showIcon = this.querySelector('.sf-eye-show');
        const hideIcon = this.querySelector('.sf-eye-hide');
        if (showIcon && hideIcon) {
          showIcon.style.display = isPassword ? 'none' : 'block';
          hideIcon.style.display = isPassword ? 'block' : 'none';
        }

        // Update aria-label
        this.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
      });
    }

    /* ══════════════════════════════════════════════════════
       AJAX LOGIN FORM SUBMISSION
    ══════════════════════════════════════════════════════ */
    const loginForm       = document.getElementById('sf-login-form');
    const usernameInput   = document.getElementById('sf-username');
    const loginSubmitBtn  = document.getElementById('sf-login-submit');
    const loginBtnText    = document.getElementById('sf-login-btn-text');
    const loginError      = document.getElementById('sf-login-error');
    const loginErrorMsg   = document.getElementById('sf-login-error-msg');
    const usernameError   = document.getElementById('sf-username-error');
    const passwordError   = document.getElementById('sf-password-error');
    const rememberCheckbox = document.getElementById('sf-remember');

    if (loginForm) {
      loginForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        // Clear previous errors
        loginError.style.display = 'none';
        usernameError.textContent = '';
        passwordError.textContent = '';
        usernameInput.classList.remove('sf-input--error');
        passwordInput.classList.remove('sf-input--error');

        // Get form values
        const username = usernameInput.value.trim();
        const password = passwordInput.value;
        const rememberMe = rememberCheckbox.checked;

        // Client-side validation
        let hasError = false;

        if (!username) {
          usernameError.textContent = 'Username is required';
          usernameInput.classList.add('sf-input--error');
          hasError = true;
        }

        if (!password) {
          passwordError.textContent = 'Password is required';
          passwordInput.classList.add('sf-input--error');
          hasError = true;
        }

        if (hasError) return;

        // Disable submit button and show loading state
        loginSubmitBtn.disabled = true;
        loginBtnText.textContent = 'Signing in...';

        try {
          // Get anti-forgery token
          const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
          const token = tokenInput ? tokenInput.value : '';

          const response = await fetch('/Account/LoginAjax', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'RequestVerificationToken': token
            },
            body: JSON.stringify({
              username: username,
              password: password,
              rememberMe: rememberMe
            })
          });

          const result = await response.json();

          if (result.success) {
            // Success! Redirect to dashboard
            loginBtnText.textContent = 'Success!';
            window.location.href = result.redirectUrl || '/Dashboard';
          } else {
            // Show error message
            loginErrorMsg.textContent = result.message || 'Login failed. Please try again.';
            loginError.style.display = 'flex';

            // Re-enable submit button
            loginSubmitBtn.disabled = false;
            loginBtnText.textContent = 'Sign In';
          }
        } catch (error) {
          console.error('Login error:', error);
          loginErrorMsg.textContent = 'An error occurred. Please try again.';
          loginError.style.display = 'flex';

          // Re-enable submit button
          loginSubmitBtn.disabled = false;
          loginBtnText.textContent = 'Sign In';
        }
      });

      // Clear field errors on input
      if (usernameInput) {
        usernameInput.addEventListener('input', function () {
          usernameError.textContent = '';
          this.classList.remove('sf-input--error');
        });
      }

      if (passwordInput) {
        passwordInput.addEventListener('input', function () {
          passwordError.textContent = '';
          this.classList.remove('sf-input--error');
        });
      }
    }

  }); // DOMContentLoaded

})();
