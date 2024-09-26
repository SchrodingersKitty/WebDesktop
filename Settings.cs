using System.Configuration;

namespace WebDesktop;

[SettingsProvider(typeof(LocalFileSettingsProvider))]
public class Settings : ApplicationSettingsBase
{
    private static readonly Settings main;
    public static Settings Main
    {
        get => main;
    }

    static Settings()
    {
        main = new();
    }

    [UserScopedSetting()]
    [DefaultSettingValue("")]
    public string URL
    {
        get => (string)this[nameof(URL)];
        set => this[nameof(URL)] = value;
    }

    [UserScopedSetting()]
    [DefaultSettingValue("0")]
    public int DisplayMode
    {
        get
        {
            var v = (int)this[nameof(DisplayMode)];
            if(v < 0 || v > 1)
            {
                v = 0;
            }
            return v;
        }
        set => this[nameof(DisplayMode)] = value;
    }
}