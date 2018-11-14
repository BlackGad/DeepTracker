using System;

namespace DeepTracker.ComponentModel.DeepTracker
{
    public class PropertyChangedEventArgs : EventArgs
    {
        #region Constructors

        public PropertyChangedEventArgs(object source, string property, object oldValue, object newValue)
        {
            PropertyName = property ?? throw new ArgumentNullException(nameof(property));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        public object NewValue { get; }
        public object OldValue { get; }
        public string PropertyName { get; }
        public object Source { get; }

        #endregion
    }
}