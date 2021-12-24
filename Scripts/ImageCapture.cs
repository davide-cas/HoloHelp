using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class ImageCapture : MonoBehaviour
{
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static ImageCapture Instance;

    /// <summary>
    /// Keep counts of the taps for image renaming
    /// </summary>
    private int captureCount = 0;

    /// <summary>
    /// Photo Capture object
    /// </summary>
    private UnityEngine.Windows.WebCam.PhotoCapture photoCaptureObject = null;

    /// <summary>
    /// Allows gestures recognition in HoloLens
    /// </summary>
    private GestureRecognizer recognizer;

    /// <summary>
    /// Application main functionalities switch
    /// </summary>
    internal enum AppModes { Analysis }

    /// <summary>
    /// Local variable for current AppMode
    /// </summary>
    internal AppModes AppMode { get; private set; }

    /// <summary>
    /// Flagging if the capture loop is running
    /// </summary>
    static bool captureIsActive;

    /// <summary>
    /// File path of current analysed photo
    /// </summary>
    internal string filePath = string.Empty;
    
    public GameObject HoloLogo;
    public AudioInfo myAudio;

    /// <summary>
    /// Called on initialization
    /// </summary>
    private void Awake()
    {
        Instance = this;
        AppMode = AppModes.Analysis;
        HoloLogo = GameObject.FindGameObjectWithTag("HoloLogo").transform.GetChild(0).gameObject;
    }

    /// <summary>
    /// Runs at initialization right after Awake method
    /// </summary>
    void Start()
    {
        // Clean up the LocalState folder of this application from all photos stored
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                Debug.LogFormat("Cannot delete file: ", file.Name);
            }
        }

        // Subscribing to the HoloLens API gesture recognizer to track user gestures
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();
    }

    public void HoloSpeech()
    {
        Debug.Log("[DEBUG]: PHOTO FRAME");

        if (!captureIsActive)
        {
            if (HoloLogo.activeInHierarchy == true) HoloLogo.SetActive(false);
            captureIsActive = true;

            // Set the cursor color to red
            SceneOrganiser.Instance.cursor.GetComponent<Renderer>().material.color = Color.red;

            // Update camera status to looping capture.
            SceneOrganiser.Instance.SetCameraStatus("Looping Capture");

            // Begin the capture loop
            ExecuteImageCaptureAndAnalysis();
            //Invoke("ResetImageCapture", 15);
        }

        else
        {
            // The user tapped while the app was analyzing 
            // therefore stop the analysis process
            ResetImageCapture();
        }
    }

    /// <summary>
    /// Respond to Tap Input.
    /// </summary>
    public void TapHandler(TappedEventArgs obj)
    {
        switch (AppMode)
        {
            case AppModes.Analysis:
                if (!captureIsActive)
                {
                    HoloLogo.SetActive(false);
                    captureIsActive = true;

                    // Set the cursor color to red
                    SceneOrganiser.Instance.cursor.GetComponent<Renderer>().material.color = Color.red;

                    // Update camera status to looping capture.
                    SceneOrganiser.Instance.SetCameraStatus("Looping Capture");

                    // Begin the capture loop
                    ExecuteImageCaptureAndAnalysis();
                    //Invoke("ResetImageCapture", 15);
                }

                else
                {
                    // The user tapped while the app was analyzing 
                    // therefore stop the analysis process
                    ResetImageCapture();
                }

                break;
        }
    }

    void changeInNull() { SceneOrganiser.Instance.SetCameraStatus(" "); }

    /// <summary>
    /// Begin process of Image Capturing and send To Azure Custom Vision Service.
    /// </summary>
    private void ExecuteImageCaptureAndAnalysis()
    {
        // Update camera status to analysis.
        myAudio.PlayAudio(29);
        SceneOrganiser.Instance.SetCameraStatus("Analysis");

        // Create a label in world space using the SceneOrganiser class 
        // Invisible at this point but correctly positioned where the image was taken
        SceneOrganiser.Instance.PlaceAnalysisLabel();

        // Set the camera resolution to be the highest possible
        Resolution cameraResolution = UnityEngine.Windows.WebCam.VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        //Resolution cameraResolution = new Resolution { width = 2272, height = 1278, refreshRate = 0 };
        Debug.Log("Camera Resolution: " + cameraResolution);

        Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Begin capture process, set the image format
        UnityEngine.Windows.WebCam.PhotoCapture.CreateAsync(true, delegate (UnityEngine.Windows.WebCam.PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject;

            UnityEngine.Windows.WebCam.CameraParameters camParameters = new UnityEngine.Windows.WebCam.CameraParameters
            {
                hologramOpacity = 0.5f,
                cameraResolutionWidth = targetTexture.width,
                cameraResolutionHeight = targetTexture.height,
                pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32
            };

            // Capture the image from the camera and save it in the App internal folder
            captureObject.StartPhotoModeAsync(camParameters, delegate (UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
            {
                string filename = string.Format(@"CapturedImage{0}.jpg", captureCount);
                filePath = Path.Combine(Application.persistentDataPath, filename);
                captureCount++;
                photoCaptureObject.TakePhotoAsync(filePath, UnityEngine.Windows.WebCam.PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
            });
        });

        Invoke("changeInNull", 5);
    }

    /// <summary>
    /// Register the full execution of the Photo Capture. 
    /// </summary>
    void OnCapturedPhotoToDisk(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        // Call StopPhotoMode once the image has successfully captured
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }


    /// <summary>
    /// The camera photo mode has stopped after the capture.
    /// Begin the Image Analysis process.
    /// </summary>
    void OnStoppedPhotoMode(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        Debug.LogFormat("Stopped Photo Mode");

        // Dispose from the object in memory and request the image analysis 
        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        switch (AppMode)
        {
            case AppModes.Analysis:
                // Call the image analysis
                StartCoroutine(CustomVisionAnalyser.Instance.AnalyseLastImageCaptured(filePath));
                break;
        }
    }

    /// <summary>
    /// Stops all capture pending actions
    /// </summary>
    public static void ResetImageCapture()
    {
        captureIsActive = false;

        // Set the cursor color to green
        SceneOrganiser.Instance.cursor.GetComponent<Renderer>().material.color = Color.green;

        // Update camera status to ready.
        SceneOrganiser.Instance.SetCameraStatus("Ready");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
