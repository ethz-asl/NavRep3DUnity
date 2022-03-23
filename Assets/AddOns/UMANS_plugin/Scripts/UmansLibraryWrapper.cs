using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public class UmansLibraryException : Exception
{
    public UmansLibraryException(string message) : base(message) { }
}

public class UmansLibraryWrapper
{ 
    /// <summary>
    /// A struct that contains basic data of an agent at a moment in time. 
    /// Used for communication between Unity and the external simulation library.
    /// </summary>
    public struct AgentData
    {
        public int id;
        public float position_x;
        public float position_y;
        public float velocity_x;
        public float velocity_y;
        public float viewingDirection_x;
        public float viewingDirection_y;
    };

    #region [UMANS API]

    [DllImport("UMANS-Library", EntryPoint = "StartSimulation")]
    static extern bool UMANS_StartSimulation(string filename, int numberOfThreads);

    [DllImport("UMANS-Library", EntryPoint = "DoSimulationSteps")]
    static extern bool UMANS_DoSimulationSteps(int nrSteps);

    [DllImport("UMANS-Library", EntryPoint = "GetSimulationTimeStep")]
    static extern bool UMANS_GetSimulationTimeStep(out float result_dt);

    [DllImport("UMANS-Library", EntryPoint = "SetSimulationTimeStep")]
    static extern bool UMANS_SetSimulationTimeStep(float result_dt);

    [DllImport("UMANS-Library", EntryPoint = "GetAgentPositions")]
    static extern bool UMANS_GetAgentPositions(out IntPtr result_agentData, out int result_nrAgents);

    [DllImport("UMANS-Library", EntryPoint = "SetAgentPositions")]
    static extern bool UMANS_SetAgentPositions(AgentData[] agentData, int nrAgents);

    [DllImport("UMANS-Library", EntryPoint = "AddAgent")]
    static extern bool UMANS_AddAgent(float x, float y, float radius, float prefSpeed, float maxSpeed, float maxAcceleration, int policyID, out int result_id, int desiredID);

    [DllImport("UMANS-Library", EntryPoint = "RemoveAgent")]
    static extern bool UMANS_RemoveAgent(int id);

    [DllImport("UMANS-Library", EntryPoint = "SetAgentGoal")]
    static extern bool UMANS_SetAgentGoal(int id, float x, float y);

    [DllImport("UMANS-Library", EntryPoint = "CleanUp")]
    static extern bool UMANS_CleanUp();


    [DllImport("UMANS-Library", EntryPoint = "AddObstacle")]
    static extern bool UMANS_AddObstacle(float[] points, int nbr_points);

    #endregion [UMANS API]

    public void StartSimulation_UMANS(string configFile, int nrThreads)
    {
        // start an UMANS simulation from a config file
        if (!UMANS_StartSimulation(configFile, nrThreads))
            throw new UmansLibraryException("Failed to start the UMANS simulation.");
    }

    public float GetSimulationTimeStep()
    {
        float dt; bool success;
        success = UMANS_GetSimulationTimeStep(out dt);

        if (dt == 0)
            dt = 0.1f; // safety net

        if (!success)
            throw new UmansLibraryException("Failed to get the simulation time step.");

        return dt;
    }

    public void DoVariableStep(float dt)
    {
        bool success;

        success = UMANS_SetSimulationTimeStep(dt);
    
        if (!success)
            throw new UmansLibraryException("Failed to do set time steps.");
    
        success = UMANS_DoSimulationSteps(1);
        
        if (!success)
            throw new UmansLibraryException("Failed to do variable step.");
    }

    public void DoSimulationSteps(int steps)
    {
        bool success;
        success = UMANS_DoSimulationSteps(steps);
        
        if (!success)
            throw new UmansLibraryException("Failed to do simulation steps.");
    }

    /// <summary>
    /// Retrieves the data of all agents from the UMANS or WASP DLL.
    /// </summary>
    /// <returns>An array of AgentData objects, with one object for each agent in the simulation.</returns>
    public AgentData[] GetAgentPositions()
    {
        // get the status of the simulation
        IntPtr agentDataPtr; int nrAgents; bool success;
        success = UMANS_GetAgentPositions(out agentDataPtr, out nrAgents);

        if (!success)
            throw new UmansLibraryException("Failed to retrieve agent positions from the library.");

        IntPtr p = agentDataPtr;
        AgentData[] data = new AgentData[nrAgents];
        for (int i = 0; i < nrAgents; i++)
        {
            // read the current agent
            data[i] = (AgentData)Marshal.PtrToStructure(p, typeof(AgentData));
            // go to the next agent
            p = new IntPtr(p.ToInt64() + Marshal.SizeOf(typeof(AgentData)));
        }

        return data;
    }

    public void SetAgentPositions(AgentData[] agentData, int nbr_agents)
    {
        bool success;
        success = UMANS_SetAgentPositions(agentData, nbr_agents);
        if (!success)
            throw new UmansLibraryException("Failed to send agent positions to the library.");
    }

    public int AddAgent(float x, float y, float radius, float pref_speed, float maxSpeed, float maxAcceleration)
    {
        int newAgentID; bool success;
        success = UMANS_AddAgent(x, y, radius, pref_speed, maxSpeed, maxAcceleration, 0, out newAgentID, -1);

        if (!success)
            throw new UmansLibraryException("Failed to add an agent to the library.");

        return newAgentID;
    }

    public void RemoveAgent(int agentID)
    {
        bool success;
        success = UMANS_RemoveAgent(agentID);

        if (!success)
            throw new UmansLibraryException("Failed to remove agent " + agentID + " from the library.");
    }

    public void SetAgentGoal(int agentID, float x, float y)
    {
        bool success;
        success = UMANS_SetAgentGoal(agentID, x, y);
        
        if (!success)
            throw new UmansLibraryException("Failed to set the goal of agent " + agentID + " in the library.");
    }

    public void CleanUp()
    {
        bool success;
        success = UMANS_CleanUp();

        if (!success)
            throw new UmansLibraryException("Failed to clean up the simulation library.");
    }

    public void AddObstacle(float[] points)
    {
        bool success;
        success = UMANS_AddObstacle(points, points.Length/2);

        if (!success)
            throw new UmansLibraryException("Failed to add an obstacle to the library.");
    }
}
