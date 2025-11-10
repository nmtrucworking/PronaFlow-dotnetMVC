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
	SELECT id FROM [dbo].[users] WHERE username = 'davidjones'; -- 2
    SELECT id FROM [dbo].[users] WHERE username = 'richardbrown'; -- 3
    SELECT id FROM [dbo].[users] WHERE username = 'charlestaylor'; -- 4
    SELECT id FROM [dbo].[users] WHERE username = 'lindawilliams'; -- 5
    SELECT id FROM [dbo].[users] WHERE username = 'susanmoore'; -- 6

    -- ================================================
    -- 2. CHÈN DỮ LIỆU BẢNG WORKSPACES (THEO NGHIỆP VỤ)
    -- ================================================
    -- Tạo workspace mặc định cho mỗi user
    /*INSERT INTO [dbo].[workspaces] ([owner_id], [name], [description]) VALUES
    (2, 'David Jones''s Workspace', 'Default workspace for David Jones.'),
    (3, 'Richard Brown''s Workspace', 'Default workspace for Richard Brown.'),
    (4, 'Charles Taylor''s Workspace', 'Default workspace for Charles Taylor.'),
    (5, 'Linda Williams''s Workspace', 'Default workspace for Linda Williams.'),
    (6, 'Susan Moore''s Workspace', 'Default workspace for Susan Moore.');
	*/
    -- Lấy ID của các workspace chính sẽ được sử dụng
    SELECT id FROM [dbo].[workspaces] WHERE owner_id = 2; -- 6
    SELECT id FROM [dbo].[workspaces] WHERE owner_id = 3; -- 5
    SELECT id FROM [dbo].[workspaces] WHERE owner_id = 5; -- @WorkspaceID_LindaWilliams = 3

    -- ================================================
    -- 3. CHÈN DỮ LIỆU BẢNG PROJECTS
    -- ================================================
    INSERT INTO [dbo].[projects] ([workspace_id], [name], [description], [cover_image_url], [status], [project_type], [start_date], [end_date]) VALUES
    (6, 'API Development for PronaFlow', 'Building the core RESTful APIs for the application.', 'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8Y29kaW5nLGFwaXx8fHx8fDE2MjgxODY2Mzc', 'in-progress', 'team', '2023-05-01', '2023-12-20'),
    (5, 'Marketing Campaign for University Event', 'Planning and executing a marketing campaign for the annual tech fair.', 'https://images.unsplash.com/photo-1557804506-669a67965ba0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8bWFya2V0aW5nLHRlYW18fHx8fHwxNjI4MTg2NzU4', 'in-review', 'team', '2023-08-15', '2023-11-10'),
    (3, 'UI/UX Redesign for E-commerce Site', 'A complete overhaul of the user interface and experience for an online store.', 'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8ZGVzaWduLHVpfHx8fHx8MTYyODE4NjgxNQ', 'done', 'personal', '2023-02-01', '2023-07-31'),
    (6, 'Learn Advanced SQL', 'Personal project to master advanced SQL concepts and techniques.', 'https://images.unsplash.com/photo-1522202176988-66273c2fd55f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=MnwxfDB8MXxyYW5kb218MHx8bGVhcm5pbmcsZGF0YWJhc2V8fHx8fHwxNjI4MTg2OTI0', 'not-started', 'personal', '2023-10-01', '2023-12-31');

    -- Lấy ID của các project vừa tạo
    SELECT id FROM [dbo].[projects] WHERE name = 'API Development for PronaFlow'; -- 0 0
    SELECT id FROM [dbo].[projects] WHERE name = 'Marketing Campaign for University Event';
    SELECT id FROM [dbo].[projects] WHERE name = 'UI/UX Redesign for E-commerce Site';
    SELECT id FROM [dbo].[projects] WHERE name = 'Learn Advanced SQL';

    -- ================================================
    -- 4. CHÈN DỮ LIỆU BẢNG TASK_LISTS
    -- ================================================
    INSERT INTO [dbo].[task_lists] ([project_id], [name], [position]) VALUES
    (0, 'Phase 1: Planning & Design', 1),
    (0, 'Phase 2: Core Module Development', 2),
    (0, 'Phase 3: Integration & Testing', 3),
    (0, 'Phase 4: Deployment', 4),
    (1, 'Phrase 1: Research & Strategy', 1),
    (1, 'Phrase 2: Content Creation', 2),
    (1, 'Phrase 3: Campaign Execution', 3),
    (1, 'Phrase 4: Performance Review', 4),
    (2, 'Discovery & Research', 1),
    (2, 'Wireframing & Prototyping', 2),
    (2, 'Visual Design & Handoff', 3);

    -- Lấy ID của các task_list vừa tạo
	SELECT id FROM [dbo].[task_lists] WHERE project_id = 0 AND name = 'Phase 1: Planning & Design';
    SELECT id FROM [dbo].[task_lists] WHERE project_id = 0 AND name = 'Phase 2: Core Module Development';
    SELECT id FROM [dbo].[task_lists] WHERE project_id = 0 AND name = 'Phase 3: Integration & Testing';
    SELECT id FROM [dbo].[task_lists] WHERE project_id = 1 AND name = 'Phrase 1: Research & Strategy';
    SELECT id FROM [dbo].[task_lists] WHERE project_id = 1 AND name = 'Phrase 2: Content Creation';
    SELECT id FROM [dbo].[task_lists] WHERE project_id = 1 AND name = 'Phrase 3: Campaign Execution';


    -- ================================================
    -- 5. CHÈN DỮ LIỆU BẢNG TASKS
    -- ================================================
    INSERT INTO [dbo].[tasks] ([project_id], [task_list_id], [creator_id], [name], [description], [priority], [status], [start_date], [end_date]) VALUES
    (0, 0, 2, 'Define API endpoints and data models', 'Document all required endpoints using Swagger/OpenAPI specification.', 'high', 'done', '2023-05-02', '2023-05-15'),
    (0, 0, 3, 'Setup database schema and migrations', 'Create the initial database schema based on the data models.', 'high', 'done', '2023-05-10', '2023-05-20'),
    (0, 1, 2, 'Implement User Authentication Service', 'Develop JWT-based authentication and authorization.', 'high', 'in-progress', '2023-05-21', '2023-06-15'),
    (0, 1, 4, 'Develop Project Management Module', 'CRUD operations for projects, task lists, and tasks.', 'normal', 'not-started', '2023-06-16', '2023-07-31'),
    (0, 3, 6, 'Write unit and integration tests', 'Ensure code coverage of at least 80% for all services.', 'normal', 'not-started', '2023-08-01', '2023-09-30'),
    (1, 4, 3, 'Analyze target audience', 'Create detailed personas for university students.', 'high', 'done', '2023-08-16', '2023-08-25'),
    (1, 5, 5, 'Design social media visuals', 'Create engaging posts for Instagram, Facebook, and TikTok.', 'normal', 'in-progress', '2023-08-26', '2023-09-15'),
    (1, 6, 3, 'Launch email marketing campaign', 'Send out a series of promotional emails to the student body.', 'high', 'not-started', '2023-09-20', '2023-10-10');

    -- Lấy ID của các task vừa tạo
    
	SELECT id FROM [dbo].[tasks] WHERE name = 'Define API endpoints and data models';
    SELECT id FROM [dbo].[tasks] WHERE name = 'Implement User Authentication Service';
    SELECT id FROM [dbo].[tasks] WHERE name = 'Develop Project Management Module';
    SELECT id FROM [dbo].[tasks] WHERE name = 'Design social media visuals';


    -- ================================================
    -- 6. CHÈN DỮ LIỆU BẢNG SUBTASKS
    -- ================================================
    INSERT INTO [dbo].[subtasks] ([task_id], [name], [is_completed], [position]) VALUES
    (0, 'Draft initial OpenAPI specification', 1, 1),
    (0, 'Review spec with frontend team', 1, 2),
    (0, 'Finalize v1.0 of the API documentation', 0, 3),
    (2, 'Setup Identity Framework', 1, 1),
    (2, 'Implement token generation endpoint', 0, 2),
    (2, 'Implement role-based access control', 0, 3);

    -- ================================================
    -- 7. CHÈN DỮ LIỆU BẢNG TAGS
    -- ================================================
    INSERT INTO [dbo].[tags] ([workspace_id], [name], [color_hex]) VALUES
    (6, 'Backend', '#3498DB'),
    (6, 'API', '#9B59B6'),
    (6, 'Database', '#F1C40F'),
    (5, 'Marketing', '#2ECC71'),
    (5, 'Social Media', '#E74C3C'),
    (3, 'UI/UX', '#1ABC9C'),
    (3, 'Design', '#E67E22');

    -- Lấy ID của các tag vừa tạo
    SELECT id FROM [dbo].[tags] WHERE name = 'Backend';
    SELECT id FROM [dbo].[tags] WHERE name = 'API';
    SELECT id FROM [dbo].[tags] WHERE name = 'Database';
    SELECT id FROM [dbo].[tags] WHERE name = 'Marketing';
    SELECT id FROM [dbo].[tags] WHERE name = 'Social Media';
    SELECT id FROM [dbo].[tags] WHERE name = 'UI/UX';
    SELECT id FROM [dbo].[tags] WHERE name = 'Design';


    -- ================================================
    -- 8. CHÈN DỮ LIỆU CÁC BẢNG NỐI
    -- ================================================
    -- Bảng project_members
    INSERT INTO [dbo].[project_members] ([project_id], [user_id], [role]) VALUES
    (0, 2, 'admin'),
    (0, 3, 'member'),
    (0, 4, 'member'),
    (0, 6, 'member'),
    (1, 3, 'admin'),
    (1, 5, 'member'),
    (2, 5, 'admin'),
    (3, 2, 'admin');

    -- Bảng project_tags
    INSERT INTO [dbo].[project_tags] ([project_id], [tag_id]) VALUES
    (0, 0),
    (0, 1),
    (0, 2),
    (1, 3),
    (1, 4),
    (2, 5),
    (2, 6);

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
    (3, 'Great progress on the API spec! I have a few questions about the pagination parameters.', 0, 'task'),
    (4, 'I will start working on the Project module next week. Please assign the relevant tickets to me.', 3, 'task'),
    (5, 'Can we get a review of the latest social media designs by Friday?', 6, 'task'),
    (2, 'This campaign is looking promising. Let''s ensure we track the analytics closely.', 1, 'project');

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