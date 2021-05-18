using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class FPSMenuController : MonoBehaviour
{
    [Tooltip("It should reference the Assets/3DUI/prefabs/FPSMenuLiteFPSCounter.prefab")]
    public GameObject menuPrefab;

    private  string defaultMenuPrefabPath = "Assets/3DUI/prefabs/FPSMenuLiteFPSCounter.prefab";

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
        OpenOrCloseMenu();
    }

    private void OpenOrCloseMenu()
    {
        if (leftHandDevice.isValid) // still connected?
        {
            // //XR input mappings: https://docs.unity3d.com/Manual/xr_input.html
            if (leftHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bButtonPressedNow))
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
                        Open(gameObject.transform); // actually doent matter where as Canvas is render in camera view, here just to let students modif the position later
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
        AttachCameraToMenuCanvasAndDisplayMenu();
    }

    private void AttachCameraToMenuCanvasAndDisplayMenu()
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
                canvas.worldCamera = userXRCamera;
            }
            menuInstanced.SetActive(true); // just to make sure
        }
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