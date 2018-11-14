using System;

namespace DeepTracker.ComponentModel.DeepTracker
{
    public interface ITrackRouteConfiguration
    {
        ITrackRouteConfiguration Close();
        ITrackRouteConfiguration Depth(uint? depth);
        ITrackRouteConfiguration Except(Type type, string propertyName);
        ITrackRouteConfiguration Except(params string[] route);
    }
}