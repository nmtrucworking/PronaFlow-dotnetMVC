# **Bước 1: Lấy Token Xác Thực (Authentication Token)**

Đây là bước tiên quyết. Mọi request đến `WorkspacesController` đều cần một token hợp lệ.
1. Mở lại request **`3. Login - Success`** trong bộ test Authentication mà chúng ta đã tạo.
2. Đảm bảo bạn đã có ít nhất một tài khoản đã được đăng ký thành công (ví dụ: `existing@example.com`).
3. Gửi (Send) request `Login - Success`.
4. Bạn sẽ nhận được một response `200 OK` chứa token. Hãy **copy toàn bộ chuỗi token** dài đó.
    ```JSON
    {
      "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ..." // <--- COPY CHUỖI NÀY
    }
    ```
# **Bước 2: Cách Gửi Một Request Đã Được Xác Thực**
Bây giờ, với mỗi request mới gửi đến Workspace API, bạn cần đính kèm token đã copy ở trên.
1. Tạo một request mới trong Insomnia (ví dụ: `POST` để tạo Workspace).
2. Chuyển sang tab **`Auth`** (nằm cạnh tab `Body`).
3. Click vào dropdown và chọn **`Bearer Token`**.
4. Một ô input có nhãn **`TOKEN`** sẽ hiện ra. **Dán (paste) chuỗi token** bạn đã copy từ Bước 1 vào đây.
5. Ô **`PREFIX`** sẽ được tự động điền là `Bearer`. Hãy giữ nguyên nó.

>**Điều gì xảy ra?** Khi bạn thiết lập như vậy, Insomnia sẽ tự động thêm một Header vào request của bạn có dạng: `Authorization: Bearer eyJhbGciOiJIUzUxMi...`. Server ASP.NET Core của bạn sẽ đọc header này để xác thực bạn là ai.
# **Bước 3: Quy Trình Test Các API Workspace (CRUD)**
Bây giờ chúng ta sẽ test các API theo một luồng logic: Tạo -> Đọc -> Cập nhật -> Xóa.
## **A. Tạo Workspace Mới (POST)**
1. **Method:** `POST` 
2. **URL:** `{{ base_url }}/workspaces`
3. **Auth:** Thiết lập **Bearer Token** như hướng dẫn ở Bước 2.
4. **Body (JSON):**
    ```JSON
    {
      "name": "My First Workspace",
      "description": "This is a test workspace for my projects."
    }
    ```
5. Nhấn **Send**.
6. **Kết quả mong đợi:** Response `201 Created` cùng với thông tin của workspace vừa tạo. **Hãy ghi lại `id` của workspace này** để dùng cho các bước tiếp theo.
## **B. Lấy Danh Sách Workspaces (GET)**

1. **Method:** `GET`
2. **URL:** `{{ base_url }}/workspaces`
3. **Auth:** Thiết lập **Bearer Token**.
4. Nhấn **Send**.
5. **Kết quả mong đợi:** Response `200 OK` với một mảng JSON chứa danh sách các workspace của bạn (bao gồm cả workspace bạn vừa tạo ở bước A).
## **C. Cập Nhật Workspace (PUT)**
1. **Method:** `PUT`
2. **URL:** `{{ base_url }}/workspaces/1` (thay số `1` bằng `id` của workspace bạn đã tạo ở bước A).
3. **Auth:** Thiết lập **Bearer Token**.
4. **Body (JSON):**
    ```JSON
    {
      "name": "My Workspace (Updated)",
      "description": "Updated description."
    }
    ```
    
5. Nhấn **Send**.    
6. **Kết quả mong đợi:** Response `204 No Content`. Điều này có nghĩa là việc cập nhật đã thành công và không có nội dung gì cần trả về.
## **D. Xóa Workspace (DELETE)**

1. **Method:** `DELETE`
2. **URL:** `{{ base_url }}/workspaces/1` (thay số `1` bằng `id` của workspace bạn đã tạo).
3. **Auth:** Thiết lập **Bearer Token**.
4. Nhấn **Send**.
5. **Kết quả mong đợi:** Response `204 No Content`.
# **Bước 4: Test Các Trường Hợp Thất Bại (Quan trọng)**

Một bộ test tốt không chỉ kiểm tra trường hợp thành công. Hãy thử các kịch bản sau:

- **Xóa Workspace không rỗng:**
    1. Tạo một workspace.
    2. Tạo một project thuộc workspace đó (bạn sẽ làm API cho Project sau).
    3. Thử xóa workspace đó.
    4. **Kết quả mong đợi:** Response `400 Bad Request` với thông báo "Workspace is not empty...".

- **Truy cập workspace của người khác:**
    1. Đăng ký 2 tài khoản: User A và User B.
    2. Dùng token của User A để tạo Workspace A (ví dụ có id là 1).
    3. Dùng token của User B để thử xóa Workspace A (`DELETE {{ base_url }}/workspaces/1`).
    4. **Kết quả mong đợi:** Response `404 Not Found`, vì trong phạm vi của User B, không tồn tại workspace nào có id là 1.