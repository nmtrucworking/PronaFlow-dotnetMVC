Mô tả chi tiết các nghiệp vụ chính liên quan đến bảng `tasks`.
# 1. Nghiệp vụ Tạo Công Việc Mới (Create Task)
Cho phép người dùng tạo một công việc mới trong một dự án và giai đoạn cụ thể.
## **Frontend:**
1.  **Giao diện:** Trên trang chi tiết `project` (Kanban Board hoặc List View), người dùng click vào nút "Add Task" hoặc nhập trực tiếp vào ô tạo task.
2.  **Input:**
    *   `name`: Tên công việc (bắt buộc).
    *   `description`: Mô tả chi tiết (tùy chọn).
    *   `project_id`: ID của dự án hiện tại (được lấy từ ngữ cảnh URL hoặc trạng thái).
    *   `task_list_id`: ID của giai đoạn/cột mà task sẽ thuộc về (được lấy từ ngữ cảnh UI).
    *   `priority`: (Mặc định 'normal', có thể chọn 'low', 'high').
    *   `assignee_ids`: Một hoặc nhiều `user_id` để giao việc (tùy chọn).
    *   `start_date`, `end_date`: Ngày bắt đầu, ngày kết thúc/deadline (tùy chọn).
    *   `is_recurring`, `recurrence_rule`: Nếu là công việc lặp lại (tùy chọn).
3.  **Action:** Nhấn "Create" hoặc "Enter".

## **Backend:**
> ***API Endpoint***: `POST /api/projects/{project_id}/tasks`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` hiện tại đã đăng nhập và có quyền `member` hoặc `admin` trong `project` (`project_members`).
    *   Nếu `project` có `is_deleted = TRUE` hoặc `is_archived = TRUE`, không cho phép tạo task.
2.  **Validation:**
    *   `name` không được trống.
    *   `project_id` và `task_list_id` phải tồn tại và thuộc về cùng một dự án.
    *   `start_date` không được sau `end_date`.
    *   Kiểm tra `assignee_ids` có tồn tại và thuộc `project_members` không.
3.  **Lưu vào Database (`tasks`):**
    *   Tạo một bản ghi mới trong bảng `tasks` với các thông tin đã nhập.
    *   `status`: Mặc định `'not-started'`.
    *   `is_completed`: Mặc định `FALSE`.
    *   `created_at`: `CURRENT_TIMESTAMP`.
    *   `updated_at`: `CURRENT_TIMESTAMP`.
4.  **Lưu vào Database (`task_assignees`):**
    *   Nếu có `assignee_ids`, tạo các bản ghi tương ứng trong bảng `task_assignees` cho mỗi người được giao.
5.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'task_create'` do `user_id` hiện tại thực hiện. `target_type = 'task'`, `target_id = task_id` vừa tạo. `content` có thể chứa tên task.
6.  **Gửi Thông Báo (`notification_recipients`):**
    *   Nếu có người được giao việc (`task_assignees`), gửi thông báo cho họ (dựa trên activity `task_assign` được tạo sau đó hoặc kèm theo `task_create`).
7.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về #201-Created cùng với thông tin task mới tạo.
    *   Nếu thất bại: Trả về #4xx với thông báo lỗi.
# 2. Nghiệp vụ Xem Chi Tiết Công Việc (View Task Details)
Cho phép người dùng xem tất cả thông tin liên quan đến một công việc.

## **Frontend:**
1.  **Giao diện:** Người dùng click vào một task trên Kanban board hoặc danh sách.
2.  **Action:** Mở một modal hoặc chuyển đến một trang chi tiết task.

