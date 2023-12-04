using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakePoint : MonoBehaviour
{
    [SerializeField] private Player player;

    private Vector2 position;


    // Update is called once per frame
    void Update()
    {
        transform.position = position;
    }

    internal void Refresh(Vector2 point)
    {
        position = point;
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        // only if the collider object is not mine
        if (col.transform.tag != player.tag)
        {
            player.RefreshGraph();
        }
    }
}
