using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using System;
using UnityEngine.Windows.WebCam;
using System.Linq;

public class SceneOrganiser : MonoBehaviour
{
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static SceneOrganiser Instance;

    /// <summary>
    /// The cursor object attached to the camera
    /// </summary>
    internal GameObject cursor;

    /// <summary>
    /// The label used to display the analysis on the objects in the real world
    /// </summary>
    internal GameObject label;

    /// <summary>
    /// Object providing the current status of the camera.
    /// </summary>
    internal TextMesh cameraStatusIndicator;

    /// <summary>
    /// Reference to the last label positioned
    /// </summary>
    internal Transform lastLabelPlaced;

    /// <summary>
    /// Reference to the last label positioned
    /// </summary>
    internal TextMesh lastLabelPlacedText;

    /// <summary>
    /// Flagging if the capture loop is running
    /// </summary>
    internal bool captureIsActive;

    Camera cam;
    GameObject gazeObject;
    Vector3 currentGazePos;
    Vector2 viewPos = new Vector2(-1, -1);

    Resolution cameraResolution;

    /// <summary>
    /// Current threshold accepted for displaying the label
    /// Reduce this value to display the recognition more often
    /// </summary>
    internal float probabilityThreshold = 0.6f;

    public GameObject sphere, cube, sink, cell_phone, knife;

    public List<Material> Materials;
    public GameObject class_detected;

    float focal_f = 1300.0f;
    float coordX, coordY;

    public AudioInfo myAudio;
    bool flag;

    /// <summary>
    /// Called on initialization
    /// </summary>
    private void Awake()
    {
        // Use this class instance as singleton
        Instance = this;

        // Add the ImageCapture class to this GameObject
        gameObject.AddComponent<ImageCapture>();

        // Add the CustomVisionAnalyser class to this GameObject
        gameObject.AddComponent<CustomVisionAnalyser>();

        // Add the CustomVisionObjects class to this GameObject
        gameObject.AddComponent<CustomVisionObjects>();

        // Create the camera Cursor
        cursor = CreateCameraCursor();

        // Load the label prefab as reference
        label = CreateLabel();

        // Create the camera status indicator label, and place it above where predictions
        // and training UI will appear.
        cameraStatusIndicator = CreateTrainingUI("Status Indicator", 0.02f, 0.2f, 3, true);

        // Set camera status indicator to loading.
        //SetCameraStatus("Loading");

        cube = GameObject.FindGameObjectWithTag("object");
        sink = GameObject.FindGameObjectWithTag("sink");
        cell_phone = GameObject.FindGameObjectWithTag("cell_phone");
        knife = GameObject.FindGameObjectWithTag("knife");
        class_detected = GameObject.FindGameObjectWithTag("class_detected");

        cube.SetActive(false);
        sink.SetActive(false);
        cell_phone.SetActive(false);
        knife.SetActive(false);

        //ASSEGNO ALLA CAMERA IL DISPOSITIVO HOLOLENS2
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        cameraResolution = UnityEngine.Windows.WebCam.VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        //cameraResolution = new Resolution { width = 2272, height = 1278, refreshRate = 0 };
    }

    /// <summary>
    /// Spawns cursor for the Main Camera
    /// </summary>
    private GameObject CreateCameraCursor()
    {
        // Create a sphere as new cursor
        GameObject newCursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Attach it to the camera
        newCursor.transform.parent = gameObject.transform;

        // Resize the new cursor
        newCursor.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        // Move it to the correct position
        newCursor.transform.localPosition = new Vector3(0, 0, 4);

        // Set the cursor color to red
        newCursor.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
        newCursor.GetComponent<Renderer>().material.color = Color.white;

        return newCursor;
    }

    /// <summary>
    /// Create the analysis label object
    /// </summary>
    private GameObject CreateLabel()
    {
        // Create a sphere as new cursor
        GameObject newLabel = new GameObject();

        // Resize the new cursor
        newLabel.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Creating the text of the label
        TextMesh t = newLabel.AddComponent<TextMesh>();
        t.anchor = TextAnchor.MiddleCenter;
        t.alignment = TextAlignment.Center;
        t.fontSize = 50;
        t.text = "";

        return newLabel;
    }

