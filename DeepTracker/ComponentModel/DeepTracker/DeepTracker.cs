using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using DeepTracker1.ComponentModel.Extensions;
using DeepTracker1.ComponentModel.Navigation;
using DeepTracker1.ComponentModel.Navigation.Extensions;
using DeepTracker1.DynamicSubscription;
using DeepTracker1.Extensions;

namespace DeepTracker1.ComponentModel
{
    public class DeepTracker : IDisposable
    {
        #region Static members

        public static ITrackRouteConfiguration Setup(object source, params object[] routeParts)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var route = Route.Create(routeParts);
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (!route.Any()) throw new ArgumentException("Path is empty");

            return new TrackRouteConfiguration(source, route);
        }

        #endregion

        private readonly List<Tuple<Route, WeakReference, object[]>> _collectionChangedRegistry;
        private readonly DynamicSubscription<INotifyCollectionChanged, NotifyCollectionChangedEventHandler> _collectionChangedSubscriptions;
        private readonly ConditionalWeakTable<object, string> _collectionChildrenIds;
        private readonly TrackRouteConfiguration _configuration;
        private readonly List<Tuple<Route, PropertyReference, object>> _propertyChangedRegistry;
        private readonly DynamicSubscription<PropertyReference, EventHandler> _propertyChangedSubscriptions;
        private readonly WeakReference _reference;

        #region Constructors

        internal DeepTracker(TrackRouteConfiguration configuration, object source)
        {
            _configuration = configuration;
            _reference = new WeakReference(source);
            _propertyChangedRegistry = new List<Tuple<Route, PropertyReference, object>>();
            _collectionChangedRegistry = new List<Tuple<Route, WeakReference, object[]>>();
            _collectionChildrenIds = new ConditionalWeakTable<object, string>();

            _propertyChangedSubscriptions = new DynamicSubscription<PropertyReference, EventHandler>(
                (changed, handler) => changed.TryAddValueChanged(handler),
                (changed, handler) => changed.TryRemoveValueChanged(handler));

            _collectionChangedSubscriptions = new DynamicSubscription<INotifyCollectionChanged, NotifyCollectionChangedEventHandler>(
                (changed, handler) => changed.CollectionChanged += handler,
                (changed, handler) => changed.CollectionChanged -= handler);
        }

        #endregion

        #region Events

        public event EventHandler<CollectionChangedEventArgs> CollectionChanged;

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
            var source = _reference.Target;
            if (source == null) return;

