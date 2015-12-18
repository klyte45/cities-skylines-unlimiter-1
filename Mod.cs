using ICities;

namespace EightyOne
{
    public class Mod : IUserMod
    {
        public static readonly string CROSS_THE_LINE_MOD = "CrossTheLine";
        public static readonly string BUILDING_THEMES_MOD = "Building Themes";
        public static readonly string REMOVE_NEED_FOR_PIPES_MOD = "Remove Need For Pipes";

        public string Description => "Allows to use all 81 map tiles";

        public string Name => "81 Tiles (Fixed for C:S 1.2+)";
    }
}
