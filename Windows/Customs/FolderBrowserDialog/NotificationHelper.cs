using Microsoft.Toolkit.Uwp.Notifications; // NuGet 필요
using Windows.UI.Notifications;

public static class NotificationHelper
{
    public static void ShowFolderSelectedToast(string folderPath)
    {
        new ToastContentBuilder()
            .AddText("📁 폴더 선택 완료")
            .AddText(folderPath)
            .Show(toast => toast.ExpirationTime = DateTimeOffset.Now.AddSeconds(5));
    }
}
