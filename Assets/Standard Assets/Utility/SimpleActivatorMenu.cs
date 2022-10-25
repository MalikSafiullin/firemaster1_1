using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 618
namespace UnityStandardAssets.Utility
{
    public class SimpleActivatorMenu : MonoBehaviour
    {
        // An incredibly simple menu which, when given references
        // to gameObjects in the scene
        [CanBeNull] public Text camSwitchButton;
        public GameObject[] objects;


        private int _mCurrentActiveObject;


        private void OnEnable()
        {
            // active object starts from first in array
            _mCurrentActiveObject = 0;
            camSwitchButton!.text = objects[_mCurrentActiveObject].name;
        }
    }
}