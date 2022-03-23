using UnityEngine;
using System.Globalization;

namespace crowdbotsim
{
    public class TcpClockSubscriber : TcpSubscriber
    {

        [HideInInspector] public float deltaTime = 0; // This gets checked by ToolsTime, which in turn gets checked by TrialManager.doStep() to initiate a simulation step
        private float lastTime = 0;

        private uint secs = 0;
        private uint nsecs = 0;

        private float currentTime;

        private bool first_message = true;

        public float DeltaTime { get { 
            float dt = Mathf.Max(0.0f, deltaTime);
            deltaTime = 0;
            return dt;
        }}
        public float Time { get { return currentTime; }}

		protected override void Start()
		{
			base.Start();
		}
		
        protected override void ReceiveMessage(string message)
        {
            // secs = message.clock.secs;
            // nsecs = message.clock.nsecs;
            currentTime = float.Parse(message, CultureInfo.InvariantCulture.NumberFormat);
            deltaTime = first_message ? 0 : currentTime - lastTime;
            // Debug.Log("deltaTime updated");
            first_message = false;
            lastTime = currentTime; 
        }
    }

}
