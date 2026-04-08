using System;
using System.Windows;

namespace IP_Switcher_WPF.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowToast(string title, string message, NotificationType type = NotificationType.Information)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var image = type switch
                {
                    NotificationType.Success => MessageBoxImage.Information,
                    NotificationType.Warning => MessageBoxImage.Warning,
                    NotificationType.Error => MessageBoxImage.Error,
                    _ => MessageBoxImage.Information
                };

                MessageBox.Show(message, title, MessageBoxButton.OK, image);
            });
        }

        public void ShowSuccess(string message, string title = null)
        {
            ShowToast(title ?? "成功", message, NotificationType.Success);
        }

        public void ShowError(string message, string title = null)
        {
            ShowToast(title ?? "错误", message, NotificationType.Error);
        }

        public void ShowWarning(string message, string title = null)
        {
            ShowToast(title ?? "警告", message, NotificationType.Warning);
        }

        public void ShowInfo(string message, string title = null)
        {
            ShowToast(title ?? "提示", message, NotificationType.Information);
        }
    }
}
