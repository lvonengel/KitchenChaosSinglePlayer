using UnityEngine;

/// <summary>
/// Class for all empty counters that holds a single kitchen object.
/// If the counter is empty and the player is holding something, the player can place
/// their object on the counter.
/// If the counter has an object and the player is empty-handed, the player picks it up.
/// </summary>
public class ClearCounter : BaseCounter {

    /// <summary>
    /// Interaction for placing and picking kitchen objects onto it.
    /// Can also combine existing kitchen object with plate.
    /// </summary>
    /// <param name="player">The player interacting with the counter</param>
    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            // Does not have kitchen object on it
            if (player.HasKitchenObject()) {
                // Player is holding something
                player.GetKitchenObject().SetKitchenObjectParent(this);
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
                } else {
                    //player is not holding a plate but something else
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject)) {
                        // counter is holding a plate
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO())) {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            } else {
                // Player is not holding anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
    
}
