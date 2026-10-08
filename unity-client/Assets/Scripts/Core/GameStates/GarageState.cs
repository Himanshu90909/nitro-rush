using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Manages the vehicle customization, upgrade tree, paint customization, and stats preview.
    /// </summary>
    public class GarageState : IGameState
    {
        private readonly GameStateManager _manager;

        public GameStateType StateType => GameStateType.Garage;

        public GarageState(GameStateManager manager)
        {
            _manager = manager;
        }

        public void Enter()
        {
            Debug.Log("[GarageState] Garage loaded. Displaying selected vehicle stage.");
        }

        public void Exit()
        {
            Debug.Log("[GarageState] Exiting Garage, saving vehicle modifications.");
        }

        public void Tick()
        {
            // Per-frame camera rotation or preview vehicle interaction updates
        }

        public void ReturnToMainMenu()
        {
            _manager.ChangeState(GameStateType.MainMenu);
        }
    }
}