    /// <summary>
    /// Set the camera status to a provided string. Will be coloured if it matches a keyword.
    /// </summary>
    /// <param name="statusText">Input string</param>
    public void SetCameraStatus(string statusText)
    {
        if (string.IsNullOrEmpty(statusText) == false)
        {
            string message = "white";

            switch (statusText.ToLower())
            {
                case "loading":
                    message = "yellow";
                    break;

                case "ready":
                    message = "green";
                    break;

                case "uploading image":
                    message = "red";
                    break;

                case "looping capture":
                    message = "yellow";
                    break;

                case "analysis":
                    message = "red";
                    break;

                default:
                    statusText = " ";
                    message = "white";
                    break;
            }
            cameraStatusIndicator.GetComponent<TextMesh>().text = $"<color={message}>{statusText}</color>";
        }
    }

    /// <summary>
    /// Instantiate a label in the appropriate location relative to the Main Camera.
    /// </summary>
    public void PlaceAnalysisLabel()
    {
        lastLabelPlaced = Instantiate(label.transform, cursor.transform.position, transform.rotation);
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
    }

    /// <summary>
    /// IMAGE PROCESSING ORGANIZATION
    /// </summary>

    public void changeImage(int n) { cube.GetComponent<MeshRenderer>().material = Materials[n]; }

    public void changeCellPhone()
    {
        if (cube.activeInHierarchy == true) { cube.SetActive(false); }
        cell_phone.SetActive(true);
    }

    public void changeKnife()
    {
        if (cube.activeInHierarchy == true) { cube.SetActive(false); }
        knife.SetActive(true);
    }

    public void changeSink()
    {
        if (cube.activeInHierarchy == true) { cube.SetActive(false); }

        sink.GetComponent<MeshRenderer>().material = Materials[index];
        index++;

        if (index > 17) { index = 10; }
    }
    
    int d_index = 18;
    public void changeDisplay()
    {
        cube.GetComponent<MeshRenderer>().material = Materials[d_index];
        d_index++;

        if (d_index > 22) { d_index = 18; }
    }

    int l_index = 23;
    public void changeLaptop()
    {
        cube.GetComponent<MeshRenderer>().material = Materials[l_index];
        l_index++;

        if (l_index > 27) { l_index = 23; }
    }

    int mw_index = 28;
    public void changeMicroWave()
    {
        cube.GetComponent<MeshRenderer>().material = Materials[mw_index];
        mw_index++;

        if (mw_index > 32) { mw_index = 28; }
    }

    int ms_index = 33;
    public void changeMouse()
    {
        cube.GetComponent<MeshRenderer>().material = Materials[ms_index];
        ms_index++;

        if (ms_index > 36) { ms_index = 33; }
    }

    /// <summary>
    /// AUDIO PLAYING ORGANIZATION
    /// </summary>

    int index = 10;
    int audioIndex = 20;

    public void firstAudioSink() { myAudio.PlayAudio(28); }
    public void audioSink()
    {
        myAudio.PlayAudio(audioIndex);
        audioIndex++;

        if (index > 25) { audioIndex = 20; }
    }

    public void audioCellPhone() { myAudio.PlayAudio(27); }
    public void audioDisplay2() { myAudio.PlayAudio(31); }
    public void audioDisplay() { myAudio.PlayAudio(30); Invoke("audioDisplay2", 6); }
    public void audioKeyboard() { myAudio.PlayAudio(32); }
    public void audioRemote() { myAudio.PlayAudio(33); }
    public void audioLaptop() { myAudio.PlayAudio(34); }
    public void audioMicroWave() { myAudio.PlayAudio(35); }
    public void audioMouse() { myAudio.PlayAudio(36); }
    public void audioKnife() { myAudio.PlayAudio(37); }

    /// <summary>
    /// RESET IMAGE CAPTURE
    /// </summary>

    public void Reset()
    {
        ImageCapture.ResetImageCapture();

        if (cube.activeInHierarchy == true) cube.SetActive(false);
        if (sink.activeInHierarchy == true) sink.SetActive(false);
        if (cell_phone.activeInHierarchy == true) cell_phone.SetActive(false);
        if (knife.activeInHierarchy == true) knife.SetActive(false);

        class_detected.GetComponent<TMP_Text>().text = " ";

        // Stop the capture loop if active
        CancelInvoke();
    }

    public void timeToLive(float waitingTime) { Invoke("Reset", waitingTime); }

