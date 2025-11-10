--`sp_DuplicateProject`, `sp_SoftDeleteUser`: Đóng-gói các nghiệp-vụ phức-tạp, đảm-bảo chúng được thực-thi một cách nhất-quán và an-toàn thông-qua TRANSACTION.
CREATE PROCEDURE [dbo].[sp_DuplicateProject]
    @SourceProjectID BIGINT,
    @UserID BIGINT -- ID của người thực hiện nhân bản
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @NewProjectID BIGINT;
        DECLARE @NewProjectName NVARCHAR(255);
        -- Bảng tạm để lưu ánh xạ giữa ID cũ và ID mới của task_lists
        DECLARE @TaskListMap TABLE (OldTaskListID BIGINT, NewTaskListID BIGINT);

        -- Bước 1: Tạo dự án mới (Không thay đổi)
        SELECT @NewProjectName = [name] + N' (Copy)' FROM [dbo].[projects] WHERE [id] = @SourceProjectID;

        INSERT INTO [dbo].[projects] ([workspace_id], [name], [description], [cover_image_url], [status], [project_type])
        SELECT 
            [workspace_id], 
            @NewProjectName, 
            [description], 
            [cover_image_url], 
            'not-started', -- Reset status
            'personal'     -- Reset type
        FROM [dbo].[projects]
        WHERE [id] = @SourceProjectID;

        SET @NewProjectID = SCOPE_IDENTITY();

        -- Thêm người nhân bản làm admin cho dự án mới
        INSERT INTO [dbo].[project_members] ([project_id], [user_id], [role]) VALUES (@NewProjectID, @UserID, 'admin');

        -- ==============================================================================
        -- BƯỚC 2: SAO CHÉP GIAI ĐOẠN (TASK_LISTS) - PHẦN ĐÃ SỬA LỖI
        -- Sử dụng MERGE để chèn và lấy ra cả ID cũ và ID mới một cách chính xác
        -- ==============================================================================
        MERGE INTO [dbo].[task_lists] AS Target
        USING (
            SELECT [id], [name], [position] 
            FROM [dbo].[task_lists] 
            WHERE project_id = @SourceProjectID
        ) AS Source
        ON (1 = 0) -- Luôn luôn không khớp để kích hoạt mệnh đề INSERT
        WHEN NOT MATCHED THEN
            INSERT ([project_id], [name], [position])
            VALUES (@NewProjectID, Source.[name], Source.[position])
        OUTPUT Source.[id], inserted.[id] -- Lấy ID cũ từ Source và ID mới từ inserted
        INTO @TaskListMap (OldTaskListID, NewTaskListID);


        -- Bước 3: Sao chép Công việc (tasks) (Không thay đổi, logic này đã đúng)
        INSERT INTO [dbo].[tasks] ([project_id], [task_list_id], [creator_id], [name], [description], [priority], [status])
        SELECT 
            @NewProjectID,
            map.NewTaskListID, -- Sử dụng ID mới từ bảng ánh xạ
            @UserID, -- Người tạo là người nhân bản
            t.[name],
            t.[description],
            t.[priority],
            'not-started' -- Reset status
        FROM [dbo].[tasks] t
        JOIN @TaskListMap map ON t.task_list_id = map.OldTaskListID
        WHERE t.project_id = @SourceProjectID;

        -- Ghi lại hoạt động
        INSERT INTO [dbo].[activities] ([user_id], [action_type], [target_id], [target_type], [content])
        VALUES (@UserID, 'project_duplicate', @NewProjectID, 'project', CONCAT(N'{"source_project_id":', @SourceProjectID, '}'));
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW; -- Ném lại lỗi để ứng dụng có thể bắt được
    END CATCH;
END;
GO

CREATE PROCEDURE [dbo].[sp_SoftDeleteUser]
    @UserID BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @Now DATETIME2 = GETDATE();

        -- Bước 1: Cập nhật bảng users
        UPDATE [dbo].[users]
        SET [is_deleted] = 1, [deleted_at] = @Now
        WHERE [id] = @UserID AND [is_deleted] = 0;

        -- Nếu không có dòng nào được cập nhật, có thể người dùng không tồn tại hoặc đã bị xóa
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Bước 2: Xóa mềm các project thuộc workspace mà người này sở hữu
        UPDATE p
        SET p.[is_deleted] = 1, p.[deleted_at] = @Now
        FROM [dbo].[projects] p
        JOIN [dbo].[workspaces] w ON p.workspace_id = w.id
        WHERE w.owner_id = @UserID;

        -- Bước 3: Xóa mềm các tasks do người này tạo
        UPDATE [dbo].[tasks]
        SET [is_deleted] = 1, [deleted_at] = @Now
        WHERE [creator_id] = @UserID;
        
        -- Bước 4: Hủy các lời mời mà người này đã gửi
        UPDATE [dbo].[invitations]
        SET [status] = 'expired'
        WHERE [inviter_id] = @UserID AND [status] = 'pending';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

