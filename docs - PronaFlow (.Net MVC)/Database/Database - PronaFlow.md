Source SQL-Server: [[CreateDBForPronaFlow]]
# I. Phân Tích Sơ Đồ Tổ Chức Dữ Liệu
## 1.  **Thực thể cốt lõi:**
- `**Users**`: 
	Người dùng của hệ thống, có thông tin cá nhân, tài khoản.
- **`Workspaces`**: 
	Không gian làm việc cấp cao nhất, chứa nhiều dự án. 
- **`Projects`**: 
	Các dự án cụ thể nằm trong một Workspace. Mỗi dự án có thành viên, giai đoạn, các thuộc tínhtính và các công việc.
- **`TaskLists` (Phases)**: 
	Các **Giai đoạn (Phase)** hoặc Nhóm Công việc logic được sử dụng để tổ chức công việc (`tasks`) trong một dự án.
	(VD: "Phase 1: Research", "Phase 2: Design").
- **`Tasks`**: 
	Các công việc cụ thể trong một dự án. Một công việc có thể có công việc con (subtask), người thực hiện, độ ưu tiên, v.v.
- **`Subtasks`**: 
	Các bước nhỏ hơn trong một Task.
- **`Tags`**: 
	Các nhãn (ví dụ: "UI/UX", "Bug") được sử dụng để phân loại dự án, có màu sắc riêng và được quản lý trong phạm vi Workspace.

## **2. Thực thể phụ trợ:**
- **`Activities` (`Notifications`)**: Ghi lại lịch sử hoạt động (ai đó đã thêm bạn vào dự án, thay đổi trạng thái công việc, nhắc đến bạn).
- **`Attachments`**: Các tệp tin được đính kèm vào dự án hoặc công việc (dựa trên nút "Attachment" trong modal).
## **3. Mối quan hệ chính:**
1. **Nhiều-Nhiều**:
`Users` <-> `Projects` (Thành viên dự án).
`Users` <-> `Tasks` (Người được giao việc).
`Projects` <-> `Tags`.
2. **Một-Nhiều**:
`Users` -> `Workspaces` (Một user có nhiều workspaceworkspace)
`Workspaces` -> `Projects` (Một workspace có nhiều dự án).
`Projects` -> `TaskLists` (Một dự án có nhiều giai đoạn/cột).
`TaskLists` -> `Tasks` (Một giai đoạn có nhiều công việc).
`Tasks` -> `Subtasks` (Một công việc có nhiều công việc con).
# II. Thiết Kế Cơ Sở Dữ Liệu Hoàn Thiện

Dưới đây là cấu trúc chi tiết cho từng bảng trong cơ sở dữ liệu, sử dụng cú pháp SQL chuẩn để mô tả.

> [!NOTE]  **Ghi Chú Kỹ Thuật Cài Đặt (Technical Implementation Notes)**
> Để đảm bảo tính toàn vẹn, hiệu suất và khả năng tương thích của cơ sở dữ liệu, tất cả các bảng được tạo ra phải tuân thủ các chỉ định kỹ thuật sau đây khi viết mã nguồn SQL `CREATE TABLE`:
>- **Storage Engine:** `ENGINE=InnoDB`
>	- **Lý do:** `InnoDB` là engine bắt buộc để hỗ trợ các ràng buộc khóa ngoại (`FOREIGN KEY`) và các giao dịch (transactions), đảm bảo tính toàn vẹn và nhất quán của dữ liệu giữa các bảng.
>- **Character Set:** `CHARSET=utf8mb4` 
>    - **Lý do:** `utf8mb4` là bộ ký tự tiêu chuẩn hỗ trợ đầy đủ Unicode, cho phép lưu trữ các ký tự đa ngôn ngữ (bao gồm tiếng Việt có dấu), biểu tượng cảm xúc (emoji) và các ký tự đặc biệt khác một cách chính xác.     
>- **Collation:** `COLLATE=utf8mb4_unicode_ci`
>    - **Lý do:** Đây là quy tắc so sánh và sắp xếp chuỗi ký tự chuẩn cho `utf8mb4`. `_ci` (case-insensitive) có nghĩa là việc so sánh chuỗi sẽ không phân biệt chữ hoa, chữ thường, phù hợp cho các chức năng tìm kiếm và xác thực dữ liệu (ví dụ: `email`, `username`).

