using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timefucker : MonoBehaviour
{
    [Range(0.001f, 2.0f)]
    public float timeScale = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
    }
}
