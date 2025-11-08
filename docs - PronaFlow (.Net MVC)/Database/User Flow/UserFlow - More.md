# III. Nghiệp Vụ Tìm Kiếm, Lọc & Sắp Xếp
Giao diện của bạn có các thanh tìm kiếm và nút lọc/sắp xếp, nhưng nghiệp vụ backend chưa được định nghĩa.
## **4. Logic Tìm Kiếm Toàn Diện (Search)**
*   **Vấn đề/Nhu cầu:** Khi người dùng gõ vào thanh tìm kiếm trên trang "My Tasks", họ mong đợi kết quả trả về không chỉ dựa trên tên task, mà còn cả mô tả, tên dự án, v.v.
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   Xây dựng API tìm kiếm linh hoạt, có khả năng tìm kiếm trên nhiều trường (`tasks.name`, `tasks.description`, `projects.name`).
    *   **Nâng cao:** Để đạt hiệu suất cao với lượng dữ liệu lớn, hãy sử dụng **Full-Text Search** của hệ quản trị CSDL (ví dụ: `tsvector` trong PostgreSQL) hoặc tích hợp một công cụ tìm kiếm chuyên dụng như Elasticsearch.

## **5. Logic Lọc và Sắp Xếp (Filtering & Sorting)**
*   **Vấn đề/Nhu cầu:** Giao diện cho phép lọc theo dự án, người được giao và sắp xếp theo ngày hết hạn, độ ưu tiên.
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   API lấy danh sách công việc (ví dụ: `/api/my-tasks`) phải chấp nhận các tham số (query parameters) như:
        *   `sortBy=priority&sortDir=desc`
        *   `filterByProject=5,10`
        *   `filterByAssignee=1,2`
    *   Backend sẽ dựa vào các tham số này để xây dựng câu lệnh SQL `WHERE`, `ORDER BY` một cách động.
# IV. Nghiệp Vụ Các Tính Năng Mở Rộng

## **6. Hệ Thống Bình Luận (Commenting System)**

*   **Vấn đề/Nhu cầu:** Bảng `activities` có `action_type = 'comment_add'` nhưng chưa có bảng `comments`.
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   Tạo bảng `comments` (tương tự bảng `attachments`, sử dụng mối quan hệ đa hình).

## **7. Tùy Chỉnh Của Người Dùng (User Preferences)**

*   **Vấn đề/Nhu cầu:** Bảng `users` chỉ có `theme_preference`. Nhưng trang Settings còn nhiều tùy chỉnh khác (ví dụ: cài đặt thông báo).
*   **Giải pháp/Nghiệp vụ đề xuất (linh hoạt):**
    *   Tạo bảng mới `user_preferences`.
*   **Lợi ích:** Bạn có thể thêm bất kỳ cài đặt nào trong tương lai mà không cần phải thay đổi cấu trúc bảng `users`.

# I. Nghiệp Vụ Liên Quan Đến Bảo Mật và Phân Quyền
Những nghiệp vụ này đảm bảo rằng người dùng chỉ có thể thấy và hành động trên những dữ liệu mà họ được phép.
## **8. Chính Sách Ủy Quyền (Authorization Policies)**

*   **Vấn đề/Nhu cầu:** Cấu trúc DB cho biết ai thuộc dự án nào, nhưng logic "ai được làm gì" cần phải được định nghĩa rõ ở tầng ứng dụng (backend).
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   Xây dựng một lớp/module `Authorization` để kiểm tra quyền trước mỗi hành động. Ví dụ:
        *   **`can_edit_project(user, project)`:** Hàm này sẽ kiểm tra xem `user` có phải là `owner` của `workspace` chứa `project` đó không, hoặc có vai trò `admin` trong `project_members` hay không.
        *   **`can_delete_task(user, task)`:** Kiểm tra xem `user` có phải là người tạo ra `task`, người được giao `task`, hay là `admin` của dự án chứa `task` đó không.
        *   **`can_view_workspace(user, workspace)`:** Kiểm tra xem `user` có phải là `owner` của `workspace` hay không.
    *   Mỗi API endpoint (ví dụ: `DELETE /api/tasks/{id}`) phải gọi đến các hàm kiểm tra quyền này trước khi thực hiện bất kỳ thao tác nào với CSDL. Nếu không có quyền, API phải trả về lỗi `403 Forbidden`.

## **9. Quản Lý Phiên Đăng Nhập và Xác Thực (Authentication & Session Management)**

