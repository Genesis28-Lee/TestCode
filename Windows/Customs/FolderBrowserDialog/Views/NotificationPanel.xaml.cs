private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
{
    if ((sender as TextBlock)?.DataContext is NotificationMessage msg && msg.Tag != null)
    {
        FileOpener.OpenFolder(msg.Tag);
    }
}
