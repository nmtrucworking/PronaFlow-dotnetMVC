--===============================--
-- ADMIN --
USE db_PronaFlow;
GO

INSERT INTO [dbo].[users] ([username], [email], [password_hash], [full_name], [bio], [role]) VALUES
('superadmin', 'admin@pronaflow.io', '$2a$11$7mi5ilhKCwuHqyeAWL1JIu2.EkAhWgKZEAsPahI1LoxrbPYjFzAOu', 'PronaFlow Admin', N'Module sprint cross-platform next-generation interface analyzing designing secure.', 'admin')
