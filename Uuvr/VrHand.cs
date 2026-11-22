using UnityEngine;
using UnityEngine.XR;

namespace Uuvr;

public class VrHand : MonoBehaviour
{
    private XRNode node;
    private GameObject model;

    public static VrHand Create(XRNode node, GameObject modelBase)
    {
        var go = new GameObject($"hand {node}");
        
        // var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Destroy(cube.GetComponent<Collider>());
        // cube.transform.SetParent(go.transform, false);
        
        var hand = go.AddComponent<VrHand>();
        hand.node = node;
        hand.model = Instantiate(modelBase);
        hand.model.transform.SetParent(go.transform, false);

        go.transform.localScale = Vector3.one * .1f;

        return hand;
    }

    private void Awake()
    {
    }

    private void Update()
    {
        // TODO at some point use steamvr input and see if that gives any difference of position
        transform.localPosition = InputTracking.GetLocalPosition(node);
        transform.localRotation = InputTracking.GetLocalRotation(node);
        
        // climbing makes its own hands. im keeping them detached
        model.SetActive(VrPlayer.Instance.isClimbingView);
    }
}