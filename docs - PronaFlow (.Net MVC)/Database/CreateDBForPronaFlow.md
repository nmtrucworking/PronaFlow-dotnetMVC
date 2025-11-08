## **1. *Tổng quan* và *Mục đích***
- Tạo cấu trúc (schema) của cơ-sở-dữ-liệu `db_PronaFlow`
- Thiết kế các cấu trúc hình quan trọng.
- Định nghĩa logic nghiệp vụ.
- Tối ưu hóa cho việc truy vấn.
***Nền tảng***: Microsoft SQL Server 2022.
## **2. *Phân tích Chi tiết theo Từng phần***
### a. Khởi tạo và Cấu hình Database.
- **Tệp vật lý**: Chỉ định đường dẫn, kích thước ban đầu, kích thước tối đa và mức ododj tăng trưởng cho cả tệp `dữ liệu` (`.mdf`) và `tệp log` (`.ldf`).
- **Collation**: `Latin1_General_100_CI_AS_SC_UTF8`: hỗ trợ đầy đủ Unicode (bao gồm tiếng Việt có dấu) và tối ưu hóa cho các phiên bản SQL Server mới.
- **Tùy chọn Database**:`RECOVERY SIMPLE`, `AUTO_SHRINK OFF`, `AUTO_CLOSE OFF` là những cấu-hình tiêu-chuẩn cho môi-trường phát-triển và nhiều môi-trường production, giúp cải-thiện hiệu-năng và giảm-thiểu các hoạt-động không cần-thiết.
### **b. Thiết kế *Lược đồ quan hệ* (*Schema Design*)**
#### **b.1. Các thực thể chính**
- `users`, `workspaces`, `projects`, `tasks`: Cấu-trúc phân-cấp cốt-lõi của ứng-dụng. Một người-dùng (`user`) sở-hữu không-gian làm-việc (`workspace`), trong đó chứa các dự-án (`project`), và mỗi dự-án lại có các công-việc (`task`).
- `task_lists`: Tổ-chức các `tasks` thành các cột hoặc giai-đoạn trong một dự-án (ví dụ: To Do, In Progress, Done).
- `tags`, `comments`, `attachments`: Các thực-thể phụ trợ để làm giàu thông-tin cho dự-án và công-việc.
#### **b.2. Bảng nối (Junction Tables)**
- `project_members`: Ai thuộc dự-án nào.
- `task_assignees`: Ai được giao công-việc nào.
- `project_tags`: Gắn thẻ cho dự-án.
- `task_dependencies`: Thiết-lập sự phụ-thuộc giữa các công-việc.
#### **b.3. Soft Deletes**
Việc sử-dụng cặp cột `is_deleted` và `deleted_at` trong các bảng quan-trọng như `users`, `projects`, `tasks` là một best practice. Nó cho phép khôi-phục dữ-liệu và duy-trì lịch-sử thay-vì xóa vĩnh-viễn.
#### **b.4. Constraints**
- `PRIMARY KEY`, `FOREIGN KEY`: Định-nghĩa quan-hệ và đảm-bảo tham-chiếu.
- `UNIQUE`: Đảm-bảo email, username không trùng-lặp.
- `CHECK`: Giới-hạn các giá-trị hợp-lệ cho các cột trạng-thái, vai-trò (ví dụ: `status`, `role`, `priority`).
- `DEFAULT`: Cung-cấp giá-trị mặc-định, giúp đơn-giản-hóa logic chèn dữ-liệu.
### **c. Logic Nghiệp vụ trong Database** ( #Triggers, #Views, #Stored-Procedures)

#### **c.1. Triggers:**

- `trg_..._UpdateTimestamp`: Tự-động cập-nhật cột updated_at khi có thay-đổi. Rất phổ-biến và hữu-ích.
```sql
 CREATE TRIGGER [trg_users_UpdateTimestamp] ON [dbo].[users]
AFTER UPDATE AS
BEGIN
    UPDATE [dbo].[users]
    SET [updated_at] = GETDATE()
    FROM [inserted]
    WHERE [dbo].[users].[id] = [inserted].[id];
END;
GO

CREATE TRIGGER [trg_workspaces_UpdateTimestamp] ON [dbo].[workspaces]
AFTER UPDATE AS
BEGIN
    UPDATE [dbo].[workspaces]
    SET [updated_at] = GETDATE()
    FROM [inserted]
    WHERE [dbo].[workspaces].[id] = [inserted].[id];
END;
GO

CREATE TRIGGER [trg_projects_UpdateTimestamp] ON [dbo].[projects]
AFTER UPDATE AS
BEGIN
    UPDATE [dbo].[projects]
    SET [updated_at] = GETDATE()
    FROM [inserted]
    WHERE [dbo].[projects].[id] = [inserted].[id];
END;
GO

CREATE TRIGGER [trg_tasks_UpdateTimestamp] ON [dbo].[tasks]
AFTER UPDATE AS
BEGIN
    UPDATE [dbo].[tasks]
    SET [updated_at] = GETDATE()
    FROM [inserted]
    WHERE [dbo].[tasks].[id] = [inserted].[id];
END;
GO
```

