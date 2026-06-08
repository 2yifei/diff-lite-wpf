using System.Windows;

namespace DiffLite;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 全局异常处理
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(
                $"发生未处理的错误：\n{args.Exception.Message}",
                "Diff-Lite 错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
