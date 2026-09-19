using HarmonyLib;
using System;
using System.Reflection;

namespace BalanceTweaksPlugin
{
    internal static class ReflectionHelper
    {
        public static FieldInfo Field(Type type, string name)
        {
            FieldInfo field = AccessTools.Field(type, name);
            WarnIfMissing(field, "field", name, type);
            return field;
        }

        public static MethodInfo Method(Type type, string name)
        {
            MethodInfo method = AccessTools.Method(type, name);
            WarnIfMissing(method, "method", name, type);
            return method;
        }

        public static MethodInfo PropertyGetter(Type type, string name)
        {
            MethodInfo getter = AccessTools.PropertyGetter(type, name);
            WarnIfMissing(getter, "property getter", name, type);
            return getter;
        }

        public static FieldInfo Field(Type type, string name, BindingFlags bindingFlags)
        {
            FieldInfo field = type.GetField(name, bindingFlags);
            WarnIfMissing(field, "field", name, type);
            return field;
        }

        public static MethodInfo Method(Type type, string name, BindingFlags bindingFlags)
        {
            MethodInfo method = type.GetMethod(name, bindingFlags);
            WarnIfMissing(method, "method", name, type);
            return method;
        }

        private static void WarnIfMissing(MemberInfo member, string kind, string name, Type declaringType)
        {
            if (member == null)
            {
                BalanceTweaksPlugin.logger?.LogWarning($"Failed to resolve {kind} '{name}' on '{declaringType?.FullName}'. The related feature will be disabled.");
            }
        }
    }
}
