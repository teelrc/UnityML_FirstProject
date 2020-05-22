using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RollerAgent : Agent
{
    Rigidbody rBody;
    public Transform target;
    public float speed = 0.001f;
    public float health = 100.0f;
    public bool isCollidingWithWall = false;
    [SerializeField] private float dist_score;
    private int currentTargetArea = -1;
    public Transform[] spawnAreas;

    // Start is called before the first frame update

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        resetAgentPosition();
    }

    public override void OnEpisodeBegin()
    {
        //base.OnEpisodeBegin();

        resetTargetPosition();

        if (transform.localPosition.y < 0 || health<0)
        {
            resetAgentPosition();
            

        }
        health = 100;
    }

    public void Eaten()
    {
        Debug.Log("Eaten!");
        AddReward(-1.0f);
        health = 0;
        EndEpisode();
    }

    public void resetTargetPosition()
    {
        resetTargetArea();
        target.localPosition = getRandomPositionInArea(currentTargetArea);
        Debug.Log("Set Target to:" + target.localPosition);
    }

    public void resetAgentPosition()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = getRandomPositionInArea(getAgentSpawnArea());
        Debug.Log("Set Agent to:" + transform.localPosition);
    }

    public void resetTargetArea()
    {
        if(currentTargetArea == -1)
        {
            currentTargetArea = Random.Range(0, spawnAreas.Length);
        }
        else
        {
            currentTargetArea = (currentTargetArea + Random.Range(1, spawnAreas.Length)) % spawnAreas.Length;
        }
    }

    public int getAgentSpawnArea()
    {
        if (currentTargetArea == -1)
        {
            return Random.Range(0, spawnAreas.Length);
        }
        else
        {
            return (currentTargetArea + Random.Range(1, spawnAreas.Length) ) % spawnAreas.Length;
        }
    }

    public Vector3 getRandomPositionInArea(int areaId)
    {
        if(spawnAreas != null && areaId < spawnAreas.Length)
        {
            Transform area = spawnAreas[areaId];
            float x_scale = area.localScale.x;
            float z_scale = area.localScale.z;
            Debug.Log("center pos: " + area.localPosition);
            Vector3 rnd = new Vector3((Random.value * x_scale) - (x_scale/ 2f),
                                           0.5f,
                                           (Random.value * z_scale) - (z_scale/ 2f));
            Debug.Log("dist moved from center: " + rnd);
            return area.localPosition + rnd;
        }
        else
        {
            Debug.Log("bad spawn area ID:" + areaId);
            return Vector3.zero;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Wall")
        {
            isCollidingWithWall = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Wall")
        {
            isCollidingWithWall = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // distance to target
        dist_score = Vector3.Distance(transform.localPosition, target.transform.localPosition) / 142f;
        sensor.AddObservation(dist_score);

        sensor.AddObservation(1.0f);
        sensor.AddObservation(1.0f);

        // Agent velocity
        //sensor.AddObservation(rBody.velocity.x);
        //sensor.AddObservation(rBody.velocity.z);

    }


    public override void OnActionReceived(float[] vectorAction)
    {
        //base.OnActionReceived(vectorAction);
        dist_score = Vector3.Distance(transform.localPosition, target.transform.localPosition) / 142f;

        AddReward(-(1.0f / MaxStep));

        //health -= 0.01f;

        MoveAgent(vectorAction);

        if (isCollidingWithWall)
        {
            //AddReward(-0.5f );
        }

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, target.localPosition);
        
        // Todo: Make this more scalable (e.g. use colliders)
        if (distanceToTarget < (1.42f * transform.localScale.x))
        {
            if (target.gameObject.tag == "Species1")
            {
                RollerAgent targetAgent = target.gameObject.GetComponent<RollerAgent>();
                if(targetAgent != null)
                {
                    targetAgent.Eaten();
                }
            }

            health = 100;
            AddReward(1.0f);
            EndEpisode();
        }

        // Fell off platform or starved
        if (this.transform.localPosition.y < 0 )
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action0 = Mathf.FloorToInt(act[0]);
        var action1 = Mathf.FloorToInt(act[1]);
        switch (action0)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
        }

        switch (action1)
        {
            case 1:
                rotateDir = transform.up * 1f;
                break;
            case 2:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        rBody.AddForce(dirToGo * speed, ForceMode.VelocityChange);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        actionsOut[1] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = 2;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }
    }

}
