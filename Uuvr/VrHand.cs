using UnityEngine;
using UnityEngine.XR;

namespace Uuvr;

public class VrHand : MonoBehaviour
{
    private XRNode node;
    private GameObject model;

    // TODO: model doesnt update. use gain climbing ability and uh probably grapple since that has little doodad
    public static VrHand Create(XRNode node, GameObject modelBase)
    {
        var go = new GameObject($"hand {node}");

        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new(.05f, 0, 0);
            cube.transform.localScale = new(.1f, .01f, .01f);
            cube.GetComponent<Renderer>().material.color = Color.red;
            cube.GetComponent<Renderer>().material.shader = VrPlayer.UnlitAlwaysDrawShader;
        }
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new(0, .05f, 0);
            cube.transform.localScale = new(.01f, .1f, .01f);
            cube.GetComponent<Renderer>().material.color = Color.green;
            cube.GetComponent<Renderer>().material.shader = VrPlayer.UnlitAlwaysDrawShader;
        }
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new(0, 0, .05f);
            cube.transform.localScale = new(.01f, .01f, .1f);
            cube.GetComponent<Renderer>().material.color = Color.blue;
            cube.GetComponent<Renderer>().material.shader = VrPlayer.UnlitAlwaysDrawShader;
        }


        var hand = go.AddComponent<VrHand>();
        hand.node = node;
        hand.model = Instantiate(modelBase);
        hand.model.transform.SetParent(go.transform, false);
        // eyeballing it
        hand.model.transform.localPosition = new(0, 0, -0.4f);
        hand.model.transform.localRotation = Quaternion.Euler(0, 0, 90);
        hand.model.transform.localScale = Vector3.one * .1f;

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
        model.SetActive(!VrPlayer.Instance.isClimbingView);
    }
}