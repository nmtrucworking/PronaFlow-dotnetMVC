Các bảng (tables - database) liên quan: `users` `projects` `workspace`
[[Database - PronaFlow]]
# 1. Nghiệp vụ Đăng ký Người dùng mới (User Registration)
Cho phép người dùng tạo tài khoản mới trong hệ thống.
## Frontend:
1. **Giao diện:** Người dùng truy cập trang "Đăng ký".
2. **Input:** Nhập `full_name`, `email`, `password`, `confirm_password`.
3. **Client-side Validation:** Kiểm tra định dạng `email`, độ dài mật khẩu, mật khẩu và xác nhận mật khẩu phải khớp.
4. **Action:** Nhấn nút "Đăng ký".
## Backend 
> ***API Endpoint***: `POST /api/register`

***Note***: {1} Server-side Validation:
Steps:
- {1} Kiểm tra tính hợp lệ của `email` (định dạng).
- {1} Kiểm tra email đã tồn tại trong bảng users chưa.
	- `exists`: trả về lỗi  #404-conflict (Email đã được sử dụng)
	- `not-exists`: tiếp tục luồng tạo tài khoản mới.
- Xử lý `Password`:
	- {1} Kiểm tra `password` có đủ mạnh không (độ dài, ký tự đặc biệt,v.v.).
	- Sử dụng hàm băm mạnh (`BCrypt`, `Argon2`) để băm `password` thành `password_hash`.
***Lưu vào Database***: Tạo một bản ghi mới trong bảng `users`.
	- `username`: (Có thể tạo tự động từ email hoặc để trống ban đầu, hoặc yêu cầu người dùng nhập)
	- `email`: Email người dùng cung cấp.
	- `password_hash`: Mật khẩu đã băm.
	- `full_name`: Tên đầy đủ người dùng.
	- `created_at`: CURRENT_TIMESTAMP.
	- Các trường khác (`avatar_url`, `bio`, `theme_preference`, `updated_at`, `role`) sẽ dùng giá trị mặc định hoặc NULL.
Tự động tạo Workspace mặc định: [[]]
Response:
	- Nếu thành công: Trả về #201-created cùng với tông tin user (trừ mật khẩu) và/hoặc token xác thực (ví dụ: JWT).
	- Nếu thất bại (validation, lỗi server): trả về #4xx hoặc #5xx với thông báo lỗi rõ ràng.
# 2. Nghiệp vụ Đăng nhập Người dùng (User login)
Cho phép người dùng truy cập tài khoản đã đăng ký
## **Frontend:**
1. **Giao diện:** Người dùng truy cập trang "Đăng nhập".
2. **Input:** Nhập email và password.
3. **Action:** Nhấn nút "Đăng nhập".
## Backend
> ***API Endpoint:*** `POST /api/login`
1. **Tìm kiếm User:** Tìm `user` trong bảng `users` dựa trên `email` cung cấp.
    - Nếu không tìm thấy: Trả về lỗi #401-Unauthorized (Thông tin đăng nhập không hợp lệ).
2. **Xác thực Mật khẩu:**
    - Sử dụng hàm kiểm tra mật khẩu đã băm (ví dụ: `bcrypt.compare()`) để so sánh `password` người dùng nhập với `password_hash` trong CSDL.
    - Nếu mật khẩu không khớp: Trả về lỗi #401-Unauthorized.
3. **Kiểm tra trạng thái tài khoản:**
    - Nếu `is_deleted = TRUE`: Trả về lỗi #403-Forbidden (Tài khoản đã bị xóa/vô hiệu hóa).
4. **Tạo phiên đăng nhập (Authentication):**
    - Nếu xác thực thành công: Tạo và cấp phát token xác thực (ví dụ: JWT `access_token` và `refresh_token`).
5. **Phản hồi (Response):**
    - Nếu thành công: Trả về #200-OK cùng với các token xác thực.
    - Nếu thất bại: Trả về #401-Unauthorized hoặc #403-Forbidden.
