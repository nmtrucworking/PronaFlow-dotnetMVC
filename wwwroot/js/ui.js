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

export function setLoadingState(button, isLoading, text) {
    button.disabled = isLoading;
    button.textContent = isLoading ? 'Loading...' : text;
}

export function toggleForms(container, show) {
    if (show === 'register') {
        container.classList.add('active');
    } else {
        container.classList.remove('active');
    }
}