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

    [SerializeField] MeshOrganiser meshOrganiser;
    [SerializeField] RewardOrganiser rewardOrganiser;
    [SerializeField] SingleGraphEngine graphEngine;

    public List<Vector2> Points => tail.Points;

    internal Vector2 GetSnakePosition() => snake.transform.position;

    internal void RefreshGraph()
    {
        //graphEngine = new GraphEngine(points);
        //meshGenerator.GenerateBlobs(graphEngine.ProcessGraph());

        meshOrganiser.RefreshBlobs();
        rewardOrganiser.RefreshRewardsState();
    }
}
