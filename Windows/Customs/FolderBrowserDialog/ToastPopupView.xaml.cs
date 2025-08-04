public partial class ToastPopupView : Window
{
    public ToastPopupView(NotificationMessage msg)
    {
        InitializeComponent();
        DataContext = msg;

        var screen = SystemParameters.WorkArea;
        Left = screen.Right - Width - 20;
        Top = screen.Bottom - Height - 100;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
        BeginAnimation(OpacityProperty, anim);

        await Task.Delay(4000);
        Close();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is NotificationMessage msg && msg.Tag != null && Directory.Exists(msg.Tag))
        {
            FileOpener.OpenFolder(msg.Tag);
        }
        Close();
    }
}
