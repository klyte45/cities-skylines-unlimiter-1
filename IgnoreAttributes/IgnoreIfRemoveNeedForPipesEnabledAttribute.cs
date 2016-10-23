using EightyOne.Redirection;

namespace EightyOne.IgnoreAttributes
{
    public class IgnoreIfRemoveNeedForPipesEnabledAttribute : IgnoreIfOtherModEnabledAttribute
    {
        public IgnoreIfRemoveNeedForPipesEnabledAttribute() : base(Mod.REMOVE_NEED_FOR_PIPES_MOD)
        {
        }
    }
}