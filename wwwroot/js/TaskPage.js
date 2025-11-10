import  store  from '../store/store.js';
import apiService from '../api/apiService.js';
import performanceUtils from '../utils/performance.js';
import { loadSidebarAndSetActiveLink } from '../components/Sidebar.js';
import { showToast } from '../utils/ui.js';
import { isAuthenticated } from '../auth/authService.js';

const { throttle, renderLongList } = performanceUtils;


const MyTaskPage = {
    /**
     * Render a view of the page.
     * HTML is merged from my-task.html.
     */
    render: async () => {
        if (!isAuthenticated()) {
            window.location.hash = '#/login';
            return '';
        }
        // Fetch correct path under wwwroot/src/pages
        const taskPage = await fetch('./src/pages/my-task.html');
        const taskPageHTML = await taskPage.text();
        return taskPageHTML;
    },
    
    /**
     * Execute script after rendering.
     * All logic from my-task.js is moved and adapted here.
     */
    after_render: async () => {
        if (!isAuthenticated()) return;

        try {
            await loadSidebarAndSetActiveLink();
            
            if (window.lucide) {
                lucide.createIcons();
            }
            
            await initializeMyTasksPage();

            // Lắng nghe thay đổi workspace và reload theo project đầu tiên của workspace
            document.addEventListener('workspaceChanged', async (e) => {
                const workspaceId = e.detail?.workspaceId;
                await selectProjectForWorkspace(workspaceId);
                await loadTasks();
            });

            // Load ban đầu theo workspace hiện tại
            await selectProjectForWorkspace(store.getCurrentWorkspace()?.id);
            await loadTasks();
            
        } catch (error) {
            console.error('Error in after_render:', error);
        }
    }
};

export default MyTaskPage;

async function loadTasks() {
    const taskListContainer = document.getElementById('task-list-container');
    const emptyState = document.getElementById('empty-state-tasks');
    
    // Guard against missing container
    if (!taskListContainer) {
        console.warn('MyTaskPage: #task-list-container not found.');
        return;
    }
    
    try {
        taskListContainer.innerHTML = '<div class="loading-spinner"></div>';

        // Lấy project hiện tại từ store
        const currentProject = store.getCurrentProject();
        if (!currentProject || !currentProject.id) {
            if (emptyState) emptyState.style.display = 'flex';
            taskListContainer.innerHTML = `
                <div class="info-state">
                    <p>Please select a project to view tasks.</p>
                </div>
            `;
            return;
        }

        // Đọc giá trị tìm kiếm từ input nếu có
        const searchInput = document.querySelector('.search-bar__input');
        const searchText = searchInput ? searchInput.value.trim() : '';

        // Gọi API với projectId và tham số tìm kiếm
        const tasks = await apiService.tasks.getAll(currentProject.id, { ...queryParams, search: searchText });
        
        if (!tasks || tasks.length === 0) {
            if (emptyState) emptyState.style.display = 'flex';
            taskListContainer.innerHTML = '';
            return;
        }

        if (emptyState) emptyState.style.display = 'none';
        
        // Render with grouping if selected
        if (queryParams.groupBy && queryParams.groupBy !== 'none') {
            taskListContainer.innerHTML = '';
            const groups = groupTasks(tasks, queryParams.groupBy);
            const order = getGroupOrder(queryParams.groupBy);
            order.forEach(groupName => {
                const list = groups[groupName] || [];
                if (!list.length) return;
                const section = createGroupSection(groupName, list);
                taskListContainer.appendChild(section);
            });
        } else {
            renderLongList(taskListContainer, tasks, renderTaskCard);
        }
        
        // Khởi tạo lại icon sau khi render
        if (window.lucide && window.lucide.createIcons) {
            window.lucide.createIcons();
        }
    } catch (error) {
        showToast('Failed to load tasks', 'error');
        console.error('Error loading tasks:', error);
        taskListContainer.innerHTML = `
            <div class="error-state">
                <p>Failed to load tasks. Please try again.</p>
                <button id="retry-load-tasks" class="btn btn--primary btn--sm">Retry</button>
            </div>
        `;
        const retryBtn = document.getElementById('retry-load-tasks');
        if (retryBtn) {
            retryBtn.addEventListener('click', loadTasks);
        }
    }
}


