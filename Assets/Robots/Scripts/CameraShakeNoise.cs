using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeNoise : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject robot; // The root element of the robot game object, which moves w.r.t. the world.
    public float maxPitch = 45.0f; // Maximum pitch of the camera.
    public float maxYaw = 30.0f; // Maximum yaw of the camera.
    public float noiseScale = 1.0f; // 0 - no accumulated noise, 1 - max accumulated noise
    public float cameraResetsEveryNSecs = 60.0f; // How often the camera resets to its original position.
    private Vector3 lastRobotPosInWorld;
    private Vector3 lastRobotVelInWorld;
    private Vector3 initialCameraPosInParent;
    private Quaternion initialCameraRotInParent;
    private float accumulatedPitchDrift = 0;
    private float accumulatedYawDrift = 0;
    private float lastSimTime;
    void Start()
    {
        lastSimTime = ToolsTime.TrialTime;
        lastRobotPosInWorld = robot.transform.position;
        initialCameraPosInParent = this.transform.localPosition;
        initialCameraRotInParent = this.transform.localRotation;
        lastRobotVelInWorld = new Vector3(0.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Get robot acceleration in world
        float time = ToolsTime.TrialTime;
        float dt = time - lastSimTime;
        if (dt < 0.0f) // if time goes backwards, ignore and reset
        {
            lastSimTime = time;
            lastRobotPosInWorld = robot.transform.position;
            lastRobotVelInWorld = new Vector3(0.0f, 0.0f, 0.0f);
            accumulatedPitchDrift = 0;
            accumulatedYawDrift = 0;
            return;
        }
        dt = Mathf.Clamp(dt, 0.0f, 1.0f);
        if (dt == 0.0f)
        {
            return;
        }
        Vector3 robotVelInWorld = (robot.transform.position - lastRobotPosInWorld) / dt;
        Vector3 robotAccelInWorld = (robotVelInWorld - lastRobotVelInWorld) / dt;
        lastSimTime = time;
        lastRobotPosInWorld = robot.transform.position;
        lastRobotVelInWorld = robotVelInWorld;

        // convert acceleration from world frame to parent frame
        Vector3 robotAccelInParent = this.transform.parent.InverseTransformVector(robotAccelInWorld);

        // Convert acceleration to angular strain
        // If accelerating forward pitch up, if backward pitch down
        // if accelerating left roll left, if accelerating right roll right
        float pitch = robotAccelInParent.z; // z is camera axis
        float roll = robotAccelInParent.x;

        // Add random noise to pitch drift
        accumulatedPitchDrift += Mathf.Abs(pitch) * (Random.value - 0.5f) * noiseScale;
        accumulatedYawDrift += Mathf.Abs(roll) * (Random.value - 0.5f) * noiseScale;
        accumulatedPitchDrift = Mathf.Clamp(accumulatedPitchDrift, -maxPitch, maxPitch);
        accumulatedYawDrift = Mathf.Clamp(accumulatedYawDrift, -maxYaw, maxYaw);
        
        float oddsOfNoResetPerSecond = Mathf.Max(0.0f, cameraResetsEveryNSecs - 1.0f) / cameraResetsEveryNSecs; // on average once per minute
        float oddsOfNoReset = Mathf.Pow(oddsOfNoResetPerSecond, dt);
        if (Random.value > oddsOfNoReset)
        {
            accumulatedPitchDrift = 0;
            accumulatedYawDrift = 0;
        }

        Quaternion inertialRotation = Quaternion.Euler(pitch + accumulatedPitchDrift,
                                                       0f,
                                                       roll + accumulatedYawDrift);
        this.transform.localRotation = initialCameraRotInParent * inertialRotation;
    }
}