    /* public float[] GetEyeGaze()
    {
        Vector3 gazePos = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized * 1;
        float[] coords = new float[] { gazePos.x, gazePos.y, gazePos.z };

        // Mapping coords from 2D to 3D space
        float focal_l = 1300.0f;

        float x = focal_l * ((1.52048f * coords[0]) / coords[2]);
        float y = focal_l * ((1.52048f * coords[1]) / coords[2]);

        float center_x = 3904 / 2; 
        float center_y = 2196 / 2;

        x += 3904 / 2;
        y = (2196 / 2) - y;

        float x2 = (int)(x - Math.Sign(x - center_x) * Math.Sqrt(Math.Abs(x - center_x) * 16));
        float y2 = (int)(y - Math.Sign(y - center_y) * Math.Sqrt(Math.Abs(y - center_y) * 9));

        float[] coords2 = new float[] { x, y };
        return coords2;
    } */

    /*
    public void updateEyeGaze()
    {
        if (gazeObject == null) { gazeObject = GameObject.Find("CursorRest"); }

        try
        {
            if (gazeObject != null)
            {
                currentGazePos = Camera.main.transform.InverseTransformPoint(gazeObject.transform.position);
                viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);

                if ((viewPos.x < 0 || viewPos.x > cameraResolution.width) || (viewPos.y < 0 || viewPos.x > cameraResolution.height))
                {
                    currentGazePos = Camera.main.transform.InverseTransformPoint(gazeObject.transform.position);
                    viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                    viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);
                }
            }
            else
            {
                currentGazePos = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized * 1;
                viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);
            }

            viewPos.x += (cameraResolution.width / 2);
            viewPos.y = (cameraResolution.height / 2) - viewPos.y;
        }

        catch (InvalidCastException e)
        {
            Debug.Log(e);
        }

        coordX = viewPos.x;
        coordY = viewPos.y;

        Debug.Log("REAL-TIME COORDS: X = " + coordX + ", Y = " + coordY);
    }*/

    /*
    public float[] GetEyeGazeSQUARE()
    {

        float center_x = 3904 / 2; 
        float center_y = 2196 / 2;

        x += 3904 / 2;
        y = (2196 / 2) - y;

        float x2 = (int)(x - Math.Sign(x - center_x) * Math.Sqrt(Math.Abs(x - center_x) * 16));
        float y2 = (int)(y - Math.Sign(y - center_y) * Math.Sqrt(Math.Abs(y - center_y) * 9));

        float[] coords2 = new float[] { x, y };
        return coords2;




        if (gazeObject == null) { gazeObject = GameObject.Find("CursorRest"); }

        try
        {
            if (gazeObject != null)
            {
                currentGazePos = Camera.main.transform.InverseTransformPoint(gazeObject.transform.position);
                viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);

                if ((viewPos.x < 0 || viewPos.x > cameraResolution.width) || (viewPos.y < 0 || viewPos.x > cameraResolution.height))
                {
                    currentGazePos = Camera.main.transform.InverseTransformPoint(gazeObject.transform.position);
                    viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                    viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);
                }
            }
            else
            {
                currentGazePos = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized * 1;
                viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);
            }

            viewPos.x += (cameraResolution.width / 2);
            viewPos.y = (cameraResolution.height / 2) - viewPos.y;
        }

        catch (InvalidCastException e)
        {
            Debug.Log(e);
        }

        float[] coords = { viewPos.x, viewPos.y };
        Debug.Log("REAL-TIME COORDS: X = " + coords[0] + ", Y = " + coords[1]);

        return coords;
    }
    */