```sql
CREATE TABLE `projects` (
  -- ... định nghĩa các cột ...
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```
## **Bảng 1: `users`**
Lưu trữ thông tin người dùng.

| Tên Cột            | Kiểu Dữ Liệu            | Ràng Buộc                                               | Ghi Chú                                                |
| :----------------- | :---------------------- | :------------------------------------------------------ | :----------------------------------------------------- |
| `id`               | `INT` / `BIGINT`        | `PRIMARY KEY`, `AUTO_INCREMENT`                         | Khóa chính định danh người dùng.                       |
| `username`         | `VARCHAR(50)`           | `NOT NULL`, `UNIQUE`                                    | Tên đăng nhập.                                         |
| `email`            | `VARCHAR(255)`          | `NOT NULL`, `UNIQUE`                                    | Email đăng nhập và liên lạc.                           |
| `password_hash`    | `VARCHAR(255)`          | `NOT NULL`                                              | Mật khẩu đã được băm.                                  |
| `full_name`        | `VARCHAR(100)`          | `NOT NULL`                                              | Tên đầy đủ hiển thị.                                   |
| `avatar_url`       | `VARCHAR(255)`          |                                                         | Đường dẫn đến ảnh đại diện.                            |
| `bio`              | `TEXT`                  |                                                         | Tiểu sử ngắn của người dùng.                           |
| `theme_preference` | `ENUM('light', 'dark')` | `DEFAULT 'light'`                                       | Lựa chọn giao diện sáng/tối.                           |
| `created_at`       | `TIMESTAMP`             | `DEFAULT CURRENT_TIMESTAMP`                             | Thời gian tạo tài khoản.                               |
| `updated_at`       | `TIMESTAMP`             | `DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP` | Thời gian cập nhật thông tin lần cuối.                 |
| `role`             | `ENUM('user','admin')`  | `DEFAULT 'user'`                                        | Phân biệt người dùng thường và quản trị viên hệ thống. |
| `is_deleted`       | `BOOLEAN`               | `DEFAULT FALSE`                                         | Hỗ trợ cho việc xóa mềm (soft-delete) tài khoản.       |
| `deleted_at`       | `TIMESTAMP`             | `NULL`                                                  | Ghi lại thời gian xóa mềm.                             |
Các nghiệp vụ (User Flow) đối với bảng `user`: [[UserFlow - Users]]
## **Bảng 2: `workspaces`**
Lưu trữ các không gian làm việc.
Mỗi `workspace` thuộc về một người duy nhất và có thể được đánh dấu là mặc định (`default`)

| Tên Cột       | Kiểu Dữ Liệu     | Ràng Buộc                                                  | Ghi Chú                                |
| :------------ | :--------------- | :--------------------------------------------------------- | :------------------------------------- |
| `id`          | `INT` / `BIGINT` | `PRIMARY KEY`, `AUTO_INCREMENT`                            | Khóa chính.                            |
| `name`        | `VARCHAR(100)`   | `NOT NULL`                                                 | Tên không gian làm việc.               |
| `owner_id`    | `INT` / `BIGINT` | `NOT NULL`, `FOREIGN KEY (users.id)`, `ON DELETE RESTRICT` | ID của người tạo và sở hữu workspace.  |
| `description` | `TEXT`           |                                                            | Mô tả workspace                        |
| `created_at`  | `TIMESTAMP`      | `DEFAULT CURRENT_TIMESTAMP`                                |                                        |
| `update_at`   | `TIMESTAMP`      | `DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP`    |                                        |

> [!NOTE] Ghi chú nghiệp vụ:
> 1. Khi tạo một `user` mới, hệ thống sẽ tự động tạo một `workspace` đi kèm với `workspaces.owner_id` là ID của user đó và đặt tên là `{user-name}'s Workpsace` mang hàm ý là Không gian làm việc đầu tiên của `user` đó.


## **Bảng 3: `projects`**
Lưu trữ thông tin các dự án.