*   **Vấn đề/Nhu cầu:** Làm thế nào để duy trì trạng thái đăng nhập của người dùng một cách an toàn? Làm thế nào để xử lý "Quên mật khẩu"?
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   **Xác thực:** Sử dụng JWT (JSON Web Tokens). Khi người dùng đăng nhập thành công, server sẽ cấp cho họ một `access_token` (có thời hạn ngắn) và một `refresh_token` (có thời hạn dài).
        *   `access_token` được gửi kèm trong header của mỗi request để xác thực.
        *   Khi `access_token` hết hạn, frontend sẽ dùng `refresh_token` để tự động lấy `access_token` mới mà không bắt người dùng đăng nhập lại.
    *   **Quên mật khẩu:**
        1.  Tạo bảng mới `password_resets` (`email`, `token`, `expires_at`).
        2.  Khi người dùng yêu cầu reset, tạo một `token` duy nhất, lưu vào bảng và gửi link reset có chứa token qua email.
        3.  Khi người dùng nhấp vào link, xác thực `token` và cho phép họ đặt mật khẩu mới.
# II. Nghiệp Vụ Tối Ưu Hóa Trải Nghiệm Người Dùng (UX)

## **10. Trạng Thái Offline và Đồng Bộ Hóa (Offline Mode & Synchronization)**

*   **Vấn đề/Nhu cầu:** Người dùng muốn tiếp tục làm việc ngay cả khi kết nối mạng chập chờn hoặc bị mất.
*   **Giải pháp/Nghiệp vụ đề xuất (phức tạp, dành cho giai đoạn sau):**
    *   **Frontend:** Sử dụng các công nghệ như Service Workers và IndexedDB để lưu trữ một bản sao của dữ liệu (dự án, công việc) trên trình duyệt.
    *   **Logic:**
        1.  Khi có mạng, ứng dụng sẽ đồng bộ dữ liệu từ server về IndexedDB.
        2.  Khi mất mạng, ứng dụng sẽ đọc và ghi trực tiếp vào IndexedDB. Mọi thay đổi (tạo task, đổi trạng thái) sẽ được lưu vào một "hàng đợi" (queue).
        3.  Khi có mạng trở lại, ứng dụng sẽ gửi toàn bộ các thay đổi trong hàng đợi lên server để đồng bộ.
        4.  Cần có cơ chế xử lý xung đột (conflict resolution), ví dụ: nếu hai người cùng sửa một task khi offline.

## **11. Tùy Chỉnh Chế Độ Xem (View Customization)**

*   **Vấn đề/Nhu cầu:** Trang Kanban board có 5 cột cố định. Người dùng có thể muốn thêm/bớt/đổi tên các cột này cho phù hợp với quy trình của họ.
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   **Bảng `task_lists` đã hỗ trợ điều này!** Đây chính là nơi lưu trữ các cột của Kanban board cho từng dự án.
    *   **Hoàn thiện nghiệp vụ:**
        *   Cung cấp giao diện cho phép `admin` của dự án có thể:
            *   Thêm một bản ghi mới vào `task_lists` (tạo cột mới).
            *   Cập nhật `name` của một bản ghi (đổi tên cột).
            *   Xóa một bản ghi (xóa cột - cần xử lý các `task` đang nằm trong cột đó, ví dụ: di chuyển chúng sang cột đầu tiên).
            *   Thay đổi `position` của các bản ghi (kéo-thả để sắp xếp lại thứ tự các cột).
# III. Nghiệp Vụ Vận Hành & Bảo Trì

## **12. Ghi Log Hệ Thống (System Logging)**

*   **Vấn đề/Nhu cầu:** Khi có lỗi xảy ra (ví dụ: API trả về lỗi 500), làm thế nào để lập trình viên biết được nguyên nhân để sửa lỗi?
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   Tích hợp một thư viện ghi log chuyên dụng (như Monolog trong PHP, Winston trong Node.js).
    *   Ghi lại tất cả các lỗi nghiêm trọng, các request API bất thường, hoặc các sự kiện quan trọng vào một tệp log hoặc một dịch vụ quản lý log tập trung (như Sentry, LogDNA).
    *   Bảng `activities` chỉ ghi lại hoạt động của người dùng, còn log hệ thống ghi lại hoạt động của chính ứng dụng.

## **13. Quản Lý Quyền Hạn Ở Cấp Độ Toàn Hệ Thống (Super Admin)**

*   **Vấn đề/Nhu cầu:** Cần có một vai trò cao nhất để quản lý toàn bộ hệ thống, xem thống kê, xử lý các tài khoản vi phạm.
*   **Giải pháp/Nghiệp vụ đề xuất:**
    *   Thêm một cột `role` vào bảng `users`: `ENUM('user', 'admin')`.
    *   Tạo một trang quản trị riêng chỉ dành cho các tài khoản có `role = 'admin'`. Từ trang này, họ có thể:
        *   Xem danh sách tất cả người dùng, workspaces.
        *   Vô hiệu hóa một tài khoản người dùng.
        *   Xem các báo cáo, thống kê toàn hệ thống.