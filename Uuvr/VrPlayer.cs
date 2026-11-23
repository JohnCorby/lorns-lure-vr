using System;
using UnityEngine;
using UnityEngine.XR;

namespace Uuvr;

/// <summary>
/// handles the walking and clinging camera, and the hands
/// </summary>
public class VrPlayer : MonoBehaviour
{
    public static VrPlayer Instance;

    private ClimbingAbilityV2 _climber;
    private Canvas _ui;
    private GameObject _vrPivot;
    
    private Camera defaultCamera, climbingCamera;
    
    public bool isClimbingView => climbingCamera.enabled;

    private GameObject origin;
    public VrHand leftHand, rightHand;
    private GrappleHook grappleHook;
    public Transform flarePoint;

    public VrPlayer()
    {
    }

    public static readonly Shader UnlitAlwaysDrawShader = Shader.Find("GUI/Text Shader");

    private void Awake()
    {
        _climber = GetComponent<ClimbingAbilityV2>(); // this should be on ClimbingSystemPrefab/Climber
        _ui = GameObject.Find("UI").GetComponent<Canvas>();
        // TODO move this to right hand. except for Blackout/DeathImage part (DemoTransition holds that stuff)
        // okay moving ui is gonna be annoying. will have to test each thing to see if it should be on hand or not. like uh terminal container probably shouldnt be maybe
        
        defaultCamera = _climber.defaultCamera.GetComponent<Camera>();
        climbingCamera = _climber.climbingCamera.GetComponent<Camera>();
        
        grappleHook = _climber.GetComponent<GrappleHook>();
        
        // move flare spawn and throw to right hand
        // move grapple to left. make laser for grapple that changes color when u can get it, turn off timing aspect of grapple
        // move picks to hands. for climbing probably just use cubes, oh well
        // move scanner raycast to right hand
        
        // maybe move everything to right hand lol
        // also grapple trigger is right hand lol that is confusing!
        
        /*
         * notes:
         * Climber and ClimbingCameraPivot rotates on y axis. Camera and ClimbingPlayerView rotates on x axis
         * Camera_Pivot squashes when crouching to half scale
         * camera is 1.6 units above feet
         * ClimbingAbility.onwall to know which camera is active. or just look at the camera lol
         *
         * try not to move components, because things are referenced a lot by Find and by GetComponent on adjacent things. this might make UI really annoying :(
         *
         * picks are on HiRezPickRightClose (AnimatedHands for climbing but idk about that). left has grapple as a child
         */

        _vrPivot = new GameObject("VrPivot");
        _vrPivot.transform.SetParent(transform, false);
        
        leftHand = VrHand.Create(XRNode.LeftHand, _climber.leftPickClose);
        leftHand.transform.SetParent(_vrPivot.transform, false);
        
        rightHand = VrHand.Create(XRNode.RightHand, _climber.rightPickClose);
        rightHand.transform.SetParent(_vrPivot.transform, false);

        // make feet origin for reference
        origin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DestroyImmediate(origin.GetComponent<Collider>());
        origin.transform.SetParent(_vrPivot.transform, false);
        origin.transform.localScale = new(.1f, .01f, .1f);
        origin.GetComponent<Renderer>().material.color = Color.grey;
        // origin.GetComponent<Renderer>().material.shader = VrPlayer.UnlitAlwaysDrawShader;


        // turn off camera picks. using setactive breaks pause somehow what
        foreach (var renderer in _climber.leftPickClose.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = false;
        foreach (var renderer in _climber.rightPickClose.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = false;
        
        // _ui.transform.SetParent(rightHand.transform, false);
        // _ui.renderMode = RenderMode.ScreenSpaceCamera;
        // _ui.planeDistance = 1;

        {
            grappleHook.gunTip.transform.SetParent(leftHand.transform, false);
            grappleHook.gunTip.transform.localPosition = new(0, .12f, .35f);
            grappleHook.gunTip.transform.localRotation = Quaternion.identity;
            grappleHook.grappleSpark.transform.SetParent(grappleHook.gunTip, false);
            grappleHook.grappleSpark.transform.localPosition = Vector3.zero;
            grappleHook.grappleSpark.transform.localRotation = Quaternion.identity;
        }

        {
            flarePoint = new GameObject("flare spawner").transform;
            flarePoint.SetParent(rightHand.transform, false);
            flarePoint.localPosition = new(0, 0, .35f);
            flarePoint.localRotation = Quaternion.identity;
        }
    }

    private void Start()
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        // used to try to be smart with pivots. but that broke with headtilt. so whatever, crouching will just scale weird
        _vrPivot.transform.SetParent(isClimbingView ? climbingCamera.transform.parent : defaultCamera.transform.parent, false);
        
        origin.SetActive(_climber.bfpsc.isGrounded);
        
        leftHand.model.SetActive(_climber.leftPickClose.activeSelf);
        rightHand.model.SetActive(_climber.rightPickClose.activeSelf);
        leftHand.model.transform.Find("GrappleAttachmentRunning").gameObject.SetActive(grappleHook.abilityActive);

        _ui.worldCamera = isClimbingView ? climbingCamera : defaultCamera;
    }
}