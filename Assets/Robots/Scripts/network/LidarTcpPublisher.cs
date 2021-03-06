using UnityEngine;

namespace crowdbotsim
{
public class LidarTcpPublisher : TcpPublisher
{
    [SerializeField]
    [Tooltip ("Sensor providing the data to be published")]
    private LidarProvider Sensor;
    
    [SerializeField]
    [Tooltip ("Frame id for Tf")]
    public string FrameId = "undefined";
    private float scanPeriod;
    private float previousScanTime = 0;
    public bool publish_seg_mask = true;
    private float last_timestamp;
    private string message;

    private string sensor_infos;
    private float[] ranges, intensities;
    private int[] seg_mask;


    protected override void Start()
    {
        base.Start();        
        scanPeriod = Sensor.GetScanTime();
        
        sensor_infos = scanPeriod.ToString("0.000");
        sensor_infos = concat_to(sensor_infos,',',FrameId);
        sensor_infos = concat_to(sensor_infos,',',Sensor.GetStartAngle() * Mathf.Deg2Rad); //angle_min
        sensor_infos = concat_to(sensor_infos,',',Sensor.GetEndAngle() * Mathf.Deg2Rad); //angle_max
        sensor_infos = concat_to(sensor_infos,',',Sensor.GetAngularResolution() * Mathf.Deg2Rad); //angle_increment

        int dataLenth = Mathf.RoundToInt((Sensor.GetEndAngle() - Sensor.GetStartAngle()) / Sensor.GetAngularResolution());
        
        sensor_infos = concat_to(sensor_infos,',', Sensor.GetScanTime() / dataLenth); // time_increment
        sensor_infos = concat_to(sensor_infos,',', Sensor.GetMinRange()); // range_min
        sensor_infos = concat_to(sensor_infos,',', Sensor.GetMaxRange()); // range_max

        ranges = new float[dataLenth];
        intensities = new float[dataLenth];
        seg_mask = new int[dataLenth];

        last_timestamp = -scanPeriod;
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

    public void LateUpdate()
    {
        UpdateMessage();
    }

    private void UpdateMessage()
    {
        if(ToolsTime.TimeSinceTrialStarted >= last_timestamp + scanPeriod)
        {
            message = "ranges";
            ranges = Sensor.GetData();
            intensities = Sensor.GetIntensities();
            if(publish_seg_mask)
            {
                seg_mask = Sensor.GetSegMask();
            }

            if(ranges != null && intensities != null) 
            {
                message = concat_to(message,'#',ranges);
                message = concat_to(message,'#',"intensities");
                message = concat_to(message,'#',intensities);
                if(publish_seg_mask && seg_mask != null)
                {
                    message = concat_to(message,'#',"masks");
                    message = concat_to(message, '#', seg_mask);
                }
            }

            last_timestamp = ToolsTime.TimeSinceTrialStarted;
        }

    }
}
}