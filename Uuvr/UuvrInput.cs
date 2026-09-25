using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace Uuvr;

public class UuvrInput: UuvrBehaviour
{
    public static InputDevice HeadDevice, LeftHandDevice, RightHandDevice;

    private enum XboxButton
    {
        DpadUp = 0x0001,
        DpadDown = 0x0002,
        DpadLeft = 0x0004,
        DpadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000,
    }

    public struct InputState
    {
        public bool X, Y, A, B;
        public Vector2 LeftAxis, RightAxis;
        public bool LeftClick, RightClick;
        public bool LeftTrigger, RightTrigger;
        public bool LeftGrip, RightGrip;
        public bool LeftMenu, RightMenu;

        public override string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(A)}: {A}, {nameof(B)}: {B}, {nameof(LeftAxis)}: {LeftAxis}, {nameof(RightAxis)}: {RightAxis}, {nameof(LeftClick)}: {LeftClick}, {nameof(RightClick)}: {RightClick}, {nameof(LeftTrigger)}: {LeftTrigger}, {nameof(RightTrigger)}: {RightTrigger}, {nameof(LeftGrip)}: {LeftGrip}, {nameof(RightGrip)}: {RightGrip}, {nameof(LeftMenu)}: {LeftMenu}, {nameof(RightMenu)}: {RightMenu}";
        }
    }

    public static InputState State;

    private void Awake()
    {
        // SteamVR.Initialize();
    }

    private void Start()
    {
        HeadDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        LeftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        RightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        Debug.Log($"head = {HeadDevice.name} with {HeadDevice.characteristics}");
        Debug.Log($"left hand = {LeftHandDevice.name} with {LeftHandDevice.characteristics}");
        Debug.Log($"right hand = {RightHandDevice.name} with {RightHandDevice.characteristics}");
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetButtonState")]
    private static extern void XInputSetButtonState(ushort wButton, bool bPressed);
    
    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetTriggerState")]
    private static extern void XInputSetTriggerState(bool bLeft, byte bValue);
    
    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetThumbState")]
    private static extern void XInputSetThumbState(bool bLeft, short sX, short sY);

    private static void SetButtonState(XboxButton button, bool pressed)
    {
        XInputSetButtonState((ushort) button, pressed);
    }

    private void Update()
    {
        // LeftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out State.X);
        // LeftHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out State.Y);
        // LeftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.LeftAxis);
        // LeftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out State.LeftClick);
        // LeftHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out State.LeftTrigger);
        // LeftHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out State.LeftGrip);
        // LeftHandDevice.TryGetFeatureValue(CommonUsages.menuButton, out State.LeftMenu);
        // RightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out State.A);
        // RightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out State.B);
        // RightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.RightAxis);
        // RightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out State.RightClick);
        // RightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out State.RightTrigger);
        // RightHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out State.RightGrip);
        // RightHandDevice.TryGetFeatureValue(CommonUsages.menuButton, out State.RightMenu);
        
        return;
        
        var actions = SteamVR_Actions.Xbox;
        // SetButtonState(XboxButton.DpadUp ,actions.DpadUp.state);
        // SetButtonState(XboxButton.DpadDown ,actions.DpadDown.state);
        // SetButtonState(XboxButton.DpadLeft ,actions.DpadLeft.state);
        // SetButtonState(XboxButton.DpadRight ,actions.DpadRight.state);
        SetButtonState(XboxButton.Start, actions.Start.state);
        SetButtonState(XboxButton.Back, actions.Select.state);
        SetButtonState(XboxButton.LeftThumb, actions.StickLeftClick.state);
        SetButtonState(XboxButton.RightThumb, actions.StickRightClick.state);
        SetButtonState(XboxButton.LeftShoulder, actions.LB.state);
        SetButtonState(XboxButton.RightShoulder, actions.RB.state);
        SetButtonState(XboxButton.A, actions.A.state);
        SetButtonState(XboxButton.B, actions.B.state);
        SetButtonState(XboxButton.X, actions.X.state);
        SetButtonState(XboxButton.Y, actions.Y.state);

        XInputSetTriggerState(true, (byte) (actions.LT.axis * 255));
        XInputSetTriggerState(false, (byte) (actions.RT.axis * 255));
        
        XInputSetThumbState(true, (short) (actions.StickLeft.axis.x * short.MaxValue), (short) (actions.StickLeft.axis.y * short.MaxValue));
        XInputSetThumbState(false, (short) (actions.StickRight.axis.x * short.MaxValue), (short) (actions.StickRight.axis.y * short.MaxValue));
    }

    private void OnGUI()
    {
        GUILayout.Label("LEFT");
        List<InputFeatureUsage> usages = new();
        usages.Clear();
        LeftHandDevice.TryGetFeatureUsages(usages);
        foreach (var usage in usages)
        {
            GUILayout.Label($"{usage.name} = {GetValue(LeftHandDevice, usage)}");
        }
        GUILayout.Label("RIGHT");
        usages.Clear();
        RightHandDevice.TryGetFeatureUsages(usages);
        foreach (var usage in usages)
        {
            GUILayout.Label($"{usage.name} = {GetValue(RightHandDevice, usage)}");
        }

        // GUILayout.Label(State.ToString());

        static object GetValue(InputDevice device, InputFeatureUsage usage)
        {
            var type = usage.type;
            if (type == typeof(bool))
            {
                device.TryGetFeatureValue(usage.As<bool>(), out var value);
                return value;
            }
            if (type == typeof(uint))
            {
                device.TryGetFeatureValue(usage.As<uint>(), out var value);
                return value;
            }
            if (type == typeof(float))
            {
                device.TryGetFeatureValue(usage.As<float>(), out var value);
                return value;
            }
            if (type == typeof(Vector2))
            {
                device.TryGetFeatureValue(usage.As<Vector2>(), out var value);
                return value;
            }
            if (type == typeof(Vector3))
            {
                device.TryGetFeatureValue(usage.As<Vector3>(), out var value);
                return value;
            }
            if (type == typeof(Quaternion))
            {
                device.TryGetFeatureValue(usage.As<Quaternion>(), out var value);
                return value;
            }
            if (type == typeof(UnityEngine.XR.Hand))
            {
                device.TryGetFeatureValue(usage.As<UnityEngine.XR.Hand>(), out var value);
                return value;
            }
            if (type == typeof(Bone))
            {
                device.TryGetFeatureValue(usage.As<Bone>(), out var value);
                return value;
            }
            if (type == typeof(Eyes))
            {
                device.TryGetFeatureValue(usage.As<Eyes>(), out var value);
                return value;
            }

            throw new NotImplementedException();
        }
    }
}
