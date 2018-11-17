using System;
using DeepTracker1.ComponentModel.Navigation;

namespace DeepTracker1.ComponentModel
{
    public interface ITrackRouteConfiguration
    {
        #region Members

        DeepTracker Create();

        ITrackRouteConfiguration Except(Type type, string propertyName);
        ITrackRouteConfiguration Except(Type type);
        ITrackRouteConfiguration Except(params object[] routeParts);

        #endregion
    }
}