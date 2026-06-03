using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fenrir.Core
{
    /// <summary>
    /// Lightweight service locator. Register services at Bootstrap, resolve anywhere.
    /// Never use FindObjectOfType or static singletons in production code.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
                Debug.LogWarning($"[ServiceLocator] Overwriting existing registration for {type.Name}");
            _services[type] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] No service registered for {typeof(T).Name}. " +
                "Ensure Bootstrap has completed before accessing services.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class =>
            _services.Remove(typeof(T));

        /// <summary>Call in tests or on full app reset.</summary>
        public static void Clear() => _services.Clear();
    }
}
