use db_PronaFlow;

SELECT * FROM task_lists;
SELECT * FROM tasks;
GO
SELECT * FROM workspaces
INSERT INTO [dbo].[task_lists] ([project_id], [name], [position]) VALUES
(6, 'Backend Tasks', 1),
(6, 'Frontend Tasks', 2);
GO

INSERT INTO [dbo].[tasks] ([project_id], [task_list_id], [creator_id], [name], [description], [priority], [status], [start_date], [end_date]) VALUES
    (6, 30, 2, 'Define API endpoints and data models', 'Document all required endpoints using Swagger/OpenAPI specification.', 'high', 'done', '2023-05-02', '2023-10-31'),
    (6, 30, 2, 'Setup database schema and migrations', 'Create the initial database schema based on the data models.', 'high', 'done', '2023-05-10', '2023-05-20')
GO

SELECT 
	w.name,
	p.name,
	tl.name,
	t.*
FROM workspaces as w
INNER JOIN projects AS p ON w.id = p.workspace_id
INNER JOIN task_lists AS tl ON p.id = tl.project_id
INNER JOIN tasks AS t ON tl.id = t.task_list_id
WHERE w.id = 6
GO