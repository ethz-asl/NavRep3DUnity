
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;
using RosSharp.RosBridgeClient;
using System.Linq;

namespace crowdbotsim
{

/// <summary>
/// Regular trial manager with moving agents/player
/// </summary>
public class CrowdBotSim_TrialManager : MonoBehaviour, TrialManager {

    protected Player player;
    protected List<Robot> robots;
    protected List<Agent> agentsList;
    protected Obstacles obstaclesList;
    // protected Recorder rec;
    protected SimManager sims;

    List<RosConnector> ROS;
    List<Publisher> Ros_Publishers;

    List<TcpConnector> Tcp;
    List<TcpPublisher> TcpPublishers;
    


    private CrowdStampedPublisher crowdpub;

    private bool player_in_sim;
    // protected ToolsCamRecord camRecord;
    private List<bool> robot_in_sim;

    public GameObject obstaclesContainer;
    public GameObject portalsContainer;

    public Vector3 ToricWorldDimensions = new Vector3(1000, 1000, 1000);

    public bool VERBOSE = false;

    /// <summary>
    /// Reset trial
    /// </summary>
    public virtual void clear()
    {
        /* Cleaning old virtual humans */
        foreach (Agent currentAgent in agentsList)
        {
            //GameObject.DestroyImmediate(currentAgent.gameObject); // Dangerous function but mandatory since the GraphicPlayer can't wait the end of the current frame to store the activehumans (Start function) at the beginning of a level, so using Destroy instead will make it store old virtualhumans from the previous level that will be clean at the end of the frame.
            GameObject.Destroy(currentAgent.gameObject);
        }
        agentsList.Clear();

        sims.clear();
        player.clear();
        robots.Clear();

        foreach(MonoBehaviour p in Ros_Publishers) p.enabled = false;

        foreach(MonoBehaviour p in TcpPublishers) p.enabled = false;


        foreach(RosConnector r in ROS)
        {
            r.enabled = false;
            r.start_scripts = true;
        }

        foreach(TcpConnector t in Tcp)
        {
            t.enabled = false;
            t.start_scripts = true;
        }

        robot_in_sim.Clear();

        GameObject[] my_robots =  GameObject.FindGameObjectsWithTag("Robot");
        foreach(GameObject r in my_robots)
        {
            r.transform.Find("base_link").localPosition = new Vector3(0,0,0);
            r.transform.Find("base_link").localRotation = Quaternion.identity;

            DestroyImmediate(r.GetComponent<RegularRobot>());
            DestroyImmediate(r.GetComponent<UnityEngine.AI.NavMeshAgent>());
        }
        //player.gameObject.SetActive(false);
        // Destroy(player);
        // if (camRecord != null)
        // {
        //     Destroy(camRecord);
        //     camRecord = null;
        // }
        // if (rec != null)
        //     rec.clear();

    }

    public List<Agent> get_agents_list()
    {
        if(agentsList == null)
            agentsList = new List<Agent>();
        return agentsList;
    }

    public Obstacles get_obstacles_list()
    {
        if(obstaclesList == null)
            obstaclesList = new Obstacles();
        return obstaclesList;
    }

    public Obstacles get_portals_list()
    {
        Obstacles portalsList = new Obstacles();
        if (portalsContainer == null)
            return portalsList;
        foreach (Transform item in portalsContainer.transform)
            portalsList.addWall(item.gameObject);
        return portalsList;
    }

