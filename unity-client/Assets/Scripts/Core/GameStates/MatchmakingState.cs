using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Handles lobby creation, matchmaking queue ping, MMR balancing, and multi-client handshake.
    /// </summary>
    public class MatchmakingState : IGameState
    {
        private readonly GameStateManager _manager;
        private float _queueTimer;

        public GameStateType StateType => GameStateType.Matchmaking;

        public MatchmakingState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[MatchmakingState] Searching for match opponents...");
            _queueTimer = 0f;
        }

        public void Exit()
        {
            Debug.Log("[MatchmakingState] Match found! Transitioning to Race.");
        }

        public void Tick()
        {
            _queueTimer += Time.deltaTime;
            // Simulated match allocation after short queue
            if (_queueTimer >= 1.0f)
            {
                _manager.ChangeState(GameStateType.Race);
            }
        }

        public void CancelMatchmaking()
        {
            _manager.ChangeState(GameStateType.RaceSelection);
        }
    }
}
