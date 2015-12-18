using System;

namespace EightyOne.Redirection
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class IgnoreIfOtherModEnabledAtribute : Attribute
    {
        protected IgnoreIfOtherModEnabledAtribute(string modName)
        {
            ModName = modName;
        }

        public string ModName { get; }
    }
}