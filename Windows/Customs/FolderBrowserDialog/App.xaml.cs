// 🔧 NuGet 패키지 설치
// Microsoft.Toolkit.Uwp.Notifications
// System.Windows.Forms 패키지와 app.ico 필요

using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;

public partial class App : Application
{
    private System.Windows.Forms.NotifyIcon _notifyIcon;

    /*
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplyTheme();

        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            // 알림 클릭 시 처리
        };

        // AppId 설정
        ToastNotificationManagerCompat.History.Clear();
    }
    */
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = new System.Drawing.Icon("app.ico"),
            Visible = true,
            Text = "MyApp 알림 시스템"
        };
    
        _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        _notifyIcon.ContextMenuStrip.Items.Add("알림 보기", null, (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var win = new NotificationWindow();
                win.Show();
            });
        });
    
        _notifyIcon.ContextMenuStrip.Items.Add("종료", null, (_, _) => Shutdown());
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
        {
            ApplyTheme();
        }
    }

    private void ApplyTheme()
    {
        var themeUri = ThemeHelper.IsLightTheme()
            ? new Uri("Themes/Light.xaml", UriKind.Relative)
            : new Uri("Themes/Dark.xaml", UriKind.Relative);

        var mergedDictionaries = Resources.MergedDictionaries;
        mergedDictionaries.Clear();
        mergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
    }
}
