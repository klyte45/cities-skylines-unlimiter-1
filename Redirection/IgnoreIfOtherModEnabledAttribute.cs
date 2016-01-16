using System;

namespace EightyOne.Redirection
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class IgnoreIfOtherModEnabledAttribute : Attribute
    {
        protected IgnoreIfOtherModEnabledAttribute(string modName)
        {
            ModName = modName;
        }

        public string ModName { get; }
    }
}