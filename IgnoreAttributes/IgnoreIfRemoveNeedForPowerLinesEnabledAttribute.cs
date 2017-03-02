using EightyOne.Redirection;

namespace EightyOne.IgnoreAttributes
{
    public class IgnoreIfRemoveNeedForPowerLinesEnabledAttribute : IgnoreIfOtherModEnabledAttribute
    {
        public IgnoreIfRemoveNeedForPowerLinesEnabledAttribute() : base(Mod.REMOVE_NEED_FOR_POWER_LINES_MOD)
        {
        }
    }
}