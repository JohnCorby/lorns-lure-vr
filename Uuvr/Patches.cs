using ECM.Components;
using ECM.Controllers;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Uuvr;

[HarmonyPatch]
public static class Patches
{
    public static Vector3 hehe = Vector3.one * 0.5f;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Camera), "set_fieldOfView")]
    // Unity already prevents __instance, but it also nags you constantly about it.
    // Some games try to change the FOV every frame, and all those logs can reduce performance.
    private static bool PreventChangingFov(float value)
    {
        return false;
    }


    #region game patches

    // TODO turn bloom off, turn down sens to 2
    // TODO: fix cutscene camera, it also never ends on level 2 cutscene, you have to skip manually

    /// <summary>
    /// dont care about fov
    ///
    /// also dash uses left hand like grapple
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(typeof(Dash), nameof(Dash.Update))]
    private static void Dash_Update(Dash __instance)
    {
        if (PauseHelper.currentlyPaused || PauseHelper.cutscenePause || PauseHelper.narrationPause)
        {
            return;
        }

        var mainCameraTransform = VrPlayer.Instance.leftHand.transform; // tada
        Debug.DrawLine(mainCameraTransform.position, mainCameraTransform.position + mainCameraTransform.forward * __instance.dashForce, Color.green);
        if (__instance.abilityActive && !__instance.dashing && !__instance.GetComponent<ClimbingAbilityV2>().cm.isGrounded && !__instance.GetComponent<FallDeath>().criticalVelocityReached && __instance.GetComponent<StaminaSystem>().stamina > 30f && Time.time - __instance.lastDashTime > __instance.dashResetTimeSeconds && !__instance.GetComponent<ClimbingAbilityV2>().onWall)
        {
            __instance.lagReticle.GetComponent<FlashUI>().enabled = true;
            if (Input.GetButtonDown("Dash"))
            {
                __instance.GetComponent<BaseFirstPersonController>().movement.StopVelocity();
                if (mainCameraTransform.forward.y < 0f)
                {
                    __instance.GetComponent<BaseFirstPersonController>().movement.ApplyImpulse(__instance.weakDashForce * 0.02f, __instance.transform.forward);
                }
                else
                {
                    __instance.GetComponent<BaseFirstPersonController>().movement.ApplyImpulse(__instance.dashForce * 0.02f, mainCameraTransform.forward);
                }

                __instance.GetComponent<BaseFirstPersonController>().airControl = __instance.airControlWhileDash;
                __instance.csem.sec.PlayOnce("dash");
                __instance.csem.lastFrameState = "inAir";
                __instance.GetComponent<StaminaSystem>().PauseStamina();
                __instance.GetComponent<StaminaSystem>().resting = false;
                __instance.GetComponent<StaminaSystem>().DepleteStaminaBurst(__instance.dashStaminaDepletionAmount);
                __instance.dashing = true;
                __instance.allowExternalFOVChange = false;
                if (__instance.im != null)
                {
                    __instance.im.dashJetCanisters--;
                }

                __instance.lastDashTime = Time.time;
            }
        }
        else if (!__instance.GetComponent<RunDash>().running)
        {
            __instance.lagReticle.GetComponent<FlashUI>().ResetColor();
            __instance.lagReticle.GetComponent<FlashUI>().enabled = false;
        }

        __instance.dashFOV = __instance.defaultFOV + __instance.FOVDashBoost;
        if (__instance.dashing && __instance.mainCamera.fieldOfView < __instance.dashFOV)
        {
            __instance.mainCamera.fieldOfView += __instance.FOVJoltSpeed * Time.deltaTime;
            if (__instance.mainCamera.fieldOfView >= __instance.dashFOV)
            {
                __instance.dashing = false;
            }

            __instance.dashing = false; // dont care about fov
        }

        if (!__instance.dashing && __instance.mainCamera.fieldOfView > __instance.defaultFOV)
        {
            __instance.mainCamera.fieldOfView -= __instance.FOVRelaxSpeed * Time.deltaTime;
            if (__instance.mainCamera.fieldOfView < __instance.defaultFOV)
            {
                __instance.mainCamera.fieldOfView = __instance.defaultFOV;
                __instance.allowExternalFOVChange = true;
            }
        }
    }

    /// <summary>
    /// cant change fov in vr. dont care
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(typeof(ZoomIn), nameof(ZoomIn.Update))]
    private static bool ZoomIn_Update() => false;

    /// <summary>
    /// no terminal velocity makes falling scarier
    /// need to keep rise velocity because dash goes up a lot for some reason
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    [HarmonyPostfix, HarmonyPatch(typeof(CharacterMovement), nameof(CharacterMovement.maxFallSpeed), MethodType.Getter)]
    private static void CharacterMovement_maxFallSpeed(CharacterMovement __instance, ref float __result)
    {
        if (ModConfiguration.Instance.NoTerminalVelocity.Value) __result = float.MaxValue;
    }

    // CheckClimbTriggered for climb casts. move to picks

    /// <summary>
    /// grapple raycast should come out of hand
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(typeof(GrappleHook), nameof(GrappleHook.Update))]
    private static bool GrappleHook_Update(GrappleHook __instance)
    {
        if (!__instance.abilityActive)
        {
            return false;
        }

        if (__instance.GetComponent<FallDeath>().dead)
        {
            __instance.StopGrapple();
            return false;
        }

        if (__instance.activeTimeOffset)
        {
            return false;
        }

        if (PauseHelper.cutscenePause || PauseHelper.currentlyPaused || PauseHelper.narrationPause)
        {
            return false;
        }

        if (!__instance.GetComponent<ClimbingAbilityV2>().abilityActive)
        {
            return false;
        }

        // raycast from left hand gun tip
        bool flag = Physics.Raycast(__instance.gunTip.position, __instance.gunTip.forward, out __instance.hit, __instance.maxDistance, __instance.whatIsGrappleable);
        if (flag && __instance.GetComponent<UIToggleControls>().liveMode == "default")
        {
            __instance.grapplableReticle.SetActive(true);
            if (__instance.GetComponent<FallDeath>().criticalVelocityReached || __instance.GetComponent<StaminaSystem>().stamina <= 0f)
            {
                __instance.grapplableReticle.GetComponent<Image>().color = __instance.terminalColor;
            }
            else
            {
                __instance.grapplableReticle.GetComponent<Image>().color = __instance.grapplableColor;
            }
        }
        else
        {
            __instance.grapplableReticle.SetActive(false);
        }

        // removing timing aspect
        if (Input.GetButton("Grapple") && !__instance.lr.enabled)
        {
            __instance.StartGrapple(flag);
        }
        else if (Input.GetButtonUp("Grapple") && !__instance.GetComponent<ClimbingAbilityV2>().exitClimbByReachingTop && !__instance.GetComponent<ClimbingAbilityV2>().climbingLedge && !__instance.GetComponent<ClimbingAbilityV2>().onWall)
        {
            __instance.StopGrapple();
        }

        if (__instance.IsGrappling() && __instance.GetComponent<StaminaSystem>().stamina <= 0f)
        {
            __instance.StopGrapple();
        }

        if (__instance.IsGrappling() && __instance.terminalDetachApplies && __instance.GetComponent<FallDeath>().criticalVelocityReached)
        {
            __instance.StopGrapple();
        }

        if (Input.GetButtonDown("TryAgain"))
        {
            __instance.StopGrapple();
        }

        __instance.DrawRope();

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GrappleHook), nameof(GrappleHook.ShowPicks))]
    [HarmonyPatch(typeof(GrappleHook), nameof(GrappleHook.HidePicks))]
    private static bool GrappleHook_Picks(GrappleHook __instance)
    {
        __instance.grappleGunModel.SetActive(false);
        return false;
    }


    /// <summary>
    /// throw flare from right hand
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(typeof(FlareGemThrower), nameof(FlareGemThrower.ThrowFlare))]
    public static bool FlareGemThrower_ThrowFlare(FlareGemThrower __instance)
    {
        GameObject gameObject = Object.Instantiate<GameObject>(__instance.flarePrefab, VrPlayer.Instance.flarePoint.position, VrPlayer.Instance.flarePoint.rotation);
        gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * __instance.throwStrength);
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        gameObject.GetComponent<FlareBehaviour>().flareLight.range = 40f;
        gameObject.GetComponent<FlareBehaviour>().thrown = true;
        gameObject.GetComponent<FlareBehaviour>().trail.Play();
        __instance.numFlares--;
        __instance.lastFlareThrowTime = Time.time;
        __instance.rts.UpdateFlares(__instance.numFlares, __instance.MAX_FLARES);
        __instance.FlareThrown(gameObject);
        __instance.cav2.csem.lastFrameState = "";
        if (__instance.flareAudioGeneral)
        {
            __instance.flareAudioGeneral.PlayOneShot(__instance.throwCrystal);
        }

        return false;
    }


    /*
    /// <summary>
    /// scan from right hand. dont entirely need this but im trying it anyway
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(typeof(Scanner), nameof(Scanner.Update))]
    private static bool Scanner_Update(Scanner __instance)
    {
        if (__instance.isScanning)
        {
            if (Input.GetButtonDown("UIScanClose") || Input.GetButtonUp("Scan"))
            {
                __instance.scanAudio.Stop();
                __instance.StopAllCoroutines();
                __instance.isScanning = false;
                __instance.ClearAllReticles();
                __instance.unclimbableCursor.color = __instance.unclimbableColor;
                __instance.scanCursor.SetActive(false);
            }

            return false;
        }

        if (__instance.GetComponent<ClimbingAbilityV2>().onWall)
        {
            if (__instance.rayOrigin.gameObject.name != "ClimbingPlayerView")
            {
                __instance.rayOrigin = GameObject.Find("ClimbingPlayerView").transform;
            }
        }
        else if (__instance.rayOrigin.gameObject.name != "Camera")
        {
            __instance.rayOrigin = GameObject.Find("Camera").transform;
        }

        bool flag = Physics.Raycast(__instance.rayOrigin.position, __instance.rayOrigin.forward, out __instance.scanRay, __instance.sensorRange);
        float num = 0f;
        if (flag)
        {
            num = Vector3.Distance(__instance.rayOrigin.position, __instance.scanRay.point);
            if (__instance.scanRay.collider.gameObject.GetComponent<ScannableObject>() && num > __instance.scanRange)
            {
                flag = false;
            }
        }

        if (flag && !PauseHelper.currentlyPaused && !PauseHelper.narrationPause)
        {
            if (__instance.scanRay.collider.tag == "Scannable" && num <= __instance.scanRange)
            {
                __instance.ClearAllReticles();
                if (!__instance.scanRay.collider.gameObject.GetComponent<ScannableObject>().historicallyScanned)
                {
                    __instance.scannableCursor.color = __instance.scannableReticleColor;
                }
                else
                {
                    __instance.scannableCursor.color = __instance.alreadyScannedReticleColor;
                }

                if (Input.GetButtonDown("Scan") && __instance.scanRay.collider.gameObject.GetComponent<ScannableObject>().allowRescan && !__instance.terminalContainer.activeSelf)
                {
                    Debug.Log("play scan sound");
                    __instance.isScanning = true;
                    __instance.scanAudio.Play();
                    __instance.ClearAllReticles();
                    __instance.scannableCursor.color = __instance.scanningReticleColor;
                    __instance.scanCursor.SetActive(true);
                    __instance.StopAllCoroutines();
                    __instance.StartCoroutine(__instance.StartScan());
                    return false;
                }
            }
            else
            {
                if (Mathf.Abs(__instance.scanRay.normal.y) < 0.0001f && __instance.scanRay.collider.tag != "Unclimbable" && __instance.scanRay.collider.tag != "NoClimbOrLedgeClimb" && __instance.scanRay.collider.tag != "Scannable")
                {
                    __instance.ClearAllReticles();
                    __instance.climbableCursor.color = __instance.climbableReticleColor;
                    return false;
                }

                __instance.ClearAllReticles();
                __instance.unclimbableCursor.color = __instance.unclimbableColor;
                return false;
            }
        }
        else
        {
            __instance.ClearAllReticles();
            __instance.reticle.color = __instance.unreachableReticleColor;
        }

        return false;
    }
    */

    #endregion
}