Tài liệu này mô tả chi tiết tất cả các nghiệp vụ (user flow) liên quan đến thực thể `projects`, dựa trên thiết kế giao diện và cấu trúc cơ sở dữ liệu đã định nghĩa.
# **1. Nghiệp vụ Tạo Dự án Mới (Create Project)**
Cho phép người dùng tạo một dự án mới trực tiếp từ giao diện Kanban board.
#### **Frontend:**
1. **Giao diện**: Trên trang Kanban Board, mỗi cột trạng thái (ví dụ: "Not Started", "In Progress") đều có một nút `+` ở phần header. 
2. **Action**: Người dùng nhấn vào nút `+` ở cột mà họ muốn tạo dự án. Một form/modal tạo dự án nhanh sẽ hiện ra, yêu cầu nhập tên dự án.
3. **Send `request`**: Gửi yêu cầu `POST` đến backend với `name` và `status` tương ứng với cột đã chọn.
#### **Backend:**
> **_API Endpoint_**: `POST /api/projects`
1. **Xác thực & Ủy quyền:**
    - Đảm bảo người dùng đã đăng nhập.
2. **Validation:**
    - `name` không được để trống.
    - `status` phải là một trong các giá trị `ENUM` hợp lệ ('temp', 'not-started', 'in-progress', 'in-review', 'done').
3. **Lưu vào Database (`projects`):**
    - Tạo một bản ghi mới trong bảng `projects`.
    - `workspace_id`: Lấy ID của workspace hiện tại mà người dùng đang xem.
    - `name`: Tên người dùng cung cấp.
    - `status`: Giá trị tương ứng với cột mà dự án được tạo.
    - `project_type`: Mặc định là `'personal'`.
    - `is_archived`, `is_deleted`: Mặc định là `FALSE`.
4. **Thêm thành viên (`project_members`):**
    - Tự động thêm người tạo dự án vào bảng `project_members` với `role = 'admin'`.
5. **Ghi Hoạt Động (`activities`):**
    - Tạo một bản ghi `activities` với `action_type = 'project_create'`.
6. **Phản hồi (Response):**
    - Nếu thành công: Trả về #201-Created cùng với thông tin dự án mới. Giao diện sẽ hiển thị `project-card` mới trong cột tương ứng.
    - Nếu thất bại: Trả về lỗi #4xx.
# **2. Nghiệp vụ Cập nhật Trạng thái Dự án (Update Project Status)**
Hệ thống cung cấp 3 cách để cập nhật trạng thái của một dự án.
## **a. Kéo-thả trên Kanban Board (Drag-and-Drop)**
- **Frontend**: Người dùng kéo một `project-card` từ cột này sang cột khác trên `kanban-view`.
- **Backend**:
    - Nhận `project_id` và `new_status` từ frontend.    
    - Cập nhật trường `status` trong bảng `projects`.    
    - Ghi lại hoạt động vào bảng `activities` với `action_type = 'project_update_status'`, `content` có thể lưu trạng thái cũ và mới.   
## **b. Dropdown trong Project Detail Modal**
- **Frontend**: Trong modal chi tiết dự án, người dùng chọn một trạng thái mới từ dropdown `prj-mdl-status`.
- **Backend**: Logic tương tự như kéo-thả.
## **c. Checkbox trong Project Detail Modal**
- **Vấn đề & Giải pháp**: Giao diện có một checkbox (`project-status-checkbox`) ở cạnh tên dự án. Vì trạng thái dự án có nhiều giá trị, checkbox này sẽ có một quy tắc nghiệp vụ đặc thù:
    - **Check (đánh dấu hoàn thành)**: Thay đổi `projects.status` thành `'done'`.
    - **Uncheck (bỏ hoàn thành)**: Thay đổi `projects.status` về trạng thái trước đó, hoặc một trạng thái mặc định như `'in-progress'`.
- **Backend**: Cần lưu lại trạng thái trước khi chuyển thành `'done'` để có thể quay lại khi người dùng uncheck.
# **3. Nghiệp vụ Cập nhật Chi tiết Dự án (Update Project Details)**
Các hành động này chủ yếu diễn ra trong `projectDetailModal`.

