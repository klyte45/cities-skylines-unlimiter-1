namespace EightyOne.Redirection
{
    public class IgnoreIfSurfacePainterEnabledAttribute : IgnoreIfOtherModEnabledAttribute
    {
        public IgnoreIfSurfacePainterEnabledAttribute() : base(Mod.SURFACE_PAINTER_MOD)
        {
        }
    }
}