## **Backend:**
> ***API Endpoint***: `GET /api/tasks/{task_id}`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` đã đăng nhập và có quyền xem `project` chứa `task` này (tức là user là `member` của project).
2.  **Truy vấn Database:**
    *   Lấy thông tin `task` từ bảng `tasks` bằng `task_id`.
    *   Lấy danh sách `assignees` từ `task_assignees` (JOIN với `users` để lấy tên, avatar).
    *   Lấy danh sách `subtasks` từ `subtasks`.
    *   Lấy danh sách `attachments` từ `attachments` (`attachable_id = task_id`, `attachable_type = 'task'`).
    *   Lấy danh sách `comments` từ `comments` (`commentable_id = task_id`, `commentable_type = 'task'`).
    *   Lấy `activities` liên quan đến `task_id` từ bảng `activities`.
3.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về `200 OK` cùng với tất cả dữ liệu chi tiết của task.
    *   Nếu thất bại: Trả về `404 Not Found` (task không tồn tại hoặc user không có quyền).
# 3. Nghiệp vụ Cập Nhật Công Việc (Update Task)
Cho phép người dùng sửa đổi các thuộc tính của một công việc.
## **Frontend:**
1.  **Giao diện:** Trong modal chi tiết task, người dùng chỉnh sửa `name`, `description`, `priority`, `start_date`, `end_date`, thêm/bớt `assignees`, thêm/bớt `tags`.
2.  **Action:** Nhấn "Save" hoặc tự động lưu khi thay đổi.
## **Backend:**
> ***API Endpoint***: `PUT /api/tasks/{task_id}`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` đã đăng nhập và có quyền chỉnh sửa `task` (là `member` hoặc `admin` của project, hoặc là người tạo task/người được giao task).
2.  **Validation:** Kiểm tra dữ liệu mới có hợp lệ không.
3.  **Truy vấn & So sánh dữ liệu cũ-mới:**
    *   Lấy bản ghi `task` hiện tại từ CSDL để so sánh các thay đổi.
4.  **Cập nhật Database:**
    *   Cập nhật các cột trong bảng `tasks` (`name`, `description`, `priority`, `start_date`, `end_date`, `recurrence_rule`, `next_recurrence_date`).
    *   Cập nhật `updated_at`.
    *   **Đối với `assignees`:**
        *   So sánh danh sách `assignee_ids` mới với cũ.
        *   Xóa các bản ghi trong `task_assignees` không còn nằm trong danh sách mới.
        *   Thêm các bản ghi mới vào `task_assignees` nếu chưa tồn tại.
    *   **Đối với `tags`:** (Nếu bạn có `task_tags` hoặc quyết định chỉ gắn tags cho project)
        *   Tương tự như `assignees`, quản lý các bản ghi trong `task_tags`.
5.  **Ghi Hoạt Động (`activities`):**
    *   Nếu có bất kỳ thay đổi quan trọng nào (`name`, `description`, `priority`, `assignees`, `deadline`):
        *   Tạo bản ghi trong `activities` với `action_type` phù hợp (ví dụ: `'task_update_details'`, `'task_assign'`, `'task_change_priority'`, `'task_set_deadline'`).
        *   `content` có thể lưu thông tin chi tiết thay đổi (giá trị cũ/mới, ID người bị gán/hủy gán).
6.  **Gửi Thông Báo (`notification_recipients`):**
    *   Gửi thông báo cho những người liên quan đến thay đổi (người được giao/hủy giao việc, người tạo task nếu có thay đổi lớn).
7.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về `200 OK` cùng với thông tin task đã cập nhật.
    *   Nếu thất bại: Trả về `4xx` với thông báo lỗi.
