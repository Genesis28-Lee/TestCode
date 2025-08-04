using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.ObjectModel;

public class NotificationService
{
    public ObservableCollection<NotificationMessage> Messages { get; } = new();

    public void Notify(string message, NotificationPriority priority = NotificationPriority.Normal, string? tag = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = Messages.FirstOrDefault(m => m.Message == message && m.Tag == tag);
            if (existing != null)
            {
                existing.IncrementCount();
            }
            else
            {
                var msg = new NotificationMessage
                {
                    Message = message,
                    Priority = priority,
                    Timestamp = DateTime.Now,
                    Tag = tag
                };
                Messages.Insert(0, msg);
                ShowToastPopup(msg);
                ScheduleRemoval(msg);
            }

            SendWindowsToast(message, priority);
        });
    }

    private void ScheduleRemoval(NotificationMessage message)
    {
        Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Remove(message);
            });
        });
    }

    private void ShowToastPopup(NotificationMessage message)
    {
        var toast = new ToastPopupView(message);
        toast.Show();
    }

    private void SendWindowsToast(string message, NotificationPriority priority)
    {
        new ToastContentBuilder()
            .AddText(priority == NotificationPriority.High ? "⚠️ 중요 알림" : "📁 알림")
            .AddText(message)
            .Show();
    }
}
