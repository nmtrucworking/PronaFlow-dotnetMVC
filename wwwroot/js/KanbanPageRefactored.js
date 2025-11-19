document.addEventListener('DOMContentLoaded', function () {
    console.log("🚀 Kanban Page Loaded");
    if (window.lucide) { lucide.createIcons(); }


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

