Mô tả chi tiết các nghiệp vụ chính liên quan đến bảng `task_lists` (hay còn gọi là "Phases").
`task_lists` là các giai đoạn tổ chức các `tasks` bên trong một `project`.
# 1. Nghiệp vụ Tạo Giai Đoạn Mới (Create TaskList/Phase)
Cho phép người dùng tạo một giai đoạn mới trong một dự án cụ thể.
## **Frontend:**
1.  **Giao diện:** Trên trang chi tiết `project` (Project Detail Modal), người dùng click vào nút "Add Phase" hoặc một biểu tượng tương tự để thêm một giai đoạn mới.
2.  **Input:**
    *   `name`: Tên giai đoạn (ví dụ: "Phase 1: Research", "To Do", "In Progress") (bắt buộc).
    *   `project_id`: ID của dự án hiện tại (được lấy từ ngữ cảnh URL hoặc trạng thái).
3.  **Action:** Nhấn "Create", "Add" hoặc "Enter".
## **Backend:**
> ***API Endpoint***: `POST /api/projects/{project_id}/tasklists`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` hiện tại đã đăng nhập và có quyền `member` hoặc `admin` trong `project` (`project_members`).
    *   Nếu `project` có `is_deleted = TRUE` hoặc `is_archived = TRUE`, không cho phép tạo tasklist mới.
2.  **Validation:**
    *   `name` không được trống.
    *   `project_id` phải tồn tại.
    *   Tên `name` của `task_list` phải là duy nhất trong cùng một `project_id` (ví dụ: không thể có hai giai đoạn tên "To Do" trong cùng một dự án).
3.  **Lưu vào Database (`task_lists`):**
    *   Tạo một bản ghi mới trong bảng `task_lists` với `name` và `project_id`.
    *   `position`: Tự động gán một giá trị là số lớn nhất hiện có của `position` trong `project` đó + 1, để đảm bảo thứ tự hiển thị hợp lý.
4.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'tasklist_create'` do `user_id` hiện tại thực hiện. `target_type = 'project'`, `target_id = project_id`. `content` có thể chứa tên tasklist.
5.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về #201-Created cùng với thông tin tasklist mới tạo.
    *   Nếu thất bại: Trả về #4xx với thông báo lỗi.

# 2. Nghiệp vụ Cập Nhật Giai Đoạn (Update TaskList/Phase)
Cho phép người dùng sửa đổi tên hoặc thứ tự của một giai đoạn.
## **Frontend:**
1.  **Giao diện:** Trên Project Detail Modal, người dùng chỉnh sửa tên giai đoạn bằng cách click vào tiêu đề, hoặc kéo thả cột để thay đổi thứ tự.
2.  **Input:**
    *   `name`: Tên giai đoạn mới (tùy chọn).
    *   `position`: Vị trí mới (tùy chọn, khi kéo thả).
3.  **Action:** Nhấn "Save", "Enter" hoặc hoàn tất thao tác kéo thả.

## **Backend:**
> ***API Endpoint***: `PUT /api/tasklists/{task_list_id}`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` đã đăng nhập và có quyền chỉnh sửa `project` chứa `task_list` này (là `member` hoặc `admin` của project).
2.  **Validation:**
    *   `task_list_id` phải tồn tại.
    *   Nếu `name` được cập nhật, đảm bảo tên mới không trùng với các tasklist khác trong cùng `project_id`.
    *   Nếu `position` được cập nhật, đảm bảo giá trị hợp lệ.
3.  **Truy vấn & So sánh dữ liệu cũ-mới:**
    *   Lấy bản ghi `task_list` hiện tại từ CSDL để so sánh các thay đổi.
4.  **Cập nhật Database (`task_lists`):**
    *   Cập nhật `name` hoặc `position` trong bảng `task_lists`.
    *   Nếu `position` thay đổi, cần điều chỉnh `position` của các `task_list` khác trong cùng `project_id` để duy trì thứ tự liền mạch (ví dụ: nếu di chuyển một cột lên trên, các cột khác sẽ dịch xuống).
5.  **Ghi Hoạt Động (`activities`):**
    *   Nếu có thay đổi về `name` hoặc `position`:
        *   Tạo bản ghi trong `activities` với `action_type` phù hợp (ví dụ: `'tasklist_rename'`, `'tasklist_reorder'`).
        *   `content` có thể lưu thông tin chi tiết thay đổi (giá trị cũ/mới).
6.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về `200 OK` cùng với thông tin tasklist đã cập nhật.
    *   Nếu thất bại: Trả về `4xx` với thông báo lỗi.
# 3. Nghiệp vụ Xóa Giai Đoạn (Delete TaskList/Phase)
Cho phép người dùng xóa một giai đoạn khỏi một dự án.
## **Frontend:**
1.  **Giao diện:** Nút "Delete" (biểu tượng thùng rác) trên tiêu đề của cột/giai đoạn trong Kanban board hoặc Project Detail Modal.
2.  **Action:** Nhấn nút "Delete", có thể có hộp thoại xác nhận.
## **Backend:**
> ***API Endpoint***: `DELETE /api/tasklists/{task_list_id}`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` có quyền xóa `task_list` (là `admin` của project, hoặc người tạo project).
2.  **Validation:**
    *   `task_list_id` phải tồn tại.
    *   **Xử lý Tasks liên quan:**
        *   Nếu `task_list` có chứa các `tasks`, hệ thống cần yêu cầu người dùng xác nhận cách xử lý:
            *   **Di chuyển tasks:** Di chuyển tất cả các `tasks` trong `task_list` này sang một `task_list` khác trước khi xóa.
            *   **Xóa tasks:** Xóa mềm tất cả các `tasks` thuộc `task_list` này cùng với nó.
            *   **Không cho phép xóa:** Nếu `task_list` không rỗng và không có tùy chọn di chuyển/xóa tự động.
