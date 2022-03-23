using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{

    public class SimUMANS : ControlSim
    {
        int ConfigId;
        string ConfigFile = "./UMANSFiles/examples/default.xml";

        int nbr_threads = 8;

        UmansLibraryWrapper sim;

        UmansLibraryWrapper.AgentData[] agents;

        int nbr_agents = 0;

        public SimUMANS(int id)
        {
            ConfigId = id;
            sim = new UmansLibraryWrapper();
            sim.StartSimulation_UMANS(ConfigFile, nbr_threads);
        }

        public SimUMANS(int id, string _ConfigFile, int _nbr_threads)
        {
            ConfigId = id;
            ConfigFile = _ConfigFile;
            nbr_threads = _nbr_threads;
            sim.StartSimulation_UMANS(ConfigFile, nbr_threads);
        }
        
        public void addAgent(Vector3 position, TrialControlSim infos)
        {
            UMANSconfig config = (UMANSconfig)infos;
            sim.AddAgent(-position.x, position.z, config.radius, config.prefered_speed, config.max_speed, config.max_acceleration);

            nbr_agents++;
            agents = new UmansLibraryWrapper.AgentData[nbr_agents];
            agents = sim.GetAgentPositions(); 
        }

        public void addNonResponsiveAgent(Vector3 position, float radius)
        {
            sim.AddAgent(-position.x, position.z, radius, 0, 0, 0);       
            
            nbr_agents++;
            agents = new UmansLibraryWrapper.AgentData[nbr_agents];
            agents = sim.GetAgentPositions(); 
        }

        public void addObstacles(Obstacles obst)
        {
            foreach (ObstCylinder pillar in obst.Pillars)
            {
                addNonResponsiveAgent(pillar.position, pillar.radius);
            }

            foreach (ObstWall wall in obst.Walls)
            {
                List<float> poly = new List<float>();

                Vector3 center = (wall.A + wall.B + wall.C + wall.D) / 4;

                if (ObstWall.isClockwise(center, wall.A, wall.B) > 0)
                {
                    // Debug.Log("Clockwise");
                    poly.Add(-wall.A.x);
                    poly.Add(wall.A.z);
                                        
                    poly.Add(-wall.B.x);
                    poly.Add(wall.B.z);
                    
                    poly.Add(-wall.C.x);
                    poly.Add(wall.C.z);   
                                        
                    poly.Add(-wall.D.x);
                    poly.Add(wall.D.z);


                }
                else
                {
                    // Debug.Log("Counter Clockwise");

                    poly.Add(-wall.A.x);
                    poly.Add(wall.A.z); 
                    
                    poly.Add(-wall.C.x);
                    poly.Add(wall.C.z);   


                    poly.Add(-wall.D.x);
                    poly.Add(wall.D.z);
                                        
                    poly.Add(-wall.B.x);
                    poly.Add(wall.B.z);
                    
                }

                sim.AddObstacle(poly.ToArray());
            }
        }

        public void clear()
        {
            sim.CleanUp();
        }

        public void doStep(float deltaTime)
        {
            sim.DoVariableStep(deltaTime);      
            agents = sim.GetAgentPositions();     
        }

        public Vector3 getAgentPos2d(int id)
        {
            return new Vector3(-agents[id].position_x, 0, agents[id].position_y);
        }

        public Vector3 getAgentSpeed2d(int id)
        {
            return new Vector3(-agents[id].velocity_x, 0, agents[id].velocity_y);
        }

        public int getConfigId()
        {
            return ConfigId;
        }

        public void updateAgentState(int id, Vector3 position, Vector3 goal)
        {
            agents[id].position_x = -position.x;
            agents[id].position_y = position.z;

            sim.SetAgentPositions(agents, nbr_agents);
            sim.SetAgentGoal(id, -position.x-goal.x, position.z+goal.z);

        }
    }
}