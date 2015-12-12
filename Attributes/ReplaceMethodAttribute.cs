using System;

namespace EightyOne.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class ReplaceMethodAttribute : Attribute
    {
        public ReplaceMethodAttribute()
        {
            this.OnCreated = false;
        }

        public ReplaceMethodAttribute(bool onCreated)
        {
            this.OnCreated = onCreated;
        }

        public bool OnCreated { get; }
    }
}
