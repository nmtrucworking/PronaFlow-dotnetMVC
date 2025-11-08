# **I. Mục đích và Vai trò của Admin**
*Mục đích* và *vai trò* của trang Admin trong hệ sinh thái PronaFlow.
## **1. Vai trò (Persona):**
-> Admin là một **Super User** (Quản trị viên hệ thống), có quyền truy cập và giám sát toàn bộ dữ liệu của ứng dụng. Vai trò này tương ứng với `users.role = 'admin'` trong bảng `users` của bạn.
## **2. Mục đích:**
- **Giám sát (Oversight):** Cung cấp một cái nhìn toàn cảnh về "sức khỏe" của hệ thống, từ số lượng người dùng, dự án đang hoạt động đến các hoạt động gần đây.
- **Quản lý Dữ liệu (Data Management):** Cho phép thực hiện các hành động can thiệp ở cấp độ cao như quản lý tài khoản người dùng, xem xét các dự án, và xử lý các vấn đề dữ liệu.
- **Bảo trì & An ninh (Maintenance & Security):** Cung cấp công cụ để quản lý các thiết lập hệ thống, theo dõi lỗi và đảm bảo an toàn cho dữ liệu người dùng.
# **II. Triết lý và Nguyên tắc Thiết kế**
1. **Tính nhất quán (Consistency):** Giao diện và trải nghiệm người dùng (UI/UX) trên toàn bộ Admin Panel phải đồng nhất với hệ thống design system đã có của PronaFlow (kế thừa từ `style.css` và `base.css`). Điều này giúp quản trị viên (Admin) dễ dàng làm quen và thao tác hiệu quả.
2. **Tính tường minh (Clarity):** Mọi thông tin, số liệu và chức năng phải được trình bày một cách rõ ràng, trực quan. Quản trị viên phải nắm bắt được "sức khỏe" của hệ thống ngay từ cái nhìn đầu tiên.
3. **Bảo mật (Security):** Đây là yếu tố tiên quyết. Quyền truy cập vào Admin Panel phải được kiểm soát chặt chẽ, và mọi hành động của quản trị viên đều phải được ghi lại (audit log) để phục vụ cho việc kiểm tra và giám sát an ninh.
4. **Hiệu năng (Performance):** Với lượng dữ liệu lớn, các trang quản lý phải được tối ưu hóa về tốc độ tải và xử lý, đặc biệt là các chức năng hiển thị dữ liệu dạng bảng (data table) thông qua cơ chế phân trang (pagination).
# **III. Cấu trúc Hệ thống Admin Panel**
Admin Panel sẽ là một khu vực riêng biệt, có thể truy cập qua một đường dẫn `/admin/...`. 
Admin Panel sẽ được tổ chức theo kiến trúc module hóa, mỗi module chịu trách nhiệm cho một nhóm chức năng cụ thể.
**Sơ đồ Điều hướng (Admin Sidebar Navigation):**
- `/admin/dashboard`: **Trang Tổng Quan (Dashboard)** - Cung cấp cái nhìn toàn cảnh về hệ thống.
- `/admin/users`: **Quản lý Người dùng (User Management)** - Quản lý toàn bộ tài khoản người dùng.
- `/admin/workspaces`: **Quản lý Không gian làm việc (Workspace Management)** - Giám sát và quản lý các workspace.
- `/admin/projects`: **Quản lý Dự án (Project Management)** - Giám sát và quản lý các dự án.
- `/admin/logs`: **Nhật ký Hoạt động (System Activity Log)** - Ghi lại và truy xuất mọi hoạt động trên hệ thống.
- `/admin/settings`: **Cài đặt Hệ thống (System Settings)** - Khu vực dành cho các cấu hình nâng cao (tùy chọn).
# **III. Thiết kế Chi tiết cho Từng Trang Module**
## **1. Module: Admin Dashboard (Trang Tổng Quan)**
Trang này là trung tâm chỉ huy, cung cấp các số liệu thống kê quan trọng và truy cập nhanh đến các khu vực khác.

- **Mục đích:** Cung cấp một báo cáo tổng hợp, giúp quản trị viên nhanh chóng đánh giá tình hình hoạt động của nền tảng PronaFlow.
    
