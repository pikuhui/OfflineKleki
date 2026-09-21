using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace OfflineKleki;

public partial class MainWindow : Window
{
    private Process? _server;

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute);

    public MainWindow()
    {
        InitializeComponent();

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/C py -m http.server 8080",
            WorkingDirectory = "build",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        _server = Process.Start(startInfo);

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;

        int darkMode = 1;

        DwmSetWindowAttribute(
            hwnd,
            DWMWA_USE_IMMERSIVE_DARK_MODE,
            ref darkMode,
            sizeof(int)
        );

        await WebView.EnsureCoreWebView2Async();

        WebView.CoreWebView2.Navigate(
            "http://127.0.0.1:8080"
        );
    }

    protected override void OnClosed(EventArgs e)
    {
        _server?.Kill();
        _server?.Dispose();

        base.OnClosed(e);
    }
}