| Thành phần Giao diện                    | Nghiệp vụ Backend                                                                                                                                                                                                                                                                 | File nghiệp vụ liên quan   |
| --------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------- |
| **Project Title** (inline-editable)     | Cập nhật trường `projects.name`. Ghi `activity` với `action_type = 'project_update_details'`.                                                                                                                                                                                     | [[Database - PronaFlow]]   |
| **Description** (textarea)              | Cập nhật trường `projects.description`. Ghi `activity` tương tự.                                                                                                                                                                                                                  | [[Database - PronaFlow]]   |
| **Manage Members**                      | Thêm/Xóa bản ghi trong bảng `project_members`. Khi thêm thành viên đầu tiên, **tự động chuyển `projects.project_type` từ `'personal'` sang `'team'`**. Ghi `activity` với `action_type = 'project_add_member'` hoặc `project_remove_member` và gửi thông báo cho người liên quan. | [[UserFlow - Projects]]    |
| **Manage Tags**                         | Thêm/Xóa bản ghi trong bảng nối `project_tags`. Cho phép tạo `tag` mới (thêm bản ghi vào bảng `tags` với `workspace_id` hiện tại).                                                                                                                                                | [[Database - PronaFlow]]   |
| **Deadline** (`start_date`, `end_date`) | Cập nhật các trường `start_date` và `end_date` trong bảng `projects`.                                                                                                                                                                                                             | [[Database - PronaFlow]]   |
| **Attachment** / **Cover Image**        | Tạo bản ghi mới trong bảng `attachments` với `attachable_id` là ID của dự án và `attachable_type` là `'project'`. Đối với ảnh bìa, cập nhật `projects.cover_image_url`.                                                                                                           | [[UserFlow - Attachments]] |

## Logic của `project_type`
1. value: `true` - `personal`: 
	- Khi một dự án được tạo với type `'personal'`, nó chỉ thuộc về và hiển thị cho người dùng đã tạo ra nó (owner_id của workspace chứa nó).
	- Mặc định khi người dùng tạo dự án mới, type sẽ là `'personal'`.
	- Khi personal-project được owner thêm thành viên (add member(s)) thì thuộc tính `project_type` sẽ được chuyển từ `'personal'` -> `'team'`.
2. value: `false` - `'team'`:
	- Khi một dự án được tạo với type `'team'`, người tạo có thể mời các `users` khác tham gia vào dự án đó (thêm bản ghi vào `project_members`).
	- Dự án này sẽ hiển thị cho tất cả các thành
	- viên trong danh sách dự án của họ.
# 4. Nghiệp vụ Actions trong Project Detail Modal
Phần này mô tả chi tiết các luồng xử lý cho các nút chức năng trong sidebar của modal chi tiết dự án.
### **a. Nghiệp vụ Lưu trữ Dự án (Archive Project)**
Cho phép người dùng ẩn một dự án khỏi giao diện làm việc chính mà không cần xóa vĩnh viễn.
#### **Frontend:**
1. **Giao diện**: Trong sidebar của "Project Detail Modal", người dùng nhấn vào nút "Archive".
2. **Action**: Một popover xác nhận hiện ra để hỏi người dùng có chắc chắn muốn lưu trữ không. Người dùng nhấn "Archive" để xác nhận.
#### **Backend:**
> **_API Endpoint_**: `POST /api/projects/{project_id}/archive`
1. **Xác thực & Ủy quyền:**
    - Đảm bảo người dùng đã đăng nhập.    
    - Kiểm tra người dùng có quyền `admin` trong dự án (`project_members.role = 'admin'`) hoặc là chủ sở hữu của workspace (`workspaces.owner_id`) chứa dự án này.     
2. **Cập nhật Database (`projects`):**
    - Cập nhật bản ghi của dự án tương ứng:
        - Set `is_archived = TRUE`.
        - Cập nhật `updated_at`.
3. **Ghi Hoạt Động (`activities`):**
    - Tạo một bản ghi mới trong bảng `activities`.
    - `action_type`: `'project_archive'`.
    - `user_id`: ID của người thực hiện hành động.
    - `target_id`: ID của dự án vừa được lưu trữ.
    - `target_type`: `'project'`.
4. **Phản hồi (Response):**
    - Nếu thành công: Trả về #200-OK. Frontend sẽ đóng modal và ẩn dự án khỏi các giao diện chính (như Kanban board).
    - Nếu thất bại: Trả về lỗi #4xx (ví dụ: `403 Forbidden` nếu không có quyền).
