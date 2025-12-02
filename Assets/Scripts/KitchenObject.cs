using UnityEngine;

/// <summary>
/// Represents a kitchen object (plate, food)
/// </summary>
public class KitchenObject : MonoBehaviour {

    /// <summary>
    /// Defines type of kitchen object
    /// </summary>
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    /// <summary>
    /// Parent that holds the kitchen object
    /// </summary>
    private IKitchenObjectParent kitchenObjectParent;

    /// <summary>
    /// Gets kitchen object SO
    /// </summary>
    /// <returns>Kitchen object SO</returns>
    public KitchenObjectSO GetKitchenObjectSO() {
        return kitchenObjectSO;
    }

    /// <summary>
    /// Gets kitchen object parent
    /// </summary>
    /// <returns>kitchen object parent</returns>
    public IKitchenObjectParent GetKitchenObjectParent() {
        return kitchenObjectParent;
    }

    /// <summary>
    /// Sets new parent for the kitchen object
    /// </summary>
    /// <param name="kitchenObjectParent">The parent to assign kitchen object to</param>
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent) {
        if (this.kitchenObjectParent != null) {
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject()) {
            Debug.LogError("IKitchenObjectParent already has a kitchen object");
        }
        kitchenObjectParent.SetKitchenObject(this);
        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Destroys the game object locally
    /// </summary>
    public void DestroySelf() {
        kitchenObjectParent.ClearKitchenObject();
        Destroy(gameObject);
    }

    /// <summary>
    /// Attempts to get kitchen object as a plate
    /// </summary>
    /// <param name="plateKitchenObject">Outputs plate if this object is a plate</param>
    /// <returns>True if this object is a plate, otherwise false</returns>
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject) {
        if (this is PlateKitchenObject) {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else {
            plateKitchenObject = null;
            return false;
        }
    }

    /// <summary>
    /// Spawns a kitchen object to the given parent
    /// </summary>
    /// <param name="kitchenObjectSO">The kitchen object SO to spawn</param>
    /// <param name="kitchenObjectParent">The parent to hold kitchen object</param>
    public static KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
        return kitchenObject;
    }

}
