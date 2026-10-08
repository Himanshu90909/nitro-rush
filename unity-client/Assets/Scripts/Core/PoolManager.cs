using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Aggregated summary statistics for an object pool.
    /// </summary>
    public struct PoolStats
    {
        public string PoolKey;
        public int InstantiationCount;
        public int DestroyCount;
        public int HitCount;
        public int ActiveCount;
        public int InactiveCount;
        public float HitRate;
    }

    /// <summary>
    /// Centralized pool registry managing named component object pools and aggregate statistics.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private readonly Dictionary<string, IObjectPool> _pools = new Dictionary<string, IObjectPool>();
        private readonly Dictionary<string, object> _typedPools = new Dictionary<string, object>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<PoolManager>();
            }
        }

        /// <summary>
        /// Creates and registers a new component object pool under a specified pool key string.
        /// </summary>
        public ObjectPool<T> CreatePool<T>(string poolKey, T prefab, int prewarmCount = 0, Transform parent = null) where T : Component
        {
            if (string.IsNullOrEmpty(poolKey))
            {
                throw new ArgumentException("Pool key cannot be null or empty.", nameof(poolKey));
            }

            if (_pools.ContainsKey(poolKey))
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{poolKey}' already exists. Returning existing instance.");
                return (ObjectPool<T>)_typedPools[poolKey];
            }

            var pool = new ObjectPool<T>(prefab, prewarmCount, parent != null ? parent : transform);
            _pools[poolKey] = pool;
            _typedPools[poolKey] = pool;
            return pool;
        }

        /// <summary>
        /// Retrieves an instance from the pool registered under poolKey.
        /// </summary>
        public T Get<T>(string poolKey) where T : Component
        {
            if (_typedPools.TryGetValue(poolKey, out object poolObj) && poolObj is ObjectPool<T> pool)
            {
                return pool.Get();
            }

            Debug.LogError($"[PoolManager] No pool registered for key '{poolKey}' of type {typeof(T).Name}");
            return null;
        }

        /// <summary>
        /// Releases an active component instance back to its pool.
        /// </summary>
        public void Release<T>(string poolKey, T instance) where T : Component
        {
            if (_typedPools.TryGetValue(poolKey, out object poolObj) && poolObj is ObjectPool<T> pool)
            {
                pool.Release(instance);
                return;
            }

            Debug.LogError($"[PoolManager] Cannot release object to unregistered pool key '{poolKey}'");
        }

        /// <summary>
        /// Gets metric stats for a single pool key.
        /// </summary>
        public PoolStats GetStats(string poolKey)
        {
            if (_pools.TryGetValue(poolKey, out IObjectPool pool))
            {
                return new PoolStats
                {
                    PoolKey = poolKey,
                    InstantiationCount = pool.InstantiationCount,
                    DestroyCount = pool.DestroyCount,
                    HitCount = pool.HitCount,
                    ActiveCount = pool.ActiveCount,
                    InactiveCount = pool.InactiveCount,
                    HitRate = pool.PoolHitRate
                };
            }

            return default;
        }

        /// <summary>
        /// Gets metric stats for all registered pools.
        /// </summary>
        public List<PoolStats> GetAllStats()
        {
            var list = new List<PoolStats>(_pools.Count);
            foreach (var kvp in _pools)
            {
                IObjectPool pool = kvp.Value;
                list.Add(new PoolStats
                {
                    PoolKey = kvp.Key,
                    InstantiationCount = pool.InstantiationCount,
                    DestroyCount = pool.DestroyCount,
                    HitCount = pool.HitCount,
                    ActiveCount = pool.ActiveCount,
                    InactiveCount = pool.InactiveCount,
                    HitRate = pool.PoolHitRate
                });
            }
            return list;
        }

        /// <summary>
        /// Clears all pools in the manager.
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
            _typedPools.Clear();
        }
    }
}