# 4. Nghiệp vụ Thay Đổi Trạng Thái Hoàn Thành Công Việc (Toggle Task Completion)
Cho phép người dùng đánh dấu công việc là hoàn thành hoặc chưa hoàn thành thông qua checkbox.
## **Frontend:**
1.  **Giao diện:** Checkbox "Hoàn thành" trong `task-card` hoặc modal chi tiết.
2.  **Input:** Trạng thái `is_completed` (`TRUE`/`FALSE`).
3.  **Action:** Click vào checkbox.
## **Backend:**
> ***API Endpoint***: `PUT /api/tasks/{task_id}/complete`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` có quyền thay đổi trạng thái hoàn thành của `task` (là `member` hoặc `admin` của project, hoặc là người được giao task).
2.  **Validation:**
    *   Kiểm tra `task_id` tồn tại.
    *   **Nghiệp vụ "Task Dependencies":** Nếu `is_completed` được set thành `TRUE`, hệ thống có thể cần kiểm tra nếu `task` này là `blocking_task_id` cho các task khác và thông báo cho assignees của các task đó.
3.  **Cập nhật Database:**
    *   Lấy bản ghi `task` hiện tại để có `old_is_completed_status`.
    *   **Cập nhật `status` trong bảng `tasks`:**
        *   Nếu `is_completed` chuyển thành `TRUE`, set `status = 'done'`.
        *   Nếu `is_completed` chuyển thành `FALSE`, giữ nguyên `status` ở trạng thái cũ.
    *   Cập nhật `updated_at`.
    *   **Nghiệp vụ "Recurring Tasks":** Nếu `is_recurring = TRUE` và task vừa chuyển sang `is_completed = TRUE`, tính toán và cập nhật `next_recurrence_date` để tạo task lặp lại tiếp theo.
4.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'task_update_status'`.
    *   `content` có thể lưu `old_status` và `new_status` (hoặc `old_is_completed` và `new_is_completed`).
5.  **Gửi Thông Báo (`notification_recipients`):**
    *   Gửi thông báo cho `assignees` hoặc `creator` của task về sự thay đổi trạng thái hoàn thành.
6.  **Phản hồi (Response):):**
    *   Nếu thành công: Trả về #200-OK cùng với thông tin task đã cập nhật.
    *   Nếu thất bại: Trả về #4xx với thông báo lỗi.
# 5. Nghiệp vụ Thay Đổi Độ Ưu Tiên Công Việc (Change Task Priority)
Cho phép người dùng điều chỉnh mức độ ưu tiên của một công việc.
## **Frontend:**
1.  **Giao diện:** Nút/biểu tượng độ ưu tiên (`priority-high`, `priority-normal`, `priority-low`) trên `task-card` hoặc trong modal chi tiết.
2.  **Input:** `new_priority` (`'low'`, `'normal'`, `'high'`).
3.  **Action:** Click vào nút độ ưu tiên hoặc chọn từ dropdown.
## **Backend:**
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` có quyền thay đổi độ ưu tiên của `task` (là `member` hoặc `admin` của project, hoặc là người được giao task).
2.  **Validation:**
    *   Kiểm tra `task_id` tồn tại.
    *   Kiểm tra `new_priority` hợp lệ.
3.  **Cập nhật Database:**
    *   Lấy bản ghi `task` hiện tại để có `old_priority`.
    *   Cập nhật `priority` trong bảng `tasks`.
    *   Cập nhật `updated_at`.
4.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'task_change_priority'`.
    *   `content` có thể lưu `old_priority` và `new_priority`.
5.  **Gửi Thông Báo (`notification_recipients`):**
    *   Gửi thông báo cho `assignees` hoặc `creator` của task về sự thay đổi độ ưu tiên.
6.  **Phản hồi (Response):):**
    *   Nếu thành công: Trả về #200-OK cùng với thông tin task đã cập nhật.
    *   Nếu thất bại: Trả về #4xx với thông báo lỗi.
