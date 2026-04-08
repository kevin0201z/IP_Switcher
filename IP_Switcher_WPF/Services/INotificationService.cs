using System;

namespace IP_Switcher_WPF.Services
{
    public interface INotificationService
    {
        void ShowToast(string title, string message, NotificationType type = NotificationType.Information);
        void ShowSuccess(string message, string title = null);
        void ShowError(string message, string title = null);
        void ShowWarning(string message, string title = null);
        void ShowInfo(string message, string title = null);
    }

    public enum NotificationType
    {
        Information,
        Success,
        Warning,
        Error
    }
}
