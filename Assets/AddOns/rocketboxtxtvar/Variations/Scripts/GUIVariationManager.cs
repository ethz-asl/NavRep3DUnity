using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GUIVariationManager : MonoBehaviour
{
    public GameObject[] CharacterPrefabs;
    int currentCharacterId = 0;

    public GameObject currentCharacterModel;
    Material[] materials;
    int materialBodyId;
    string bodyTextureName;
    string texturePath = "";
    string textureRessourcePath = "";
    bool isV1Texture = false;

    public int variationId = 0;
    public InputField variationText;
    public Button variationPreviousButton;
    public Button saveButton;
    public Button createButton;

    Texture2D textureVariation;

    public HSVSliders[] VariationParts;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < VariationParts.Length; ++i)
            if (VariationParts[i] != null) VariationParts[i].SetGuiManager(this);

        textureVariation = new Texture2D(8, 1, TextureFormat.RGB24, false);
        textureVariation.filterMode = FilterMode.Point;
        UpdateVariationTextureFromSliders();

        variationText.text = variationId.ToString();
        LoadCurrentCharacter();

        if(variationId == 0)
        {
            saveButton.interactable = false;
            createButton.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PreviousCharacter()
    {
        currentCharacterId--;
        if (currentCharacterId < 0) currentCharacterId = (CharacterPrefabs.Length - 1);
        LoadCurrentCharacter();
    }

    public void NextCharacter()
    {
        currentCharacterId++;
        if (currentCharacterId >= CharacterPrefabs.Length) currentCharacterId = 0;
        LoadCurrentCharacter();
    }

    public void LoadCurrentCharacter()
    {
        if (currentCharacterModel != null)
        {
            currentCharacterModel.SetActive(false);
            Object.Destroy(currentCharacterModel);
            currentCharacterModel = null;
            materials = null;
            bodyTextureName = "";
        }

        if (CharacterPrefabs.Length > 0 && currentCharacterId < CharacterPrefabs.Length && currentCharacterId >= 0 && CharacterPrefabs[currentCharacterId] != null)
        {
            currentCharacterModel = Instantiate(CharacterPrefabs[currentCharacterId], this.gameObject.transform.parent);

            TextureVariation tv = currentCharacterModel.GetComponentInChildren<TextureVariation>();
            if (tv != null)
                tv.enabled = false;

            materials = currentCharacterModel.GetComponentInChildren<Renderer>().materials;
            for (int i = 0; i < materials.Length; ++i)
            {
                if (materials[i].name.Contains("body"))
                {
                    materialBodyId = i;
                    bodyTextureName = materials[i].mainTexture.name;

#if UNITY_EDITOR
                    texturePath = UnityEditor.AssetDatabase.GetAssetPath(materials[i].mainTexture);
                    texturePath = Path.GetDirectoryName(texturePath) + "/";
                    textureRessourcePath = texturePath.Substring(17); // Remove Asset/Resources/
#endif
                }

                materials[i].SetTexture("_HSVoffsetMap", textureVariation);
            }
            SetMainBodyTexture(isV1Texture);
        }
    }

    public void SetMainBodyTexture(bool v1)
    {
        isV1Texture = v1;

        string textFullPath = textureRessourcePath + bodyTextureName + (isV1Texture ? "_v1" : "");
        Texture t = Resources.Load(textFullPath, typeof(Texture)) as Texture;
        if (t != null)
        {
            materials[materialBodyId].mainTexture = t;
            ReloadCurrentVariationTexture();
        }
        else
            Debug.Log("Texture " + textFullPath + " not found");
    }

    public void ChangeMainTexture(Toggle change)
    {
        SetMainBodyTexture(change.isOn);
    }

    public void NextVariation()
    {
        variationId++;
        if (variationPreviousButton != null)
            variationPreviousButton.interactable = true;
        ChangeVariation();
    }

    public void PreviousVariation()
    {
        variationId--;
        if (variationId == 0 && variationPreviousButton != null)
            variationPreviousButton.interactable = false;
        ChangeVariation();

        if (variationId == 0)
        {
            saveButton.interactable = false;
            createButton.interactable = false;
        }
    }

    public void ChangeVariation()
    {
        variationText.text = variationId.ToString();
        ReloadCurrentVariationTexture();
    }

    public void ResetDefault()
    {
        for (int i = 0; i < VariationParts.Length; ++i)
            ResetDefault(VariationParts[i]);
    }

    public void ResetDefault(GameObject go)
    {
        HSVSliders[] hs = go.GetComponentsInChildren<HSVSliders>();
        foreach (HSVSliders h in hs)
            ResetDefault(h);
    }

    public void ResetDefault(HSVSliders slider)
    {
        if (slider != null) slider.SetHSV(0.5f, 0.5f, 0.5f);
    }

    public void UpdateVariationTextureFromSliders()
    {
        Color c = new Color();
        for (int i = 0; i < VariationParts.Length; ++i)
        {
            c.r = VariationParts[i].H_slider.value;
            c.g = VariationParts[i].S_slider.value;
            c.b = VariationParts[i].V_slider.value;
            textureVariation.SetPixel(i, 0, c);
        }
        textureVariation.Apply();
    }

    public void OnSliderChanged(HSVSliders sliders)
    {
        Color c = new Color();
        for (int i = 0; i < VariationParts.Length; ++i)
        {
            if (VariationParts[i] == sliders)
            {
                c.r = VariationParts[i].H_slider.value;
                c.g = VariationParts[i].S_slider.value;
                c.b = VariationParts[i].V_slider.value;
                textureVariation.SetPixel(i, 0, c);
            }
        }
        textureVariation.Apply();
    }

    public void DebugVariationTexture()
    {
        string r = "";
        string g = "";
        string b = "";
        for (int i = 0; i < VariationParts.Length; ++i)
        {
            Color c = textureVariation.GetPixel(i, 0);
            r += c.r.ToString("F2") + "  /  ";
            g += c.g.ToString("F2") + "  /  ";
            b += c.b.ToString("F2") + "  /  ";
        }
        Debug.Log(r + "\n" + g + "\n" + b);
    }

    public string CurrentVariationTexture()
    {
        return textureRessourcePath + bodyTextureName.Substring(0, 5) + "Variation_" + (isV1Texture ? "v1_" : "") + variationId;
    }

    public bool ExistsCurrentVariationTexture()
    {
        string fileNameBase = CurrentVariationTexture();
#if UNITY_EDITOR
        fileNameBase = "Assets/Resources/" + fileNameBase;
#endif
        return File.Exists(fileNameBase + ".tga");
    }

    public void SaveCurrentVariationTexture()
    {
        string fileNameBase = CurrentVariationTexture();
#if UNITY_EDITOR
        fileNameBase = "Assets/Resources/" + fileNameBase;
#endif
        TGAFile.SaveTGA(fileNameBase + ".tga", textureVariation);

        saveButton.interactable = true;
        createButton.interactable = false;
    }

    public void ReloadCurrentVariationTexture()
    {
        if (variationId == 0)
        {
            ResetDefault();
            for (int i = 0; i < materials.Length; ++i)
                materials[i].SetTexture("_HSVoffsetMap", textureVariation);
        }
        else if (ExistsCurrentVariationTexture())
        {
            string fileNameBase = CurrentVariationTexture();

            Texture2D prev = textureVariation;

#if UNITY_EDITOR
            fileNameBase = "Assets/Resources/" + fileNameBase;
#endif
            textureVariation = TGAFile.LoadTGA(fileNameBase);
            if (textureVariation != null)
            {
                Color[] cs = textureVariation.GetPixels();
                for (int i = 0; i < VariationParts.Length; ++i)
                {
                    VariationParts[i].H_slider.value = cs[i].r;
                    VariationParts[i].S_slider.value = cs[i].g;
                    VariationParts[i].V_slider.value = cs[i].b;
                }

                for (int i = 0; i < materials.Length; ++i)
                    materials[i].SetTexture("_HSVoffsetMap", textureVariation);
            }

            Object.DestroyImmediate(prev, true);
            saveButton.interactable = true;
            createButton.interactable = false;
        }
        else
        {
            saveButton.interactable = false;
            createButton.interactable = true;
        }
    }

    public void LoadFromTextFile()
    {
#if UNITY_EDITOR
        string fPath = Path.GetFullPath("./" + textureRessourcePath);
        string path = UnityEditor.EditorUtility.OpenFilePanel("Open text file", fPath, "txt");
        if (path.Length != 0)
            LoadFromTextFile(path);
#endif
    }

        public void LoadFromTextFile(string filename)
    {
        if (File.Exists(filename))
        {
            string fileText = File.ReadAllText(filename);
            string[] lines = fileText.Split("\n"[0]);
            string[][] splitLines = new string[lines.Length][];

            for (int i = 0; i < lines.Length; ++i)
                splitLines[i] = lines[i].Split(',');

            Color c = new Color();
            for (int i = 0; i < splitLines[0].Length; ++i)
            {
                c.r = float.Parse(splitLines[0][i]);
                c.g = float.Parse(splitLines[1][i]);
                c.b = float.Parse(splitLines[2][i]);
                VariationParts[i].H_slider.value = c.r;
                VariationParts[i].S_slider.value = c.g * .5f;
                VariationParts[i].V_slider.value = c.b * .5f;
                textureVariation.SetPixel(i, 0, c);
            }
            textureVariation.Apply();
        }
    }
}