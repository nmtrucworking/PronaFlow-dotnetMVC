import { isAuthenticated, logout } from '../auth/authService.js';
import { loadSidebarAndSetActiveLink } from '../components/Sidebar.js';
import apiService from '../api/apiService.js';
import store from '../store/store.js';
import { showToast } from '../utils/ui.js';

const SettingPage = {
    render: async () => {
        if (!isAuthenticated()) {
            window.location.hash = '#/setting'; 
            return ''; 
        }
        const response = await fetch('./src/pages/settings.html');
        const html = await response.text();
        return html;
    },
    
    after_render: async () => {
        if (!isAuthenticated()) return;
        await loadSidebarAndSetActiveLink();
        if (window.lucide && window.lucide.createIcons) {
            window.lucide.createIcons();
        }
        initializeSettingsPage();
    }
};

/**
 * Main function to initialize interactions on the Settings page.
 */
function initializeSettingsPage() {
    
    // --- STATE ---
    let workspaces = [];
    let currentTags = [];
    let selectedWorkspaceId = null;

    // --- DOM ELEMENTS ---
    const displayNameInput = document.getElementById('display-name');
    const bioInput = document.getElementById('bio');
    const saveProfileBtn = document.getElementById('save-profile-btn');
    
    const emailInput = document.getElementById('change-email');
    const currentPassword = document.getElementById('current-password');
    const userPassword = document.getElementById('user-password');
    const comfirmPassword = document.getElementById('comfirm-password');
    
    const workspaceSelector = document.getElementById('current-workspace');
    const tagsContainer = document.getElementById('tags-list-container');
    const emptyTagsMessage = document.getElementById('tags-list-empty');
    const newTagNameInput = document.getElementById('new-tag-name');
    const newTagColorInput = document.getElementById('new-tag-color');
    const addNewTagBtn = document.getElementById('add-new-tag-btn');
    const colorPickerCircle = document.querySelector('.color-picker-circle');
    
    const logoutBtn = document.getElementById('logout-btn');
    const deleteConfirmInput = document.getElementById('delete-confirm');
    const deleteAccountBtn = document.getElementById('delete-account-btn');

    // --- FUNCTIONS ---

    // Load user data into profile form
    function loadProfileData() {
        const user = store.getState().user;
        if (user) {
            displayNameInput.value = user.fullName || '';
            bioInput.value = user.bio || '';
        }
    }

    // Load workspaces into selector
    async function loadWorkspaces() {
        try {
            workspaces = await apiService.workspaces.getAll();
            workspaceSelector.innerHTML = workspaces
                .map(ws => `<option value="${ws.id}">${ws.name}</option>`)
                .join('');
            if (workspaces.length > 0) {
                selectedWorkspaceId = workspaces[0].id;
                await loadTagsForWorkspace(selectedWorkspaceId);
            }
        } catch (error) {
            showToast("Failed to load workspaces.", "error");
        }
    }

    // Load tags for the selected workspace
    async function loadTagsForWorkspace(workspaceId) {
        if (!workspaceId) return;
        selectedWorkspaceId = workspaceId;
        try {
            currentTags = await apiService.tags.getForWorkspace(workspaceId);
            renderTags();
        } catch (error) {
            showToast("Failed to load tags for this workspace.", "error");
        }
    }

    // Render the list of tags
    function renderTags() {
        tagsContainer.innerHTML = '';
        if (currentTags.length === 0) {
            emptyTagsMessage.style.display = 'block';
            return;
        }
        emptyTagsMessage.style.display = 'none';
        currentTags.forEach(tag => {
            const tagEl = document.createElement('div');
            tagEl.className = 'tag-list-item'; // You need to style this class
            tagEl.innerHTML = `
                <span class="tag-card" style="background-color: ${tag.colorHex};">${tag.name}</span>
                <button class="btn-delete-tag" data-tag-id="${tag.id}"><i data-lucide="x"></i></button>
            `;
            tagsContainer.appendChild(tagEl);
        });
        if (window.lucide && window.lucide.createIcons) {
            window.lucide.createIcons();
        }
    }

    // --- EVENT LISTENERS ---

    // Save Profile
    saveProfileBtn.addEventListener('click', async () => {
        const fullName = displayNameInput.value.trim();
        const bio = bioInput.value.trim();
        try {
            await apiService.users.updateCurrentUser({ fullName, bio });
            // Refresh user data in store
            await authService.checkAuthStatus(); 
            showToast("Profile updated successfully!", "success");
        } catch (error) {
            showToast("Failed to update profile.", "error");
        }
    });

    // Workspace Selector
    workspaceSelector.addEventListener('change', () => {
        loadTagsForWorkspace(workspaceSelector.value);
    });

    // Add New Tag
    addNewTagBtn.addEventListener('click', async () => {
        const name = newTagNameInput.value.trim();
        const colorHex = newTagColorInput.value;
        if (!name) {
            showToast("Tag name cannot be empty.", "error");
            return;
        }
        try {
            await apiService.tags.create(selectedWorkspaceId, { name, colorHex });
            newTagNameInput.value = '';
            await loadTagsForWorkspace(selectedWorkspaceId);
            showToast("Tag created successfully!", "success");
        } catch (error) {
            showToast("Failed to create tag.", "error");
        }
    });

    // Delete Tag (using event delegation)
    tagsContainer.addEventListener('click', async (e) => {
        const deleteBtn = e.target.closest('.btn-delete-tag');
        if (deleteBtn) {
            const tagId = deleteBtn.dataset.tagId;
            if (confirm("Are you sure you want to delete this tag?")) {
                try {
                    await apiService.tags.delete(tagId);
                    await loadTagsForWorkspace(selectedWorkspaceId);
                    showToast("Tag deleted.", "success");
                } catch (error) {
                    showToast("Failed to delete tag.", "error");
                }
            }
        }
    });
    
    // Color Picker UI
    newTagColorInput.addEventListener('input', () => {
        colorPickerCircle.style.backgroundColor = newTagColorInput.value;
    });

    // Logout Button
    logoutBtn.addEventListener('click', () => {
        logout();
        window.location.hash = '#/login';
    });
    
    // Delete Account confirmation
    deleteConfirmInput.addEventListener('input', () => {
        deleteAccountBtn.disabled = deleteConfirmInput.value !== 'DELETE';
    });
    
    deleteAccountBtn.addEventListener('click', async () => {
        if (deleteConfirmInput.value === 'DELETE') {
            if (confirm("This action is IRREVERSIBLE. Are you absolutely sure you want to delete your account?")) {
                try {
                    await apiService.users.deleteCurrentUser();
                    logout(); // Log out on client side
                    alert("Your account has been permanently deleted.");
                    window.location.hash = '#/login';
                } catch (error) {
                    showToast("Failed to delete account.", "error");
                }
            }
        }
    });

    // --- INITIALIZATION ---
    loadProfileData();
    loadWorkspaces();
    // initializePlugin();
}

