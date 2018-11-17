using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeepTracker1.Extensions
{
    public static class ObjectExtensions
    {
        #region Static members

        public static bool AreEqual(this object source, object target)
        {
            if (ReferenceEquals(source, target)) return true;
            if (source == null && target == null) return true;
            if (source == null) return false;
            if (target == null) return false;

            var sourceType = source.GetType();
            var targetType = target.GetType();
            if (sourceType != targetType) return false;

            var equatableInterface = sourceType.GetTypeInfo()
                                               .ImplementedInterfaces
                                               .Where(i => i.IsGenericType)
                                               .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEquatable<>));
            if (equatableInterface != null && equatableInterface.GetGenericArguments().Single().IsAssignableFrom(targetType))
            {
                var equalMethod = equatableInterface.GetMethods().Single();
                return (bool)equalMethod.Invoke(source, new[] { target });
            }

            var equalityComparerInterface = sourceType.GetTypeInfo()
                                                      .ImplementedInterfaces
                                                      .Where(i => i.IsGenericType)
                                                      .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEqualityComparer<>));
            if (equalityComparerInterface != null && equalityComparerInterface.GetGenericArguments().Single().IsAssignableFrom(targetType))
            {
                var equalsMethod = equalityComparerInterface.GetMethods().Single(m => Equals(m.Name, nameof(Equals)));
                return (bool)equalsMethod.Invoke(source, new[] { source, target });
            }

            return Equals(source, target);
        }

        public static int GetHash(this object instance)
        {
            return instance?.GetHashCode() ?? 0;
        }

        public static int MergeHash(this int hash, int addHash)
        {
            return (hash * 397) ^ addHash;
        }

        #endregion
    }
}