| Tên Cột           | Kiểu Dữ Liệu                                                      | Ràng Buộc                                                       | Ghi Chú                                                                                                                      |
| :---------------- | :---------------------------------------------------------------- | :-------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------- |
| `id`              | `INT` / `BIGINT`                                                  | `PRIMARY KEY`, `AUTO_INCREMENT`                                 | Khóa chính.                                                                                                                  |
| `workspace_id`    | `INT` / `BIGINT`                                                  | `NOT NULL`, `FOREIGN KEY (workspaces.id)`, `ON DELETE RESTRICT` | Dự án này thuộc workspace nào.                                                                                               |
| `name`            | `VARCHAR(255)`                                                    | `NOT NULL`                                                      | Tên dự án.                                                                                                                   |
| `description`     | `TEXT`                                                            |                                                                 | Mô tả chi tiết dự án.                                                                                                        |
| `cover_image_url` | `VARCHAR(255)`                                                    |                                                                 | Ảnh bìa của dự án (trong modal).                                                                                             |
| `status`          | `ENUM('temp', 'not-started', 'in-progress', 'in-review', 'done')` | `DEFAULT 'temp'`                                                | Trạng thái dự án. **Cột này quyết định vị trí của thẻ Dự án trên Project Portfolio Kanban Board.**                           |
| `project_type`    | ENUM(`'personal'`, `'team'`)                                      | NOT NULL DEFAULT `'personal'`                                   | - **personal** → Dự án cá nhân, chỉ thuộc quyền sở hữu 1 user.<br>- **team** → Dự án nhóm, có thể mời nhiều thành viên khác. |
| `start_date`      | `DATE`                                                            |                                                                 | Ngày bắt đầu.                                                                                                                |
| `end_date`        | `DATE`                                                            |                                                                 | Ngày kết thúc (deadline).                                                                                                    |
| `is_archived`     | `BOOLEAN`                                                         | `DEFAULT FALSE`                                                 | `TRUE` nếu dự án đã được lưu trữ.                                                                                            |
| `is_deleted`      | `BOOLEAN`                                                         | `DEFAULT FALSE`                                                 | Dùng cho soft-delete.                                                                                                        |
| `deleted_at`      | `TIMESTAMP`                                                       |                                                                 | Thời gian xóa (để tự động xóa vĩnh viễn sau 30 ngày).                                                                        |
| `created_at`      | `TIMESTAMP`                                                       | `DEFAULT CURRENT_TIMESTAMP`                                     |                                                                                                                              |
| `updated_at`      | `TIMESTAMP`                                                       | ...                                                             |                                                                                                                              |
Các nghiệp vụ (User Flow) đối với bảng `projects`: [[UserFlow - Projects]]
## **Bảng 4: `task_lists`**
Các giai đoạn trong một dự án.

| Tên Cột      | Kiểu Dữ Liệu     | Ràng Buộc                                                     | Ghi Chú                                                             |
| :----------- | :--------------- | :------------------------------------------------------------ | :------------------------------------------------------------------ |
| `id`         | `INT` / `BIGINT` | `PRIMARY KEY`, `AUTO_INCREMENT`                               | Khóa chính.                                                         |
| `project_id` | `INT` / `BIGINT` | `NOT NULL`, `FOREIGN KEY (projects.id)`, `ON DELETE RESTRICT` | Thuộc dự án nào.                                                    |
| `name`       | `VARCHAR(100)`   | `NOT NULL`                                                    | Tên giai đoạn/nhóm công việc                                        |
| `position`   | `INT`            | `NOT NULL`                                                    | Thứ tự hiển thị của giai đoạn/nhóm công việc (`tasks`) trong dự án. |
| Constraint   |                  | `UNIQUE(project_id, name)`                                    | Đảm bảo tên giai đoạn là duy nhất trong một dự án.                  |
## **Bảng 5: `tasks`**
Lưu trữ các công việc.

