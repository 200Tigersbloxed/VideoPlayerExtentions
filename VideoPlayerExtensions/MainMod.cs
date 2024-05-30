using System.Reflection;
using ABI_RC.Core.Savior;
using ABI_RC.VideoPlayer.Scripts;
using ABI_RC.VideoPlayer.Scripts.Players.AvPro;
using ABI_RC.VideoPlayer.Scripts.Players.LibVLC;
using ABI.CCK.Components;
using HarmonyLib;
using MelonLoader;
using VideoPlayerExtensions;

[assembly: MelonInfo(typeof(MainMod), MainMod.MOD_NAME, MainMod.MOD_VERSION, MainMod.MOD_AUTHOR)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
[assembly: MelonOptionalDependencies("BTKUILib")]
[assembly: AssemblyVersion(MainMod.MOD_VERSION)]
[assembly: AssemblyFileVersion(MainMod.MOD_VERSION)]
[assembly: AssemblyInformationalVersion(MainMod.MOD_VERSION)]

namespace VideoPlayerExtensions;

public class MainMod : MelonMod
{
    internal const string MOD_NAME = "VideoPlayerExtensions";
    internal const string MOD_VERSION = "1.1.0";
    internal const string MOD_AUTHOR = "200Tigersbloxed";
    
    private static Type playerType = typeof(CVRVideoPlayer);
    private static Type vlcPlayer = typeof(LibVLCPlayer);
    private static Type avPlayer = typeof(AvProPlayer);

    private OptionalUI optionalUI;

    public override void OnInitializeMelon() => optionalUI = new();

    [HarmonyPatch(typeof(CVRVideoPlayer), "OnEnable", new Type[0])]
    private class VideoPlayerEnablePatch
    {
        static void Postfix(ref CVRVideoPlayer __instance)
        {
            if (Config.ForceDirect)
                __instance.SetAudioMode(VideoPlayerUtils.AudioMode.Direct);
        }
    }
    
    [HarmonyPatch(typeof(CVRVideoPlayer), nameof(CVRVideoPlayer.SetVideoUrl), new Type[5]{typeof(string), typeof(bool), typeof(string), typeof(string), typeof(bool)})]
    private class VideoPlayerSetUrlPatch
    {
        static void Prefix(ref CVRVideoPlayer __instance, string url, bool broadcast = true, string objPath = "",
            string username = null, bool isPaused = false)
        {
            if(Config.DyanmicLibVLC)
            {
                bool useVlc = false;
                try
                {
                    Uri uri = new Uri(url);
                    switch (uri.Scheme.ToLower())
                    {
                        case "rtmp":
                        case "rtsp":
                        case "srt":
                        case "udp":
                        case "tcp":
                            useVlc = true;
                            break;
                    }

                    if (!useVlc)
                    {
                        string destFile = uri.Segments[uri.Segments.Length - 1];
                        string extension = Path.GetExtension(destFile);
                        foreach (string vlcFile in Config.VLCFiles)
                        {
                            if (extension == vlcFile)
                            {
                                useVlc = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MelonLogger.Error("Failed to parse URL " + url + "! Falling back to simple checks.");
                    foreach (string protocol in Config.VLCProtocols)
                    {
                        if (url.Contains(protocol))
                        {
                            useVlc = true;
                            break;
                        }
                    }

                    if (!useVlc)
                    {
                        foreach (string vlcFile in Config.VLCFiles)
                        {
                            if (url.Contains(vlcFile))
                            {
                                useVlc = true;
                                break;
                            }
                        }
                    }
                }
                finally
                {
#if DEBUG
                MelonLogger.Msg(System.ConsoleColor.Gray, url + " will " + (useVlc ? "" : "not ") + "use VLC");
#endif
                    CheckVR.Instance.enableVlcPlayers = useVlc;
                    Type targetPlayerType = useVlc ? vlcPlayer : avPlayer;
                    Type t = (Type) playerType.GetField("lastType", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(__instance);
                    if (t != null && t != targetPlayerType)
                    {
                        if (__instance.VideoPlayer != null)
                        {
                            // TODO: Calling Destroy on a LibVLCPlayer will freeze the game.
                            if (__instance.VideoPlayer.GetType() == vlcPlayer)
                            {
                                LibVLCPlayer v = (LibVLCPlayer) __instance.VideoPlayer;
                                object mediaPlayer =
                                    vlcPlayer.GetField("_videoPlayer", BindingFlags.Instance | BindingFlags.NonPublic)!
                                        .GetValue(v);
                                mediaPlayer.GetType().GetMethod("Stop")!.Invoke(mediaPlayer, new object[0]);
                            }
                            else
                                __instance.VideoPlayer.Destroy();
                        }

                        MethodInfo? switchMethod = playerType.GetMethod("SwitchVideoPlayer",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        switchMethod?.Invoke(__instance, new object?[1] {targetPlayerType});
                    }
                }
            }
        }
    }
}