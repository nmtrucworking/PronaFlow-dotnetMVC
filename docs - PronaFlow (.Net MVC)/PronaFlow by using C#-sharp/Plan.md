# **Giai Đoạn 1: Xây Dựng Nền Tảng - Core Models & Authentication**

Đây là giai đoạn cốt lõi, thiết lập "bộ xương" của ứng dụng. Mọi tính năng khác đều sẽ được xây dựng dựa trên nền tảng này.
1. **Thiết Lập Database (SQL Server):** 
    - Dựa vào tài liệu `Database - PronaFlow.md`, bạn tiến hành tạo tất cả 18 bảng đã được định nghĩa, từ `users`, `workspaces` cho đến các bảng nối như `project_members` và `task_assignees`.
    - **Lưu ý quan trọng:** Hãy đảm bảo thiết lập đúng các khóa chính, khóa ngoại, các ràng buộc `UNIQUE`, `DEFAULT` và đặc biệt là các mối quan hệ (`ON DELETE CASCADE` hoặc `ON DELETE RESTRICT`) như đã phân tích trong tài liệu.
	Phát triển giai đoạn này: [[Overview Database - PronaFlow]]
2. **Phát Triển API Quản Lý Người Dùng (Users) & Xác Thực (Authentication):**
    
    - **API Endpoints:** Triển khai các nghiệp vụ được mô tả trong `UserFlow - Users.md`.
        - `POST /api/register`: Đăng ký người dùng mới. Tại bước này, cần xử lý việc băm mật khẩu bằng một thuật toán mạnh như BCrypt.
        - `POST /api/login`: Đăng nhập và trả về token.
        - `PUT /api/profile`: Cập nhật thông tin cá nhân.
        - `POST /api/change-password`, `POST /api/forgot-password`, `POST /api/reset-password`: Triển khai đầy đủ luồng quản lý mật khẩu, bao gồm việc sử dụng bảng `password_resets`.
            
    - **Authentication:** Áp dụng cơ chế xác thực bằng JWT (JSON Web Tokens) như đã đề cập trong `UserFlow - More.md`. Server sẽ cấp `access_token` (ngắn hạn) và `refresh_token` (dài hạn) để duy trì phiên đăng nhập an toàn.
![[Cấu hình cài đặt project (VS).png]]
1. **Phát Triển API Quản Lý Không Gian Làm Việc (Workspaces):**
    
    - **Nghiệp vụ tự động:** Triển khai trigger hoặc logic ở tầng ứng dụng để tự động tạo một Workspace mặc định ngay sau khi một người dùng mới đăng ký thành công. Tên workspace có thể là `{user-name}'s Workspace`.
        
    - **API Endpoints:** Triển khai các nghiệp vụ CRUD từ tài liệu `UserFlow - Workspace.md`.
        
        - `POST /api/workspaces`: Tạo Workspace mới.
            
        - `PUT /api/workspaces/{workspace_id}`: Cập nhật thông tin Workspace.
            
        - `DELETE /api/workspaces/{workspace_id}`: Xóa Workspace, với logic kiểm tra nghiêm ngặt rằng workspace phải rỗng (không còn project nào) trước khi xóa.
            
[[Phrase1 - API]]
# **Giai Đoạn 2: Xây Dựng Tính Năng Cốt Lõi - Quản Lý Dự Án & Công Việc**

Sau khi người dùng có thể đăng nhập và có không gian làm việc, giai đoạn này tập trung vào các tính năng chính mà họ sẽ tương tác hàng ngày.

1. **Phát Triển API Quản Lý Dự Án (Projects):**
    
    - **API Endpoints:** Dựa trên `UserFlow - Projects.md`, triển khai các endpoints chính.
        
        - `POST /api/projects`: Tạo dự án mới. Logic cần tự động thêm người tạo vào `project_members` với vai trò 'admin' và ghi lại hoạt động `project_create`.
            
        - `PUT /api/projects/{project_id}`: Cập nhật các chi tiết dự án (tên, mô tả, ngày tháng).
            
        - `POST /api/projects/{project_id}/archive`: Lưu trữ dự án.
            
        - `PUT /api/projects/{project_id}/move`: Di chuyển dự án giữa các workspace.
            
        - `POST /api/projects/{project_id}/duplicate`: Nhân bản dự án, bao gồm cả việc sao chép `task_lists` và `tasks`.
            
2. **Phát Triển API Quản Lý Giai Đoạn (Task Lists / Phases):**
    
    - **API Endpoints:** Dựa trên `UserFlow - Task-list.md`, triển khai các nghiệp vụ liên quan đến các cột trong Kanban board.
        
        - `POST /api/projects/{project_id}/tasklists`: Tạo một giai đoạn mới.
            
        - `PUT /api/tasklists/{task_list_id}`: Cập nhật tên hoặc vị trí (`position`) của giai đoạn.
            
        - `DELETE /api/tasklists/{task_list_id}`: Xóa một giai đoạn, cần xử lý các công việc (`tasks`) bên trong nó.
            
