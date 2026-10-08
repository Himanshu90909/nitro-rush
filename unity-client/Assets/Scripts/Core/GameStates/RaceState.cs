using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Active gameplay state orchestrating race countdown, active driving physics, and lap monitoring.
    /// </summary>
    public class RaceState : IGameState
    {
        private readonly GameStateManager _manager;

        public GameStateType StateType => GameStateType.Race;

        public RaceState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[RaceState] Entering live race. Initializing RaceManager and vehicle physics.");
        }

        public void Exit()
        {
            Debug.Log("[RaceState] Race completed or terminated.");
        }

        public void Tick()
        {
            // High-frequency race monitoring or state progression checks
        }

        public void FinishRace()
        {
            _manager.ChangeState(GameStateType.Results);
        }
    }
}
