Việc kết hợp giao diện (frontend) `pronaflow-web` (HTML, CSS, JS) với backend (`PronaFlow-api`) bao gồm 2 phần chính:
1. **Cấu hình Backend** để cho phép giao diện "nói chuyện" với nó (giải quyết vấn đề CORS).
2. **Viết mã JavaScript** ở phía giao diện để gọi các API, gửi và nhận dữ liệu.
### **Phần 1: Cấu Hình Backend - Cho Phép Giao Tiếp (CORS)**

Đây là **bước quan trọng nhất và là rào cản đầu tiên** bạn sẽ gặp phải.

#### **Vấn đề là gì? (CORS)**

Vì lý do bảo mật, các trình duyệt web mặc định sẽ chặn các yêu cầu JavaScript từ một "nguồn" (origin - ví dụ: `http://127.0.0.1:5500` nơi bạn chạy file HTML) đến một "nguồn" khác (ví dụ: `http://localhost:5226` nơi backend của bạn đang chạy). Cơ chế này được gọi là **CORS (Cross-Origin Resource Sharing)**.

Để việc kết hợp hoạt động, bạn phải "bảo" cho backend của mình rằng: _"Tôi cho phép các yêu cầu đến từ địa chỉ của frontend"_.

#### **Cách giải quyết trong `Program.cs`**

1. Mở file `Program.cs` trong project `PronaFlow.API`.
    
2. **Định nghĩa một Policy CORS:** Thêm đoạn code sau vào khu vực đăng ký dịch vụ (trước dòng `var app = builder.Build();`).
    ```C#
    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
                          policy =>
                          {
                              // Trong môi trường phát triển, chúng ta có thể cho phép mọi nguồn
                              // Khi deploy lên production, bạn nên chỉ định rõ nguồn của frontend
                              // policy.WithOrigins("http://127.0.0.1:5500", "https://your-frontend-domain.com")
                              policy.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          });
    });
    ```
    
3. **Sử dụng Policy CORS:** Thêm middleware `UseCors` vào pipeline xử lý request (sau `app.UseRouting()` và trước `app.UseAuthentication()`).
    
    C#
    
    ```
    var app = builder.Build();
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    // ...
    
    app.UseRouting(); // Thường thì UseRouting sẽ được gọi ngầm, nhưng để rõ ràng thì nó ở đây
    
    app.UseCors(MyAllowSpecificOrigins); // <-- THÊM DÒNG NÀY VÀO ĐÂY
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
    
    app.Run();
    ```
    

Sau khi thực hiện xong, backend của bạn đã sẵn sàng nhận request từ frontend.

---

### **Phần 2: Viết JavaScript để Gọi API**

Bây giờ ở phía `pronaflow-web`, bạn sẽ sử dụng JavaScript (cụ thể là `fetch` API) để tương tác với backend.

#### **Khái niệm cốt lõi:**

1. **Lưu trữ Token:** Sau khi người dùng đăng nhập thành công, bạn phải lưu chuỗi JWT mà backend trả về. Nơi lưu trữ phổ biến và an toàn nhất trên trình duyệt là `localStorage`.
    
2. **Gửi Token:** Với mỗi yêu cầu đến các API cần xác thực (như tạo workspace, project...), bạn phải đọc token từ `localStorage` và đính kèm nó vào `Header` của request.
    

#### **Ví dụ thực tế: Luồng Đăng Nhập**

Hãy xem một ví dụ hoàn chỉnh về cách xử lý form đăng nhập.

**Giả sử bạn có file HTML `login.html` với form sau:**

HTML

```
<form id="login-form">
    <input type="email" id="email" placeholder="Email" required>
    <input type="password" id="password" placeholder="Password" required>
    <button type="submit">Login</button>
    <div id="error-message" style="color: red;"></div>
</form>

<script src="login.js"></script>
```

**File `login.js` của bạn sẽ trông như sau:**

JavaScript

```
// Đặt địa chỉ gốc của API vào một biến để dễ thay đổi
const API_BASE_URL = 'http://localhost:5226/api'; // Thay port cho đúng với backend của bạn

// Lấy các element từ DOM
const loginForm = document.getElementById('login-form');
const errorMessageDiv = document.getElementById('error-message');

// Bắt sự kiện submit của form
loginForm.addEventListener('submit', async (event) => {
    // Ngăn form submit theo cách truyền thống (tải lại trang)
    event.preventDefault(); 
    
    errorMessageDiv.textContent = ''; // Xóa thông báo lỗi cũ

    // Lấy dữ liệu từ form
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    // Tạo body cho request
    const requestBody = {
        email: email,
        password: password
    };

    try {
        // Gọi API login bằng fetch
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        // Lấy dữ liệu JSON từ response
        const data = await response.json();

        if (!response.ok) {
            // Nếu server trả về lỗi (4xx, 5xx), ném ra lỗi để khối catch xử lý
            throw new Error(data.message || 'Login failed');
        }

        // --- ĐĂNG NHẬP THÀNH CÔNG ---
        console.log('Login successful!', data);

        // Lưu token vào localStorage
        localStorage.setItem('authToken', data.token);

        // Chuyển hướng người dùng đến trang dashboard (hoặc trang chính)
        window.location.href = '/dashboard.html';

    } catch (error) {
        // --- ĐĂNG NHẬP THẤT BẠI ---
        console.error('Error:', error);
        errorMessageDiv.textContent = error.message;
    }
});
```

#### **Ví dụ: Gọi API Cần Xác Thực (Lấy danh sách Workspaces)**

Sau khi đăng nhập và có token, để gọi một API được bảo vệ, bạn làm như sau:

JavaScript

```
async function getWorkspaces() {
    // 1. Đọc token từ localStorage
    const token = localStorage.getItem('authToken');

    if (!token) {
        console.error('No token found. Please login.');
        // Chuyển hướng về trang login nếu chưa có token
        window.location.href = '/login.html';
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/workspaces`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                // 2. Đính kèm token vào Header Authorization
                'Authorization': `Bearer ${token}` 
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to fetch workspaces');
        }

        const workspaces = await response.json();
        console.log('User workspaces:', workspaces);
        // Bây giờ bạn có thể dùng biến 'workspaces' để render ra giao diện
        // ví dụ: renderWorkspacesToUI(workspaces);

    } catch (error) {
        console.error('Error:', error);
    }
}

// Gọi hàm này khi trang dashboard được tải
getWorkspaces();
```

### **Quy Trình Tiếp Theo**

Bây giờ bạn có thể áp dụng các nguyên tắc trên để tích hợp toàn bộ các tính năng còn lại:

1. Hoàn thiện trang đăng ký và đăng nhập.
    
2. Sau khi đăng nhập, gọi API để lấy danh sách workspaces và projects, sau đó dùng JavaScript để render (hiển thị) chúng ra giao diện.
    
3. Gắn sự kiện `click` cho các nút "Add Project", "Delete Task"... để gọi đến các API `POST`, `DELETE` tương ứng.
    
4. Sau mỗi hành động (tạo, sửa, xóa), hãy gọi lại API lấy danh sách để cập nhật giao diện với dữ liệu mới nhất.