    public void SetTagsToLastLabel(AnalysisObject analysisObject)
    {
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
        
        Debug.Log("GAZECOORDS: X = " + coordX + ", Y = " + coordY);

        if (analysisObject.Predictions != null)
        {
            flag = false;

            foreach (Prediction p in analysisObject.Predictions)
            {
                if (p.Probability > 0.60)
                {
                    // HoloLens 2 Camera Resolution
                    // Width Resolution = 2272
                    // Height Resolution = 1278

                    double left = p.boundingBox.left * cameraResolution.width;
                    double top = p.boundingBox.top * cameraResolution.height;
                    double width = p.boundingBox.width * cameraResolution.width;
                    double height = p.boundingBox.height * cameraResolution.height;

                    // "Error" Pixels, 50x50 or 100x100 box enlarged
                    int offset = 50;
                    int offset2 = 100;

                    string tagName = "";
                    double probability;

                    if (coordX >= (left - offset) && coordX <= (left + width + offset) && coordY >= (top - offset) && coordY <= (top + height + offset))
                    {
                        flag = true;
                        tagName = p.TagName;
                        probability = p.Probability;
                        SetCameraStatus(" ");

                        switch (tagName)
                        {
                            case "cell phone":
                                changeImage(1);
                                myAudio.PlayAudio(0);
                                cube.SetActive(true);

                                Invoke("audioCellPhone", 3.5f);
                                Invoke("changeCellPhone", 5);

                                timeToLive(12);
                                break;

                            case "display":
                                changeImage(2);
                                myAudio.PlayAudio(1);

                                d_index = 18;
                                Invoke("audioDisplay", 3);
                                InvokeRepeating("changeDisplay", 10, 3.5f);
                                cube.SetActive(true);

                                timeToLive(25);
                                break;

                            case "keyboard":
                                changeImage(3);
                                myAudio.PlayAudio(2);

                                Invoke("audioKeyboard", 3.5f);
                                cube.SetActive(true);

                                timeToLive(15);
                                break;

                            case "knife":
                                changeImage(4);
                                myAudio.PlayAudio(3);
                                cube.SetActive(true);

                                Invoke("audioKnife", 3.5f);
                                Invoke("changeKnife", 5);
                                
                                timeToLive(22.5f);
                                break;

                            case "laptop":
                                changeImage(5);
                                myAudio.PlayAudio(4);

                                Invoke("audioLaptop", 3.5f);
                                InvokeRepeating("changeLaptop", 5, 5);
                                cube.SetActive(true);

                                timeToLive(31.5f);
                                break;

                            case "microwave":
                                changeImage(6);
                                myAudio.PlayAudio(5);

                                mw_index = 28;
                                Invoke("audioMicroWave", 3.5f);
                                InvokeRepeating("changeMicroWave", 5, 5);
                                cube.SetActive(true);

                                timeToLive(27);
                                break;

                            case "mouse":
                                changeImage(7);
                                myAudio.PlayAudio(6);

                                ms_index = 33;
                                Invoke("audioMouse", 3.5f);
                                InvokeRepeating("changeMouse", 5, 5);
                                cube.SetActive(true);

                                timeToLive(24);
                                break;

                            case "remote":
                                changeImage(8);
                                myAudio.PlayAudio(7);

                                Invoke("audioRemote", 3.5f);
                                cube.SetActive(true);

                                timeToLive(21.5f);
                                break;

                            case "sink":
                                changeImage(9);
                                myAudio.PlayAudio(8);
                                cube.SetActive(true);

                                index = 10;
                                audioIndex = 20;
                                Invoke("firstAudioSink", 3);
                                InvokeRepeating("changeSink", 5, 4);
                                InvokeRepeating("audioSink", 8, 4.5f);
                                sink.SetActive(true);

                                timeToLive(36);
                                break;

                            default: break;
                        }
                        class_detected.GetComponent<TMP_Text>().text = $"Detected: {tagName} {probability:P}";
                        Debug.Log("DETECTED CLASS: " + tagName);
                    }

                    else if (coordX >= (left - offset2) && coordX <= (left + width + offset2) && coordY >= (top - offset2) && coordY <= (top + height + offset2))
                    {
                        flag = true;
                        tagName = p.TagName;
                        probability = p.Probability;
                        SetCameraStatus(" ");

                        switch (tagName)
                        {
                            case "cell phone":
                                changeImage(1);
                                myAudio.PlayAudio(11);
                                cube.SetActive(true);

                                Invoke("audioCellPhone", 6.5f);
                                Invoke("changeCellPhone", 8);

                                timeToLive(15);
                                break;

                            case "display":
                                changeImage(2);
                                myAudio.PlayAudio(12);

                                d_index = 18;
                                Invoke("audioDisplay", 6);
                                InvokeRepeating("changeDisplay", 13, 3.5f);
                                cube.SetActive(true);

                                timeToLive(28);
                                break;

                            case "keyboard":
                                changeImage(3);
                                myAudio.PlayAudio(13);

                                Invoke("audioKeyboard", 6.5f);
                                cube.SetActive(true);

                                timeToLive(18);
                                break;

                            case "knife":
                                changeImage(4);
                                myAudio.PlayAudio(14);
                                cube.SetActive(true);

                                Invoke("audioKnife", 6.5f);
                                Invoke("changeKnife", 8);

                                timeToLive(25.5f);
                                break;

                            case "laptop":
                                changeImage(5);
                                myAudio.PlayAudio(15);

                                Invoke("audioLaptop", 6.5f);
                                InvokeRepeating("changeLaptop", 8, 5);
                                cube.SetActive(true);

                                timeToLive(34.5f);
                                break;

                            case "microwave":
                                changeImage(6);
                                myAudio.PlayAudio(16);

                                mw_index = 28;
                                Invoke("audioMicroWave", 6.5f);
                                InvokeRepeating("changeMicroWave", 8, 5);
                                cube.SetActive(true);

                                timeToLive(30);
                                break;

                            case "mouse":
                                changeImage(7);
                                myAudio.PlayAudio(17);

                                ms_index = 33;
                                Invoke("audioMouse", 6.5f);
                                InvokeRepeating("changeMouse", 8, 5);
                                cube.SetActive(true);

                                timeToLive(27);
                                break;

                            case "remote":
                                changeImage(8);
                                myAudio.PlayAudio(18);

                                Invoke("audioRemote", 6.5f);
                                cube.SetActive(true);

                                timeToLive(24.5f);
                                break;

                            case "sink":
                                changeImage(9);
                                myAudio.PlayAudio(19);
                                cube.SetActive(true);

                                index = 10;
                                audioIndex = 20;
                                Invoke("firstAudioSink", 6);
                                InvokeRepeating("changeSink", 8, 4);
                                InvokeRepeating("audioSink", 11, 4.5f);
                                sink.SetActive(true);

                                timeToLive(39);
                                break;

                            default: break;
                        }
                        class_detected.GetComponent<TMP_Text>().text = $"Detected: {tagName} {probability:P}";
                        Debug.Log("DETECTED CLASS: " + tagName);
                    }
                    if (flag == true) break;
                }
            }

            // The predictor couldn't detect anything.
            System.Random rnd = new System.Random();
            int num = rnd.Next(10, 20);

            if (flag == false)
            {
                if (num % 2 == 0) myAudio.PlayAudio(9);
                else myAudio.PlayAudio(10);
                timeToLive(5);
            }
        }
    }

