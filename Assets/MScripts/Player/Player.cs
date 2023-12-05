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

    public List<Vector2> Points => tail.Points;

    public Vector2 GetSnakePosition() => snake.transform.position;

    public void RefreshGraph()
    {
        meshOrganiser.RefreshBlobs();
        rewardOrganiser.RefreshRewardsState();
    }
}
