using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeepTracker.Data;
using DeepTracker.Extensions;

namespace DeepTracker.ComponentModel.Extensions
{
    public static class PropertyReferenceExtensions
    {
        #region Constants

        private static readonly ObjectsStorage<Type, IReadOnlyList<PropertyDescriptor>> CachedTypeProperties;

        #endregion

        #region Static members

        public static IReadOnlyList<PropertyReference> GetPropertyReferences(this object source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var type = source.GetType();
            return CachedTypeProperties[type].Select(p => new PropertyReference(source, p)).ToList();
        }

        #endregion

        #region Constructors

        static PropertyReferenceExtensions()
        {
            CachedTypeProperties = new ObjectsStorage<Type, IReadOnlyList<PropertyDescriptor>>(type =>
            {
                try
                {
                    return TypeDescriptor.GetProperties(type).Enumerate<PropertyDescriptor>().ToList();
                }
                catch (Exception)
                {
                    return Enumerable.Empty<PropertyDescriptor>().ToList();
                }
            });
        }

        #endregion
    }
}