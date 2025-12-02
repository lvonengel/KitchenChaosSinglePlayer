using UnityEngine;
using System;

/// <summary>
/// Counter that player can cut kitchen objects on.
/// </summary>
public class CuttingCounter : BaseCounter, IHasProgress {

    /// <summary>
    /// Fires when any cutting counter does a cut.
    /// Used mostly for sound effects.
    /// </summary>
    public static event EventHandler OnAnyCut;

    // Resets static event to prevent listener duplication
    new public static void ResetStaticData() {
        OnAnyCut = null;
    }

    /// <summary>
    /// Array of all valid kitchen objects that can be placed on the counter.
    /// Takes in the input of SO, and spawns the output.
    /// </summary>
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;


    /// <summary>
    /// Fires when the cutting progress on counter updates.
    /// </summary>
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    /// <summary>
    /// Fires when counter performs an actual cut.
    /// Used specifically for cutting animation.
    /// </summary>
    public event EventHandler OnCut;

    // tracks how many cuts were done on the current object
    private int cuttingProgress;

    /// <summary>
    /// Handles placing kitchen object on the counter and once cuts have reached
    /// max, spawns the cut version of that from cuttingRecipeSO
    /// </summary>
    /// <param name="player">The player interacting with counter</param>
    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            // Does not have kitchen object on it
            if (player.HasKitchenObject()) {
                // Player is holding something
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    cuttingProgress = 0;
                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
                    });
                }
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
                    }
                }
            }
            else {
                // Player is not holding anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    // Cuts a kitchen object if its here
    public override void InteractAlternate(Player player) {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
            // There is a kitchen object here and it can be cut
            cuttingProgress++;
            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);

            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });
            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
                GetKitchenObject().DestroySelf();

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }

    /// <summary>
    /// // Checks if the given kitchen object has a CuttingRecipeSO (if it can be cut)
    /// </summary>
    /// <param name="inputKitchenObjectSO">The kitchen object attempted to place on the counter</param>
    /// <returns>True if it can be cut, else false</returns>
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    #region getters/setters

    /// <summary>
    /// Goes through list of Cutting Recipes to find the one that matches
    /// </summary>
    /// <param name="inputKitchenObjectSO">The kitchen object placed on the counter</param>
    /// <returns>The output of the kitchen object</returns>
    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO != null) {
            return cuttingRecipeSO.output;
        }
        return null;
    }

    /// <summary>
    /// Searches through all cutting recipes and returns the one whose input matches
    /// the given kitchen object. Returns null if no recipe is found.
    /// </summary>
    /// <param name="inputKitchenObjectSO"The kitchen object to get the cutting recipe</param>
    /// <returns>The cutting recipe SO</returns>
    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray) {
            if (cuttingRecipeSO.input == inputKitchenObjectSO) {
                return cuttingRecipeSO;
            }
        }
        return null;
    }

    #endregion
}
