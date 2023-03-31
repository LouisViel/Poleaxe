using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Poleaxe.Helper;

namespace Poleaxe.Utils.Dynamic
{
    [Serializable]
    public class DynamicClass { }

    [Serializable]
    public class DynamicField
    {
        public string name;
        public Type type;
    }

    public static class DynamicAssembly
    {
        private class Assembly
        {
            public AssemblyBuilder assemblyBuilder;
            public ModuleBuilder moduleBuilder;
        }

        private static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        public static void RemoveAssembly(string name) => assemblies.Remove(name);

        private static Assembly CreateAssembly(string name)
        {
            AssemblyName assemblyName = new AssemblyName($"Poleaxe.Dynamic.{RandomHelper.GuidWithoutNumber()}");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            Assembly assembly = new Assembly { assemblyBuilder = assemblyBuilder, moduleBuilder = moduleBuilder };
            if (!assemblies.TryAdd(name, assembly)) assemblies[name] = assembly;
            return assembly;
        }
        
        private static Assembly GetAssembly(string name)
        {
            return assemblies.TryGetValue(name, out Assembly assembly) ? assembly : CreateAssembly(name);
        }

        public static Type CreateEnum(string name, string[] values)
        {
            string enumName = $"EnumeratedTypes.{name}";
            ModuleBuilder moduleBuilder = GetAssembly(name).moduleBuilder;
            EnumBuilder enumBuilder = moduleBuilder.DefineEnum(enumName, TypeAttributes.Public, typeof(int));
            for (int i = 0; i < values.Length; ++i) enumBuilder.DefineLiteral(values[i], i);
            return enumBuilder.CreateTypeInfo().AsType();
        }

        public static Type CreateType(string name, DynamicField[] fields)
        {
            string typeName = $"DynamicClass.{name}";
            ModuleBuilder moduleBuilder = GetAssembly(name).moduleBuilder;
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, typeof(DynamicClass));
            foreach (DynamicField field in fields) typeBuilder.DefineField(field.name, field.type, FieldAttributes.Public);
            return typeBuilder.CreateTypeInfo().AsType();
        }
    }
}