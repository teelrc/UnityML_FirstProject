using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public Transform[] species1Creatures;
    public Transform[] species2Creatures;
    public Transform[] grasses;

    private float maxDist = 142f;

    public float NormalizedDistanceScore(Transform src, string target_tag)
    {
        Transform[] targets = GetTransformsByTag(target_tag);
        float score = 0;
        if (targets != null && targets.Length > 0 )
        {
            foreach(Transform target in targets)
            {
                if(target == null)
                {
                    Debug.Log("bad target in GameManager");
                    return 0;
                }
                //can't be yourself
                if (src.GetInstanceID() != target.GetInstanceID())
                {
                    score += Mathf.Log(Vector3.Distance(src.localPosition, target.localPosition)) / Mathf.Log(maxDist);
                }
                
            }
            score /= targets.Length;
        }
        return score;
    }

    private Transform[] GetTransformsByTag(string value)
    {
        switch (value)
        {
            case "Grass":
                return grasses;
            case "Species1":
                return species1Creatures;
            case "Species2":
                return species2Creatures;
            default:
                return null;
        }
    }
}