    private void lookForObst(Transform container, ref Obstacles obst, int n_random_obstacles_left_to_initialize, int recursion_depth)
    {
        // This is a recursive method. It parses the object tree below the container transform, looking for obstacles, initializes them as required,
        // then adds them to the obstacle list used by the simulator for avoidance.
        // Debug.Log(LoaderConfig.obstaclesInfo.Count);
        float ARENA_SIZE = 14.0f;
        float BORDER_OFFSET = 1.0f + 0.5f; // 1m from original navrep sim + half wall thickness
        // arena bounds (initialed to avoid having the bounds at 0 if no obstacles)
        float xmin, xmax, zmin, zmax;
        xmin = -ARENA_SIZE / 2.0f - BORDER_OFFSET;
        xmax = ARENA_SIZE / 2.0f + BORDER_OFFSET;
        zmin = -ARENA_SIZE / 4.0f - BORDER_OFFSET;
        zmax = ARENA_SIZE / 4.0f + BORDER_OFFSET;
        List<Agent> agents = get_agents_list();
        foreach (Agent agent in agents)
        {
            xmin = Mathf.Min(agent.Position.x - BORDER_OFFSET, xmin);
            xmax = Mathf.Max(agent.Position.x + BORDER_OFFSET, xmax);
            zmin = Mathf.Min(agent.Position.z - BORDER_OFFSET, zmin);
            zmax = Mathf.Max(agent.Position.z + BORDER_OFFSET, zmax);
            xmin = Mathf.Min(-agent.Position.x - BORDER_OFFSET, xmin);
            xmax = Mathf.Max(-agent.Position.x + BORDER_OFFSET, xmax);
            zmin = Mathf.Min(-agent.Position.z - BORDER_OFFSET, zmin);
            zmax = Mathf.Max(-agent.Position.z + BORDER_OFFSET, zmax);
        }
        // Look for random obstacles to initialize, initialize them
        foreach (Transform item in container.transform)
        {
            if (item.tag == "RandomPolygon") {
                item.gameObject.SetActive(false);
                if (n_random_obstacles_left_to_initialize > 0) {
                    float x, z;
                    float width, depth;
                    n_random_obstacles_left_to_initialize -= 1;
                    item.gameObject.SetActive(true);
                    while (true) {  
                        // this random generation is a bit contrived, to be similar to the SOADRL simulator
                        x = ARENA_SIZE * (Random.value - 0.5f);
                        z = ARENA_SIZE * (Random.value - 0.5f);

                        if (Random.value > 0.5f) {
                            // square
                            width = (Random.value + 0.5f) * 1.4f;
                            depth = width;
                        } else {
                            // rectangle
                            if (Random.value > 0.5f) {
                                width = 1.0f;
                                depth = 2.0f + Mathf.Round(Random.value);
                            } else {
                                width = 2.0f + Mathf.Round(Random.value);
                                depth = 1.0f;
                            }
                        }
                        if (!checkObstacleInitialCollisions(x, z, width, depth, agents))
                            break;
                    }
                    //
                    xmin = Mathf.Min(x - width / 2.0f - BORDER_OFFSET, xmin);
                    xmax = Mathf.Max(x + width / 2.0f + BORDER_OFFSET, xmax);
                    zmin = Mathf.Min(z - depth / 2.0f - BORDER_OFFSET, zmin);
                    zmax = Mathf.Max(z + depth / 2.0f + BORDER_OFFSET, zmax);
                    item.transform.position = new Vector3(x, 0.0f, z);
                    item.transform.localScale = new Vector3(width, 3.0f, depth);
                    obst.addWall(item.gameObject);
                }
            }
        }
        
        // Now we look for walls in the scene hierarchy. If they are found, we move
        // them to form a fence around the random obstacles
        // Daniel: i'm not sure what the intention is with the pillars, I've left that code as is.
        foreach (Transform item in container.transform)
        {
            if (item.tag == "NavRepEnclosingWall") {
                // move the walls back out to their starting position
                item.position = new Vector3((item.position.x != 0 ? Mathf.Sign(item.position.x) * 25.0f : 0),
                                            item.position.y,
                                            (item.position.z != 0 ? Mathf.Sign(item.position.z) * 25.0f : 0));
                // move the walls to "hug" the outermost obstacles
                item.position = new Vector3(Mathf.Clamp(item.position.x, xmin, xmax),
                                            item.position.y,
                                            Mathf.Clamp(item.position.z, zmin, zmax));
                obst.addWall(item.gameObject);
            }
            else if (item.tag == "Wall")
                obst.addWall(item.gameObject);
            else if (item.tag == "WallRobotOnly") {
                obst.addRobotOnlyWall(item.gameObject);
            }
            else if (item.tag == "Pillar")
                obst.addPillar(item.gameObject);
            else if (item.tag == "FixedObstacle")
                obst.addWall(item.gameObject);
            else
                lookForObst(item, ref obst, n_random_obstacles_left_to_initialize, recursion_depth+1);
        }
        if (recursion_depth == 0)
        {
            if (n_random_obstacles_left_to_initialize > 0) {
                Debug.Log("Config requests random obstacles to be initialized, but not enough random obstacle GameObjects found in scene hierarchy.");
                Debug.Log(LoaderConfig.obstaclesInfo.Count);
                Debug.Log(n_random_obstacles_left_to_initialize);
            }
        }
    }

    private bool checkAgentInitialCollisions(Agent a)
    {
        return false;
    }

