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

document.addEventListener('DOMContentLoaded', () => {
    initBackdropHome();
});