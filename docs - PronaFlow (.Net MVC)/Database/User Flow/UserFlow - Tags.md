Tài liệu này mô tả chi tiết các nghiệp vụ liên quan đến việc quản lý và sử dụng `tags`. `Tags` được dùng để phân loại, tổ chức các `projects` và được quản lý trong phạm vi của một `workspace` cụ thể.
**Các bảng Database liên quan:**
- `tags`: Lưu trữ thông tin của từng tag (tên, màu sắc, workspace trực thuộc)
- `project_tags`: Bảng nối, thể hiện mối quan hệ nhiều-nhiều giữa `projects` và `tags`.
---
# **1. Nghiệp vụ Tạo Tag Mới (Create Tag)**
Hệ thống cho phép tạo tag mới từ hai nơi: trang Cài đặt và modal chi tiết Dự án.
## **a. Tạo Tag từ trang Cài đặt (Settings Page)**
Đây là luồng quản lý chính.
### **Frontend:**
1. **Giao diện**: Người dùng truy cập trang Settings và chọn mục "Tags Management".
2. **Chọn Workspace**: Người dùng chọn `workspace` mà họ muốn quản lý tags từ dropdown (`current-workspace`). Giao diện sẽ tải danh sách các tags hiện có của workspace đó.
3. **Input**: Người dùng nhập tên vào ô `new-tag-name`, chọn màu từ `color-input`, và nhấn nút "Add Tag".
### **Backend:**
> **_API Endpoint_**: `POST /api/workspaces/{workspace_id}/tags`
1. **Xác thực & Ủy quyền**:
    - Đảm bảo người dùng đã đăng nhập và có quyền quản lý (`admin`/`owner`) trong `workspace` được chọn.  
2. **Validation**:
    - `name` và `color_hex` không được để trống.
3. **Lưu vào Database (`tags`):**
    - Tạo một bản ghi mới trong bảng `tags` với `name`, `color_hex` và `workspace_id` đã cung cấp.
4. **Phản hồi (Response):**
    - **Thành công**: Trả về `201 Created` cùng với thông tin tag mới. Giao diện sẽ hiển thị tag mới trong danh sách.
    - **Thất bại**: Trả về #4xx với thông báo lỗi (ví dụ: tên tag đã tồn tại).
## **b. Tạo Tag từ Modal Chi tiết Dự án (Shortcut)**
Đây là luồng tạo nhanh khi đang làm việc.
### **Frontend:**
1. **Giao diện**: Trong modal chi tiết dự án, người dùng nhấn vào nút quản lý tags để mở popover `manageTagsPopover`.
2. **Input**: Người dùng sử dụng form "Add new tag" ngay trong popover để nhập tên và chọn màu, sau đó nhấn "Add".
### **Backend:**
- Logic xử lý hoàn toàn tương tự như khi tạo từ trang Settings. `workspace_id` sẽ được lấy từ dự án hiện tại đang được xem.
# **2. Nghiệp vụ Cập nhật Tag (Update Tag)**
## **Frontend:**
1. **Giao diện**: Tại trang "Tags Management", bên cạnh mỗi tag trong danh sách sẽ có nút "Edit". 
2. **Action**: Khi nhấn "Edit", tên và màu của tag sẽ cho phép chỉnh sửa. Sau khi thay đổi, người dùng nhấn "Save".
## **Backend:**
> **_API Endpoint_**: `PUT /api/tags/{tag_id}`
1. **Xác thực & Ủy quyền**: Đảm bảo người dùng có quyền quản lý `workspace` chứa tag này.
2. **Validation**: Tương tự như khi tạo mới.
3. **Cập nhật Database (`tags`):**
    - Cập nhật lại `name` và/hoặc `color_hex` cho bản ghi có `tag_id` tương ứng trong bảng `tags`.   
4. **Phản hồi (Response):**
    - **Thành công**: Trả về #200-OK. Giao diện cập nhật lại thông tin tag.
    - **Thất bại**: Trả về #4xx.
# **3. Nghiệp vụ Xóa Tag (Delete Tag)**
## **Frontend:**
1. **Giao diện**: Tại trang "Tags Management", bên cạnh mỗi tag sẽ có nút "Delete".
2. **Action**: Người dùng nhấn "Delete". Một hộp thoại xác nhận sẽ hiện ra để tránh xóa nhầm.
## **Backend:**
> **_API Endpoint_**: `DELETE /api/tags/{tag_id}`
1. **Xác thực & Ủy quyền**: Đảm bảo người dùng có quyền quản lý `workspace` chứa tag này.
2. **Xóa Liên kết (`project_tags`):**
    - **Quan trọng**: Trước khi xóa tag, hệ thống phải xóa tất cả các bản ghi liên quan trong bảng `project_tags` nơi `tag_id` bằng với ID của tag sắp bị xóa. `DELETE FROM project_tags WHERE tag_id = ?`
3. **Xóa Tag (`tags`):**
    - Sau khi đã xóa các liên kết, xóa bản ghi tag khỏi bảng `tags`. `DELETE FROM tags WHERE id = ?`
4. **Phản hồi (Response):**
    - **Thành công**: Trả về #200-OK. Giao diện sẽ xóa tag khỏi danh sách.
    - **Thất bại**: Trả về #4xx.
# **4. Nghiệp vụ Gán và Gỡ Tag khỏi Dự án (Assign & Unassign)**
## **Frontend:**
1. **Giao diện**: Trong modal chi tiết dự án, người dùng mở popover quản lý tags (`manageTagsPopover`).
2. **Action**: Một danh sách các tags có sẵn trong workspace sẽ hiện ra cùng với các checkbox. Người dùng đánh dấu hoặc bỏ đánh dấu các checkbox để gán hoặc gỡ tag khỏi dự án.
## **Backend:**
> **_API Endpoint_**: `PUT /api/projects/{project_id}/tags`
1. **Xác thực & Ủy quyền**: Đảm bảo người dùng là thành viên (`member`/`admin`) của dự án.
2. **Input**: Backend nhận một mảng chứa ID của tất cả các tag sẽ được áp dụng cho dự án (ví dụ: `[1, 5, 12]`).
3. **Cập nhật Database (`project_tags`):**
    - **Chiến lược "Thay thế toàn bộ":**
        1. Xóa tất cả các bản ghi hiện có của `project_id` trong bảng `project_tags`.
        2. Lặp qua mảng `tag_id` nhận được từ frontend, với mỗi `tag_id`, tạo một bản ghi mới trong `project_tags`.
4. **Phản hồi (Response):**
    - **Thành công**: Trả về #200-OK. Giao diện cập nhật lại danh sách tag của dự án.
    - **Thất bại**: Trả về #4xx.