namespace EightyOne.Redirection
{
    public class IgnoreIfBuildingThemesEnabledAttribute : IgnoreIfOtherModEnabledAttribute
    {
        public IgnoreIfBuildingThemesEnabledAttribute() : base(Mod.BUILDING_THEMES_MOD)
        {
        }
    }
}