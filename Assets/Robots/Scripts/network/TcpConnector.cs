using System;
using System.Threading;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine.Profiling;

namespace crowdbotsim
{
    public class TcpConnector : MonoBehaviour
    {
        public enum Protocols {TCP};
        public Protocols Protocol;
        private ManualResetEvent isConnected = new ManualResetEvent(false);
        private MonoBehaviour[] scripts; 
        public bool VERBOSE = false;

        [HideInInspector]
        public bool start_scripts = true;
        [HideInInspector] public bool new_step_required = false; // this flag allows the trial manager to 'tell' the Trial manager to execute a new step
        [HideInInspector] public bool new_step_executed = true; // this flag allows the trial manager to 'tell' the connector that a new step has been completed

        //TCP attributes
        [Header("Sockets basic attributes")]
        public string connectionIP = "127.0.0.1";
        private int connectionPort = 25001;
        IPAddress localAdd;
        TcpListener listener;
        TcpClient client;
        private bool running;
        private bool quit = false;

        private Dictionary<string, Delegate> SubscriberHandlers;
        private List<TcpPublisher> Publishers;

        private int data_counter = 0;

        public void Awake()
        {
            SubscriberHandlers = new Dictionary<string, Delegate>();
            Publishers = new List<TcpPublisher>();
            var main_thread = new Thread(ConnectAndWait);
            // main_thread.Priority = System.Threading.ThreadPriority.Highest;
            main_thread.Start();
        }

        private void ConnectAndWait()
        {
            if(Protocol == Protocols.TCP)
            {
                GetInfo();
            }
            
        }
        
        private void Update()
        {
            if(quit) ToolsDebug.Quit();

            if(start_scripts)
            {
                scripts = GetComponents<MonoBehaviour>();
                foreach( MonoBehaviour script in scripts )
                {
                    script.enabled = true;
                }
                start_scripts = false;
            }
        }

        // Helper function for getting the command line arguments
        private static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
        void GetInfo()
        {
            var port_arg = GetArg("-port");
            if (port_arg != null) {
                connectionPort = int.Parse(port_arg);
            }
            Debug.Log("TcpConnector listening on port " + connectionPort.ToString());
            localAdd = IPAddress.Parse(connectionIP);
            listener = new TcpListener(IPAddress.Any, connectionPort);
            listener.Start();

            client = listener.AcceptTcpClient();
    
            running = true;

            Debug.Log("Connected to python client");

            while (running)
            {
                TCPConnection();
            }
            listener.Stop();
            quit = true;
        }

        void TCPConnection()
        {
            Profiler.BeginSample("TCPConnection");
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
            string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (dataReceived != null)
            {
                if (dataReceived == "stop")
                {
                    Debug.Log("Stop TCP server & CrowdBotSim");
                    byte[] buffer_out = Encoding.UTF8.GetBytes(dataReceived+"@");
                    nwStream.Write(buffer_out, 0, buffer_out.Length);
                    running = false;
                }
                else
                {
                    List<string> data_out = new List<string>();

                    if(dataReceived != "")
                    {

                        if (VERBOSE) Debug.Log("---------- Con: data received -----------");
                        
                        // find the topic of the clock subscriber (used to make sure it gets called last)
                        string clock_topic = "clock";
                        foreach( MonoBehaviour script in scripts ) {
                            if(script.GetType() == typeof(TcpClockSubscriber) )
                            {
                                clock_topic = ((TcpClockSubscriber)script).Topic;
                            }
                        }

                        // put the string of incoming data into dictionnary if the incoming data are in the form of key1=value1;key2=value2 
                        //(key and value are stored as string)
                        var incoming_data = 
                        dataReceived.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(part => part.Split('=')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

                        foreach( KeyValuePair<string, Delegate> sub in SubscriberHandlers )
                        {
                            if (sub.Key == clock_topic) { // See next block
                                continue;
                            }
                            string msg = null;
                            
                            incoming_data.TryGetValue(sub.Key, out msg);
                            
                            if(msg != null) 
                            {
                                sub.Value.DynamicInvoke(msg);
                            }
                        }
                        // In the previous loop, subscribers modify values
                        // As the LAST subscriber, we should update delta time (inside TcpClockSubscriber),
                        // which allows the simulation thread to proceed with fresh values
                        foreach( KeyValuePair<string, Delegate> sub in SubscriberHandlers )
                        {
                            if (sub.Key != clock_topic) {
                                continue;
                            }
                            string msg = null;
                            
                            incoming_data.TryGetValue(sub.Key, out msg);
                            
                            if(msg != null) 
                            {
                                sub.Value.DynamicInvoke(msg);
                            }
                        }
                        new_step_required = true;
                        new_step_executed = false;

                        if (VERBOSE) Debug.Log("Con: going to wait for step execution");

                        // How things work
                        // TCP incoming message sets delta time -> trialmanager loop doStep() is free to go past deltatime check 
                        // -> agents move, etc -> we wait here for doStep() to finish (new_step_executed flag) -> we publish values
                        int retries = 0;
                        while (true) {
                            retries += 1;
                            if (new_step_executed) {
                                new_step_executed = false;
                                break;
                            }
                            if (retries >= 3000) {
                                Debug.Log("Con: Waiting to populate TCP messages failed: Maximum retries reached while waiting for simulation step to complete.");
                                break;
                            }
                            System.Threading.Thread.Sleep(1); // sleep 1 milliseconds
                        }

                        new_step_required = false;

                        if (VERBOSE) Debug.Log("Con: Detected step complete, publishing");

                        //sending clock back first
                        data_out.Add( "clock=" + data_counter.ToString() + "#" + incoming_data["clock"] );

                        foreach( TcpPublisher pub in Publishers)
                        {
                            data_out.Add(pub.Publish(data_counter.ToString(), float.Parse(incoming_data["clock"] , CultureInfo.InvariantCulture.NumberFormat) ) );
                        }


                        data_counter++;

                    }

                    data_out.Add("@"); //end
                    string join_data_out = string.Join( ";", data_out);                    
                    byte[] buffer_out = Encoding.UTF8.GetBytes(join_data_out);

                    nwStream.Write(buffer_out, 0, buffer_out.Length);
                }
            }
            Profiler.EndSample();
        }

        public void Subscribe(string Topic, Action<string> subscriberHandler)
        {        
            SubscriberHandlers[Topic] = new Action<string>(subscriberHandler);
        }

        public void Advertise(TcpPublisher pub)
        {
            Publishers.Add(pub); 
        }

        public void ExecuteSubscriberCommands()
        {
            foreach( MonoBehaviour script in scripts ) {
                if(script.GetType() == typeof(TwistTcpSubscriber) )
                {
                    ((TwistTcpSubscriber)script).ExecuteCurrentCommand();
                }
            }
        }

        public void UpdateMessageValues()
        {
            foreach( MonoBehaviour script in scripts ) {
                if(script.GetType().IsSubclassOf(typeof(TcpPublisher)) )
                {
                    ((TcpPublisher)script).UpdateMessageValue();
                }
            }
        }
    }
}
