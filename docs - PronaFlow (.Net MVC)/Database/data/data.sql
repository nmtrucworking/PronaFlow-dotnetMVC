USE db_PronaFlow;
GO
ALTER TABLE [dbo].[projects]
ALTER COLUMN [cover_image_url] NVARCHAR(MAX) NULL;
GO

SELECT * FROM [users];          --
SELECT * FROM [workspaces];     --
SELECT * FROM [projects];       --
SELECT * FROM [task_lists];     --error
SELECT * FROM [tasks];          --
SELECT * FROM [task_assignees]; --
SELECT * FROM [task_dependencies];
SELECT * FROM [subtasks];       --
SELECT * FROM [attachments];    --
SELECT * FROM [project_members];--
SELECT * FROM [tags];           --
SELECT * FROM [project_tags];   --
SELECT * FROM [invitations];
SELECT * FROM [activities];
SELECT * FROM [comments];       --
SELECT * FROM [notification_recipients];
SELECT * FROM [password_resets];
SELECT * FROM [user_preferences];

DELETE FROM [users];          --
DELETE FROM [workspaces];     --
DELETE FROM [projects];       --
DELETE FROM [task_lists];     --error
DELETE FROM [tasks];          --
DELETE FROM [task_assignees]; --
DELETE FROM [task_dependencies];
DELETE FROM [subtasks];       --
DELETE FROM [attachments];    --
DELETE FROM [project_members];--
DELETE FROM [tags];           --
DELETE FROM [project_tags];   --
DELETE FROM [invitations];
DELETE FROM [activities];
DELETE FROM [comments];       --
DELETE FROM [notification_recipients];
DELETE FROM [password_resets];
DELETE FROM [user_preferences];

DBCC CHECKIDENT ('[dbo].[users]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[workspaces]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[projects]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[task_lists]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[tasks]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[subtasks]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[tags]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[activities]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[attachments]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[invitations]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[comments]', RESEED, 0);

-- =================================================================================
--  PronaFlow - SCRIPT CHÈN DỮ LIỆU MẪU (ĐÃ CẬP NHẬT USER VÀ SỬA LỖI LOGIC)
--  PURPOSE: Chèn dữ liệu mẫu với bộ người dùng mới, đảm bảo tính toàn vẹn dữ liệu
--           bằng cách sử dụng biến để lưu trữ và tham chiếu các ID tự tăng.
--  AUTHOR:  Gemini (dựa trên yêu cầu của bạn)
-- =================================================================================

USE db_PronaFlow;
GO

-- Khai báo các biến để lưu trữ ID
DECLARE @UserID_DavidJones BIGINT, @UserID_RichardBrown BIGINT, @UserID_CharlesTaylor BIGINT, @UserID_LindaWilliams BIGINT, @UserID_SusanMoore BIGINT;
DECLARE @WorkspaceID_DavidJones BIGINT, @WorkspaceID_RichardBrown BIGINT, @WorkspaceID_CharlesTaylor BIGINT, @WorkspaceID_LindaWilliams BIGINT, @WorkspaceID_SusanMoore BIGINT;
DECLARE @ProjectID_ApiDev BIGINT, @ProjectID_Marketing BIGINT, @ProjectID_Redesign BIGINT, @ProjectID_Sql BIGINT;
DECLARE @TaskListID_Api1 BIGINT, @TaskListID_Api2 BIGINT, @TaskListID_Api3 BIGINT, @TaskListID_Api4 BIGINT;
DECLARE @TaskListID_Mkt1 BIGINT, @TaskListID_Mkt2 BIGINT, @TaskListID_Mkt3 BIGINT, @TaskListID_Mkt4 BIGINT;
DECLARE @TaskListID_Design1 BIGINT, @TaskListID_Design2 BIGINT, @TaskListID_Design3 BIGINT;
DECLARE @TaskID_ApiDefine BIGINT, @TaskID_ApiDbSetup BIGINT, @TaskID_ApiAuth BIGINT, @TaskID_ApiProjectModule BIGINT, @TaskID_ApiTest BIGINT;
DECLARE @TaskID_MktAnalyze BIGINT, @TaskID_MktDesign BIGINT, @TaskID_MktEmail BIGINT;
DECLARE @TagID_Backend BIGINT, @TagID_Api BIGINT, @TagID_Db BIGINT, @TagID_Marketing BIGINT, @TagID_Social BIGINT, @TagID_UiUx BIGINT, @TagID_Design BIGINT;

