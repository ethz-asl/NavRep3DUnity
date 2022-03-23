using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace crowdbotsim
{
    public class OdomTcpPublisher : TcpPublisher
    {
        public OdomProvider odom;

        [SerializeField]
        [Tooltip ("Frame id for Tf")]
        public string FrameId = "undefined";
        private string message = "";
        private string sensor_infos = "";
        private OdomProvider.Pose_and_twist pose_And_Twist;
        
        protected override void Start()
        {
            base.Start();
            
            switch (odom.type)
            {
                case OdomProvider.mobile_base_type.differential_drive:
                    sensor_infos = "diff_drive";
                break;
                
                default:
                    sensor_infos = "other";
                break;
            }
        }

        public override string Publish(string data_id, float time)
        {
            string output = "";
            lock (message) {
            string header = concat_to(Topic, '=', data_id);

            // publish infos on first message only
            // if(int.Parse(data_id) == 0)
            // {
            //     header = concat_to(header, '-', sensor_infos);
            // }

            //Debug.Log(message); // WTF, this line seems to stop the message from being cutoff halfway...
            output = concat_to(header, '#', message);
            }
            return output;
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
                string newmessage = "pose";
                
                pose_And_Twist = odom.get_pose_and_twist();

                newmessage = concat_to(newmessage, ' ', pose_And_Twist.x);
                newmessage = concat_to(newmessage, ' ', pose_And_Twist.y);
                newmessage = concat_to(newmessage, ' ', pose_And_Twist.theta);
                newmessage = concat_to(newmessage, ' ', pose_And_Twist.dxdt);
                newmessage = concat_to(newmessage, ' ', pose_And_Twist.dydt);
                newmessage = concat_to(newmessage, ' ', pose_And_Twist.dthetadt);
                newmessage = concat_to(newmessage, ' ', pose_And_Twist.height_above_ground);
                lock (message) {
                    message = newmessage;
                    //Debug.Log(message);
                }

                //Debug.Log("published pose");
                //Debug.Log(pose_And_Twist.y);
        
        }

    }
}
