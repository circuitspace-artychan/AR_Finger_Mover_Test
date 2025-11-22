using UnityEngine;
using System.IO.Ports;
using System.Text;

public class serial_data : MonoBehaviour
{
    SerialPort data_stream = new SerialPort("COM6", 9600);
    public string receivedstring;

    // References to finger bones in the new rig
    public Transform thumb1;
    public Transform thumb2;

    public Transform index2;
    public Transform index3;

    public Transform middle2;
    public Transform middle3;

    public Transform ring2;
    public Transform ring3;

    public Transform little2;
    public Transform little3;

    public float sensitivity = 1;

    private StringBuilder serialBuffer = new StringBuilder();
    private bool readingMessage = false;

    // Simulation mode toggle
    public bool simulateInput = true;
    public float curlSpeed = 60f;

    // Simulated finger angles
    private float simThumb = 0f;
    private float simIndex = 0f;
    private float simMiddle = 0f;
    private float simRing = 0f;
    private float simPinky = 0f;

    public virtual void Start()
    {
        if (!simulateInput)
        {
            data_stream.Open();
            Debug.Log("Start (Serial Active)");
        }
        else
        {
            Debug.Log("Start (Simulation Mode)");
        }
    }

    void Update()
    {
        if (simulateInput)
        {
            UpdateSimulatedFingers();
            ApplySimulatedRotation();
            return;
        }

        if (data_stream.IsOpen)
        {
            try
            {
                while (data_stream.BytesToRead > 0)
                {
                    char incomingChar = (char)data_stream.ReadChar();

                    if (incomingChar == '#')
                    {
                        serialBuffer.Clear();
                        readingMessage = true;
                    }
                    else if (incomingChar == ';' && readingMessage)
                    {
                        readingMessage = false;
                        ProcessMessage(serialBuffer.ToString());
                    }
                    else if (readingMessage)
                    {
                        serialBuffer.Append(incomingChar);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    public static float Map(float analogValue, float analogMin, float analogMax, float rotationalMin, float rotationalMax)
    {
        return (analogValue - analogMin) * (rotationalMax - rotationalMin) / (analogMax - analogMin) + rotationalMin;
    }

    void ProcessMessage(string message)
    {
        string[] datas = message.Split(',');
        float thumbread = 0, indexread = 0, middleread = 0, ringread = 0, pinkyread = 0;
        float mappedThumbRead = 0, mappedIndexRead = 0, mappedMiddleRead = 0, mappedRingRead = 0, mappedPinkyRead = 0;

        if (datas.Length >= 5)
        {
            thumbread = float.Parse(datas[0]);
            mappedThumbRead = Map(thumbread, 0, 1023, 0, 90);

            indexread = float.Parse(datas[1]);
            mappedIndexRead = Map(indexread, 0, 1023, 0, 90);

            middleread = float.Parse(datas[2]);
            mappedMiddleRead = Map(middleread, 0, 1023, 0, 90);

            ringread = float.Parse(datas[3]);
            mappedRingRead = Map(ringread, 0, 1023, 0, 90);

            pinkyread = float.Parse(datas[4]);
            mappedPinkyRead = Map(pinkyread, 0, 1023, 0, 90);
        }

        // Apply serial values to finger bones
        index2.localRotation = Quaternion.Euler(mappedIndexRead, 0, 0);
        index3.localRotation = Quaternion.Euler(mappedIndexRead, 0, 0);

        // Thumb has multi-axis rotation
        thumb1.localRotation = Quaternion.Euler(mappedThumbRead, 0, 0);
        thumb2.localRotation = Quaternion.Euler(0, 0, mappedThumbRead);

        middle2.localRotation = Quaternion.Euler(mappedMiddleRead, 0, 0);
        middle3.localRotation = Quaternion.Euler(mappedMiddleRead, 0, 0);

        ring2.localRotation = Quaternion.Euler(mappedRingRead, 0, 0);
        ring3.localRotation = Quaternion.Euler(mappedRingRead, 0, 0);

        little2.localRotation = Quaternion.Euler(mappedPinkyRead, 0, 0);
        little3.localRotation = Quaternion.Euler(mappedPinkyRead, 0, 0);
    }

    private void OnApplicationQuit()
    {
        if (data_stream.IsOpen)
        {
            data_stream.Close();
        }
    }

    // ------------------ Sim Mode Methods ------------------

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
    }
}
