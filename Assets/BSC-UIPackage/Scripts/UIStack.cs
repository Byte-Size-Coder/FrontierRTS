using System.Collections.Generic;
using UnityEngine;

namespace BSC.UI
{
    /// <summary>
    /// Class that handles the hierarchy of all the UI in the scene
    /// This is in stack formation, meaning UI is "Stack" on top of eachother
    /// The UI at the top is enabled, the rest are either removed (cant see) or disable (cant interact)
    /// Traditional Push and Pop methods available to change the UI stack and what is presented
    /// </summary>
    public class UIStack : MonoBehaviour
    {
        public static UIStack Instance { get; private set; }

        [SerializeField] private string startingUI;

        private Stack<UIController> stack;

        public UIStack()
        {
            stack = new Stack<UIController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                stack = new Stack<UIController>();
                Instance = this;
            }
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(startingUI)) return;
            Push(startingUI);
        }

        // Finds UI from the pool by the string key name
        // Then pushes onto the UI stack
        public void Push(string controllerName)
        {
            UIController fetchedController = UIPool.Instance.GetUI(controllerName);

            Push(fetchedController);
        }


        // Pushes a UI controller onto the stack
        public void Push(UIController controller)
        {

            // Make sure to hold reference of the previous UI controller
            UIController previousUIController = null;

            if (stack.Count > 0)
            {
                previousUIController = stack.Peek();
            }

            // If the controller coming onto stack is a overlay UI,
            // only disable the previous UI controller (still visbile)
            // Otherwise, Remove it (hidden)
            if (controller.IsOverlayUI())
            {
                stack.Push(controller);
                controller.AddUI();

                if (previousUIController != null)
                {
                    previousUIController.DisableUI();
                }
            }
            else
            {
                if (previousUIController != null)
                {
                    previousUIController.RemoveUI();
                }

                stack.Push(controller);
                controller.AddUI();
            }
        }

        // Removes the top UI controller from stack 
        // Useful to go "back" to previous UI
        public void Pop()
        {
            if (stack.Count > 0)
            {
                UIController uIController = stack.Pop();

                uIController.RemoveUI();

                // If it is overlay UI that is being removed, then we can reenable 
                if (stack.Count > 0)
                {
                    UIController newCurrentUIController = stack.Peek();

                    if (newCurrentUIController != null)
                    {
                        if (uIController.IsOverlayUI())
                        {
                            newCurrentUIController.EnableUI();
                        }
                        else
                        {
                            newCurrentUIController.AddUI();
                        }
                    }
                }
            }
        }
    }
}
