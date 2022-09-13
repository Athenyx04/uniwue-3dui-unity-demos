using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayModeController : MonoBehaviour
{

    private InputDevice rightHandDevice;
    private bool bButtonWasPressed = false;
    private bool gripButtonWasPressed = false;


    ///  
    ///  Events
    /// 

    void Start()
    {
        GetRightHandDevice();
    }

    /// 
    ///  Start Functions (to get VR Devices)
    /// 

    private void GetRightHandDevice()
    {
        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
            | InputDeviceCharacteristics.Right
            | InputDeviceCharacteristics.Controller;

        var controller = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controller);

        foreach (var device in controller)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            rightHandDevice = device;
        }
    }


    /// 
    ///   Update Functions 
    /// 

    void Update()
    {
        if (rightHandDevice.isValid) // still connected?
        {
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bButtonPressedNow))
            {
                if (!bButtonWasPressed && bButtonPressedNow)
                {
                    bButtonWasPressed = true;
                }
                if (!bButtonPressedNow && bButtonWasPressed) // Button was released?
                {
                    bButtonWasPressed = false;
                    Debug.Log("Button B pressed on Right Controller");    
                }
            }

            if (rightHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButtonPressedNow))
            {
                if (!gripButtonWasPressed && gripButtonPressedNow)
                {
                    gripButtonWasPressed = true;
                }
                if (!gripButtonPressedNow && gripButtonWasPressed) // Button was released?
                {
                    gripButtonWasPressed = false;
                    Debug.Log("Grip Button pressed on Right Controller");
                }
            }
        }
    }
}