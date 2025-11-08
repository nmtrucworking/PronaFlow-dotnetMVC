Cột `action_type` là cực kỳ quan trọng, vì đây chính là "bộ não" điều khiển hai tính năng cốt lõi: **Dòng Hoạt Động (Activity Feed)** và **Hệ Thống Thông Báo (Notifications)**.
## I. Mục Đích và Vai Trò Của Cột `action_type`
- Cột `action_type` là "trái tim" của bảng `activities`. Nó đóng vai trò như một **"động từ"** ghi lại chính xác **hành động gì đã xảy ra** trong hệ thống. Mỗi khi một người dùng thực hiện một thao tác quan trọng, một bản ghi mới sẽ được tạo trong bảng `activities` và cột `action_type` sẽ cho chúng ta biết đó là hành động gì.
***Nó phục vụ hai mục đích chính:***
1.  **Xây dựng Dòng Hoạt Động (`Activity Feed`):** Trên trang chi tiết dự án, bạn có một mục "Activity". Bằng cách truy vấn các bản ghi `activities` liên quan đến dự án đó, bạn có thể hiển thị một lịch sử đầy đủ:
    *   "**Trần Văn An** *đã tạo công việc* 'Design wireframe...'" (`action_type = 'task_create'`)
    *   "**Nguyễn Minh Trúc** *đã di chuyển thẻ này từ Not Started sang In Progress*'" (`action_type = 'project_update_status'`)
2.  **Tạo Thông Báo (`Notifications`):** Dựa vào `action_type`, hệ thống biết được cần phải gửi thông báo cho ai.
    *   Nếu `action_type` là `'task_assign'`, hệ thống sẽ tạo thông báo cho người được giao việc.
    *   Nếu `action_type` là `'user_mention'`, hệ thống sẽ phân tích nội dung để tìm người bị nhắc đến và gửi thông báo cho họ.
## III. Danh Sách Chi Tiết Các `action_type` và Liên Kết Với Giao Diện
Dưới đây là danh sách các giá trị `ENUM` khả thi cho `action_type`, được phân loại và mô tả.

|           Giá trị `ENUM`           |                Hành Động Của Người Dùng                 |                                                  Dữ Liệu Liên Quan                                                  |                                                                Ghi Chú                                                                 |
| :--------------------------------: | :-----------------------------------------------------: | :-----------------------------------------------------------------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------------------------------: |
| **HÀNH ĐỘNG VỚI DỰ ÁN (PROJECT)**  |                                                         |                                                                                                                     |                                                                                                                                        |
|          `project_create`          |                   Tạo một dự án mới.                    |                              `target_type`: 'project', `target_id`: ID của dự án mới.                               |                                                                                                                                        |
|      `project_update_status`       | Thay đổi trạng thái dự án (Not Started -> In Progress). |             `target_type`: 'project', `target_id`: ID dự án. `content` có thể lưu trạng thái cũ và mới.             | Kích hoạt khi **kéo-thả thẻ Project** trên Project Portfolio Kanban hoặc đổi trong modal. Hành động này **cập nhật `projects.status`** |
|      `project_update_details`      |       Chỉnh sửa tên, mô tả, ngày tháng của dự án.       |              `target_type`: 'project', `target_id`: ID dự án. `content` có thể lưu chi tiết thay đổi.               |                                                                                                                                        |
|        `project_add_member`        |             Thêm một thành viên vào dự án.              |          `target_type`: 'project', `target_id`: ID dự án. `content` nên chứa ID của thành viên được thêm.           |                                          Rất quan trọng để gửi thông báo cho người được thêm.                                          |
|      `project_remove_member`       |             Xóa một thành viên khỏi dự án.              |              `target_type`: 'project', `target_id`: ID dự án. `content` chứa ID của thành viên bị xóa.              |                                                    Gửi thông báo cho người bị xóa.                                                     |
|         `project_archive`          |                   Lưu trữ một dự án.                    |                                  `target_type`: 'project', `target_id`: ID dự án.                                   |                                                                                                                                        |
|         `project_restore`          |           Khôi phục một dự án từ kho lưu trữ.           |                                  `target_type`: 'project', `target_id`: ID dự án.                                   |                                                                                                                                        |
|          `project_delete`          |           Xóa (mềm) một dự án vào thùng rác.            |                                  `target_type`: 'project', `target_id`: ID dự án.                                   |                                                                                                                                        |
| **HÀNH ĐỘNG VỚI CÔNG VIỆC (TASK)** |                                                         |                                                                                                                     |                                                                                                                                        |
|           `task_create`            |                 Tạo một công việc mới.                  |                                `target_type`: 'task', `target_id`: ID công việc mới.                                |                                                                                                                                        |
|        `task_update_status`        |    Thay đổi trạng thái công việc (checkbox, select).    |                `target_type`: 'task', `target_id`: ID công việc. `content` lưu trạng thái cũ và mới.                |                                                                                                                                        |
|           `task_assign`            |              Giao việc cho một thành viên.              |              `target_type`: 'task', `target_id`: ID công việc. `content` chứa ID của người được giao.               |                                                   Gửi thông báo cho người được giao.                                                   |
|       `task_change_priority`       |           Thay đổi độ ưu tiên của công việc.            |                `target_type`: 'task', `target_id`: ID công việc. `content` lưu độ ưu tiên cũ và mới.                |                                                                                                                                        |
|        `task_set_deadline`         |        Đặt hoặc thay đổi hạn chót cho công việc.        |                   `target_type`: 'task', `target_id`: ID công việc. `content` lưu ngày tháng mới.                   |                                                Gửi thông báo nhắc nhở khi gần đến hạn.                                                 |
|           `task_delete`            |         Xóa (mềm) một công việc vào thùng rác.          |                                  `target_type`: 'task', `target_id`: ID công việc.                                  |                                                                                                                                        |
|      **HÀNH ĐỘNG TƯƠNG TÁC**       |                                                         |                                                                                                                     |                                                                                                                                        |
|           `user_mention`           |            Nhắc đến (@) một người dùng khác.            | `target_type`: 'project' hoặc 'task', `target_id`: ID của nơi mention. `content`: toàn bộ nội dung có chứa mention. |                                                  Gửi thông báo cho người bị nhắc đến.                                                  |
|           `comment_add`            |        Thêm một bình luận (tính năng tương lai).        |       `target_type`: 'project' hoặc 'task', `target_id`: ID của nơi bình luận. `content`: nội dung bình luận.       |                                                Gửi thông báo cho những người theo dõi.                                                 |
|       **HÀNH ĐỘNG HỆ THỐNG**       |                                                         |                                                                                                                     |                                                                                                                                        |
|     `system_deadline_reminder`     |        Hệ thống tự động gửi thông báo nhắc nhở.         |                                  `target_type`: 'task', `target_id`: ID công việc.                                  |                                                 Không do người dùng trực tiếp gây ra.                                                  |
|       `system_task_overdue`        |         Hệ thống đánh dấu công việc đã quá hạn.         |                                  `target_type`: 'task', `target_id`: ID công việc.                                  |                                                Gửi thông báo cho người được giao việc.                                                 |
|                                    |                                                         |                                                                                                                     |                                                                                                                                        |
|          `attachment_add`          |                  Đính kèm một tệp mới                   |       `target_type`: `project` hoặc `task`<br>`target_id`: ID của nơi đính kèm.<br>`content`: có thể chứa tệp       |                                                 Gửi thông báo cho các thành viên dự án                                                 |
|        `attachment_remove`         |                  Xóa một tệp đính kèm                   |        `target_type`: `project` hoặc `task`<br>`target_id`: ID của nơi xóa<br>`content`: có thể chứa tên tệp        |                                                                                                                                        |