function renderTaskCard(task) {
    return `
        <div class="task-card" data-task-id="${task.id}" style="background: var(--color-background-${task.status});">
            <label class="custom-checkbox">
                <input type="checkbox" ${task.status === 'done' ? 'checked' : ''}>
                <span class="custom-checkbox__checkmark round"></span>
            </label>
            <div class="task-card__content">
                <span class="task__name">${task.name}</span>
                <div class="task-card__detail">
                    <div class="task__address">
                        <span id="taskAddress__prjId">${task.projectName || 'Uncategorized'}</span>
                        <span> / </span>
                        <span id="taskAddress__tasklistId">${task.taskListName || '-'}</span>
                    </div>
                    <div class="task__deadline">
                        <i data-lucide="calendar-fold" class="icon--minium"></i>
                        <span>${task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'Not set'}</span>
                    </div>
                </div>
            </div>
            <button class="btn priority-${task.priority}"><i data-lucide="star"></i></button>
        </div>`;
}

/**
 * @file This file contains the core logic for the "My Tasks" page.
 * @summary It handles state management, event listeners, API interactions, and UI updates for tasks.
 */

/**
 * Main function to initialize all interactions and logic on the My Tasks page.
 * This acts as the entry point for the page's client-side functionality.
 */
function initializeMyTasksPage() {

    // --- 1. DOM ELEMENT SELECTION ---
    // Select all necessary elements from the DOM to avoid repeated queries.
    const taskListContainer = document.getElementById('task-list-container');
    const detailPanel = document.getElementById('task-detail-panel');
    const detailContent = document.getElementById('task-detail-content');
    const emptyStateMessage = document.getElementById('empty-state-message');
    const closeTaskDetailBtn = document.getElementById('close-task-detail-btn');
    const addTaskForm = document.getElementById('add-task-form');
    const newTaskInput = document.getElementById('new-task-input');
    const deleteTaskBtn = document.getElementById('delete-task-btn');

    // --- 2. STATE MANAGEMENT ---
    // These variables hold the client-side state of the page.

    // Holds the ID of the currently selected task for viewing/editing in the detail panel.
    let currentSelectedTaskId = null;

    // A temporary object to hold attributes for a new task before it's created.
    // This object is populated by interactions with various popovers.
    let newTaskData = {
        name: '',
        description: null,
        priority: 'normal', // Default priority
        assigneeIds: [],
        startDate: null,
        endDate: null,
        taskListId: null // **CRITICAL:** This must be set before creation.
    };
    
    // Holds the current filter and sort parameters for fetching tasks.
    // Phạm vi module: tham số truy vấn cho MyTasks
    let queryParams = {
        search: '',
        status: null,
        assigneeId: null,
        sortBy: 'creation-date',
        sortDir: 'desc',
        groupBy: 'none'
    };


    // --- 3. CORE FUNCTIONS ---

    /**
     * Resets the detail panel to its initial empty state.
     * It hides the task details, shows the placeholder message, and deselects any active task.
     */
    const resetToEmptyState = () => {
        if (detailContent) detailContent.style.display = 'none';
        if (emptyStateMessage) emptyStateMessage.style.display = 'flex';
        
        const activeCard = taskListContainer ? taskListContainer.querySelector('.active-task') : null;
        if (activeCard) {
            activeCard.classList.remove('active-task');
        }
        
        currentSelectedTaskId = null;
        // Reset the form in the detail panel to clear old data.
        const form = detailPanel ? detailPanel.querySelector('form') : null;
        if (form && typeof form.reset === 'function') form.reset();
    };

    /**
     * Fetches detailed information for a specific task and populates the detail panel.
     * @param {string} taskId - The ID of the task to display.
     */
    const displayTaskDetails = async (taskId) => {
        try {
            // Fetch full task details from the API to ensure data is up-to-date.
            // Note: The backend endpoint for getting a single task needs to be implemented.
            // Assuming `apiService.tasks.getById(taskId)` exists.
            const task = await apiService.tasks.getById(taskId);

            if (!task) {
                showToast('Could not fetch task details.', 'error');
                return;
            }

            currentSelectedTaskId = taskId;
            
            // Populate the detail panel form with the fetched data.
            const idInput = document.getElementById('task-id-input');
            if (idInput) idInput.value = task.id;
            const nameInput = document.getElementById('detail-taskName');
            if (nameInput) nameInput.value = task.name;
            const descInput = document.getElementById('detail-task-description');
            if (descInput) descInput.value = task.description || '';
            const prioritySelect = document.getElementById('detail-task-priority');
            if (prioritySelect) prioritySelect.value = task.priority || 'normal';
            const statusSelect = document.getElementById('detail-task-status');
            if (statusSelect) statusSelect.value = task.status || 'not-started';
            // Đồng bộ checkbox trạng thái theo giá trị status
            const statusCheckboxEl = document.getElementById('detail-taskStatusCheckbox');
            if (statusCheckboxEl) statusCheckboxEl.checked = (task.status === 'done');
            
            // Populate dates
            const startDisplay = document.getElementById('detail-task-start-date-display');
            if (startDisplay) startDisplay.textContent = task.startDate ? new Date(task.startDate).toLocaleDateString() : 'No start date';
            const endDisplay = document.getElementById('detail-task-end-date-display');
            if (endDisplay) endDisplay.textContent = task.endDate ? new Date(task.endDate).toLocaleDateString() : 'No end date';

            // Populate project/tasklist info
            const prjEl = document.getElementById('detail-taskAddress__prjId');
            if (prjEl) prjEl.textContent = task.projectName || 'Project';
            const tlEl = document.getElementById('detail-taskAddress__tasklistId');
            if (tlEl) tlEl.textContent = task.taskListName || 'Task-list';

            // Show the content and hide the empty state message.
            if (emptyStateMessage) emptyStateMessage.style.display = 'none';
            if (detailContent) detailContent.style.display = 'block';

        } catch (error) {
            console.error("Error fetching task details:", error);
            showToast('Failed to load task details.', 'error');
        }
    };

    /**
     * Handles debounced updates for text-based inputs in the detail panel.
     * This prevents excessive API calls while the user is typing.
     */
    const handleDetailUpdate = performanceUtils.debounce(async (updateData) => {
        if (!currentSelectedTaskId) return;
        try {
            await taskOperations.updateTask(currentSelectedTaskId, updateData);
        } catch (error) {
            // Error is handled within taskOperations
        }
    }, 500); // 500ms delay after user stops typing


    // --- 4. EVENT LISTENER SETUP ---

    // Close the detail panel when the 'X' button is clicked.
    if (closeTaskDetailBtn) {
        closeTaskDetailBtn.addEventListener('click', resetToEmptyState);
    }

    // Handle clicks within the main task list container (Event Delegation).
    if (taskListContainer) {
        taskListContainer.addEventListener('click', (event) => {
            const clickedCard = event.target.closest('.task-card');
            if (clickedCard && !event.target.closest('.custom-checkbox')) {
                // Deselect previous card and select the new one.
                const currentlyActive = taskListContainer.querySelector('.active-task');
                if (currentlyActive) currentlyActive.classList.remove('active-task');
                
                clickedCard.classList.add('active-task');
                displayTaskDetails(clickedCard.dataset.taskId);
            }
        });
    }

    // Handle the creation of a new task.
    if (addTaskForm) {
        addTaskForm.addEventListener('submit', async (event) => {
            event.preventDefault();
            const taskName = newTaskInput ? newTaskInput.value.trim() : '';
            
            // Basic validation
            if (taskName === '') {
                showToast('Task name cannot be empty.', 'error');
                return;
            }
            // **CRITICAL BUSINESS LOGIC:** A task must belong to a TaskList.
            // We require the user to have selected a project/tasklist via the popovers.
            if (!newTaskData.taskListId) {
                showToast('Please select a project for the new task.', 'error');
                return;
            }

            newTaskData.name = taskName;

            try {
                await taskOperations.createTask(newTaskData.taskListId, newTaskData);
                
                // Reset form and temporary state object for the next task.
                if (newTaskInput) newTaskInput.value = '';
                newTaskData = {
                    name: '', description: null, priority: 'normal', assigneeIds: [],
                    startDate: null, endDate: null, taskListId: null
                };
                // You might want to visually reset the popover-related buttons here as well.

            } catch (error) {
                // Error is already handled inside taskOperations.
                return;
            }
        });
    }

    // Handle the deletion of the currently selected task.
    if (deleteTaskBtn) {
        deleteTaskBtn.addEventListener('click', async () => {
            if (!currentSelectedTaskId || !confirm("Are you sure you want to permanently delete this task?")) return;
            
            try {
                await taskOperations.deleteTask(currentSelectedTaskId);
                resetToEmptyState(); // Reset the UI after successful deletion.
            } catch (error) {
                // Error is handled inside taskOperations.
                return;
            }
        });
    }
    
    // --- Event Listeners for Live Updates from Detail Panel ---

    // Listen for text input changes (Name, Description) using the debounced handler.
    if (detailPanel) {
        detailPanel.addEventListener('input', (event) => {
            const target = event.target;
            if (target.matches('#detail-taskName') || target.matches('#detail-task-description')) {
                const key = target.id === 'detail-taskName' ? 'name' : 'description';
                handleDetailUpdate({ [key]: target.value });
            }
        });
    }

    // Lắng nghe thay đổi Priority, Status (select)
    const detailPrioritySelect = document.getElementById('detail-task-priority');
    if (detailPrioritySelect) {
        detailPrioritySelect.addEventListener('change', () => {
            handleDetailUpdate({ priority: detailPrioritySelect.value });
        });
    }

    const detailStatusSelect = document.getElementById('detail-task-status');
    if (detailStatusSelect) {
        detailStatusSelect.addEventListener('change', async () => {
            const newStatus = detailStatusSelect.value;
            // Đồng bộ checkbox khi đổi select
            const statusCheckbox = document.getElementById('detail-taskStatusCheckbox');
            if (statusCheckbox) statusCheckbox.checked = (newStatus === 'done');
            handleDetailUpdate({ status: newStatus });
        });
    }

    // Lắng nghe thay đổi ngày bắt đầu/kết thúc
    const startDateInput = document.getElementById('detail-task-start-date');
    const endDateInput = document.getElementById('detail-task-end-date');
    if (startDateInput) {
        startDateInput.addEventListener('change', () => {
            const val = startDateInput.value || null;
            const startDisplay = document.getElementById('detail-task-start-date-display');
            if (startDisplay) startDisplay.textContent = val ? new Date(val).toLocaleDateString() : 'No start date';
            handleDetailUpdate({ startDate: val });
        });
    }
    if (endDateInput) {
        endDateInput.addEventListener('change', () => {
            const val = endDateInput.value || null;
            const endDisplay = document.getElementById('detail-task-end-date-display');
            if (endDisplay) endDisplay.textContent = val ? new Date(val).toLocaleDateString() : 'No end date';
            handleDetailUpdate({ endDate: val });
        });
    }

    // Thêm: tìm kiếm theo thời gian thực (throttle)
    const searchInput = document.querySelector('.search-bar__input');
    if (searchInput) {
        searchInput.addEventListener('input', throttle(() => {
            loadTasks();
        }, 300));
    }

    // Sort popover actions
    const sortButtons = document.querySelectorAll('#sort-popover [data-sort]');
    if (sortButtons && sortButtons.length) {
        sortButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const sortBy = btn.getAttribute('data-sort');
                queryParams.sortBy = sortBy || 'creation-date';
                // Simple rule: alphabetical asc, others desc
                queryParams.sortDir = (sortBy === 'alphabetical') ? 'asc' : 'desc';
                loadTasks();
                const sortPopover = document.getElementById('sort-popover');
                if (sortPopover && sortPopover.hidePopover) sortPopover.hidePopover();
            });
        });
    }

    // Group by popover actions
    const groupbyButtons = document.querySelectorAll('#groupby-popover [data-groupby]');
    if (groupbyButtons && groupbyButtons.length) {
        groupbyButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const groupBy = btn.getAttribute('data-groupby') || 'none';
                queryParams.groupBy = groupBy;
                loadTasks();
                const groupbyPopover = document.getElementById('groupby-popover');
                if (groupbyPopover && groupbyPopover.hidePopover) groupbyPopover.hidePopover();
            });
        });
    }

    // Filter popover actions (apply only assignee simple filter)
    const filterPopover = document.getElementById('filter-popover');
    if (filterPopover) {
        const clearBtn = filterPopover.querySelector('.btn--tertiary');
        const applyBtn = filterPopover.querySelector('.btn--primary');
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                // Clear local filters
                filterPopover.querySelectorAll('input[type=checkbox]').forEach(cb => cb.checked = false);
                queryParams.assigneeId = null;
                queryParams.status = null;
                loadTasks();
                if (filterPopover.hidePopover) filterPopover.hidePopover();
            });
        }
        if (applyBtn) {
            applyBtn.addEventListener('click', () => {
                // Example: pick first checked assignee
                const assignees = Array.from(filterPopover.querySelectorAll('.filter-group:nth-of-type(2) input[type=checkbox]:checked'));
                queryParams.assigneeId = assignees.length ? assignees[0].value : null;
                // Could also add project/status if needed
                loadTasks();
                if (filterPopover.hidePopover) filterPopover.hidePopover();
            });
        }
    }

    // Add Task: deadline popover Save/Remove
    const deadlinePopover = document.getElementById('deadline-popover');
    if (deadlinePopover) {
        const startInput = document.getElementById('start-date-input');
        const endInput = document.getElementById('due-date-input');
        const footerBtns = deadlinePopover.querySelectorAll('.popover__footer .btn');
        const removeBtn = footerBtns && footerBtns[0];
        const saveBtn = footerBtns && footerBtns[1];

        if (removeBtn) {
            removeBtn.addEventListener('click', () => {
                if (startInput) startInput.value = '';
                if (endInput) endInput.value = '';
                newTaskData.startDate = null;
                newTaskData.endDate = null;
                showToast('Removed dates for new task', 'success');
                if (deadlinePopover.hidePopover) deadlinePopover.hidePopover();
            });
        }
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                newTaskData.startDate = startInput && startInput.value ? startInput.value : null;
                newTaskData.endDate = endInput && endInput.value ? endInput.value : null;
                showToast('Saved dates for new task', 'success');
                if (deadlinePopover.hidePopover) deadlinePopover.hidePopover();
            });
        }
    }

    // Add Task: optional priority popover
    const priorityPopover = document.getElementById('priority-popover');
    if (priorityPopover) {
        const removeBtn = priorityPopover.querySelector('.btn--tertiary');
        const saveBtn = priorityPopover.querySelector('.btn--primary');
        const priorityOptions = priorityPopover.querySelectorAll('.priority-option');
        let selectedPriority = null;
        
        // Add visual selection feedback
        priorityOptions.forEach(option => {
            option.addEventListener('click', () => {
                // Remove previous selection
                priorityOptions.forEach(opt => opt.style.backgroundColor = '');
                // Highlight selected option
                option.style.backgroundColor = 'var(--color-primary-50, #f0f9ff)';
                selectedPriority = option.getAttribute('data-priority');
            });
        });
        
        if (removeBtn) {
            removeBtn.addEventListener('click', () => {
                newTaskData.priority = 'normal'; // Reset to default
                selectedPriority = null;
                // Clear visual selection
                priorityOptions.forEach(opt => opt.style.backgroundColor = '');
                showToast('Removed priority for new task', 'success');
                if (priorityPopover.hidePopover) priorityPopover.hidePopover();
            });
        }
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                if (selectedPriority) {
                    newTaskData.priority = selectedPriority;
                    showToast(`Set priority: ${selectedPriority}`, 'success');
                } else {
                    showToast('Please select a priority first', 'warning');
                    return;
                }
                if (priorityPopover.hidePopover) priorityPopover.hidePopover();
            });
        }
    }

    // Add Task: optional assignee popover
    const assigneePopover = document.getElementById('assignee-popover');
    if (assigneePopover) {
        const clearBtn = assigneePopover.querySelector('.btn--tertiary');
        const saveBtn = assigneePopover.querySelector('.btn--primary');
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                assigneePopover.querySelectorAll('input[type=checkbox]').forEach(cb => cb.checked = false);
                newTaskData.assigneeIds = [];
                showToast('Cleared assignees for new task', 'success');
                if (assigneePopover.hidePopover) assigneePopover.hidePopover();
            });
        }
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                const ids = Array.from(assigneePopover.querySelectorAll('input[type=checkbox]:checked')).map(cb => cb.value);
                newTaskData.assigneeIds = ids;
                showToast(`Selected ${ids.length} assignee(s)`, 'success');
                if (assigneePopover.hidePopover) assigneePopover.hidePopover();
            });
        }
    }

    // Add Task: project & tasklist popover
    const projectPopover = document.getElementById('project-popover');
    async function populateProjectPopover() {
        if (!projectPopover) return;
        const listEl = projectPopover.querySelector('.tasklist-list');
        if (!listEl) return;
        listEl.innerHTML = '<div class="loading-spinner"></div>';
        const currentProject = store.getCurrentProject();
        if (!currentProject || !currentProject.id) {
            listEl.innerHTML = '<div class="empty-state">No project selected</div>';
            return;
        }
        try {
            const tasklists = await apiService.utils.get(`/projects/${currentProject.id}/tasklists`);
            if (!Array.isArray(tasklists) || tasklists.length === 0) {
                listEl.innerHTML = '<div class="empty-state">No task lists found</div>';
                return;
            }
            listEl.innerHTML = tasklists.map(tl => (
                `<li><label class="custom-radio"><input type="radio" name="tasklist-option" value="${tl.id}"> ${tl.name}</label></li>`
            )).join('');
        } catch (err) {
            console.error('Failed to load tasklists:', err);
            listEl.innerHTML = '<div class="error-state">Failed to load task lists</div>';
        }
    }
    const btnSetProject = document.getElementById('btn-set-project');
    btnSetProject && btnSetProject.addEventListener('click', populateProjectPopover);
    if (projectPopover) {
        const clearBtn = projectPopover.querySelector('.btn--tertiary');
        const saveBtn = projectPopover.querySelector('.btn--primary');
        clearBtn && clearBtn.addEventListener('click', () => {
            newTaskData.taskListId = null;
            projectPopover.querySelectorAll('input[name="tasklist-option"]').forEach(r => r.checked = false);
            showToast('Cleared project selection', 'success');
            projectPopover.hidePopover && projectPopover.hidePopover();
        });
        saveBtn && saveBtn.addEventListener('click', () => {
            const sel = projectPopover.querySelector('input[name="tasklist-option"]:checked');
            newTaskData.taskListId = sel ? sel.value : null;
            if (!newTaskData.taskListId) {
                showToast('Please select a task list', 'error');
                return;
            }
            showToast('Selected task list for new task', 'success');
            projectPopover.hidePopover && projectPopover.hidePopover();
        });
    }

    // Thêm: đồng bộ checkbox trạng thái trong panel chi tiết
    const statusCheckbox = document.getElementById('detail-taskStatusCheckbox');
    if (statusCheckbox) {
        statusCheckbox.addEventListener('change', () => {
            const newStatus = statusCheckbox.checked ? 'done' : 'not-started';
            // Sử dụng debounced handler để tránh gọi API liên tục
            handleDetailUpdate({ status: newStatus });
        });
    }


    // --- Initial Page Setup ---
    resetToEmptyState(); 
    const emptyStateEl = document.getElementById('empty-state-tasks');
    if (taskListContainer && taskListContainer.children.length <= 1) {
        if (emptyStateEl) emptyStateEl.style.display = 'flex';
    }
}


