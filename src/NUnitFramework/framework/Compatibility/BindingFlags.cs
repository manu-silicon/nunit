// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

#if PORTABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NUnit.Framework.Compatibility
{
    static class TypeExtensions
    {
        public static bool IsAssignableFrom(this Type type, Type from)
        {
            return type.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
        }

        public static IEnumerable<MemberInfo> GetMembers(this Type type, BindingFlags flags)
        {
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var members = GetMembers(type, children);

            return members;
        }

        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return GetProperty(type, name, BindingFlags.Default);
        }

        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            var comparisonType = flags.HasFlag(BindingFlags.IgnoreCase) ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var properties = GetProperties(type, children);

            return properties.Where(p => p.Name.Equals(name, comparisonType)).FirstOrDefault();
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
        {
            return GetMethods(type, name, flags).FirstOrDefault();
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] args)
        {
            foreach(MethodInfo method in GetMethods(type, name, BindingFlags.Default))
            {
                var prms = method.GetParameters();
                if (args.Length != prms.Length)
                    continue;

                bool match = true;
                for(int i=0; i<args.Length; i++)
                {
                    if (!prms[0].ParameterType.IsAssignableFrom(args[0]))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return method;
            }
            return null;
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type type, BindingFlags flags)
        {
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var methods = GetMethods(type, children);

            // Instance/Static
            if (flags.HasFlag(BindingFlags.Instance) && !flags.HasFlag(BindingFlags.Static))
                methods = methods.Where(m => !m.IsStatic);
            else if (flags.HasFlag(BindingFlags.Static) && !flags.HasFlag(BindingFlags.Instance))
                methods = methods.Where(m => m.IsStatic);

            // Public/NonPublic
            if (flags.HasFlag(BindingFlags.NonPublic) && !flags.HasFlag(BindingFlags.Public))
                methods = methods.Where(m => !m.IsPublic);
            else if (flags.HasFlag(BindingFlags.Public) && !flags.HasFlag(BindingFlags.NonPublic))
                methods = methods.Where(m => m.IsPublic);

            return methods;
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type type, string name, BindingFlags flags)
        {
            var comparisonType = flags.HasFlag(BindingFlags.IgnoreCase) ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return GetMethods(type, flags).Where(m => m.Name.Equals(name, comparisonType));
        }

        static IEnumerable<MemberInfo> GetMembers(Type type, bool children)
        {
            var info = type.GetTypeInfo();
            if (info.BaseType != null && children)
                foreach (var members in GetMembers(info.BaseType, children))
                    yield return members;

            foreach (var members in info.DeclaredMembers)
                yield return members;
        }

        static IEnumerable<PropertyInfo> GetProperties(Type type, bool children)
        {
            var info = type.GetTypeInfo();
            if (info.BaseType != null && children)
                foreach (var property in GetProperties(info.BaseType, children))
                    yield return property;

            foreach (var property in info.DeclaredProperties)
                yield return property;
        }

        static IEnumerable<MethodInfo> GetMethods(Type type, bool children)
        {
            var info = type.GetTypeInfo();
            if (info.BaseType != null && children)
                foreach (var method in GetMethods(info.BaseType, children))
                    yield return method;

            foreach (var method in info.DeclaredMethods)
                yield return method;
        }
    }

    /// <summary>
    /// Specifies flags that control binding and the way in which the search for members
    /// and types is conducted by reflection.
    /// </summary>
    [Flags]
    public enum BindingFlags
    {
        /// <summary>
        /// Specifies no binding flag.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Specifies that the case of the member name should not be considered when binding.
        /// </summary>
        IgnoreCase = 1,
        /// <summary>
        /// Specifies that only members declared at the level of the supplied type's hierarchy
        /// should be considered. Inherited members are not considered.
        /// </summary>
        DeclaredOnly = 2,
        /// <summary>
        /// Specifies that instance members are to be included in the search.
        /// </summary>
        Instance = 4,
        /// <summary>
        /// Specifies that static members are to be included in the search.
        /// </summary>
        Static = 8,
        /// <summary>
        /// Specifies that public members are to be included in the search.
        /// </summary>
        Public = 16,
        /// <summary>
        /// Specifies that non-public members are to be included in the search.
        /// </summary>
        NonPublic = 32,
        /// <summary>
        /// Specifies that public and protected static members up the hierarchy should be
        /// returned. Private static members in inherited classes are not returned. Static
        /// members include fields, methods, events, and properties. Nested types are not
        /// returned.
        /// </summary>
        FlattenHierarchy = 64,
        /// <summary>
        /// Specifies that a method is to be invoked. This must not be a constructor or a
        /// type initializer.
        /// </summary>
        //InvokeMethod = 256,
        /// <summary>
        /// Specifies that Reflection should create an instance of the specified type. Calls
        /// the constructor that matches the given arguments. The supplied member name is
        /// ignored. If the type of lookup is not specified, (Instance | Public) will apply.
        /// It is not possible to call a type initializer.
        /// </summary>
        //CreateInstance = 512,
        /// <summary>
        /// Specifies that the value of the specified field should be returned.
        /// </summary>
        //GetField = 1024,
        /// <summary>
        /// Specifies that the value of the specified field should be set.
        /// </summary>
        //SetField = 2048,
        /// <summary>
        /// Specifies that the value of the specified property should be returned.
        /// </summary>
        //GetProperty = 4096,
        /// <summary>
        /// Specifies that the value of the specified property should be set. For COM properties,
        /// specifying this binding flag is equivalent to specifying PutDispProperty and
        /// PutRefDispProperty.
        /// </summary>
        //SetProperty = 8192
    }
}
#endif