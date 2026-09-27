using ECM.Controllers;
using HarmonyLib;
using UnityEngine;

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

    [HarmonyPrefix, HarmonyPatch(typeof(Dash), nameof(Dash.Update))]
    private static void Dash_Update(Dash __instance)
    {
        if (PauseHelper.currentlyPaused || PauseHelper.cutscenePause || PauseHelper.narrationPause)
        {
            return;
        }

        Debug.DrawLine(__instance.mainCamera.transform.position, __instance.mainCamera.transform.position + __instance.mainCamera.transform.forward * __instance.dashForce, Color.green);
        if (__instance.abilityActive && !__instance.dashing && !__instance.GetComponent<ClimbingAbilityV2>().cm.isGrounded && !__instance.GetComponent<FallDeath>().criticalVelocityReached && __instance.GetComponent<StaminaSystem>().stamina > 30f && Time.time - __instance.lastDashTime > __instance.dashResetTimeSeconds && !__instance.GetComponent<ClimbingAbilityV2>().onWall)
        {
            __instance.lagReticle.GetComponent<FlashUI>().enabled = true;
            if (Input.GetButtonDown("Dash"))
            {
                __instance.GetComponent<BaseFirstPersonController>().movement.StopVelocity();
                if (__instance.mainCamera.transform.forward.y < 0f)
                {
                    __instance.GetComponent<BaseFirstPersonController>().movement.ApplyImpulse(__instance.weakDashForce * 0.02f, __instance.transform.forward);
                }
                else
                {
                    __instance.GetComponent<BaseFirstPersonController>().movement.ApplyImpulse(__instance.dashForce * 0.02f, __instance.mainCamera.transform.forward);
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

    // cant change fov in vr. dont care
    [HarmonyPrefix, HarmonyPatch(typeof(ZoomIn), nameof(ZoomIn.Update))]
    private static bool ZoomIn_Update() => false;

    #region input

    

    #endregion

    #endregion
}