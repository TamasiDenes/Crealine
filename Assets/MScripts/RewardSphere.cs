using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RewardSphere : MonoBehaviour
{
    [SerializeField] RewardType type;

    RewardOrganiser organiser;

    MeshRenderer mr;
    MeshGenerator generator;

    Color defaultColor;

    // Start is called before the first frame update
    void Start()
    {
        ScoreManager.Instance.MaxScore++;

        organiser = transform.parent.GetComponent<RewardOrganiser>();

        mr = GetComponent<MeshRenderer>();
        defaultColor = mr.material.color;


        switch (type)
        {
            case RewardType.P_01:
                generator = organiser.player01MeshGenerator;
                break;
            case RewardType.P_01_WALL:
                generator = organiser.wallPlayer01MeshGenerator;
                break;
            case RewardType.P_02:
                generator = organiser.player02MeshGenerator;
                break;
            case RewardType.P_02_WALL:
                generator = organiser.wallPlayer02MeshGenerator;
                break;
            case RewardType.P_COMMON:
                generator = organiser.playersMeshGenerator;
                break;
            case RewardType.P_COMMON_WALL:
                generator = organiser.wallPlayersMeshGenerator;
                break;
            default:
                break;
        }
    }

    public void RefreshState()
    {
        if (generator == null)
            return;

        if (generator.ContainsPoint(transform.position))
        {
            Color recentColor = mr.material.color;
            mr.material.color = Color.white;
            if (mr.material.color != recentColor)
            {
                ScoreManager.Instance.Score++;
                FindObjectOfType<AudioManager>().Play("CollectReward");
            }
        }
        else
        {
            Color recentColor = mr.material.color;

            mr.material.color = defaultColor;
            if (mr.material.color != recentColor)
                ScoreManager.Instance.Score--;
        }
    }
}

public enum RewardType
{
    P_01,
    P_01_WALL,
    P_02,
    P_02_WALL,
    P_COMMON,
    P_COMMON_WALL,
}