--`sp_GetUserDashboardMetrics`, `sp_GetProjectsForKanban`, `sp_GetUserTasks`: Các SP chuyên-dụng để lấy dữ-liệu cho các thành-phần UI cụ-thể như Dashboard, Kanban Board. Cách-tiếp-cận này rất hiệu-quả, giúp giảm-thiểu số-lần gọi từ client lên server.
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserDashboardMetrics]
    @UserID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Đếm tổng số dự án mà user là thành viên
    DECLARE @TotalProjects INT;
    SELECT @TotalProjects = COUNT(DISTINCT project_id) 
    FROM [dbo].[project_members] 
    WHERE user_id = @UserID;

    -- Đếm tổng số task đang làm (in-progress, in-review) được giao cho user
    DECLARE @InProgressTasks INT;
    SELECT @InProgressTasks = COUNT(t.id)
    FROM [dbo].[tasks] t
    JOIN [dbo].[task_assignees] ta ON t.id = ta.task_id
    WHERE ta.user_id = @UserID AND t.status IN ('in-progress', 'in-review') AND t.is_deleted = 0;

    -- Đếm tổng số task đã quá hạn được giao cho user
    DECLARE @OverdueTasks INT;
    SELECT @OverdueTasks = COUNT(t.id)
    FROM [dbo].[tasks] t
    JOIN [dbo].[task_assignees] ta ON t.id = ta.task_id
    WHERE ta.user_id = @UserID AND t.status != 'done' AND t.end_date < GETDATE() AND t.is_deleted = 0;

    -- Trả về kết quả
    SELECT @TotalProjects AS TotalProjects, @InProgressTasks AS InProgressTasks, @OverdueTasks AS OverdueTasks;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetProjectsForKanban]
    @WorkspaceID BIGINT,
    @UserID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.id,
        p.name,
        p.status,
        p.start_date,
        p.end_date,
        ISNULL(pd.total_tasks, 0) AS total_tasks,
        ISNULL(pd.completed_tasks, 0) AS completed_tasks,
        -- Lấy danh sách avatar của 3 thành viên đầu tiên
        (
            SELECT STRING_AGG(u.avatar_url, ',') 
            FROM (
                SELECT TOP 3 user_id FROM [dbo].[project_members] pm_inner WHERE pm_inner.project_id = p.id
            ) pm
            JOIN [dbo].[users] u ON pm.user_id = u.id
        ) AS member_avatars,
        ISNULL(pd.member_count, 0) AS member_count,
        -- Lấy danh sách màu của các tag
        (
            SELECT STRING_AGG(t.color_hex, ',')
            FROM [dbo].[project_tags] pt
            JOIN [dbo].[tags] t ON pt.tag_id = t.id
            WHERE pt.project_id = p.id
        ) AS tag_colors
    FROM 
        [dbo].[projects] p
    JOIN 
        [dbo].[project_members] pm ON p.id = pm.project_id
    LEFT JOIN
        [dbo].[vw_ProjectDetails] pd ON p.id = pd.project_id
    WHERE 
        p.workspace_id = @WorkspaceID
        AND pm.user_id = @UserID
        AND p.is_deleted = 0
        AND p.is_archived = 0;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserTasks]
    @UserID BIGINT,
    @WorkspaceID BIGINT,
    @SearchTerm NVARCHAR(255) = NULL,
    @FilterByProjectIDs NVARCHAR(MAX) = NULL, -- vd: '1,5,10'
    @FilterByStatus NVARCHAR(100) = NULL,     -- vd: 'in-progress,in-review'
    @SortBy NVARCHAR(50) = 'due_date_asc'      -- vd: 'due_date_asc', 'priority_desc', 'creation_date_desc'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        vwt.task_id,
        vwt.task_name,
        vwt.task_status,
        vwt.task_priority,
        vwt.task_end_date AS due_date,
        vwt.project_id,
        vwt.project_name,
        vwt.task_list_id,
        vwt.task_list_name
    FROM
        [dbo].[vw_TaskDetails] vwt
    JOIN
        [dbo].[task_assignees] ta ON vwt.task_id = ta.task_id
    WHERE
        ta.user_id = @UserID
        AND vwt.workspace_id = @WorkspaceID
        -- Lọc theo từ khóa tìm kiếm
        AND (@SearchTerm IS NULL OR vwt.task_name LIKE '%' + @SearchTerm + '%')
        -- Lọc theo danh sách project ID
        AND (@FilterByProjectIDs IS NULL OR vwt.project_id IN (SELECT value FROM STRING_SPLIT(@FilterByProjectIDs, ',')))
        -- Lọc theo danh sách status
        AND (@FilterByStatus IS NULL OR vwt.task_status IN (SELECT value FROM STRING_SPLIT(@FilterByStatus, ',')))
    ORDER BY
        -- Sắp xếp động
        CASE WHEN @SortBy = 'due_date_asc' THEN vwt.task_end_date END ASC,
        CASE WHEN @SortBy = 'due_date_desc' THEN vwt.task_end_date END DESC,
        CASE WHEN @SortBy = 'priority_desc' THEN 
            CASE vwt.task_priority
                WHEN 'high' THEN 1
                WHEN 'normal' THEN 2
                WHEN 'low' THEN 3
            END
        END ASC,
        CASE WHEN @SortBy = 'creation_date_desc' THEN vwt.task_created_at END DESC,
        CASE WHEN @SortBy = 'alphabetical_asc' THEN vwt.task_name END ASC
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_LogActivity]
    @UserID BIGINT,
    @ActionType NVARCHAR(50),
    @TargetID BIGINT,
    @TargetType NVARCHAR(10),
    @Content NVARCHAR(MAX) -- Dữ liệu dạng JSON
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra xem content có phải là JSON hợp lệ không
    IF (ISJSON(@Content) > 0)
    BEGIN
        INSERT INTO [dbo].[activities] ([user_id], [action_type], [target_id], [target_type], [content])
        VALUES (@UserID, @ActionType, @TargetID, @TargetType, @Content);
    END
    ELSE
    BEGIN
        -- Ghi log lỗi hoặc bỏ qua tùy theo yêu cầu nghiệp vụ
        -- Ở đây, chúng ta sẽ bỏ qua để không làm dừng các tiến trình khác
        RETURN;
    END
END;
GO