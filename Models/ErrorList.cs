using System;

namespace PronaFlow_MVC.Models
{
    /// <summary>
    /// Error messages for All Controllers
    /// </summary>
    public static class ErrorList
    {
        public const string ProjectNotFound = "Project không tồn tại.";
        public const string UnauthorizedUpdateProject = "Bạn không có quyền cập nhật project này.";
        public const string NoWorkspaceForCurrentUser = "Không tìm thấy workspace cho người dùng hiện tại.";
        public const string WorkspaceNotBelongToYou = "Workspace không thuộc quyền của bạn.";
        public const string NoWorkspaceSelectedOrExists = "No workspace selected or no workspace exists.";
        public const string LoginRequired = "Bạn cần đăng nhập.";

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