# 6. Nghiệp vụ Xóa Công Việc (Delete Task)
Cho phép người dùng xóa một công việc (soft-delete).
## **Frontend:**
1.  **Giao diện:** Nút "Delete" trong modal chi tiết task hoặc menu ngữ cảnh.
2.  **Action:** Nhấn nút "Delete", có thể có hộp thoại xác nhận.
## **Backend:**
> ***API Endpoint***: `DELETE /api/tasks/{task_id}`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` có quyền xóa `task` (là `admin` của project, hoặc người tạo task/người được giao task).
2.  **Validation:**
    *   Kiểm tra `task_id` có tồn tại.
    *   **Nghiệp vụ "Task Dependencies":** Nếu task này là `blocking_task_id` cho bất kỳ task nào khác, cần cảnh báo và yêu cầu xử lý (ví dụ: gỡ bỏ phụ thuộc hoặc xóa luôn các task bị chặn).
3.  **Soft-delete Task:**
    *   Cập nhật bản ghi `task` trong bảng `tasks`:
        *   `is_deleted = TRUE`.
        *   `deleted_at = CURRENT_TIMESTAMP`.
        *   `updated_at = CURRENT_TIMESTAMP`.
    *   **Không xóa các bản ghi con vật lý ngay lập tức:** `subtasks`, `attachments`, `comments`, `task_assignees` vẫn được giữ lại nhưng sẽ không hiển thị trên UI chính. Chúng sẽ được xóa vĩnh viễn bởi cron job sau 30 ngày.
4.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'task_delete'`.
5.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về `200 OK` (thông báo task đã được chuyển vào thùng rác).
    *   Nếu thất bại: Trả về `4xx` với thông báo lỗi.
# 7. Nghiệp vụ Khôi Phục Công Việc (Restore Task)
Cho phép người dùng khôi phục một công việc đã bị soft-delete.
## **Frontend:**
1.  **Giao diện:** Trang "Trash" hoặc "Archive", hiển thị các task đã bị xóa mềm.
2.  **Action:** Nút "Restore" bên cạnh task.
## **Backend:**
>***API Endpoint***: `POST /api/tasks/{task_id}/restore`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` có quyền khôi phục `task`.
2.  **Validation:**
    *   Kiểm tra `task_id` tồn tại và `is_deleted = TRUE`.
3.  **Khôi phục Task:**
    *   Cập nhật bản ghi `task` trong bảng `tasks`:
        *   `is_deleted = FALSE`.
        *   `deleted_at = NULL`.
        *   `updated_at = CURRENT_TIMESTAMP`.
    *   Khôi phục các bản ghi con nếu cần (ví dụ: `subtasks`, `attachments`).
4.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'task_restore'`.
5.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về `200 OK`.
    *   Nếu thất bại: Trả về `4xx` với thông báo lỗi.
