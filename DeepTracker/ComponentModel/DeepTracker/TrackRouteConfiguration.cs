using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using DeepTracker.ComponentModel.Navigation;

namespace DeepTracker.ComponentModel.DeepTracker
{
    internal class TrackRouteConfiguration : ITrackRouteConfiguration
    {
        #region Constructors

        public TrackRouteConfiguration(Route route)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
        }

        #endregion

        #region Properties

        public Route Route { get; }

        #endregion

        #region ITrackRouteConfiguration Members

        public ITrackRouteConfiguration Except(Type type, string propertyName)
        {
            return this;
        }

        public ITrackRouteConfiguration Except(Route route)
        {
            return this;
        }

        #endregion

        #region Members

        public bool IsRestricted(Type sourceType, Route propertyRoute)
        {
            if (typeof(Type).IsAssignableFrom(sourceType)) return true;
            if (typeof(Assembly).IsAssignableFrom(sourceType)) return true;
            if (typeof(AppDomain).IsAssignableFrom(sourceType)) return true;
            if (typeof(Dispatcher).IsAssignableFrom(sourceType)) return true;
            if (typeof(Thread).IsAssignableFrom(sourceType)) return true;
            if (typeof(Task).IsAssignableFrom(sourceType)) return true;
            return false;
        }

        public bool IsValid(Route propertyRoute)
        {
            return false;
        }

        #endregion
    }
}