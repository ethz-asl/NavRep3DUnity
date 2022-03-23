using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureVariation : MonoBehaviour
{
    public Texture v0;
    public Texture v1;
    public Texture variation_0;
    public Texture[] variations;

    public bool automaticVariationId = true;
    public int variationID = 0;

    bool initialised = false;

    Material[] materials;
    int materialBodyId;

    // Use this for initialization
    void Start ()
    {
        if (initialised)
            return;

        if (automaticVariationId)
            variationID = Random.Range(0, getNumberOfVariationsMax());
        InitialiseTextureVariation();
        SetVariation();
	}

    public void InitialiseTextureVariation()
    {
        materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length; ++i)
        {
            if (materials[i].name.Contains("body"))
                materialBodyId = i;
        }

        initialised = true;
    }

    // Update is called once per frame
    void Update () {
		
	}

    public int getNumberOfVariationsMax()
    {
        return 2 + variations.Length;
    }

    public void SetVariationId(int id)
    {
        if (!initialised)
            InitialiseTextureVariation();

        variationID = id;
        SetVariation();
    }

    private void SetVariation()
    {
        if(variationID >= getNumberOfVariationsMax())
        {
            Debug.Log("Error variation ID " + variationID + " incorrect for GameObject " + gameObject.name);
            return;
        }
        if(variationID < 2)
        {
            materials[materialBodyId].mainTexture = (variationID == 0 ? v0 : v1);
            for (int i = 0; i < materials.Length; ++i)
                materials[i].SetTexture("_HSVoffsetMap", variation_0);
        }
        else 
        {
            if(variations[variationID - 2].name.Contains("_v1_"))
                materials[materialBodyId].mainTexture = v1;
            else
                materials[materialBodyId].mainTexture = v0;
            for (int i = 0; i < materials.Length; ++i)
                materials[i].SetTexture("_HSVoffsetMap", variations[variationID - 2]);
        }
    }
}
