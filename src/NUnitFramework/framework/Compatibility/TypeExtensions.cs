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

using System;
#if PORTABLE
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endif

#if NET_2_0
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Enables compiling extension methods in .NET 2.0
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}
#endif

namespace NUnit.Framework.Compatibility
{
    static class TypeExtensions
    {
#if PORTABLE
        public static bool IsAssignableFrom(this Type type, Type from)
        {
            return type.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
        }

        public static bool IsInstanceOfType(this Type type, object instance)
        {
            return instance == null ? false : instance.GetType() == type;
        }

        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors;
        }

        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] args)
        {
            foreach (ConstructorInfo ctor in type.GetTypeInfo().DeclaredConstructors)
            {
                var prms = ctor.GetParameters();
                if (args.Length != prms.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < args.Length; i++)
                {
                    if (!prms[0].ParameterType.IsAssignableFrom(args[0]))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return ctor;
            }
            return null;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeParameters;
        }

        public static MemberInfo[] GetMember(this Type type, string name, BindingFlags flags)
        {
            var comparisonType = flags.HasFlag(BindingFlags.IgnoreCase) ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return GetMembers(type, flags).Where(m => m.Name.Equals(name, comparisonType)).ToArray();
        }

        public static IEnumerable<MemberInfo> GetMembers(this Type type, BindingFlags flags)
        {
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var members = GetMembers(type, children);

            return members;
        }

        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return GetProperty(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            var comparisonType = flags.HasFlag(BindingFlags.IgnoreCase) ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var properties = GetProperties(type, children);

            return properties.Where(p => p.Name.Equals(name, comparisonType)).FirstOrDefault();
        }

        public static MethodInfo GetMethod(this Type type, string name)
        {
            return GetMethod(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
        {
            return GetMethods(type, name, flags).FirstOrDefault();
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] args)
        {
            foreach (MethodInfo method in GetMethods(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                var prms = method.GetParameters();
                if (args.Length != prms.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < args.Length; i++)
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

        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags)
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

            return methods.ToArray();
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
#elif !NET_4_5
        /// <summary>
        /// GetTypeInfo gives access to most of the Type information we take for granted
        /// on .NET Core and Windows Runtime. Rather than #ifdef different code for different
        /// platforms, it is easiest to just code all platforms as if they worked this way,
        /// thus the simple passthrough.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
#endif
    }
}
