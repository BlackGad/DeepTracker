using System;
using System.Collections.Generic;
using System.Linq;
using DeepTracker.ComponentModel.Extensions;
using DeepTracker.ComponentModel.Navigation;
using DeepTracker.ComponentModel.Navigation.Extensions;
using DeepTracker.DynamicSubscription;
using DeepTracker.Extensions;

namespace DeepTracker.ComponentModel.DeepTracker
{
    class TrackContext
    {
        private readonly Action<PropertyChangedEventArgs> _propertyChangedAction;
        private readonly DynamicSubscription<PropertyReference, EventHandler> _propertyChangedSubscriptions;
        private readonly WeakReference _reference;
        private readonly List<Tuple<Route, PropertyReference, object>> _registry;

        #region Constructors

        public TrackContext(TrackRouteConfiguration configuration, object source, Action<PropertyChangedEventArgs> propertyChangedAction)
        {
            Configuration = configuration;
            _reference = new WeakReference(source);
            _propertyChangedAction = propertyChangedAction;
            _registry = new List<Tuple<Route, PropertyReference, object>>();
            _propertyChangedSubscriptions = new DynamicSubscription<PropertyReference, EventHandler>(
                (changed, handler) => changed.TryAddValueChanged(handler),
                (changed, handler) => changed.TryRemoveValueChanged(handler));
        }

        #endregion

        #region Properties

        public TrackRouteConfiguration Configuration { get; }

        #endregion

        #region Members

        public void Activate()
        {
            var source = _reference.Target;
            if (source == null) return;

            AddBranch(Routes.Empty, source, Configuration, Enumerable.Empty<int>().ToList());
        }

        public void Deactivate()
        {
            _propertyChangedSubscriptions.UnsubscribeAll();
        }

        private void AddBranch(Route visitedRoute, object source, TrackRouteConfiguration configuration, IReadOnlyList<int> visitedObjects)
        {
            visitedObjects = visitedObjects.UnionWith(source.GetHash()).ToList();
            foreach (var reference in source.GetPropertyReferences())
            {
                AddBranch(visitedRoute, reference, configuration, visitedObjects);
            }
        }

        private void AddBranch(Route visitedRoute,
                               PropertyReference reference,
                               TrackRouteConfiguration configuration,
                               IReadOnlyList<int> visitedObjects)
        {
            if (reference.Name.Contains(".")) return;
            var propertyRoute = Route.Create(visitedRoute, reference.Name);

            var match = propertyRoute.Match(configuration.Route);
            if (!match) return;

            var isRestricted = configuration.IsRestricted(reference.Type, propertyRoute);
            if (isRestricted) return;

            try
            {
                if (!reference.TryGetValue(out var value)) return;

                if (reference.SupportsChangeEvents)
                {
                    var closureValue = new WeakReference(value);

                    void PropertyChangedHandler(object sender, EventArgs e)
                    {
                        var oldValue = closureValue.Target;

                        RemoveBranch(propertyRoute);

                        try
                        {
                            if (!reference.TryGetValue(out var newValue)) return;

                            //Closure detect
                            if (newValue != null && !visitedObjects.Contains(newValue.GetHash()))
                            {
                                AddBranch(propertyRoute, newValue, configuration, visitedObjects);
                            }

                            var args = new PropertyChangedEventArgs(reference.GetSource(), reference.Name, oldValue, newValue);
                            _propertyChangedAction?.Invoke(args);
                        }
                        catch
                        {
                            //Nothing
                        }
                    }

                    lock (_registry)
                    {
                        _registry.Add(new Tuple<Route, PropertyReference, object>(propertyRoute, reference, value));
                    }

                    _propertyChangedSubscriptions.Subscribe(reference, PropertyChangedHandler);
                }

                //Closure detect
                if (value != null && !visitedObjects.Contains(value.GetHash()))
                {
                    AddBranch(propertyRoute, value, configuration, visitedObjects);
                }
            }
            catch
            {
                //Nothing
            }
        }

        private void RemoveBranch(Route contextRoute)
        {
            lock (_registry)
            {
                var recordsToRemove = _registry.Where(r => r.Item1.StartWith(contextRoute)).ToList();
                foreach (var recordToRemove in recordsToRemove)
                {
                    _propertyChangedSubscriptions.Unsubscribe(recordToRemove.Item2);
                    _registry.Remove(recordToRemove);
                }
            }
        }

        #endregion
    }
}