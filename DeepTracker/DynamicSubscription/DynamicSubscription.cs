using System;
using System.Runtime.CompilerServices;

namespace DeepTracker.DynamicSubscription
{
    public class DynamicSubscription<TObject, THandler>
        where TObject : class
        where THandler : class
    {
        private readonly Action<TObject, THandler> _subscribeAction;
        private readonly ConditionalWeakTable<TObject, THandler> _table;
        private readonly Action<TObject, THandler> _unsubscribeAction;

        #region Constructors

        public DynamicSubscription(Action<TObject, THandler> subscribeAction, Action<TObject, THandler> unsubscribeAction)
        {
            _subscribeAction = subscribeAction ?? throw new ArgumentNullException("subscribeAction");
            _unsubscribeAction = unsubscribeAction ?? throw new ArgumentNullException("unsubscribeAction");
            _table = new ConditionalWeakTable<TObject, THandler>();
        }

        #endregion

        #region Members

        public bool Subscribe(object target, THandler handler)
        {
            var targetRef = target as TObject;
            if (targetRef == null) return false;
            Unsubscribe(target);

            _subscribeAction(targetRef, handler);
            _table.Add(targetRef, handler);

            return true;
        }

        public bool Unsubscribe(object target)
        {
            if (target is TObject targetRef && _table.TryGetValue(targetRef, out var existingHandler))
            {
                _unsubscribeAction(targetRef, existingHandler);
                _table.Remove(targetRef);
                return true;
            }

            return false;
        }

        #endregion
    }
}