using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadAllCharactersWithVariations : MonoBehaviour {

    public string folder = "Characters/CharactersHumanoidConfiguration/RocketBox/";

    public RuntimeAnimatorController animator;

    public int nbCharacters = 20;

    // Use this for initialization
    void Awake ()
    {
        string path, actSex, actId, characterPath;
        GameObject temp;
        Object prefab;

        string[] sexFolder = { "male" , "female"};

        GameObject characters = new GameObject("Characters");

        for (int s = 0; s<2; ++s)
        {
            path = sexFolder[s % 2];
            actSex = path.Substring(0, 1);
            for (int i = 1; i <= nbCharacters; ++i)
            {
                actId = actSex + i.ToString("D3");
                characterPath = folder + path + "/" + actId + "/" + actId + "_variation";

                // load character in Unity.
                Object ro = Resources.Load(characterPath, typeof(GameObject));
                if(ro != null)
                { 
                    temp = UnityEngine.Object.Instantiate(ro, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), characters.transform) as GameObject;

                    if(animator != null)
                        temp.GetComponent<Animator>().runtimeAnimatorController = UnityEngine.Object.Instantiate(animator);

                    temp.transform.position = new Vector3( 1 - i - nbCharacters * s, 0, 0);
                    TextureVariation tv = temp.GetComponentInChildren<TextureVariation>();
                    tv.automaticVariationId = false;
                    tv.InitialiseTextureVariation();
                    tv.SetVariationId(0);

                    for (int v = 1; v < tv.getNumberOfVariationsMax(); v++)
                    {
                        temp = UnityEngine.Object.Instantiate(ro, new Vector3(1 - i - nbCharacters * s, 0, -v), Quaternion.Euler(0, 0, 0), characters.transform) as GameObject;

                        tv = temp.GetComponentInChildren<TextureVariation>();
                        tv.automaticVariationId = false;
                        tv.InitialiseTextureVariation();
                        tv.SetVariationId(v);
                    }
                }
            }
        }
        characters.transform.localRotation = Quaternion.Euler(0, 180.0f, 0);
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
