using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    public GameObject groundSegmentPrefab;
    public int segmentCount = 2;
    public float segmentWidth = 40f;
    public float scrollSpeed = 5f;
    public float groundY = -3f;
    public float recycleThreshold = -40f;

    private List<Transform> segments = new List<Transform>();
    private float nextXPosition;
    
    void Start()
    {
        nextXPosition = 0f;
        for (int i = 0; i < segmentCount; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
        foreach (Transform segment in segments)
        {
            segment.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
        }

        if (segments[0].position.x < recycleThreshold)
        {
            RecycleFirstSegment();
        }
    }

    void SpawnSegment()
    {
        GameObject segment = Instantiate(groundSegmentPrefab, new Vector3(nextXPosition, groundY, 0f), Quaternion.identity);
        segments.Add(segment.transform);
        nextXPosition += segmentWidth;
    }

    void RecycleFirstSegment()
    {
        Transform recycled = segments[0];
        segments.RemoveAt(0);

        Transform lastSegment = segments[segments.Count - 1];
        float newX = lastSegment.position.x + segmentWidth;
        recycled.position = new Vector3(newX, groundY, 0f);

        segments.Add(recycled);
    }
}