- `trg_after_user_insert`: Tự-động tạo một workspace mặc-định cho người-dùng mới. Giúp cải-thiện trải-nghiệm người-dùng.
```sql
CREATE TRIGGER [trg_after_user_insert]
ON [dbo].[users]
AFTER INSERT
AS
BEGIN
    INSERT INTO [dbo].[workspaces] ([owner_id], [name], [description])
    SELECT 
        i.id, 
        i.full_name + N'''s Workspace', 
        N'Your first workspace'
    FROM [inserted] i;
END;
GO
```

- `trg_insteadof_tasklist_insert`: Một trigger phức-tạp và thông-minh, dùng INSTEAD OF để can-thiệp trước khi chèn dữ-liệu, tự-động tính-toán và gán giá-trị position cho task_list mới. Việc sử-dụng cursor đảm-bảo nó hoạt-động đúng ngay-cả khi chèn nhiều dòng cùng-lúc.
```sql
CREATE TRIGGER [trg_insteadof_tasklist_insert]
ON [dbo].[task_lists]
INSTEAD OF INSERT
AS
BEGIN
    -- Sử dụng cursor để xử lý trường hợp chèn nhiều dòng cùng lúc
    DECLARE @project_id BIGINT, @name NVARCHAR(100);
    DECLARE insert_cursor CURSOR FOR
    SELECT [project_id], [name] FROM [inserted];

    OPEN insert_cursor;
    FETCH NEXT FROM insert_cursor INTO @project_id, @name;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @max_pos INT;
        SELECT @max_pos = ISNULL(MAX([position]), 0) FROM [dbo].[task_lists] WHERE [project_id] = @project_id;

        INSERT INTO [dbo].[task_lists] ([project_id], [name], [position])
        VALUES (@project_id, @name, @max_pos + 1);

        FETCH NEXT FROM insert_cursor INTO @project_id, @name;
    END;

    CLOSE insert_cursor;
    DEALLOCATE insert_cursor;
END;
GO
```

- `trg_before_task_update_check_deps`: Thi-hành một quy-tắc nghiệp-vụ quan-trọng: không cho phép bắt-đầu một công-việc nếu công-việc nó phụ-thuộc chưa hoàn-thành.
```sql
CREATE TRIGGER [trg_before_task_update_check_deps]
ON [dbo].[tasks]
AFTER UPDATE
AS
BEGIN
    IF UPDATE([status])
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM [inserted] i
            JOIN [dbo].[task_dependencies] td ON i.id = td.task_id
            JOIN [dbo].[tasks] blocking_task ON td.blocking_task_id = blocking_task.id
            WHERE i.[status] IN ('in-progress', 'done') AND blocking_task.[status] != 'done'
        )
        BEGIN
			ROLLBACK TRANSACTION;
			THROW 51000, 'Cannot start or complete this task. It depends on other tasks that are not yet done.', 1;
		END
    END
END;
GO
```

#### **c.2. Views:**
`vw_TaskDetails` và `vw_ProjectDetails`: Các view này "trải phẳng" (flatten) các cấu-trúc dữ-liệu phức-tạp bằng cách join sẵn các bảng liên-quan. Chúng giúp đơn-giản-hóa các câu lệnh SELECT từ phía ứng-dụng, giảm-thiểu sự lặp-lại code và cải-thiện hiệu-năng đọc.
```sql
 
```

#### **c.3. Stored Procedures (SPs):**
- `sp_DuplicateProject`, `sp_SoftDeleteUser`: Đóng-gói các nghiệp-vụ phức-tạp, đảm-bảo chúng được thực-thi một cách nhất-quán và an-toàn thông-qua TRANSACTION.
```sql
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
```

```sql
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
```

- `sp_GetUserDashboardMetrics`, `sp_GetProjectsForKanban`, `sp_GetUserTasks`: Các SP chuyên-dụng để lấy dữ-liệu cho các thành-phần UI cụ-thể như Dashboard, Kanban Board. Cách-tiếp-cận này rất hiệu-quả, giúp giảm-thiểu số-lần gọi từ client lên server.
```sql
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
```

```sql
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
```

```sql
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
```

- `sp_LogActivity`: Một procedure trung-tâm để ghi-lại hoạt-động của người-dùng, một best practice cho việc kiểm-toán (auditing) và hiển-thị lịch-sử hoạt-động.
```sql
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
```

