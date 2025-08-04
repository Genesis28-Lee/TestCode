using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.ObjectModel;

public class NotificationService
{
    public ObservableCollection<NotificationMessage> Messages { get; } = new();

    public void Notify(string message, NotificationPriority priority = NotificationPriority.Normal)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 중복 메시지 처리
            var existing = Messages.FirstOrDefault(m => m.Message == message && m.Priority == priority);
            if (existing != null)
            {
                existing.IncrementCount();
            }
            else
            {
                Messages.Insert(0, new NotificationMessage
                {
                    Message = message,
                    Priority = priority,
                    Timestamp = DateTime.Now
                });
            }

            // Windows Toast 발송
            SendWindowsToast(message, priority);
        });
    }

    private void SendWindowsToast(string message, NotificationPriority priority)
    {
        var builder = new ToastContentBuilder()
            .AddText(priority == NotificationPriority.High ? "⚠️ 중요 알림" : "📁 알림")
            .AddText(message);

        if (priority == NotificationPriority.High)
        {
            builder.SetToastDuration(ToastDuration.Long);
        }

        builder.Show(toast =>
        {
            toast.Group = "MyAppGroup";
            toast.Tag = Guid.NewGuid().ToString(); // 각 Toast 개별 발송
            toast.ExpirationTime = DateTimeOffset.Now.AddSeconds(priority == NotificationPriority.High ? 15 : 5);
        });
    }
}
