using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class s1_target_roller : Agent
{
    Rigidbody rBody;

    private List<GameObject> prefabList;
    private GameObject Target1;
    private GameObject Target2;
    private GameObject Target3;
    private GameObject Target4;
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
        if(collided_object.gameObject.name == "room" || collided_object.gameObject.name == "reference man")
        {
            GameObject.Destroy(Target1);
            GameObject.Destroy(Target2);
            GameObject.Destroy(Target3);
            GameObject.Destroy(Target4);
            EndEpisode(); // End the episode when the ball collides with the wall
        }
    }

    public override void OnEpisodeBegin()
    {
        // position agent (0,1,-15)
        // position target1 (-5,1,5)
        // position target2 (5,1,5)
        // position target3 (-5,1,-5)
        // position target4 (5,1,-5)

        this.transform.localPosition = new Vector3(0,1,-15); // move the agent back to its original location
        // Select a random prefab from the list
        int index1 = Random.Range(0, prefabList.Count);
        int index2 = Random.Range(0, prefabList.Count);
        int index3 = Random.Range(0, prefabList.Count);
        int index4 = Random.Range(0, prefabList.Count);
        GameObject prefab1 = prefabList[index1];
        GameObject prefab2 = prefabList[index2];
        GameObject prefab3 = prefabList[index3];
        GameObject prefab4 = prefabList[index4];
        // Instantiate the prefab
        Target1 = Instantiate(prefab1, new Vector3(-5,1,5), Quaternion.identity);
        Target2 = Instantiate(prefab2, new Vector3(5,1,5), Quaternion.identity);
        Target3 = Instantiate(prefab3, new Vector3(-5,1,-5), Quaternion.identity);
        Target4 = Instantiate(prefab4, new Vector3(5,1,-5), Quaternion.identity);
        Renderer renderer1 = Target1.GetComponent<Renderer>();
        renderer1.material = material1;
        Renderer renderer2 = Target2.GetComponent<Renderer>();
        renderer2.material = material2;
        Renderer renderer3 = Target3.GetComponent<Renderer>();
        renderer3.material = material3;
        Renderer renderer4 = Target4.GetComponent<Renderer>();
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
        sensor.AddObservation(Target1.transform.localPosition); // target1
        sensor.AddObservation(Target2.transform.localPosition); // target2
        sensor.AddObservation(Target3.transform.localPosition); // target3
        sensor.AddObservation(Target4.transform.localPosition); // target4
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
        float distanceToTarget1 = Vector3.Distance(this.transform.localPosition, Target1.transform.localPosition); //calcualtes the distance between these 2 positions
        float distanceToTarget2 = Vector3.Distance(this.transform.localPosition, Target2.transform.localPosition); //calcualtes the distance between these 2 positions
        float distanceToTarget3 = Vector3.Distance(this.transform.localPosition, Target3.transform.localPosition); //calcualtes the distance between these 2 positions
        float distanceToTarget4 = Vector3.Distance(this.transform.localPosition, Target4.transform.localPosition); //calcualtes the distance between these 2 positions

        // Reached target
        float lengthUnitCube = 1.42f; // The value 1.42f appears to be the square root of 2, which suggests that it's the distance between two points on a diagonal.
        float scaleTarget = 1; // change this based on the scale you are using in unity
        float reachedDistance = lengthUnitCube*scaleTarget;

        if (distanceToTarget1 < reachedDistance || distanceToTarget2 < reachedDistance || distanceToTarget3 < reachedDistance || distanceToTarget4 < reachedDistance)
        {
            SetReward(1.0f); // SetReward is generally used when you finalise the task (before EndEpisode)
             // you can instead use AddReward() but this is used more for intermediate task, for example, need hit the target 5 times in a row before considerered complete, then maybe for loop then AddReward(0.2f), then SetReward(1.0f) then finally EndEpisode()
            GameObject.Destroy(Target1);
            GameObject.Destroy(Target2);
            GameObject.Destroy(Target3);
            GameObject.Destroy(Target4);
            EndEpisode(); // end of task
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            GameObject.Destroy(Target1);
            GameObject.Destroy(Target2);
            GameObject.Destroy(Target3);
            GameObject.Destroy(Target4);
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