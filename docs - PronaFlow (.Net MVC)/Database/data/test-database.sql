-- =================================================================================
--  PronaFlow - Execution Script for Stored Procedures
--  PURPOSE: Demonstrates how to run the auxiliary Stored Procedures.
--  LƯU Ý: Đây là file ví dụ. Trong ứng dụng thực tế, các tham số
--         sẽ được truyền vào từ tầng backend (API) dựa trên hành động của người dùng.
-- =================================================================================

USE db_PronaFlow;
GO

SELECT * FROM [attachments];    --
SELECT * FROM [comments];       --
SELECT * FROM [project_members];--
SELECT * FROM [project_tags];   --
SELECT * FROM [projects];       --
SELECT * FROM [subtasks];       --
SELECT * FROM [tags];           --
SELECT * FROM [task_assignees]; --
SELECT * FROM [tasks];          --
SELECT * FROM [users];          --
SELECT * FROM [workspaces];     --
SELECT * FROM [task_lists];     --error
SELECT * FROM [invitations];
SELECT * FROM [activities];
SELECT * FROM [notification_recipients];
SELECT * FROM [password_resets];
SELECT * FROM [task_dependencies];
SELECT * FROM [user_preferences];

SELECT * FROM [project_tags];
GO
-- =================================================================================
-- VÍ DỤ 1: Lấy số liệu cho Dashboard của người dùng 'minhtruc' (ID = 2)
-- Tương ứng với hàm JS: loadDashboardMetrics()
-- =================================================================================
PRINT '--- Ví dụ 1: Chạy sp_GetUserDashboardMetrics ---';

DECLARE @DashboardUserID BIGINT = 2; -- ID của Nguyễn Minh Trúc
EXEC [dbo].[sp_GetUserDashboardMetrics] @UserID = @DashboardUserID;
GO


-- =================================================================================
-- VÍ DỤ 2: Lấy danh sách dự án cho Kanban Board
-- Tương ứng với hàm JS: loadKanbanBoard()
-- =================================================================================
PRINT '--- Ví dụ 2: Chạy sp_GetProjectsForKanban ---';

DECLARE @KanbanUserID BIGINT = 2;
DECLARE @KanbanWorkspaceID BIGINT = 2; -- Workspace 'Marketing Q4 2025' của Minh Trúc
EXEC [dbo].[sp_GetProjectsForKanban] @WorkspaceID = @KanbanWorkspaceID, @UserID = @KanbanUserID;
GO


-- =================================================================================
-- VÍ DỤ 3: Lấy danh sách công việc của người dùng (Trang My Tasks)
-- Tương ứng với hàm JS: loadUserTasks()
-- =================================================================================
PRINT '--- Ví dụ 3: Chạy sp_GetUserTasks (với nhiều kịch bản) ---';

DECLARE @MyTasksUserID BIGINT = 2; -- Nguyễn Minh Trúc
DECLARE @MyTasksWorkspaceID BIGINT = 2; -- Workspace 'Marketing Q4 2025'

-- Kịch bản 3A: Lấy tất cả task được giao, sắp xếp theo ngày hết hạn tăng dần
PRINT 'Kịch bản 3A: Lấy tất cả task, sắp xếp theo ngày hết hạn';
EXEC [dbo].[sp_GetUserTasks] 
    @UserID = @MyTasksUserID, 
    @WorkspaceID = @MyTasksWorkspaceID, 
    @SortBy = 'due_date_asc';

-- Kịch bản 3B: Tìm kiếm task có chứa từ "Thiết kế"
PRINT 'Kịch bản 3B: Tìm kiếm task chứa từ "Thiết kế"';
EXEC [dbo].[sp_GetUserTasks] 
    @UserID = @MyTasksUserID, 
    @WorkspaceID = @MyTasksWorkspaceID, 
    @SearchTerm = N'Thiết kế';

