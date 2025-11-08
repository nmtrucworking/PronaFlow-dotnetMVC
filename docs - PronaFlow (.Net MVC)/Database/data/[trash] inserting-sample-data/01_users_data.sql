-- ========================================================================================
-- PRONAFLOW - NEW ENGLISH SAMPLE DATA SCRIPT (Generated without Faker)
-- PURPOSE: Deletes all existing data and populates the database (tags).
-- TABLE: [db].[users]
-- FILE: 01_users_data.sql
-- ========================================================================================
SET IDENTITY_INSERT [dbo].[users] ON;
GO

INSERT INTO [dbo].[users] ([username], [email], [password_hash], [full_name], [bio], [role]) VALUES
('superadmin', 'admin@pronaflow.io', '$2a$12$placeholderhash', 'PronaFlow Admin', N'Module sprint cross-platform next-generation interface analyzing designing secure.', 'admin'),
('david.jones77', 'david.jones77@pronaflow.dev', '$2a$12$placeholderhash', N'David Jones', N'Protocol automated sprint deployment network. High-performance algorithm feature agile framework finalizing responsive framework upgrading.', 'user'),
('richard.brown83', 'richard.brown83@pronaflow.dev', '$2a$12$placeholderhash', N'Richard Brown', N'Network automated feature automated network module robust optimized network database. Optimized algorithm robust agile cloud-native developing test robust developing high-performance.', 'user'),
('charles.taylor15', 'charles.taylor15@pronaflow.dev', '$2a$12$placeholderhash', N'Charles Taylor', N'Developing modular automated cross-platform user-friendly. Integrating network agile cross-platform testing network integrated user-friendly.', 'user'),
('linda.williams78', 'linda.williams78@pronaflow.dev', '$2a$12$placeholderhash', N'Linda Williams', N'Cloud-native integration algorithm module interface secure implementing. Secure developing portal analyzing api.', 'user'),
('susan.moore41', 'susan.moore41@pronaflow.dev', '$2a$12$placeholderhash', N'Susan Moore', N'Deploying documentation documentation protocol portal. Testing interface finalizing sprint api.', 'user'),
('alex.jackson34', 'alex.jackson34@pronaflow.dev', '$2a$12$placeholderhash', N'Alex Jackson', N'System framework modular documentation implementing module. Developing automated protocol portal integrated interface documentation.', 'user'),
('peter.anderson65', 'peter.anderson65@pronaflow.dev', '$2a$12$placeholderhash', N'Peter Anderson', N'Api sprint cloud-native cross-platform integration module system feature. Designing designing testing deployment automated next-generation secure build deploying system.', 'user'),
('susan.harris21', 'susan.harris21@pronaflow.dev', '$2a$12$placeholderhash', N'Susan Harris', N'Integration build documentation documentation system finalizing. Network optimized deployment next-generation modular refactoring responsive developing responsive developing.', 'user'),
('alex.thomas61', 'alex.thomas61@pronaflow.dev', '$2a$12$placeholderhash', N'Alex Thomas', N'System framework documentation cloud-native deployment. Next-generation release user-friendly upgrading modular.', 'user'),
('alex.thompson43', 'alex.thompson43@pronaflow.dev', '$2a$12$placeholderhash', N'Alex Thompson', N'Upgrading implementing scalable secure finalizing build integrated release test. Feature deployment upgrading algorithm implementing.', 'user'),
('susan.williams13', 'susan.williams13@pronaflow.dev', '$2a$12$placeholderhash', N'Susan Williams', N'Protocol high-performance integrated test automated documentation scalable system protocol database. Finalizing release analyzing integrating finalizing sprint.', 'user'),
('james.davis54', 'james.davis54@pronaflow.dev', '$2a$12$placeholderhash', N'James Davis', N'Modular module optimized framework algorithm build implementing algorithm next-generation upgrading. Component user-friendly network optimized release portal implementing interface.', 'user'),
('susan.martin31', 'susan.martin31@pronaflow.dev', '$2a$12$placeholderhash', N'Susan Martin', N'Researching framework network user-friendly release. Responsive integration build user-friendly database integrated integrating modular responsive optimized.', 'user'),
('richard.thompson56', 'richard.thompson56@pronaflow.dev', '$2a$12$placeholderhash', N'Richard Thompson', N'Deploying refactoring sprint interface interface. Robust integration interface cross-platform framework analyzing test component finalizing high-performance.', 'user'),
('susan.martin49', 'susan.martin49@pronaflow.dev', '$2a$12$placeholderhash', N'Susan Martin', N'Database secure integration developing documentation scalable. Integrated cross-platform optimized finalizing analyzing framework user-friendly integration robust.', 'user'),
('peter.thompson86', 'peter.thompson86@pronaflow.dev', '$2a$12$placeholderhash', N'Peter Thompson', N'Algorithm deployment component refactoring network. Database user-friendly researching refactoring cloud-native.', 'user'),
('mary.thomas56', 'mary.thomas56@pronaflow.dev', '$2a$12$placeholderhash', N'Mary Thomas', N'Secure responsive module upgrading system finalizing. Testing module cross-platform integration automated.', 'user'),
('john.brown25', 'john.brown25@pronaflow.dev', '$2a$12$placeholderhash', N'John Brown', N'Upgrading algorithm finalizing protocol finalizing interface refactoring next-generation algorithm portal. Robust developing protocol integrating api automated.', 'user'),
('jennifer.anderson60', 'jennifer.anderson60@pronaflow.dev', '$2a$12$placeholderhash', N'Jennifer Anderson', N'System framework api build release documentation integrating integration interface component. Next-generation upgrading scalable framework sprint integrated.', 'user'),
('laura.thompson31', 'laura.thompson31@pronaflow.dev', '$2a$12$placeholderhash', N'Laura Thompson', N'Documentation developing framework network designing optimized deploying. Implementing component deploying cross-platform designing component.', 'user'),
('david.martin71', 'david.martin71@pronaflow.dev', '$2a$12$placeholderhash', N'David Martin', N'Integrating automated modular network sprint test scalable. Robust deployment portal high-performance sprint scalable integration.', 'user'),
('william.martinez69', 'william.martinez69@pronaflow.dev', '$2a$12$placeholderhash', N'William Martinez', N'Cross-platform integration test interface sprint designing integration. Module agile module user-friendly database next-generation documentation finalizing high-performance.', 'user'),
('jane.williams70', 'jane.williams70@pronaflow.dev', '$2a$12$placeholderhash', N'Jane Williams', N'Integrated protocol release framework integrated upgrading modular optimized deployment component. Finalizing analyzing integration modular database network.', 'user'),
('emily.jackson77', 'emily.jackson77@pronaflow.dev', '$2a$12$placeholderhash', N'Emily Jackson', N'Database developing agile documentation integrated build refactoring analyzing. Refactoring developing integrating optimized test implementing researching analyzing.', 'user'),
('sarah.clark48', 'sarah.clark48@pronaflow.dev', '$2a$12$placeholderhash', N'Sarah Clark', N'Automated designing analyzing integrating designing responsive api portal deploying api. Release database upgrading upgrading agile secure.', 'user'),
('chris.moore99', 'chris.moore99@pronaflow.dev', '$2a$12$placeholderhash', N'Chris Moore', N'Feature documentation module modular test integrating developing integration. Integration integration deploying scalable high-performance.', 'user'),
('patricia.anderson77', 'patricia.anderson77@pronaflow.dev', '$2a$12$placeholderhash', N'Patricia Anderson', N'Network portal agile test test. Secure framework scalable framework automated upgrading release.', 'user'),
('david.thompson82', 'david.thompson82@pronaflow.dev', '$2a$12$placeholderhash', N'David Thompson', N'Testing integrated upgrading integration integrating deploying network developing. Next-generation interface integrating system documentation system portal feature.', 'user'),
('richard.miller79', 'richard.miller79@pronaflow.dev', '$2a$12$placeholderhash', N'Richard Miller', N'Sprint implementing release automated next-generation algorithm algorithm feature. Robust release developing optimized finalizing.', 'user'),
('john.anderson85', 'john.anderson85@pronaflow.dev', '$2a$12$placeholderhash', N'John Anderson', N'Component researching robust developing refactoring user-friendly interface interface agile. Build agile deploying testing refactoring agile deploying component integrating.', 'user'),
('william.jones99', 'william.jones99@pronaflow.dev', '$2a$12$placeholderhash', N'William Jones', N'Testing testing scalable optimized cross-platform. Finalizing build responsive release upgrading release finalizing agile developing.', 'user'),
('laura.jackson33', 'laura.jackson33@pronaflow.dev', '$2a$12$placeholderhash', N'Laura Jackson', N'Database high-performance automated modular optimized designing modular. System developing optimized integrating deployment module build.', 'user'),
('jennifer.white34', 'jennifer.white34@pronaflow.dev', '$2a$12$placeholderhash', N'Jennifer White', N'Next-generation interface integrating next-generation api component test. Upgrading portal integrating optimized scalable agile.', 'user'),
('john.garcia38', 'john.garcia38@pronaflow.dev', '$2a$12$placeholderhash', N'John Garcia', N'Developing robust database deployment agile. Refactoring next-generation build sprint component documentation framework release secure designing.', 'user');
GO

SET IDENTITY_INSERT [dbo].[users] OFF;
GO