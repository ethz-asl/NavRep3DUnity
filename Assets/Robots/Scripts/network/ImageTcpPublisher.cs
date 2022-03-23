using UnityEngine;
using System.Linq;

namespace crowdbotsim
{
public class ImageTcpPublisher : TcpPublisher
{
    [SerializeField]
    [Tooltip ("Sensor providing the data to be published")]
    public Camera ImageCamera;
    public int resolutionWidth = 640;
    public int resolutionHeight = 480;
    public bool encodeToJPG = true;
    [Range(0, 100)]
    public int qualityLevel = 50;

    private Texture2D texture2D;
    private RenderTexture renderTexture;
    private Rect rect;
    
    [SerializeField]
    [Tooltip ("Frame id for Tf")]
    public string FrameId = "undefined";
    private string message;
    private string sensor_infos;

    protected override void Start()
    {
        base.Start();        
        InitializeGameObject();
        InitializeMessage();
        Camera.onPostRender += UpdateImage;
    }
    private void UpdateImage(Camera _camera)
    {
        if (texture2D == null) { // && _camera == this.ImageCamera)
            return; // when closing the app, This class (and Texture2D get destroyed, but the UpdateImage method still gets called for a few frames. Hence this check is necessary.
        }
        // UpdateMessage();
    }

    public override string Publish(string data_id, float time)
    {
        //ImageCamera.Render(); // can only be called from main thread. Instead we have to wait:
        // wait for new image
        // problem occurs when the incoming data asks for no step (delta time = 0), but we still expect outgoing data.
       // message = "image_required";

        if (false) {
            int retries = 0;
            while (true) {
                retries += 1;
                if (message != "") {
                    break;
                }
                if (retries >= 300) {
                    Debug.Log("Waiting for camera to render failed: Maximum retries reached while waiting for camera to render new image.");
                    break;
                }
                System.Threading.Thread.Sleep(10); // sleep 10 milliseconds
            }
        }

        string header = concat_to(Topic, '=', data_id);

        // publish infos on first message only
        // if(int.Parse(data_id) == 0)
        // {
        //     header = concat_to(header, '-', sensor_infos);
        // }

        string output = concat_to(header, '#', message);

        // Image has been sent, it is no longer 'new'. Doing this means we will wait for a new image next time.
        message = "";

        return output;
    }

    public override void UpdateMessageValue()
    {
        UpdateMessage();
    }

    private void InitializeGameObject()
        {
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
            renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
            rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
            ImageCamera.targetTexture = renderTexture;
        }

        private void InitializeMessage()
        {
            message = "";
        }
    public void LateUpdate()
    {
        //UpdateMessage();
    }
    public void onPostRender()
    {
        //UpdateMessage();
    }

    private void UpdateMessage()
    {
        ImageCamera.Render(); // makes the image current
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(rect, 0, 0);
        RenderTexture.active = null;
        texture2D.Apply();
        byte[] byteArray;
        if (encodeToJPG) {
           byteArray = texture2D.EncodeToJPG(qualityLevel);
        } else {
           byteArray = texture2D.EncodeToPNG();
        }
        lock (message) {
            message = System.Convert.ToBase64String(byteArray);
            if (message == "") {
                Debug.Log(texture2D);
            }
        }
    }
}
}