### **b. Nghiệp vụ Di chuyển Dự án (Move Project)**
Cho phép người dùng di chuyển một dự án từ workspace này sang một workspace khác mà họ sở hữu.
#### **Frontend:**
1. **Giao diện**: Người dùng nhấn vào nút "Move" trong sidebar.
2. **Action**: Một popover hiện ra, chứa một dropdown cho phép người dùng chọn một `workspace` khác làm đích đến.
3. **Send `request`**: Gửi yêu cầu lên backend với `target_workspace_id`.
#### **Backend:**
> **_API Endpoint_**: `PUT /api/projects/{project_id}/move`
1. **Xác thực & Ủy quyền:**
    - Đảm bảo người dùng là chủ sở hữu (`owner_id`) của cả workspace hiện tại và workspace đích. Đây là hành động cấp cao, chỉ chủ sở hữu workspace mới nên thực hiện.
2. **Validation:**
    - Kiểm tra `target_workspace_id` có tồn tại và thuộc sở hữu của người dùng hiện tại không.
3. **Cập nhật Database (`projects`):**
    - Cập nhật cột `workspace_id` của dự án thành `target_workspace_id`.
4. **Xử lý Dữ liệu liên quan (Side Effects):**
    - **Tags**: Vì `tags` thuộc về một `workspace`, hệ thống cần xử lý các `project_tags` hiện có. Logic đề xuất: Xóa tất cả các liên kết tag cũ trong bảng `project_tags` của dự án này. Người dùng sẽ cần phải gán lại tag trong workspace mới.
5. **Ghi Hoạt Động (`activities`):**
    - Tạo một bản ghi `activities` với `action_type = 'project_move_workspace'` (Cần thêm giá trị ENUM này).
    - `content` có thể lưu `{ "old_workspace_id": X, "new_workspace_id": Y }`.
6. **Phản hồi (Response):**
    - Nếu thành công: Trả về #200-OK. Frontend sẽ làm mới lại danh sách dự án trong cả hai workspace.
    - Nếu thất bại: Trả về lỗi #4xx.
### **c. Nghiệp vụ Nhân bản Dự án (Duplicate Project)**
Cho phép người dùng tạo một bản sao của một dự án hiện có.
#### **Frontend:**
1. **Giao diện**: Người dùng nhấn vào nút "Duplicate" trong sidebar.
2. **Action**: Một popover hiện ra cho phép người dùng tùy chỉnh tên của dự án mới (ví dụ: "Copy of...") và có thể chọn các thành phần muốn sao chép (ví dụ: tasks, members).
3. **Send `request`**: Gửi yêu cầu nhân bản lên backend.
#### **Backend:**
> **_API Endpoint_**: `POST /api/projects/{project_id}/duplicate`
1. **Xác thực & Ủy quyền:**
    - Kiểm tra người dùng có quyền `admin` trong dự án (`project_members.role = 'admin'`) hoặc là chủ sở hữu của workspace.
2. **Logic Nhân bản (Database Operations):**
    - **Bước 1: Tạo Dự án mới**: Tạo một bản ghi mới trong bảng `projects`, sao chép các thông tin như `name` (thêm hậu tố "(Copy)"), `description`, `cover_image_url`. Các trường như `status` nên được reset về giá trị mặc định (ví dụ: `'not-started'`).
    - **Bước 2: Sao chép Giai đoạn (`task_lists`)**: Đọc tất cả các bản ghi `task_lists` của dự án gốc. Tạo các bản ghi mới tương ứng cho dự án mới, giữ nguyên `name` và `position`. Lưu lại một bản map từ ID cũ sang ID mới (`old_task_list_id` -> `new_task_list_id`).
    - **Bước 3: Sao chép Công việc (`tasks`)**: Đọc tất cả các `tasks` của dự án gốc. Với mỗi task, tạo một bản ghi mới trong bảng `tasks` cho dự án mới, sử dụng `new_task_list_id` tương ứng từ bản map ở bước 2.
    - **Lưu ý**: Các dữ liệu mang tính thời điểm hoặc cá nhân như `task_assignees`, `comments`, `attachments`, và `project_members` (ngoại trừ người thực hiện hành động) **không nên** được sao chép theo mặc định để tránh tạo ra thông báo và dữ liệu không mong muốn.
3. **Ghi Hoạt Động (`activities`):**
    - Tạo một bản ghi `activities` với `action_type = 'project_duplicate'` (Cần thêm giá trị ENUM này).
    - `content` có thể lưu `{ "source_project_id": X, "new_project_id": Y }`.
4. **Phản hồi (Response):**
    - Nếu thành công: Trả về #201-Created cùng với thông tin của dự án mới. Frontend sẽ hiển thị dự án mới này trong workspace.
    - Nếu thất bại: Trả về lỗi #4xx hoặc `500 Internal Server Error` nếu quá trình sao chép phức tạp gặp lỗi.