using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    private List<(Vector3 position, Quaternion rotation)> recordedPositions = new List<(Vector3, Quaternion)>();
    private const float RECORD_INTERVAL = 0.1f; // Mobile için daha az sık kayıt (0.05f -> 0.1f)
    private const int MAX_POSITIONS = 100; // Mobile için daha az pozisyon (200 -> 100)
    private bool isRecording = false;
    private Coroutine recordingCoroutine;

    private void Start()
    {
        StartRecording();
    }

    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            if (recordingCoroutine != null)
            {
                StopCoroutine(recordingCoroutine);
            }
            recordingCoroutine = StartCoroutine(RecordPositionCoroutine());
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            if (recordingCoroutine != null)
            {
                StopCoroutine(recordingCoroutine);
                recordingCoroutine = null;
            }
        }
    }

    private IEnumerator RecordPositionCoroutine()
    {
        while (isRecording)
        {
            RecordPosition();
            yield return new WaitForSeconds(RECORD_INTERVAL);
        }
    }

    private void RecordPosition()
    {
        if (!isRecording) return;

        // Sadece pozisyon değişmişse kaydet (optimize edilmiş)
        if (recordedPositions.Count == 0 || 
            Vector3.Distance(transform.position, recordedPositions[recordedPositions.Count - 1].position) > 0.1f)
        {
            recordedPositions.Add((transform.position, transform.rotation));
            
            if (recordedPositions.Count > MAX_POSITIONS)
            {
                recordedPositions.RemoveAt(0);
            }
        }
    }

    public List<(Vector3 position, Quaternion rotation)> GetRecordedPositions()
    {
        return new List<(Vector3, Quaternion)>(recordedPositions);
    }

    public void ResetPositions()
    {
        recordedPositions.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (recordedPositions.Count == 0) return;

        Color startColor = new Color(1f, 0f, 0f, 0.3f);
        Color endColor = new Color(0f, 1f, 0f, 1f);

        for (int i = 0; i < recordedPositions.Count; i++)
        {
            float age = (float)i / (recordedPositions.Count - 1);
            Color positionColor = Color.Lerp(startColor, endColor, age);
            Gizmos.color = positionColor;

            Vector3 position = recordedPositions[i].position;
            
            // Sadece küçük küre çiz (daha optimize)
            Gizmos.DrawWireSphere(position, 0.1f);

            // Her 5. pozisyonda çizgi çiz (daha az çizgi)
            if (i < recordedPositions.Count - 1 && i % 5 == 0)
            {
                Gizmos.DrawLine(position, recordedPositions[i + 1].position);
            }
        }
    }
#endif

    private void OnDisable()
    {
        StopRecording();
    }
} 