using System.Reflection;
using MelonLoader;

namespace VideoPlayerExtensions;

internal class OptionalUI
{
    private const string COMMONBTKUI_NAME = "Daky.DakyBTKUI";
    private const string PAGE_TYPE_NAME = "BTKUILib.UIObjects.Page";
    private const string CATEGORY_TYPE_NAME = "BTKUILib.UIObjects.Category";
    private const string TOGGLEBUTTON_TYPE_NAME = "BTKUILib.UIObjects.Components.ToggleButton";
    
    private static object rootPage;
    private static object rootCategory;
    
    internal OptionalUI()
    {
        Type? commonType = null;
        Type? pageType = null;
        Type? categoryType = null;
        Type? toggleType = null;
        foreach (MelonAssembly melonAssembly in MelonAssembly.LoadedAssemblies)
        {
            commonType ??= melonAssembly.Assembly.GetType(COMMONBTKUI_NAME);
            pageType ??= melonAssembly.Assembly.GetType(PAGE_TYPE_NAME);
            categoryType ??= melonAssembly.Assembly.GetType(CATEGORY_TYPE_NAME);
            toggleType ??= melonAssembly.Assembly.GetType(TOGGLEBUTTON_TYPE_NAME);
        }
        if (pageType == null || categoryType == null || toggleType == null)
        {
            MelonLogger.Warning(
                "BTKUI was not detected! You must set settings manually through the MelonPreferences file.");
            return;
        }
        if (commonType != null)
        {
            commonType.GetMethod("AutoGenerateCategory")!.Invoke(null, new object?[2]
            {
                Config.preferencesCategory,
                null
            });
            return;
        }
        // Create the BTKUI Page
        rootPage = Activator.CreateInstance(pageType, MainMod.MOD_NAME, MainMod.MOD_NAME + " Settings", true, "", null, false);
        pageType.GetProperty("MenuTitle")!.SetValue(rootPage, MainMod.MOD_NAME + "Settings");
        pageType.GetProperty("MenuSubtitle")!.SetValue(rootPage, "Edit Settings for " + MainMod.MOD_NAME);
        // Add the Category
        rootCategory = pageType.GetMethod("AddCategory", new Type[1]{typeof(string)})!.Invoke(rootPage, new object[1] {"VideoPlayer Settings"});
        // Toggles
        CreateToggle(Config.forceDirect, categoryType, toggleType,
            new object[3] {Config.forceDirect.DisplayName, Config.forceDirect.Description, Config.forceDirect.Value});
        CreateToggle(Config.dynamicLibVLC, categoryType, toggleType,
            new object[3]
                {Config.dynamicLibVLC.DisplayName, Config.dynamicLibVLC.Description, Config.dynamicLibVLC.Value});
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