using System;
using UnityEngine;

namespace _differences.Scripts.Helpers
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnumAsStringAttribute : PropertyAttribute
    {
        public readonly Type enumType;

        public EnumAsStringAttribute(Type enumType) => this.enumType = enumType;
    }
}
