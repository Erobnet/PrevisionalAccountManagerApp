using System.Windows;
using PrevisionalAccountManager.ViewModels;

namespace PrevisionalAccountManager.Services
{
    public interface IStyleService
    {
        void LoadStyleTheme(ColorTheme theme);
        ColorTheme CurrentTheme { get; }
        event EventHandler<ColorTheme>? ThemeChanged;
        IReadOnlyList<ColorTheme> AvailableThemes { get; }
    }

    public class StyleService : IStyleService
    {
        private const string _LIGHT_THEME_FILE_NAME = "LightTheme.xaml";
        private const string _DARKTHEMEFILENAME = "DarkTheme.xaml";

        public ColorTheme CurrentTheme {
            get;
            private set {
                if ( field == value )
                    return;

                field = value;
                ThemeChanged?.Invoke(this, value);
            }
        }

        public IReadOnlyList<ColorTheme> AvailableThemes => ColorThemeExtensions.GetValues();

        public event EventHandler<ColorTheme>? ThemeChanged;

        public void LoadStyleTheme(ColorTheme theme)
        {
            try
            {
                var app = Application.Current;
                if ( app?.Resources?.MergedDictionaries == null )
                    return;

                // Remove existing theme dictionaries
                var dictionaries = app.Resources.MergedDictionaries;
                for ( int i = dictionaries.Count - 1; i >= 0; i-- )
                {
                    var source = dictionaries[i].Source?.ToString();
                    if ( source != null && (source.EndsWith(_LIGHT_THEME_FILE_NAME) || source.EndsWith(_DARKTHEMEFILENAME)) )
                    {
                        dictionaries.RemoveAt(i);
                    }
                }

                // Load the appropriate theme
                string nextThemeFileName = GetThemeFileName(theme);

                var themeUri = new Uri($"pack://application:,,,/PrevisionalAccountManager;component/Styles/{nextThemeFileName}", UriKind.Absolute);
                var themeDict = new ResourceDictionary { Source = themeUri };

                // Insert theme at the beginning so common styles can reference theme colors
                dictionaries.Insert(0, themeDict);

                CurrentTheme = theme;
                System.Diagnostics.Debug.WriteLine($"Successfully loaded theme: {theme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme '{theme}': {ex.Message}");
            }
        }

        private static string GetThemeFileName(ColorTheme themeName)
        {
            return themeName switch {
                ColorTheme.LightTheme => _LIGHT_THEME_FILE_NAME,
                _ => _DARKTHEMEFILENAME // Default to light theme
            };
        }
    }
}