/**
 * Handles task operations with API calls and UI updates
 */
const taskOperations = {
    async createTask(taskListId, taskData) {
        try {
            // Gọi đến apiService đã được sửa lỗi ở bước trước
            const newTask = await apiService.tasks.create(taskListId, taskData);
            await loadTasks(); // Tải lại danh sách công việc
            showToast('Task created successfully', 'success');
            return newTask;
        } catch (error) {
            showToast(error.message || 'Failed to create task', 'error');
            console.error('Error creating task:', error);
            throw error;
        }
    },

    async updateTask(taskId, updateData) {
        try {
            await apiService.tasks.update(taskId, updateData);
            showToast('Task updated successfully', 'success');
        } catch (error) {
            showToast('Failed to update task', 'error');
            console.error('Error updating task:', error);
            throw error;
        }
    },

    async deleteTask(taskId) {
        try {
            await apiService.tasks.delete(taskId);
            await loadTasks();
            showToast('Task deleted successfully', 'success');
        } catch (error) {
            showToast('Failed to delete task', 'error');
            console.error('Error deleting task:', error);
            throw error;
        }
    }
};

async function selectProjectForWorkspace(workspaceId) {
    try {
        if (!workspaceId) return;
        const projects = await apiService.projects.getAll(workspaceId);
        if (projects && projects.length > 0) {
            // Chiến lược đơn giản: chọn project đầu tiên của workspace
            const firstProject = projects[0];
            store.setCurrentProject(firstProject);
        } else {
            // Không có project: clear project hiện tại
            store.setCurrentProject(null);
        }
    } catch (err) {
        console.error('Failed to select project for workspace', err);
        showToast('Failed to load projects for workspace', 'error');
    }
}

