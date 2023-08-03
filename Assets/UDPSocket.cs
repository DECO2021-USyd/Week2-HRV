/*
Created by Youssef Elashry to allow two-way communication between Python3 and Unity to send and receive strings

Feel free to use this in your individual or commercial projects BUT make sure to reference me as: Two-way communication between Python 3 and Unity (C#) - Y. T. Elashry
It would be appreciated if you send me how you have used this in your projects (e.g. Machine Learning) at youssef.elashry@gmail.com

Use at your own risk
Use under the Apache License 2.0

Modified by: 
Youssef Elashry 12/2020 (replaced obsolete functions and improved further - works with Python as well)
Based on older work by Sandra Fang 2016 - Unity3D to MATLAB UDP communication - [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]

Modified by:
Andres Pinilla 05/2023
University of Sydney
Integrating with EoM
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using TMPro;
using ExciteOMeter;

public class UdpSocket : MonoBehaviour
{
    
    // public TMP_Text hr;
    // public TMP_Text rri;
    // public TMP_Text rmssd;
    // public TMP_Text sdnn;
    public TMP_Text eom;

    [HideInInspector] public bool isTxStarted = false;

    [SerializeField] string IP = "127.0.0.1"; // local host
    [SerializeField] int rxPort = 8000; // port to receive data from Python on
    [SerializeField] int txPort = 8001; // port to send data to Python on

    // int i = 0; // DELETE THIS: Added to show sending data from Unity to Python via UDP

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    // IEnumerator SendDataCoroutine() // DELETE THIS: Added to show sending data from Unity to Python via UDP
    // {
    //     while (true)
    //     {
    //         SendData("Sent from Unity: " + i.ToString());
    //         i++;
    //         yield return new WaitForSeconds(1f);
    //     }
    // }

    public void SendData(string message) // Use to send data to Python
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    void Awake()
    {
        // Create remote endpoint (to Matlab) 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);

        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Initialize (seen in comments window)
        print("UDP Comms Initialised");

        // StartCoroutine(SendDataCoroutine()); // DELETE THIS: Added to show sending data from Unity to Python via UDP

        // Subscribe to events
        EoM_Events.OnDataReceived += ExciteOMeterDataReceived;
    }

    // Receive data, update packets received
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                print(">> " + text);
                ProcessInput(text);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private void ProcessInput(string input)
    {
        // PROCESS INPUT RECEIVED STRING HERE

        if (!isTxStarted) // First data arrived so tx started
        {
            isTxStarted = true;
        }
    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }

    // Retrive EoM data
    private void ExciteOMeterDataReceived(DataType type, float timestamp, float value)
    {
        ///// You can uncomment the line below to receive data only when 
        ///// the Excite-O-Meter is recording data.
        // if (!isCurrentlyRecording) return;

        switch (type)
        {
            case DataType.NONE:
                break;
            // case DataType.HeartRate:
            //     Debug.Log($"Received HR with timestamp {timestamp}, value {value}");
            //     SendData(value.ToString());
            //     hr.text = value.ToString();
            //     break;
            // case DataType.RRInterval:
            //     Debug.Log($"Received RR-interval with timestamp {timestamp}, value {value}");
            //     SendData(value.ToString());
            //     rri.text = value.ToString();
            //     break;
            // case DataType.RMSSD:
            //     Debug.Log($"Received RR-interval with timestamp {timestamp}, value {value}");
            //     SendData(value.ToString());
            //     rmssd.text = value.ToString();
            //     break;
            // case DataType.SDNN:
            //     Debug.Log($"Received RR-interval with timestamp {timestamp}, value {value}");
            //     SendData(value.ToString());
            //     sdnn.text = value.ToString();
            //     break;
            case DataType.EOM:
                Debug.Log($"Received EOM with timestamp {timestamp}, value {value}");
                SendData(value.ToString());
                eom.text = value.ToString();
                break;
            default:
                break;
        }
    }

}