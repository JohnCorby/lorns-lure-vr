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

    private static SteamVR_Input_ActionSet_game Actions => SteamVR_Actions.game;


    #region patches

    // everything goes through these. just patch the methods to give hardcoded outputs using the default binds
    // this code is terrible. i dont care! i want to work on other things!
    // TODO: eventually, open unity, change actions.json to have these rewired actions directly instead of mapping in the patches.
    //       thisll make this code a million times simpler because buttons and axes can just be those directly

    private const int UIHorizontal = 20;
    private const int UIVertical = 21;
    private const int UISubmit = 22;
    private const int UICancel = 23;

    [HarmonyPostfix, HarmonyPatch(typeof(Input), nameof(Input.GetButton))]
    private static void Input_GetButton(string buttonName, ref bool __result)
    {
        __result |= buttonName switch
        {
            "Jump" => Actions.Jump.state,
            "SlowDown" => Actions.SlowDown.state,
            "Interact" => Actions.Interact.state,
            // "Zoom" => Actions.Y.state,
            "Dash" => Actions.Dash.state,
            "TryAgain" => Actions.TryAgain.state,
            "Crouch" => Actions.Crouch.state,
            "Hints" => Actions.Hints.state,
            // "PictureMode" => Actions.DUp.state,
            // "VideoMode" => Actions.DLeft.state,

            "Cling" => Actions.Cling.state,
            "Grapple" => Actions.Grapple.state,
            "Scan" => Actions.Scan.state,
            "Flare" => Actions.Flare.state,

            "DropCheckpoint" => Actions.DropCheckpoint.state,
            "UseBattery" => Actions.UseBattery.state,

            "Submit" => Actions.Submit.state,
            "Cancel" => Actions.Cancel.state,
            "UIScanClose" => Actions.UIScanClose.state,
            "ScrollTerminalUp" => Actions.ScrollTerminalUp.state,
            "ScrollTerminalDown" => Actions.ScrollTerminalDown.state,

            "UISubmit" => Actions.UISubmit.state,
            "UICancel" => Actions.UICancel.state,
            _ => false,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Input), nameof(Input.GetButtonDown))]
    private static void Input_GetButtonDown(string buttonName, ref bool __result)
    {
        __result |= buttonName switch
        {
            "Jump" => Actions.Jump.stateDown,
            "SlowDown" => Actions.SlowDown.stateDown,
            "Interact" => Actions.Interact.stateDown,
            // "Zoom" => Actions.Y.stateDown,
            "Dash" => Actions.Dash.stateDown,
            "TryAgain" => Actions.TryAgain.stateDown,
            "Crouch" => Actions.Crouch.stateDown,
            "Hints" => Actions.Hints.stateDown,
            // "PictureMode" => Actions.DUp.stateDown,
            // "VideoMode" => Actions.DLeft.stateDown,

            "Cling" => Actions.Cling.stateDown,
            "Grapple" => Actions.Grapple.stateDown,
            "Scan" => Actions.Scan.stateDown,
            "Flare" => Actions.Flare.stateDown,

            "DropCheckpoint" => Actions.DropCheckpoint.stateDown,
            "UseBattery" => Actions.UseBattery.stateDown,

            "Submit" => Actions.Submit.stateDown,
            "Cancel" => Actions.Cancel.stateDown,
            "UIScanClose" => Actions.UIScanClose.stateDown,
            "ScrollTerminalUp" => Actions.ScrollTerminalUp.stateDown,
            "ScrollTerminalDown" => Actions.ScrollTerminalDown.stateDown,

            "UISubmit" => Actions.UISubmit.stateDown,
            "UICancel" => Actions.UICancel.stateDown,
            _ => false,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Input), nameof(Input.GetButtonUp), typeof(string))]
    private static void Input_GetButtonUp(string buttonName, ref bool __result)
    {
        __result |= buttonName switch
        {
            "Jump" => Actions.Jump.stateUp,
            "SlowDown" => Actions.SlowDown.stateUp,
            "Interact" => Actions.Interact.stateUp,
            // "Zoom" => Actions.Y.stateUp,
            "Dash" => Actions.Dash.stateUp,
            "TryAgain" => Actions.TryAgain.stateUp,
            "Crouch" => Actions.Crouch.stateUp,
            "Hints" => Actions.Hints.stateUp,
            // "PictureMode" => Actions.DUp.stateUp,
            // "VideoMode" => Actions.DLeft.stateUp,

            "Cling" => Actions.Cling.stateUp,
            "Grapple" => Actions.Grapple.stateUp,
            "Scan" => Actions.Scan.stateUp,
            "Flare" => Actions.Flare.stateUp,

            "DropCheckpoint" => Actions.DropCheckpoint.stateUp,
            "UseBattery" => Actions.UseBattery.stateUp,

            "Submit" => Actions.Submit.stateUp,
            "Cancel" => Actions.Cancel.stateUp,
            "UIScanClose" => Actions.UIScanClose.stateUp,
            "ScrollTerminalUp" => Actions.ScrollTerminalUp.stateUp,
            "ScrollTerminalDown" => Actions.ScrollTerminalDown.stateUp,

            "UISubmit" => Actions.UISubmit.stateUp,
            "UICancel" => Actions.UICancel.stateUp,
            _ => false,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
    private static void Input_GetAxis(string axisName, ref float __result)
    {
        if (__result != 0) return;
        __result = axisName switch
        {
            "Mouse X" => Actions.MouseX_MouseY.axis.x,
            "Mouse Y" => Actions.MouseX_MouseY.axis.y,
            "Horizontal" => Actions.Horizontal_Vertical.axis.x,
            "Vertical" => Actions.Horizontal_Vertical.axis.y,

            "UIHorizontal" => Actions.UIHorizontal_UIVertical.axis.x,
            "UIVertical" => Actions.UIHorizontal_UIVertical.axis.y,
            _ => 0f,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
    private static void Input_GetAxisRaw(string axisName, ref float __result)
    {
        if (__result != 0) return;
        __result = axisName switch
        {
            "Mouse X" => Actions.MouseX_MouseY.axis.x,
            "Mouse Y" => Actions.MouseX_MouseY.axis.y,
            "Horizontal" => Actions.Horizontal_Vertical.axis.x,
            "Vertical" => Actions.Horizontal_Vertical.axis.y,

            "UIHorizontal" => Actions.UIHorizontal_UIVertical.axis.x,
            "UIVertical" => Actions.UIHorizontal_UIVertical.axis.y,
            _ => 0f,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetButton))]
    private static void RewiredStandaloneInputModule_GetButton(int actionId, ref bool __result)
    {
        __result |= actionId switch
        {
            UIHorizontal => Actions.UIHorizontal_UIVertical.axis.x > .5f,
            UIVertical => Actions.UIHorizontal_UIVertical.axis.y > .5f,
            _ => false,
        };
        __result |= actionId switch
        {
            UISubmit => Actions.UISubmit.state,
            UICancel => Actions.UICancel.state,
            _ => false,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetButtonDown))]
    private static void RewiredStandaloneInputModule_GetButtonDown(int actionId, ref bool __result)
    {
        __result |= actionId switch
        {
            UIHorizontal => Actions.UIHorizontal_UIVertical.axis.x > .5f && Actions.UIHorizontal_UIVertical.lastAxis.x <= .5f,
            UIVertical => Actions.UIHorizontal_UIVertical.axis.y > .5f && Actions.UIHorizontal_UIVertical.lastAxis.y <= .5f,
            _ => false,
        };
        __result |= actionId switch
        {
            UISubmit => Actions.UISubmit.stateDown,
            UICancel => Actions.UICancel.stateDown,
            _ => false,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetNegativeButton))]
    private static void RewiredStandaloneInputModule_GetNegativeButton(int actionId, ref bool __result)
    {
        __result |= actionId switch
        {
            UIHorizontal => Actions.UIHorizontal_UIVertical.axis.x < -.5f,
            UIVertical => Actions.UIHorizontal_UIVertical.axis.y < -.5f,
            _ => false,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetNegativeButtonDown))]
    private static void RewiredStandaloneInputModule_GetNegativeButtonDown(int actionId, ref bool __result)
    {
        __result |= actionId switch
        {
            UIHorizontal => Actions.UIHorizontal_UIVertical.axis.x < -.5f && Actions.UIHorizontal_UIVertical.lastAxis.x >= -.5f,
            UIVertical => Actions.UIHorizontal_UIVertical.axis.y < -.5f && Actions.UIHorizontal_UIVertical.lastAxis.y >= -.5f,
            _ => false,
        };
    }

    [HarmonyPostfix, HarmonyPatch(typeof(RewiredStandaloneInputModule), nameof(RewiredStandaloneInputModule.GetAxis))]
    private static void RewiredStandaloneInputModule_GetAxis(int actionId, ref float __result)
    {
        if (__result != 0) return;
        __result = actionId switch
        {
            UIHorizontal => Actions.UIHorizontal_UIVertical.axis.x,
            UIVertical => Actions.UIHorizontal_UIVertical.axis.y,
            _ => 0f,
        };
    }

    #endregion
}