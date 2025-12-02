using System;
using UnityEngine;

/// <summary>
/// Represents the player in the game.
/// Handles movement, interaction, input listening, and holding kitchen objects.
/// </summary>
public class Player : MonoBehaviour, IKitchenObjectParent {

    public static Player Instance { get; private set; }

    /// <summary>
    /// Fired when player successfully picks up a kitchen object
    /// </summary>
    public event EventHandler OnPickedSomething;

    /// <summary>
    /// Fired whenever the currently selected counter changes
    /// </summary>
    public event EventHandler<onSelectedCounterChangedEventArgs> onSelectedCounterChanged;
    public class onSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }

    // the player movement speed
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;

    // the transform the player holds kitchen objects
    [SerializeField] private Transform kitchenObjectHoldPoint;

    // whether the player is walking
    private bool isWalking;

    //cache of the last interaction direction
    private Vector3 lastInteractDir;

    /// <summary>
    /// The counter the player has currently selected
    /// </summary>
    private BaseCounter selectedCounter;

    /// <summary>
    /// The kitchen object the player currently holds
    /// </summary>
    private KitchenObject kitchenObject;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one player instance");
        }
        Instance = this;
    }

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    // Fires when player interacts
    private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        
        if (selectedCounter != null) {
            selectedCounter.Interact(this);
        }
    }

    // Fires when player alternate interacts
    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e) {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        
        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);
        }
    }

    // Update is called once per frame
    private void Update() {
        HandleMovement();
        HandleInteractions();
    }

    /// <summary>
    /// Checks if player is actively walking
    /// </summary>
    /// <returns>True if player is walking, otherwise false</returns>
    public bool IsWalking() {
        return isWalking;
    }

    /// <summary>
    /// Does raycast based on movement direction to determine which counter to interact with
    /// </summary>
    private void HandleInteractions() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // Even if you're not actively moving forward into object, will still interact with it
        if (moveDir != Vector3.zero) {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)) {
                // Has ClearCounter
                if (baseCounter != selectedCounter) {
                    SetSelectedCounter(baseCounter);
                }
            }
            else {
                SetSelectedCounter(null);
            }
        }
        else {
            // Raycast does not hit anything
            SetSelectedCounter(null);
        }
    }

    /// <summary>
    /// Moves the player using CapsuleCase and updates whether the player is walking.
    /// </summary>
    private void HandleMovement() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove) {
            // Cannot move towards moveDir

            // Attempt x movement only
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove) {
                // Can move only on the x
                moveDir = moveDirX;
            }
            else {
                // Cannot move only in the x

                // Attempy z movement only
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove) {
                    // Can only move in the Z
                    moveDir = moveDirZ;
                }
                else {
                    //Cannot move in any direction
                }
            }
        }

        if (canMove) {
            transform.position += moveDir * moveDistance;
        }

        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 13f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    #region getters/setters

    /// <summary>
    /// Sets the currently selected counter
    /// </summary>
    /// <param name="selectedCounter">The selected counter</param>
    private void SetSelectedCounter(BaseCounter selectedCounter) {
        this.selectedCounter = selectedCounter;
        // assigns selected counter to the field onSelectedCounterChangedEventArgs
        onSelectedCounterChanged?.Invoke(this, new onSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }

    /// <summary>
    /// Gets the transform where held kitchen objects should follow
    /// </summary>
    /// <returns>Transform that kitchen objects are</returns>
    public Transform GetKitchenObjectFollowTransform() {
        return kitchenObjectHoldPoint;
    }

    /// <summary>
    /// Sets the kitchen object to the player
    /// </summary>
    /// <param name="kitchenObject">The kitchen object the player holds</param>
    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets the currently held kitchen object
    /// </summary>
    /// <returns>Currently held kitchen object</returns>
    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    /// <summary>
    /// Clears the held kitchen object from player
    /// </summary>
    public void ClearKitchenObject() {
        kitchenObject = null;
    }
    
    /// <summary>
    /// Checks whether the player is holding a kitchen object
    /// </summary>
    /// <returns>True if the player is holding a kitchen object, false otherwise</returns>
    public bool HasKitchenObject() {
        return kitchenObject != null;
    }

    #endregion
}
