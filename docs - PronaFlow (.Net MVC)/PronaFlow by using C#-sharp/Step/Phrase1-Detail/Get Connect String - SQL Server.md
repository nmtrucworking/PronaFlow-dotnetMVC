### **Cách 1: Sử Dụng "Server Explorer" (Cách Phổ Biến Nhất)**

Đây là cách trực quan và được nhiều người sử dụng nhất để kết nối và lấy chuỗi kết nối.

1. **Mở Server Explorer:**
    
    - Trên thanh menu của Visual Studio, chọn `View` -> `Server Explorer`.
        
    - (Hoặc sử dụng phím tắt: `Ctrl + Alt + S`).
        
2. **Tạo Kết Nối Mới:**
    
    - Trong cửa sổ Server Explorer, chuột phải vào **"Data Connections"** và chọn **"Add Connection..."**.
        
3. **Điền Thông Tin Kết Nối:**
    
    - Một cửa sổ "Add Connection" sẽ hiện ra.
        
    - **Data source**: Đảm bảo bạn đã chọn "Microsoft SQL Server". Nếu chưa, nhấn nút `Change...` để chọn.
        
    - **Server name**: Đây là phần quan trọng nhất.
        
        - Nếu bạn đang dùng SQL Server Express cài đặt trên máy local, tên server thường là `(localdb)\mssqllocaldb` hoặc `.\SQLEXPRESS`.
            
        - Bạn cũng có thể mở **SQL Server Management Studio (SSMS)** để xem và copy chính xác tên Server của mình.
            
    - **Log on to the server**:
        
        - Chọn **"Use Windows Authentication"** nếu bạn đăng nhập vào SQL Server bằng tài khoản Windows của mình (cách phổ biến khi phát triển trên máy cá nhân).
            
        - Chọn **"Use SQL Server Authentication"** nếu bạn đã tạo một tài khoản riêng (như `sa`) với username và password.
            
    - **Connect to a database**:
        
        - Trong mục "Select or enter a database name", bạn click vào dropdown. Nếu thông tin Server và Authentication ở trên đúng, Visual Studio sẽ liệt kê tất cả các database có trong server đó.
            
        - Chọn database của bạn (ví dụ: `PronaFlowDB`).
            
4. **Kiểm Tra và Lấy Connection String:**
    
    - Nhấn nút **"Test Connection"** để đảm bảo mọi thông tin bạn nhập đều chính xác. Nếu thành công, bạn sẽ nhận được thông báo "Test connection succeeded."
        
    - Nhấn **OK**. Bây giờ trong Server Explorer, dưới mục "Data Connections", bạn sẽ thấy kết nối tới database của mình.
        
    - Click chuột vào kết nối đó.
        
    - Mở cửa sổ **Properties** (`View` -> `Properties Window` hoặc phím tắt `F4`).
        
    - Trong cửa sổ Properties, tìm đến dòng **"Connection String"**. **Đây chính là chuỗi kết nối bạn cần!** Hãy copy toàn bộ giá trị của nó.
        

---

### **Cách 2: Sử Dụng "SQL Server Object Explorer"**

Cách này cũng rất tiện lợi nếu bạn đã quen làm việc với các đối tượng của SQL Server.

1. **Mở SQL Server Object Explorer:**
    
    - Trên thanh menu, chọn `View` -> `SQL Server Object Explorer`.
        
2. **Tìm Database của bạn:**
    
    - Cửa sổ này sẽ tự động liệt kê các instance SQL Server có trên máy của bạn.
        
    - Mở rộng cây thư mục: `SQL Server` -> `(localdb)\mssqllocaldb` (hoặc tên server của bạn) -> `Databases`.
        
    - Tìm và chuột phải vào database của bạn (ví dụ: `PronaFlowDB`).
        
3. **Lấy Connection String:**
    
    - Chọn **"Properties"** từ menu ngữ cảnh.
        
    - Một cửa sổ Properties sẽ hiện ra ở phía dưới.
        
    - Tìm đến mục **"Connection String"** và copy giá trị của nó.
        

---

### **Kết Quả Cuối Cùng**

Sau khi thực hiện một trong hai cách trên, chuỗi kết nối bạn nhận được sẽ có dạng tương tự như sau:

- **Với Windows Authentication:** `Data Source=(localdb)\mssqllocaldb;Initial Catalog=PronaFlowDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False`
    
- **Với SQL Server Authentication:** `Data Source=your_server_name;Initial Catalog=PronaFlowDB;User ID=your_user;Password=your_password;Connect Timeout=30;...`
    

Bây giờ bạn chỉ cần đặt chuỗi kết nối này vào lệnh `Scaffold-DbContext` hoặc file `appsettings.json` là xong.