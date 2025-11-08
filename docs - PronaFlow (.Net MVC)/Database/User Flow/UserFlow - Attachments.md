## Mô tả luồng nghiệp vụ (User Flow).
### 1. Luồng Tải lên - ***Upload Flow***.
1. **Giao diện**: Trong modal chi tiết dự án, user (user_id) nhấn vào nút "Attachment". Một cửa sổ chọn tệp hiện ra.
2. **Actions**: User chọn một tệp có lên `file-name.pdf`
3. **Send `request` (API Request)**: Frontend gửi mộy yêu cầu #POST đến backend (ví dụ: `/api/attachments`) cùng với dữ liệu tệp và thông tin ngữ cảnh.
	- `file_data`: Dữ liệu của tệp `file-name.pdf`.
	- `attachable_id`: <- ID của project trực thuộc.
	- `attachable_type`: `project`.
4. ***Xử lý Backend***:
	a. Backend nhận tệp, tạo một file duy nhất để lưu trữ (ví dụ: `uuid_timestamp_file-name.pdf`) để tránh trùng lặp file.
	b. Tải tệp lên nơi lưu trữ (thư mục trên server, Amazon S3, GG Cloud Storage) và lấy về đường nhấn `storage_path`.
	c. Lưu một bản ghi mới vào database: bảng `attachments`.
*Ví dụ Database lưu trữ:*

| Cột                   | Giá trị                                         |
| --------------------- | ----------------------------------------------- |
| `id`                  | 23 (tự tăng)                                    |
| `uploaded_by_user_id` | 1 (Trúc)                                        |
| `attachable_id`       | 5                                               |
| `attachable_type`     | `'project'`                                     |
| `original_filename`   | `'design_brief_v2.pdf'`                         |
| `storage_path`        | `'/uploads/uuid_timestamp_design_brief_v2.pdf'` |
| `file_type`           | `'application/pdf'`                             |
| `file_size`           | 204800 (ví dụ: 200 KB)                          |
	d. Tạo Activity: Hệ thống tạo một bản ghi hoạt động.

| Cột                   | Giá trị                                         |
| --------------------- | ----------------------------------------------- |
| `id`                  | 23 (tự tăng)                                    |
| `uploaded_by_user_id` | 1 (Trúc)                                        |
| `attachable_id`       | 5                                               |
| `attachable_type`     | `'project'`                                     |
| `original_filename`   | `'design_brief_v2.pdf'`                         |
| `storage_path`        | `'/uploads/uuid_timestamp_design_brief_v2.pdf'` |
| `file_type`           | `'application/pdf'`                             |
| `file_size`           | 204800 (ví dụ: 200 KB)                          |
	e. Tạo Notifications (Đối với `project team`): Hệ thống gửi thông báo cho các thàn viên khác trong dự án.
5. `Response` Frontend: Backend trả về một thông tin của tệp vừa tạo (dưới dạng JSON). Fronten nhận được và ngay lập tức hiển thị tệp `file-name.pdf` trong danh sách tệp đính kèm của dự án mà không cần tải lại trang.
## 2. Luồng Hiển thị
- Khi người dùng mở modal chi tiết dự án, frontend sẽ gửi một yêu cầu để lấy tất cả các tệp đnsh kèm có `attachable_id` và `attachable_type` của file đính kèm.
- Kết quả trả về được dùng để hiển thị danh sách các tệp đính kèm.
## 3. Luồng Xóa:
- Khi User nhấn nút xóa bên cạnh `attachment_file`.
- Frontend gửi yêu cầu `DELETE` đến `/api/attachment/{record_id}`
- Backend sẽ:
	- Xóa tệp vật lý khỏi nơi lưu trữ.
	- Xóa bản ghi có `{record_id}` khỏi bảng `attachments`
	- (Tùy chọn) Tạo một activity với `action_type = 'attachment_remove'`.
