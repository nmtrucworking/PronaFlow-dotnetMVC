namespace PronaFlow_MVC.Models
{
    /// <summary>
    /// Centralized success messages for the application to ensure consistency.
    /// </summary>
    public static class SuccessList
    {
        // --- Common ---
        public const string SavedSuccessfully = "Lưu dữ liệu thành công.";
        public const string DeletedSuccessfully = "Xóa dữ liệu thành công.";
        public const string UpdatedSuccessfully = "Cập nhật thông tin thành công.";
        public const string OperationCompleted = "Thao tác đã được thực hiện.";

        // --- Account / Auth ---
        public const string LoginSuccess = "Đăng nhập thành công. Chào mừng bạn trở lại!";
        public const string RegisterSuccess = "Đăng ký tài khoản thành công. Vui lòng đăng nhập.";
        public const string LogoutSuccess = "Đăng xuất thành công.";
        public const string PasswordChanged = "Đổi mật khẩu thành công.";

        // --- Project / Workspace ---
        public const string ProjectCreated = "Dự án mới đã được tạo.";
        public const string WorkspaceCreated = "Workspace mới đã được khởi tạo.";
        public const string MemberAdded = "Đã thêm thành viên vào dự án.";
        public const string ProjectUpdated = "Cập nhật thông tin dự án thành công.";
        public const string ProjectDeleted = "Dự án đã được xóa thành công.";

        // --- Dynamic Methods (Nếu cần tham số) ---
        public static string WelcomeUser(string userName)
        {
            return $"Xin chào, {userName}!";
        }
    }
}