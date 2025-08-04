// 🔧 NuGet 패키지 설치
// Microsoft.Toolkit.Uwp.Notifications

using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplyTheme();

        SystemEvents.UserPreferenceChanged += (_, args) =>
        {
            if (args.Category == UserPreferenceCategory.General)
            {
                ApplyTheme();
            }
        };

        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            // 알림 클릭 시 처리
        };

        // AppId 설정
        ToastNotificationManagerCompat.History.Clear();
    }

    private void ApplyTheme()
    {
        string themeFile = ThemeHelper.IsLightTheme()
            ? "Themes/Light.xaml"
            : "Themes/Dark.xaml";

        var rd = new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) };
        Resources.MergedDictionaries.Clear();
        Resources.MergedDictionaries.Add(rd);
    }
}
