using System;
using UnityEngine;

/// <summary>
/// The counter that the player can cook kitchen objects on
/// </summary>
public class StoveCounter : BaseCounter, IHasProgress {

    // reference to frying recipe from uncooked to cooked
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;

    // reference to frying recipe from cooked to burned
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    /// <summary>
    /// Fired whenever stove cooking progress changes
    /// </summary>    
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    
    /// <summary>
    /// Fired whenever stove state changes 
    /// </summary>
    public event EventHandler<onStateChangedEventArgs> OnStateChanged;
    public class onStateChangedEventArgs : EventArgs {
        public State state;
    }

    /// <summary>
    /// States that stove can be in
    /// </summary>
    public enum State {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    //current time that the kitchen object has been cooking
    private float fryingTimer;

    //current time that the kitchen object has been burning
    private float burningTimer;

    // the frying recipe used for the currently cooking object
    private FryingRecipeSO fryingRecipeSO;

    // the frying recipe used for the currently burned object
    private BurningRecipeSO burningRecipeSO;

    // current stove state
    private State state;

    private void Start() {
        state = State.Idle;
    }

    // logic for changing the state for the stove
    public void Update() {
        if (HasKitchenObject()) {
            switch (state) {
                case State.Idle:
                    break;
                case State.Frying:
                    //updates current frying timer
                    fryingTimer += Time.deltaTime;
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });
                    if (fryingTimer > fryingRecipeSO.fryingTimerMax) {
                        // Fried
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                        state = State.Fried;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                        OnStateChanged?.Invoke(this, new onStateChangedEventArgs {
                            state = state
                        });
                    }
                    break;
                case State.Fried:
                    //updates current burned timer
                    burningTimer += Time.deltaTime;
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                    });
                    if (burningTimer > burningRecipeSO.burningTimerMax) {
                        // Fried
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                        state = State.Burned;
                        OnStateChanged?.Invoke(this, new onStateChangedEventArgs {
                            state = state
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                    break;
                case State.Burned:
                    break;
            }
        }

    }

    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            // Does not have kitchen object on it
            if (player.HasKitchenObject()) {
                // Player is holding something that can be fried
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    state = State.Frying;
                    fryingTimer = 0f;
                    OnStateChanged?.Invoke(this, new onStateChangedEventArgs {
                        state = state
                    });
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });
                }
            } else {
                // Player not carring anything
            }
        }
        else {
            // There is a kitchen object here
            if (player.HasKitchenObject()) {
                // Player is holding something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    //player is holding a plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        GetKitchenObject().DestroySelf();
                        state = State.Idle;
                        OnStateChanged?.Invoke(this, new onStateChangedEventArgs {
                            state = state
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                }
            }
            else {
                // Player is not holding anything
                GetKitchenObject().SetKitchenObjectParent(player);
                state = State.Idle;
                OnStateChanged?.Invoke(this, new onStateChangedEventArgs {
                    state = state
                });
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                    progressNormalized = 0f
                });
            }
        }
    }

    
    /// <summary>
    /// Checks if the given kitchen object has a CuttingRecipeSO (if it can be cut)
    /// </summary>
    /// <param name="inputKitchenObjectSO">The kitchen object SO to be checked</param>
    /// <returns>True if the given kitchen object has a frying recipe</returns>
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    /// <summary>
    /// Goes through list of Cutting Recipes to find the one that matches
    /// </summary>
    /// <param name="inputKitchenObjectSO">The kitchen object SO to be checked</param>
    /// <returns>Kitchen object out with the given input</returns>
    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null) {
            return fryingRecipeSO.output;
        }
        return null;
    }

    /// <summary>
    /// Goes through all frying recipes to find the one whose input matches the given kitchen object
    /// </summary>
    /// <param name="inputKitchenObjectSO">The kitchen object SO to be checked</param>
    /// <returns>Frying recipe for that given kitchen object</returns>
    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray) {
            if (fryingRecipeSO.input == inputKitchenObjectSO) {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    /// <summary>
    /// Goes through all burned recipes to find the one whose input matches the given
    /// </summary>
    /// <param name="inputKitchenObjectSO">The kitchen object SO to be checked</param>
    /// <returns>Burning recipe for that given kitchen object</returns>
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray) {
            if (burningRecipeSO.input == inputKitchenObjectSO) {
                return burningRecipeSO;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if the stove state is fried
    /// </summary>
    /// <returns>True if the stove state is fried</returns>
    public bool IsFried() {
        return state == State.Fried;
    }

}