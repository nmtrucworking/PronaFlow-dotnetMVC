export function showToast(message, type = 'success', duration = 3000) {
    let toast = document.getElementById('toast-notification');
    let toastMessage = document.getElementById('toast-message');

    // Create toast container if missing
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'toast-notification';
        toast.className = 'toast';
        toast.innerHTML = `
            <div class="toast-content">
                <span id="toast-message"></span>
                <button class="toast-close">&times;</button>
            </div>
        `;
        document.body.appendChild(toast);

        // Add click handler for close button
        const closeButton = toast.querySelector('.toast-close');
        closeButton.addEventListener('click', () => {
            toast.className = toast.className.replace('show', '');
        });

        // Re-query after creating
        toastMessage = toast.querySelector('#toast-message');
    }

    toastMessage.textContent = message;
    toast.className = `toast show ${type}`;

    // Auto-hide after duration
    toast.timeoutId = setTimeout(() => {
        toast.className = toast.className.replace('show', '');
    }, duration);
}
window.showToast = showToast;
export function setLoadingState(button, isLoading, text) {
    button.disabled = isLoading;
    button.textContent = isLoading ? 'Loading...' : text;
}
window.setLoadingState = setLoadingState;

// export function toggleForms(container, show) {
//     if (show === 'register') {
//         container.classList.add('active');
//     } else {
//         container.classList.remove('active');
//     }
// }

// ...existing code...

/* 
createBackdropManager({
        backdropSelector: '#backdrop',
        items: [
            {
                openBtnSelector: '#login-btn',
                cardSelector: '#login-card',
                formRole: 'form',
                successMessage: 'Đăng nhập thành công'
            },
            {
                openBtnSelector: '#sigup-btn',
                cardSelector: '#register-card',
                formRole: 'form',
                successMessage: 'Đăng ký thành công'
            }
        ]
    });
*/

// Chuyển thành hàm tổng: createBackdropManager(config)
function createBackdropManager(config = {}) {
    const backdrop = document.querySelector(config.backdropSelector || '#backdrop');
    const items = Array.isArray(config.items) ? config.items : [];

    if (!backdrop) return;

    const setScrollLock = (lock) => {
        document.body.style.overflow = lock ? 'hidden' : '';
    };

    const centerCard = (card) => {
        if (!card) return;
        card.style.position = 'fixed';
        card.style.top = '50%';
        card.style.left = '50%';
        card.style.transform = 'translate(-50%, -50%)';
        card.style.margin = '0';
        card.style.zIndex = '20001';
    };

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
        if (!card) return;
        const input = card.querySelector('input, textarea, select, button');
        if (input && typeof input.focus === 'function') input.focus();
    };

    const showToast = typeof window.showToast === 'function'
        ? window.showToast
        : (msg, type = 'success') => console.warn('showToast missing:', type, msg);

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
                        if (data.redirectUrl) window.location.href = data.redirectUrl;
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

    // Build map of card elements for easy control
    const cards = items.reduce((map, it) => {
        const card = document.querySelector(it.cardSelector);
        if (card) map[it.cardSelector] = { el: card, successMessage: it.successMessage || null };
        return map;
    }, {});

    // Attach AJAX forms and open buttons
    items.forEach(it => {
        const card = document.querySelector(it.cardSelector);
        if (!card) return;

        // Attach AJAX submit if form specified
        if (it.formRole) {
            const form = card.querySelector(`form[role="${it.formRole}"]`);
            attachAjaxSubmit(form, it.successMessage);
        }

        // Bind open button
        if (it.openBtnSelector) {
            const btn = document.querySelector(it.openBtnSelector);
            if (btn) {
                btn.addEventListener('click', () => openCard(card));
            }
        }
    });

    const openCard = (cardToOpen) => {
        backdrop.classList.add('is-visible');

        // mark classes on cards: add .is-open on the opened one, remove from others
        Object.values(cards).forEach(({ el }) => {
            if (el === cardToOpen) {
                el.classList.add('is-open');
                centerCard(el);
                focusFirstInput(el);
            } else {
                el.classList.remove('is-open');
                resetCardPosition(el);
            }
        });

        setScrollLock(true);
    };

    const closeAll = () => {
        backdrop.classList.remove('is-visible');
        Object.values(cards).forEach(({ el }) => {
            el.classList.remove('is-open');
            resetCardPosition(el);
        });
        setScrollLock(false);
    };

    // Backdrop click closes when clicking exact backdrop element
    backdrop.addEventListener('click', (e) => {
        if (e.target === backdrop) closeAll();
    });

    // Escape to close
    const onEsc = (e) => {
        if (e.key === 'Escape') closeAll();
    };
    document.addEventListener('keydown', onEsc);

    // Provide a small API for external usage if needed
    return {
        open: (selectorOrElement) => {
            const el = typeof selectorOrElement === 'string' ? document.querySelector(selectorOrElement) : selectorOrElement;
            if (el) openCard(el);
        },
        close: closeAll
    };
}

import store from '../store/store.js';

export function autoResizeTextarea(selector) {
    document.querySelectorAll(selector).forEach(textarea => {
        const resize = () => {
            textarea.style.height = 'auto';
            textarea.style.height = `${textarea.scrollHeight}px`;
        };
        textarea.addEventListener('input', resize);
        resize();
    });
}

/**
 * Kích hoạt chức năng chỉnh sửa nội tuyến cho một phần tử.
 * @param {HTMLElement} container - Phần tử container bao quanh.
 */
export function enableInlineEditing(container) {
    if (!container) return;

    const displayElement = container.querySelector('.inline-editable__display');
    const editContainer = container.querySelector('.inline-editable__edit');
    const inputElement = editContainer.querySelector('.inline-edit-input');

    if (!displayElement || !editContainer || !inputElement) return;

    container.addEventListener('click', () => {
        if (container.classList.contains('is-editing')) return; // Tránh gọi lại khi đang sửa
        container.classList.add('is-editing');
        inputElement.value = displayElement.textContent.trim();
        inputElement.focus();
        inputElement.select();
    });

    const finishEditing = () => {
        const newValue = inputElement.value.trim();
        if (newValue) {
            displayElement.textContent = newValue;
        } else {
            displayElement.textContent = container.dataset.placeholder || 'Untitled';
        }
        container.classList.remove('is-editing');
    };

    inputElement.addEventListener('blur', finishEditing);
    inputElement.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            finishEditing();
        } else if (e.key === 'Escape') {
            inputElement.value = displayElement.textContent.trim(); // Hoàn lại giá trị cũ
            container.classList.remove('is-editing');
        }
    });
}


/**
 * Renders the greeting widget based on the time of day and user's name.
 */
export function renderGreetingWidget() {
    const greetingWidget = document.getElementById('greeting-widget');
    if (!greetingWidget) return;

    const hour = new Date().getHours();
    const user = store.getState().user;
    const userName = user?.fullName || 'Guest';

    let greetingText = (hour < 12) ? "Good morning" : (hour < 18) ? "Good afternoon" : "Good night";
    greetingWidget.textContent = `${greetingText}, ${userName}`;
}