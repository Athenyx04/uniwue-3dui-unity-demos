using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectSpawner : MonoBehaviour
{

    public GameObject objectPrefabToCreate;

    private InputDevice rightHandDevice;
    private bool bButtonWasPressed = false;
    


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
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

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
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool bButtonPressedNow))
            {
                if (!bButtonWasPressed && bButtonPressedNow)
                {
                    bButtonWasPressed = true;
                }
                if (!bButtonPressedNow && bButtonWasPressed) // Button was released?
                {
                    bButtonWasPressed = false;
                    Debug.Log("Trigger Button  pressed on Right Controller");
                    GameObject blockInstance = Instantiate(objectPrefabToCreate, this.transform.position, this.transform.rotation);
                }
            }
        }
    }

}