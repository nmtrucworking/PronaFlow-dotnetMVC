function setActiveSidebarLink() {
    const currentHash = window.location.hash || '#/dashboard';
    document.querySelectorAll('#sidebar .sidebar__nav-item').forEach(link => {
        const linkHash = link.getAttribute('href');
        if (linkHash === currentHash) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
}

const BREAKPOINTS = { TABLET: 768, DESKTOP: 1024 };

function applySidebarState(isCollapsed) {
    const sidebar = document.getElementById('sidebar');
    const root = document.documentElement;
    if (!sidebar) return;

    if (isCollapsed) {
        sidebar.classList.add('collapsed');
        root.style.setProperty('--sidebar-width', 'var(--sidebar-width-collapsed)');
    } else {
        sidebar.classList.remove('collapsed');
        const lastWidth = localStorage.getItem('sidebarWidth') || '240px';
        root.style.setProperty('--sidebar-width', lastWidth);
    }
}

function handleSidebarState() {
    const screenWidth = window.innerWidth;
    const savedState = localStorage.getItem('sidebarCollapsed');
    const defaultStateForViewport = { isMobile: false, isTablet: true, isDesktop: false };

    let shouldBeCollapsed;
    if (screenWidth < BREAKPOINTS.TABLET) {
        shouldBeCollapsed = defaultStateForViewport.isMobile;
    } else if (screenWidth < BREAKPOINTS.DESKTOP) {
        shouldBeCollapsed = savedState !== null ? (savedState === 'true') : defaultStateForViewport.isTablet;
    } else {
        shouldBeCollapsed = savedState !== null ? (savedState === 'true') : defaultStateForViewport.isDesktop;
    }
    applySidebarState(shouldBeCollapsed);
}

function saveSidebarState(sidebar, root) {
    const isCollapsed = sidebar.classList.contains('collapsed');
    localStorage.setItem('sidebarCollapsed', isCollapsed);
    if (!isCollapsed) {
        const currentWidth = root.style.getPropertyValue('--sidebar-width-collapsed');
        localStorage.setItem('sidebarWidth', currentWidth);
    }
}

function loadSidebarState(sidebar, root) {
    const togglePointOpen = document.querySelector('.icon-open');
    const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
    if (isCollapsed) {
        sidebar.classList.add('collapsed');
        root.style.setProperty('--sidebar-width', 'var(--sidebar-width-collapsed)');
        togglePointOpen.setAttribute('visibility', 'hidden');
        togglePointOpen.removeAttribute('visibility', 'hidden');
    } else {
        sidebar.classList.remove('collapsed');
        const savedWidth = localStorage.getItem('sidebarWidth') || '240px';
        root.style.setProperty('--sidebar-width', savedWidth);
        togglePointOpen.removeAttribute('visibility');
        togglePointOpen.setAttribute('visibility', 'hidden');
    }
}

function toggleSidebarAction(sidebar, root) {
    if (window.innerWidth >= BREAKPOINTS.TABLET) {
        sidebar.classList.toggle('collapsed');
        saveSidebarState(sidebar, root);
        applySidebarState(sidebar.classList.contains('collapsed'));
    }
}

function initResize(sidebar, root, minWidth, maxWidth, onResizeEnd) {
    const resizeHandle = document.querySelector('.resize-handle');
    if (!resizeHandle) return;

    let isResizing = false;

    resizeHandle.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        isResizing = true;
        document.body.classList.add('is-resizing');
        const startX = e.clientX;
        const startWidth = sidebar.offsetWidth;

        const handleMouseMove = (e) => {
            if (!isResizing) return;
            let newWidth = startX > e.clientX ? startWidth - (startX - e.clientX) : startWidth + (e.clientX - startX);
            if (newWidth < minWidth) newWidth = minWidth;
            if (newWidth > maxWidth) newWidth = maxWidth;
            root.style.setProperty('--sidebar-width', `${newWidth}px`);
        };

        const handleMouseUp = () => {
            if (!isResizing) return;
            isResizing = false;
            document.body.classList.remove('is-resizing');
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
            if (typeof onResizeEnd === 'function') onResizeEnd();
        };

        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseup', handleMouseUp);
    });
}

function initializeSidebar() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;

    const toggleBtn = document.getElementById('sidebar-toggle-button');
    const root = document.documentElement;
    const MIN_WIDTH = 180;
    const MAX_WIDTH = 500;

    loadSidebarState(sidebar, root);

    if (toggleBtn) {
        toggleBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            toggleSidebarAction(sidebar, root);
        });
    }

    initResize(sidebar, root, MIN_WIDTH, MAX_WIDTH, () => {
        saveSidebarState(sidebar, root);
    });

    const mobileToolsGroup = document.getElementById('mobile-tools-group');
    if (mobileToolsGroup) {
        const toggleButton = mobileToolsGroup.querySelector('.mobile-group-toggle');
        toggleButton.addEventListener('click', function (event) {
            event.stopPropagation();
            mobileToolsGroup.classList.toggle('is-open');
        });
    }

    document.addEventListener('click', function () {
        if (mobileToolsGroup && mobileToolsGroup.classList.contains('is-open')) {
            mobileToolsGroup.classList.remove('is-open');
        }
    });

    window.addEventListener('resize', handleSidebarState);
    handleSidebarState();
}

document.addEventListener('DOMContentLoaded', () => {
    initializeSidebar();
    setActiveSidebarLink();
});
window.addEventListener('hashchange', setActiveSidebarLink);