            AddBranch(Routes.Empty, source, _configuration, Enumerable.Empty<int>().ToList());
        }

        public void Deactivate()
        {
            lock (_propertyChangedRegistry)
            {
                _propertyChangedRegistry.Clear();
                _propertyChangedSubscriptions.UnsubscribeAll();
            }

            lock (_collectionChangedRegistry)
            {
                _collectionChangedRegistry.Clear();
                _collectionChangedSubscriptions.UnsubscribeAll();
            }
        }

        private void AddBranch(Route visitedRoute, object source, TrackRouteConfiguration configuration, IReadOnlyList<int> visitedObjects)
        {
            var sourceHash = source.GetHash();
            if (visitedObjects.Contains(sourceHash)) return;

            visitedObjects = visitedObjects.UnionWith(source.GetHash()).ToList();
            if (source is IEnumerable)
            {
                var sourceItems = source.Enumerate().ToArray();

                if (source is INotifyCollectionChanged notifyCollection)
                {
                    var closureSourceItems = new WeakReference(sourceItems);

                    void NotifyCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
                    {
                        var previousItems = closureSourceItems.Target as object[];

                        if (args.Action == NotifyCollectionChangedAction.Reset)
                        {
                            foreach (var item in previousItems.Enumerate())
                            {
                                if (_collectionChildrenIds.TryGetValue(item, out string id))
                                {
                                    RemoveBranch(Route.Create(visitedRoute, id));
                                }
                            }

                            return;
                        }

                        foreach (var item in args.NewItems.Enumerate())
                        {
                            var id = Guid.NewGuid().ToString("N");
                            _collectionChildrenIds.Add(item, id);
                            AddBranch(Route.Create(visitedRoute, id), item, _configuration, visitedObjects);
                        }

                        foreach (var item in args.OldItems.Enumerate())
                        {
                            if (_collectionChildrenIds.TryGetValue(item, out string id))
                            {
                                RemoveBranch(Route.Create(visitedRoute, id));
                            }
                        }

                        var collectionChangedEventArgs = new CollectionChangedEventArgs(visitedRoute, sender, args);
                        CollectionChanged?.Invoke(this, collectionChangedEventArgs);
                    }

                    lock (_collectionChangedRegistry)
                    {
                        Debug.WriteLine($"+++ COLLECTION {visitedRoute}");
                        var record = new Tuple<Route, WeakReference, object[]>(visitedRoute, new WeakReference(source), sourceItems);
                        _collectionChangedRegistry.Add(record);
                    }

                    _collectionChangedSubscriptions.Subscribe(notifyCollection, NotifyCollectionChangedHandler);
                }

                foreach (var item in sourceItems)
                {
                    var id = Guid.NewGuid().ToString("N");
                    _collectionChildrenIds.Add(item, id);
                    AddBranch(Route.Create(visitedRoute, id), item, _configuration, visitedObjects);
                }
            }

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

            var match = false;
            var currentConfigurationRoute = configuration.Route;
            while (!currentConfigurationRoute.IsEmpty())
            {
                match = propertyRoute.Match(currentConfigurationRoute);
                if (match) break;
                currentConfigurationRoute = Route.Create(currentConfigurationRoute.Take(currentConfigurationRoute.Count - 1));
            }

            if (!match) return;

            var isRestricted = configuration.IsRestricted(reference, propertyRoute);
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

                            if (newValue != null)
                            {
                                closureValue = new WeakReference(newValue);
                                AddBranch(propertyRoute, newValue, configuration, visitedObjects);
                            }

                            var args = new PropertyChangedEventArgs(propertyRoute, reference, oldValue, newValue);
                            ObjectPropertyChanged?.Invoke(this, args);
                        }
                        catch
                        {
                            //Nothing
                        }
                    }

                    lock (_propertyChangedRegistry)
                    {
                        Debug.WriteLine($"+++ PROPERTY {propertyRoute}");
                        var record = new Tuple<Route, PropertyReference, object>(propertyRoute, reference, value);
                        _propertyChangedRegistry.Add(record);
                        _propertyChangedSubscriptions.Subscribe(reference, PropertyChangedHandler);
                    }
                }

                if (value != null) AddBranch(propertyRoute, value, configuration, visitedObjects);
            }
            catch
            {
                //Nothing
            }
        }

        private void RemoveBranch(Route contextRoute)
        {
            lock (_propertyChangedRegistry)
            {
                var recordsToRemove = _propertyChangedRegistry.Where(r => r.Item1.StartWith(contextRoute)).ToList();
                foreach (var recordToRemove in recordsToRemove)
                {
                    if (recordToRemove.Item1.Match(contextRoute)) continue;

                    Debug.WriteLine($"--- PROPERTY {recordToRemove.Item1}");
                    _propertyChangedSubscriptions.Unsubscribe(recordToRemove.Item2);
                    _propertyChangedRegistry.Remove(recordToRemove);
                }
            }

            lock (_collectionChangedRegistry)
            {
                var recordsToRemove = _collectionChangedRegistry.Where(r => r.Item1.StartWith(contextRoute)).ToList();
                foreach (var recordToRemove in recordsToRemove)
                {
                    Debug.WriteLine($"--- COLLECTION {recordToRemove.Item1}");
                    _collectionChangedSubscriptions.Unsubscribe(recordToRemove.Item2.Target);
                    _collectionChangedRegistry.Remove(recordToRemove);
                }
            }
        }

        #endregion
    }
}