using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Represents a plate that can hold ingredient kitchen object SO.
/// Handles validation and adding ingredients.
/// </summary>
public class PlateKitchenObject : KitchenObject{

    /// <summary>
    /// Fired whenever an ingredient is successfully added to a plate/
    /// </summary>
    public event EventHandler<onIngredientAddedEventArgs> onIngredientAdded;
    public class onIngredientAddedEventArgs : EventArgs {
        public KitchenObjectSO kitchenObjectSO;
    }

    /// <summary>
    /// List of all ingredients that this plate is allowed to have
    /// </summary>
    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    /// <summary>
    /// The current ingredients on the plate.
    /// </summary>
    private List<KitchenObjectSO> KitchenObjectSOList;

    private void Awake() {
        KitchenObjectSOList = new List<KitchenObjectSO>();
    }

    /// <summary>
    /// Attempts to add an ingredient to the plate.
    /// Validates with the allowed ingredients and does not allow duplicates.
    /// </summary>
    /// <param name="kitchenObjectSO">The ingredient to attempt to add</param>
    /// <returns>True if the ingredient was added, otherwise false</returns>
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO) {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO)) {
            // Not a valid ingredient
            return false;
        }
        if (KitchenObjectSOList.Contains(kitchenObjectSO)) {
            //Plate already has ingredient, not allowing duplicates
            return false;
        } else {
            KitchenObjectSOList.Add(kitchenObjectSO);
            onIngredientAdded?.Invoke(this, new onIngredientAddedEventArgs {
                kitchenObjectSO = kitchenObjectSO
            });
            return true;
        }
    }

    /// <summary>
    /// Gets the list of kitchen object SO that are currently on the plate
    /// </summary>
    /// <returns>List of all ingredients on a plate</returns>
    public List<KitchenObjectSO> GetKitchenObjectSOList() {
        return KitchenObjectSOList;
    }
}