|       Tên Cột        |                       Kiểu Dữ Liệu                        |                           Ràng Buộc                           |                       Ghi Chú                        |
| :------------------: | :-------------------------------------------------------: | :-----------------------------------------------------------: | :--------------------------------------------------: |
|         `id`         |                     `INT` / `BIGINT`                      |                `PRIMARY KEY`, `AUTO_INCREMENT`                |                     Khóa chính.                      |
|     `project_id`     |                     `INT` / `BIGINT`                      | `NOT NULL`, `FOREIGN KEY (projects.id)`, `ON DELETE RESTRICT` |              Công việc thuộc dự án nào.              |
|    `task_list_id`    |                     `INT` / `BIGINT`                      |      `FOREIGN KEY (task_lists.id)`, `ON DELETE RESTRICT`      |            Công việc thuộc giai đoạn nào.            |
|        `name`        |                      `VARCHAR(255)`                       |                          `NOT NULL`                           |                    Tên công việc.                    |
|    `description`     |                          `TEXT`                           |                                                               |                   Mô tả chi tiết.                    |
|      `priority`      |              `ENUM('low', 'normal', 'high')`              |                      `DEFAULT 'normal'`                       |                     Độ ưu tiên.                      |
|       `status`       | `ENUM('not-started', 'in-progress', 'in-review', 'done')` |                    `DEFAULT 'not-started'`                    |                     Trạng thái.                      |
|     `start_date`     |                          `DATE`                           |                                                               |                    Ngày bắt đầu.                     |
|      `end_date`      |                        `TIMESTAMP`                        |                                                               |          Hạn chót (có thể bao gồm cả giờ).           |
|     `is_deleted`     |                         `BOOLEAN`                         |                        `DEFAULT FALSE`                        |                Dùng cho soft-delete.                 |
|     `deleted_at`     |                        `TIMESTAMP`                        |                                                               |                    Thời gian xóa.                    |
|     `created_at`     |                        `TIMESTAMP`                        |                  `DEFAULT CURRENT_TIMESTAMP`                  |                                                      |
|     `updated_at`     |                        `TIMESTAMP`                        |                              ...                              |                                                      |
|     `creator_id`     |                      `INT`/`BIGINT`                       |                         `FK(users.id`                         |                 ai là người tạo task                 |
|     is_recurirng     |                          BOOLEAN                          |                         DEFAULT FALSE                         |     Đánh dấu đây có phải là task lặp lại không.      |
|   recurrence_rule    |                       VARCHAR(255)                        |                             NULL                              | Lưu quy tắc lặp lại (ví dụ: `FREQ=WEEKLY;BYDAY=MO`). |
| next_recurrence_date |                           DATE                            |                             NULL                              |     Ngày mà task lặp lại tiếp theo sẽ được tạo.      |

> [!NOTE] Phân tích nghiệp vụ `tasks`
> **Vòng đời của một Task:** Một `task` bắt đầu ở trạng thái `not-started`. Khi bắt đầu làm, nó chuyển sang `in-progress`. Sau khi hoàn thành, nó có thể được chuyển sang `in-review` để chờ người khác xác nhận, hoặc chuyển thẳng sang `done` nếu không cần xét duyệt.

Nghiệp vụ (User flow) đối với bảng `tasks`: [[UserFlow - Task-list]]
## **Bảng 6: `subtasks`**
Các công việc con.

| Tên Cột        | Kiểu Dữ Liệu     | Ràng Buộc                                                  | Ghi Chú                  |
| :------------- | :--------------- | :--------------------------------------------------------- | :----------------------- |
| `id`           | `INT` / `BIGINT` | `PRIMARY KEY`, `AUTO_INCREMENT`                            | Khóa chính.              |
| `task_id`      | `INT` / `BIGINT` | `NOT NULL`, `FOREIGN KEY (tasks.id)`, `ON DELETE RESTRICT` | Thuộc công việc cha nào. |
| `name`         | `VARCHAR(255)`   | `NOT NULL`                                                 | Tên công việc con.       |
| `is_completed` | `BOOLEAN`        | `DEFAULT FALSE`                                            |                          |
| `position`     | `INT`            | `NOT NULL`                                                 | Thứ tự hiển thị.         |
## **Bảng 7: `tags`**
Các nhãn được quản lý theo Workspace.

