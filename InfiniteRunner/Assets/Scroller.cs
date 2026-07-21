using UnityEngine;

public class Scroller : MonoBehaviour
{
    void Start()
    {
        
    }

    public float speed = 5f;

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }
}