# 8. Nghiệp vụ Thêm/Xóa Subtasks (Add/Remove Subtask)
Quản lý các công việc con trong một task.
## **Frontend:**
1.  **Giao diện:** Trong modal chi tiết task, danh sách các subtasks.
2.  **Input:** `name` của subtask.
3.  **Action:** Nút "Add Subtask", nút "Delete" bên cạnh subtask, checkbox "Complete".
## **Backend:**
> ***API Endpoint***: `POST /api/tasks/{task_id}/subtasks`, `PUT /api/subtasks/{subtask_id}`, `DELETE /api/subtasks/{subtask_id}`
4.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` có quyền chỉnh sửa `task` mẹ.
5.  **Tạo Subtask:**
    *   Tạo bản ghi trong `subtasks`. `position` có thể là số lớn nhất hiện có + 1.
    *   Ghi activity `subtask_create`.
6.  **Cập nhật Subtask:**
    *   Sửa `name` hoặc `is_completed` của subtask.
    *   Ghi activity `subtask_update` hoặc `subtask_complete`.
7.  **Xóa Subtask:**
    *   Xóa bản ghi khỏi `subtasks`.
    *   Ghi activity `subtask_delete`.
8.  **Phản hồi (Response):**
    *   Nếu thành công: `200 OK` hoặc `201 Created`.
    *   Nếu thất bại: `4xx` với thông báo lỗi.
# 9. Nghiệp vụ Tìm Kiếm, Lọc & Sắp Xếp Công Việc (Search, Filter & Sort Tasks)
Cho phép người dùng dễ dàng tìm thấy các công việc cụ thể.
## **Frontend:**
1.  **Giao diện:** Thanh tìm kiếm, dropdown lọc theo trạng thái, người giao, deadline; dropdown sắp xếp.
2.  **Input:** `search_query`, `filter_status`, `filter_assignee_id`, `sort_by`, `sort_direction`.
## **Backend:**
>***API Endpoint***: `GET /api/tasks` hoặc `GET /api/projects/{project_id}/tasks`
1.  **Xác thực & Ủy quyền:** Đảm bảo `user` chỉ xem được các task mà họ có quyền truy cập.
2.  **Truy vấn Database:**
    *   Xây dựng câu lệnh SQL `SELECT` linh hoạt với `WHERE` clause dựa trên các tham số lọc:
        *   `WHERE tasks.name LIKE '%search_query%' OR tasks.description LIKE '%search_query%'` (hoặc Full-Text Search).
        *   `WHERE tasks.status = 'filter_status'`.
        *   `WHERE tasks.id IN (SELECT task_id FROM task_assignees WHERE user_id = filter_assignee_id)`.
    *   Thêm `ORDER BY` clause dựa trên các tham số sắp xếp.
    *   Sử dụng `JOIN` với `projects`, `users`, `task_assignees` để lấy dữ liệu liên quan.
3.  **Phản hồi (Response):**
    *   Trả về `200 OK` cùng với danh sách các task phù hợp.
# **Nghiệp vụ Tự động hóa Nâng cao.**
## **1. Công Việc Lặp Lại (Recurring Tasks)**

*   **Vấn đề/Nhu cầu:** Người dùng có những công việc cần thực hiện định kỳ (hàng ngày, hàng tuần) như "Báo cáo cuối ngày", "Kiểm tra email". Việc tạo lại thủ công rất tốn thời gian.
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   Thêm các cột mới vào bảng `tasks`:

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Ghi Chú |
| :--- | :--- | :--- | :--- |
| `is_recurring` | `BOOLEAN` | `DEFAULT FALSE` | Đánh dấu đây có phải là task lặp lại không. |
| `recurrence_rule` | `VARCHAR(255)`| `NULL` | Lưu quy tắc lặp lại, ví dụ theo chuẩn iCalendar: `FREQ=WEEKLY;BYDAY=MO,WE,FR`. |
| `next_recurrence_date`| `DATE` | `NULL` | Ngày mà task lặp lại tiếp theo sẽ được tạo. |
*   **Luồng hoạt động:**
    1.  Khi người dùng tạo một task, họ có thể chọn các tùy chọn lặp lại.
    2.  Khi một task lặp lại được hoàn thành (`is_completed = TRUE`), backend sẽ dựa vào `recurrence_rule` để tính toán `next_recurrence_date`.
    3.  Một tác vụ nền (cron job) chạy hàng ngày, tìm tất cả các task có `next_recurrence_date` là ngày hôm nay, sao chép chúng thành một task mới (với `is_recurring = FALSE`) và cập nhật lại `next_recurrence_date` cho task gốc.

## **2. Sự Phụ Thuộc Giữa Các Công Việc (Task Dependencies)**

*   **Vấn đề/Nhu cầu:** Trong nhiều quy trình, "Công việc B" chỉ có thể bắt đầu sau khi "Công việc A" đã hoàn thành.
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   Tạo một bảng nối mới: `task_dependencies`.

| Tên Cột            | Kiểu Dữ Liệu     | Ràng Buộc                                | Ghi Chú                          |
| :----------------- | :--------------- | :--------------------------------------- | :------------------------------- |
| `task_id`          | `INT` / `BIGINT` | `FK(tasks.id)`                           | Công việc bị chặn.               |
| `blocking_task_id` | `INT` / `BIGINT` | `FK(tasks.id)`                           | Công việc phải hoàn thành trước. |
|                    |                  | *PRIMARY KEY(task_id, blocking_task_id)* |                                  |
*   **Luồng hoạt động:**
    1.  Trên giao diện, người dùng có thể thiết lập rằng Task B phụ thuộc vào Task A.
    2.  Hệ thống tạo một bản ghi trong `task_dependencies` với `task_id` là của B và `blocking_task_id` là của A.
    3.  Giao diện sẽ không cho phép người dùng thay đổi trạng thái của Task B sang `in-progress` hoặc `done` nếu trạng thái của Task A chưa phải là `done`.