using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CrowdMP.Core;

namespace crowdbotsim
{
    public class GoalTcpPublisher : TcpPublisher
    {

        private string message;

        protected override void Start()
        {
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
            //if(!trialManager.gameObject.activeInHierarchy)
            //    this.Start();
        }

        public void UpdateMessage()
        {
            GameObject[] my_goals = GameObject.FindGameObjectsWithTag("Goal");
            Vector3 p = my_goals[0].transform.position;
            message = p.x.ToString("F3") + ' ' + p.z.ToString("F3") + ' ' + p.y.ToString("F3");
        }

    }
}