    private bool checkObstacleInitialCollisions(float x, float z, float width, float depth, List<Agent> agents)
    {
        // TODO get actual robot, goal pos
        //foreach( TrialRobot trial_r in LoaderConfig.robotsInfo ) {} 
        float ROBOT_X = 7;
        float GOAL_X = -7;
        float ROBOT_Z = 0;
        float GOAL_Z = 0;
        float ROBOT_R = 0.3f;
        float GOAL_R = 0.5f;
        float PAD_Z = 1.0f;
        float PAD_X = 3.0f;
        float xmin = x - width / 2.0f;
        float xmax = x + width / 2.0f;
        float zmin = z - depth / 2.0f;
        float zmax = z + depth / 2.0f;
        bool collides_with_robot_agent_or_goal = false;
        if (
            ROBOT_X > (xmin - ROBOT_R - PAD_X) && ROBOT_X < (xmax + ROBOT_R + PAD_X) &&
            ROBOT_Z > (zmin - ROBOT_R - PAD_Z) && ROBOT_Z < (zmax + ROBOT_R + PAD_Z)
            )
            collides_with_robot_agent_or_goal = true;
        if (
            GOAL_X > (xmin - GOAL_R - PAD_X) && GOAL_X < (xmax + GOAL_R + PAD_X) &&
            GOAL_Z > (zmin - GOAL_R - PAD_Z) && GOAL_Z < (zmax + GOAL_R + PAD_Z)
            )
            collides_with_robot_agent_or_goal = true;
        foreach (var agent in agents) {
            float agent_x = agent.Position.x;
            float agent_z = agent.Position.z;
            float AGENT_R = 0.3f;
            if (
                agent_x > (xmin - AGENT_R - PAD_X) && agent_x < (xmax + AGENT_R + PAD_X) &&
                agent_z > (zmin - AGENT_R - PAD_Z) && agent_z < (zmax + AGENT_R + PAD_Z)
                )
                collides_with_robot_agent_or_goal = true;

        }
        return collides_with_robot_agent_or_goal;        
    }

