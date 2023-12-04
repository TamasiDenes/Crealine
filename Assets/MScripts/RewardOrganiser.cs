using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RewardOrganiser : MonoBehaviour
{
    public MeshOrganiser meshOrganiser;

    public MeshGenerator player01MeshGenerator => meshOrganiser.player01_Mesh;
    public MeshGenerator wallPlayer01MeshGenerator => meshOrganiser.wall_player01_Mesh;
    public MeshGenerator player02MeshGenerator => meshOrganiser.player02_Mesh;
    public MeshGenerator wallPlayer02MeshGenerator => meshOrganiser.wall_player02_Mesh;

    public MeshGenerator playersMeshGenerator => meshOrganiser.players_Mesh;
    public MeshGenerator wallPlayersMeshGenerator => meshOrganiser.wall_players_Mesh;

    public void RefreshRewardsState()
    {
        foreach (RewardSphere item in transform.gameObject.GetComponentsInChildren<RewardSphere>())
        {
            item.RefreshState();
        }
    }
}