# 3. Nghiệp vụ Quản lý Hồ sơ Người dùng (User Profile Management)
Cho phép người dùng xem và Cập nhật thông tin cá nhân.
## Frontend
1. **Giao diện:** Trang "Profile Settings" hoặc "My Account".
2. **Input:** Form cho phép chỉnh sửa `full_name`, `avatar_url`, `bio`, `theme_preference`. (Thay đổi email/mật khẩu thường có luồng riêng).
3. **Action:** Nhấn nút "Lưu thay đổi".
## Backend
> ***API Endpoint:*** `PUT /api/users/{id}` hoặc `PUT /api/profile`
1. **Xác thực Token:** Đảm bảo request có `access_token` hợp lệ và user đang thực hiện hành động là chính chủ hoặc có quyền `admin`.
2. **Validation:** Kiểm tra dữ liệu mới có hợp lệ không (ví dụ: `avatar_url` là URL hợp lệ).
3. **Cập nhật Database:**
    - Tìm bản ghi `user` dựa trên `ID` của `user` đã xác thực.
    - Cập nhật các cột `full_name`, `avatar_url`, `bio`, `theme_preference`.
    - Cập nhật `updated_at` (tự động nếu có ràng buộc `ON UPDATE CURRENT_TIMESTAMP`).
4. **Phản hồi (Response):**
    - Nếu thành công: Trả về #200-OK cùng với thông tin user đã cập nhật.
    - Nếu thất bại: Trả về #4xx với thông báo lỗi.
# 4. Nghiệp vụ Thay đổi Mật khẩu (Change Password)
Cho phép người dùng thay đổi mật khẩu khi đang đăng nhập.
## Frontend
1. **Giao diện:** Trang "Change Password" trong cài đặt hồ sơ.
2. **Input:** Nhập `current_password`, `new_password`, `confirm_new_password`.
3. **Action:** Nhấn nút "Đổi mật khẩu".
## Backend
> ***API Endpoind***: `POST /api/change-password`
1. **Xác thực Token:** Đảm bảo user đã đăng nhập.
2. **Validation:**
    - Kiểm tra `new_password` và `confirm_new_password` khớp nhau.
    - Kiểm tra `new_password` có đủ mạnh không.
3. **Xác thực Mật khẩu hiện tại:**
    - Tìm user trong bảng users dựa trên ID của user đã xác thực.
    - So sánh `current_password` với `password_hash` trong CSDL. Nếu không khớp: Trả về lỗi #401-Unauthorized.
4. **Cập nhật Mật khẩu:**
    - Băm `new_password` thành `new_password_hash`.
    - Cập nhật `password_hash` trong bảng `users` với giá trị mới.
    - Cập nhật `updated_at`.
5. **Response:**
    - Nếu thành công: Trả về #200-OK (có thể yêu cầu đăng nhập lại để làm mới token).
    - Nếu thất bại: Trả về #4xx với thông báo lỗi.
# 5. Nghiệp vụ Quên mật khẩu (Forgot Password / Password Reset)
Cho phép người dùng lấy lại quyền truy cập khi quên mật khẩu.
## ***Forgot Password***
### Frontend
1. **Giao diện:** Trang "Quên mật khẩu".
2. **Input:** Nhập `email` của tài khoản.
3. **Action:** Nhấn nút "Gửi yêu cầu".
### Backend
> ***API Endpoind***: `POST /api/forgot-password`
1. **Validation:** Kiểm tra `email` có hợp lệ không.
2. **Tìm kiếm User:** Tìm `user` trong bảng `users` bằng `email`.
    - Nếu không tìm thấy hoặc tài khoản `is_deleted`: Có thể trả về thành công giả để tránh lộ thông tin user, hoặc lỗi.
3. **Tạo Reset Token:**
    - Tạo một `token` duy nhất, ngẫu nhiên (ví dụ: `UUID`).
    - Tính toán thời gian hết hạn (`expires_at`, ví dụ: 1 giờ kể từ bây giờ).
    - **Lưu vào bảng `password_resets`:**
        - `email`: Email người dùng.
        - `token`: Token vừa tạo.
        - `expires_at`: Thời gian hết hạn.
        - `created_at`: `CURRENT_TIMESTAMP`.
