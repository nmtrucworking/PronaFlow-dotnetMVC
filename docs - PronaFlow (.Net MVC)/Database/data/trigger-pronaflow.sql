-- PronaFlow Trigger
--`trg_..._UpdateTimestamp`: Tự-động cập-nhật cột updated_at khi có thay-đổi. Rất phổ-biến và hữu-ích.

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

--`trg_after_user_insert`: Tự-động tạo một workspace mặc-định cho người-dùng mới. Giúp cải-thiện trải-nghiệm người-dùng.
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


--`trg_insteadof_tasklist_insert`: Một trigger phức-tạp và thông-minh, dùng INSTEAD OF để can-thiệp trước khi chèn dữ-liệu, tự-động tính-toán và gán giá-trị position cho task_list mới. Việc sử-dụng cursor đảm-bảo nó hoạt-động đúng ngay-cả khi chèn nhiều dòng cùng-lúc.
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

--`trg_before_task_update_check_deps`: Thi-hành một quy-tắc nghiệp-vụ quan-trọng: không cho phép bắt-đầu một công-việc nếu công-việc nó phụ-thuộc chưa hoàn-thành.
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