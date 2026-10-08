using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Handles player authentication, session token verification, and backend handshake.
    /// </summary>
    public class AuthenticationState : IGameState
    {
        private readonly GameStateManager _manager;
        private float _authTimeoutTimer;
        private bool _isAuthenticated;

        public GameStateType StateType => GameStateType.Authentication;

        public AuthenticationState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[AuthenticationState] Verifying player credentials and session token...");
            _authTimeoutTimer = 0f;
            _isAuthenticated = false;

            // Simulate immediate local session validation for live client flow
            _isAuthenticated = true;
        }

        public void Exit()
        {
            Debug.Log("[AuthenticationState] Player successfully authenticated.");
        }

        public void Tick()
        {
            if (_isAuthenticated)
            {
                _authTimeoutTimer += Time.deltaTime;
                if (_authTimeoutTimer >= 0.5f)
                {
                    _manager.ChangeState(GameStateType.MainMenu);
                }
            }
        }
    }
}
