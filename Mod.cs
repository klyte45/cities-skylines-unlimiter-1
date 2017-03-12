using ICities;

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

        public string Description => "Allows to use all 81 map tiles";

        public string Name => "81 Tiles (Fixed for C:S 1.2+)";

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddButton(UNLOCK_ALL_TILES_FOR_FREE, UnlockAllCheat.UnlockAllAreas);
        }
    }
}