-- Kịch bản 3C: Lọc theo Project ID = 1 và trạng thái 'in-progress', sắp xếp theo độ ưu tiên giảm dần
PRINT 'Kịch bản 3C: Lọc và sắp xếp phức tạp';
EXEC [dbo].[sp_GetUserTasks]
    @UserID = @MyTasksUserID,
    @WorkspaceID = @MyTasksWorkspaceID,
    @FilterByProjectIDs = '1',
    @FilterByStatus = 'in-progress',
    @SortBy = 'priority_desc';
GO


-- =================================================================================
-- VÍ DỤ 4: Lấy chi tiết một dự án (cho Modal)
-- Tương ứng với hàm JS: loadProjectDetails()
-- Lưu ý: SP này trả về 5 bảng dữ liệu khác nhau trong một lần chạy.
-- =================================================================================
PRINT '--- Ví dụ 4: Chạy sp_GetProjectDetails ---';

DECLARE @DetailProjectID BIGINT = 1; -- Project 'Chiến dịch quảng cáo Tết 2026'
DECLARE @DetailUserID BIGINT = 2;    -- User phải là member của project để có quyền xem
EXEC [dbo].[sp_GetProjectDetails] @ProjectID = @DetailProjectID, @UserID = @DetailUserID;
GO


-- =================================================================================
-- VÍ DỤ 5: Tìm kiếm người dùng để mời vào dự án
-- Tương ứng với hàm JS: manageProjectMembers()
-- =================================================================================
PRINT '--- Ví dụ 5: Chạy sp_SearchUsersForProject ---';

-- Tìm người dùng có tên/email chứa chữ 'Vinh' để mời vào Project ID 1
EXEC [dbo].[sp_SearchUsersForProject]
    @WorkspaceID = 2,
    @ProjectID = 1,
    @SearchTerm = N'Vinh'; 
GO


-- =================================================================================
-- VÍ DỤ 6: Ghi log một hoạt động (ví dụ: người dùng thay đổi trạng thái dự án)
-- Tương ứng với nhiều hàm JS khác nhau
-- =================================================================================
PRINT '--- Ví dụ 6: Chạy sp_LogActivity ---';

DECLARE @LogUserID BIGINT = 4; -- User 'Phạm Quang Vinh' thực hiện hành động
DECLARE @LogProjectID BIGINT = 1;

EXEC [dbo].[sp_LogActivity]
    @UserID = @LogUserID,
    @ActionType = 'project_update_status',
    @TargetID = @LogProjectID,
    @TargetType = 'project',
    @Content = N'{"old_status": "not-started", "new_status": "in-progress"}';

-- Kiểm tra lại log vừa được thêm vào
PRINT 'Kiểm tra bảng activities để xem log mới nhất:';
SELECT TOP 1 * FROM [dbo].[activities] ORDER BY id DESC;
GO


-- =================================================================================
-- VÍ DỤ 7: Di chuyển một task sang một task list khác
-- Tương ứng với logic kéo-thả task trong modal
-- =================================================================================
PRINT '--- Ví dụ 7: Chạy sp_MoveTask ---';

DECLARE @TaskToMoveID BIGINT = 8;     -- Task 'Vẽ wireframe cho trang chủ'
DECLARE @NewTaskListID BIGINT = 6; -- Chuyển từ 'To Do' (ID=5) sang 'In Progress' (ID=6)

PRINT N'Trạng thái của Task 8 trước khi chuyển (task_list_id):';
SELECT task_list_id FROM [dbo].[tasks] WHERE id = @TaskToMoveID;

-- Thực thi di chuyển
EXEC [dbo].[sp_MoveTask] @TaskID = @TaskToMoveID, @NewTaskListID = @NewTaskListID;

PRINT N'Trạng thái của Task 8 sau khi chuyển (task_list_id):';
SELECT task_list_id FROM [dbo].[tasks] WHERE id = @TaskToMoveID;

-- Hoàn tác lại để giữ nguyên dữ liệu mẫu
UPDATE [dbo].[tasks] SET task_list_id = 5 WHERE id = @TaskToMoveID;
PRINT N'Đã hoàn tác lại trạng thái ban đầu cho Task 8.';
GO

PRINT '--- Hoàn tất thực thi các ví dụ ---';
GO