-- ========================================================================================
-- PRONAFLOW - NEW ENGLISH SAMPLE DATA SCRIPT (Generated without Faker)
-- MASTER SCRIPT FOR DATABASE SEEDING
-- PURPOSE: Deletes all existing data and populates the database using individual table scripts.
-- EXECUTION MODE: Ensure SQLCMD mode is enabled in SQL Server Management Studio.
-- (Query > SQLCMD Mode)
-- ========================================================================================

USE db_PronaFlow;
GO
SELECT * FROM notification_recipients;
SELECT * FROM task_dependencies;
SELECT * FROM project_tags;
SELECT * FROM task_assignees;
SELECT * FROM project_members;
SELECT * FROM password_resets;
SELECT * FROM user_preferences;
SELECT * FROM comments;
SELECT * FROM invitations;
SELECT * FROM attachments;
SELECT * FROM activities;
SELECT * FROM tags;
SELECT * FROM subtasks;
SELECT * FROM tasks;
SELECT * FROM task_lists;
SELECT * FROM projects;
SELECT * FROM workspaces;
SELECT * FROM users;

-- ========================================================================================
-- PRE-INSERT SETUP
-- ========================================================================================
PRINT '--- Starting Database Seeding Process ---';
PRINT 'Disabling all triggers and constraints...';

EXEC sp_msforeachtable 'ALTER TABLE ? DISABLE TRIGGER all';
GO
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all';
GO

BEGIN TRANSACTION;
BEGIN TRY

    -- ========================================================================================
    -- STEP 1: TRUNCATE ALL DATA TABLES (in reverse dependency order)
    -- ========================================================================================
    PRINT '1. Truncating all data tables...';
    TRUNCATE TABLE [dbo].[notification_recipients];
	TRUNCATE TABLE [dbo].[task_dependencies];
	TRUNCATE TABLE [dbo].[project_tags];
	TRUNCATE TABLE [dbo].[tags];
	TRUNCATE TABLE [dbo].[task_assignees];
	TRUNCATE TABLE [dbo].[project_members];
	TRUNCATE TABLE [dbo].[password_resets];
	TRUNCATE TABLE [dbo].[user_preferences];
	TRUNCATE TABLE [dbo].[comments];
	TRUNCATE TABLE [dbo].[invitations];
	TRUNCATE TABLE [dbo].[attachments];
	TRUNCATE TABLE [dbo].[activities];
	
	TRUNCATE TABLE [dbo].[subtasks];
	TRUNCATE TABLE [dbo].[tasks];
	TRUNCATE TABLE [dbo].[task_lists];
	TRUNCATE TABLE [dbo].[projects];
	TRUNCATE TABLE [dbo].[workspaces];
	TRUNCATE TABLE [dbo].[users];
    

    -- ========================================================================================
    -- STEP 2: INSERT DATA FROM FILES (in correct dependency order)
    -- ========================================================================================
    PRINT '2. Populating data from individual files...';
    --:r .\01_users_data.sql
	--:r .\02_workspaces_data.sql
	--:r .\03_tags_data.sql
	--:r .\04_projects_data.sql
	--:r .\05_project_members_data.sql
	--:r .\06_project_tags_data.sql
	--:r .\07_task_lists_data.sql
	--:r .\08_tasks_data.sql
	--:r .\09_task_assignees_data.sql
	--:r .\10_subtasks_data.sql
	--:r .\11_comments_data.sql
	--:r .\12_attachments_data.sql

    COMMIT TRANSACTION;
    PRINT '--- Data seeding committed successfully. ---';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '--- An error occurred. Transaction rolled back. ---';
    -- In ra thông tin lỗi chi tiết
    SELECT  
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_SEVERITY() AS ErrorSeverity,
        ERROR_STATE() AS ErrorState,
        ERROR_PROCEDURE() AS ErrorProcedure,
        ERROR_LINE() AS ErrorLine,
        ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO

-- ========================================================================================
-- POST-INSERT CLEANUP
-- ========================================================================================
PRINT 'Re-enabling all constraints and triggers...';

EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all';
GO
EXEC sp_msforeachtable 'ALTER TABLE ? ENABLE TRIGGER all';
GO

PRINT '--- Database Seeding Process Completed ---';