- **Các thành phần chính:**
    1. **Các thẻ Thống kê Nhanh (Stat Cards):**
        - **Tổng số Người dùng:** `SELECT COUNT(id) FROM users;`
        - **Dự án Đang hoạt động:** `SELECT COUNT(id) FROM projects WHERE is_deleted = FALSE AND is_archived = FALSE;`
        - **Tổng số Workspaces:** `SELECT COUNT(id) FROM workspaces;`
        - **Tổng số Tasks đã tạo:** `SELECT COUNT(id) FROM tasks;`
            
    2. **Biểu đồ "Tăng trưởng Người dùng mới":**
        - **Mục đích:** Trực quan hóa tốc độ tăng trưởng của nền tảng.
        - **Loại biểu đồ:** Biểu đồ đường (Line chart).
        - **Trục X:** Thời gian (Ngày, Tuần, Tháng).
		- **Trục Y:** Số lượng người dùng mới.
        - **Dữ liệu:** Dựa trên cột `created_at` trong bảng `users`, nhóm theo ngày/tuần/tháng (`SELECT DATE(created_at) as date, COUNT(id) FROM users GROUP BY DATE(created_at);`)
            
    3. **Bảng "Hoạt động Gần đây" (Live Feed):**
        - **Mục đích:** Hiển thị một feed trực tiếp các hành động quan trọng vừa diễn ra.
        - **Dữ liệu:** Truy vấn 10-15 bản ghi mới nhất từ bảng `activities`, kết hợp với bảng `users` để hiển thị tên người thực hiện.
        - **Ví dụ:** "Minh Trúc (`@minhtruc`) đã tạo dự án mới: _'Website Redesign 2025'_".
            
    4. **Bảng "Dự án mới tạo":**
        - **Mục đích:** Giúp admin theo dõi các dự án mới.
        - **Dữ liệu:** 5 dự án mới nhất từ bảng `projects`.
## **2. Module: User Management (Trang Quản lý Người dùng)**
Đây là trung tâm quản lý danh tính và quyền hạn của người dùng.

- **Mục đích:** Cho phép Admin thực hiện đầy đủ các thao tác CRUD (Tạo, Đọc, Cập nhật, Xóa) đối với tài khoản người dùng, đảm bảo an toàn và hỗ trợ khi cần thiết.
    
- **Các thành phần chính:**
    
    1. **Thanh Tìm kiếm và Bộ lọc:**
        - Tìm kiếm theo `full_name` hoặc `email`.
        - Lọc theo `role` ('user', 'admin') và trạng thái (`is_deleted`).
        
    2. **Bảng Dữ liệu Người dùng (Data Table):**
        - **Các cột:** ID, Họ tên (`full_name`), Email, Vai trò (`role`), Ngày tham gia (`created_at`), Trạng thái (Hoạt động / Đã xóa mềm).
        - **Dữ liệu:** Lấy từ bảng `users`.
            
    3. **Các Hành động Quản trị (Actions):**
        - **Xem chi tiết:** Mở một modal/trang riêng hiển thị toàn bộ thông tin người dùng, danh sách các workspace họ sở hữu và các dự án họ tham gia (dữ liệu từ `workspaces.owner_id` và `project_members`).
        - **Chỉnh sửa vai trò:** Cho phép thay đổi `users.role` từ `user` thành `admin` và ngược lại.
        - **Xóa mềm (Soft Delete):** Cập nhật `is_deleted = TRUE` và `deleted_at`. Giao diện sẽ có nút "Khôi phục" cho các tài khoản đã bị xóa mềm.
        - **Xóa vĩnh viễn (Permanent Delete):** Xóa hoàn toàn người dùng (yêu cầu xác nhận).
## **3.Module: Workspace & Project Management (Quản lý Không gian & Dự án)**
Admin có thể xem và quản lý tất cả các không gian làm việc và dự án, bất kể ai là người sở hữu.

- **Mục đích:** Đảm bảo việc sử dụng tài nguyên hợp lý, hỗ trợ người dùng trong các vấn đề liên quan đến quyền sở hữu và quản lý vòng đời của dự án.
    
