using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Generic object pool interface for unified metrics aggregation.
    /// </summary>
    public interface IObjectPool
    {
        int InstantiationCount { get; }
        int DestroyCount { get; }
        int HitCount { get; }
        int ActiveCount { get; }
        int InactiveCount { get; }
        float PoolHitRate { get; }
        void Clear();
    }

    /// <summary>
    /// Main-thread high-performance Unity Component object pool eliminating GC allocations during gameplay spawning.
    /// </summary>
    /// <typeparam name="T">Target Component type.</typeparam>
    public class ObjectPool<T> : IObjectPool where T : Component
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly T _prefab;
        private readonly Transform _parent;

        public int InstantiationCount { get; private set; }
        public int DestroyCount { get; private set; }
        public int HitCount { get; private set; }
        public int ActiveCount => InstantiationCount - DestroyCount - _pool.Count;
        public int InactiveCount => _pool.Count;

        /// <summary>
        /// Exact pool hit rate efficiency ratio: hits / (hits + instantiations).
        /// </summary>
        public float PoolHitRate
        {
            get
            {
                int totalOps = HitCount + InstantiationCount;
                return totalOps > 0 ? (float)HitCount / totalOps : 0f;
            }
        }

        public ObjectPool(T prefab, int prewarmCount = 0, Transform parent = null)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            _parent = parent;

            if (prewarmCount > 0)
            {
                Prewarm(prewarmCount);
            }
        }

        /// <summary>
        /// Pre-instantiates a specified count of inactive objects to populate the pool up front.
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = CreateNewInstance();
                instance.gameObject.SetActive(false);
                _pool.Push(instance);
            }
        }

        /// <summary>
        /// Gets an active object from the pool, instantiating new instances if empty.
        /// </summary>
        public T Get()
        {
            T instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Pop();
                HitCount++;
            }
            else
            {
                instance = CreateNewInstance();
            }

            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Releases an active object back to the pool, deactivating its GameObject.
        /// </summary>
        public void Release(T instance)
        {
            if (instance == null) return;

            instance.gameObject.SetActive(false);
            if (!_pool.Contains(instance))
            {
                _pool.Push(instance);
            }
        }

        /// <summary>
        /// Destroys all pooled inactive game objects and resets pool counters.
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                T instance = _pool.Pop();
                if (instance != null && instance.gameObject != null)
                {
                    UnityEngine.Object.Destroy(instance.gameObject);
                    DestroyCount++;
                }
            }
            _pool.Clear();
        }

        private T CreateNewInstance()
        {
            T instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            InstantiationCount++;
            return instance;
        }
    }

    /// <summary>
    /// Generic memory pool for non-UnityEngine pure C# objects (e.g., event args, state buffers).
    /// </summary>
    public class Pool<T> where T : class, new()
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;

        public int InstantiationCount { get; private set; }
        public int DestroyCount { get; private set; }
        public int HitCount { get; private set; }
        public int ActiveCount => InstantiationCount - DestroyCount - _pool.Count;
        public int InactiveCount => _pool.Count;

        public float PoolHitRate
        {
            get
            {
                int totalOps = HitCount + InstantiationCount;
                return totalOps > 0 ? (float)HitCount / totalOps : 0f;
            }
        }

        public Pool(int prewarmCount = 0, Func<T> factory = null, Action<T> onGet = null, Action<T> onRelease = null)
        {
            _factory = factory ?? (() => new T());
            _onGet = onGet;
            _onRelease = onRelease;

            if (prewarmCount > 0)
            {
                Prewarm(prewarmCount);
            }
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = _factory();
                InstantiationCount++;
                _pool.Push(instance);
            }
        }

        public T Get()
        {
            T instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Pop();
                HitCount++;
            }
            else
            {
                instance = _factory();
                InstantiationCount++;
            }

            _onGet?.Invoke(instance);
            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null) return;

            _onRelease?.Invoke(instance);
            if (!_pool.Contains(instance))
            {
                _pool.Push(instance);
            }
        }

        public void Clear()
        {
            DestroyCount += _pool.Count;
            _pool.Clear();
        }
    }
}
