﻿using LibSM64;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static SM64BS.Utils.Types;

namespace SM64BS.Behaviours
{
    public class VRInputProvider : SM64InputProvider
    {
        internal Camera camera = null;
        internal List<InputDevice> leftControllers;
        internal List<InputDevice> rightControllers;

        internal bool leftControllerPresent = false;
        internal bool rightControllerPresent = false;
        internal bool eitherControllerPresent = false;

        public void Awake()
        {
            CheckForControllers();
            InputDevices.deviceConnected += DeviceChanged;
            InputDevices.deviceDisconnected += DeviceChanged;
        }

        private void DeviceChanged(InputDevice connectedDevice)
        {
            CheckForControllers();
        }

        private void CheckForControllers()
        {
            leftControllers = new List<InputDevice>();
            rightControllers = new List<InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, leftControllers);
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, rightControllers);

            leftControllerPresent = leftControllers.Count > 0;
            rightControllerPresent = rightControllers.Count > 0;
            eitherControllerPresent = leftControllerPresent || rightControllerPresent;
        }

        public override Vector3 GetCameraLookDirection()
        {
            if (camera == null)
                camera = Camera.main;
            if(camera != null)
                return camera.transform.forward;
            return Vector3.forward;
        }

        public override Vector2 GetJoystickAxes()
        {
            Vector2 axis = Vector2.zero;
            if (leftControllers.Count > 0)
                leftControllers[0].TryGetFeatureValue(CommonUsages.primary2DAxis, out axis);
            else
                axis = new Vector2(Input.GetAxis("HorizontalLeftHand"), -Input.GetAxis("VerticalLeftHand"));
            return axis;
        }

        public override bool GetButtonHeld(Button button)
        {
            bool held = false;
            if (leftControllers.Count > 0 && rightControllers.Count > 0)
            {
                switch (button)
                {
                    case Button.Jump:
                        rightControllers[0].TryGetFeatureValue(CommonUsages.primaryButton, out held);
                        break;
                    case Button.Kick:
                        rightControllers[0].TryGetFeatureValue(CommonUsages.secondaryButton, out held);
                        break;
                    case Button.Stomp:
                        float triggerValue = 0.0f;
                        leftControllers[0].TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
                        held = triggerValue >= 0.5f;
                        break;
                }
            }
            else
            {
                switch (button)
                {
                    case Button.Jump:
                        held = Input.GetButton("MenuButtonRightHand");
                        break;
                    case Button.Kick:
                        held = Input.GetButton("MenuButtonLeftHand");
                        break;
                    case Button.Stomp:
                        float triggerValue = Input.GetAxis("TriggerLeftHand");
                        held = triggerValue >= 0.5f;
                        break;
                }
            }
            return held;
        }


        // VR-specific functions

        public bool GetVRButtonHeld(InputFeatureUsage<bool> button, VRControllerType controllerType = VRControllerType.Left | VRControllerType.Right) 
        {
            bool buttonHeld = false;
            if ((uint)(controllerType & VRControllerType.Left) > 0 && leftControllerPresent)
                leftControllers[0].TryGetFeatureValue(button, out buttonHeld);
            if ((uint)(controllerType & VRControllerType.Right) > 0 && rightControllerPresent)
                rightControllers[0].TryGetFeatureValue(button, out buttonHeld);
            return buttonHeld;
        }

        public Vector3 GetVRHandPosition(VRControllerType controllerType)
        {
            Vector3 controllerPos = Vector3.zero;
            if ((uint)(controllerType & VRControllerType.Left) > 0 && leftControllerPresent)
                leftControllers[0].TryGetFeatureValue(CommonUsages.devicePosition, out controllerPos);
            else if ((uint)(controllerType & VRControllerType.Right) > 0 && rightControllerPresent)
                rightControllers[0].TryGetFeatureValue(CommonUsages.devicePosition, out controllerPos);
            return controllerPos;
        }

        public Quaternion GetVRHandRotation(VRControllerType controllerType)
        {
            Quaternion controllerRot = Quaternion.identity;
            if ((uint)(controllerType & VRControllerType.Left) > 0 && leftControllerPresent)
                leftControllers[0].TryGetFeatureValue(CommonUsages.deviceRotation, out controllerRot);
            else if ((uint)(controllerType & VRControllerType.Right) > 0 && rightControllerPresent)
                rightControllers[0].TryGetFeatureValue(CommonUsages.deviceRotation, out controllerRot);
            return controllerRot;
        }

        public Vector3 GetVRHandVelocity(VRControllerType controllerType)
        {
            Vector3 controllerVel = Vector3.zero;
            if ((uint)(controllerType & VRControllerType.Left) > 0 && leftControllerPresent)
                leftControllers[0].TryGetFeatureValue(CommonUsages.deviceVelocity, out controllerVel);
            else if ((uint)(controllerType & VRControllerType.Right) > 0 && rightControllerPresent)
                rightControllers[0].TryGetFeatureValue(CommonUsages.deviceVelocity, out controllerVel);
            return controllerVel;
        }

        public Vector3 GetVRHandAngularVelocity(VRControllerType controllerType)
        {
            Vector3 controllerAngVel = Vector3.zero;
            if ((uint)(controllerType & VRControllerType.Left) > 0 && leftControllerPresent)
                leftControllers[0].TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out controllerAngVel);
            else if ((uint)(controllerType & VRControllerType.Right) > 0 && rightControllerPresent)
                rightControllers[0].TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out controllerAngVel);
            return controllerAngVel;
        }
    }
}