3. **Phát Triển API Quản Lý Công Việc (Tasks):**
    
    - Đây là phần có nhiều nghiệp vụ phức tạp nhất, được mô tả chi tiết trong `UserFlow - Task.md`.
        
        - `POST /api/projects/{project_id}/tasks`: Tạo công việc mới.
            
        - `GET /api/tasks/{task_id}`: Lấy chi tiết một công việc, bao gồm cả `assignees`, `subtasks`, `attachments` và `comments` liên quan.
            
        - `PUT /api/tasks/{task_id}`: Cập nhật công việc (tên, mô tả, độ ưu tiên, deadline...).
            
        - `DELETE /api/tasks/{task_id}`: Xóa mềm công việc.
            
        - `POST /api/tasks/{task_id}/restore`: Khôi phục công việc từ thùng rác.
            
        - `PUT /api/tasks/{task_id}/move-to-tasklist`: Xử lý logic kéo-thả công việc giữa các giai đoạn.
            

#### **Giai Đoạn 3: Hoàn Thiện Tương Tác & Tính Năng Phụ Trợ**

Giai đoạn này tập trung vào việc làm cho ứng dụng trở nên "sống động" và hoàn chỉnh hơn.

1. **Quản Lý Thành Viên & Phân Công:**
    
    - Triển khai API để thêm/xóa thành viên khỏi dự án (`project_members`) và gán/bỏ gán người thực hiện cho công việc (`task_assignees`).
        
    - **Nghiệp vụ quan trọng:** Khi thành viên đầu tiên được thêm vào một dự án, tự động chuyển `projects.project_type` từ `'personal'` sang `'team'`.
        
2. **Hệ Thống Hoạt Động (Activities) & Thông Báo (Notifications):**
    
    - Đây là "bộ não" của hệ thống tương tác, được mô tả trong `UserFlow - Activities.md`.
        
    - Với mỗi hành động quan trọng (tạo task, gán việc, bình luận, thêm thành viên), hãy tạo một bản ghi tương ứng trong bảng `activities` với `action_type` phù hợp.
        
    - Dựa trên `action_type`, tạo các bản ghi trong `notification_recipients` để gửi thông báo đến những người dùng liên quan. Ví dụ: `action_type = 'task_assign'` sẽ tạo thông báo cho người được giao việc.
        
3. **Các Tính Năng Phụ Trợ:**
    
    - **Tags:** Triển khai API CRUD cho tags và API để gán/gỡ tag khỏi dự án, dựa trên `UserFlow - Tags.md`.
        
    - **Attachments:** Triển khai API cho phép tải lên, hiển thị và xóa tệp đính kèm, sử dụng mối quan hệ đa hình (`attachable_id`, `attachable_type`) như mô tả trong `UserFlow - Attachments.md`.
        
    - **Subtasks & Comments:** Triển khai các API endpoint để quản lý công việc con và bình luận.
        

#### **Giai Đoạn 4: Triển Khai Các Tính Năng Nâng Cao & Tối Ưu**

Đây là các tính năng phức tạp, có thể thực hiện sau khi các chức năng cốt lõi đã ổn định.

1. **Workflows Nâng Cao:**
    
    - **Recurring Tasks & Task Dependencies:** Triển khai logic cho công việc lặp lại và phụ thuộc giữa các công việc như trong `UserFlow - More.md`. Lưu ý, công việc lặp lại sẽ cần một tác vụ nền (cron job/scheduled task) để tự động tạo task mới.
        
2. **Tìm Kiếm, Lọc & Sắp Xếp:**
    
    - Xây dựng các API (ví dụ: `GET /api/my-tasks`) có khả năng nhận các tham số truy vấn (query parameters) như `sortBy`, `filterByProject`, `filterByAssignee` để phục vụ cho việc lọc và sắp xếp dữ liệu động.
        
3. **Phân Quyền & Bảo Mật:**
    
    - Xây dựng một lớp/module `Authorization` để kiểm tra quyền hạn trước mỗi hành động quan trọng, đảm bảo người dùng chỉ có thể tác động lên dữ liệu họ được phép. Ví dụ, `can_edit_project(user, project)`.
        
    - Triển khai vai trò 'admin' hệ thống có thể quản lý tất cả người dùng.
        

### **Lời Khuyên Về Cấu Trúc Code & Công Cụ**

- **Cấu Trúc Dự Án ASP.NET MVC:** Để chuyên nghiệp và dễ bảo trì, bạn nên phân tách project theo các lớp (layers) như:
    
    - **Presentation Layer (Controllers):** Chịu trách nhiệm nhận request và trả về response.
        
    - **Service Layer:** Chứa toàn bộ business logic (nghiệp vụ). Ví dụ, khi tạo một dự án, service sẽ thực hiện cả việc lưu vào bảng `projects`, thêm người dùng vào `project_members` và ghi `activity`.
        
    - **Repository/Data Access Layer:** Chịu trách nhiệm giao tiếp trực tiếp với CSDL (SQL Server).
        
- **Sử Dụng DTOs (Data Transfer Objects):** Đừng trả về trực tiếp các đối tượng Entity (model của CSDL) qua API. Hãy tạo các lớp DTO để định hình dữ liệu đầu ra, giúp API linh hoạt và bảo mật hơn.
    
- **Sử Dụng Insomnia:**
    
    - Hãy tạo các Collection trong Insomnia tương ứng với từng nhóm nghiệp vụ (Users, Workspaces, Projects, Tasks).
        
    - Sử dụng Environments để quản lý các biến như URL của server (local, staging, production) và `access_token` để tự động đính kèm vào header của các request cần xác thực.