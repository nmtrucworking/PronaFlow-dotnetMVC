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


        /// <summary>
        /// Error message common to multiple modules.
        /// </summary>
        public static class Common
        {
            public const string InvalidInput = "Dữ liệu đầu vào không hợp lệ.";
            public const string NotFound = "Không tìm thấy dữ liệu yêu cầu.";
            public const string SystemError = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
            public const string SaveFailedPrefix = "Lưu dữ liệu thất bại: ";
            public const string UpdateFailedPrefix = "Cập nhật thất bại: ";
            public const string CreateFailedPrefix = "Tạo mới thất bại: ";
        }

        /// <summary>
        /// Error messages related to Authentication and Authorization.
        /// </summary>
        public static class Auth
        {
            public const string LoginRequired = "Bạn cần đăng nhập để thực hiện thao tác này.";
            public const string UnauthorizedAccess = "Bạn không có quyền thực hiện thao tác này.";
            public const string SessionExpired = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
        }

        /// <summary>
        /// Error messages related to Workspace operations.
        /// </summary>
        public static class Workspace
        {
            public const string NotFound = "Không tìm thấy workspace hoặc workspace không tồn tại.";
            public const string NotSelected = "Chưa chọn workspace làm việc.";
            public const string NotOwned = "Workspace này không thuộc quyền sở hữu của bạn.";

            public static string NotOwnedWithId(long workspaceId) =>
                $"Workspace ID {workspaceId} không thuộc quyền của người dùng hiện tại.";

            public static string NotFoundWithId(long workspaceId) =>
                $"Không tìm thấy Workspace với ID {workspaceId}.";
        }

        /// <summary>
        /// Error messages related to Project operations.
        /// </summary>
        public static class Project
        {
            public const string NotFound = "Dự án không tồn tại hoặc đã bị xóa.";
            public const string NameRequired = "Tên dự án không được để trống.";
            public const string UnauthorizedUpdate = "Bạn không có quyền cập nhật dự án này.";
            public const string UnauthorizedCreate = "Bạn không có quyền tạo dự án trong workspace này.";
        }

        /// <summary>
        /// Error messages related to Task operations.
        /// </summary>
        public static class Task
        {
            public const string NotFound = "Công việc không tồn tại.";
            public const string AssigneeNotFound = "Người được giao việc không tồn tại trong dự án.";
        }
    }
}