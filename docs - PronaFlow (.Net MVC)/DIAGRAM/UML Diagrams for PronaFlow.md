# 1. Sơ đồ mô tả Chức năng và Yêu cầu (Function View) 
## Sơ đồ Ca sử dụng ( #Use-Case-Diagram)
- Mục đích: Mô tả các chức năng chính của hệ thống từ góc nhìn của người dùng (Actors). 
- **Trả lời cho câu hỏi**: "*Hệ thống làm gì?*"
Chi tiết Use Case Diagram: [[Use Case Diagram for PronaFlow System]]
# 2. Sơ đồ mô tả Cấu trúc Tĩnh (Static/Structural View)
Mô tả các thành phần cố định của hệ thống, không phụ thuộc vào thời gian.
## Sơ đồ lớp ( #Class-Diagram)
- Mục đích: Mô tả cấu trúc các
	- lớp (classes), 
	- thuộc tính (attributes), 
	- phương thức (operations), 
	- mối quan hệ (relationships) 
--> Một trong những sơ đồ quan trọng nhất.
>[!NOTE] Dựa theo source:
> - **Mô hình hóa Entities (Domain Model):** Bạn có thể tạo một sơ đồ lớp chi tiết cho các thực thể trong `PronaFlow.Core/Models/` như `User.cs`, `Project.cs`, `PronaTask.cs`, `TaskList.cs`, `Workspace.cs`, v.v. Sơ đồ này sẽ trực quan hóa các mối quan hệ (ví dụ: một `Project` có nhiều `TaskList`, một `TaskList` có nhiều `PronaTask`, một `User` có thể thuộc nhiều `ProjectMember`).
> - **Mô hình hóa Thiết kế (Design Model):** Một sơ đồ lớp khác có thể mô tả mối quan hệ giữa các thành phần trong kiến trúc 3 lớp của bạn: `ProjectController` (trong `.API`) phụ thuộc vào `IProjectService` (trong `.Core/Interfaces`), và `ProjectService` (trong `.Services`) triển khai `IProjectService` và phụ thuộc vào `PronaFlowDbContext`.

Chi tiết Class Diagram: [[Class Diagram for PronaFlow System]]
## Sơ đồ gói ( #Package-Diagram)
- Mục đích: Tổ chức các thành phần của hệ thống thành các thành nhóm (gói) và chỉ ra sự phụ thuộc (dependencies) giữa chúng.
- Mô tả kiến trúc tổng thể. 
- Các gói (`packages`) chính là các `project` trong `solution`: 
	- `PronaFlow.API`
	- `PronaFlow.Services`
	- `PronaFlow.Core`

Chi tiết Package Diagram: [[Package Diagram for PronaFlow System]]
## Sơ đồ Thành phần ( #Component-Diagram)
- Mục đích: Mô tả cách hệ thống được chia thành các thành phần (components) và các giao diện (interfaces) mà chúng cung cấp hoặc sử dụng.
- Mô tả kiến trúc ở mức cao hơn. 
- Ví dụ các thành phần trong Component Diagram:
	- `PronaFlow API` (Component cung cấp REST API)
	- `PronaFlow Frontend` (Component JavaScript trong `wwwroot/` sử dụng API)
	- `Database` (Component lưu trữ dữ liệu)
- Mô tả rõ giao diện (VD: REST API) mà `PronaFlow API` cung cấp và `PronaFlow Frontend` sử dụng

Chi tiết Component Diagram: [[Component Diagram for PronaFlow System]]

# 3. Sơ đồ mô tả Hành vi Động (Dynamic Behaviral View)
Mô tả hệ thống thay đổi và tương tác như thế nào ***theo thời gian***.
## Sơ đồ Tuần tự ( #Sequence-Diagram)
**Mục đích:** Mô tả sự tương tác (luồng thông điệp) giữa các đối tượng theo một trình tự thời gian cụ thể. Rất tốt để làm rõ một ca sử dụng.
**Lý do & Dẫn chứng:** Cực kỳ hữu ích để mô tả chi tiết một luồng xử lý request. Ví dụ, bạn có thể vẽ sơ đồ tuần tự cho ca sử dụng "Người dùng tạo một Task mới":     
1. `User` (gửi request từ `apiService.js`) -> `TaskController` (`CreateTask` method)
2. `TaskController` -> `ITaskService` (`CreateTaskAsync` method)
3. `TaskService` -> `PronaFlowDbContext` (tạo đối tượng `PronaTask`)
4. `TaskService` -> `IActivityService` (tạo log hoạt động)
5. `TaskService` (trả về `TaskDto`) -> `TaskController`
6. `TaskController` (trả về `OkResult` với DTO) -> `User`
Chi tiết Package Diagram: [[Sequence Diagram for PronaFlow System]]
## Sơ đồ Hoạt động ( #Activity-Diagram)
    
