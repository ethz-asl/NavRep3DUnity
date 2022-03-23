using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CrowdMP.Core
{

    public class UMANSconfig : TrialControlSim
    {
        [XmlAttribute("SimulationID")]
        public int id;

        [XmlAttribute]
        public float radius;
        
        [XmlAttribute]
        public float prefered_speed;
        
        [XmlAttribute]
        public float max_speed;

        [XmlAttribute]
        public float max_acceleration;

        public int getConfigId()
        {
            return id;
        }

        public ControlSim createControlSim(int id)
        {
            return new SimUMANS(id);
        }

        public UMANSconfig()
        {
            id = 0;
            radius = 0.3f;
            prefered_speed = 1.4f;
            max_speed = 3.0f;
            max_acceleration = float.MaxValue;
        }

    }
}