| Tên Cột        | Kiểu Dữ Liệu     | Ràng Buộc                                                       | Ghi Chú                                         |
| :------------- | :--------------- | :-------------------------------------------------------------- | :---------------------------------------------- |
| `id`           | `INT` / `BIGINT` | `PRIMARY KEY`, `AUTO_INCREMENT`                                 | Khóa chính.                                     |
| `workspace_id` | `INT` / `BIGINT` | `NOT NULL`, `FOREIGN KEY (workspaces.id)`, `ON DELETE RESTRICT` | Tag này thuộc workspace nào.                    |
| `name`         | `VARCHAR(50)`    | `NOT NULL`                                                      | Tên nhãn (UI/UX, Bug,...).                      |
| `color_hex`    | `VARCHAR(7)`     | `NOT NULL`                                                      | Mã màu HEX (VD: tag80c8ff).                     |
| Constraint     |                  | `UNIQUE(workspace_id, name`                                     | Đảm bảo tên tag là duy nhất trong một workspace |
## **Bảng 8: `activities`**
Lưu trữ lịch sử hoạt động và thông báo.

| Tên Cột       | Kiểu Dữ Liệu              | Ràng Buộc                            | Ghi Chú                                       |
| :------------ | :------------------------ | :----------------------------------- | :-------------------------------------------- |
| `id`          | `INT` / `BIGINT`          | `PRIMARY KEY`, `AUTO_INCREMENT`      | Khóa chính.                                   |
| `user_id`     | `INT` / `BIGINT`          | `NOT NULL`, `FOREIGN KEY (users.id)` | Người thực hiện hành động.                    |
| `action_type` | `ENUM(...)`               | `NOT NULL`                           |                                               |
| `content`     | `NVARCHAR(MAX)`           |                                      |                                               |
| `target_id`   | `INT` / `BIGINT`          | `NOT NULL`                           | ID của đối tượng bị tác động (project, task). |
| `target_type` | `ENUM('project', 'task')` | `NOT NULL`                           | Loại đối tượng bị tác động.                   |
| `created_at`  | `TIMESTAMP`               | `DEFAULT CURRENT_TIMESTAMP`          |                                               |
Danh sách chi tiết của `action_type`: [[UserFlow - Activities]]

Mô tả kỹ hơn về cách hoạt độn của bảng `activities` [[Flow Activities-table]]
## **Bảng 11**: `comments`
`activities` có `action_type = 'comment_add`.

| **Tên Cột**        | **Kiểu Dữ Liệu** | **Ràng Buộc**  | **Ghi Chú**                           |
| ------------------ | ---------------- | -------------- | ------------------------------------- |
| `id`               | `INT` / `BIGINT` | PRIMARY KEY    |                                       |
| `user_id`          | `INT` / `BIGINT` | `FK(users.id)` | Ai là người bình luận.                |
| `content`          | `TEXT`           | `NOT NULL`     | Nội dung bình luận.                   |
| `commentable_id`   | `INT` / `BIGINT` | `NOT NULL`     | `ID` của project/task được bình luận. |
| `commentable_type` | `ENUM(...)`      | `NOT NULL`     | `'project'`, `'task'`                 |
| `created_at`       | `TIMESTAMP`      |                |                                       |
Các nghiệp vụ (User Flow) đối với bảng `comments`: [[UserFlow - comments]]
## **Bảng 9**: `attachments`

