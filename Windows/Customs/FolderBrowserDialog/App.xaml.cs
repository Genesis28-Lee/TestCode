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

        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            // 알림 클릭 시 처리
        };

        // AppId 설정
        ToastNotificationManagerCompat.History.Clear();
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
