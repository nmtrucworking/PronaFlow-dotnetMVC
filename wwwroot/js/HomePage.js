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
    if (!backdrop || !loginBtn || !registerBtn || !loginCard || !registerCard) {
        return;
    }

    loginBtn.addEventListener('click', () => {
        backdrop.classList.add('is-visible');
        loginCard.classList.add('is-open');
        registerCard.classList.remove('is-open');
    });

    registerBtn.addEventListener('click', () => {
        backdrop.classList.add('is-visible');
        loginCard.classList.remove('is-open');
        registerCard.classList.add('is-open');
    });
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