- **Mục đích:** Mô tả các luồng công việc (workflows) hoặc các bước logic của một quy trình nghiệp vụ phức tạp, tương tự như lưu đồ (flowchart).

- **Lý do & Dẫn chứng:** Phù hợp để mô tả các logic nghiệp vụ có nhiều điều kiện rẽ nhánh. Ví dụ:

- Quy trình "Đăng nhập" (`AuthController.cs`): Bắt đầu -> Nhận `UserForLoginDto` -> Kiểm tra người dùng tồn tại -> Kiểm tra mật khẩu (hashing) -> Nếu sai, trả về `Unauthorized` -> Nếu đúng, tạo JWT Token -> Trả về Token.
	
- Quy trình di chuyển một task (`TaskController` - `UpdateTaskOrder`): Mô tả logic cập nhật thứ tự (order) của task và có thể cả `TaskList` của nó.
Chi tiết Activity Diagram: [[Activity Diagram for PronaFlow System]]
## Sơ đồ Trạng thái ( #State-Machine-Diagram)
- **Mục đích:** Mô tả các trạng thái (states) khác nhau của một đối tượng và các sự kiện (events) gây ra sự chuyển đổi (transitions) giữa các trạng thái đó.
- **Lý do & Dẫn chứng:** Rất phù hợp cho các đối tượng có vòng đời phức tạp. Trong dự án của bạn, đối tượng `PronaTask` (trong `PronaTask.cs`) là ứng cử viên sáng giá. Một Task có thể có các trạng thái như: "Mới tạo (To-do)", "Đang tiến hành (In Progress)", "Đang xem xét (In Review)", "Hoàn thành (Done)". Sơ đồ sẽ chỉ rõ các hành động (ví dụ: "Assign user", "Set due date", "Mark complete") làm thay đổi trạng thái của task.
Chi tiết State Machine Diagram: [[State Machine Diagram for PronaFlow System]]
# Triển khai
| N.O. | Type of Diagram   | Links                                     |
| ---- | ----------------- | ----------------------------------------- |
| 1    | #Use-Case-Diagram | [[Use Case Diagram for PronaFlow System]] |
| 2    | #Class-Diagram    | [[Class Diagram for PronaFlow System]]    |
| 3    | #Sequence-Diagram | [[Sequence Diagram for PronaFlow System]] |
| 4    | #Package-Diagram  | [[Package Diagram for PronaFlow System]]  |
| 5    | #Activity-Diagram | [[Activity Diagram for PronaFlow System]] |
Để mô tả website của bạn một cách toàn diện, tôi khuyến nghị bắt đầu với bộ ba sơ đồ cốt lõi sau:
1. **Sơ đồ Ca sử dụng (Use Case Diagram):** Để xác định _hệ thống làm gì_ (phạm vi chức năng).
    
2. **Sơ đồ Lớp (Class Diagram):** Để mô tả _cấu trúc dữ liệu_ (các `Models` trong `.Core`) và _cấu trúc thiết kế_ (mối quan hệ giữa Controller-Service-Repository).
    
3. **Sơ đồ Tuần tự (Sequence Diagram):** Để mô tả _hệ thống làm như thế nào_ cho một vài ca sử dụng quan trọng nhất (ví dụ: Đăng nhập, Tạo Task, Thêm thành viên vào Project).
    

Sau đó, bạn có thể bổ sung:

- **Sơ đồ Gói (Package Diagram)** hoặc **Sơ đồ Thành phần (Component Diagram)** để trình bày kiến trúc tổng thể.
    
- **Sơ đồ Hoạt động (Activity Diagram)** cho các quy trình nghiệp vụ phức tạp.