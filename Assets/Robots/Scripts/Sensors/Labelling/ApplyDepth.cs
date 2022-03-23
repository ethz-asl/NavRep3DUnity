using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Camera))]
public class ApplyDepth : MonoBehaviour
{

    public Shader depthShader;

    void Start()
    {
        GetComponent<Camera>().depthTextureMode = GetComponent<Camera>().depthTextureMode | DepthTextureMode.Depth;
        GetComponent<Camera>().SetReplacementShader(depthShader, "RenderType");
    }
}