    /// <summary>
    /// Initialize trial
    /// </summary>
    /// <param name="p">The player</param>
    /// <param name="agentModels">All the available agent models</param>
    /// <returns>True if the trial has been correctly intialized</returns>
    public virtual bool initializeTrial(GameObject[] playersModel, GameObject[] agentModels)
    {
        

        // Setup simulations for collision avoidance
        if(robots == null)
            robots = new List<Robot>();
        if (agentsList == null)
            agentsList = new List<Agent>();
        if (sims==null)
            sims = new SimManager();

        if(Ros_Publishers == null)
            Ros_Publishers = new List<Publisher>();
        if(ROS == null)
            ROS = new List<RosConnector>();

       //if(Tcp == null)
            Tcp = new List<TcpConnector>();
        if(TcpPublishers == null)
            TcpPublishers = new List<TcpPublisher>();

        if(robot_in_sim == null)
            robot_in_sim = new List<bool>();

        // Setup the player
        foreach (GameObject p in playersModel)
        {

            if (p.name == LoaderConfig.playerInfo.mesh)
            {
                player = LoaderConfig.playerInfo.createPlayerComponnent(p, 0);
                p.SetActive(true);
            } else
            {
                p.SetActive(false);
            }
        }

        if(player.GetType() == typeof(CamPlayer))
        {
            player_in_sim = ((CamPlayer)player).in_sim;
        }
        else
        {
            player_in_sim = true;
        }


        GameObject[] my_robots =  GameObject.FindGameObjectsWithTag("Robot");
        foreach(GameObject r in my_robots)
        {
            r.SetActive(false);
        }

        // Create each agents from the trial.xml file
        uint i = 0;

        GameObject[] my_goals = GameObject.FindGameObjectsWithTag("Goal");
        foreach( TrialRobot trial_r in LoaderConfig.robotsInfo )
        {
            ++i;
            foreach(GameObject r in my_robots)
            {
                if(r.name == trial_r.mesh)
                {
                    robots.Add(trial_r.createRobotComponnent(r,i));
                    r.SetActive(true);
                    robot_in_sim.Add( ((TrialRegularRobot)trial_r).in_sim );
                    if ((i-1) < my_goals.Length) {
                        my_goals[i-1].transform.position = ((TrialRegularRobot)trial_r).Goal.vect;
                    }
                }
            }
        }

        foreach (TrialAgent a in LoaderConfig.agentsInfo)
        {
            ++i;
            GameObject currentAgentGameObject = null;

            foreach (GameObject currentAgentInModel in agentModels)
            {

                if (currentAgentInModel.name == a.mesh)
                {
                    currentAgentGameObject = (GameObject)GameObject.Instantiate(currentAgentInModel);
                    currentAgentGameObject.name = i.ToString() + " - " + currentAgentGameObject.name;

                    break;
                }
            }

            if (currentAgentGameObject == null)
            {
                ToolsDebug.logFatalError("Error, unknown mesh " + a.mesh);
                Application.Quit();
            }
            currentAgentGameObject.SetActive(true);
            Agent currentAgent = a.createAgentComponnent(currentAgentGameObject, i);
            currentAgentGameObject.tag = "VirtualHumanActive";
            agentsList.Add(currentAgent);
        }

        // Compute obstacles list
        obstaclesList = new Obstacles();
        int n_random_obstacles_left_to_initialize = LoaderConfig.obstaclesInfo.Count;
        if (obstaclesContainer != null)
            lookForObst(obstaclesContainer.transform, ref obstaclesList, n_random_obstacles_left_to_initialize, 0);
        sims.initSimulations(obstaclesList);

        foreach(GameObject r in my_robots)
        {
            if(r.activeSelf)
            {
                try
                {
                    ROS.Add(r.transform.Find("RosConnector").gameObject.GetComponent<RosConnector>());
                }
                catch (System.Exception)
                {
                }
                try
                {
                    Tcp.Add(r.transform.Find("TcpConnector").gameObject.GetComponent<TcpConnector>());
                }
                catch (System.Exception)
                {
                }
            }
        }

        try
        {
            ROS.Add(GameObject.Find("RosConnector").gameObject.GetComponent<RosConnector>());
        }
        catch (System.Exception)
        {
        }
        try
        {
            Tcp.Add(GameObject.Find("TcpConnector").gameObject.GetComponent<TcpConnector>());
        }
        catch (System.Exception)
        {
        }

        foreach(RosConnector RosCon in ROS)
        {
            Ros_Publishers.AddRange(new List<Publisher>(RosCon.GetComponents<Publisher>()));
            foreach(Publisher script in Ros_Publishers)
            {
                if(script.GetType() == typeof(TwistArrayStampedPublisher) )
                {
                    ((TwistArrayStampedPublisher)script).agents = new List<Agent>(agentsList);
                }
                if(script.GetType() == typeof(CrowdStampedPublisher) )
                {
                    crowdpub = (CrowdStampedPublisher)script;
                    crowdpub.agents = new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent[agentsList.Count];
                    for(int j = 0; j < agentsList.Count; j++)
                    {
                        crowdpub.agents[j] = 
                            new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent(
                                (int)(agentsList[j].id), agentsList[j].Position, 
                                Vector3.zero, Vector3.zero, agentsList[j].Position);
                    }
                }
            }

            RosCon.enabled = true;
            RosCon.start_scripts = true;
        }

        foreach(TcpConnector TcpCon in Tcp)
        {
            TcpPublishers.AddRange(new List<TcpPublisher>(TcpCon.GetComponents<TcpPublisher>()));
            foreach(TcpPublisher script in TcpPublishers)
            {
                // TODO : no need TODO anymore thanks to CrowdTcpPublisher

                // if(script.GetType() == typeof(TwistArrayStampedPublisher) )
                // {
                //     ((TwistArrayStampedPublisher)script).agents = new List<Agent>(agentsList);
                // }
                // if(script.GetType() == typeof(CrowdStampedPublisher) )
                // {
                //     crowdpub = (CrowdStampedPublisher)script;
                //     crowdpub.agents = new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent[agentsList.Count];
                //     for(int j = 0; j < agentsList.Count; j++)
                //     {
                //         crowdpub.agents[j] = 
                //             new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent(
                //                 (int)(agentsList[j].id), agentsList[j].Position, 
                //                 Vector3.zero, Vector3.zero, agentsList[j].Position);
                //     }
                // }
            }

            TcpCon.enabled = true;
            TcpCon.start_scripts = true;
            TcpCon.new_step_executed = true;
        }
        

        
        // Set the list of agent to watch in the recorder
        // rec=gameObject.GetComponent<Recorder>();
        // if (rec!=null)
        //     rec.initRecorder(player, agentsList);

        // TrialScreenRecorder screenRecorderInfos = LoaderConfig.screenRecorder;
        // if (screenRecorderInfos!=null)
        // {
        //     camRecord=Camera.main.gameObject.AddComponent<ToolsCamRecord>();
        //     camRecord.record = screenRecorderInfos.record;
        //     camRecord.timeToStart = screenRecorderInfos.timeToStart;               
        //     camRecord.timeToStop = screenRecorderInfos.timeToStop;               
        //     camRecord.framerate = screenRecorderInfos.framerate;                  
        //     camRecord.saveDir = screenRecorderInfos.saveDir;     
        // }

        return true;
    }

        
    /// <summary>
    /// Check the ending conditions of the trials
    /// </summary>
    /// <returns>True if the trial is over</returns>
    public virtual bool hasEnded()
    {
        bool isEnd = false;
        foreach (TrialEnding condition in LoaderConfig.sceneEndings)
        {
            float currentValue=0;
            switch (condition.parameter)
            {
                case TrialParam.time:
                    currentValue = ToolsTime.TrialTime;
                    break;
                case TrialParam.x:
                    currentValue = robots[0].transform.position.x;
                    break;
                case TrialParam.y:
                    currentValue = robots[0].transform.position.z;
                    break;
            }

            switch (condition.test)
            {
                case TrialTest.greater:
                    isEnd = isEnd || currentValue > condition.value;
                    break;
                case TrialTest.less:
                    isEnd = isEnd || currentValue < condition.value;
                    break;
            }
        }

        return isEnd;
    }

