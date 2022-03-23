using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(Camera))]
public class ApplySegmentation : MonoBehaviour
{

    public Shader segmentShader;

    Dictionary<string, Color32> segmentDict = new Dictionary<string, Color32>();

    void Start()
    {
        // Fill the Dictionary with Tag names and corresponding colors
        segmentDict.Add("NavRepEnclosingWall", new Color32(255, 0, 1, 255));
        segmentDict.Add("RandomPolygon", new Color32(0, 255, 2, 255));
        segmentDict.Add("VirtualHumanActive", new Color32(255, 125, 3, 255));
        segmentDict.Add("Goal", new Color32(255, 255, 4, 255));
        segmentDict.Add("Floor", new Color32(125, 50, 5, 255));

        // Find all GameObjects with Mesh Renderer and add a color variable to be
        // used by the shader in it's MaterialPropertyBlock
        var renderers = FindObjectsOfType<MeshRenderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {

            if (segmentDict.TryGetValue(r.transform.tag, out Color32 outColor))
            {
                mpb.SetColor("_SegmentColor", outColor);
                r.SetPropertyBlock(mpb);
            }
        }

        // Finally set the Segment shader as replacement shader
        GetComponent<Camera>().SetReplacementShader(segmentShader, "RenderType");
    }

    void Update()
    {
        // Find all GameObjects with Mesh Renderer and add a color variable to be
        // used by the shader in it's MaterialPropertyBlock
        var renderers = FindObjectsOfType<SkinnedMeshRenderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            if (segmentDict.TryGetValue("VirtualHumanActive", out Color32 outColor))
            {
                mpb.SetColor("_SegmentColor", outColor);
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