4. **Gửi `Email`:** Gửi một email đến `email` của người dùng, chứa một đường dẫn reset mật khẩu có gắn token (ví dụ: `https://pronaflow.com/reset-password?token=YOUR_TOKEN`).
5. **Phản hồi (Response):**
    - Trả về #200-OK thông báo đã gửi email (không tiết lộ nếu email không tồn tại).
        
## ***Step 2: Reset Password***
### **Frontend**
1. **Giao diện:** Người dùng nhấp vào link trong email, được chuyển đến trang "Đặt lại mật khẩu". URL chứa `token` (từ query param).
2. **Input:** Nhập `new_password`, `confirm_new_password`.
3. **Action:** Nhấn nút "Đặt lại mật khẩu".

### **Backend:**
>***API Endpoint***: `POST /api/reset-password`
1. **Lấy Token:** Nhận `token` từ request (query param hoặc body).
2. **Validation:**
    - Kiểm tra `new_password` và `confirm_new_password` khớp nhau.
    - Kiểm tra `new_password` có đủ mạnh không.
3. **Xác thực Token:**
    - Tìm `token` trong bảng `password_resets`.
    - Nếu không tìm thấy: Trả về lỗi #400-Bad_Request (Token không hợp lệ).
    - Kiểm tra `expires_at`: Nếu `CURRENT_TIMESTAMP` > `expires_at`: Trả về lỗi #400-Bad_Request (Token đã hết hạn).
4. **Cập nhật Mật khẩu:**
    - Tìm `user` trong bảng `users` bằng `email` từ bản ghi `password_resets` tìm được.
    - Băm `new_password` thành `new_password_hash`.
    - Cập nhật `password_hash` trong bảng `users`.
    - Cập nhật `updated_at`.
5. **Xóa Reset Token:**
    - Xóa bản ghi `token` khỏi bảng `password_resets` để ngăn chặn việc sử dụng lại.
6. **Phản hồi (Response):**
    - Nếu thành công: Trả về #200-OK (thông báo mật khẩu đã được đặt lại thành công, có thể chuyển hướng đến trang đăng nhập).
    - Nếu thất bại: Trả về #4xx với thông báo lỗi.
# 6. Nghiệp vụ Xóa Tài khoản (User Deletion)
## Frontend
1. **Giao diện:** Trang "Account Settings" (thường là một khu vực riêng biệt, cảnh báo rõ ràng).
2. **Input:** Có thể yêu cầu người dùng nhập lại mật khẩu hoặc xác nhận qua OTP/email để đảm bảo.
3. **Action:** Nhấn nút "Xóa tài khoản".
## Backend
> ***API Endpoind***: `DELETE /api/users/{id}` hoặc `DELETE /api/profile`
1. **Xác thực Token & Quyền:** Đảm bảo user đã đăng nhập và đang xóa tài khoản của chính họ. (Hoặc một `admin` đang xóa tài khoản khác).
2. **Validation (tùy chọn):** Yêu cầu xác nhận lại mật khẩu.
3. **Nghiệp vụ chuyển giao quyền sở hữu `project-team` (quan trọng):**
	- **Logic:** Trước khi cho phép xóa tài khoản, hệ thống **phải** kiểm tra xem `user` này có đang là `admin` trong bất kỳ `project_team` nào không.
    - **Nếu có:** Hệ thống sẽ yêu cầu người dùng chỉ định một `user_id` khác làm chủ sở hữu mới cho các `project-team` đó.
        - Đây sẽ là một bước riêng biệt trên frontend/backend, hoặc backend sẽ từ chối xóa và trả về lỗi 400 Bad Request với thông báo yêu cầu chuyển giao quyền.
        - Nếu có chuyển giao, bản ghi trong `project_members` sẽ được cập nhật `owner_id` hoặc `user_id` tương ứng.
