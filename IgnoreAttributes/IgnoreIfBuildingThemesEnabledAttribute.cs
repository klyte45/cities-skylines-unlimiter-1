using EightyOne.Redirection;

namespace EightyOne.IgnoreAttributes
{
    public class IgnoreIfBuildingThemesEnabledAttribute : IgnoreIfOtherModEnabledAttribute
    {
        public IgnoreIfBuildingThemesEnabledAttribute() : base(Mod.BUILDING_THEMES_MOD)
        {
        }
    }
}