using System;

namespace NitroRush.Core
{
    /// <summary>
    /// Represents the distinct lifecycle states of the game finite state machine.
    /// </summary>
    public enum GameStateType
    {
        Boot,
        Authentication,
        MainMenu,
        Garage,
        RaceSelection,
        Matchmaking,
        Race,
        Results
    }

    /// <summary>
    /// Contract for individual game state implementations managed by GameStateManager.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Gets the state identifier for this game state.
        /// </summary>
        GameStateType StateType { get; }

        /// <summary>
        /// Executed when the state machine enters this state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Executed when the state machine exits this state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Executed per-frame while this state is active.
        /// </summary>
        void Tick();
    }
}
