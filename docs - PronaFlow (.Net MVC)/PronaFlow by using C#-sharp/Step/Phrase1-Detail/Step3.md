Giả định rằng bạn đã có một solution với một project `PronaFlow.API` được tạo từ template "ASP.NET Core Web API".

#### **Bước 1: Tạo Project `PronaFlow.Core` (Class Library)**
Lớp này sẽ là "trái tim" của ứng dụng, chứa các định nghĩa cốt lõi.
1. Trong cửa sổ **Solution Explorer**, chuột phải vào **Solution 'PronaFlow.API'** (dòng trên cùng).
2. Chọn `Add` -> `New Project...`.
3. Trong hộp thoại "Add a new project", tìm và chọn template **"Class Library"**. Hãy chắc chắn bạn chọn đúng loại cho C#.
4. Nhấn **Next**.
5. Đặt tên project là `PronaFlow.Core`.
6. Nhấn **Next**.
7. Chọn cùng phiên bản Framework (.NET 8.0, .NET 7.0, etc.) với project `PronaFlow.API` của bạn.
8. Nhấn **Create**.
9. Visual Studio sẽ tạo project mới và một file `Class1.cs` mặc định. Bạn có thể xóa file này đi.
#### **Bước 2: Tạo Project `PronaFlow.Services` (Class Library)**
Lớp này sẽ là "bộ não", chứa toàn bộ logic nghiệp vụ.
1. Lặp lại quy trình tương tự như Bước 1.
2. Chuột phải vào **Solution** -> `Add` -> `New Project...`.
3. Chọn template **"Class Library"**.
4. Đặt tên project là `PronaFlow.Services`.
5. Chọn cùng phiên bản Framework.
6. Nhấn **Create** và xóa file `Class1.cs` mặc định.

Sau khi hoàn thành, Solution Explorer của bạn sẽ trông như thế này:

```bash
Solution 'PronaFlow.API' (3 projects)
|- PronaFlow.API
|- PronaFlow.Core
|- PronaFlow.Services
```
#### **Bước 3: Di Chuyển Các File Vào Đúng Vị Trí**
Lệnh `Scaffold-DbContext` trước đó đã tạo các file model và DbContext trong project `PronaFlow.API`. Bây giờ chúng ta sẽ chuyển chúng về đúng "nhà" của mình.

1. **Di chuyển Entities (Models):**
    - Trong project `PronaFlow.API`, tìm thư mục `Models` (thư mục này chứa các file như `User.cs`, `Project.cs`...).
    - **Kéo và thả (Drag and Drop)** toàn bộ thư mục `Models` này từ project `PronaFlow.API` vào project `PronaFlow.Core`.
    - Visual Studio sẽ tự động di chuyển file và cập nhật các file project liên quan.

2. **Di chuyển DbContext:**
    - File `PronaFlowDbContext.cs` cũng đang nằm trong project `PronaFlow.API`.
    - Để giữ cho `PronaFlow.Core` được gọn gàng, hãy tạo một thư mục mới bên trong nó tên là `Data`. Chuột phải vào `PronaFlow.Core` -> `Add` -> `New Folder`.
    - **Kéo và thả** file `PronaFlowDbContext.cs` từ `PronaFlow.API` vào thư mục `Data` vừa tạo trong `PronaFlow.Core`.
#### **Bước 4: Thiết Lập Tham Chiếu (Project References)**

Đây là bước cực kỳ quan trọng để tạo ra luồng phụ thuộc `API -> Services -> Core`.

1. **Thiết lập tham chiếu cho `PronaFlow.API`:**
    
    - Trong Solution Explorer, chuột phải vào project `PronaFlow.API`.
        
    - Chọn `Add` -> `Project Reference...`.
        
    - Trong hộp thoại "Reference Manager", tích vào ô `PronaFlow.Services`.
        
    - Nhấn **OK**.
        
    - _(Lưu ý: `PronaFlow.API` chỉ cần tham chiếu đến `Services`. Nó không cần biết đến `Core` vì `Services` sẽ là lớp trung gian)_.
        
2. **Thiết lập tham chiếu cho `PronaFlow.Services`:**
    
    - Chuột phải vào project `PronaFlow.Services`.
        
    - Chọn `Add` -> `Project Reference...`.
        
    - Trong hộp thoại "Reference Manager", tích vào ô `PronaFlow.Core`.
        
    - Nhấn **OK**.
        

Bây giờ, bạn đã thiết lập thành công chuỗi phụ thuộc. `PronaFlow.Services` không thể "thấy" được code trong `PronaFlow.API`, và `PronaFlow.Core` không thể "thấy" được code trong `PronaFlow.Services`, đảm bảo quy tắc kiến trúc được tuân thủ.

#### **Bước 5: Dọn Dẹp Namespace và `using` Statements**

Sau khi di chuyển file, namespace của chúng vẫn có thể là `PronaFlow.API.Models`. Chúng ta cần sửa lại cho đúng.

1. **Sửa Namespace:**
    
    - Mở một file bất kỳ trong thư mục `Models` đã di chuyển (ví dụ: `User.cs` trong `PronaFlow.Core`).
        
    - Sửa dòng `namespace PronaFlow.API.Models` thành `namespace PronaFlow.Core.Models`.
        
    - Làm tương tự cho tất cả các file model khác.
        
    - Mở file `PronaFlowDbContext.cs` (giờ đang ở `PronaFlow.Core/Data`). Sửa namespace của nó thành `namespace PronaFlow.Core.Data`.
        
2. **Sửa `using` Statements:**
    
    - Việc di chuyển file và sửa namespace sẽ gây ra lỗi biên dịch ở những nơi sử dụng chúng, đặc biệt là file `Program.cs`.
        
    - Mở file `Program.cs` trong project `PronaFlow.API`.
        
    - Bạn sẽ thấy `PronaFlowDbContext` bị báo lỗi gạch đỏ.
        
    - Thêm các dòng `using` cần thiết ở đầu file:
        
        C#
        
        ```
        using PronaFlow.Core.Data; // Để sử dụng PronaFlowDbContext
        using Microsoft.EntityFrameworkCore; // Để sử dụng .UseSqlServer()
        ```
        
    - Tương tự, khi bạn tạo các service và controller sau này, bạn sẽ cần thêm các `using` statement trỏ đến đúng namespace trong các project tương ứng.
        

### **Kết Quả Cuối Cùng**

Sau khi hoàn thành 5 bước trên, solution của bạn đã có một cấu trúc phân lớp sạch sẽ và chuyên nghiệp.

**Solution Explorer của bạn sẽ có cấu trúc như sau:**

```
Solution 'PronaFlow.API' (3 projects)
|- PronaFlow.API (Presentation Layer)
|  |- Controllers
|  |- appsettings.json
|  |- Program.cs
|
|- PronaFlow.Core (Core/Domain Layer)
|  |- Data
|  |  |- PronaFlowDbContext.cs
|  |- Models
|  |  |- User.cs
|  |  |- Project.cs
|  |  |- ... (các model khác)
|
|- PronaFlow.Services (Business Logic Layer)
```