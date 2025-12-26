using UnityEngine;

namespace Foundation.Singletons
{
    /// <summary>
    /// Type-per-singleton base class for <see cref="MonoBehaviour"/>.
    /// Provides <see cref="Instance"/> (auto-create) and <see cref="TryGetInstance(out T)"/> (no-create),
    /// persists across scene loads via <see cref="Object.DontDestroyOnLoad(Object)"/>,
    /// and remains correct when Domain Reload is disabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Override <see cref="OnSingletonAwake"/> / <see cref="OnSingletonDestroy"/> for custom logic.
    /// Do not declare Awake, OnEnable, or OnDestroy in derived classes.
    /// </para>
    /// <para>
    /// Lookup excludes inactive objects and assets.
    /// Re-initializes once per Play session even if the instance survives (Scene Reload disabled).
    /// </para>
    /// </remarks>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private const int UninitializedPlaySessionId = -1;
        private const FindObjectsInactive FindInactivePolicy = FindObjectsInactive.Exclude;

        // ReSharper disable once StaticMemberInGenericType
        private static T _instance;

        // ReSharper disable once StaticMemberInGenericType
        private static int _cachedPlaySessionId = UninitializedPlaySessionId;

        // Instance-level per-Play initialization marker (soft reset).
        private int _initializedPlaySessionId = UninitializedPlaySessionId;

        /// <summary>
        /// Mandatory access point: returns the singleton instance.
        /// If no instance exists in the loaded scene, it auto-creates one.
        /// Returns null while the application is quitting.
        /// </summary>
        public static T Instance
        {
            get
            {
                EnsurePlaySession();

                if (SingletonRuntime.IsQuitting) return null;
                if (_instance != null) return _instance;

                _instance = FindAnyObjectByType<T>(findObjectsInactive: FindInactivePolicy);
                if (_instance != null) return _instance;

                var go = new GameObject(name: typeof(T).Name);
                _instance = go.AddComponent<T>();
                return _instance;
            }
        }

        /// <summary>
        /// Optional access point: does not create an instance.
        /// If an instance exists in the loaded scene, it caches it into <paramref name="instance"/> and returns true.
        /// Returns false while the application is quitting, or when no instance exists.
        /// </summary>
        public static bool TryGetInstance(out T instance)
        {
            EnsurePlaySession();

            if (SingletonRuntime.IsQuitting)
            {
                instance = null;
                return false;
            }

            if (_instance != null)
            {
                instance = _instance;
                return true;
            }

            _instance = FindAnyObjectByType<T>(findObjectsInactive: FindInactivePolicy);
            instance = _instance;
            return instance != null;
        }

        // Unity message methods: keep them non-public; Unity invokes them by name.
        private void Awake()
        {
            if (!Application.isPlaying) return;

            this.TryInitializeForCurrentPlaySession();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            this.TryInitializeForCurrentPlaySession();
        }

        /// <summary>
        /// Establishes the singleton instance and performs per-Play-session initialization (soft reset).
        /// </summary>
        private void TryInitializeForCurrentPlaySession()
        {
            // Do not (re)bind or persist during shutdown / Play Mode exit.
            if (SingletonRuntime.IsQuitting)
            {
                Destroy(obj: this.gameObject);
                return;
            }

            // New Play session must take precedence (Domain Reload disabled-safe).
            EnsurePlaySession();

            // CRTP-like misuse guard (runtime).
            var typedThis = this as T;
            if (typedThis == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(
                    message:
                    $"[{this.GetType().Name}] must inherit SingletonBehaviour<{this.GetType().Name}>, not SingletonBehaviour<{typeof(T).Name}>.",
                    context: this
                );
#endif
                Destroy(obj: this.gameObject);
                return;
            }

            // Duplicate rejection (after possible invalidation).
            if (_instance != null && !ReferenceEquals(objA: _instance, objB: this))
            {
                Destroy(obj: this.gameObject);
                return;
            }

            _instance = typedThis;

            // Soft reset: run once per Play session, even for the same surviving object.
            if (this._initializedPlaySessionId == SingletonRuntime.PlaySessionId) return;

            // DontDestroyOnLoad only works for root GameObjects (or components on root GameObjects).
            if (this.transform.parent != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    message: $"[{typeof(T).Name}] Reparented to root for DontDestroyOnLoad.",
                    context: this
                );
#endif
                this.transform.SetParent(parent: null, worldPositionStays: true);
            }

            DontDestroyOnLoad(target: this.gameObject);

            // Mark first to prevent double-run via Awake + OnEnable re-entrancy in the same Play session.
            this._initializedPlaySessionId = SingletonRuntime.PlaySessionId;
            this.OnSingletonAwake();
        }

        /// <summary>
        /// Customization hook called once per Play session after the singleton is established and made persistent.
        /// </summary>
        protected virtual void OnSingletonAwake()
        {
        }

        private void OnDestroy()
        {
            if (!ReferenceEquals(objA: _instance, objB: this)) return;

            _instance = null;
            this.OnSingletonDestroy();
        }

        /// <summary>
        /// Customization hook called only when the established singleton instance is being destroyed.
        /// </summary>
        protected virtual void OnSingletonDestroy()
        {
        }

        /// <summary>
        /// Syncs the cached Play session id with <see cref="SingletonRuntime.PlaySessionId"/>.
        /// If the Play session changed (e.g., Domain Reload disabled), invalidates the cached instance.
        /// </summary>
        private static void EnsurePlaySession()
        {
            var current = SingletonRuntime.PlaySessionId;
            if (_cachedPlaySessionId == current) return;

            _cachedPlaySessionId = current;
            _instance = null;
        }
    }
}
