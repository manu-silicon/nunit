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

namespace NUnit.Framework.Compatibility
{
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
    }
}
#endif