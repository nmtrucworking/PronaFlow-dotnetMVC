namespace PronaFlow_MVC.Models
{
    /// <summary>
    /// Centralized success messages for the application to ensure consistency.
    /// </summary>
    public static class SuccessList
    {
        public static string baseMessage (string objectName, string action) {
            return $"{objectName} has been {action} successfully"; 
        }

        // --- Dynamic Methods (Nếu cần tham số) ---
        public static string WelcomeUser(string userName)
        {
            return $"Xin chào, {userName}!";
        }
        /// <summary>
        /// Success messages related to common operations.
        /// </summary>
        public static class Common
        {
            public const string Saved = "Lưu dữ liệu thành công.";
            public const string Deleted = "Xóa dữ liệu thành công.";
            public const string Updated = "Cập nhật thông tin thành công.";
            public const string OperationCompleted = "Thao tác đã được thực hiện thành công.";
        }

        /// <summary>
        /// Thông báo liên quan đến tài khoản.
        /// </summary>
        public static class Account
        {
            public const string Login = "Đăng nhập thành công. Chào mừng bạn trở lại!";
            public const string Register = "Đăng ký tài khoản thành công. Vui lòng đăng nhập.";
            public const string Logout = "Đăng xuất thành công.";
            public const string ChangePassword = "Đổi mật khẩu thành công.";

            public static string WelcomeUser(string userName) => $"Xin chào, {userName}!";
        }

        /// <summary>
        /// Thông báo liên quan đến Dự án và Workspace.
        /// </summary>
        public static class Project
        {
            public const string Created = "New Project has been created successfully.";
            public const string Updated = "Cập nhật dự án thành công.";
            public const string Deleted = "Dự án đã được xóa.";
            public const string MemberAdded = "Đã thêm thành viên mới vào dự án.";
            public const string Archived = "Project has been archived successfully.";
        }

        public static class Workspace
        {
            public const string Created = "New Workspace has been created successfully.";
        }

        public static class Task
        {
            public const string Created = "New Task has been created successfully.";
            public const string Deleted = "Task has been deleted successfully.";
            public const string Renamed = "Task has been renamed successfully.";
            public const string Updated = "Task has been updated successfully.";
            public static string StatusUpdated (string lastStatus, string newStatus) {
                return $"Task status has been updated from '{lastStatus}' to '{newStatus}' successfully.";
            }
            public static string AssignedToUser (string userName)
            {
                return $"Task has been assigned to '{userName}' successfully.";
            }
        }
    }
}