function initializePlugin() {
    const {
            gsap: { registerPlugin, set, to, timeline },
            MorphSVGPlugin,
            Draggable
        } = window;
        registerPlugin(MorphSVGPlugin);

        let startX, startY;

        const AUDIO = {
            CLICK: new Audio('https://assets.codepen.io/605876/click.mp3')
        };

        const CORD_DURATION = 0.1;
        const CORDS = document.querySelectorAll('.toggle-scene__cord');
        const HIT = document.querySelector('.toggle-scene__hit-spot');
        const DUMMY_CORD = document.querySelector('.toggle-scene__dummy-cord line');
        const PROXY = document.createElement('div');

        const ENDX = DUMMY_CORD.getAttribute('x2');
        const ENDY = DUMMY_CORD.getAttribute('y2');
        const RESET = () => set(PROXY, { x: ENDX, y: ENDY });

        RESET();

        // Timeline này giờ chỉ tập trung vào animation kéo dây
        const CORD_TL = timeline({
            paused: true,
            onStart: () => {
                // KHI ANIMATION BẮT ĐẦU, GỌI HÀM TOGGLE THEME TOÀN CỤC
                if (typeof toggleTheme === 'function') {
                    toggleTheme();
                }

                // Phát âm thanh và quản lý các thành phần của animation
                AUDIO.CLICK.play();
                set([HIT], { display: 'none' });
                set(CORDS[0], { display: 'block' });
            },
            onComplete: () => {
                // Reset lại các thành phần của animation
                set([HIT], { display: 'block' });
                set(CORDS[0], { display: 'none' });
                RESET();
            }
        });

        // Xây dựng chuỗi animation
        for (let i = 1; i < CORDS.length; i++) {
            CORD_TL.add(
                to(CORDS[0], {
                    morphSVG: CORDS[i],
                    duration: CORD_DURATION,
                    repeat: 1,
                    yoyo: true
                })
            );
        }

        // Thiết lập sự kiện kéo
        Draggable.create(PROXY, {
            trigger: HIT,
            type: 'x,y',
            onPress: e => {
                startX = e.x;
                startY = e.y;
            },
            onDrag: function () {
                set(DUMMY_CORD, { attr: { x2: this.x, y2: this.y } });
            },
            onRelease: function (e) {
                const DISTX = Math.abs(e.x - startX);
                const DISTY = Math.abs(e.y - startY);
                const TRAVELLED = Math.sqrt(DISTX * DISTX + DISTY * DISTY);

                to(DUMMY_CORD, {
                    attr: { x2: ENDX, y2: ENDY },
                    duration: CORD_DURATION,
                    onComplete: () => {
                        // Nếu kéo đủ xa, chạy animation
                        if (TRAVELLED > 50) {
                            CORD_TL.restart();
                        } else {
                            RESET();
                        }
                    }
                });
            }
        });
}
export default SettingPage;