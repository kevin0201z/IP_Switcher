using System.Threading.Tasks;

namespace IP_Switcher_WPF.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task ShowMessageAsync(string title, string message, DialogType type = DialogType.Information);
        Task ShowErrorAsync(string message, string title = "错误");
        Task ShowSuccessAsync(string message, string title = "成功");
        Task ShowWarningAsync(string message, string title = "警告");
    }

    public enum DialogType
    {
        Information,
        Warning,
        Error,
        Success
    }
}
