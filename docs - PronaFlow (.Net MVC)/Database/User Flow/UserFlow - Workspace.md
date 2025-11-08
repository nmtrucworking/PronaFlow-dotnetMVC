Tài liệu này mô tả chi tiết các nghiệp vụ liên quan đến việc quản lý `Workspaces`. Workspace là không gian làm việc cấp cao nhất, chứa nhiều dự án và thuộc sở hữu của một người dùng duy nhất (`owner_id`).

**Bảng Database liên quan:** `workspaces`.
# **1. Nghiệp vụ Tạo Workspace Mới (Create New Workspace)**
Cho phép người dùng tạo thêm các không gian làm việc mới để phân tách các lĩnh vực công việc khác nhau.
## **Frontend:**
1. **Giao diện**: Người dùng nhấn vào dropdown `workspace-selector` trên `sidebar`. Ở dưới cùng của danh sách các workspace hiện có, sẽ có một nút "Add new workspace". 
2. **Action**: Khi nhấn vào nút, một modal sẽ hiện ra yêu cầu người dùng nhập `name` (bắt buộc) và `description` (tùy chọn) cho workspace mới.
3. **Send `request`**: Gửi yêu cầu `POST` đến backend với các thông tin đã nhập.
## **Backend:**

> **_API Endpoint_**: `POST /api/workspaces`
1. **Xác thực & Ủy quyền**:
    - Đảm bảo người dùng đã đăng nhập.
2. **Validation**:
    - `name` không được để trống.
    - Có thể kiểm tra để một người dùng không thể có hai workspace với tên trùng lặp.
3. **Lưu vào Database (`workspaces`):**
    - Tạo một bản ghi mới trong bảng `workspaces`.
    - `name`, `description`: Lấy từ input của người dùng.
    - `owner_id`: ID của người dùng đang thực hiện hành động.
4. **Phản hồi (Response):**
    - **Thành công**: Trả về `201 Created` cùng với thông tin workspace mới. Giao diện sẽ tự động thêm workspace mới vào dropdown và có thể chuyển người dùng sang workspace vừa tạo.
    - **Thất bại**: Trả về `4xx` với thông báo lỗi.
# **2. Nghiệp vụ Cập nhật Workspace (Update Workspace)**
Cho phép chủ sở hữu đổi tên và mô tả của workspace.
## **Frontend:**
1. **Giao diện**: Gần dropdown `workspace-selector` trên `sidebar`, khi một workspace được chọn, có một biểu tượng "Cài đặt" (Settings) nhỏ xuất hiện.
2. **Action**: Khi nhấn vào, người dùng được điều hướng đến một trang/modal "Workspace Settings", nơi họ có thể chỉnh sửa `name` và `description`.
3. **Send `request`**: Gửi yêu cầu `PUT` đến backend với thông tin mới.
## **Backend:**
> **_API Endpoint_**: `PUT /api/workspaces/{workspace_id}`
1. **Xác thực & Ủy quyền**:
    - Đảm bảo người dùng đã đăng nhập và `user_id` của họ chính là `owner_id` của workspace này.
2. **Validation**:
    - `name` không được để trống.
3. **Cập nhật Database (`workspaces`):**
    - Cập nhật lại `name` và `description` cho bản ghi có `workspace_id` tương ứng.
4. **Phản hồi (Response):**
    - **Thành công**: Trả về `200 OK`. Giao diện cập nhật lại tên workspace trên sidebar.
    - **Thất bại**: Trả về `403 Forbidden` (không có quyền) hoặc `404 Not Found`.
# **3. Nghiệp vụ Xóa Workspace (Delete Workspace)**

Đây là một hành động nguy hiểm và cần có các bước xác nhận cẩn thận.
## **Frontend:**
1. **Giao diện**: Tại trang "Workspace Settings", có một khu vực "Danger Zone" với nút "Delete this workspace".
2. **Action**: Khi nhấn vào, một modal xác nhận hiện ra, yêu cầu người dùng nhập lại tên của workspace để kích hoạt nút xóa (tương tự như xóa dự án).
3. **Cảnh báo**: Modal phải thông báo rõ ràng cho người dùng về điều kiện xóa (ví dụ: "Bạn phải di chuyển hoặc xóa tất cả các dự án trong workspace này trước khi xóa nó").
## **Backend:**
> **_API Endpoint_**: `DELETE /api/workspaces/{workspace_id}`
1. **Xác thực & Ủy quyền**:
    - Đảm bảo người dùng là `owner_id` của workspace.
2. **Kiểm tra Điều kiện (Business Rule):**
    - **Quan trọng**: Truy vấn bảng `projects` để kiểm tra xem có bất kỳ dự án nào có `workspace_id` này không.
    - **Nếu có dự án**: Trả về lỗi `400 Bad Request` với thông báo "Workspace not empty. Please move or delete all projects before deleting the workspace."
    - **Nếu không có dự án**: Cho phép tiếp tục quá trình xóa.
3. **Xóa Database (`workspaces`):**
    - Xóa bản ghi workspace khỏi bảng `workspaces`.
4. **Phản hồi (Response):**
    - **Thành công**: Trả về `200 OK`. Giao diện sẽ xóa workspace khỏi dropdown và tự động chuyển người dùng về workspace mặc định của họ.
    - **Thất bại**: Trả về `4xx` với thông báo lỗi tương ứng.
# **4. Nghiệp vụ Chuyển đổi Workspace (Switch Workspace)**
Nghiệp vụ này chủ yếu diễn ra ở phía frontend để thay đổi ngữ cảnh hiển thị.
#### **Frontend:**
1. **Giao diện**: Người dùng chọn một workspace khác từ dropdown `workspace-selector` trên `sidebar`.
2. **Action**:
    - Ứng dụng cập nhật lại trạng thái (state) toàn cục với `current_workspace_id` mới.
    - Gửi các yêu cầu API mới để lấy dữ liệu thuộc về workspace mới này, ví dụ:
        - Lấy danh sách dự án cho trang Kanban Board (`GET /api/workspaces/{new_id}/projects`).
        - Lấy danh sách tags cho các bộ lọc (`GET /api/workspaces/{new_id}/tags`).
    - Giao diện được render lại với dữ liệu mới.