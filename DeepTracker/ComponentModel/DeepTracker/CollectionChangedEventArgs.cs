using System;
using System.Collections;
using System.Collections.Specialized;
using DeepTracker1.ComponentModel.Navigation;

namespace DeepTracker1.ComponentModel
{
    public class CollectionChangedEventArgs : EventArgs
    {
        #region Constructors

        public CollectionChangedEventArgs(Route route, object collection, NotifyCollectionChangedEventArgs eventArgs)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            EventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
        }

        #endregion

        #region Properties

        public object Collection { get; }
        public NotifyCollectionChangedEventArgs EventArgs { get; }
        public Route Route { get; }

        #endregion
    }
}