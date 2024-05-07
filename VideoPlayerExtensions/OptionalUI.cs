using System.Reflection;
using MelonLoader;

namespace VideoPlayerExtensions;

internal class OptionalUI
{
    private const string PAGE_TYPE_NAME = "BTKUILib.UIObjects.Page";
    private const string CATEGORY_TYPE_NAME = "BTKUILib.UIObjects.Category";
    private const string TOGGLEBUTTON_TYPE_NAME = "BTKUILib.UIObjects.Components.ToggleButton";
    
    private static object rootPage;
    private static object rootCategory;
    
    internal OptionalUI()
    {
        Type? pageType = null;
        Type? categoryType = null;
        Type? toggleType = null;
        // TODO: there's probably a way better way to do this
        foreach (MelonAssembly melonAssembly in MelonAssembly.LoadedAssemblies)
        {
            foreach (Type type in melonAssembly.Assembly.GetTypes())
            {
                switch (type.FullName)
                {
                    case PAGE_TYPE_NAME:
                        pageType = type;
                        break;
                    case CATEGORY_TYPE_NAME:
                        categoryType = type;
                        break;
                    case TOGGLEBUTTON_TYPE_NAME:
                        toggleType = type;
                        break;
                }
            }
        }
        if (pageType == null || categoryType == null || toggleType == null)
        {
            MelonLogger.Warning(
                "BTKUI was not detected! You must set settings manually through the MelonPreferences file.");
            return;
        }
        // Create the BTKUI Page
        rootPage = Activator.CreateInstance(pageType, MainMod.MOD_NAME, MainMod.MOD_NAME + "Settings", true, "", null, false);
        pageType.GetProperty("MenuTitle")!.SetValue(rootPage, MainMod.MOD_NAME + "Settings");
        pageType.GetProperty("MenuSubtitle")!.SetValue(rootPage, "Edit Settings for " + MainMod.MOD_NAME);
        // Add the Category
        rootCategory = pageType.GetMethod("AddCategory", new Type[1]{typeof(string)})!.Invoke(rootPage, new object[1] {"VideoPlayer Settings"});
        // Toggles
        CreateToggle(Config.forceDirect, categoryType, toggleType,
            new object[3] {"Force Direct", "Switches Audio on Video Players to Direct", Config.forceDirect.Value});
        CreateToggle(Config.dynamicLibVLC, categoryType, toggleType,
            new object[3]
            {
                "Dyanmic LibVLC", "Switches VideoPlayer to LibVLC if media cannot be played on AVPro",
                Config.dynamicLibVLC.Value
            });
    }

    private static void CreateToggle(MelonPreferences_Entry<bool> p, Type categoryType, Type toggleType, object[] pa)
    {
        object toggleObject = categoryType.GetMethod("AddToggle")!.Invoke(rootCategory, pa);
        FieldInfo fieldInfo = toggleType.GetField("OnValueUpdated");
        Action<bool> toggleAction = (Action<bool>) fieldInfo.GetValue(toggleObject);
        toggleAction += b => ToggleValueUpdated(p, b);
        fieldInfo.SetValue(toggleObject, toggleAction);
    }
    
    private static void ToggleValueUpdated(MelonPreferences_Entry<bool> p, bool b)
    {
        p.Value = b;
        Config.Save();
    }
}