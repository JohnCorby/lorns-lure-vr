using System.ComponentModel;
using BepInEx.Configuration;
using UnityEngine;

namespace Uuvr;

public class ModConfiguration
{
    public static ModConfiguration Instance;
    
    public enum CameraTrackingMode
    {
        [Description("Absolute")]
        Absolute,
        [Description("Relative matrix")]
        RelativeMatrix,
#if MODERN
        // TODO: could add this for legacy too.
        [Description("Relative Transform")]
        RelativeTransform,
#endif
        [Description("Child")]
        Child,
    }

#if MODERN
    public enum VrApi
    {
        [Description("OpenVR")]
        OpenVr,
        [Description("OpenXR")]
        OpenXr,
    }
#endif
    
    public enum ScreenSpaceCanvasType
    {
        [Description("None")]
        None,
        [Description("Not rendering to texture")]
        NotToTexture,
        [Description("All")]
        All,
    }

    public enum UiRenderMode
    {
        [Description("Overlay camera (draws on top of everything)")]
        OverlayCamera,
        [Description("In world (can be occluded)")]
        InWorld,
    }

    public enum UiPatchMode
    {
        [Description("Don't touch UI")]
        None,
        [Description("Mirror flat screen (game not mirrored)")]
        Mirror,
        [Description("Patch Canvas objects")]
        CanvasRedirect,
    }

    public readonly ConfigFile Config;
    public readonly HardcodedConfigEntry<CameraTrackingMode> CameraTracking = new(CameraTrackingMode.Absolute);
    public readonly HardcodedConfigEntry<bool> RelativeCameraSetStereoView = new (false);
    public readonly HardcodedConfigEntry<int> VrCameraDepth = new(1);
    public readonly HardcodedConfigEntry<int> VrUiLayerOverride = new(-1);
    public readonly HardcodedConfigEntry<bool> AlignCameraToHorizon = new(false);
    public readonly HardcodedConfigEntry<float> CameraPositionOffsetX = new(0);
    public readonly HardcodedConfigEntry<float> CameraPositionOffsetY = new(0);
    public readonly HardcodedConfigEntry<float> CameraPositionOffsetZ = new(0);
    public readonly HardcodedConfigEntry<bool> OverrideDepth = new(false);
    public readonly HardcodedConfigEntry<bool> PhysicsMatchHeadsetRefreshRate = new(false);
    public readonly HardcodedConfigEntry<UiPatchMode> PreferredUiPatchMode = new(UiPatchMode.Mirror);
    public readonly HardcodedConfigEntry<UiRenderMode> PreferredUiRenderMode = new(UiRenderMode.OverlayCamera);
    public readonly HardcodedConfigEntry<ScreenSpaceCanvasType> ScreenSpaceCanvasTypesToPatch = new(ScreenSpaceCanvasType.NotToTexture);
    public ConfigEntry<bool> NoTerminalVelocity;
    
#if MODERN
    public readonly HardcodedConfigEntry<VrApi> PreferredVrApi = new(VrApi.OpenXr);
#endif

    public ModConfiguration(ConfigFile config)
    {
        Instance = this;

        Config = config;

        NoTerminalVelocity = Config.Bind(
            "Gameplay",
            "No Terminal Velocity",
            true,
            "Turns off vertical velocity limit. Makes falling cool and scary but breaks a few sections."
        );
    }
}

/// <summary>
/// same interface as ConfigEntry so i can refactor less
/// </summary>
public class HardcodedConfigEntry<T>
{
    public readonly T Value;
    public HardcodedConfigEntry(T value) => Value = value;
}