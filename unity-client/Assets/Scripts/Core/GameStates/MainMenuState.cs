using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Manages top-level main menu UI, news updates, and primary navigation choices.
    /// </summary>
    public class MainMenuState : IGameState
    {
        private readonly GameStateManager _manager;

        public GameStateType StateType => GameStateType.MainMenu;

        public MainMenuState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[MainMenuState] Entered Main Menu. Ready for player selection.");
        }

        public void Exit()
        {
            Debug.Log("[MainMenuState] Leaving Main Menu.");
        }

        public void Tick()
        {
            // Handles UI animation updates or transition requests from UI buttons
        }

        public void SelectGarage()
        {
            _manager.ChangeState(GameStateType.Garage);
        }

        public void SelectRaceMode()
        {
            _manager.ChangeState(GameStateType.RaceSelection);
        }
    }
}