    /// <summary>
    /// Create a 3D Text Mesh in scene, with various parameters.
    /// </summary>
    /// <param name="name">name of object</param>
    /// <param name="name">name of object</param>
    /// <param name="scale">scale of object (i.e. 0.04f)</param>
    /// <param name="yPos">height above the cursor (i.e. 0.3f</param>
    /// <param name="zPos">distance from the camera</param>
    /// <param name="setActive">whether the text mesh should be visible when it has been created</param>
    /// <returns>Returns a 3D text mesh within the scene</returns>

    internal TextMesh CreateTrainingUI(string name, float scale, float yPos, float zPos, bool setActive)
    {
        GameObject display = new GameObject(name, typeof(TextMesh));
        display.transform.parent = Camera.main.transform;
        display.transform.localPosition = new Vector3(0, yPos, zPos);
        display.SetActive(setActive);
        display.transform.localScale = new Vector3(scale, scale, scale);
        display.transform.rotation = new Quaternion();
        TextMesh textMesh = display.GetComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        return textMesh;
    }

    void Update()
    {
        sphere.transform.position = (CoreServices.InputSystem.EyeGazeProvider.GazeOrigin + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized * 1);
        //updateEyeGaze();

        if (gazeObject == null) { gazeObject = GameObject.Find("CursorRest"); }

        try
        {
            if (gazeObject != null)
            {
                currentGazePos = Camera.main.transform.InverseTransformPoint(gazeObject.transform.position);
                viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);

                if ((viewPos.x < 0 || viewPos.x > cameraResolution.width) || (viewPos.y < 0 || viewPos.x > cameraResolution.height))
                {
                    currentGazePos = Camera.main.transform.InverseTransformPoint(gazeObject.transform.position);
                    viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                    viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);
                }
            }
            else
            {
                currentGazePos = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized * 1;
                viewPos.x = focal_f * ((1.52048f * currentGazePos.x) / currentGazePos.z);
                viewPos.y = focal_f * ((1.52048f * currentGazePos.y) / currentGazePos.z);
            }

            viewPos.x += (cameraResolution.width / 2);
            viewPos.y = (cameraResolution.height / 2) - viewPos.y;
        }

        catch (InvalidCastException e)
        {
            Debug.Log(e);
        }

        coordX = viewPos.x;
        coordY = viewPos.y;

        Debug.Log("REAL-TIME COORDS: X = " + coordX + ", Y = " + coordY);
    }
}