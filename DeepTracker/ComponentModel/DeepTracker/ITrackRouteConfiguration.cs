using System;
using DeepTracker.ComponentModel.Navigation;

namespace DeepTracker.ComponentModel.DeepTracker
{
    public interface ITrackRouteConfiguration
    {
        #region Members

        ITrackRouteConfiguration Except(Type type, string propertyName);
        ITrackRouteConfiguration Except(Route route);

        #endregion
    }
}