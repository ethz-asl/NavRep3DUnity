using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CrowdMP.Core;

namespace crowdbotsim
{
    public class TrialInfoTcpPublisher : TcpPublisher
    {

        private string message;
        private CrowdBotSim_MainManager mainManager;

        protected override void Start()
        {
            base.Start();
            mainManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CrowdBotSim_MainManager>();
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
            // UpdateMessage();
        }

        public void UpdateMessage()
        {
            message = "";
            try {
                message = mainManager.currentTrialName;
            } catch (System.Exception e) {
                Debug.LogError (e);
            }
        }

    }
}