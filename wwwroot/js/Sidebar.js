import { isAuthenticated, logout } from '../auth/authService.js';
import { decodeToken } from '../utils/index.js';
import store from '../store/store.js';
import apiService from '../api/apiService.js';

/**
 * Sets the 'active' class on the current sidebar navigation link
 * based on the window's URL hash.
 */
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

/**
 * A helper function to render the sidebar into a specified container element
 * and then execute its after_render logic.
 */
export async function loadSidebarAndSetActiveLink() {
    const sidebarContainer = document.getElementById('sidebar-container');
    if (sidebarContainer) {
        sidebarContainer.innerHTML = await Sidebar.render();
        await Sidebar.after_render();
    }
}

/**
 * Fetches workspaces from the API and populates the workspace selector dropdown.
 */
async function loadWorkspaces() {
    const workspaceSelector = document.getElementById('workspace-selector');
    if (!workspaceSelector) return;

    try {
        const workspaces = await apiService.workspaces.getAll();
        workspaceSelector.innerHTML = 'option value="" disabled>Choose Workspace</option>';
        if (workspaces && workspaces.length > 0) {
            workspaces.forEach(ws => {
                workspaceSelector.innerHTML += `<option value="${ws.id}">${ws.name}</option>`;
            });

            workspaceSelector.addEventListener('change', () => {
                const selectedWorkspaceId = workspaceSelector.value;
                const selectedWorkspace = workspaces.find(ws => String(ws.id) === String(selectedWorkspaceId));
                if (selectedWorkspace) {
                    store.setCurrentWorkspace(selectedWorkspace);
                    document.dispatchEvent(new CustomEvent('workspaceChanged', {
                        detail: { workspaceId: selectedWorkspace.id, workspace: selectedWorkspace }
                    }));
                }
            });

            // Ưu tiên workspace đã lưu; nếu chưa có, chọn workspace đầu tiên
            const savedWorkspace = store.getCurrentWorkspace();
            if (savedWorkspace) {
                workspaceSelector.value = savedWorkspace.id;
            } else {
                workspaceSelector.selectedIndex = 1;
                const defaultWs = workspaces[0];
                store.setCurrentWorkspace(defaultWs);
            }
            // Kích hoạt change để các trang nhận sự kiện và load dữ liệu
            workspaceSelector.dispatchEvent(new Event('change'));
        } else {
            workspaceSelector.innerHTML = '<option value="" disabled selected>No workspaces found</option>';
        }
    } catch (error) {
        console.error('Failed to load workspaces:', error);
        workspaceSelector.innerHTML = '<option>Error loading</option>';
    }
}

/**
 * @constant {object} BREAKPOINTS
 * Defines the viewport width thresholds for different device types.
 * This makes the responsive logic easier to read and maintain.
 */
const BREAKPOINTS = {
    TABLET: 768,
    DESKTOP: 1024,
};

/**
 * Determines the sidebar's state (collapsed or expanded) based on the current screen width
 * and the user's previously saved preference in localStorage.
 * This is the core logic for responsive sidebar behavior.
 */
function handleSidebarState() {
    const screenWidth = window.innerWidth;
    const savedState = localStorage.getItem('sidebarCollapsed');

    const defaultStateForViewport = {
        isMobile: false,
        isTablet: true,
        isDesktop: false,
    };

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


/**
 * Initializes the resize functionality for the sidebar.
 * It attaches mousedown, mousemove, and mouseup event listeners to a resize handle.
 * @param {HTMLElement} sidebar - The sidebar element.
 * @param {HTMLElement} root - The root element (html) for setting CSS variables.
 * @param {number} minWidth - The minimum width the sidebar can be resized to.
 * @param {number} maxWidth - The maximum width the sidebar can be resized to.
 * @param {Function} onResizeEnd - A callback function to execute when resizing is finished.
 */
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



/**
 * The main initialization function for all sidebar-related functionalities.
 * It sets up the initial state, event listeners for toggling, resizing, and responsive adjustments.
 */
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
    handleSidebarState(); // Gọi lần đầu để thiết lập trạng thái ban đầu
}

/**
 * Handles the user's click action to toggle the sidebar's collapsed state.
 * NOTE: This action is only effective on screens wider than the mobile breakpoint.
 * @param {HTMLElement} sidebar - The sidebar element.
 * @param {HTMLElement} root - The root element.
 */
function toggleSidebarAction(sidebar, root) {
    if (window.innerWidth >= BREAKPOINTS.TABLET) {
        sidebar.classList.toggle('collapsed');
        saveSidebarState(sidebar, root);
        applySidebarState(sidebar.classList.contains('collapsed'));
    }
}

/**
 * Saves the current state of the sidebar (collapsed status and width) to localStorage.
 * @param {HTMLElement} sidebar - The sidebar element.
 * @param {HTMLElement} root - The root element to get the current width from.
 */
function saveSidebarState(sidebar, root) {
    const isCollapsed = sidebar.classList.contains('collapsed'); // if contains return `true`
    localStorage.setItem('sidebarCollapsed', isCollapsed);
    if (!isCollapsed) {
        const currentWidth = root.style.getPropertyValue('--sidebar-width-collapsed');
        localStorage.setItem('sidebarWidth', currentWidth);
    }
}

/**
 * Loads and applies the saved sidebar state from localStorage when the application starts.
 * @param {HTMLElement} sidebar - The sidebar element.
 * @param {HTMLElement} root - The root element.
 */
function loadSidebarState(sidebar, root) {
    const togglePointOpen = document.querySelector('.icon-open');
    const togglePointClosed = document.querySelector('.icon-closed');
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

/**
 * Cập nhật giao diện của sidebar dựa trên trạng thái được quyết định.
 * @param {boolean} isCollapsed - True nếu sidebar nên được đóng.
 */
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