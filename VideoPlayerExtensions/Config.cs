using MelonLoader;

namespace VideoPlayerExtensions;

internal static class Config
{
    private const int CONFIG_VERSION = 2;
    
    public static bool ForceDirect => forceDirect.Value;
    public static bool DyanmicLibVLC => dynamicLibVLC.Value;
    public static bool ForceVLCWithYouTube => forceVLCWithYouTube.Value;
    public static string[] VLCProtocols => vlcProtocols.Value;
    public static string[] VLCFiles => vlcFiles.Value;

    private static readonly string[] defaultVLCProtocols = {"rtmp", "rtsp", "srt", "udp", "tcp"};
    private static readonly string[] defaultVLCFiles = {".m3u8", ".flv"};
    
    internal static MelonPreferences_Category preferencesCategory = MelonPreferences.CreateCategory(MainMod.MOD_NAME + " Settings");
    internal static MelonPreferences_Entry<bool> forceDirect;
    internal static MelonPreferences_Entry<bool> dynamicLibVLC;
    internal static MelonPreferences_Entry<bool> forceVLCWithYouTube;
    private static MelonPreferences_Entry<string[]> vlcProtocols;
    private static MelonPreferences_Entry<string[]> vlcFiles;
    private static MelonPreferences_Entry<int> configVersion;

    static Config()
    {
        forceDirect = preferencesCategory.CreateEntry("forceDirect", false, "Force Direct",
            "Switches Audio on Video Players to Direct");
        dynamicLibVLC = preferencesCategory.CreateEntry("dynamicLibVLC", true, "Dynamic LibVLC",
            "Switches VideoPlayer to LibVLC if media cannot be played on AVPro");
        forceVLCWithYouTube = preferencesCategory.CreateEntry("forceVLCWithYouTube", true, "Force VLC with YouTube",
            "Forces VLC to be used with all YouTube links (Requires DynamicLibVLC)");
        vlcProtocols = preferencesCategory.CreateEntry("VLCProtocols", defaultVLCProtocols,
            description: "Set the stream protocols that will activate VLC");
        vlcFiles = preferencesCategory.CreateEntry("VLCFiles", defaultVLCFiles,
            description: "Set the file types that will activate VLC");
        configVersion = preferencesCategory.CreateEntry("ConfigVersion", CONFIG_VERSION, description: "DO NOT CHANGE!");
        if (configVersion.Value == CONFIG_VERSION) return;
        vlcProtocols.Value = defaultVLCProtocols;
        vlcFiles.Value = defaultVLCFiles;
        configVersion.Value = CONFIG_VERSION;
        Save();
    }

    internal static void Save() => preferencesCategory.SaveToFile(false);
}