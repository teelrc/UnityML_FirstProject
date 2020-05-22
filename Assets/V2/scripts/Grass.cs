using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{

    public Transform[] spawnAreas;

    public void Start()
    {
        transform.localPosition = getRandomPositionInArea(getAgentSpawnArea());
    }

    public void Eaten()
    {
        Debug.Log("Grass Eaten!");
        transform.localPosition = getRandomPositionInArea(getAgentSpawnArea());
    }

    public int getAgentSpawnArea()
    {
        if(spawnAreas!= null)
        {
            return Random.Range(0, spawnAreas.Length);
        }
        else
        {
            return -1;
        }
    }

    public Vector3 getRandomPositionInArea(int areaId)
    {
        if (spawnAreas != null && areaId < spawnAreas.Length && areaId != -1)
        {
            Transform area = spawnAreas[areaId];
            float x_scale = area.localScale.x;
            float z_scale = area.localScale.z;
            Debug.Log("center pos: " + area.localPosition);
            Vector3 rnd = new Vector3((Random.value * x_scale) - (x_scale / 2f),
                                           0.5f,
                                           (Random.value * z_scale) - (z_scale / 2f));
            Debug.Log("dist moved from center: " + rnd);
            return area.localPosition + rnd;
        }
        else
        {
            Debug.Log("bad spawn area ID:" + areaId);
            return Vector3.zero;
        }
    }
}
