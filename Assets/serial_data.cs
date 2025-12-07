using UnityEngine;
using System.IO.Ports;
using System.Text;
using System.Globalization;


public class serial_data : MonoBehaviour
{
    SerialPort data_stream = new SerialPort("COM9", 9600)
    {
        ReadTimeout = 100,
        WriteTimeout = 100,
        Encoding = Encoding.ASCII,
        NewLine = ";"
    };

    // Scene refs
    public GameObject index, indexTip;
    public GameObject thumb, thumbTip;
    public GameObject middle, middleTip;
    public GameObject ring, ringTip;
    public GameObject pinky, pinkyTip;

    public float sensitivity = 1f;

    private readonly StringBuilder serialBuffer = new StringBuilder();
    private bool readingMessage = false;

    // throttle writes so we don't slam the Arduino each frame
    private float lastSendTime;
    public float minSendInterval = 0.03f; // ~30 Hz

    void Start()
    {
        Debug.Log("start");
        try
        {
            if (!data_stream.IsOpen)
            {
                data_stream.Open();
                Debug.Log("Bluetooth/USB Serial Opened");
                // Give Arduino time to reset after opening port (common on Uno/Leonardo)
                System.Threading.Thread.Sleep(300);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    void Update()
    {
        if (!data_stream.IsOpen) return;

        // READ: accumulate chars until we see '#' ... ';'
        try
        {
            string incomingData = data_stream.ReadExisting();
            foreach (char ch in incomingData)
            {
                if (ch == '#')
                {
                    serialBuffer.Clear();
                    readingMessage = true;
                }
                else if (ch == ';' && readingMessage)
                {
                    readingMessage = false;
                    ProcessMessage(serialBuffer.ToString());
                }
                else if (readingMessage)
                {
                    serialBuffer.Append(ch);
                }
            }
        }
        catch (System.TimeoutException) { /* ignore */ }
        catch (System.Exception e)
        {
            Debug.LogWarning("Serial Read Error: " + e.Message);
        }
    }

    public static float Map(float v, float inMin, float inMax, float outMin, float outMax)
    {
        return (v - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    void ProcessMessage(string message)
    {
        // Expect 5 comma-separated analogs: thumb,index,middle,ring,pinky (0..1023)
        string[] datas = message.Split(',');
        if (datas.Length < 5) return;

        // Safe parse with invariant culture
        if (!TryParseFloat(datas[0], out float thumbRead)) return;
        if (!TryParseFloat(datas[1], out float indexRead)) return;
        if (!TryParseFloat(datas[2], out float middleRead)) return;
        if (!TryParseFloat(datas[3], out float ringRead)) return;
        if (!TryParseFloat(datas[4], out float pinkyRead)) return;

        float mappedThumb = Map(thumbRead, 0, 1023, 0, 90) * sensitivity;
        float mappedIndex = Map(indexRead, 0, 1023, 0, 90) * sensitivity;
        float mappedMiddle = Map(middleRead, 0, 1023, 0, 90) * sensitivity;
        float mappedRing = Map(ringRead, 0, 1023, 0, 90) * sensitivity;
        float mappedPinky = Map(pinkyRead, 0, 1023, 0, 90) * sensitivity;

        // Apply rotations (X-axis) – keep Y/Z from current transforms
        if (mappedIndex >= 2f)
        {
            Vector3 r = index.transform.eulerAngles;
            index.transform.rotation = Quaternion.Euler(mappedIndex, r.y, r.z);
            indexTip.transform.rotation = Quaternion.Euler(mappedIndex * 2, r.y, r.z);
        }

        if (mappedThumb >= 2f)
        {
            Vector3 r = thumb.transform.eulerAngles;
            thumb.transform.rotation = Quaternion.Euler(mappedThumb, r.y, r.z);
            thumbTip.transform.rotation = Quaternion.Euler(mappedThumb * 2, r.y, r.z);
        }

        if (mappedMiddle >= 2f)
        {
            Vector3 r = middle.transform.eulerAngles;
            middle.transform.rotation = Quaternion.Euler(mappedMiddle, r.y, r.z);
            middleTip.transform.rotation = Quaternion.Euler(mappedMiddle * 2, r.y, r.z);
        }

        if (mappedRing >= 2f)
        {
            Vector3 r = ring.transform.eulerAngles;
            ring.transform.rotation = Quaternion.Euler(mappedRing, r.y, r.z);
            ringTip.transform.rotation = Quaternion.Euler(mappedRing * 2, r.y, r.z);
        }

        if (mappedPinky >= 2f)
        {
            Vector3 r = pinky.transform.eulerAngles; // FIX: use pinky, not index
            pinky.transform.rotation = Quaternion.Euler(mappedPinky, r.y, r.z);
            pinkyTip.transform.rotation = Quaternion.Euler(mappedPinky * 2, r.y, r.z);
        }

        // Compute brightness from index angle (0..90 -> 0..255)
        // Compute brightness from index angle (0..90 -> 0..255)
        float actualAngleIndex = Mathf.Abs(index.transform.eulerAngles.x) % 360f;
        float indexBrightnessF = Mathf.Clamp((actualAngleIndex / 90f) * 255f, 0f, 255f);
        int indexBrightness = Mathf.RoundToInt(indexBrightnessF);

        // Compute a second value from the thumb angle (0..90 -> 0..255)
        float actualAngleThumb = Mathf.Abs(thumb.transform.eulerAngles.x) % 360f;
        float thumbValueF = Mathf.Clamp((actualAngleThumb / 90f) * 255f, 0f, 255f);
        int thumbValue = Mathf.RoundToInt(thumbValueF);


        // Throttle writes
        // Throttle writes
        if (Time.unscaledTime - lastSendTime >= minSendInterval)
        {
            string frame = "#"
                + indexBrightness.ToString(CultureInfo.InvariantCulture)
                + ","
                + thumbValue.ToString(CultureInfo.InvariantCulture)
                + ";";
            try
            {
                data_stream.Write(frame); // Arduino (bridge) forwards this over BT
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Serial Write Error: " + e.Message);
            }
            lastSendTime = Time.unscaledTime;
        }

        // Optional debug
        Debug.Log($"RX: {message}  TX: #{indexBrightness},{thumbValue};");
    }
    static bool TryParseFloat(string s, out float value)
    {
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    void OnApplicationQuit()
    {
        if (data_stream != null && data_stream.IsOpen)
        {
            try { data_stream.Close(); } catch { /* ignore */ }
        }
    }

    /*// ------------------ Sim Mode Methods ------------------

    void UpdateSimulatedFingers()
    {
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shiftHeld)
        {
            // Index finger (Shift + F/R)
            if (Input.GetKey(KeyCode.F))
                simIndex += Time.deltaTime * curlSpeed;
            if (Input.GetKey(KeyCode.R))
                simIndex -= Time.deltaTime * curlSpeed;

            // Middle finger (Shift + D/E)
            if (Input.GetKey(KeyCode.D))
                simMiddle += Time.deltaTime * curlSpeed;
            if (Input.GetKey(KeyCode.E))
                simMiddle -= Time.deltaTime * curlSpeed;

            // Ring finger (Shift + S/W)
            if (Input.GetKey(KeyCode.S))
                simRing += Time.deltaTime * curlSpeed;
            if (Input.GetKey(KeyCode.W))
                simRing -= Time.deltaTime * curlSpeed;

            // Pinky finger (Shift + A/Q)
            if (Input.GetKey(KeyCode.A))
                simPinky += Time.deltaTime * curlSpeed;
            if (Input.GetKey(KeyCode.Q))
                simPinky -= Time.deltaTime * curlSpeed;

            // Thumb (Shift + B/V)
            if (Input.GetKey(KeyCode.B))
                simThumb += Time.deltaTime * curlSpeed;
            if (Input.GetKey(KeyCode.V))
                simThumb -= Time.deltaTime * curlSpeed;
        }

        // Clamp values
        simThumb = Mathf.Clamp(simThumb, 0, 90);
        simIndex = Mathf.Clamp(simIndex, 0, 90);
        simMiddle = Mathf.Clamp(simMiddle, 0, 90);
        simRing = Mathf.Clamp(simRing, 0, 90);
        simPinky = Mathf.Clamp(simPinky, 0, 90);
    }

    void ApplySimulatedRotation()
    {
        // Index finger
        index2.localRotation = Quaternion.Euler(simIndex, 0, 0);
        index3.localRotation = Quaternion.Euler(simIndex, 0, 0);

        // Thumb with multi-axis movement
        thumb1.localRotation = Quaternion.Euler(simThumb, 0, 0);   // bends toward palm
        thumb2.localRotation = Quaternion.Euler(0, 0, -simThumb);   // curls inward

        // Middle finger
        middle2.localRotation = Quaternion.Euler(simMiddle, 0, 0);
        middle3.localRotation = Quaternion.Euler(simMiddle, 0, 0);

        // Ring finger
        ring2.localRotation = Quaternion.Euler(simRing, 0, 0);
        ring3.localRotation = Quaternion.Euler(simRing, 0, 0);

        // Pinky finger
        little2.localRotation = Quaternion.Euler(simPinky, 0, 0);
        little3.localRotation = Quaternion.Euler(simPinky, 0, 0);
    }*/
}
