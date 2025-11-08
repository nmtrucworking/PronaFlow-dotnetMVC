- **Lấy Chi Tiết Project (`GET`):**
    - **Method:** `GET`
    - **URL:** `http://localhost:PORT/api/workspaces/1/projects/1` (thay `PORT`, `workspaceId`, `projectId` cho đúng)
    - **Auth:** Dùng Bearer Token.
    - **Kết quả mong đợi:** `200 OK` với đầy đủ thông tin chi tiết của project.
        
- **Cập Nhật Project (`PUT`):**
    - **Method:** `PUT`
    - **URL:** `http://localhost:PORT/api/workspaces/1/projects/1`
    - **Auth:** Dùng Bearer Token.
    - **Body (JSON):**
        ```JSON
        {
          "name": "Updated Project Name",
          "description": "This is an updated description.",
          "coverImageUrl": null,
          "status": "in-progress",
          "startDate": "2025-10-15",
          "endDate": "2025-11-30"
        }
        ```
        
    - **Kết quả mong đợi:** `204 No Content` nếu cập nhật thành công, `403 Forbidden` nếu bạn không có quyền admin, hoặc `404 Not Found` nếu không tìm thấy project.