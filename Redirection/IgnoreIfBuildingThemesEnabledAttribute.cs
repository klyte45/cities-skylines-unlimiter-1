namespace EightyOne.Redirection
{
    public class IgnoreIfBuildingThemesEnabledAttribute : IgnoreIfOtherModEnabledAtribute
    {
        public IgnoreIfBuildingThemesEnabledAttribute() : base(Mod.BUILDING_THEMES_MOD)
        {
        }
    }
}