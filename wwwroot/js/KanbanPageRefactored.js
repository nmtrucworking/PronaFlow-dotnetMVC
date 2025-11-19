document.addEventListener('DOMContentLoaded', function () {
    console.log("🚀 Kanban Page Loaded");
    if (window.lucide) { lucide.createIcons(); }


    initializeAddProjectButtons();
    initializeProjectCardClicks();
    initKanbanDragDrop();
});
function notify(type, message) {
    // Mapping type của bạn: 'success', 'error' (khớp với CSS class trong ui.js)
    if (window.showToast) {
        // ui.js signature: showToast(message, type, duration)
        window.showToast(message, type);
    } else {
        // Fallback nếu chưa load ui.js
        console.warn(`[${type.toUpperCase()}] ${message}`);
        alert(message);
    }
}


/**
 * Gán sự kiện click cho các project card để mở modal.
 * (Giữ nguyên logic Event Delegation)
 */
function initializeProjectCardClicks() {
    const kanbanView = document.getElementById('kanban-view');
    kanbanView.addEventListener('click', (e) => {
        const projectCard = e.target.closest('.project-card');
        if (projectCard) {
            const projectId = projectCard.dataset.projectId;
            // Giả định initializeProjectDetailModal, showProjectDetailModal, populateModalWithData 
            // được định nghĩa trong một tệp JS khác và đã được tải.
            if (typeof showProjectDetailModal === 'function') {
                showProjectDetailModal();
                populateModalWithData(projectId);
            } else {
                console.warn('Modal functions not found. Project ID:', projectId);
            }
        }
    });
}

/**
 * Khởi tạo sự kiện cho các nút "Add Project".
 * Gửi yêu cầu tạo Project mới về Controller.
 */
function initializeAddProjectButtons() {
    document.querySelectorAll('.add-project-btn, .add-card-in-popover').forEach(button => {
        button.addEventListener('click', async (e) => {
            const kanbanCol = e.target.closest('.kanban__col');
            const status = kanbanCol.dataset.status;
            const statusTitle = kanbanCol.querySelector('.kanban-column__header h3').textContent.trim();
            const projectName = prompt(`Enter a name for the new project in "${statusTitle}" status:`);

            if (projectName && projectName.trim() !== '') {
                const workspaceIdEl = document.getElementById('currentWorkspaceId');
                if (!workspaceIdEl || !workspaceIdEl.value) {
                    alert("Cannot determine current workspace ID. Please reload.");
                    return;
                }
                const workspaceId = workspaceIdEl.value;
                
                try {
                    const response = await fetch('/Kanbanboard/CreateProject', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            // Thêm Anti-forgery token nếu cần thiết
                        },
                        body: JSON.stringify({
                            workspaceId: workspaceId,
                            projectName: projectName.trim(),
                            initialStatus: status
                        })
                    });

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success && result.project) {
                            const projectCardHtml = createProjectCardHtml(result.project);
                            const column = kanbanCol.querySelector('.list-card');
                            if (column) {
                                column.insertAdjacentHTML('beforeend', projectCardHtml);
                                if (window.lucide) { lucide.createIcons(); }
                                initKanbanDragDrop(); // Khởi tạo lại drag drop cho thẻ mới
                            }
                        } else {
                             alert("Error creating project: " + (result.message || "Unknown error."));
                        }
                    } else {
                        throw new Error(`Server responded with status: ${response.status}`);
                    }
                } catch (error) {
                    console.error("Failed to create project:", error);
                    alert("Error: Could not create the project.");
                }
            }
        });
    });
}


/**
 * Khởi tạo chức năng kéo và thả cho các thẻ project.
 * Gửi yêu cầu cập nhật Status về Controller khi DragEnd.
 */
function initKanbanDragDrop() {
    const draggables = document.querySelectorAll('.project-card');
    const columns = document.querySelectorAll('.kanban__col .list-card');

    draggables.forEach(draggable => {
        draggable.addEventListener('dragstart', () => {
            draggable.classList.add('dragging');
        });

        draggable.addEventListener('dragend', async () => {
            draggable.classList.remove('dragging');

            const projectId = draggable.dataset.projectId;
            const newColumn = draggable.closest('.kanban__col');
            const newStatus = newColumn.dataset.status; // Lấy data-status mới

            try {
                const response = await fetch('/Kanbanboard/UpdateProjectStatus', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ projectId: projectId, newStatus: newStatus })
                });

                const result = await response.json();

                if (!result.success) {
                    alert(result.message || 'Failed to update project status. Please reload the page.');
                    // Nếu lỗi, nên tải lại trang để đảm bảo tính nhất quán của trạng thái
                    window.location.reload(); 
                }
                console.log(`Project ${projectId} moved to ${newStatus} successfully.`);
            } catch (error) {
                console.error('Failed to update project status:', error);
                alert('Connection error. Could not update project status. Please reload the page.');
                window.location.reload(); 
            }
        });
    });

    // Logic dragover để chèn thẻ vào vị trí chính xác
    columns.forEach(column => {
        column.addEventListener('dragover', e => {
            e.preventDefault();
            const afterElement = getDragAfterElement(column, e.clientY);
            const dragging = document.querySelector('.dragging');
            if (dragging) {
                if (afterElement == null) {
                    column.appendChild(dragging);
                } else {
                    column.insertBefore(dragging, afterElement);
                }
            }
        });
    });
}

