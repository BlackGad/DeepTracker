using System;
using DeepTracker1.ComponentModel.Navigation;

namespace DeepTracker1.ComponentModel
{
    public class PropertyChangedEventArgs : EventArgs
    {
        #region Constructors

        public PropertyChangedEventArgs(Route route, PropertyReference propertyReference, object oldValue, object newValue)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            PropertyReference = propertyReference ?? throw new ArgumentNullException(nameof(propertyReference));
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        public object NewValue { get; }
        public object OldValue { get; }
        public PropertyReference PropertyReference { get; }
        public Route Route { get; }

        #endregion
    }
}