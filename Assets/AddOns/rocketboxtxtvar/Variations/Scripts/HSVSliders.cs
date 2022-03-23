using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HSVSliders : MonoBehaviour
{
    GUIVariationManager guiManager;

    public Slider H_slider;
    public Slider S_slider;
    public Slider V_slider;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void SetGuiManager(GUIVariationManager m)
    {
        guiManager = m;
    }

    public void SetHSV(float h, float s, float v)
    {
        H_slider.value = h;
        S_slider.value = s;
        V_slider.value = v;
    }
}
