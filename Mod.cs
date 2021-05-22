using CitiesHarmony.API;
using ColossalFramework.UI;
using EightyOne.Areas;
using ICities;
using UnityEngine;

namespace EightyOne
{
    public class Mod : IUserMod
    {
        public const string UNLOCK_ALL_TILES_FOR_FREE = "Unlock all tiles for free";
        public const string CROSS_THE_LINE_MOD = "CrossTheLine";
        public const string BUILDING_THEMES_MOD = "Building Themes";
        public const string REMOVE_NEED_FOR_PIPES_MOD = "Remove Need For Pipes";
        public const string SURFACE_PAINTER_MOD = "Surface Painter";
        public const string ALL_TILES_START_MOD = "Unlock All Tiles at Start";
        public const string REMOVE_NEED_FOR_POWER_LINES_MOD = "Remove Need For Power Lines";
        public static string DISABLE_ZONE_CHECK_MOD = "Disable ZoneCheck";
        public static string PLOP_GROWBALES_MOD = "Plop Growables";

        public string Description => "Allows to use all 81 map tiles";

        public string Name => "81 Tiles (Fixed for C:S 1.2+)"; //never change this name! Used by Surface Painter do detect it

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddGroup("Utilities").AddButton(UNLOCK_ALL_TILES_FOR_FREE, UnlockAllCheat.UnlockAllAreas);
            var dangerZone = (UIHelper) helper.AddGroup("DANGER ZONE");
            UIButton recovery = null;
            recovery = (UIButton) dangerZone.AddButton(
                "Activate loading recovery mode -  READ TOOLTIP BEFORE PRESSING!",
                () =>
                {
                    Detours.SetUp();
                    FakeGameAreaManager.RecoveryMode = true;
                    recovery.Disable();
                });
            recovery.textColor = Color.red;
            recovery.tooltip = "The button is only needed if you saved with 81 Tiles after Sunset Harbor release,\n" +
                               "but before the mod was fixed for it,\n" +
                               "If you enable it without having that issue you won't be able to load a properly saved game!\n" +
                               "The option is not persisted and will be automatically disabled when the save is loaded!\n" +
                               "As soon as the save is loaded, save it again, exit the game to desktop and load it normally\n" +
                               "This button will become disabled after pressing it!";
        }
        
        public void OnEnabled() {
            HarmonyHelper.EnsureHarmonyInstalled();
        }
    }
}