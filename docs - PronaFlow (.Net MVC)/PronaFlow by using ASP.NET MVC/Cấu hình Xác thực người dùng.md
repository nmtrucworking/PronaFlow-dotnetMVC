```bash
filePath: d:\workspace\3-Programming\Workspace\Web\project\pronaflow\PronaFlow_MVC\Web.config
``` 
          
**Tổng Quan**
- Kiểu xác thực: Forms Authentication (ASP.NET MVC, .NET 4.7.2).
- Mức bảo vệ: Toàn cục, mọi action đều bị chặn trừ khi đánh dấu `[AllowAnonymous]`.

**Cấu Hình**
- `Web.config:20–22` cấu hình `<authentication mode="Forms">` với `loginUrl="~/Home/Index"`, `timeout=60`, `slidingExpiration=true`.
- Global filters: `App_Start/FilterConfig.cs:11–14` đăng ký `AuthorizeAttribute` toàn cục, `HandleErrorAttribute`, và `CurrentUserFilter`.
- Đăng ký filters: `Global.asax.cs:16`.

**Luồng Đăng Nhập**
- View GET:
  - `AccountController.cs:37–42` `GET /Account/Login` hiển thị form đăng nhập.
  - `AccountController.cs:47–52` `GET /Account/Register` hiển thị form đăng ký.
  - `HomeController.cs:16–19` `GET /Home/Index` cho phép ẩn danh (landing).
- Xử lý POST:
  - `AccountController.cs:65–96` `POST /Account/Login` có `[ValidateAntiForgeryToken]`, kiểm tra user, PBKDF2 verify, đặt cookie `FormsAuthentication.SetAuthCookie`, chuyển hướng `Dashboard`.
  - `AccountController.cs:108–157` `POST /Account/Register` có `[ValidateAntiForgeryToken]`, tạo user, PBKDF2 hash, đặt cookie, chuyển hướng `Dashboard`.
- Đăng xuất:
  - `AccountController.cs:100–106` `Logout` có `[Authorize]`, gọi `FormsAuthentication.SignOut()`.

**Bảo Vệ Controllers**
- Toàn bộ controllers mặc định bị chặn bởi `Authorize` toàn cục.
- Cho phép ẩn danh: chỉ `Account` (Login/Register/Forgot/Reset) và `Home/Index`.
- Ví dụ:
  - `KanbanboardController.cs:22–58` không gắn `[AllowAnonymous]` → bị bảo vệ; còn có kiểm tra `User.Identity.Name` và redirect login nội bộ.
  - `ProjectController.cs` có các helper `AuthorizeUser/AuthorizeForProject` (bổ sung kiểm tra sở hữu workspace/project) và không gắn `[AllowAnonymous]` → bị bảo vệ.
  - `DashboardController.cs` có thể thêm `[Authorize]`, nhưng hiện tại đã được bảo vệ bởi filter toàn cục, nên là dư thừa.

**Tiện Ích Hiện Tại**
- `CurrentUserFilter.cs:14–22` lấy `User.Identity.Name` (email), nạp `ViewBag.CurrentUser` từ DB sau mỗi action, hữu ích cho view/layout.

**Đánh Giá Bảo Mật**
- Cookie đăng nhập: dùng FormsAuth mặc định; đặt qua `SetAuthCookie`. Chưa cấu hình thêm bảo mật trong `<forms>` (ví dụ `requireSSL`, `cookieless`, `cookieSameSite`, `protection`).
- Hash mật khẩu: PBKDF2/SHA256 với 10k iterations (`AccountController.cs:162–177`); có fallback legacy (`VerifyPassword`:205–207).
- Chống CSRF: Login/Register có `[ValidateAntiForgeryToken]`.
- Phân quyền vai trò: chưa áp dụng `[Authorize(Roles = "...")]`; `role` có trong user (được set “user” khi đăng ký), chưa được dùng trong auth ticket.

**Vấn Đề Tiềm Ẩn**
- `loginUrl` hiện là `~/Home/Index`, nhưng form đăng nhập đầy đủ nằm ở `~/Account/Login`. Người dùng bị redirect khi chưa đăng nhập sẽ tới trang Home (có thể không có form), gây lệch trải nghiệm.
- Một số controller tự redirect `RedirectToAction("Login","Account")` trong logic; Forms Auth mặc định redirect về `Home/Index`, có thể tạo luồng không thống nhất.

**Khuyến Nghị**
- Đồng bộ điểm vào đăng nhập:
  - Cập nhật `Web.config` `forms loginUrl` thành `~/Account/Login` để thống nhất redirect khi chưa đăng nhập.
- Tăng cường bảo mật cookie:
  - Thiết lập `requireSSL="true"` (production, bắt buộc HTTPS), `cookieless="UseCookies"`, `cookieSameSite="Lax"` hoặc `Strict` tùy nghiệp vụ, và xem xét `protection="All"`.
  - Cấu hình `machineKey` cố định cho multi-server, `validation="SHA256"`.
- Nâng sức kháng hash:
  - Tăng `iterations` PBKDF2 lên 100k–300k nếu hiệu năng cho phép; chuẩn hóa loại bỏ fallback plaintext sau giai đoạn chuyển đổi.
- Phân quyền:
  - Nếu cần phân vai trò, lưu `role` vào `FormsAuthenticationTicket.UserData` hoặc kiểm tra từ DB trong custom authorize; dùng `[Authorize(Roles = "...")]` sau khi có provider phù hợp.

Nếu bạn muốn, tôi có thể thực hiện bước đổi `loginUrl` sang `~/Account/Login` để thống nhất luồng xác thực và đề xuất cấu hình cookie an toàn trong `Web.config`.