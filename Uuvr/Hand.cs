using UnityEngine;
using UnityEngine.XR;

namespace Uuvr;

public class Hand : MonoBehaviour
{
    private XRNode hand;

    public static void Create(VrCamera.VrCamera vrCamera, XRNode hand)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(go.GetComponent<Collider>());
        go.AddComponent<Hand>().hand = hand;

        go.transform.SetParent(vrCamera.transform.parent);
        go.transform.localScale = Vector3.one * .1f;
    }

    private void Awake()
    {
    }

    private void Update()
    {
        transform.localPosition = InputTracking.GetLocalPosition(hand);
        transform.localRotation = InputTracking.GetLocalRotation(hand);
    }
}