```sql
ENUM(
    -- Hành động với Dự án (Project)
    'project_create',
    'project_update_status',
    'project_update_details',
    'project_add_member',
    'project_remove_member',
    'project_archive',
    'project_restore',
    'project_delete',
    'project_move_workspace',
    'project_duplicate',

    -- Hành động với Giai đoạn (TaskList/Phase)
    'tasklist_create',
    'tasklist_rename',
    'tasklist_reorder',
    'tasklist_delete',

    -- Hành động với Công việc (Task)
    'task_create',
    'task_update_status',
    'task_assign',
    'task_change_priority',
    'task_set_deadline',
    'task_delete',
    'task_restore',
    'task_move_tasklist',

    -- Hành động với Công việc con (Subtask)
    'subtask_create',
    'subtask_update',
    'subtask_complete',
    'subtask_delete',

    -- Hành động Tương tác
    'user_mention',
    'comment_add',
    'attachment_add',
    'attachment_remove',

    -- Hành động của Hệ thống
    'system_deadline_reminder',
    'system_task_overdue'
)
```
## IV. Ví Dụ Về Luồng Hoạt Động Cụ Thể
##### Ví dụ 1: Giao việc
1.  **Hành động:** Người dùng **Nguyễn Minh Trúc (user_id: 1)** giao công việc **"Lập trình Sidebar Component" (task_id: 101)** cho **Trần Văn An (user_id: 2)**.
2.  **Tạo Activity:** Hệ thống tạo một bản ghi trong bảng `activities`:
    *   `id`: (tự tăng)
    *   `user_id`: 1 (Người thực hiện hành động)
    *   `action_type`: `'task_assign'`
    *   `content`: '{"assignee_id": 2}' (Lưu dưới dạng JSON để dễ xử lý)
    *   `target_id`: 101
    *   `target_type`: `'task'`
3.  **Tạo Notification:** Dựa vào `action_type` và `content`, hệ thống biết rằng cần thông báo cho `user_id = 2`. Nó tạo một bản ghi trong `notification_recipients`:
    *   `activity_id`: (ID của bản ghi vừa tạo ở trên)
    *   `user_id`: 2 (Người nhận thông báo)
    *   `is_read`: `FALSE`
##### Ví dụ 2: Mention
1.  **Hành động:** **Trần Văn An (user_id: 2)** thêm mô tả vào dự án **"Website Redesign" (project_id: 5)** với nội dung: *"@minhtruc (user_id: 1) vui lòng xem lại thiết kế này."*
2.  **Tạo Activity:**
    *   `id`: (tự tăng)
    *   `user_id`: 2
    *   `action_type`: `'user_mention'`
    *   `content`: "@minhtruc (user_id: 1) vui lòng xem lại thiết kế này."
    *   `target_id`: 5
    *   `target_type`: `'project'`
3.  **Tạo Notification:** Backend sẽ phân tích chuỗi `content`, nhận dạng `@minhtruc` tương ứng với `user_id = 1` và tạo một bản ghi trong `notification_recipients` cho người dùng này.