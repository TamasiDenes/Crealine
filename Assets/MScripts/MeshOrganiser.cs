using LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshOrganiser : MonoBehaviour
{
    public Player player01;
    public Player player02;

    public WallManager wallManager;

    public MeshGenerator player01_Mesh;
    public MeshGenerator player02_Mesh;

    public MeshGenerator wall_player01_Mesh;
    public MeshGenerator wall_player02_Mesh;

    public MeshGenerator players_Mesh;
    public MeshGenerator wall_players_Mesh;

    public MeshGenerator globalMesh;

    MultiGraphEngine FullEngine;

    public bool separeteMeshes;


    internal void RefreshBlobs()
    {
        List<List<Vector2>> allPoints = new List<List<Vector2>>();

        allPoints.Add(player01.Points);
        allPoints.Add(player02.Points);

        foreach (Wall wall in wallManager.Walls)
        {
            allPoints.Add(wall.Points);
        }

        FullEngine = new MultiGraphEngine(allPoints);

        List<List<Vector3>> allBlobs = FullEngine.ProcessGraph();

        if (!separeteMeshes)
        {
            globalMesh.GenerateBlobs(allBlobs);
        }
        else
        {
            List<List<Vector3>> player01Blobs = new List<List<Vector3>>();
            List<List<Vector3>> player02Blobs = new List<List<Vector3>>();
            List<List<Vector3>> playersBlobs = new List<List<Vector3>>();

            List<List<Vector3>> wallPlayer01Blobs = new List<List<Vector3>>();
            List<List<Vector3>> wallPlayer02Blobs = new List<List<Vector3>>();
            List<List<Vector3>> wallPlayersBlobs = new List<List<Vector3>>();

            // blobokon végig megyek
            foreach (var blob in allBlobs)
            {
                bool inPlayer01 = false;
                bool inPlayer02 = false;
                bool inWall = false;
                // minden elemén végig megyek
                foreach (var point in blob)
                {
                    // ha kiderült hogy mindháromban benne van - nincs értelme tovább nézelõdni
                    if (inPlayer01 && inPlayer02 && inWall)
                        continue;

                    // adott elem amelyik MeshGenerator elemeibe benne van -> ahhoz felveszem mint blob
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

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
