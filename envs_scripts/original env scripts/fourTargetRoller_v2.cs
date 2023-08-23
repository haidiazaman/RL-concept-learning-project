using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class fourTargetRoller : Agent
{
    Rigidbody rBody;
    private List<GameObject> prefabList;
    private GameObject Target;
    public Material material;

    // STAGE 0

    public override void Initialize()
    {
        // Load all prefabs from the "Prefabs" folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        // Convert the array to a list
        prefabList = new List<GameObject>(prefabs);
        rBody = GetComponent<Rigidbody>(); //refers to the agent which is the sphere
    }

    // function for checking when agent collides with room or reference man
    void OnCollisionEnter(Collision collided_object)
    {
        if(collided_object.gameObject.name == "room" || collided_object.gameObject.name == "reference man")
        {
            GameObject.Destroy(Target);
            EndEpisode(); // End the episode when the ball collides with the wall
        }
    }

    public override void OnEpisodeBegin()
    {
        Vector3 pos1 = new Vector3(-5,1,5);
        Vector3 pos2 = new Vector3(5,1,5);
        Vector3 pos3 = new Vector3(-5,1,-5);
        Vector3 pos4 = new Vector3(5,1,-5);
    
        this.transform.localPosition = new Vector3(0,1,-15); // move the agent back to its original location
        // Select a random prefab from the list
        int prefab_index = Random.Range(0, prefabList.Count);
        GameObject prefab = prefabList[prefab_index];
        // generate a random number from 1 to 4, number selected will be the position spawned
        int pos_index = Random.Range(1, 5);
        if (pos_index==1)
        {
            Target = Instantiate(prefab, pos1, Quaternion.identity);
        }
        else if (pos_index==2)
        {
            Target = Instantiate(prefab, pos2, Quaternion.identity);
        }
        else if (pos_index==3)
        {
            Target = Instantiate(prefab, pos3, Quaternion.identity);
        }
        else
        {
            Target = Instantiate(prefab, pos4, Quaternion.identity);
        }
        Renderer renderer = Target.GetComponent<Renderer>();
        renderer.material = material;
        material.color = Random.ColorHSV(); // randomise the colours of the cubes at start of every episode

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.GetComponent<Transform>().localPosition); // target1
        sensor.AddObservation(this.transform.localPosition); // agent

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x); // agent
        sensor.AddObservation(rBody.velocity.z);
    }

    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2, 
        Vector3 controlSignal = Vector3.zero; // initialise all zero
        controlSignal.x = actionBuffers.ContinuousActions[0]; // ContinuousActions vs DiscreteActions, DiscreteActions mean its only a few possinble decisions, for e.g. move forward, yes or no? need to make certain number of branches
        controlSignal.z = actionBuffers.ContinuousActions[1]; // ContinuousActions means we can take any number or floating number and apply it to the behaviour for the agent 
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.GetComponent<Transform>().localPosition); //calcualtes the distance between these 2 positions
        // Reached target
        float lengthUnitCube = 1.42f; // The value 1.42f appears to be the square root of 2, which suggests that it's the distance between two points on a diagonal.
        float scaleTarget = 1; // change this based on the scale you are using in unity
        float reachedDistance = lengthUnitCube*scaleTarget;

        if (distanceToTarget < reachedDistance)
        {
            SetReward(1.0f); // SetReward is generally used when you finalise the task (before EndEpisode)
             // you can instead use AddReward() but this is used more for intermediate task, for example, need hit the target 5 times in a row before considerered complete, then maybe for loop then AddReward(0.2f), then SetReward(1.0f) then finally EndEpisode()
            GameObject.Destroy(Target);
            EndEpisode(); // end of task
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            GameObject.Destroy(Target);
            EndEpisode(); // if the object falls off the plane, it has no way to get back on the plane so should EndEpisode here
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Horizontal"); // file > build settings > players settings > input manager > axis
            continuousActionsOut[1] = Input.GetAxis("Vertical");
        }
}