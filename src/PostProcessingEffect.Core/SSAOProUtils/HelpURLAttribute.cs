using System;

namespace SSAOProUtils
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HelpURLAttribute : Attribute
    {
        public HelpURLAttribute(string url)
        {
        }
    }
}
