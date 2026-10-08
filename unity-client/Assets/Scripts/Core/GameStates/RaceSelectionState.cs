using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Manages track selection, game mode selection (Time Trial, Ranked, Casual), and difficulty settings.
    /// </summary>
    public class RaceSelectionState : IGameState
    {
        private readonly GameStateManager _manager;

        public GameStateType StateType => GameStateType.RaceSelection;

        public RaceSelectionState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[RaceSelectionState] Displaying available racing tracks and game modes.");
        }

        public void Exit()
        {
            Debug.Log("[RaceSelectionState] Track selected.");
        }

        public void Tick()
        {
        }

        public void StartMatchmaking()
        {
            _manager.ChangeState(GameStateType.Matchmaking);
        }

        public void ReturnToMainMenu()
        {
            _manager.ChangeState(GameStateType.MainMenu);
        }
    }
}
