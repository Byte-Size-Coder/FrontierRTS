using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BSC.UI
{

    /// <summary>
    /// A object pool style for UI
    /// Use to only create/access UI when needed, and then be able to reuse.
    /// </summary>
    public class UIPool : MonoBehaviour
    {
        public static UIPool Instance { get; private set; }


        [SerializeField] private UILibrary[] uiLibraries;

        private Dictionary<string, UIController> library;
        private Dictionary<string, UIController> pool;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            library = new Dictionary<string, UIController>();
            pool = new Dictionary<string, UIController>();

            foreach (var libraryReference in uiLibraries)
            {
                foreach (var ui in libraryReference.library)
                {
                    library.Add(ui.name, ui.uiPrefab);
                }
            }
        }

        // Get UI from a string key name;
        public UIController GetUI(string controllerName)
        {
            // If controller already exist in scene, return that one.
            if (pool.ContainsKey(controllerName))
            {
                return pool[controllerName];
            }

            // If it does not exists, look for refrence and create it. 
            if (library.ContainsKey(controllerName))
            {
                UIController foundController = library[controllerName];

                if (foundController != null)
                {

                    UIController newControllerObject = Instantiate(foundController, this.transform);

                    pool.Add(controllerName, newControllerObject);

                    return newControllerObject;
                }
            }

            Debug.LogError("Could not find UI in library, please check/update your scriptable object");

            throw new System.Exception("Could not find UI in library, please check/update your scriptable object");
        }
    }
}

