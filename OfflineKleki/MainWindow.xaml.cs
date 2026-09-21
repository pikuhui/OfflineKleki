using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;

namespace OfflineKleki;

public partial class MainWindow : Window
{
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

        WebView.CoreWebView2.AddWebResourceRequestedFilter(
            "https://kleki.local/*",
            CoreWebView2WebResourceContext.All
        );

        WebView.CoreWebView2.WebResourceRequested +=
            WebResourceRequested;

        WebView.CoreWebView2.Navigate(
            "https://kleki.local/"
        );
    }

    private void WebResourceRequested(
        object? sender,
        CoreWebView2WebResourceRequestedEventArgs e)
    {
        var path = Uri.UnescapeDataString(
            new Uri(e.Request.Uri).AbsolutePath.TrimStart('/')
        );

        if (path == "")
        {
            path = "index.html";
        }

        var resourceName =
            "OfflineKleki.build." +
            path.Replace("/", ".");

        var assembly = Assembly.GetExecutingAssembly();

        var stream =
            assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            return;
        }

        e.Response =
            WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                stream,
                200,
                "OK",
                $"Content-Type: {GetContentType(path)}"
            );
    }

    private static string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".html" => "text/html",
            ".js" => "text/javascript",
            ".css" => "text/css",
            ".json" => "application/json",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".ico" => "image/x-icon",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            ".wasm" => "application/wasm",
            _ => "application/octet-stream"
        };
    }
}