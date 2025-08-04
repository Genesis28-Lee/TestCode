// Windows 10/11의 앱 테마는 레지스트리에서 확인할 수 있습니다:
// HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize
// - AppsUseLightTheme (DWORD): 1=Light, 0=Dark

using Microsoft.Win32;

public static class ThemeHelper
{
    public static bool IsLightTheme()
    {
        const string key = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var regKey = Registry.CurrentUser.OpenSubKey(key);
        var value = regKey?.GetValue("AppsUseLightTheme");
        return value is int v && v == 1;
    }
}
