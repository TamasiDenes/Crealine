using LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Snake snake;
    [SerializeField] Tail tail;
    [SerializeField] SnakePoint snakeNeck; // point after head
    [SerializeField] SnakePoint snakeEnd; // point after the last point

    [SerializeField] MeshOrganiser meshOrganiser;
    [SerializeField] RewardOrganiser rewardOrganiser;
    [SerializeField] SingleGraphEngine graphEngine;

    public List<Vector2> Points => tail.Points;

    internal void RefreshEnd(Vector2 point) => snakeEnd.Refresh(point);
    internal void RefreshNeck(Vector2 point) => snakeNeck.Refresh(point);
    internal Vector2 GetSnakePosition() => snake.transform.position;

    internal void RefreshGraph()
    {
        //graphEngine = new GraphEngine(points);
        //meshGenerator.GenerateBlobs(graphEngine.ProcessGraph());

        meshOrganiser.RefreshBlobs();
        rewardOrganiser.RefreshRewardsState();
    }
}