- **Các thành phần chính:**
    1. **Giao diện dạng Bảng (Data Table) với khả năng tìm kiếm và lọc:**
        - **Các cột:** Tên Dự án (`projects.name`), Tên Workspace (`workspaces.name`), Chủ sở hữu Workspace (`users.full_name` từ `workspaces.owner_id`), Số lượng thành viên (đếm từ `project_members`), Trạng thái (`projects.status`), Ngày tạo.
    2. **Hành động Quản trị:**
        - **Xem chi tiết Dự án (Read-only):** Mở một giao diện tương tự modal chi tiết dự án của người dùng, nhưng admin không thể sửa nội dung (task, description) mà chỉ có thể thực hiện các hành động quản trị.
        - **Thay đổi Chủ sở hữu Workspace:** Cập nhật `workspaces.owner_id`.
        - **Lưu trữ / Hủy lưu trữ:** Thay đổi cờ `projects.is_archived`.
        - **Di chuyển vào Thùng rác / Xóa vĩnh viễn:** Quản lý vòng đời của dự án.
## **4. System Activity Log (Nhật ký Hoạt động Hệ thống)**

Đây là phiên bản mở rộng của trang Notifications, cung cấp một cái nhìn sâu và chi tiết hơn về mọi hành động.

- **Mục đích:** Cung cấp một bản ghi chi tiết, không thể thay đổi về mọi hành động diễn ra trong hệ thống, phục vụ cho việc kiểm toán (auditing), phân tích hành vi và gỡ lỗi (debugging).
    
- **Các thành phần chính:**
    1. **Bộ lọc Nâng cao:**
        - Lọc theo `user_id` (Người thực hiện).
        - Lọc theo `action_type` (Loại hành động).
        - Lọc theo `target_type` và `target_id` (Đối tượng bị tác động).
        - Lọc theo khoảng thời gian.
            
    2. **Dòng thời gian Hoạt động (Activity Timeline):**
        - Hiển thị chi tiết từng bản ghi trong bảng `activities`.
        - Display: `[Timestamp] User 'admin' (ID: 1) performed 'update_role' on User (ID: 25) to 'admin'`
        - **Ví dụ:** `[2025-10-12 14:30:00] User 'admin' (ID: 1) thực hiện 'change_status' trên Task (ID: 15) thành 'done'`.
        - Cung cấp các siêu liên kết (hyperlink) để điều hướng nhanh đến người dùng, tác vụ hoặc dự án liên quan.
### **IV. Cân nhắc về Kỹ thuật và UI/UX**

- **Tái sử dụng Design System:** Tất cả các thành phần như nút bấm (`.btn`), thẻ (`.card-style-box`), form (`.form-input`), modal (`.simple-modal`) sẽ kế thừa từ `style.css` và `base.css` để đảm bảo sự đồng nhất.
    
- **Phân trang (Pagination):** Các trang quản lý dạng bảng (User, Project) phải có cơ chế phân trang để xử lý lượng dữ liệu lớn một cách hiệu quả.
    
- **Phân quyền và Bảo mật:**
	- Tất cả các API endpoint cho Admin Panel phải được bảo vệ bằng một middleware, kiểm tra `users.role = 'admin'`.
	- Mọi hành động do Admin thực hiện (ví dụ: thay đổi vai trò người dùng, xóa dự án) phải được ghi lại trong bảng `activities` với một `action_type` đặc biệt, ví dụ `admin_update_user_role`.
    
- **Trực quan hóa Dữ liệu:** Trang Dashboard nên sử dụng các thư viện biểu đồ (như Chart.js, D3.js) để trình bày dữ liệu một cách sinh động và dễ hiểu.

- **Tối ưu hóa Trải nghiệm:**
	- Sử dụng cơ chế tải không đồng bộ (AJAX) để cập nhật dữ liệu trên các bảng mà không cần tải lại toàn bộ trang.
	- Các hành động nguy hiểm như "Xóa vĩnh viễn" luôn cần có hộp thoại xác nhận (confirmation modal) để ngăn ngừa các sai sót không đáng có.