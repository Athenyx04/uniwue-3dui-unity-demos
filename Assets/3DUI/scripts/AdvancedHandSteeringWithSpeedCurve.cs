using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AdvancedHandSteeringWithSpeedCurve: MonoBehaviour
{
    public float speedInMeterPerSecond = 1;
    public float angleInDegreePerSecond = 25;
    public float anglePerClick = 45;
    public AnimationCurve highSpeedModeAccelerationCurve;
    public float highSpeedModeMaximumSpeed = 30;
    public float highSpeedModeMinimumSpeed = 1;

    private InputDevice handDevice;
    private GameObject handController;
    private GameObject trackingSpaceRoot;
    private bool bModeSnapRotation;
    private bool isStickWasPressed;
    private bool isTriggerWasPressed;
    private bool bModeHighSpeed;
    private bool snapTurn;
    private float curveDeltaTime = 0;
    private float speed;


    // aka tracking space's position in virtual environment 
    // i.e a game object positon and orientation is added to the tracking data

    //--------------------------------------------------------

    void Start()
    {
        GetHandDevice();
        GetHandControllerGameObject();
        GetTrackingSpaceRoot();
        speed = speedInMeterPerSecond;
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
                    StartCoroutine(GenerateVibrationsHandler(2, 0.5f, 0.2f, 0.1f, 0.2f, 0.2f));
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
                    bModeHighSpeed = !bModeHighSpeed;
                    isTriggerWasPressed = false;
                    StartCoroutine(GenerateVibrationsHandler(4, 0.5f, 0.2f, 0.1f, 0.1f, 0.2f));
                    if (bModeHighSpeed)
                    {
                        Debug.Log("High Speed Is ON");
                        StartCoroutine(GenerateVibrationsHandler(4, 0.5f, 0.2f, 0.1f, 0.1f, 0.2f));
                    } 
                    else
                    {
                        Debug.Log("High Speed Is OFF (Low Speed)");
                        StartCoroutine(GenerateVibrationsHandler(4, 0.2f, 0.5f, 0.1f, 0.1f, 0.2f));
                        speed = speedInMeterPerSecond; // Return back to normal speed
                        curveDeltaTime = 0; // Reset the delta time curve
                    }
                }

            }


            // see https://docs.unity3d.com/Manual/xr_input.html
            Vector2 thumbstickAxisValue; //  where left (-1.0,0.0), right (1.0,0.0), up (0.0,1.0), down (0.0,-1.0)
           
            if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
            {
                if (thumbstickAxisValue.y != 0)
                {
                    if (bModeHighSpeed)
                    {
                        if (curveDeltaTime <= 5) // Add time to the horizontal axis until reaching 5 seconds
                            curveDeltaTime += Time.deltaTime;

                        // In high speed mode, real speed is the minimum speed, plus the difference between max and min times the acceleration coeficient marked by the curve evaluation
                        speed = highSpeedModeMinimumSpeed + (highSpeedModeMaximumSpeed - highSpeedModeMinimumSpeed) * highSpeedModeAccelerationCurve.Evaluate(curveDeltaTime);
                    }
                    // Translate front/back Moving
                    trackingSpaceRoot.transform.position +=
                        handController.transform.forward * (speed * Time.deltaTime * thumbstickAxisValue.y);
                }
                else if (bModeHighSpeed && curveDeltaTime != 0)
                    curveDeltaTime = 0;

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

    private IEnumerator GenerateVibrationsHandler(int bursts, float evenAmplitude, float oddAmplitude, float evenDuration, float oddDuration, float waitTime)
    {
        HapticCapabilities capabilities;
        if (handDevice.TryGetHapticCapabilities(out capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                for (int i = bursts; i != 0; i--)
                {
                    if (i % 2 == 0)
                        handDevice.SendHapticImpulse(0, evenAmplitude, evenDuration);
                    else
                        handDevice.SendHapticImpulse(0, oddAmplitude, oddDuration);
                    yield return new WaitForSeconds(waitTime);
                }
            }
            else
            {
                Debug.LogError("No Haptic Capabilities!");
            }
        }
    }
}
