using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AdvancedHandSteering: MonoBehaviour
{
    public float speedInMeterPerSecond = 1;
    public float angleInDegreePerSecond = 25;
    public float anglePerClick = 45;
    public float speedMultiplierInHighSpeed = 15;
    public float angleMultiplierInHighSpeed = 5;

    private InputDevice handDevice;
    private GameObject handController;
    private GameObject trackingSpaceRoot;
    private bool bModeSnapRotation;
    private bool isStickWasPressed;
    private bool isTriggerWasPressed;
    private bool bModeTranslateSpeed;
    private bool snapTurn;


    // aka tracking space's position in virtual environment 
    // i.e a game object positon and orientation is added to the tracking data

    //--------------------------------------------------------

    void Start()
    {
        GetHandDevice();
        GetHandControllerGameObject();
        GetTrackingSpaceRoot();
    }

    void Update()
    {
        MoveTrackingSpaceRootWithHandSteering();
    }

    //--------------------------------------------------------

    private void GetHandDevice()
    {
      
       var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
            | InputDeviceCharacteristics.Left
            | InputDeviceCharacteristics.Controller;

        var controller = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controller);

        foreach (var device in controller)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            handDevice = device;
        }
    }


    private void GetTrackingSpaceRoot()
    {
        var XRRig = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRRig>(); // i.e Roomscale tracking space 
        trackingSpaceRoot = XRRig.rig; // Gameobject representing the center of tracking space in virtual enviroment
    }


    private void GetHandControllerGameObject()
    {
        handController = this.gameObject; // i.e. with this script component and an XR controller component
    }


    private void MoveTrackingSpaceRootWithHandSteering()  // simple - with no strafing 
    {
        if (handDevice.isValid) // still connected?
        {
            // check if smooth or snap rotation mode
            // see https://docs.unity3d.com/Manual/xr_input.html
            if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool isStickPressedNow))
            {
                if (isStickPressedNow)
                {
                    isStickWasPressed = true;
                }
                else if(isStickWasPressed) // release
                {
                    bModeSnapRotation = !bModeSnapRotation;
                    isStickWasPressed = false;
                    if(bModeSnapRotation) Debug.Log("Snap Turning Is ON");
                    else Debug.Log("Snap Turning Is OFF (Smooth Rotation)");
                }

            }

            // check if low or high speed mode
            // see https://docs.unity3d.com/Manual/xr_input.html
            if (handDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressedNow))
            {
                if (isTriggerPressedNow)
                {
                    isTriggerWasPressed = true;
                }
                else if (isTriggerWasPressed) // release
                {
                    bModeTranslateSpeed = !bModeTranslateSpeed;
                    isTriggerWasPressed = false;
                    if (bModeTranslateSpeed)
                    {
                        Debug.Log("High Speed Is ON");
                        speedInMeterPerSecond *= speedMultiplierInHighSpeed;
                        angleInDegreePerSecond *= angleMultiplierInHighSpeed;
                    }
                    else
                    {
                        Debug.Log("High Speed Is OFF (Low Speed)");
                        speedInMeterPerSecond /= speedMultiplierInHighSpeed;
                        angleInDegreePerSecond /= angleMultiplierInHighSpeed;
                    }
                }

            }


            // see https://docs.unity3d.com/Manual/xr_input.html
            Vector2 thumbstickAxisValue; //  where left (-1.0,0.0), right (1.0,0.0), up (0.0,1.0), down (0.0,-1.0)
           
            if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
            {
                // Translate front/back Moving
                trackingSpaceRoot.transform.position +=
                    handController.transform.forward * (speedInMeterPerSecond * Time.deltaTime * thumbstickAxisValue.y);
                //// Translate Left/right Moving
                // do something here (Exercise tasks)

                if (bModeSnapRotation)
                {
                    // Treating the thumbsticks as buttons for Snap Turning
                    if (thumbstickAxisValue.x >= 0.75f || thumbstickAxisValue.x <= -0.75f)
                        snapTurn = true;
                    else if (snapTurn)
                    {
                        // Differentiate between left or right turn
                        if (thumbstickAxisValue.x > 0)
                            trackingSpaceRoot.transform.Rotate(Vector3.up, anglePerClick);
                        else if (thumbstickAxisValue.x < 0)
                            trackingSpaceRoot.transform.Rotate(Vector3.up, -anglePerClick);
                        snapTurn = false;
                    }
                        
                }
                else
                {
                    //// Smooth Rotate Left/right Moving
                    ///
                    trackingSpaceRoot.transform.Rotate(Vector3.up, angleInDegreePerSecond * Time.deltaTime * thumbstickAxisValue.x);
                }

            }

        }
    }
}
