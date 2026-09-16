using System.Windows;
using DailyQuest.Theming;

namespace DailyQuest.Services;

public sealed class WpfThemeService(Application application) : IThemeService
{
    private const string ThemeDictionarySuffix = "Theme.xaml";
    private readonly Application _application = application;

    public void Apply(string themeCode)
    {
        var normalized = ThemeCatalog.Normalize(themeCode);
        if (_application.Dispatcher.CheckAccess())
        {
            ApplyCore(normalized);
            return;
        }

        _application.Dispatcher.Invoke(() => ApplyCore(normalized));
    }

    private void ApplyCore(string themeCode)
    {
        var dictionaries = _application.Resources.MergedDictionaries;
        var assemblyName = typeof(WpfThemeService).Assembly.GetName().Name;
        var source = new Uri(
            $"/{assemblyName};component/Themes/{(themeCode == ThemeCatalog.DarkCode ? "Dark" : "Light")}Theme.xaml",
            UriKind.Relative);

        var currentIndex = -1;
        for (var index = 0; index < dictionaries.Count; index++)
        {
            var original = dictionaries[index].Source?.OriginalString;
            if (original?.EndsWith(ThemeDictionarySuffix, StringComparison.OrdinalIgnoreCase) == true)
            {
                currentIndex = index;
                if (original.EndsWith(source.OriginalString, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                break;
            }
        }

        var replacement = new ResourceDictionary { Source = source };
        if (currentIndex >= 0)
        {
            dictionaries[currentIndex] = replacement;
        }
        else
        {
            dictionaries.Insert(0, replacement);
        }
    }
}
