using EightyOne.Redirection;

namespace EightyOne.IgnoreAttributes
{
    public class IgnoreIfDisableZoneCheckEnabled : IgnoreIfOtherModEnabledAttribute
    {
        public IgnoreIfDisableZoneCheckEnabled() : base(Mod.DISABLE_ZONE_CHECK_MOD)
        {
        }
    }
}