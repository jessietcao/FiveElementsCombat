using UnityEngine;

public class SplitScreenSetup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.parent.name == "Player1")
        {
            GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1); // Left half
        }
        else
        {
            GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1); // Right half
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
