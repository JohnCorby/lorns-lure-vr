using UnityEngine;
using UnityEngine.XR;

namespace Uuvr;

public class VrHand : MonoBehaviour
{
    private XRNode node;
    public GameObject model;
    public GameObject offset;

    public static VrHand Create(XRNode node, GameObject modelBase)
    {
        var go = new GameObject($"hand {node}");

        AddGizmo(go.transform);

        var offset = new GameObject("hacky offset");
        offset.transform.SetParent(go.transform, false);
        // eyeballing it based on my controllers to make it face the pointer direction
        offset.transform.localPosition = new(0, -.06f, 0);
        offset.transform.localEulerAngles =  new(30, 0,0);

        var hand = go.AddComponent<VrHand>();
        hand.node = node;
        hand.offset = offset;
        hand.model = Instantiate(modelBase); // need to clone because something moves the picks every frame
        hand.model.transform.SetParent(offset.transform, false);
        // eyeballing it
        hand.model.transform.localPosition = new(0, 0, -0.4f);
        hand.model.transform.localEulerAngles = new(0, 0, 90);
        hand.model.transform.localScale = Vector3.one * .1f;

        return hand;
    }

    private void Update()
    {
        // TODO at some point use steamvr input and see if that gives any difference of position
        
        // TODO make a child thingy and tilt picks down (on y axis)
        transform.localPosition = InputTracking.GetLocalPosition(node);
        transform.localRotation = InputTracking.GetLocalRotation(node);
    }

    public static GameObject AddGizmo(Transform transform)
    {
        var go = new GameObject("gizmo");
        go.transform.SetParent(transform, false);

        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new(.05f, 0, 0);
            cube.transform.localScale = new(.1f, .01f, .01f);
            cube.GetComponent<Renderer>().material.color = Color.red;
            // cube.GetComponent<Renderer>().material.shader = VrPlayer.UnlitAlwaysDrawShader;
        }
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new(0, .05f, 0);
            cube.transform.localScale = new(.01f, .1f, .01f);
            cube.GetComponent<Renderer>().material.color = Color.green;
            // cube.GetComponent<Renderer>().material.shader = VrPlayer.UnlitAlwaysDrawShader;
        }
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new(0, 0, .05f);
            cube.transform.localScale = new(.01f, .01f, .1f);
            cube.GetComponent<Renderer>().material.color = Color.blue;
            // cube.GetComponent<Renderer>().material.shader = VrPlayer.UnlitAlwaysDrawShader;
        }
        return go;
    }
}