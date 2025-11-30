using System;
using System.Collections.Generic;

namespace ProjectRoguelike.Core
{
    /// <summary>
    /// Lightweight event aggregator for decoupled communication.
    /// </summary>
    public sealed class GameEventBus
    {
        private readonly Dictionary<Type, Delegate> _listeners = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var key = typeof(TEvent);
            if (_listeners.TryGetValue(key, out var existing))
            {
                _listeners[key] = Delegate.Combine(existing, handler);
            }
            else
            {
                _listeners[key] = handler;
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            var key = typeof(TEvent);
            if (!_listeners.TryGetValue(key, out var existing) || existing == null)
            {
                return;
            }

            var updated = Delegate.Remove(existing, handler);
            if (updated == null)
            {
                _listeners.Remove(key);
            }
            else
            {
                _listeners[key] = updated;
            }
        }

        public void Publish<TEvent>(TEvent payload) where TEvent : struct
        {
            var key = typeof(TEvent);
            if (_listeners.TryGetValue(key, out var existing) && existing is Action<TEvent> action)
            {
                action.Invoke(payload);
            }
        }
    }
}

