using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public List<Wall> Walls;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Wall item in transform.gameObject.GetComponentsInChildren<Wall>())
        {
            Walls.Add(item);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
