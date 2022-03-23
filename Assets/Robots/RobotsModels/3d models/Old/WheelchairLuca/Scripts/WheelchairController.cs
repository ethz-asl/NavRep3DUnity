using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelchairController : MonoBehaviour {

    public float linear;
    public float angular;

    private Transform leftWheel;
    private Transform RightWheel;
    private Transform Wheels;
    private float K = 0.45f; //0.45f ad hoc
    public Rigidbody rb;
    public Transform tr;
    public float Kp = 500.0f;
    public float Ki = 50.0f;

    private void Start()
    {
        leftWheel = transform.Find("L_Tire");
        RightWheel = transform.Find("R_Tire");
        tr = rb.gameObject.transform;
    }

    void FixedUpdate()
    {
        Vector3 LW_rot = new Vector3((linear + angular) / K, 0, 0);
        leftWheel.Rotate(LW_rot * 180.0f/Mathf.PI * Time.fixedDeltaTime);

        Vector3 RW_rot = new Vector3((linear - angular) / K, 0, 0);
        RightWheel.Rotate(RW_rot * 180.0f/Mathf.PI * Time.fixedDeltaTime);

        Vector3 linearVelocity = new Vector3(0, 0, linear);
        Vector3 angularVelocity = new Vector3(0, angular, 0);
        //transform.parent.position += Wheels.rotation * linearVelocity * Time.fixedDeltaTime;

        //transform.parent.RotateAround(Wheels.position, Wheels.forward , angular * 180.0f/Mathf.PI * Time.fixedDeltaTime);

        rb.AddForce(rb.rotation * linearVelocity * Kp * Time.fixedDeltaTime);
        rb.AddTorque(angularVelocity * Kp * Time.fixedDeltaTime);
    }

    private void Update()
    {
    
    }
}
