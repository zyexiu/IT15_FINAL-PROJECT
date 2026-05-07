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
          // Get reCAPTCHA token
          const siteKey = document.querySelector('script[src*="recaptcha"]')?.src.match(/render=([^&]+)/)?.[1];
          let recaptchaToken = '';
          
          if (siteKey && typeof grecaptcha !== 'undefined') {
            try {
              recaptchaToken = await grecaptcha.execute(siteKey, { action: 'login' });
            } catch (recaptchaError) {
              console.error('reCAPTCHA error:', recaptchaError);
              loginErrorMsg.textContent = 'Security verification failed. Please refresh the page and try again.';
              loginError.style.display = 'flex';
              loginSubmitBtn.disabled = false;
              loginBtnText.textContent = 'Sign In';
              return;
            }
          }

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
              rememberMe: rememberMe,
              recaptchaToken: recaptchaToken
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

    /* ══════════════════════════════════════════════════════
       SIGN UP / SIGN IN PANEL SWITCHING
    ══════════════════════════════════════════════════════ */
    const signupPanel = document.getElementById('sf-signup-panel');
    const showSignupBtn = document.getElementById('sf-show-signup');
    const showLoginBtn = document.getElementById('sf-show-login');
    
    // Password toggles for signup form
    const signupPasswordToggle = document.getElementById('sf-signup-password-toggle');
    const signupPasswordInput = document.getElementById('sf-signup-password');
    const signupConfirmPasswordToggle = document.getElementById('sf-signup-confirm-password-toggle');
    const signupConfirmPasswordInput = document.getElementById('sf-signup-confirm-password');

    function showSignupPanel() {
      if (loginPanel && signupPanel) {
        loginPanel.style.display = 'none';
        loginPanel.classList.remove('sf-login-panel--open');
        signupPanel.style.display = 'block';
        setTimeout(() => {
          signupPanel.classList.add('sf-login-panel--open');
          signupPanel.setAttribute('aria-hidden', 'false');
          // Focus first input
          const firstInput = signupPanel.querySelector('input');
          if (firstInput) firstInput.focus();
        }, 10);
      }
    }

    function showLoginPanel() {
      if (loginPanel && signupPanel) {
        signupPanel.style.display = 'none';
        signupPanel.classList.remove('sf-login-panel--open');
        loginPanel.style.display = 'block';
        setTimeout(() => {
          loginPanel.classList.add('sf-login-panel--open');
          loginPanel.setAttribute('aria-hidden', 'false');
          // Focus first input
          const firstInput = loginPanel.querySelector('input');
          if (firstInput) firstInput.focus();
        }, 10);
      }
    }

    if (showSignupBtn) {
      showSignupBtn.addEventListener('click', function (e) {
        e.preventDefault();
        showSignupPanel();
      });
    }

    if (showLoginBtn) {
      showLoginBtn.addEventListener('click', function (e) {
        e.preventDefault();
        showLoginPanel();
      });
    }

    // Password toggle for signup password
    if (signupPasswordToggle && signupPasswordInput) {
      signupPasswordToggle.addEventListener('click', function () {
        const isPassword = signupPasswordInput.type === 'password';
        signupPasswordInput.type = isPassword ? 'text' : 'password';

        const showIcon = this.querySelector('.sf-eye-show');
        const hideIcon = this.querySelector('.sf-eye-hide');
        if (showIcon && hideIcon) {
          showIcon.style.display = isPassword ? 'none' : 'block';
          hideIcon.style.display = isPassword ? 'block' : 'none';
        }

        this.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
      });
    }

    // Password toggle for signup confirm password
    if (signupConfirmPasswordToggle && signupConfirmPasswordInput) {
      signupConfirmPasswordToggle.addEventListener('click', function () {
        const isPassword = signupConfirmPasswordInput.type === 'password';
        signupConfirmPasswordInput.type = isPassword ? 'text' : 'password';

        const showIcon = this.querySelector('.sf-eye-show');
        const hideIcon = this.querySelector('.sf-eye-hide');
        if (showIcon && hideIcon) {
          showIcon.style.display = isPassword ? 'none' : 'block';
          hideIcon.style.display = isPassword ? 'block' : 'none';
        }

        this.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
      });
    }

    // Close signup panel on outside click
    document.addEventListener('click', function (e) {
      if (
        signupPanel &&
        !signupPanel.contains(e.target) &&
        signinBtn &&
        !signinBtn.contains(e.target)
      ) {
        signupPanel.classList.remove('sf-login-panel--open');
        signupPanel.setAttribute('aria-hidden', 'true');
      }
    });

    /* ══════════════════════════════════════════════════════
       AJAX SIGNUP FORM SUBMISSION
    ══════════════════════════════════════════════════════ */
    const signupForm = document.getElementById('sf-signup-form');
    const signupFullNameInput = document.getElementById('sf-signup-fullname');
    const signupEmailInput = document.getElementById('sf-signup-email');
    const signupUsernameInput = document.getElementById('sf-signup-username');
    const signupSubmitBtn = document.getElementById('sf-signup-submit');
    const signupBtnText = document.getElementById('sf-signup-btn-text');
    const signupError = document.getElementById('sf-signup-error');
    const signupErrorMsg = document.getElementById('sf-signup-error-msg');
    const signupSuccess = document.getElementById('sf-signup-success');
    const signupSuccessMsg = document.getElementById('sf-signup-success-msg');
    const termsCheckbox = document.getElementById('sf-terms');

    // Error message elements
    const signupFullNameError = document.getElementById('sf-signup-fullname-error');
    const signupEmailError = document.getElementById('sf-signup-email-error');
    const signupUsernameError = document.getElementById('sf-signup-username-error');
    const signupPasswordError = document.getElementById('sf-signup-password-error');
    const signupConfirmPasswordError = document.getElementById('sf-signup-confirm-password-error');

    if (signupForm) {
      signupForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        // Clear previous errors
        signupError.style.display = 'none';
        signupSuccess.style.display = 'none';
        signupFullNameError.textContent = '';
        signupEmailError.textContent = '';
        signupUsernameError.textContent = '';
        signupPasswordError.textContent = '';
        signupConfirmPasswordError.textContent = '';

        // Remove error classes
        signupFullNameInput.classList.remove('sf-input--error');
        signupEmailInput.classList.remove('sf-input--error');
        signupUsernameInput.classList.remove('sf-input--error');
        signupPasswordInput.classList.remove('sf-input--error');
        signupConfirmPasswordInput.classList.remove('sf-input--error');

        // Get form values
        const fullName = signupFullNameInput.value.trim();
        const email = signupEmailInput.value.trim();
        const username = signupUsernameInput.value.trim();
        const password = signupPasswordInput.value;
        const confirmPassword = signupConfirmPasswordInput.value;
        const acceptTerms = termsCheckbox.checked;

        // Client-side validation
        let hasError = false;

        if (!fullName) {
          signupFullNameError.textContent = 'Full name is required';
          signupFullNameInput.classList.add('sf-input--error');
          hasError = true;
        }

        if (!email) {
          signupEmailError.textContent = 'Email is required';
          signupEmailInput.classList.add('sf-input--error');
          hasError = true;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
          signupEmailError.textContent = 'Invalid email format';
          signupEmailInput.classList.add('sf-input--error');
          hasError = true;
        }

        if (!username) {
          signupUsernameError.textContent = 'Username is required';
          signupUsernameInput.classList.add('sf-input--error');
          hasError = true;
        } else if (username.length < 3) {
          signupUsernameError.textContent = 'Username must be at least 3 characters';
          signupUsernameInput.classList.add('sf-input--error');
          hasError = true;
        } else if (!/^[a-zA-Z0-9_-]+$/.test(username)) {
          signupUsernameError.textContent = 'Username can only contain letters, numbers, hyphens, and underscores';
          signupUsernameInput.classList.add('sf-input--error');
          hasError = true;
        }

        if (!password) {
          signupPasswordError.textContent = 'Password is required';
          signupPasswordInput.classList.add('sf-input--error');
          hasError = true;
        } else if (password.length < 8) {
          signupPasswordError.textContent = 'Password must be at least 8 characters';
          signupPasswordInput.classList.add('sf-input--error');
          hasError = true;
        } else if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]/.test(password)) {
          signupPasswordError.textContent = 'Password must contain uppercase, lowercase, number, and special character';
          signupPasswordInput.classList.add('sf-input--error');
          hasError = true;
        }

        if (!confirmPassword) {
          signupConfirmPasswordError.textContent = 'Please confirm your password';
          signupConfirmPasswordInput.classList.add('sf-input--error');
          hasError = true;
        } else if (password !== confirmPassword) {
          signupConfirmPasswordError.textContent = 'Passwords do not match';
          signupConfirmPasswordInput.classList.add('sf-input--error');
          hasError = true;
        }

        if (!acceptTerms) {
          signupErrorMsg.textContent = 'You must accept the terms and conditions';
          signupError.style.display = 'flex';
          hasError = true;
        }

        if (hasError) return;

        // Disable submit button and show loading state
        signupSubmitBtn.disabled = true;
        signupBtnText.textContent = 'Creating Account...';

        try {
          // Get reCAPTCHA token
          const siteKey = document.querySelector('script[src*="recaptcha"]')?.src.match(/render=([^&]+)/)?.[1];
          let recaptchaToken = '';
          
          if (siteKey && typeof grecaptcha !== 'undefined') {
            try {
              recaptchaToken = await grecaptcha.execute(siteKey, { action: 'signup' });
            } catch (recaptchaError) {
              console.error('reCAPTCHA error:', recaptchaError);
              signupErrorMsg.textContent = 'Security verification failed. Please refresh the page and try again.';
              signupError.style.display = 'flex';
              signupSubmitBtn.disabled = false;
              signupBtnText.textContent = 'Create Account';
              return;
            }
          }

          // Get anti-forgery token
          const tokenInput = signupForm.querySelector('input[name="__RequestVerificationToken"]');
          const token = tokenInput ? tokenInput.value : '';

          const response = await fetch('/Account/RegisterAjax', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'RequestVerificationToken': token
            },
            body: JSON.stringify({
              fullName: fullName,
              email: email,
              userName: username,
              password: password,
              confirmPassword: confirmPassword,
              acceptTerms: acceptTerms,
              recaptchaToken: recaptchaToken
            })
          });

          const result = await response.json();

          if (result.success) {
            // Success! Show success message
            signupSuccessMsg.textContent = result.message || 'Account created successfully!';
            signupSuccess.style.display = 'flex';
            signupBtnText.textContent = 'Success!';

            // Redirect to dashboard after 1.5 seconds
            setTimeout(() => {
              window.location.href = result.redirectUrl || '/Dashboard';
            }, 1500);
          } else {
            // Show error message
            signupErrorMsg.textContent = result.message || 'Registration failed. Please try again.';
            signupError.style.display = 'flex';

            // Re-enable submit button
            signupSubmitBtn.disabled = false;
            signupBtnText.textContent = 'Create Account';
          }

        } catch (error) {
          console.error('Signup error:', error);
          signupErrorMsg.textContent = 'An error occurred. Please try again.';
          signupError.style.display = 'flex';

          // Re-enable submit button
          signupSubmitBtn.disabled = false;
          signupBtnText.textContent = 'Create Account';
        }
      });

      // Clear field errors on input
      if (signupFullNameInput) {
        signupFullNameInput.addEventListener('input', function () {
          signupFullNameError.textContent = '';
          this.classList.remove('sf-input--error');
        });
      }

      if (signupEmailInput) {
        signupEmailInput.addEventListener('input', function () {
          signupEmailError.textContent = '';
          this.classList.remove('sf-input--error');
        });
      }

      if (signupUsernameInput) {
        signupUsernameInput.addEventListener('input', function () {
          signupUsernameError.textContent = '';
          this.classList.remove('sf-input--error');
        });
      }

      if (signupPasswordInput) {
        signupPasswordInput.addEventListener('input', function () {
          signupPasswordError.textContent = '';
          this.classList.remove('sf-input--error');
        });
      }

      if (signupConfirmPasswordInput) {
        signupConfirmPasswordInput.addEventListener('input', function () {
          signupConfirmPasswordError.textContent = '';
          this.classList.remove('sf-input--error');
        });
      }
    }

  }); // DOMContentLoaded

})();
