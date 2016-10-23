using EightyOne.Redirection;

namespace EightyOne.IgnoreAttributes
{
    public class IgnoreIfSurfacePainterEnabledAttribute : IgnoreIfOtherModEnabledAttribute
    {
        public IgnoreIfSurfacePainterEnabledAttribute() : base(Mod.SURFACE_PAINTER_MOD)
        {
        }
    }
}