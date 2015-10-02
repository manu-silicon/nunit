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
#if PORTABLE
    /// <summary>
    /// Provides NUnit specific extensions to the
    /// MemberInfo class
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns an array of custom attributes of the specified type applied to this member
        /// </summary>
        /// <remarks> Portable throws an argument exception if T does not
        /// derive from Attribute. NUnit uses interfaces to find attributes, thus
        /// this method</remarks>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo info, bool inherit) where T : class
        {
            return GetAttributesImpl<T>(info.GetCustomAttributes(inherit));
        }

        /// <summary>
        /// Returns an array of custom attributes of the specified type applied to this parameter
        /// </summary>
        public static IEnumerable<T> GetAttributes<T>(this ParameterInfo info, bool inherit) where T : class
        {
            return GetAttributesImpl<T>(info.GetCustomAttributes(inherit));
        }

        /// <summary>
        /// Returns an array of custom attributes of the specified type applied to this assembly
        /// </summary>
        public static IEnumerable<T> GetAttributes<T>(this Assembly info) where T : class
        {
            return GetAttributesImpl<T>(info.GetCustomAttributes());
        }

        private static IEnumerable<T> GetAttributesImpl<T>(IEnumerable<Attribute> attributes) where T : class
        {
            var attrs = new List<T>();

            attributes.Where(a => typeof(T).IsAssignableFrom(a.GetType()))
                .All(a => { attrs.Add(a as T); return true; });

            return attrs;
        }
    }
#endif

    /// <summary>
    /// Provides extensions on Type that are not available
    /// in our portable class library
    /// </summary>
    public static class TypeExtensions
    {
#if PORTABLE
        /// <summary>
        /// Is type assignable from another type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this Type type, Type from)
        {
            return type.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
        }

        /// <summary>
        /// Is object an instance of type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool IsInstanceOfType(this Type type, object instance)
        {
            return instance == null ? false : instance.GetType() == type;
        }

        /// <summary>
        /// Get the constructors for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors;
        }

        /// <summary>
        /// Gets the field with the given name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetField(this Type type, string name)
        {
            return from field in GetFieldsImpl(type, true)
                   where field.Name == name && field.IsPublic == true
                   select field;
        }

        /// <summary>
        /// Get the interfaces for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        /// <summary>
        /// Get a constructor for the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ConstructorInfo GetConstructor(this Type type, Type[] args)
        {
            return type.GetTypeInfo()
                .DeclaredConstructors
                .Where(ctor => ParametersMatch(ctor.GetParameters(), args))
                .FirstOrDefault();
        }

        /// <summary>
        /// Get the generic arguments for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        /// <summary>
        /// Get the members of a type by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static MemberInfo[] GetMember(this Type type, string name, BindingFlags flags)
        {
            var comparisonType = flags.HasFlag(BindingFlags.IgnoreCase) ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return GetMembers(type, flags).Where(m => m.Name.Equals(name, comparisonType)).ToArray();
        }

        /// <summary>
        /// Get the members of a type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetMembers(this Type type, BindingFlags flags)
        {
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var members = GetMembersImpl(type, children);

            return members;
        }

        /// <summary>
        /// Get a property of a type by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return GetProperty(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        /// <summary>
        /// Get a property of a type by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            var comparisonType = flags.HasFlag(BindingFlags.IgnoreCase) ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var properties = GetPropertiesImpl(type, children);

            return properties.Where(p => p.Name.Equals(name, comparisonType)).FirstOrDefault();
        }

        /// <summary>
        /// Get a method of a type by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this Type type, string name)
        {
            return GetMethod(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        /// <summary>
        /// Get a method of a type by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
        {
            return GetMethods(type, name, flags).FirstOrDefault();
        }

        /// <summary>
        /// Get a method of a type by name with specified arguments
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this Type type, string name, Type[] args)
        {
            return GetMethods(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(method => ParametersMatch(method.GetParameters(), args))
                .FirstOrDefault();
        }

        private static bool ParametersMatch(ParameterInfo[] prms, Type[] args)
        {
            if (args.Length != prms.Length)
                return false;

            bool match = true;
            for (int i = 0; i < args.Length; i++)
            {
                if (prms[i].ParameterType == args[i])
                {
                    match = false;
                    break;
                }
            }
            return match;
        }

        /// <summary>
        /// Get methods on a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethods(this Type type)
        {
            return GetMethodsImpl(type, true).ToArray();
        }

        /// <summary>
        /// Get methods on a type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags)
        {
            var children = !flags.HasFlag(BindingFlags.DeclaredOnly);
            var methods = GetMethodsImpl(type, children);

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

        /// <summary>
        /// Get methods on a type by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethods(this Type type, string name, BindingFlags flags)
        {
            var comparisonType = flags.HasFlag(BindingFlags.IgnoreCase) ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return GetMethods(type, flags).Where(m => m.Name.Equals(name, comparisonType));
        }

        private static IEnumerable<FieldInfo> GetFieldsImpl(Type type, bool children)
        {
            var info = type.GetTypeInfo();
            if (info.BaseType != null && children)
                foreach (var members in GetFieldsImpl(info.BaseType, children))
                    yield return members;

            foreach (var members in info.DeclaredFields)
                yield return members;
        }

        private static IEnumerable<MemberInfo> GetMembersImpl(Type type, bool children)
        {
            var info = type.GetTypeInfo();
            if (info.BaseType != null && children)
                foreach (var members in GetMembersImpl(info.BaseType, children))
                    yield return members;

            foreach (var members in info.DeclaredMembers)
                yield return members;
        }

        private static IEnumerable<PropertyInfo> GetPropertiesImpl(Type type, bool children)
        {
            var info = type.GetTypeInfo();
            if (info.BaseType != null && children)
                foreach (var property in GetPropertiesImpl(info.BaseType, children))
                    yield return property;

            foreach (var property in info.DeclaredProperties)
                yield return property;
        }

        private static IEnumerable<MethodInfo> GetMethodsImpl(Type type, bool children)
        {
            var info = type.GetTypeInfo();
            if (info.BaseType != null && children)
                foreach (var method in GetMethodsImpl(info.BaseType, children))
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
