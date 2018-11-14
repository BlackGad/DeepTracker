using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using DeepTracker.DynamicSubscription;
using DeepTracker.Extensions;

namespace DeepTracker.ComponentModel.DeepTracker
{
    public class DeepTracker
    {
        private readonly DynamicSubscription<PropertyReference, EventHandler> _propertyChangedSubscriptions;
        private readonly WeakReference _source;
        private readonly Dictionary<Route, ITrackRouteConfiguration> _trackConfiguration;
        private readonly ConditionalWeakTable<object, DeepTrackerObjectReferences> _trackedObjects;

        #region Constructors

        public DeepTracker(object source)
        {
            _source = new WeakReference(source);
            _trackConfiguration = new Dictionary<Route, ITrackRouteConfiguration>();
            _trackedObjects = new ConditionalWeakTable<object, DeepTrackerObjectReferences>();
            _propertyChangedSubscriptions = new DynamicSubscription<PropertyReference, EventHandler>(
                (changed, handler) => changed.TryAddValueChanged(handler),
                (changed, handler) => changed.TryRemoveValueChanged(handler));
        }

        #endregion

        #region Events

        public event EventHandler<PropertyChangedEventArgs> ObjectPropertyChanged;

        #endregion

        #region Event handlers

        private void PropertyChangedHandler(object sender, EventArgs e)
        {
        }

        #endregion

        #region Members

        public void Activate()
        {
            var source = _source.Target;
            if (source == null) return;

            Process(source, 0);
        }

        public void Deactivate()
        {
        }

        public ITrackRouteConfiguration Track(params object[] path)
        {
            var route = new Route(path);
            if (route.IsEmpty) throw new ArgumentException("Path is empty");
            return _trackConfiguration.GetOrAdd(route, r => new TrackRouteConfiguration());
        }

        private void Process(object source, int level)
        {
            var trackedPropertiesOnThisLevel = _trackConfiguration.Keys
                                                                  .Where(p => level < p.Count)
                                                                  .Select(p => p[level])
                                                                  .ToList();

            var properties = new Lazy<List<PropertyDescriptor>>(() => TypeDescriptor.GetProperties(source).Enumerate<PropertyDescriptor>().ToList());

            foreach (var propertyName in trackedPropertiesOnThisLevel)
            {
                var references = _trackedObjects.GetOrCreateValue(source);

                PropertyReferenceWithValue PropertyReferenceFactory()
                {
                    var descriptor = properties.Value.FirstOrDefault(p => p.Name.AreEqual(propertyName));
                    if (descriptor == null) return null;

                    WeakReference value = null;
                    try
                    {
                        value = new WeakReference(descriptor.GetValue(source));
                    }
                    catch
                    {
                        //Nothing
                    }

                    return new PropertyReferenceWithValue
                    {
                        Reference = new PropertyReference(source, descriptor),
                        Value = value
                    };
                }

                var propertyReference = references.GetOrAdd(propertyName, PropertyReferenceFactory);
                if (propertyReference == null) continue;

                var propertyValue = propertyReference.Value?.Target;
                if (propertyValue != null)
                {
                    Process(propertyValue, level + 1);
                }

                if (propertyReference.Reference.SupportsChangeEvents)
                {
                    propertyReference.Reference.TryAddValueChanged(PropertyChangedHandler);

                    void PropertyChangedHandler(object sender, EventArgs args)
                    {
                        //TODO: Unsubscribe old branch
                        //TODO: Subscribe new branch

                        //Process()
                        ObjectPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName, propertyValue,));
                    }

                    _propertyChangedSubscriptions.Subscribe(propertyReference, PropertyChangedHandler);
                }
            }
        }

        #endregion

        #region Nested type: DeepTrackerObjectReferences

        private class DeepTrackerObjectReferences
        {
            private readonly ConcurrentDictionary<string, Lazy<PropertyReferenceWithValue>> _references;

            #region Constructors

            public DeepTrackerObjectReferences()
            {
                _references = new ConcurrentDictionary<string, Lazy<PropertyReferenceWithValue>>();
            }

            #endregion

            #region Members

            public PropertyReferenceWithValue GetOrAdd(string property, Func<PropertyReferenceWithValue> factory)
            {
                return _references.GetOrAdd(property,
                                            name => new Lazy<PropertyReferenceWithValue>(factory,
                                                                                         LazyThreadSafetyMode.ExecutionAndPublication)).Value;
            }

            #endregion
        }

        #endregion

        #region Nested type: PropertyReferenceWithValue

        private class PropertyReferenceWithValue
        {
            #region Properties

            public PropertyReference Reference { get; set; }
            public WeakReference Value { get; set; }

            #endregion
        }

        #endregion
    }
}