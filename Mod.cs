using ICities;

namespace EightyOne
{
    public class Mod : IUserMod
    {
        public static readonly string CROSS_THE_LINE_MOD = "CrossTheLine";
        public static readonly string BUILDING_THEMES_MOD = "Building Themes";
        public static readonly string REMOVE_NEED_FOR_PIPES_MOD = "Remove Need For Pipes";
        public static readonly string SURFACE_PAINTER_MOD = "Surface Painter";

        public string Description => "Allows to use all 81 map tiles";

        public string Name => "81 Tiles (Fixed for C:S 1.2+)";

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddButton("Unlock all tiles for free", UnlockAllCheat.UnlockAllAreas);
        }

    }
}
