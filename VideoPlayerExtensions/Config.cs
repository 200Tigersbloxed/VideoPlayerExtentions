using MelonLoader;

namespace VideoPlayerExtensions;

internal static class Config
{
    private const int CONFIG_VERSION = 1;
    
    public static bool ForceDirect => forceDirect.Value;
    public static bool DyanmicLibVLC => dynamicLibVLC.Value;
    public static string[] VLCProtocols => vlcProtocols.Value;
    public static string[] VLCFiles => vlcFiles.Value;

    private static readonly string[] defaultVLCProtocols = {"rtmp", "rtsp", "srt", "udp", "tcp"};
    private static readonly string[] defaultVLCFiles = {".m3u8", ".flv"};
    
    private static MelonPreferences_Category preferencesCategory = MelonPreferences.CreateCategory(MainMod.MOD_NAME + " Settings");
    internal static MelonPreferences_Entry<bool> forceDirect;
    internal static MelonPreferences_Entry<bool> dynamicLibVLC;
    private static MelonPreferences_Entry<string[]> vlcProtocols;
    private static MelonPreferences_Entry<string[]> vlcFiles;
    private static MelonPreferences_Entry<int> configVersion;

    static Config()
    {
        forceDirect = preferencesCategory.CreateEntry("forceDirect", false);
        dynamicLibVLC = preferencesCategory.CreateEntry("dynamicLibVLC", true);
        vlcProtocols = preferencesCategory.CreateEntry("VLCProtocols", defaultVLCProtocols);
        vlcFiles = preferencesCategory.CreateEntry("VLCFiles", defaultVLCFiles);
        configVersion = preferencesCategory.CreateEntry("ConfigVersion", CONFIG_VERSION);
        if (configVersion.Value == CONFIG_VERSION) return;
        vlcProtocols.Value = defaultVLCProtocols;
        vlcFiles.Value = defaultVLCFiles;
        configVersion.Value = CONFIG_VERSION;
        Save();
    }

    internal static void Save() => preferencesCategory.SaveToFile(false);
}