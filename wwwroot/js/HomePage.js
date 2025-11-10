document.addEventListener('DOMContentLoaded', () => {
    initBackdropHome();

    initializeHomePage();

    if (window.lucide && window.lucide.createIcons) {
        window.lucide.createIcons();
    }
});

// backdrop for Login/Register cards
function initBackdropHome() {
    const backdrop = document.getElementById('backdrop');
    const loginBtn = document.getElementById('login-btn');
    const registerBtn = document.getElementById('sigup-btn');
    const loginCard = document.getElementById('login-card');
    const registerCard = document.getElementById('register-card');

    // Nếu thiếu phần tử cần thiết thì bỏ qua, tránh lỗi
    if (!backdrop || !loginCard || !registerCard) {
        return;
    }

    // Khóa/Mở khóa scroll cho trang Home
    const setScrollLock = (lock) => {
        document.body.style.overflow = lock ? 'hidden' : '';
    };

    // Đặt vị trí form vào giữa viewport bằng inline style (không phụ thuộc scroll)
    const centerCard = (card) => {
        if (!card) return;
        card.style.position = 'fixed';
        card.style.top = '50%';
        card.style.left = '50%';
        card.style.transform = 'translate(-50%, -50%)';
        card.style.margin = '0'; // tránh margin đẩy lệch
        card.style.zIndex = '20001'; // đảm bảo cao hơn backdrop
    };

    // Trả form về style mặc định (để không phá CSS khác)
    const resetCardPosition = (card) => {
        if (!card) return;
        card.style.position = '';
        card.style.top = '';
        card.style.left = '';
        card.style.transform = '';
        card.style.margin = '';
        card.style.zIndex = '';
    };

    const focusFirstInput = (card) => {
        const input = card.querySelector('input, textarea, select, button');
        if (input && typeof input.focus === 'function') {
            input.focus();
        }
    };

    // Sử dụng showToast từ ui.js (được gán vào window trong Index.cshtml)
    const showToast = typeof window.showToast === 'function'
        ? window.showToast
        : (msg, type = 'success') => console.warn('showToast missing:', type, msg);

    // Submit form qua AJAX và xử lý kết quả (bind 1 lần)
    const attachAjaxSubmit = (form, successMessage) => {
        if (!form || form.dataset.ajaxBound === 'true') return;
        form.dataset.ajaxBound = 'true';

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) submitBtn.disabled = true;

            try {
                const formData = new FormData(form);
                const res = await fetch(form.action, {
                    method: 'POST',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    body: formData
                });

                const contentType = res.headers.get('content-type') || '';
                if (contentType.includes('application/json')) {
                    const data = await res.json();
                    if (data.success) {
                        showToast(successMessage || 'Thao tác thành công', 'success');
                        closeAll();
                        if (data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        }
                    } else {
                        const msg = Array.isArray(data.errors) && data.errors.length
                            ? data.errors.join('\n')
                            : 'Thao tác thất bại. Vui lòng kiểm tra lại.';
                        showToast(msg, 'error');
                    }
                } else {
                    if (res.redirected) {
                        showToast(successMessage || 'Thao tác thành công', 'success');
                        closeAll();
                        window.location.href = res.url;
                    } else if (!res.ok) {
                        showToast('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
                    }
                }
            } catch (err) {
                showToast('Lỗi mạng hoặc máy chủ. Vui lòng thử lại.', 'error');
                console.error('Auth submit error:', err);
            } finally {
                if (submitBtn) submitBtn.disabled = false;
            }
        });
    };

    // Bind AJAX submit ngay khi khởi tạo (không đợi mở modal)
    attachAjaxSubmit(loginCard.querySelector('form[role="form"]'), 'Đăng nhập thành công');
    attachAjaxSubmit(registerCard.querySelector('form[role="form"]'), 'Đăng ký thành công');

    const openLogin = () => {
        backdrop.classList.add('is-visible');
        loginCard.classList.add('is-open');
        registerCard.classList.remove('is-open');
        centerCard(loginCard);
        resetCardPosition(registerCard);
        setScrollLock(true);
        focusFirstInput(loginCard);
    };

    const openRegister = () => {
        backdrop.classList.add('is-visible');
        loginCard.classList.remove('is-open');
        registerCard.classList.add('is-open');
        centerCard(registerCard);
        resetCardPosition(loginCard);
        setScrollLock(true);
        focusFirstInput(registerCard);
    };

    const closeAll = () => {
        backdrop.classList.remove('is-visible');
        loginCard.classList.remove('is-open');
        registerCard.classList.remove('is-open');
        resetCardPosition(loginCard);
        resetCardPosition(registerCard);
        setScrollLock(false);
    };

    // Mở form qua buttons (nếu tồn tại)
    if (loginBtn) loginBtn.addEventListener('click', openLogin);
    if (registerBtn) registerBtn.addEventListener('click', openRegister);

    // Đóng form khi click vào backdrop (chỉ khi click đúng backdrop)
    backdrop.addEventListener('click', (e) => {
        if (e.target === backdrop) {
            closeAll();
        }
    });

    // Đóng bằng phím Escape
    const onEsc = (e) => {
        if (e.key === 'Escape') {
            closeAll();
        }
    };
    document.addEventListener('keydown', onEsc);
}

