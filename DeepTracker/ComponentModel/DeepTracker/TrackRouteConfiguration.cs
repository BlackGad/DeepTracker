using System;

namespace DeepTracker.ComponentModel.DeepTracker
{
    internal class TrackRouteConfiguration : ITrackRouteConfiguration
    {
        public ITrackRouteConfiguration Close()
        {
            return this;
        }

        public ITrackRouteConfiguration Depth(uint? depth)
        {
            return this;
        }

        public ITrackRouteConfiguration Except(Type type, string propertyName)
        {
            return this;
        }

        public ITrackRouteConfiguration Except(params string[] route)
        {
            return this;
        }
    }
}