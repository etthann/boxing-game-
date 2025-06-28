using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CalibrationController : MonoBehaviour
{
    [Header("References")]
    private UDPClientHandler udp;
    public TextMeshProUGUI countDownText;

    [Header("Settings")]
    public int countdownSeconds = 5;
    public string nextScene = "MainScene";

    [System.Serializable]
    public class StatusMessage
    {
        public string status;
    }

    void Start()
    {
        StartCoroutine(InitializeAndStart());
    }

    private System.Collections.IEnumerator InitializeAndStart()
    {
        // Check if UI components are assigned, if not try to find them
        if (countDownText == null)
        {
            countDownText = FindFirstObjectByType<TextMeshProUGUI>();
            if (countDownText == null)
            {
                Debug.LogWarning("⚠️ No TextMeshProUGUI found in scene. UI updates will be skipped.");
            }
            else
            {
                Debug.Log("🔧 Auto-found TextMeshProUGUI component");
            }
        }

        if (udp == null)
        {
            udp = FindFirstObjectByType<UDPClientHandler>();
            if (udp == null)
            {
                // Create a new UDPClientHandler if none exists
                GameObject udpGO = new GameObject("UDPClientHandler");
                udp = udpGO.AddComponent<UDPClientHandler>();
                Debug.Log("🔧 Created new UDPClientHandler instance");
            }
        }

        Debug.Log("📡 Initializing UDP client...");
        udp.Init("127.0.0.1", 8001, 8000); // TX to Python on 8001, RX from Python on 8000

        // Wait a moment for UDP to initialize
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(CountdownAndCalibrate());
    }

    private System.Collections.IEnumerator CountdownAndCalibrate()
    {
        Debug.Log($"⏳ Starting {countdownSeconds}-second calibration countdown...");
        for (int i = countdownSeconds; i > 0; i--)
        {
            if (countDownText != null)
                countDownText.text = $"Calibrating in: {i}...";
            Debug.Log($"⏱ Countdown: {i}");
            yield return new WaitForSeconds(1f);
        }

        if (countDownText != null)
            countDownText.text = "Calibrating...";
        Debug.Log("📤 Sending mode request: calibrate");

        if (udp != null)
        {
            Debug.Log("📤 Sending mode request: calibrate");
            udp.RequestMode("calibrate");
            Debug.Log("📤 Mode request sent successfully");
        }
        else
        {
            Debug.LogError("❌ UDP client is null when trying to request calibrate mode!");
            yield break;
        }

        bool confirmed = false;
        yield return StartCoroutine(WaitForCalibrationConfirmation(result => confirmed = result));

        if (confirmed)
        {
            Debug.Log("✅ Calibration confirmed. Loading next scene.");
            if (countDownText != null)
                countDownText.text = "✅ Calibration Completed!";
            CalibrationDataHolder.CalibrationData = udp.LatestJson;
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning("❌ Calibration failed after all attempts.");
            if (countDownText != null)
                countDownText.text = "❌ Calibration Failed. Try Again.";
            yield return new WaitForSeconds(2f);
            Application.Quit();
        }
    }

    private System.Collections.IEnumerator WaitForCalibrationConfirmation(System.Action<bool> onComplete)
    {
        float timeout = 10f;
        int maxAttempts = 5;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            float timer = 0f;
            Debug.Log($"🔁 Calibration attempt {attempts + 1}/{maxAttempts}");

            while (timer < timeout)
            {
                string json = udp.LatestJson;

                if (!string.IsNullOrEmpty(json))
                {
                    Debug.Log("📨 Raw received data: " + json);

                    try
                    {
                        StatusMessage msg = JsonUtility.FromJson<StatusMessage>(json);

                        if (msg != null && !string.IsNullOrEmpty(msg.status))
                        {
                            Debug.Log($"🔍 Parsed status: '{msg.status}'");
                            if (msg.status == "calibrated")
                            {
                                Debug.Log("🟢 Calibration status 'calibrated' received!");
                                onComplete(true);
                                yield break;
                            }
                        }
                        else
                        {
                            Debug.Log("⚠️ StatusMessage is null or status field is empty");
                            // Check if this might be calibration data instead
                            if (json.Contains("left_shoulder") || json.Contains("right_shoulder"))
                            {
                                Debug.Log("🔍 Received calibration data, treating as successful calibration");
                                onComplete(true);
                                yield break;
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"⚠️ Failed to parse JSON: {e.Message}");
                    }
                }

                timer += Time.deltaTime;
                yield return null;
            }

            attempts++;
            if (countDownText != null)
                countDownText.text = $"Calibration Timeout! Retrying...";
            Debug.LogWarning($"⌛ Timeout waiting for calibration response. Attempt {attempts}/{maxAttempts}");

            // Resend calibrate mode request on retry
            if (attempts < maxAttempts && udp != null)
            {
                Debug.Log("📤 Resending calibrate mode request...");
                udp.RequestMode("calibrate");
            }

            yield return new WaitForSeconds(1f);
        }

        onComplete(false);
    }
}
