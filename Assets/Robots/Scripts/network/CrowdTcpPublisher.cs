
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace crowdbotsim
{
    public class CrowdTcpPublisher : TcpPublisher
    {

        [SerializeField]
        [Tooltip ("Frame id for Tf")]
        public string FrameId = "undefined";
        private string message = "";
        private string sensor_infos = "";
        
        private CrowdBotSim_TrialManager trialManager;
        protected override void Start()
        {
            trialManager = GameObject.FindGameObjectWithTag("Stage").GetComponent<CrowdBotSim_TrialManager>();

            base.Start();

            sensor_infos = trialManager.get_agents_list().Count.ToString();

        }

        public override string Publish(string data_id, float time)
        {
            string header = concat_to(Topic, '=', data_id);

            // publish infos on first message only
            // if(int.Parse(data_id) == 0)
            // {
            //     header = concat_to(header, '-', sensor_infos);
            // }

            return concat_to(header, '#', message);
        }

        public override void UpdateMessageValue()
        {
            UpdateMessage();
        }

        private void LateUpdate()
        {
            //UpdateMessage();
            if(!trialManager.gameObject.activeInHierarchy)
                this.Start();
        }

        public void UpdateMessage()
        {

            List<Agent> agents = trialManager.get_agents_list();
            lock (message) {
                message = "id pose";
                foreach (var agent in agents)
                {
                    message = concat_to(message, ' ', agent.GetID().ToString());
                    message = concat_to(message, ' ', '(' + agent.Position.x.ToString("F3") + ' ' + agent.Position.z.ToString("F3") + ' ' + agent.Rotation.eulerAngles.y.ToString("F3") + ')');
                }
            }
        }

    }
}