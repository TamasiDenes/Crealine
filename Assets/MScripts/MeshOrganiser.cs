using LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshOrganiser : MonoBehaviour
{
    [SerializeField] bool separateMeshes = true;

    [SerializeField] Player player01;
    [SerializeField] Player player02;

    public WallManager wallManager;

    public MeshGenerator player01_Mesh;
    public MeshGenerator player02_Mesh;

    public MeshGenerator wall_player01_Mesh;
    public MeshGenerator wall_player02_Mesh;

    public MeshGenerator players_Mesh;
    public MeshGenerator wall_players_Mesh;

    public MeshGenerator globalMesh;

    MultiGraphEngine graphEngine;

    private List<List<Vector2>> CollectAllPoints()
    {
        List<List<Vector2>> result = new List<List<Vector2>>();

        result.Add(player01.Points);
        result.Add(player02.Points);

        foreach (Wall wall in wallManager.Walls)
        {
            result.Add(wall.Interpolate());
        }

        return result;
    }


    public void RefreshBlobs()
    {
        List<List<Vector2>> allPoints = CollectAllPoints();

        graphEngine = new MultiGraphEngine(allPoints);

        List<List<Vector3>> allBlobs = graphEngine.ProcessGraph();

        if (separateMeshes)
        {
            HandleSeparateMeshes(allBlobs);
        }
        else
        {
            globalMesh.GenerateBlobs(allBlobs);
        }
    }

    private void HandleSeparateMeshes(List<List<Vector3>> allBlobs)
    {
        List<List<Vector3>> player01Blobs = new List<List<Vector3>>();
        List<List<Vector3>> player02Blobs = new List<List<Vector3>>();
        List<List<Vector3>> playersBlobs = new List<List<Vector3>>();

        List<List<Vector3>> wallPlayer01Blobs = new List<List<Vector3>>();
        List<List<Vector3>> wallPlayer02Blobs = new List<List<Vector3>>();
        List<List<Vector3>> wallPlayersBlobs = new List<List<Vector3>>();

        foreach (var blob in allBlobs)
        {
            bool inPlayer01 = false;
            bool inPlayer02 = false;
            bool inWall = false;

            foreach (var point in blob)
            {
                // we can stop if it is in both player and wall
                if (inPlayer01 && inPlayer02 && inWall)
                    continue;

                // check if it is in players and/or walls
                if (!inPlayer01 && player01.Points.Contains(point))
                {
                    inPlayer01 = true;
                }

                if (!inPlayer02 && player02.Points.Contains(point))
                {
                    inPlayer02 = true;
                }

                if (!inWall && wallManager.Walls.Any(wall => wall.ContainsPoint(point)))
                {
                    inWall = true;
                }
            }

            // refresh blob containers based on flags
            if (inWall && inPlayer01 && inPlayer02)
                wallPlayersBlobs.Add(blob);
            else if (inPlayer01 && inPlayer02)
                playersBlobs.Add(blob);
            else if (inPlayer01 && inWall)
                wallPlayer01Blobs.Add(blob);
            else if (inPlayer02 && inWall)
                wallPlayer02Blobs.Add(blob);
            else if (inPlayer01)
                player01Blobs.Add(blob);
            else if (inPlayer02)
                player02Blobs.Add(blob);
        }

        player01_Mesh.GenerateBlobs(player01Blobs);
        player02_Mesh.GenerateBlobs(player02Blobs);
        players_Mesh.GenerateBlobs(playersBlobs);

        wall_player01_Mesh.GenerateBlobs(wallPlayer01Blobs);
        wall_player02_Mesh.GenerateBlobs(wallPlayer02Blobs);
        wall_players_Mesh.GenerateBlobs(wallPlayersBlobs);
    }

}
