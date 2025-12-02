using UnityEngine;
using System;

/// <summary>
/// Manages the game state, countdown, gameplay timer,
/// player ready state, and pause logic.
/// </summary>
public class KitchenGameManager : MonoBehaviour {
    
    public static KitchenGameManager Instance {get; private set;}

    /// <summary>
    /// Fired when the game state changes
    /// </summary>
    public event EventHandler OnStateChanged;

    /// <summary>
    /// Fired when the player pauses the game
    /// </summary>
    public event EventHandler OnGamePaused;

    /// <summary>
    /// Fired when the player unpauses the game
    /// </summary>
    public event EventHandler OnGameUnpaused;

    /// <summary>
    /// The states that the game can be in
    /// </summary>
    private enum State {
        WaitingToStart, //When players are readying up
        CoundownToStart,  //Countdown before game starts
        GamePlaying, // When game is running
        GameOver, // When game is over
    }

    /// <summary>
    /// Network state that is synchronized across all clients
    /// </summary>
    private State state;

    /// <summary>
    /// Timer for when the game is about to begin
    /// </summary>
    private float countdownToStartTimer = 3f;

    /// <summary>
    /// How long the game has currently been on for
    /// </summary>
    private float gamePlayingTimer;

    /// <summary>
    /// How long the game will play for
    /// </summary>
    private float gamePlayingTimerMax = 100f;

    /// <summary>
    /// Checks whether the game is paused
    /// </summary>
    private bool isGamePaused = false;

    private void Awake() {
        state = State.WaitingToStart;
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e) {
        if (state == State.WaitingToStart) {
            state = State.CoundownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private void GameInput_OnPauseAction(object sender, EventArgs e) {
        TogglePauseGame();
    }

    //handles the game states for during a game
    private void Update() {
        switch (state) {
            case State.WaitingToStart:

                break;
            case State.CoundownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f) {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f) {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
        }
    }

    /// <summary>
    /// Checks if the state of the game is playing
    /// </summary>
    /// <returns>True if state is playing, otherwise false</returns>
    public bool IsGamePlaying() {
        return state == State.GamePlaying;
    }

    /// <summary>
    /// Checks if the state of the game is counting down
    /// </summary>
    /// <returns>True if state is counting down, otherwise false</returns>
    public bool IsCountdownToStartActive() {
        return state == State.CoundownToStart;
    }

    /// <summary>
    /// Gets the countdown to start timer value
    /// Used mostly for UI
    /// </summary>
    /// <returns>The countdown start value</returns>
    public float GetCountdownToStartTimer() {
        return countdownToStartTimer;
    }

    /// <summary>
    /// Checks if the state of the game is over
    /// </summary>
    /// <returns>True if state is over, otherwise false</returns>
    public bool IsGameOver() {
        return state == State.GameOver;
    }

    /// <summary>
    /// Gets the normalized value of the game playing timer
    /// </summary>
    /// <returns>Normalized value of the game playing timer</returns>
    public float GetGamePlayingTimerNormalized() {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }

    /// <summary>
    /// Toggles the local pause state and updates global pause state
    /// </summary>
    public void TogglePauseGame() {
        isGamePaused = !isGamePaused;
        if (isGamePaused) {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

}
