using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class CreatureAgent : Agent
{
    Rigidbody rBody;
    public float speed = 1f;
    //public float health = 100.0f;
    //public bool isCollidingWithWall = false;
    [SerializeField] private float grass_score;
    [SerializeField] private float sp1_score;
    [SerializeField] private float sp2_score;
    //private int currentTargetArea = -1;
    private bool agentResetNeeded;
    public Transform[] spawnAreas;
    public GameManager gameManager;

    // Start is called before the first frame update

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        resetAgentPosition();
        agentResetNeeded = true;

    }

    public override void OnEpisodeBegin()
    {
        //base.OnEpisodeBegin();

        //resetTargetPosition();

        if (agentResetNeeded)
        {
            resetAgentPosition();
            agentResetNeeded = false;

        }
        //health = 100;
    }

    public void Eaten()
    {
        Debug.Log("Eaten!");
        AddReward(-1.0f);
        agentResetNeeded = true;
        //health = 0;
        EndEpisode();
    }

    /*
    public void resetTargetPosition()
    {
        resetTargetArea();
        target.localPosition = getRandomPositionInArea(currentTargetArea);
        Debug.Log("Set Target to:" + target.localPosition);
    }
    */

    public void resetAgentPosition()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = getRandomPositionInArea(getAgentSpawnArea());
        Debug.Log("Set Agent to:" + transform.localPosition);
    }

    /*
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
    */

    public int getAgentSpawnArea()
    {
        return Random.Range(0, spawnAreas.Length);

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
        if(collision.transform.tag == GetFoodTag())
        {
            if (collision.transform.tag == "Species1")
            {
                CreatureAgent targetAgent = collision.transform.GetComponent<CreatureAgent>();
                if (targetAgent != null)
                {
                    targetAgent.Eaten();
                }
            }
            if (collision.transform.tag == "Grass")
            {
                Grass grass = collision.transform.GetComponent<Grass>();
                if (grass != null)
                {
                    grass.Eaten();
                }
            }

            AddReward(1.0f);

            //Commenting out EndEpisode as the agents don't concern with consequences of after eating (i.e. other thing eats them)
            //EndEpisode();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Wall")
        {
            //isCollidingWithWall = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        grass_score = gameManager.NormalizedDistanceScore(transform, "Grass");
        sp1_score = gameManager.NormalizedDistanceScore(transform, "Species1");
        sp2_score = gameManager.NormalizedDistanceScore(transform, "Species2");

        sensor.AddObservation(grass_score);
        sensor.AddObservation(sp1_score);
        sensor.AddObservation(sp2_score);

        // Agent velocity
        //sensor.AddObservation(rBody.velocity.x);
        //sensor.AddObservation(rBody.velocity.z);

    }
    
    private string GetFoodTag()
    {
        switch(transform.tag)
        {
            case "Species1":
                return "Grass";
            case "Species2":
                return "Species1";
            default:
                return "";
        }
    }

    private string GetPredatorTag()
    {
        switch (transform.tag)
        {
            case "Species1":
                return "Species2";
            default:
                return "";
        }
    }
    

    public override void OnActionReceived(float[] vectorAction)
    {
        


        AddReward(-(1.0f / MaxStep));

        MoveAgent(vectorAction);
        
        /*
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
            AddReward(1.0f);
            EndEpisode();
        }
        */

        // Fell off platform or starved
        if (this.transform.localPosition.y < 0 || StepCount + 100 >= MaxStep)
        {
            Debug.Log("Starved");
            AddReward(-1.0f);
            agentResetNeeded = true;
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
        //add these so the values show up in heuristic mode
        grass_score = gameManager.NormalizedDistanceScore(transform, "Grass");
        sp1_score = gameManager.NormalizedDistanceScore(transform, "Species1");
        sp2_score = gameManager.NormalizedDistanceScore(transform, "Species2");

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
