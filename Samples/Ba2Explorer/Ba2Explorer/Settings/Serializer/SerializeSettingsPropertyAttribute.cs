using System;

namespace Ba2Explorer.Settings.Serializer
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    sealed class SerializeSettingsPropertyAttribute : Attribute
    {
    }
}