/**
 * Helper function to determine where to place the dragged element within a column.
 */
function getDragAfterElement(column, y) {
    const draggableElements = [...column.querySelectorAll('.project-card:not(.dragging)')];

    return draggableElements.reduce((closest, child) => {
        const box = child.getBoundingClientRect();
        const offset = y - box.top - box.height / 2;
        if (offset < 0 && offset > closest.offset) {
            return { offset: offset, element: child };
        } else {
            return closest;
        }
    }, { offset: Number.NEGATIVE_INFINITY }).element;
}

/**
 * Tạo chuỗi HTML cho một project card từ JSON data trả về từ Controller (sau khi tạo mới).
 * (Cần đảm bảo logic render khớp với RenderProjectCard Helper trong Razor View)
 */
function createProjectCardHtml(project) {
    const defaultAvatarUrl = '/wwwroot/images/avt-notion_1.png'; 
    
    // 1. Render Tags
    const tagsHtml = project.Tags && project.Tags.length > 0
        ? `<div class="prj-card-tags-group">
               ${project.Tags.map(tag => `<div class="prj-card-tag" style="background-color: ${tag.ColorHex};"></div>`).join('')}
           </div>`
        : '';
    
    // 2. Render TaskStatics
    const taskProgressHtml = (project.TotalTasks > 0)
        ? `<div class="prj-card-total-task prj-attribute">
               <i data-lucide="circle-check-big" class="prj-card-icon"></i>
               <span>${project.CompletedTasks || 0}</span>
               <span>/</span>
               <span>${project.TotalTasks}</span>
           </div>`
        : '';
    
    // Logic tính toán countdown
    let countdownHtml = '';
    if (project.EndDate && project.RemainingDays >= 0) {
        countdownHtml = `
            <div class="prj-card-coutdown prj-attribute">
                <i data-lucide="hourglass" class="prj-card-icon"></i>
                <span>${project.RemainingDays}d</span>
            </div>`;
    }

    const membersHtml = project.Members && project.Members.length > 0
        ? `<div class="prj-card-members">
               ${project.Members.map(member => `<img src="${member.AvatarUrl || defaultAvatarUrl}" alt="Member Avatar" class="prj-member">`).join('')}
           </div>`
        : '';
    
    // Format ngày tháng (YYYY-MM-DD to MMM DD)
    const formatDate = (dateString) => {
        if (!dateString) return '...';
        try {
            const date = new Date(dateString);
            // Sử dụng toLocaleDateString với options để định dạng giống Razor
            return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
        } catch {
            return '...';
        }
    };
    
    const startDate = formatDate(project.StartDate);
    const endDate = formatDate(project.EndDate);


    return `
    <div class="project-card" draggable="true" data-project-id="${project.Id}">
        <div class="prj-card-attributes">
            ${tagsHtml}
            <div class="prj-card-project-name prj-attribute">
                <label class="custom-checkbox">
                    <input type="checkbox" name="project-status" ${project.IsCompleted ? 'checked' : ''}>
                    <span class="custom-checkbox__checkmark round"></span>
                </label>
                <span>${project.Name}</span>
            </div>
            <div class="prj-card-deadline prj-attribute">
                 <i data-lucide="clock" class="prj-card-icon"></i>
                 <span>${startDate}</span>
                 <span>-</span>
                 <span>${endDate}</span>
            </div>
            ${countdownHtml}
            ${taskProgressHtml}    
        </div>
        ${membersHtml}
    </div>`;
}

// Giữ các hàm này ở phạm vi toàn cục để có thể gọi trong DOMContentLoaded event của View
// window.initializeAddProjectButtons = initializeAddProjectButtons;
// window.initializeProjectCardClicks = initializeProjectCardClicks;
// window.initKanbanDragDrop = initKanbanDragDrop;
// window.createProjectCardHtml = createProjectCardHtml;