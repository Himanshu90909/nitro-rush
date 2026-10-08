using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroRush.Core
{
    /// <summary>
    /// Core Finite State Machine (FSM) manager responsible for modular game state transitions.
    /// Replaces legacy monolith GameManager pattern with decoupled, state-specific behavior classes.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        private readonly Dictionary<GameStateType, IGameState> _states = new Dictionary<GameStateType, IGameState>();
        
        /// <summary>
        /// Currently active state instance.
        /// </summary>
        public IGameState CurrentState { get; private set; }

        /// <summary>
        /// Current state type enum value.
        /// </summary>
        public GameStateType CurrentStateType => CurrentState?.StateType ?? GameStateType.Boot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeStates();
        }

        private void Start()
        {
            ChangeState(GameStateType.Boot);
        }

        private void Update()
        {
            CurrentState?.Tick();
        }

        private void InitializeStates()
        {
            _states[GameStateType.Boot] = new BootState(this);
            _states[GameStateType.Authentication] = new AuthenticationState(this);
            _states[GameStateType.MainMenu] = new MainMenuState(this);
            _states[GameStateType.Garage] = new GarageState(this);
            _states[GameStateType.RaceSelection] = new RaceSelectionState(this);
            _states[GameStateType.Matchmaking] = new MatchmakingState(this);
            _states[GameStateType.Race] = new RaceState(this);
            _states[GameStateType.Results] = new ResultsState(this);
        }

        /// <summary>
        /// Transition to a target state by its enum type.
        /// </summary>
        public void ChangeState(GameStateType targetStateType)
        {
            if (!_states.TryGetValue(targetStateType, out var nextState))
            {
                Debug.LogError($"[GameStateManager] State {targetStateType} is not registered!");
                return;
            }

            if (CurrentState == nextState)
                return;

            GameStateType prevState = CurrentStateType;
            
            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();

            GameEvents.GameStateChanged.Invoke(new GameStateChangedEventArgs(prevState, targetStateType));
            Debug.Log($"[GameStateManager] Transitioned state: {prevState} -> {targetStateType}");
        }

        /// <summary>
        /// Retrieves a registered state instance of concrete type T.
        /// </summary>
        public T GetState<T>() where T : class, IGameState
        {
            foreach (var state in _states.Values)
            {
                if (state is T concreteState)
                    return concreteState;
            }
            return null;
        }
    }
}
