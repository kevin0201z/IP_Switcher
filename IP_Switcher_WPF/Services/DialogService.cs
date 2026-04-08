using System.Threading.Tasks;
using System.Windows;

namespace IP_Switcher_WPF.Services
{
    public class DialogService : IDialogService
    {
        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        public Task ShowMessageAsync(string title, string message, DialogType type = DialogType.Information)
        {
            var image = type switch
            {
                DialogType.Warning => MessageBoxImage.Warning,
                DialogType.Error => MessageBoxImage.Error,
                DialogType.Success => MessageBoxImage.Information,
                _ => MessageBoxImage.Information
            };
            
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string message, string title = "错误")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return Task.CompletedTask;
        }

        public Task ShowSuccessAsync(string message, string title = "成功")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string message, string title = "警告")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return Task.CompletedTask;
        }
    }
}
