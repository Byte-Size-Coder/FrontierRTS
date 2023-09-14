using UnityEngine;
using UnityEngine.UI;


namespace BSC.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIController : MonoBehaviour
    {
        [SerializeField] private bool isOverlay;

        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        // Checks if this UI is a overlay UI (Meaning it will sit ontop of other UI and not replace it)
        public bool IsOverlayUI()
        {
            return isOverlay;
        }

        // Enables the canvas componet and allows UI to be present on the screen
        // Can accept dynamic data to be use within the UI controller
        // Allows controller to handle when this event happens
        public void AddUI(object data = null)
        {
            canvas.enabled = true;
            OnUIAdded(data);
        }

        // Disables the canvas component and removes the UI from the screen
        // Allows controller to handle when this event happens
        public void RemoveUI()
        {
            OnUIRemoved();
            canvas.enabled = false;
        }

        // Overridable method that Keeps the UI present on screen
        // but disables the UI from interaction
        // This is for any overlay UI that appears ontop
        public virtual void DisableUI() { }

        // Overridable method that reneables anything that was disabled
        // This is for any overlay UI that appears ontop and then is removed
        public virtual void EnableUI() { }

        public abstract void OnUIAdded(object data = null);

        public abstract void OnUIRemoved();

    }
}

