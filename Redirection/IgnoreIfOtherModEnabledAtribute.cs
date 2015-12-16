using System;

namespace EightyOne.Redirection
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class IgnoreIfOtherModEnabledAtribute : Attribute
    {
        public IgnoreIfOtherModEnabledAtribute(string modName)
        {
            ModName = modName;
        }

        public string ModName { get; }
    }
}