using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectFactoryMenuController : MonoBehaviour
{

    [Tooltip("It should reference the prefab: Assets/3DUI/prefabs/ObjectFactoryMenu.prefab")]
    public GameObject menuPrefab;


    private string defaultMenuPrefabPath = "Assets/3DUI/prefabs/ObjectFactoryMenu.prefab";


    private GameObject menuInstanced;
    private InputDevice leftHandDevice;
    private bool bButtonWasPressed = false;
    private Camera userXRCamera;
    private Canvas canvas;


    ///  
    ///  Events
    /// 

    void Start()
    {
        GetLeftHandDevice();
        GetXRRigMainCamera();
    }

    /// 
    ///  Start Functions (to get VR Devices)
    /// 

    private void GetLeftHandDevice()
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
            | InputDeviceCharacteristics.Left
            | InputDeviceCharacteristics.Controller;

        var controller = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controller);

        foreach (var device in controller)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            leftHandDevice = device;
        }
    }

    private void GetXRRigMainCamera()
    {
        var XRRig = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRRig>(); // i.e Roomscale tracking space 
        userXRCamera = XRRig.GetComponentInChildren<Camera>();
        if (userXRCamera == null)
        {
            Debug.LogError("MainCamera in XR Rig not found! (XR Rig should be parent of this game object:)" + gameObject + " =>> cannot open help menu");
        }
    }


    /// 
    ///   Update Functions 
    /// 

    void Update()
    {
        OpenOrCloseHelpMenu();
    }

    private void OpenOrCloseHelpMenu()
    {
        if (leftHandDevice.isValid) // still connected?
        {
            if (leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool bButtonPressedNow))
            {
                if (!bButtonWasPressed && bButtonPressedNow)
                {
                    bButtonWasPressed = true;
                }
                if (!bButtonPressedNow && bButtonWasPressed) // Button was released?
                {
                    bButtonWasPressed = false;
                    if (menuInstanced == null && userXRCamera != null)
                    {
                        Open(gameObject.transform); // actually doent matter as we are going to place it in relative position to left hand controller
                    }
                    else
                    {
                        Close();
                    }
                }
            }
        }
    }


    public void Open(Transform Where)
    {
        CreateMenuFromPrebab(Where);
        AttachCameraToMenuCanvaAndPositioninFrontofLeftController();
    }


    private void CreateMenuFromPrebab(Transform Where)
    {
        if (menuPrefab != null)
        {
            menuInstanced = Instantiate(menuPrefab, Where.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("No Menu Prefab Specified - You should reference this one: " + defaultMenuPrefabPath);
        }
    }


    private void AttachCameraToMenuCanvaAndPositioninFrontofLeftController()
    {
        if (menuInstanced != null)
        {
            canvas = menuInstanced.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("canvas Not Found! in " + gameObject + " =>> cannot open help menu");
            }
            else if (userXRCamera != null)
            {
                canvas.worldCamera = userXRCamera; // required for corrects event system registion
                                                   // position 3 metters in front of user hand
                Vector3 position = gameObject.transform.position + (gameObject.transform.forward.normalized * 5f);
                menuInstanced.transform.position = position;
                // orientation facing user head
                menuInstanced.transform.LookAt(userXRCamera.transform.position);
                menuInstanced.SetActive(true); // just to make sure
            }
            menuInstanced.SetActive(true); // just to make sure
        }
    }
    

    public void Close()
    {
        if (menuInstanced != null)
        {
            menuInstanced.SetActive(false); // just to make sure
            Destroy(menuInstanced);
            menuInstanced = null;
        }
    }

}