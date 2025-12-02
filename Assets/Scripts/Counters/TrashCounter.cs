using System;
using UnityEngine;

/// <summary>
/// The counter that destroys whatever is in the players 
/// </summary>
public class TrashCounter : BaseCounter {

    /// <summary>
    /// Fired whenever any trash is interacted with
    /// </summary>    
    public static event EventHandler OnAnyObjectTrashed;

    // makes sure that if a player is delayed, that it won't crash game
    new public static void ResetStaticData() {
        OnAnyObjectTrashed = null;
    }

    public override void Interact(Player player) {
        if (player.HasKitchenObject()) {
            player.GetKitchenObject().DestroySelf();

            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
        }
    }
}