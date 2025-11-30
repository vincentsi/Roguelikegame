using System;
using System.Collections.Generic;

namespace ProjectRoguelike.Core
{
    /// <summary>
    /// Minimal service locator for wiring runtime systems during the MVP phase.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<TService>(TService instance) where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), $"Cannot register null service: {typeof(TService).Name}");
            }

            _services[typeof(TService)] = instance;
        }

        public TService Resolve<TService>() where TService : class
        {
            if (TryResolve(out TService service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service not registered: {typeof(TService).Name}");
        }

        public bool TryResolve<TService>(out TService service) where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var stored))
            {
                service = stored as TService;
                return service != null;
            }

            service = null;
            return false;
        }
    }
}

