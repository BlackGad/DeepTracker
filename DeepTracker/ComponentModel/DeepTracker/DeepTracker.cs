using System;
using System.Linq;
using DeepTracker.ComponentModel.Navigation;
using DeepTracker.Data;
using DeepTracker.Extensions;

namespace DeepTracker.ComponentModel.DeepTracker
{
    public class DeepTracker : IDisposable
    {
        private readonly ObjectsStorage<int, TrackContext> _trackRegistry;

        #region Constructors

        public DeepTracker()
        {
            _trackRegistry = new ObjectsStorage<int, TrackContext>();
        }

        #endregion

        #region Events

        public event EventHandler<PropertyChangedEventArgs> ObjectPropertyChanged;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Deactivate();
        }

        #endregion

        #region Members

        public void Activate()
        {
            foreach (var context in _trackRegistry.Values)
            {
                context.Activate();
            }
        }

        public void Deactivate()
        {
            foreach (var context in _trackRegistry.Values)
            {
                context.Deactivate();
            }
        }

        public ITrackRouteConfiguration Track(object source, Route route)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (!route.Any()) throw new ArgumentException("Path is empty");

            var key = source.GetHash().MergeHash(route.GetHash());

            TrackContext Factory(int r)
            {
                var configuration = new TrackRouteConfiguration(route);
                return new TrackContext(configuration, source, RaiseObjectPropertyChanged);
            }

            return _trackRegistry.GetOrCreate(key, Factory).Configuration;
        }

        public bool Untrack(object source, Route route)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (!route.Any()) throw new ArgumentException("Path is empty");

            var key = source.GetHash().MergeHash(route.GetHash());
            return _trackRegistry.Remove(key) != null;
        }

        private void RaiseObjectPropertyChanged(PropertyChangedEventArgs args)
        {
            try
            {
                ObjectPropertyChanged?.Invoke(this, args);
            }
            catch
            {
                //Nothing
            }
        }

        #endregion
    }
}