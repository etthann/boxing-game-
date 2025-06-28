using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class UDPClientHandler : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private int portRx;
    private int portTx;
    private string server;
    private string latestJsonData;
    private readonly object lockObj = new();

    public string LatestJson
    {
        get
        {
            lock (lockObj)
            {
                return latestJsonData;
            }
        }
    }

    public void Init(string serverIP, int sendPort, int receivePort)
    {
        server = serverIP;
        portTx = sendPort;
        portRx = receivePort;

        udpClient = new UdpClient(portRx);
        udpClient.Connect(server, portTx);

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public void RequestMode(string mode)
    {
        string json = $"{{\"mode\": \"{mode}\"}}";
        byte[] data = Encoding.UTF8.GetBytes(json);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        byte[] compressedData = CompressGzip(jsonBytes);
        udpClient.Send(compressedData, compressedData.Length);
        try
        {
            udpClient.Send(data, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Send failed: {e.Message}");
        }
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, portRx);
        while (true)
        {
            try
            {
                byte[] received = udpClient.Receive(ref remoteEP);
                string decompressedJson = DecompressGzip(received);

                lock (lockObj)
                {
                    latestJsonData = decompressedJson;
                    // Optionally log
                    // Debug.Log("Received: " + decompressedJson);
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.Interrupted)
                    Debug.LogWarning("UDP socket error: " + se.Message);
                else
                    break; // graceful shutdown
            }
            catch (Exception ex)
            {
                Debug.LogWarning("UDP receive error: " + ex.Message);
            }
        }
    }

    private string DecompressGzip(byte[] gzipData)
    {
        try
        {
            using MemoryStream compressedStream = new MemoryStream(gzipData);
            using GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using StreamReader reader = new StreamReader(zipStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
        catch (Exception e)
        {
            Debug.LogError("Decompression failed: " + e.Message);
            return string.Empty;
        }
    }

    private byte[] CompressGzip(byte[] rawData)
    {
        try
        {
            using MemoryStream output = new MemoryStream();
            using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(rawData, 0, rawData.Length);
            }
            return output.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError("Compression failed: " + e.Message);
            return rawData;
        }
    }

    private void OnApplicationQuit()
    {
        Close();
    }

    public void Close()
    {
        try
        {
            receiveThread?.Abort(); // okay here for background thread
        }
        catch (Exception) { }

        udpClient?.Close();
    }
}
