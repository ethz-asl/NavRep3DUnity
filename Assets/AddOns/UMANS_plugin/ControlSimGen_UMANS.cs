using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlSimGen_UMANS : ControlSimGen
    {
        [Header("Main Parameters")]
        public int id = 0;
        public float radius = 0.33f;
        public float prefered_speed = 1.4f;
        public float max_speed = 3.0f;
        public float max_acceleration = float.MaxValue;

        [System.Serializable]
        public class SpawnerParams
        {
            public float radius_rand;
            public float prefered_speed_rand;
            public float max_speed_rand;
            public float max_acceleration_rand;
        }

        [Header("Spawner Parameters")]
        public SpawnerParams randomness;

        public override CustomXmlSerializer<TrialControlSim> createControlSim(int GroupSeed)
        {
            UMANSconfig sim = new UMANSconfig();
            sim.radius = radius;
            sim.prefered_speed = prefered_speed;
            sim.max_speed = max_speed;
            sim.max_acceleration = max_acceleration;

            return sim;
        }

        public override ControlSimGen randDraw(GameObject agent, int id = 0, int groupID = 0)
        {
            ControlSimGen_UMANS csg = agent.AddComponent<ControlSimGen_UMANS>();

            csg.id = id;
            csg.radius = radius + Random.Range(-randomness.radius_rand, randomness.radius_rand);
            csg.max_speed = max_speed + Random.Range(-randomness.max_speed_rand, randomness.max_speed_rand);
            csg.prefered_speed = prefered_speed + Random.Range(-randomness.prefered_speed_rand, randomness.prefered_speed_rand);
            csg.max_acceleration = max_acceleration + Random.Range(-randomness.max_acceleration_rand, randomness.max_acceleration_rand);          
            
            return csg;
        }
    }
}
