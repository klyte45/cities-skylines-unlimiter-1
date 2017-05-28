using System;
using System.Reflection;
using EightyOne.RedirectionFramework.Attributes;

namespace EightyOne
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class IgnoreIfOtherModEnabledAttribute : IgnoreConditionAttribute
    {
        protected IgnoreIfOtherModEnabledAttribute(ulong id, string modName)
        {
            ModWorkshopId = id;
            ModName = modName;
        }

        public override bool IsIgnored(MemberInfo methodInfo)
        {
            return Util.IsModActive(ModWorkshopId) || Util.IsModActive(ModName);
        }

        protected ulong ModWorkshopId { get;  }

        protected string ModName { get; }
    }
}