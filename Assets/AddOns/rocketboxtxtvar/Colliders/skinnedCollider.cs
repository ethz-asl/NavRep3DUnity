using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skinnedCollider : MonoBehaviour {
    public enum ColliderType
    {
        None,
        CapsuleCollider,
        BasicColliders,
        MeshCollider
        // , Smart //TODO
    };

    public ColliderType collider_type = ColliderType.BasicColliders;

    // TODO
    // public GameObject smartReference;

    // capsule colider type
    private CapsuleCollider mainCapsule;

    // basic colliders type
    private CapsuleCollider[] basicColliders;

    // mesh collider type
    private SkinnedMeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh colliderMesh;

    public void SetupBasicColliders()
    {
        GameObject root = transform.parent.Find("Bip01").gameObject;
        Transform[] allChildren = root.GetComponentsInChildren<Transform>();
        
        foreach(Transform tf in allChildren)
        {
            
            if (tf.name == "Bip01 Head") { createCapsule(tf, 0.14f, 0f, new Vector3(-0.08f, 0f, 0f), 0); } // head

            // Trunk
            if (tf.name == "Bip01 Spine") { createCapsule(tf, 0.12f, 0.4f, new Vector3(0f, 0f, 0f), 2); } // lower body
            if (tf.name == "Bip01 Spine1") { createCapsule(tf, 0.12f, 0.4f, new Vector3(-0.07f, 0f, 0f), 2); } // middle body
            if (tf.name == "Bip01 Spine2") { createCapsule(tf, 0.12f, 0.4f, new Vector3(-0.11f, 0f, 0f), 2); } // upper body
            //if (tf.name == "Bip01 Spine2") { createCapsule(tf,0.17f, 0.75f,new Vector3(0.15f,0f,0f),0); } // whole trunk

            // Arms
            if (tf.name.Contains("UpperArm")) { createCapsule(tf, 0.05f, 0.28f, new Vector3(-0.1f, 0f, 0f), 0); } // UpperArm
            if (tf.name.Contains("Forearm")) { createCapsule(tf, 0.04f, 0.34f, new Vector3(-0.1f, 0f, 0f), 0); } // ForeArm
            if (tf.name.Contains("Hand")) { createCapsule(tf, 0.04f, 0.18f, new Vector3(-0.08f, 0f, 0f), 0); } // Hand

            // Legs
            if (tf.name.Contains("Thigh")) { createCapsule(tf, 0.08f, 0.5f, new Vector3(-0.23f, 0f, 0f), 0); } // Thigh
            if (tf.name.Contains("Calf")) { createCapsule(tf, 0.07f, 0.4f, new Vector3(-0.25f, 0f, 0f), 0); } // Shank
            if (tf.name.Contains("Toe") && !tf.name.Contains("Nub")) { createCapsule(tf, 0.08f, 0.27f, new Vector3(0.08f, 0.02f, 0f), 0); } // Foot

        }


    }

    void createCapsule(Transform tf, float radius, float height, Vector3 center, int Direction)
    {
        CapsuleCollider capsule = tf.GetComponent<CapsuleCollider>();
        if(capsule == null) capsule = (CapsuleCollider)tf.gameObject.AddComponent(typeof(CapsuleCollider));
        capsule.center = center;
        capsule.direction = Direction; //X axis
        capsule.radius = radius;
        capsule.height = height;
    }
    
    public void RemoveBasicColliders()
    {
        basicColliders = transform.parent.Find("Bip01").GetComponentsInChildren<CapsuleCollider>();
        foreach(CapsuleCollider col in basicColliders)
        {
            DestroyImmediate(col);
        }
    }

    void Start(){
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        colliderMesh = new Mesh();
        mainCapsule = transform.parent.GetComponent<CapsuleCollider>();
        basicColliders = transform.parent.Find("Bip01").GetComponentsInChildren<CapsuleCollider>();
        UpdateMeshCollider();

        meshCollider.enabled = false;
        mainCapsule.enabled = false;
        foreach(CapsuleCollider col in basicColliders)
        {
            col.enabled = false;
        }
    }

    public void UpdateMeshCollider()
    {
        colliderMesh.Clear();
        meshRenderer.BakeMesh(colliderMesh);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = colliderMesh;
    }
	
    // Update is called once per frame
	void Update () {
        switch(collider_type)
        {
            case ColliderType.None:
                meshCollider.enabled = false;
                mainCapsule.enabled = false;
                foreach(CapsuleCollider col in basicColliders)
                {
                    col.enabled = false;
                }
            break;

            case ColliderType.CapsuleCollider:
                meshCollider.enabled = false;
                mainCapsule.enabled = true;
                foreach(CapsuleCollider col in basicColliders)
                {
                    col.enabled = false;
                }
            break;

            case ColliderType.BasicColliders:
                meshCollider.enabled = false;
                mainCapsule.enabled = false;
                foreach(CapsuleCollider col in basicColliders)
                {
                    col.enabled = true;
                }
            break;

            case ColliderType.MeshCollider:
                meshCollider.enabled = true;
                mainCapsule.enabled = false;
                foreach(CapsuleCollider col in basicColliders)
                {
                    col.enabled = false;
                }
                UpdateMeshCollider();
            break;

            // case ColliderType.Smart:
            //     meshCollider.enabled = false;
            //     mainCapsule.enabled = false;
            //     foreach(CapsuleCollider col in basicColliders)
            //     {
            //         col.enabled = false;
            //     }
            // break;
        }
	}
    void OnDestroy(){
        try
        {
            Destroy(colliderMesh);
            Destroy(mainCapsule);
            foreach(CapsuleCollider col in basicColliders)
            {
                Destroy(col);
            }
        }
        catch{}
    }
}