| Tên Cột               | Kiểu Dữ Liệu              | Ràng Buộc                            | Ghi Chú                                                                       |
| :-------------------- | :------------------------ | :----------------------------------- | :---------------------------------------------------------------------------- |
| `id`                  | `INT` / `BIGINT`          | `PRIMARY KEY`, `AUTO_INCREMENT`      | Khóa chính của tệp đính kèm.                                                  |
| `uploaded_by_user_id` | `INT` / `BIGINT`          | `NOT NULL`, `FOREIGN KEY (users.id)` | ID của người dùng đã tải tệp lên.                                             |
| **`attachable_id`**   | `INT` / `BIGINT`          | `NOT NULL`                           | **ID của đối tượng được đính kèm (VD: `project_id=5` hoặc `task_id=101`).**   |
| **`attachable_type`** | `ENUM('project', 'task')` | `NOT NULL`                           | **Loại đối tượng được đính kèm. Cho biết `attachable_id` thuộc về bảng nào.** |
| `original_filename`   | `VARCHAR(255)`            | `NOT NULL`                           | Tên tệp gốc mà người dùng tải lên.                                            |
| `storage_path`        | `VARCHAR(512)`            | `NOT NULL`                           | Đường dẫn/URL đến tệp đã được lưu trên server hoặc cloud (VD: S3).            |
| `file_type`           | `VARCHAR(100)`            |                                      | Kiểu MIME của tệp (VD: 'image/jpeg', 'application/pdf').                      |
| `file_size`           | `BIGINT`                  |                                      | Kích thước tệp (tính bằng byte).                                              |
| `created_at`          | `TIMESTAMP`               | `DEFAULT CURRENT_TIMESTAMP`          |                                                                               |
**Giải thích về Mối quan hệ Đa hình (`attachable_id` và `attachable_type`):**
Cặp đôi này cho phép một tệp đính kèm có thể thuộc về nhiều loại đối tượng khác nhau mà không cần tạo nhiều cột khóa ngoại.
*   Nếu một tệp được đính kèm vào dự án có `id = 5`, thì:
    *   `attachable_id` = 5
    *   `attachable_type` = `'project'`
*   Nếu một tệp được đính kèm vào công việc có `id = 101`, thì:
    *   `attachable_id` = 101
    *   `attachable_type` = `'task'`
*   Thiết kế này cực kỳ linh hoạt để sau này bạn có thể cho phép đính kèm tệp vào cả bình luận (`comment`) mà không cần sửa đổi cấu trúc bảng.
Mô tả User Flow đối với bảng `attachments`: [[UserFlow - Attachments]]
## **Bảng 10**: `invitations`
Hệ thống Lời mời.
Giải quyết vấn đề: User A có thể mời một người khác (User B) vào một project-team.

| Tên Cột         | Kiểu Dữ Liệu                                         | Ràng Buộc         | Ghi Chú                                                                           |
| --------------- | ---------------------------------------------------- | ----------------- | --------------------------------------------------------------------------------- |
| `id`            | `INT` / `BIGINT`                                     | `PRIMARY KEY`     |                                                                                   |
| `project_id`    | `INT` / `BIGINT`                                     | `FK(projects.id)` | Lời mời cho dự án nào.                                                            |
| `inviter_id`    | `INT` / `BIGINT`                                     | `FK(users.id)`    | Ai là người gửi lời mời.                                                          |
| `invitee_email` | `VARCHAR(255)`                                       | `NOT NULL`        | Email của người được mời.                                                         |
| `token`         | `VARCHAR(255)`                                       | `UNIQUE`          | Một chuỗi mã hóa duy nhất cho lời mời này.                                        |
| `status`        | `ENUM('pending', 'accepted', 'declined', 'expired')` | `'pending'`       | Trạng thái: <br>`'pending'`:<br>`'accepted'`:<br>`'declined'`: <br>`'expired'`: . |
| `expires_at`    | `TIMESTAMP`                                          |                   | Lời mời có thể hết hạn sau một khoảng thời gian.                                  |