function initializeHomePage() {
    
    // Xử lý menu toggle cho mobile
    const menuToggle = document.getElementById('menu-toggle');
    const mainNav = document.getElementById('main-nav');
    
    if (menuToggle && mainNav) {
      menuToggle.addEventListener('click', () => {
        mainNav.classList.toggle('active');
        menuToggle.classList.toggle('active');
      });
    }
    
    // Xử lý các tab tính năng
    const featureTabs = document.querySelectorAll('.feature-tab');
    featureTabs.forEach(tab => {
      tab.addEventListener('click', () => {
        // Xóa active class từ tất cả các tab
        featureTabs.forEach(t => t.classList.remove('active'));
        
        // Thêm active class cho tab được click
        tab.classList.add('active');
        
        // Hiển thị nội dung tương ứng
        const tabId = tab.getAttribute('data-tab');
        document.querySelectorAll('.feature-content').forEach(content => {
          content.classList.remove('active');
        });
        document.getElementById(`${tabId}-content`)?.classList.add('active');
      });
    });
    
    // Smooth scrolling cho các anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
      anchor.addEventListener('click', function(e) {
        const href = this.getAttribute('href');
        
        // Nếu là link điều hướng SPA thì không xử lý
        if (href.startsWith('#/')) return;
        
        e.preventDefault();
        
        const targetId = href === '#' ? 'home-page' : href.substring(1);
        const targetElement = document.getElementById(targetId);
        
        if (targetElement) {
          window.scrollTo({
            top: targetElement.offsetTop - 100,
            behavior: 'smooth'
          });
        }
      });
    });
    

    // Initialize scroll-based header style changes
    
    const header = document.getElementById('header');
    if (header) {
        window.addEventListener('scroll', () => {
            header.classList.toggle('scrolled', window.scrollY > 50);
        });
    }

    const typingElement = document.getElementById('typing-effect');
    if (typingElement) {
        const words = ["Chaos to Clarity.", "Ideas to Action.", "Work in Flow."];
        let wordIndex = 0;
        let charIndex = 0;
        let isDeleting = false;
        const type = () => {
            const currentWord = words[wordIndex];
            const typeSpeed = isDeleting ? 75 : 150;
            if (isDeleting) {
                typingElement.textContent = currentWord.substring(0, charIndex - 1);
                charIndex--;
                if (charIndex === 0) {
                    isDeleting = false;
                    wordIndex = (wordIndex + 1) % words.length;
                }
            } else {
                typingElement.textContent = currentWord.substring(0, charIndex + 1);
                charIndex++;
                if (charIndex === currentWord.length) {
                    setTimeout(() => isDeleting = true, 2000);
                }
            }
            setTimeout(type, typeSpeed);
        };
        type();
    }

    const animatedElements = document.querySelectorAll('.animate-on-scroll');
    if (animatedElements.length > 0) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });
        animatedElements.forEach(element => observer.observe(element));
    }
}

