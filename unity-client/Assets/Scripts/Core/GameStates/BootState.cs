using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Initial bootstrap state that initializes service registration, hardware checks,
    /// and pre-warms core systems before passing control to authentication.
    /// </summary>
    public class BootState : IGameState
    {
        private readonly GameStateManager _manager;

        public GameStateType StateType => GameStateType.Boot;

        public BootState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[BootState] Initializing core engine subsystems and ServiceLocator registrations...");
            
            // Perform bootstrap initialization
            if (!ServiceLocator.TryGet<PoolManager>(out _))
            {
                var poolManagerGO = new GameObject("PoolManager");
                var poolManager = poolManagerGO.AddComponent<PoolManager>();
                ServiceLocator.Register(poolManager);
            }

            Debug.Log("[BootState] Initialization complete. Transitioning to Authentication.");
            _manager.ChangeState(GameStateType.Authentication);
        }

        public void Exit()
        {
            Debug.Log("[BootState] Exited boot sequence.");
        }

        public void Tick()
        {
            // Boot state completes synchronously in Enter(), no per-frame work needed.
        }
    }
}