> [!NOTE] Luồng hoạt đồng đối với bảng `invitations`
> 1. User A nhập email của User B vào modal `Add Member` trong project-detail.
> 2. Backend tạo một record trong table: `invitations` với `status = 'pending'`, tạo một `token` duy nhất, và gửi 1 email đến `invitee_email` chứa một link có token đó (vd: `pronaflow.com/invite?token=xyz)`.
> **3. Case: *`User B`*?**
> 	**1. Case 1** - User B đã có tài khoản
> 		User B nhận được thông báo trong ứng dụng (PronaFlow.Notifications). 
> 			Khi chấp nhận (`accept`), hệ thống sẽ đổi `status` thành `accepted` và thêm User B vào bảng `project_members`.
> 	**2. Case 2** - User B chưa có tài khoản
> 		User B nhấp vào link trong email, được dấn đến trang đăng ký.
> 		Sau khi đăng ký thành công, hệ thống sẽ tự động chấp nhận lời mời và thêm họ vào dựa án.

Các nghiệp vụ (User Flow) đối với bảng `invitations`: [[UserFlow - Invitations]]
## **Bảng 12**: `user_preferences`
Tùy chỉnh `preferences` của `1 account` mà không cần tùy chỉnh bảng `users`

| **Tên Cột**     | **Kiểu Dữ Liệu** | **Ràng Buộc**                     | **Ghi Chú**                         |
| --------------- | ---------------- | --------------------------------- | ----------------------------------- |
| `user_id`       | `INT` / `BIGINT` | `FK(users.id)`                    |                                     |
| `setting_key`   | `VARCHAR(100)`   | `NOT NULL`                        | VD: 'notification_email_on_mention' |
| `setting_value` | `VARCHAR(255)`   | `NOT NULL`                        | VD: 'true', 'false', 'daily_digest' |
|                 |                  | PRIMARY KEY(user_id, setting_key) |                                     |
Các nghiệp vụ (User Flow) đối với bảng `user_preferences`: [[UserFlow - user_preferences]]
## **Bảng 13**: `password_resets`
Xử lý cho luồng "Forgot password" trong phần "Quản lý Phiên Đăng nhập và Xác thực".
Đây là bảng lưu trữ `token` reset mật khẩu.

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Ghi Chú |
| :----------- | :--------------- | :---------------------------------- | :------------------------------------------ |
| email | VARCHAR(255) | NOT NULL, FOREIGN KEY (users.email) | Email của người dùng yêu cầu reset. |
| token | VARCHAR(255) | PRIMARY KEY, UNIQUE | Mã token duy nhất, được gửi qua email. |
| expires_at | TIMESTAMP | NOT NULL | Thời gian token hết hạn (ví dụ: 1 giờ sau). |
| created_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Thời gian tạo yêu cầu reset. |

## **Bảng 14: Junction Tables `project_members`**
*   **`project_members`**:
    *   `project_id`: FOREIGN KEY (projects.id)
    *   `user_id`: FOREIGN KEY (users.id)
    *   `role`: ENUM('admin', 'member')
    *   *PRIMARY KEY (project_id, user_id)*
## **Bảng 15: Junction Tables `task_assignees`**
*   **`task_assignees`**:
    *   `task_id`: FOREIGN KEY (tasks.id)
    *   `user_id`: FOREIGN KEY (users.id)
    *   *PRIMARY KEY (task_id, user_id)*
## **Bảng 16: Junction Tables `project_tags`**
*   **`project_tags`**:
    *   `project_id`: FOREIGN KEY (projects.id)
    *   `tag_id`: FOREIGN KEY (tags.id)
    *   *PRIMARY KEY (project_id, tag_id)*
## **Bảng 17: Junction Tables `notification_recipients`**

| Tên Cột       | Kiểu Dữ Liệu     | Ràng Buộc                           | Ghi Chú                                  |
| ------------- | ---------------- | ----------------------------------- | ---------------------------------------- |
| `activity_id` | `INT` / `BIGINT` | `FK(activities.id)`                 | Thông báo này thuộc về hoạt động nào.    |
| `user_id`     | `INT` / `BIGINT` | `FK(users.id)`                      | Ai là người nhận thông báo.              |
| `is_read`     | `BOOLEAN`        | `DEFAULT FALSE`                     | Đánh dấu thông báo đã được đọc hay chưa. |
|               |                  | `PRIMARY KEY(activity_id, user_id)` |                                          |
## Bảng 18: `task_dependencies`
Hỗ trợ nghiệp vụ "Sự Phụ Thuộc Giữa Các Công Việc (Task Dependencies)", cho phép thiết lập rằng "Công việc B" chỉ có thể bắt đầu sau khi "Công việc A" hoàn thành.

| Tên Cột            | Kiểu Dữ Liệu     | Ràng Buộc      | Ghi Chú                                  |
| ------------------ | ---------------- | -------------- | ---------------------------------------- |
| `task_id`          | `INT` / `BIGINT` | `FK(tasks.id)` | Công việc bị chặn.                       |
| `blocking_task_id` | `INT` / `BIGINT` | `FK(tasks.id)` | Công việc phải hoàn thành trước.         |
|                    |                  |                | `PRIMARY KEY(task_id, blocking_task_id)` |
