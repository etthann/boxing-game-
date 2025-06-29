using UnityEngine;
using Unity.FPS.Gameplay;

public class PlayerDataController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Initalize());
    }

    // Update is called once per aframe
    void Update()
    {
        if (UDPClientHandler.Instance != null)
        {
            PlayerDataHolder.latestJson = UDPClientHandler.Instance.LatestJson;
            // Uncomment the next line if you want to see the raw data
            // Debug.Log(UDPClientHandler.Instance.LatestJson);
        }
    }

    private System.Collections.IEnumerator Initalize()
    {
        // Check if UDPClientHandler.Instance exists before initializing
        if (UDPClientHandler.Instance == null)
        {
            Debug.LogError("UDPClientHandler.Instance is null! Cannot initialize UDP connection.");
            yield break;
        }

        // Initialize UDP with same ports as calibration
        UDPClientHandler.Instance.Init("127.0.0.1", 8001, 8000);

        // Wait a moment for initialization
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(SetModeToTracking());
    }

    private System.Collections.IEnumerator SetModeToTracking()
    {
        Debug.Log("Telling server to start tracking mode");

        if (UDPClientHandler.Instance == null)
        {
            Debug.Log("UDP client is null");
            yield break;
        }

        UDPClientHandler.Instance.RequestMode("track");

    }

}