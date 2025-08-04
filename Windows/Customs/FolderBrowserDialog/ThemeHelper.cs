// Windows 10/11의 앱 테마는 레지스트리에서 확인할 수 있습니다:
// HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize
// - AppsUseLightTheme (DWORD): 1=Light, 0=Dark

using Microsoft.Win32;

public static class ThemeHelper
{
    public static bool IsLightTheme()
    {
        const string registryKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var key = Registry.CurrentUser.OpenSubKey(registryKey);
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int intValue && intValue > 0;
    }
}
