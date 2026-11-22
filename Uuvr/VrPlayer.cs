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

    private void Awake()
    {
        _climber = GetComponent<ClimbingAbilityV2>(); // this should be on ClimbingSystemPrefab/Climber
        _ui = GameObject.Find("UI").GetComponent<Canvas>();
        // TODO move this to right hand. except for Blackout/DeathImage part (DemoTransition holds that stuff)
        // okay moving ui is gonna be annoying. will have to test each thing to see if it should be on hand or not. like uh terminal container probably shouldnt be maybe
        
        defaultCamera = _climber.defaultCamera.GetComponent<Camera>();
        climbingCamera = _climber.climbingCamera.GetComponent<Camera>();
        
        // move flare spawn and throw to right hand
        // move grapple to left. make laser for grapple that changes color when u can get it, turn off timing aspect of grapple
        // move picks to hands. for climbing probably just use cubes, oh well
        // move scanner raycast to right hand
        
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
        // TODO: when wal turning. make sure to use climbing camera pivot for y axis turning.
        
        var leftHand = VrHand.Create(XRNode.LeftHand, GameObject.Find("HiRezPickLeftClose"));
        leftHand.transform.SetParent(_vrPivot.transform, false);
        
        var rightHand = VrHand.Create(XRNode.RightHand, GameObject.Find("HiRezPickRightClose"));
        rightHand.transform.SetParent(_vrPivot.transform, false);

        // make feet origin for reference
        var origin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(origin.GetComponent<Collider>());
        origin.transform.SetParent(transform.parent, false);
        origin.transform.localScale = Vector3.one * .1f;
        origin.transform.SetParent(_vrPivot.transform, false);
        
        GameObject.Find("WalkShifting").gameObject.SetActive(false); // turn off the hands
    }

    private void Update()
    {
        _vrPivot.transform.SetParent(isClimbingView ? _climber.climbingPivot.transform : _climber.transform);
    }
}