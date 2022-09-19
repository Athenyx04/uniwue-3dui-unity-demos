using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class AdvancedRayPicking : MonoBehaviour
{
    public float translationIncrement = 0.1f;
    public float rotationIncrement = 1.0f;
    public float scaleIncrement = 0.1f;
    public float thumbstickDeadZone = 0.5f;  // a bit of a dead zone (make it less sensitive to axis movement)
    public string RayCollisionLayer = "Default";
    public bool PickedUpObjectPositionNotControlledByPhysics = true; //otherwise object position will be still computed by physics engine, even when attached to ray

    private InputDevice righHandDevice;
    private GameObject rightHandController;
    private GameObject trackingSpaceRoot;

    private RaycastHit lastRayCastHit;
    private bool bButtonWasPressed = false;
    private bool isStickWasPressed = false;
    private bool isSecondaryWasPressed = false;
    private bool gripButtonWasPressed = false;
    private GameObject objectPickedUP = null;
    private GameObject previousObjectCollidingWithRay = null;
    private GameObject lastObjectCollidingWithRay = null;
    private bool IsThereAnewObjectCollidingWithRay = false;
    private bool rotationMode = false;
    private Vector3 scaleChange;

  
    /// 
    ///  Events
    /// 

    void Start()
    {
        GetRightHandDevice();
        GetRighHandController();
        GetTrackingSpaceRoot();
        scaleChange = new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
    }

    void Update()
    {
        if (objectPickedUP == null)
        {
            GetTargetedObjectCollidingWithRayCasting();
            UpdateObjectCollidingWithRay();
            UpdateFlagNewObjectCollidingWithRay();
            OutlineObjectCollidingWithRay();
        }
        AttachOrDetachTargetedObject();
        
        if (rotationMode)
        {
            RotateTargetedObjectOnLocalUpAxis();
            RotateTargetedObjectOnLocalRightAxis();
        }  
        else if (!rotationMode)
        {
            MoveTargetedObjectAlongRay();
            ScaleTargetedObject();
        }

        ChangeGravityOfTargetedObject();
        RemoveTargetedObject();
    }


    /// 
    ///  Start Functions (to get VR Devices)
    /// 

    private void GetRightHandDevice()
    {
        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
            | InputDeviceCharacteristics.Right
            | InputDeviceCharacteristics.Controller;

        var rightHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, rightHandedControllers);

        foreach (var device in rightHandedControllers)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            righHandDevice = device;
        }
    }
    private void GetTrackingSpaceRoot()
    {
        var XRRig = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRRig>(); // i.e Roomscale tracking space 
        trackingSpaceRoot = XRRig.rig; // Gameobject representing the center of tracking space in virtual enviroment
    }

    private void GetRighHandController()
    {
        rightHandController = gameObject; // i.e. with this script component and an XR controller component
    }

    /// 
    ///  Update Functions 
    /// 

    private void GetTargetedObjectCollidingWithRayCasting()
    {
        // see raycast example from https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        if (Physics.Raycast(transform.position,
            transform.TransformDirection(Vector3.forward),
            out RaycastHit hit,
            Mathf.Infinity,
            1 << LayerMask.NameToLayer(RayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
           // Debug.Log("Ray collided with:  " + hit.collider.gameObject + " collision point: " + hit.point);
            Debug.DrawLine(hit.point, (hit.point + hit.normal * 2));
            lastRayCastHit = hit;
        }
    }

    private void UpdateObjectCollidingWithRay()
    {
        if (lastRayCastHit.collider != null)
        {
            GameObject currentObjectCollidingWithRay = lastRayCastHit.collider.gameObject;
            if (lastObjectCollidingWithRay != currentObjectCollidingWithRay)
            {
                previousObjectCollidingWithRay = lastObjectCollidingWithRay;
                lastObjectCollidingWithRay = currentObjectCollidingWithRay;
            }
        }
    }
    private void UpdateFlagNewObjectCollidingWithRay()
    {
        if (lastObjectCollidingWithRay != previousObjectCollidingWithRay)
        {
            IsThereAnewObjectCollidingWithRay = true;
        }
        else
        {
            IsThereAnewObjectCollidingWithRay = false;
        }
    }

    private void OutlineObjectCollidingWithRay()
    {
        if (IsThereAnewObjectCollidingWithRay)
        {
            //add outline to new one
            if (lastObjectCollidingWithRay != null)
            {
                var outliner = lastObjectCollidingWithRay.GetComponent<OutlineModified>();
                if (outliner == null) // if not, we will add a component to be able to outline it
                {
                    //Debug.Log("Outliner added t" + lastObjectCollidingWithRay.gameObject.ToString());
                    outliner = lastObjectCollidingWithRay.AddComponent<OutlineModified>();
                }

                if (outliner != null)
                {
                    outliner.enabled = true;
                    //Debug.Log("outline new object color"+ lastObjectCollidingWithRay);
                }
                // remove outline from previous one
                //add outline new one
                if (previousObjectCollidingWithRay != null)
                {
                    outliner = previousObjectCollidingWithRay.GetComponent<OutlineModified>();
                    if (outliner != null)
                    {
                        outliner.enabled = false;
                        //Debug.Log("outline new object color"+ previousObjectCollidingWithRay);
                    }
                }
            }

        }
    }

    private void AttachOrDetachTargetedObject()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool bButtonAPressedNow))
            {

                if (!bButtonWasPressed && bButtonAPressedNow && lastRayCastHit.collider != null)
                {
                    bButtonWasPressed = true;
                }
                if (!bButtonAPressedNow && bButtonWasPressed) // Button was released?
                {
                    if (objectPickedUP != null) // already pick up an object?
                    {
                        GenerateSoundHandler(-1);
                        StartCoroutine(GenerateVibrationsHandler(2, 0.5f, 0.2f, 1.0f, 0.4f, 0.3f));

                        if (PickedUpObjectPositionNotControlledByPhysics)
                        {
                            Rigidbody rb = objectPickedUP.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = false;
                            }
                        }
                        objectPickedUP.transform.parent = null;
                        objectPickedUP = null;
                        Debug.Log("Object released: " + objectPickedUP);
                    }
                    else
                    {
                        GenerateSoundHandler(1);
                        StartCoroutine(GenerateVibrationsHandler(2, 0.2f, 0.5f, 0.4f, 1.0f, 0.3f));

                        objectPickedUP = lastRayCastHit.collider.gameObject;
                        objectPickedUP.transform.parent = gameObject.transform; // see Transform.parent https://docs.unity3d.com/ScriptReference/Transform-parent.html?_ga=2.21222203.1039085328.1595859162-225834982.1593000816
                        if (PickedUpObjectPositionNotControlledByPhysics)
                        {
                            Rigidbody rb = objectPickedUP.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = true;
                            }
                        }
                        Debug.Log("Object Picked up:" + objectPickedUP);
                    }
                    bButtonWasPressed = false;
                }
            }

            // check rotation mode
            // see https://docs.unity3d.com/Manual/xr_input.html
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool isStickPressedNow))
            {
                if (isStickPressedNow)
                {
                    isStickWasPressed = true;
                }
                else if (isStickWasPressed) // release
                {
                    rotationMode = !rotationMode;
                    GenerateSoundHandler(2);
                    StartCoroutine(GenerateVibrationsHandler(2, 0.5f, 0.2f, 0.1f, 0.2f, 0.2f));
                    isStickWasPressed = false;
                    if (rotationMode) Debug.Log("Rotation Mode is ON");
                    else Debug.Log("Rotation Mode is OFF");
                }

            }
        }
    }

    private void MoveTargetedObjectAlongRay()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (objectPickedUP != null) // already picked up an object?
                {
                    if (thumbstickAxis.y > thumbstickDeadZone || thumbstickAxis.y < -thumbstickDeadZone)
                    {
                        objectPickedUP.transform.position += transform.TransformDirection(Vector3.forward) * translationIncrement * thumbstickAxis.y;
                        //Debug.Log("Move object along ray: " + objectPickedUP + " axis: " + thumbstickAxis);
                    }
                }
            }
        }
    }

    private void ScaleTargetedObject()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (objectPickedUP != null) // already picked up an object?
                {
                    if (thumbstickAxis.x > thumbstickDeadZone || thumbstickAxis.x < -thumbstickDeadZone)
                    {
                        objectPickedUP.transform.localScale += scaleChange * thumbstickAxis.x;
                        //Debug.Log("Move object along ray: " + objectPickedUP + " axis: " + thumbstickAxis);
                    }
                }
            }
        }
    }

    private void RemoveTargetedObject()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isSecondaryPressedNow))
            {
                if (objectPickedUP != null) // already picked up an object?
                {
                    if (isSecondaryPressedNow)
                    {
                        isSecondaryWasPressed = true;
                    }
                    else if (isSecondaryWasPressed) // release
                    {
                        isSecondaryWasPressed = false;
                        Destroy(objectPickedUP);
                        objectPickedUP = null;
                    }
                }
            }
        }
    }

    private void ChangeGravityOfTargetedObject()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButtonPressedNow))
            {
                if (objectPickedUP != null) // already picked up an object?
                {
                    if (gripButtonPressedNow)
                    {
                        gripButtonWasPressed = true;
                    }
                    else if (gripButtonWasPressed) // release
                    {
                        gripButtonWasPressed = false;
                        if (objectPickedUP.GetComponent<Rigidbody>() == null)
                            objectPickedUP.AddComponent<Rigidbody>();
                        objectPickedUP.GetComponent<Rigidbody>().useGravity = !objectPickedUP.GetComponent<Rigidbody>().useGravity;
                        StartCoroutine(GenerateVibrationsHandler(3, 0.2f, 0.5f, 0.1f, 0.2f, 0.2f));
                    }
                }
            }
        }
    }

    private void RotateTargetedObjectOnLocalUpAxis()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (objectPickedUP != null) // already pick up an object?
                {
                    if (thumbstickAxis.x > thumbstickDeadZone || thumbstickAxis.x < -thumbstickDeadZone)
                    {
                        objectPickedUP.transform.Rotate(Vector3.up, rotationIncrement * thumbstickAxis.x, Space.Self);
                    }
                    //Debug.Log("Rotate Object: " + objectPickedUP + "axis " + thumbstickAxis);
                }
            }
        }
    }

    private void RotateTargetedObjectOnLocalRightAxis()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (objectPickedUP != null) // already pick up an object?
                {
                    if (thumbstickAxis.y > thumbstickDeadZone || thumbstickAxis.y < -thumbstickDeadZone)
                    {
                        objectPickedUP.transform.Rotate(Vector3.right, rotationIncrement * thumbstickAxis.y, Space.Self);
                    }
                    //Debug.Log("Rotate Object: " + objectPickedUP + "axis " + thumbstickAxis);
                }
            }
        }
    }

    private void GenerateSoundHandler(int pitch)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            switch (pitch)
            {
                case -1:
                    audioSource.pitch = -1;
                    audioSource.timeSamples = audioSource.clip.samples - 1;
                    audioSource.Play();
                    break;
                case 2:
                    audioSource.pitch = 2;
                    audioSource.timeSamples = 0;
                    audioSource.Play();
                    break;
                default:
                    audioSource.pitch = 1;
                    audioSource.timeSamples = 0;
                    audioSource.Play();
                    break;
            }
        }
        else
        {
            Debug.LogError("No Audio Source Found!");
        }
    }

    private IEnumerator GenerateVibrationsHandler(int bursts, float evenAmplitude, float oddAmplitude, float evenDuration, float oddDuration, float waitTime)
    {
        HapticCapabilities capabilities;
        if (righHandDevice.TryGetHapticCapabilities(out capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                for (int i = bursts; i != 0; i--)
                {
                    if (i%2 == 0)
                        righHandDevice.SendHapticImpulse(0, evenAmplitude, evenDuration);
                    else
                        righHandDevice.SendHapticImpulse(0, oddAmplitude, oddDuration);
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