4. **Soft-delete tài khoản:**
    - Cập nhật bản ghi user trong bảng users:
        - `is_deleted` = `TRUE`.
        - `deleted_at` = `CURRENT_TIMESTAMP`.
        - `updated_at` = `CURRENT_TIMESTAMP`.
    - **Vô hiệu hóa đăng nhập:** Khi `is_deleted = TRUE`, `user` đó không thể đăng nhập (đã xử lý trong nghiệp vụ Đăng nhập).
5. **Soft-delete tài nguyên liên quan:**
    - **Logic:** Tất cả các `projects` mà user này sở hữu (thông qua `owner_id` của `workspace` chứa `project` đó, hoặc nếu `project_type = 'personal'`), hoặc `tasks` do họ tạo/được giao, có thể cần được đánh dấu là `is_deleted = TRUE`.
    - **Thực hiện:**
        - Tìm tất cả `projects` thuộc về `workspaces` do `user` này sở hữu, và cập nhật 
	        - `projects.is_deleted = TRUE`, 
	        - `projects.deleted_at = CURRENT_TIMESTAMP`.
        - Tìm tất cả `tasks` mà `user` này là `assignee` hoặc `creator`, và cập nhật 
	        - `tasks.is_deleted = TRUE`, 
	        - `tasks.deleted_at = CURRENT_TIMESTAMP`.
        - **Lưu ý:** Nếu `project` đã được chuyển giao, thì không soft-delete project đó.
6. **Tác vụ nền (Cron Job):**
    - Một tác vụ nền định kỳ (ví dụ: hàng ngày, hàng tuần) sẽ quét bảng `users`, `projects`, `tasks`.
    - Nếu `is_deleted = TRUE` và `deleted_at` đã quá 30 ngày, tác vụ này sẽ **xóa vĩnh viễn (`hard delete`)** các bản ghi đó và tất cả các bản ghi con liên quan (`subtasks`, attachments, `comments`, `activity` liên quan đến `user_id` đó, v.v.) để giải phóng dung lượng CSDL. Điều này cần được xử lý cẩn thận với `ON DELETE CASCADE` ở cấp độ CSDL hoặc logic xóa tầng ứng dụng.
7. **Phản hồi (Response):**
    - Nếu thành công: Trả về #200-OK (thông báo tài khoản đã được xóa/vô hiệu hóa, có thể yêu cầu đăng xuất ngay lập tức).
    - Nếu thất bại: Trả về #4xx với thông báo lỗi (ví dụ: cần chuyển giao quyền sở hữu).
# 7. Nghiệp vụ Quản lý Vai trò Người dùng (User Role Management - cho Admin)
Cho phép admin hệ thống quản lý quyền hạn của người dùng.

## **Frontend (Chỉ Admin):**

1. **Giao diện:** Trang "Admin Dashboard" -> "User Management".
2. **Input:** Bảng danh sách người dùng với cột role có thể chỉnh sửa.
3. **Action:** Thay đổi `role` của một người dùng.
## **Backend:**
> ***API Endpoint***: `PUT /api/admin/users/{id}/role`
1. **Xác thực Quyền Admin:** Đảm bảo người thực hiện request có `role = 'admin'` trong bảng `users`.
2. **Validation:**
    - Kiểm tra `user_id` tồn tại.
    - Kiểm tra `new_role` hợp lệ (thuộc `ENUM('user', 'admin')`).
    - Ngăn chặn `admin` tự hạ cấp tài khoản của chính mình (tùy chọn).
3. **Cập nhật Database:**
    - Tìm `user` trong bảng `users` bằng `id`.
    - Cập nhật cột `role` với giá trị mới.
    - Cập nhật `updated_at`.
4. **Phản hồi (Response):**
    - Nếu thành công: Trả về #200-OK.
    - Nếu thất bại: Trả về #403-Forbidden (không có quyền), #404-Not_Found, hoặc #400-Bad_Request.