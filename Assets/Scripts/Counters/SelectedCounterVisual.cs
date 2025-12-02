using UnityEngine;

/// <summary>
/// Visual to highlight the counter that the player can interact with it.
/// </summary>
public class SelectedCounterVisual : MonoBehaviour {

    /// <summary>
    /// Reference to the counter that will be highlighted
    /// </summary>
    [SerializeField] private BaseCounter baseCounter;

    //the gameobjects in the counter that need to be highlighted
    [SerializeField] private GameObject[] visualGameObjectArray;

    private void Start() {
        Player.Instance.onSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }

    /// <summary>
    /// Calls when players current selected counter changes
    /// </summary>
    private void Player_OnSelectedCounterChanged(object sender, Player.onSelectedCounterChangedEventArgs e) {
        if (e.selectedCounter == baseCounter) {
            Show();
        } else {
            Hide();
        }
    }

    private void Show() {
        foreach(GameObject visualGameObject in visualGameObjectArray) {
            visualGameObject.SetActive(true);
        }
    }
    
    private void Hide() {
        foreach(GameObject visualGameObject in visualGameObjectArray) {
            visualGameObject.SetActive(false);
        }
    }

}
