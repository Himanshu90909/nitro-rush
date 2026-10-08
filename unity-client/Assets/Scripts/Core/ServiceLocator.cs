using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Global Dependency Injection / Service Locator container.
    /// Enables decoupled component interaction without rigid Inspector wiring.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service instance against type T.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already registered. Overwriting.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
            }
        }

        /// <summary>
        /// Unregisters a service of type T.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Resolves and returns the registered service of type T.
        /// Throws InvalidOperationException if the service is missing.
        /// </summary>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"[ServiceLocator] Requested service of type {type.Name} is not registered!");
        }

        /// <summary>
        /// Attempts to resolve the service of type T without throwing exceptions.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object foundService))
            {
                service = (T)foundService;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
