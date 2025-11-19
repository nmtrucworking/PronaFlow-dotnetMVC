using System;

namespace PronaFlow_MVC.Models
{
    /// <summary>
    /// Centralized error messages for the application.
    /// </summary>
    public static class ErrorList
    {
        // --- Common ---
        public const string LoginRequired = "Bạn cần đăng nhập.";
        public const string UnauthorizedAccess = "Bạn không có quyền thực hiện thao tác này.";

        // --- Workspace ---
        public const string NoWorkspaceForCurrentUser = "Không tìm thấy workspace cho người dùng hiện tại.";
        public const string WorkspaceNotBelongToYou = "Workspace không thuộc quyền của bạn.";
        public const string NoWorkspaceSelectedOrExists = "Chưa chọn workspace hoặc workspace không tồn tại.";

        // --- Project ---
        public const string ProjectNotFound = "Project không tồn tại.";
        public const string ProjectNameRequired = "Tên project không được để trống.";
        public const string UnauthorizedUpdateProject = "Bạn không có quyền cập nhật project này.";
        public const string UnauthorizedCreateProject = "Không có quyền tạo project trong workspace này.";

        // --- Exception Prefixes ---
        public const string UpdateFailedPrefix = "Cập nhật thất bại: ";
        public const string CreateFailedPrefix = "Tạo mới thất bại: ";

        // --- Methods ---
        public static string WorkspaceNotOwned(long workspaceId)
        {
            return $"Workspace ID {workspaceId} không thuộc quyền của người dùng hiện tại.";
        }

        public static string WorkspaceWithIdNotFound(long workspaceId)
        {
            return $"Workspace with ID {workspaceId} not found.";
        }
    }
}