    /// <summary>
    /// Perform a step of the trial
    /// </summary>
    public virtual void doStep()
    {
        if(!player_in_sim)
            player.doStep();

        foreach(TcpConnector TcpCon in Tcp)
        {
            if (!TcpCon.new_step_required)
                return;
        }    

        if (VERBOSE) Debug.Log("TM: executing subscriber commands");

        foreach(TcpConnector TcpCon in Tcp)
        {
            TcpCon.ExecuteSubscriberCommands();
            TcpCon.new_step_executed = false;
        }    

        if (VERBOSE) Debug.Log("TM: starting sim step");

        if (ToolsTime.DeltaTime > 0)
        {

            
                
            List<Vector3> currPos = new List<Vector3>();

            // Do regular step
            if(player_in_sim)
            {
                currPos.Add(player.Position);
                player.doStep();
            }

            int robot_index = 0;
            foreach (Robot r in robots)
            {
                if(robot_in_sim[robot_index])
                {
                    Vector3 pos = r.transform.Find("base_link").position;
                    currPos.Add(pos);
                }
                r.doStep();
            }

            foreach (Agent a in agentsList)
            {
                currPos.Add(a.Position);
                a.doStep();
            }

            int start_index = 0; 
            if(player_in_sim) start_index++; //player
            if(robots != null)
            {

                start_index += robot_in_sim.Where(b => b==true).Count();
            }

            if( crowdpub != null)
            {
                if(currPos.Count - start_index > 0) crowdpub.UpdateAgents(currPos.GetRange(start_index, currPos.Count-start_index), agentsList);
            }

            sims.doStep(ToolsTime.DeltaTime, currPos, player, agentsList, robots);
            
            int i = start_index;
            foreach(Agent a in agentsList)
            {
                Vector3 posAgent = new Vector3( Mathf.Repeat(a.Position.x + ToricWorldDimensions.x/2 , ToricWorldDimensions.x) - ToricWorldDimensions.x/2,
                    a.Position.y,
                    Mathf.Repeat(a.Position.z + ToricWorldDimensions.z/2 , ToricWorldDimensions.z) - ToricWorldDimensions.z/2);
                
                a.transform.position = posAgent;
                i++;
            }

            
        }

        if (VERBOSE) Debug.Log("TM: step complete, going to update tcpcon values");

        foreach(TcpConnector TcpCon in Tcp)
            {
                TcpCon.UpdateMessageValues();
                TcpCon.new_step_executed = true;
            }

        foreach(Publisher script in Ros_Publishers)
        {
            if(((MonoBehaviour)script).enabled == true)
                script.UpdateMessage();
        }

    }

    // Use this for initialization
    void Start () {
        if (agentsList==null)
            agentsList = new List<Agent>();
    }

    // Update is called once per frame
    void Update ()
    {

    }

    public Player getPlayer()
    {
        return player;

    }

    public virtual bool isReady()
    {
        return true;
    }
    public virtual void startTrial()
    {


    }


}
}
