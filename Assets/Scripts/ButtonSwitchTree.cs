namespace StageAR
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using GoogleARCore;



    /*
     *  Class is currently not in use. 
     *  Will implement future onClick() callbacks for UI elements
     */
    public class ButtonSwitchTree : MonoBehaviour
    {
        Button SwitchTree;

        private GameObject Controller;
        private MainController controllerInstance;


        // Use this for initialization
        void Awake()
        {
            SwitchTree = GetComponent<Button>();
            SwitchTree.onClick.AddListener(SwitchControllerPrefab);

            controllerInstance = Controller.GetComponent<MainController>();
        }


        void SwitchControllerPrefab()
        {
            controllerInstance.ModelPrefabToggle = !controllerInstance.ModelPrefabToggle;
        }
    }
}