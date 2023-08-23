using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class s1_target_roller : Agent
{
    Rigidbody rBody;
    private List<GameObject> prefabList;
    private GameObject Target;
    private GameObject otherObject1;
    private GameObject otherObject2;
    private GameObject otherObject3;
    public Material material1;
    public Material material2;
    public Material material3;
    public Material material4;

    // STAGE 1

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
        if(collided_object.gameObject.name == "room" || collided_object.gameObject.name == "reference man" || collided_object.gameObject.name == otherObject1.name || collided_object.gameObject.name == otherObject2.name || collided_object.gameObject.name == otherObject3.name)
        {
            GameObject.Destroy(Target);
            GameObject.Destroy(otherObject1);
            GameObject.Destroy(otherObject2);
            GameObject.Destroy(otherObject3);
            EndEpisode(); // End the episode when the ball collides with the wall
        }
    }

    public override void OnEpisodeBegin()
    {
        this.transform.localPosition = new Vector3(0,1,-15); // move the agent back to its original location
        // generate the prefab for the target object first
        int target_prefab_index = Random.Range(0, prefabList.Count);
        GameObject target_prefab = prefabList[target_prefab_index];
        Debug.Log("target prefab is "+target_prefab.name); // print out the name of the target_prefab, might need to save to a json file

        // generate the prefabs for the other objects, need to ensure they are not the same as the target prefab
        // other object 1
        int other_object_prefab_index1 = Random.Range(0, prefabList.Count);
        GameObject other_object_prefab1 = prefabList[other_object_prefab_index1];
        while (other_object_prefab1.name==target_prefab.name)
        {
            other_object_prefab_index1 = Random.Range(0, prefabList.Count);
            other_object_prefab1 = prefabList[other_object_prefab_index1];
        }
        // other object 2
        int other_object_prefab_index2 = Random.Range(0, prefabList.Count);
        GameObject other_object_prefab2 = prefabList[other_object_prefab_index2];
        while (other_object_prefab2.name==target_prefab.name)
        {
            other_object_prefab_index2 = Random.Range(0, prefabList.Count);
            other_object_prefab2 = prefabList[other_object_prefab_index2];
        }
        // other object 3
        int other_object_prefab_index3 = Random.Range(0, prefabList.Count);
        GameObject other_object_prefab3 = prefabList[other_object_prefab_index3];
        while (other_object_prefab3.name==target_prefab.name)
        {
            other_object_prefab_index3 = Random.Range(0, prefabList.Count);
            other_object_prefab3 = prefabList[other_object_prefab_index3];
        }
        // Debug.Log("other object prefab1 is "+other_object_prefab1);
        // Debug.Log("other object prefab2 is "+other_object_prefab2);
        // Debug.Log("other object prefab3 is "+other_object_prefab3);

        GameObject[] prefabs =  { target_prefab, other_object_prefab1, other_object_prefab2, other_object_prefab3 };
        Vector3[] positions = { new Vector3(-5, 1, 5), new Vector3(5, 1, 5), new Vector3(-5, 1, -5), new Vector3(5, 1, -5) };

        // Generate a random permutation of integers from 1 to 4
        List<int> assigned_pos = new List<int> { 1, 2, 3, 4 };
        // Debug.Log(string.Join(",", assigned_pos));
        assigned_pos = assigned_pos.OrderBy( x => Random.value ).ToList();
        // Debug.Log(string.Join(",", assigned_pos));


        // Get the assigned positions for each prefab
        int target_prefab_int = assigned_pos[0];
        int other_object_prefab1_int = assigned_pos[1];
        int other_object_prefab2_int = assigned_pos[2];
        int other_object_prefab3_int = assigned_pos[3];

        // Instantiate the prefabs at their assigned positions
        Target = Instantiate(prefabs[0], positions[target_prefab_int - 1], Quaternion.identity);
        otherObject1 = Instantiate(prefabs[1], positions[other_object_prefab1_int - 1], Quaternion.identity);
        otherObject2 = Instantiate(prefabs[2], positions[other_object_prefab2_int - 1], Quaternion.identity);
        otherObject3 = Instantiate(prefabs[3], positions[other_object_prefab3_int - 1], Quaternion.identity);
        Renderer renderer1 = Target.GetComponent<Renderer>();
        renderer1.material = material1;
        Renderer renderer2 = otherObject1.GetComponent<Renderer>();
        renderer2.material = material2;
        Renderer renderer3 = otherObject2.GetComponent<Renderer>();
        renderer3.material = material3;
        Renderer renderer4 = otherObject3.GetComponent<Renderer>();
        renderer4.material = material4;
        // // randomise the colours of the cubes at start of every episode
        material1.color = Random.ColorHSV();
        material2.color = Random.ColorHSV();
        material3.color = Random.ColorHSV();
        material4.color = Random.ColorHSV();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.GetComponent<Transform>().localPosition); // target
        sensor.AddObservation(otherObject1.GetComponent<Transform>().localPosition); // target
        sensor.AddObservation(otherObject2.GetComponent<Transform>().localPosition); // target
        sensor.AddObservation(otherObject3.GetComponent<Transform>().localPosition); // target
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
            GameObject.Destroy(otherObject1);
            GameObject.Destroy(otherObject2);
            GameObject.Destroy(otherObject3);
            EndEpisode(); // end of task
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            GameObject.Destroy(Target);
            GameObject.Destroy(otherObject1);
            GameObject.Destroy(otherObject2);
            GameObject.Destroy(otherObject3);
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