BEGIN TRANSACTION;

BEGIN TRY

    -- ================================================
    -- 1. CHÈN DỮ LIỆU BẢNG USERS (ĐÃ CẬP NHẬT)
    -- ================================================
    INSERT INTO [dbo].[users] ([username], [email], [password_hash], [full_name], [avatar_url], [bio], [theme_preference], [role]) VALUES
    ('davidjones', 'david.jones77@pronaflow.dev', '$2a$11$391sNVkYms1LJnAk684ONeiT89QnngWUuldu0WX9x2G/qJdy6ioXi', 'David Jones', 'https://i.pravatar.cc/150?u=davidjones', 'Backend Developer', 'dark', 'admin'),
    ('richardbrown', 'richard.brown83@pronaflow.dev', '$2a$11$3zEP2cjdzpN5oBH0ge.1BuN.OJAGAdpcnr/D7jJoaBK0DQr5oxXiG', 'Richard Brown', 'https://i.pravatar.cc/150?u=richardbrown', 'Project Manager', 'light', 'user'),
    ('charlestaylor', 'charles.taylor15@pronaflow.dev', '$2a$11$bjNlJhYSci2kyThVUJWjYOKRMrdZA675ZgSjybmAXkZl1FtUybjB2', 'Charles Taylor', 'https://i.pravatar.cc/150?u=charlestaylor', 'Frontend Developer', 'dark', 'user'),
    ('lindawilliams', 'linda.williams78@pronaflow.dev', '$2a$11$At3C71KCDcgrrXBC4zUbtuH4pkl8UoL617xMS6.5qqrIaKoAglqVi', 'Linda Williams', 'https://i.pravatar.cc/150?u=lindawilliams', 'UX/UI Designer', 'light', 'user'),
    ('susanmoore', 'susan.moore41@pronaflow.dev', '$2a$11$jeplLITyVchMWWNSBCz6zOT64TJ5kyxm187vmR.UhmrKC4OurWx.q', 'Susan Moore', 'https://i.pravatar.cc/150?u=susanmoore', 'QA Engineer', 'dark', 'user');

    -- Lấy ID của các user vừa tạo
    SELECT @UserID_DavidJones = id FROM [dbo].[users] WHERE username = 'davidjones';
    SELECT @UserID_RichardBrown = id FROM [dbo].[users] WHERE username = 'richardbrown';
    SELECT @UserID_CharlesTaylor = id FROM [dbo].[users] WHERE username = 'charlestaylor';
    SELECT @UserID_LindaWilliams = id FROM [dbo].[users] WHERE username = 'lindawilliams';
    SELECT @UserID_SusanMoore = id FROM [dbo].[users] WHERE username = 'susanmoore';

    -- ================================================
    -- 2. CHÈN DỮ LIỆU BẢNG WORKSPACES (THEO NGHIỆP VỤ)
    -- ================================================
    -- Tạo workspace mặc định cho mỗi user
    INSERT INTO [dbo].[workspaces] ([owner_id], [name], [description]) VALUES
    (@UserID_DavidJones, 'David Jones''s Workspace', 'Default workspace for David Jones.'),
    (@UserID_RichardBrown, 'Richard Brown''s Workspace', 'Default workspace for Richard Brown.'),
    (@UserID_CharlesTaylor, 'Charles Taylor''s Workspace', 'Default workspace for Charles Taylor.'),
    (@UserID_LindaWilliams, 'Linda Williams''s Workspace', 'Default workspace for Linda Williams.'),
    (@UserID_SusanMoore, 'Susan Moore''s Workspace', 'Default workspace for Susan Moore.');

    -- Lấy ID của các workspace chính sẽ được sử dụng
    SELECT @WorkspaceID_DavidJones = id FROM [dbo].[workspaces] WHERE owner_id = @UserID_DavidJones;
    SELECT @WorkspaceID_RichardBrown = id FROM [dbo].[workspaces] WHERE owner_id = @UserID_RichardBrown;
    SELECT @WorkspaceID_LindaWilliams = id FROM [dbo].[workspaces] WHERE owner_id = @UserID_LindaWilliams;

    -- ================================================
    -- 3. CHÈN DỮ LIỆU BẢNG PROJECTS
    -- ================================================
    INSERT INTO [dbo].[projects] ([workspace_id], [name], [description], [cover_image_url], [status], [project_type], [start_date], [end_date]) VALUES
    (@WorkspaceID_DavidJones, 'API Development for PronaFlow', 'Building the core RESTful APIs for the application.', 'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8Y29kaW5nLGFwaXx8fHx8fDE2MjgxODY2Mzc', 'in-progress', 'team', '2023-05-01', '2023-12-20'),
    (@WorkspaceID_RichardBrown, 'Marketing Campaign for University Event', 'Planning and executing a marketing campaign for the annual tech fair.', 'https://images.unsplash.com/photo-1557804506-669a67965ba0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8bWFya2V0aW5nLHRlYW18fHx8fHwxNjI4MTg2NzU4', 'in-review', 'team', '2023-08-15', '2023-11-10'),
    (@WorkspaceID_LindaWilliams, 'UI/UX Redesign for E-commerce Site', 'A complete overhaul of the user interface and experience for an online store.', 'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8ZGVzaWduLHVpfHx8fHx8MTYyODE4NjgxNQ', 'done', 'personal', '2023-02-01', '2023-07-31'),
    (@WorkspaceID_DavidJones, 'Learn Advanced SQL', 'Personal project to master advanced SQL concepts and techniques.', 'https://images.unsplash.com/photo-1522202176988-66273c2fd55f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8bGVhcm5pbmcsZGF0YWJhc2V8fHx8fHwxNjI4MTg2OTI0', 'not-started', 'personal', '2023-10-01', '2023-12-31');

    -- Lấy ID của các project vừa tạo
    SELECT @ProjectID_ApiDev = id FROM [dbo].[projects] WHERE name = 'API Development for PronaFlow';
    SELECT @ProjectID_Marketing = id FROM [dbo].[projects] WHERE name = 'Marketing Campaign for University Event';
    SELECT @ProjectID_Redesign = id FROM [dbo].[projects] WHERE name = 'UI/UX Redesign for E-commerce Site';
    SELECT @ProjectID_Sql = id FROM [dbo].[projects] WHERE name = 'Learn Advanced SQL';

    -- ================================================
    -- 4. CHÈN DỮ LIỆU BẢNG TASK_LISTS
    -- ================================================
    INSERT INTO [dbo].[task_lists] ([project_id], [name], [position]) VALUES
    (@ProjectID_ApiDev, 'Phase 1: Planning & Design', 1),
    (@ProjectID_ApiDev, 'Phase 2: Core Module Development', 2),
    (@ProjectID_ApiDev, 'Phase 3: Integration & Testing', 3),
    (@ProjectID_ApiDev, 'Phase 4: Deployment', 4),
    (@ProjectID_Marketing, 'Phrase 1: Research & Strategy', 1),
    (@ProjectID_Marketing, 'Phrase 2: Content Creation', 2),
    (@ProjectID_Marketing, 'Phrase 3: Campaign Execution', 3),
    (@ProjectID_Marketing, 'Phrase 4: Performance Review', 4),
    (@ProjectID_Redesign, 'Discovery & Research', 1),
    (@ProjectID_Redesign, 'Wireframing & Prototyping', 2),
    (@ProjectID_Redesign, 'Visual Design & Handoff', 3);

    -- Lấy ID của các task_list vừa tạo
    SELECT @TaskListID_Api1 = id FROM [dbo].[task_lists] WHERE project_id = @ProjectID_ApiDev AND name = 'Phase 1: Planning & Design';
    SELECT @TaskListID_Api2 = id FROM [dbo].[task_lists] WHERE project_id = @ProjectID_ApiDev AND name = 'Phase 2: Core Module Development';
    SELECT @TaskListID_Api3 = id FROM [dbo].[task_lists] WHERE project_id = @ProjectID_ApiDev AND name = 'Phase 3: Integration & Testing';
    SELECT @TaskListID_Mkt1 = id FROM [dbo].[task_lists] WHERE project_id = @ProjectID_Marketing AND name = 'Phrase 1: Research & Strategy';
    SELECT @TaskListID_Mkt2 = id FROM [dbo].[task_lists] WHERE project_id = @ProjectID_Marketing AND name = 'Phrase 2: Content Creation';
    SELECT @TaskListID_Mkt3 = id FROM [dbo].[task_lists] WHERE project_id = @ProjectID_Marketing AND name = 'Phrase 3: Campaign Execution';


    -- ================================================
    -- 5. CHÈN DỮ LIỆU BẢNG TASKS
    -- ================================================
    INSERT INTO [dbo].[tasks] ([project_id], [task_list_id], [creator_id], [name], [description], [priority], [status], [start_date], [end_date]) VALUES
    (@ProjectID_ApiDev, @TaskListID_Api1, @UserID_DavidJones, 'Define API endpoints and data models', 'Document all required endpoints using Swagger/OpenAPI specification.', 'high', 'done', '2023-05-02', '2023-05-15'),
    (@ProjectID_ApiDev, @TaskListID_Api1, @UserID_RichardBrown, 'Setup database schema and migrations', 'Create the initial database schema based on the data models.', 'high', 'done', '2023-05-10', '2023-05-20'),
    (@ProjectID_ApiDev, @TaskListID_Api2, @UserID_DavidJones, 'Implement User Authentication Service', 'Develop JWT-based authentication and authorization.', 'high', 'in-progress', '2023-05-21', '2023-06-15'),
    (@ProjectID_ApiDev, @TaskListID_Api2, @UserID_CharlesTaylor, 'Develop Project Management Module', 'CRUD operations for projects, task lists, and tasks.', 'normal', 'not-started', '2023-06-16', '2023-07-31'),
    (@ProjectID_ApiDev, @TaskListID_Api3, @UserID_SusanMoore, 'Write unit and integration tests', 'Ensure code coverage of at least 80% for all services.', 'normal', 'not-started', '2023-08-01', '2023-09-30'),
    (@ProjectID_Marketing, @TaskListID_Mkt1, @UserID_RichardBrown, 'Analyze target audience', 'Create detailed personas for university students.', 'high', 'done', '2023-08-16', '2023-08-25'),
    (@ProjectID_Marketing, @TaskListID_Mkt2, @UserID_LindaWilliams, 'Design social media visuals', 'Create engaging posts for Instagram, Facebook, and TikTok.', 'normal', 'in-progress', '2023-08-26', '2023-09-15'),
    (@ProjectID_Marketing, @TaskListID_Mkt3, @UserID_RichardBrown, 'Launch email marketing campaign', 'Send out a series of promotional emails to the student body.', 'high', 'not-started', '2023-09-20', '2023-10-10');

    -- Lấy ID của các task vừa tạo
    SELECT @TaskID_ApiDefine = id FROM [dbo].[tasks] WHERE name = 'Define API endpoints and data models';
    SELECT @TaskID_ApiAuth = id FROM [dbo].[tasks] WHERE name = 'Implement User Authentication Service';
    SELECT @TaskID_ApiProjectModule = id FROM [dbo].[tasks] WHERE name = 'Develop Project Management Module';
    SELECT @TaskID_MktDesign = id FROM [dbo].[tasks] WHERE name = 'Design social media visuals';


    -- ================================================
    -- 6. CHÈN DỮ LIỆU BẢNG SUBTASKS
    -- ================================================
    INSERT INTO [dbo].[subtasks] ([task_id], [name], [is_completed], [position]) VALUES
    (@TaskID_ApiDefine, 'Draft initial OpenAPI specification', 1, 1),
    (@TaskID_ApiDefine, 'Review spec with frontend team', 1, 2),
    (@TaskID_ApiDefine, 'Finalize v1.0 of the API documentation', 0, 3),
    (@TaskID_ApiAuth, 'Setup Identity Framework', 1, 1),
    (@TaskID_ApiAuth, 'Implement token generation endpoint', 0, 2),
    (@TaskID_ApiAuth, 'Implement role-based access control', 0, 3);

    -- ================================================
    -- 7. CHÈN DỮ LIỆU BẢNG TAGS
    -- ================================================
    INSERT INTO [dbo].[tags] ([workspace_id], [name], [color_hex]) VALUES
    (@WorkspaceID_DavidJones, 'Backend', '#3498DB'),
    (@WorkspaceID_DavidJones, 'API', '#9B59B6'),
    (@WorkspaceID_DavidJones, 'Database', '#F1C40F'),
    (@WorkspaceID_RichardBrown, 'Marketing', '#2ECC71'),
    (@WorkspaceID_RichardBrown, 'Social Media', '#E74C3C'),
    (@WorkspaceID_LindaWilliams, 'UI/UX', '#1ABC9C'),
    (@WorkspaceID_LindaWilliams, 'Design', '#E67E22');

    -- Lấy ID của các tag vừa tạo
    SELECT @TagID_Backend = id FROM [dbo].[tags] WHERE name = 'Backend';
    SELECT @TagID_Api = id FROM [dbo].[tags] WHERE name = 'API';
    SELECT @TagID_Db = id FROM [dbo].[tags] WHERE name = 'Database';
    SELECT @TagID_Marketing = id FROM [dbo].[tags] WHERE name = 'Marketing';
    SELECT @TagID_Social = id FROM [dbo].[tags] WHERE name = 'Social Media';
    SELECT @TagID_UiUx = id FROM [dbo].[tags] WHERE name = 'UI/UX';
    SELECT @TagID_Design = id FROM [dbo].[tags] WHERE name = 'Design';


    -- ================================================
    -- 8. CHÈN DỮ LIỆU CÁC BẢNG NỐI
    -- ================================================
    -- Bảng project_members
    INSERT INTO [dbo].[project_members] ([project_id], [user_id], [role]) VALUES
    (@ProjectID_ApiDev, @UserID_DavidJones, 'admin'),
    (@ProjectID_ApiDev, @UserID_RichardBrown, 'member'),
    (@ProjectID_ApiDev, @UserID_CharlesTaylor, 'member'),
    (@ProjectID_ApiDev, @UserID_SusanMoore, 'member'),
    (@ProjectID_Marketing, @UserID_RichardBrown, 'admin'),
    (@ProjectID_Marketing, @UserID_LindaWilliams, 'member'),
    (@ProjectID_Redesign, @UserID_LindaWilliams, 'admin'),
    (@ProjectID_Sql, @UserID_DavidJones, 'admin');

    -- Bảng project_tags
    INSERT INTO [dbo].[project_tags] ([project_id], [tag_id]) VALUES
    (@ProjectID_ApiDev, @TagID_Backend),
    (@ProjectID_ApiDev, @TagID_Api),
    (@ProjectID_ApiDev, @TagID_Db),
    (@ProjectID_Marketing, @TagID_Marketing),
    (@ProjectID_Marketing, @TagID_Social),
    (@ProjectID_Redesign, @TagID_UiUx),
    (@ProjectID_Redesign, @TagID_Design);

    -- Bảng task_assignees
    INSERT INTO [dbo].[task_assignees] ([task_id], [user_id])
    SELECT T.id, U.id
    FROM (
        VALUES
            ('Define API endpoints and data models', 'davidjones'),
            ('Define API endpoints and data models', 'charlestaylor'),
            ('Setup database schema and migrations', 'davidjones'),
            ('Implement User Authentication Service', 'davidjones'),
            ('Develop Project Management Module', 'charlestaylor'),
            ('Write unit and integration tests', 'susanmoore'),
            ('Analyze target audience', 'richardbrown'),
            ('Design social media visuals', 'lindawilliams'),
            ('Launch email marketing campaign', 'richardbrown')
    ) AS V(TaskName, UserName)
    JOIN [dbo].[tasks] T ON V.TaskName = T.name
    JOIN [dbo].[users] U ON V.UserName = U.username;


    -- Bảng comments
    INSERT INTO [dbo].[comments] ([user_id], [content], [commentable_id], [commentable_type]) VALUES
    (@UserID_RichardBrown, 'Great progress on the API spec! I have a few questions about the pagination parameters.', @TaskID_ApiDefine, 'task'),
    (@UserID_CharlesTaylor, 'I will start working on the Project module next week. Please assign the relevant tickets to me.', @TaskID_ApiProjectModule, 'task'),
    (@UserID_LindaWilliams, 'Can we get a review of the latest social media designs by Friday?', @TaskID_MktDesign, 'task'),
    (@UserID_DavidJones, 'This campaign is looking promising. Let''s ensure we track the analytics closely.', @ProjectID_Marketing, 'project');

    COMMIT TRANSACTION;
    PRINT 'Dữ liệu mẫu đã được chèn thành công!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Đã xảy ra lỗi. Giao dịch đã được rollback.';
    -- In thông tin lỗi chi tiết
    THROW;
END CATCH;
GO