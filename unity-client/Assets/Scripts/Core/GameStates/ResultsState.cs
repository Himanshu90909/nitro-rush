using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Displays post-race podium standings, currency rewards, experience gain, and telemetry summaries.
    /// </summary>
    public class ResultsState : IGameState
    {
        private readonly GameStateManager _manager;

        public GameStateType StateType => GameStateType.Results;

        public ResultsState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[ResultsState] Displaying post-race results and reward payouts.");
        }

        public void Exit()
        {
            Debug.Log("[ResultsState] Exiting results screen.");
        }

        public void Tick()
        {
        }

        public void ReturnToMainMenu()
        {
            _manager.ChangeState(GameStateType.MainMenu);
        }
    }
}
