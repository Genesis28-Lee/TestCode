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
