using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;




public class testCPP : MonoBehaviour {

    [DllImport("CrowdSim")]
    public static extern int add(int num1, int num2);
    [DllImport("CrowdSim")]
    public static extern int multiply(int num1, int num2);
    [DllImport("CrowdSim")]
    public static extern int substract(int num1, int num2);
    [DllImport("CrowdSim")]
    public static extern int divide(int num1, int num2);

    // Use this for initialization
    void Start () {
        Debug.Log("Add: " + add(10, 2));
        Debug.Log("Multiply: " + multiply(10, 2));
        Debug.Log("Substract: " + substract(10, 2));
        Debug.Log("Divide: " + divide(10, 2));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
