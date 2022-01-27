using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ScyberLog
{
    internal static class IEnumerableExtensions
    {
        /// <summary>
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">action is null</exception>
        [DebuggerHidden]
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            if (collection == null) { return; }
            foreach (var item in collection) { action(item); }
        }
    }
}