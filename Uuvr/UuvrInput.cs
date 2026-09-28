using System.Runtime.InteropServices;
using HarmonyLib;
using Rewired.Integration.UnityUI;
using Valve.VR;

namespace Uuvr;

[HarmonyPatch]
public class UuvrInput : UuvrBehaviour
{
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

    private void Awake()
    {
        // SteamVR.Initialize();
    }

    private static bool _initialized = false;

    private void Start()
    {
        // uuvr gets destroyed and recreated for some reason. we only wanna init steamvr once tho
        if (_initialized) return;
        _initialized = true;

        // openvr initializes after Awake. need to init steamvr after that
        SteamVR_Actions.PreInitialize(); // in testing i dont need this but wtv everyone else does it
        SteamVR.Initialize();
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetButtonState")]
    private static extern void XInputSetButtonState(ushort wButton, bool bPressed);

    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetTriggerState")]
    private static extern void XInputSetTriggerState(bool bLeft, byte bValue);

    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetThumbState")]
    private static extern void XInputSetThumbState(bool bLeft, short sX, short sY);

    private static void SetButtonState(XboxButton button, bool pressed)
    {
        XInputSetButtonState((ushort)button, pressed);
    }

    /*
    private void Update()
    {
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
*/

    private static SteamVR_Input_ActionSet_Xbox Actions => SteamVR_Actions.Xbox;


    #region patches

    // everything goes through these. just patch the methods to give hardcoded outputs using the default binds
    // this code is terrible. i dont care! i want to work on other things!

    private const int UIHorizontal = 20;
    private const int UIVertical = 21;
    private const int UISubmit = 22;
    private const int UICancel = 23;

    [HarmonyPrefix, HarmonyPatch(typeof(Input), nameof(Input.GetButton))]
    private static bool Input_GetButton(string buttonName, out bool __result)
    {
        __result = false;
        __result |= buttonName switch
        {
            "Jump" => Actions.A.state,
            "SlowDown" => Actions.B.state,
            "Interact" => Actions.X.state,
            "Zoom" => Actions.Y.state,
            "Dash" => Actions.LB.state,
            "TryAgain" => Actions.Select.state,
            "Crouch" => Actions.DDown.state,
            "Hints" => Actions.DRight.state,
            "PictureMode" => Actions.DUp.state,
            "VideoMode" => Actions.DLeft.state,
            
            "Cling" => Actions.LT.axis > .5f,
            "Grapple" => Actions.RT.axis > .5f,
            "Scan" => Actions.LT.axis > .5f,
            "Flare" => Actions.RT.axis > .5f,
            
            "DropCheckpoint" => Actions.StickLeftClick.state,
            "UseBattery" => Actions.StickRightClick.state,
            
            "Submit" => Actions.A.state,
            "Cancel" => Actions.Start.state,
            "UIScanClose" => Actions.X.state,
            
            "UISubmit" => Actions.A.state,
            "UICancel" => Actions.B.state,
            _ => false,
        };

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Input), nameof(Input.GetButtonDown))]
    private static bool Input_GetButtonDown(string buttonName, out bool __result)
    {
        __result = false;
        __result |= buttonName switch
        {
            "Jump" => Actions.A.stateDown,
            "SlowDown" => Actions.B.stateDown,
            "Interact" => Actions.X.stateDown,
            "Zoom" => Actions.Y.stateDown,
            "Dash" => Actions.LB.stateDown,
            "TryAgain" => Actions.Select.stateDown,
            "Crouch" => Actions.DDown.stateDown,
            "Hints" => Actions.DRight.stateDown,
            "PictureMode" => Actions.DUp.stateDown,
            "VideoMode" => Actions.DLeft.stateDown,
 
            "Cling" => Actions.LT.axis > .5f && Actions.LT.lastAxis <= .5f,
            "Grapple" => Actions.RT.axis > .5f && Actions.RT.lastAxis <= .5f,
            "Scan" => Actions.LT.axis > .5f && Actions.LT.lastAxis <= .5f,
            "Flare" => Actions.RT.axis > .5f && Actions.RT.lastAxis <= .5f,
            
            "DropCheckpoint" => Actions.StickLeftClick.stateDown,
            "UseBattery" => Actions.StickRightClick.stateDown,

            "Submit" => Actions.A.stateDown,
            "Cancel" => Actions.Start.stateDown,
            "UIScanClose" => Actions.X.stateDown,
            
            "UISubmit" => Actions.A.stateDown,
            "UICancel" => Actions.B.stateDown,
            _ => false,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Input), nameof(Input.GetButtonUp), typeof(string))]
    private static bool Input_GetButtonUp(string buttonName, out bool __result)
    {
        __result = false;
        __result |= buttonName switch
        {
            "Jump" => Actions.A.stateUp,
            "SlowDown" => Actions.B.stateUp,
            "Interact" => Actions.X.stateUp,
            "Zoom" => Actions.Y.stateUp,
            "Dash" => Actions.LB.stateUp,
            "TryAgain" => Actions.Select.stateUp,
            "Crouch" => Actions.DDown.stateUp,
            "Hints" => Actions.DRight.stateUp,
            "PictureMode" => Actions.DUp.stateUp,
            "VideoMode" => Actions.DLeft.stateUp,
 
            "Cling" => Actions.LT.axis <= .5f && Actions.LT.lastAxis > .5f,
            "Grapple" => Actions.RT.axis <= .5f && Actions.RT.lastAxis > .5f,
            "Scan" => Actions.LT.axis <= .5f && Actions.LT.lastAxis > .5f,
            "Flare" => Actions.RT.axis <= .5f && Actions.RT.lastAxis > .5f,
            
            "DropCheckpoint" => Actions.StickLeftClick.stateUp,
            "UseBattery" => Actions.StickRightClick.stateUp,

            "Submit" => Actions.A.stateUp,
            "Cancel" => Actions.Start.stateUp,
            "UIScanClose" => Actions.X.stateUp,
            
            "UISubmit" => Actions.A.stateUp,
            "UICancel" => Actions.B.stateUp,
            _ => false,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
    private static bool Input_GetAxis(string axisName, out float __result)
    {
        __result = axisName switch
        {
            "Mouse X" => Actions.StickRight.axis.x,
            "Mouse Y" => Actions.StickRight.axis.y,
            "Horizontal" => Actions.StickLeft.axis.x,
            "Vertical" => Actions.StickLeft.axis.y,

            "UIHorizontal" => Actions.StickLeft.axis.x,
            "UIVertical" => Actions.StickLeft.axis.y,
            _ => 0f,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
    private static bool Input_GetAxisRaw(string axisName, out float __result)
    {
        __result = axisName switch
        {
            "Mouse X" => Actions.StickRight.axis.x,
            "Mouse Y" => Actions.StickRight.axis.y,
            "Horizontal" => Actions.StickLeft.axis.x,
            "Vertical" => Actions.StickLeft.axis.y,
            
            "UIHorizontal" => Actions.StickLeft.axis.x,
            "UIVertical" => Actions.StickLeft.axis.y,
            _ => 0f,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetButton))]
    private static bool RewiredStandaloneInputModule_GetButton(int actionId, out bool __result)
    {
        __result = false;
        __result |= actionId switch
        {
            UIHorizontal => Actions.StickLeft.axis.x > .5f,
            UIVertical => Actions.StickLeft.axis.y > .5f,
            _ => false,
        };
        __result |= actionId switch
        {
            UISubmit => Actions.A.stateDown,
            UICancel => Actions.B.stateDown,
            _ => false,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetButtonDown))]
    private static bool RewiredStandaloneInputModule_GetButtonDown(int actionId, out bool __result)
    {
        __result = false;
        __result |= actionId switch
        {
            UIHorizontal => Actions.StickLeft.axis.x > .5f && Actions.StickLeft.lastAxis.x <= .5f,
            UIVertical => Actions.StickLeft.axis.y > .5f && Actions.StickLeft.lastAxis.y <= .5f,
            _ => false,
        };
        __result |= actionId switch
        {
            UISubmit => Actions.A.stateDown,
            UICancel => Actions.B.stateDown,
            _ => false,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetNegativeButton))]
    private static bool RewiredStandaloneInputModule_GetNegativeButton(int actionId, out bool __result)
    {
        __result = actionId switch
        {
            UIHorizontal => Actions.StickLeft.axis.x < -.5f,
            UIVertical => Actions.StickLeft.axis.y < -.5f,
            _ => false,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetNegativeButtonDown))]
    private static bool RewiredStandaloneInputModule_GetNegativeButtonDown(int actionId, out bool __result)
    {
        __result = actionId switch
        {
            UIHorizontal => Actions.StickLeft.axis.x < -.5f && Actions.StickLeft.lastAxis.x >= -.5f,
            UIVertical => Actions.StickLeft.axis.y < -.5f && Actions.StickLeft.lastAxis.y >= -.5f,
            _ => false,
        };
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetAxis))]
    private static bool RewiredStandaloneInputModule_GetAxis(int actionId, out float __result)
    {
        __result = actionId switch
        {
            UIHorizontal => Actions.StickLeft.axis.x,
            UIVertical => Actions.StickLeft.axis.y,
            _ => 0f,
        };
        return false;
    }

    #endregion
}