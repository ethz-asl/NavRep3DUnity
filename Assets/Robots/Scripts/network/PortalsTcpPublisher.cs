
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CrowdMP.Core;

namespace crowdbotsim
{
    public class PortalsTcpPublisher : TcpPublisher
    {
        private string message;
        private CrowdBotSim_TrialManager trialManager;
        protected override void Start()
        {
            trialManager = GameObject.FindGameObjectWithTag("Stage").GetComponent<CrowdBotSim_TrialManager>();
            base.Start();
        }

        public override string Publish(string data_id, float time)
        {
            string header = concat_to(Topic, '=', data_id);
            return concat_to(header, '#', message);
        }

        public override void UpdateMessageValue()
        {
            UpdateMessage();
        }

        private void LateUpdate()
        {
            UpdateMessage();
            if(!trialManager.gameObject.activeInHierarchy)
                this.Start();
        }

        public void UpdateMessage()
        {
            if(ToolsTime.DeltaTime > 0)
            {
                message = "vertxy";

                Obstacles portals = trialManager.get_portals_list();
                foreach (var wall in portals.Walls)
                {
                    message = concat_to(message, ' ',
                      wall.A.x.ToString("F3") + ' ' + wall.A.z.ToString("F3") + ' '
                    + wall.B.x.ToString("F3") + ' ' + wall.B.z.ToString("F3") + ' '
                    + wall.C.x.ToString("F3") + ' ' + wall.C.z.ToString("F3") + ' '
                    + wall.D.x.ToString("F3") + ' ' + wall.D.z.ToString("F3"));
                }
            }
        }

    }
}