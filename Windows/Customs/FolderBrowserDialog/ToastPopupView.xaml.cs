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

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var slideIn = new ThicknessAnimation
        {
            From = new Thickness(0, 100, 0, -100),
            To = new Thickness(0),
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        BeginAnimation(MarginProperty, slideIn);
    
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
