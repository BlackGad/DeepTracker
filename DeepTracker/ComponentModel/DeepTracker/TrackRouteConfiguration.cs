using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using DeepTracker1.ComponentModel.Navigation;
using DeepTracker1.ComponentModel.Navigation.Extensions;

namespace DeepTracker1.ComponentModel
{
    internal class TrackRouteConfiguration : ITrackRouteConfiguration
    {
        #region Constants

        private static readonly List<Type> BannedTypes;

        #endregion

        private readonly Dictionary<Type, List<string>> _reflectionExceptions;
        private readonly List<Route> _routeExceptions;
        private readonly List<Type> _typeExceptions;
        private object _source;

        #region Constructors

        static TrackRouteConfiguration()
        {
            BannedTypes = new List<Type>
            {
                typeof(Type),
                typeof(Assembly),
                typeof(AppDomain),
                typeof(Dispatcher),
                typeof(Thread),
                typeof(Task)
            };
        }

        public TrackRouteConfiguration(object source, Route route)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            _reflectionExceptions = new Dictionary<Type, List<string>>();
            _routeExceptions = new List<Route>();
            _typeExceptions = new List<Type>();
        }

        #endregion

        #region Properties

        public Route Route { get; }

        #endregion

        #region ITrackRouteConfiguration Members

        public ITrackRouteConfiguration Except(Type sourceType, string propertyName)
        {
            if (!_reflectionExceptions.ContainsKey(sourceType)) _reflectionExceptions.Add(sourceType, new List<string>());
            if (!_reflectionExceptions[sourceType].Contains(propertyName))
            {
                _reflectionExceptions[sourceType].Add(propertyName);
            }

            return this;
        }

        public ITrackRouteConfiguration Except(Type propertyType)
        {
            if (!_typeExceptions.Contains(propertyType)) _typeExceptions.Add(propertyType);
            return this;
        }

        public ITrackRouteConfiguration Except(params object[] routeParts)
        {
            var route = Route.Create(routeParts);
            if (!_routeExceptions.Contains(route)) _routeExceptions.Add(route);
            return this;
        }

        public DeepTracker Create()
        {
            var result = new DeepTracker(this, _source);
            _source = null;
            return result;
        }

        #endregion

        #region Members

        public bool IsRestricted(PropertyReference reference, Route propertyRoute)
        {
            var propertyType = reference.PropertyType;
            var sourceType = reference.SourceType;

            if (BannedTypes.Contains(propertyType)) return true;
            if (_typeExceptions.Contains(propertyType)) return true;

            if (_reflectionExceptions.ContainsKey(sourceType))
            {
                var exceptionProperties = _reflectionExceptions[sourceType];
                if (exceptionProperties.Contains(reference.Name)) return true;
            }

            if (_routeExceptions.Any(r => propertyRoute.Match(r))) return true;

            return false;
        }

        #endregion
    }
}