3.  **Xóa Database:**
    *   Nếu đã xử lý các `tasks` liên quan: Xóa bản ghi `task_list` khỏi bảng `task_lists`.
    *   **Cập nhật `tasks.task_list_id`:** Nếu `tasks` được di chuyển, cập nhật `task_list_id` của chúng.
    *   **Cập nhật `position`:** Điều chỉnh `position` của các `task_list` còn lại trong cùng `project_id` để lấp đầy khoảng trống.
4.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'tasklist_delete'`.
    *   `content` có thể lưu tên tasklist bị xóa và cách xử lý các task bên trong.
5.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về `200 OK` (thông báo tasklist đã được xóa).
    *   Nếu thất bại: Trả về `4xx` với thông báo lỗi.

# 4. Nghiệp vụ Di Chuyển Công Việc Giữa Các Giai Đoạn (Move Task Between TaskLists)
Cho phép người dùng di chuyển một công việc từ giai đoạn này sang giai đoạn khác (kéo thả).
## **Frontend:**
1.  **Giao diện:** Trên Project Detail Modal, người dùng kéo thả một `task-card` từ cột này sang cột khác.
2.  **Input:**
    *   `task_id`: ID của công việc được di chuyển.
    *   `old_task_list_id`: ID của giai đoạn cũ.
    *   `new_task_list_id`: ID của giai đoạn mới.
    *   `new_position`: Vị trí mới của task trong `new_task_list_id` (nếu cần duy trì thứ tự).
3.  **Action:** Hoàn tất thao tác kéo thả.
## **Backend:**
> ***API Endpoint***: `PUT /api/tasks/{task_id}/move-to-tasklist`
1.  **Xác thực & Ủy quyền:**
    *   Đảm bảo `user` có quyền chỉnh sửa `task` (là `member` hoặc `admin` của project, hoặc là người được giao task).
2.  **Validation:**
    *   `task_id`, `old_task_list_id`, `new_task_list_id` phải tồn tại và thuộc cùng một `project_id`.
3.  **Cập nhật Database (`tasks`):**
    *   Cập nhật `task_list_id` của `task` thành `new_task_list_id`.
    *   Cập nhật `status` của task nếu cần (ví dụ: chuyển từ 'not-started' sang 'in-progress' khi di chuyển vào cột "In Progress").
    *   Cập nhật `updated_at`.
    *   Nếu có quản lý `position` của task trong mỗi `task_list`, cần điều chỉnh `position` của các task trong `old_task_list_id` và `new_task_list_id`.
4.  **Ghi Hoạt Động (`activities`):**
    *   Tạo bản ghi trong `activities` với `action_type = 'task_move_tasklist'`.
    *   `content` có thể lưu `task_id`, `old_task_list_id` và `new_task_list_id` (hoặc tên của chúng).
5.  **Gửi Thông Báo (`notification_recipients`):**
    *   Gửi thông báo cho `assignees` hoặc `creator` của task về việc task đã được di chuyển.
6.  **Phản hồi (Response):**
    *   Nếu thành công: Trả về `200 OK` cùng với thông tin task đã cập nhật.
    *   Nếu thất bại: Trả về `4xx` với thông báo lỗi.