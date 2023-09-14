using System;
using UnityEngine;

namespace BSC.UI
{
    // Data to hold information on the UI belonging to this library
    // Name is the key to be able to locate the UIcontroller when trying to access it.

    [Serializable]
    public struct UIInfo
    {
        public string name;
        public UIController uiPrefab;
    }

    /// <summary>
    /// A scriptable object that can hold information on a series of UI controllers to be able to use
    /// Can have multiple libraries used in various scenes, This allows for seperation if only certain UI is for certain scenes
    /// </summary>
    [CreateAssetMenu(fileName = "UI Library", menuName = "BSC-UI/UILibrary", order = 1)]
    public class UILibrary : ScriptableObject
    {
        public UIInfo[] library;
    }
}

