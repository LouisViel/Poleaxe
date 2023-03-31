using System;
using System.Reflection;

namespace Poleaxe.Helper
{
    public static class ReflectionHelper
    {
        public const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /////////////////////////////////////////////////
        //////////////////  GET VALUE  //////////////////
        /////////////////////////////////////////////////

        public static object GetValue(object obj, string name) => GetValue<object>(obj, name, Flags);
        public static object GetValue(object obj, string name, BindingFlags flags) => GetValue<object>(obj, name, flags);
        public static t GetValue<t>(object obj, string name) => GetValue<t>(obj, name, Flags);
        public static t GetValue<t>(object obj, string name, BindingFlags flags) => (t)GetMember(obj, name, flags)?.GetValue(obj);
        public static object GetValue(this MemberInfo member, object obj)
        {
            if (member.MemberType == MemberTypes.Field) return ((FieldInfo)member).GetValue(obj);
            if (member.MemberType == MemberTypes.Property) return ((PropertyInfo)member).GetValue(obj);
            return null;
        }

        /////////////////////////////////////////////////
        //////////////////  SET VALUE  //////////////////
        /////////////////////////////////////////////////

        public static void SetValue(object obj, string name, object value) => SetValue(obj, name, value, Flags);
        public static void SetValue(object obj, string name, object value, BindingFlags flags) => GetMember(obj, name, flags)?.SetValue(obj, value);
        public static void SetValue(this MemberInfo member, object obj, object value)
        {
            if (member.MemberType == MemberTypes.Field) ((FieldInfo)member).SetValue(obj, value);
            else if (member.MemberType == MemberTypes.Property) ((PropertyInfo)member).SetValue(obj, value);
        }

        /////////////////////////////////////////////////
        /////////////////  GET MEMBER  //////////////////
        /////////////////////////////////////////////////

        public static MemberInfo GetMember(object obj, string name) => GetMember(obj.GetType(), name, Flags);
        public static MemberInfo GetMember(object obj, string name, BindingFlags flags) => GetMember(obj.GetType(), name, flags);
        public static MemberInfo GetMember(Type type, string name) => GetMember(type, name, Flags);
        public static MemberInfo GetMember(Type type, string name, BindingFlags flags)
        {
            MemberInfo info = GetField(type, name, flags);
            return info ?? GetProperty(type, name, flags);
        }

        /////////////////////////////////////////////////
        //////////////////  GET FIELD  //////////////////
        /////////////////////////////////////////////////

        public static FieldInfo GetField(object obj, string name) => GetField(obj.GetType(), name, Flags);
        public static FieldInfo GetField(object obj, string name, BindingFlags flags) => GetField(obj.GetType(), name, flags);
        public static FieldInfo GetField(Type type, string name) => GetField(type, name, Flags);
        public static FieldInfo GetField(Type type, string name, BindingFlags flags)
        {
            while (type != null) {
                FieldInfo info = type.GetField(name, flags);
                if (info != null) return info;
                type = type.BaseType;
            }
            return null;
        }

        /////////////////////////////////////////////////
        ////////////////  GET PROPERTY  /////////////////
        /////////////////////////////////////////////////

        public static PropertyInfo GetProperty(object obj, string name) => GetProperty(obj.GetType(), name, Flags);
        public static PropertyInfo GetProperty(object obj, string name, BindingFlags flags) => GetProperty(obj.GetType(), name, flags);
        public static PropertyInfo GetProperty(Type type, string name) => GetProperty(type, name, Flags);
        public static PropertyInfo GetProperty(Type type, string name, BindingFlags flags)
        {
            while (type != null) {
                PropertyInfo info = type.GetProperty(name, flags);
                if (info != null) return info;
                type = type.BaseType;
            }
            return null;
        }

        /////////////////////////////////////////////////
        /////////////////  GET METHOD  //////////////////
        /////////////////////////////////////////////////

        public static MethodInfo GetMethod(object obj, string name) => GetMethod(obj.GetType(), name, Flags);
        public static MethodInfo GetMethod(object obj, string name, BindingFlags flags) => GetMethod(obj.GetType(), name, flags);
        public static MethodInfo GetMethod(Type type, string name) => GetMethod(type, name, Flags);
        public static MethodInfo GetMethod(Type type, string name, BindingFlags flags)
        {
            while (type != null) {
                MethodInfo info = type.GetMethod(name, flags);
                if (info != null) return info;
                type = type.BaseType;
            }
            return null;
        }

        /////////////////////////////////////////////////
        ////////////////  GET GENERIC  //////////////////
        /////////////////////////////////////////////////

        public static Type GetGeneric(object obj) => GetGeneric(obj.GetType());
        public static Type GetGeneric(Type type)
        {
            while (type != null) {
                if (type.IsGenericType) {
                    foreach (Type generic in type.GenericTypeArguments) {
                        if (generic != null) return generic;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}