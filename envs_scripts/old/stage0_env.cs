using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// STAGE 0

public class stage0_env : Agent
{
    private Rigidbody rBody;
    private List<GameObject> prefabList;
    private List<string> prefabNames;
    private GameObject Target;
    public Material material;
    private string target_name;
    private int currentPrefabIndex;

    public override void Initialize()
    {
        // Load all prefabs from the "Prefabs" folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        // Convert the array to a list
        prefabList = new List<GameObject>(prefabs);
        rBody = GetComponent<Rigidbody>(); //refers to the agent which is the sphere

        prefabNames = new List<string>();
        for (int i=0; i<prefabList.Count;i++)
        {
            prefabNames.Add(prefabList[i].name);
        }
    }

    public override void OnEpisodeBegin()
    {
        if (Target != null) // Destroy the old target, if any, need this because i am setting a max step in the scene
        {
            Destroy(Target);
        }

        this.transform.localPosition = new Vector3(0,0.5f,-15); // move the agent back to its original location
        this.transform.localRotation = Quaternion.identity; // reset the rotation of the agent
        rBody.velocity = Vector3.zero;

        int prefab_index = Random.Range(0, prefabList.Count);
        GameObject prefab = prefabList[prefab_index];
        Vector3[] positions = {new Vector3(-5,1,5),new Vector3(5,1,5),new Vector3(-5,1,-5),new Vector3(5,1,-5)}; 

        int random_pos = Random.Range(0,4);
        Target = Instantiate(prefab, positions[random_pos], Quaternion.identity);
        Renderer renderer = Target.GetComponent<Renderer>();
        renderer.material = material;
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        material.color = new Color(r, g, b); // randomise the colours of the cubes at start of every episode
        target_name = Target.name.Substring(0,Target.name.Length-7);
        currentPrefabIndex = prefabNames.IndexOf(target_name);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddOneHotObservation((int)currentPrefabIndex,prefabNames.Count);
        sensor.AddObservation(currentPrefabIndex);
    }

    public float movementSpeed = 10;
    public float rotationSpeed = 50;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        float moveForward = actionBuffers.DiscreteActions[0]; 
        float moveBackward = actionBuffers.DiscreteActions[1];
        float rotateRight = actionBuffers.DiscreteActions[2];
        float rotateLeft = actionBuffers.DiscreteActions[3];
        float move = (moveForward-moveBackward) * movementSpeed * Time.deltaTime;
        float rotate = (rotateRight-rotateLeft) * rotationSpeed * Time.deltaTime;

        Vector3 movement = rBody.transform.forward * move;
        Vector3 current_position = rBody.transform.localPosition;
        Vector3 new_position = current_position + movement;
        float distance = Vector3.Distance(new_position,new Vector3(0,0,19));

        if  (distance < 2) // if next action will enter the radius of ref man, then dont take action, rotation is fine
        {
        }
        else
        {
            rBody.MovePosition(rBody.position + movement);
            rBody.transform.Rotate(0, rotate, 0);
        }

        // this entire block is activated when the OnCollisionEnter block fails, and the agent can still somehow  break through the wall
        if (current_position.x <-14f) // hit the left wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x+3,current_position.y,current_position.z);
        }
        else if (current_position.x > 14f) // hit the right wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x-3,current_position.y,current_position.z);
        }
        else if (current_position.z < -19f) // hit the back wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z+3);
        }
        else if (current_position.z > 19f) // hit the front wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z-3);
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Mathf.RoundToInt(Input.GetAxisRaw("move forward"));
        discreteActionsOut[1] = Mathf.RoundToInt(Input.GetAxisRaw("move backward"));
        discreteActionsOut[2] = Mathf.RoundToInt(Input.GetAxisRaw("rotate right"));
        discreteActionsOut[3] = Mathf.RoundToInt(Input.GetAxisRaw("rotate left"));
    }


    // reward function for when agent collides with reference man, wall or target
    void OnCollisionEnter(Collision collided_object)
    {
        if(collided_object.gameObject.name == "wall")
        // if(collided_object.gameObject.name == "reference man")
        {
            SetReward(-1.0f);
        }

        else if (collided_object.gameObject.name == Target.name)
        {
            SetReward(10.0f);
            // GameObject.Destroy(Target);
            EndEpisode();
        }
    }
}



    // enum ItemType {capsule,cube,cylinder,sphere};
    // const int NUM_ITEM_TYPES = (int)ItemType.sphere + 1;

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     // The first argument is the selection index; the second is the
    //     // number of possibilities

    //     sensor.AddOneHotObservation((string)Target.name, NUM_ITEM_TYPES);
    // }

    // public override void CollectObservations(VectorSensor sensor)
    // {

    //     sensor.AddOneHotObservation(currentPrefabIndex,prefabNames.Count);
    // }

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     // float random_num=Random.Range(0,5);
    //     // sensor.AddObservation(random_num);
    //     if (Target != null)
    //     {
    //         Debug.Log("not null");
    //         sensor.AddObservation(Target.GetComponent<Transform>().localPosition);
    //     }
    //     else
    //     {
    //         // Handle the case when the Target object is not assigned or null
    //         // You can add default or placeholder observations in this case
    //         Debug.Log("is null");
    //         sensor.AddObservation(Vector3.zero); // Example: Add a default observation
    //     }
    // }

    // enum ItemType{item for item in prefabList};
    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     // sensor.AddObservation(Target.transform.localPosition);
    //     // float random_num = Random.Range(0,5);
    //     // sensor.AddObservation(random_num);
    //     // // sensor.AddObservation(Target.transform.localRotation);
    //     // sensor.AddOneHotObservation
    // }

    // public float movementSpeed = 10;
    // public float rotationSpeed = 40;
    // public override void OnActionReceived(ActionBuffers actionBuffers)
    // {
    //     float move = actionBuffers.ContinuousActions[0] * movementSpeed * Time.deltaTime;
    //     float rotate = actionBuffers.ContinuousActions[1] * rotationSpeed * Time.deltaTime;
    //     rBody.transform.Translate(0,0,move);
    //     rBody.transform.Rotate(0,rotate,0);
        // // Rewards
        // float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.GetComponent<Transform>().localPosition); //calcualtes the distance between these 2 positions
        // // Reached target
        // float lengthUnitCube = 1.42f; // The value 1.42f appears to be the square root of 2, which suggests that it's the distance between two points on a diagonal.
        // float scaleTarget = 1; // change this based on the scale you are using in unity
        // float reachedDistance = lengthUnitCube*scaleTarget;

        // if (distanceToTarget < reachedDistance)
        // {
        //     SetReward(1.0f); // SetReward is generally used when you finalise the task (before EndEpisode)
        //      // you can instead use AddReward() but this is used more for intermediate task, for example, need hit the target 5 times in a row before considerered complete, then maybe for loop then AddReward(0.2f), then SetReward(1.0f) then finally EndEpisode()
        //     GameObject.Destroy(Target);
        //     EndEpisode(); // end of task
        // }

        // // Fell off platform
        // else if (this.transform.localPosition.y < 0)
        // {
        //     GameObject.Destroy(Target);
        //     EndEpisode(); // if the object falls off the plane, it has no way to get back on the plane so should EndEpisode here
        // }
    // }

    // public Transform cameraParent; // drag the "CameraParent" object here in the Inspector

    // void LateUpdate()
    // {
    //     cameraParent.position = rBody.transform.position;
    //     cameraParent.rotation = Quaternion.Euler(0, rBody.transform.rotation.y, 0);
    // }

// }

        // Select a random prefab from the list

        // // generate a random number from 1 to 4, number selected will be the position spawned
        // int pos_index = Random.Range(1, 5);
        // if (pos_index==1)
        // {
        //     Target = Instantiate(prefab, pos1, Quaternion.identity);
        // }
        // else if (pos_index==2)
        // {
        //     Target = Instantiate(prefab, pos2, Quaternion.identity);
        // }
        // else if (pos_index==3)
        // {
        //     Target = Instantiate(prefab, pos3, Quaternion.identity);
        // }
        // else
        // {
        //     Target = Instantiate(prefab, pos4, Quaternion.identity);
        // }

            // continuousActionsOut[0] = Input.GetAxis("Horizontal"); // move left/right
            // continuousActionsOut[1] = Input.GetAxis("Vertical"); // move forward/backward
            // continuousActionsOut[2] = Input.GetAxis("Rotation"); // rotate around y axis
        // Target and Agent positions coordinates
        // sensor.AddObservation(Target.GetComponent<Transform>().localPosition); // target1
        // sensor.AddObservation(this.transform.localPosition); // agent

        // Agent velocity
        // sensor.AddObservation(rBody.velocity.x); // agent
        // sensor.AddObservation(rBody.velocity.z);
        // sensor.AddObservation(this.transform.rotation.eulerAngles.y); // agent rotation

        // Actions, size = 2
        // left right forward backward movement
        // Vector3 controlSignal = Vector3.zero; // initialise all zero
        // controlSignal.x = actionBuffers.ContinuousActions[0]; // ContinuousActions vs DiscreteActions, DiscreteActions mean its only a few possinble decisions, for e.g. move forward, yes or no? need to make certain number of branches
        // controlSignal.z = actionBuffers.ContinuousActions[1]; // ContinuousActions means we can take any number or floating number and apply it to the behaviour for the agent 
        // rBody.AddForce(controlSignal * forceMultiplier);
        // // rotate clockwise anticlockwise movement
        // float rotationAction = actionBuffers.ContinuousActions[2]; 
        // rBody.transform.Rotate(0,rotationAction*rotateMultiplier,0);


        // Vector3 pos1 = new Vector3(-5,1,5);
        // Vector3 pos2 = new Vector3(5,1,5);
        // Vector3 pos3 = new Vector3(-5,1,-5);
        // Vector3 pos4 = new Vector3(5,1,-5);
        // Vector3[] positions = {pos1,pos2,pos3,pos4}; 

        // // add observation of agent coordinates so its knows where it is in the envt and the rotation of the agent
        // sensor.AddObservation(this.transform.localPosition); // agent
        // sensor.AddObservation(this.transform.rotation.eulerAngles.y); // agent rotation about y axis
    
        // renderTexture = new RenderTexture(textureWidth, textureHeight, 32); // 32pixels  in total 8x4 R8G8B8A8, means each channel RGBA has 8 pixel values
        // agentCamera.targetTexture = renderTexture;
        // texture2D = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false); // RGBA32 since using R8G8B8A8, false sets mipmaps to false; mipmaps use up more resources
        // public override void CollectObservations(VectorSensor sensor)

    // public Camera agentCamera;
    // private int textureWidth = 512;
    // private int textureHeight = 512;
    // private RenderTexture renderTexture;
    // private Texture2D texture2D;
    // {
    //     // // add observation of agent camera RGB values
    //     // // Get RGB values from the camera
    //     // RenderTexture.active = renderTexture;
    //     // texture2D.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
    //     // RenderTexture.active = null;
    //     // Color[] pixels = texture2D.GetPixels();
    //     // // Add RGB values as observations, total number of obs = 786432
    //     // foreach (Color pixel in pixels)
    //     // {
    //     //     sensor.AddObservation(pixel.r);
    //     //     sensor.AddObservation(pixel.g);
    //     //     sensor.AddObservation(pixel.b);
    //     // }
    //     sensor.AddObservation(Target.transform.localPosition);
    //     sensor.AddObservation(this.transform.localPosition);
    // }

    // public float movementForce = 1;
    // public float rotationTorque = 0.75f;

        // float move = actionBuffers.ContinuousActions[0];
        // Vector3 movement = new Vector3(0, 0, move * movementForce);
        // rBody.AddForce(rBody.transform.rotation * movement);

        // float rotate = actionBuffers.ContinuousActions[1];
        // Vector3 rotation = new Vector3(0, rotate * rotationTorque, 0);
        // rBody.AddTorque(rotation);

        // var continuousActionsOut = actionsOut.ContinuousActions;
        // // file > build settings > players settings > input manager > axis
        // continuousActionsOut[0] = Input.GetAxis("Movement"); // movement
        // continuousActionsOut[1] = Input.GetAxis("Rotation"); // rotation

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var discreteActionsOut = actionsOut.DiscreteActions;
    //     discreteActionsOut[0] = Input.GetAxis("Movement"); // movement
    //     discreteActionsOut[1] = Input.GetAxis("Rotation"); // rotation        


    // }

        // float move = actionBuffers.DiscreteActions[0] * movementSpeed * Time.deltaTime;
        // float rotate = actionBuffers.DiscreteActions[1] * rotationSpeed * Time.deltaTime;
        // rBody.transform.Translate(0,0,move);
        // rBody.transform.Rotate(0,rotate,0);


    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var discreteActionsOut = actionsOut.DiscreteActions;
    //     discreteActionsOut[0] = Mathf.RoundToInt(Input.GetAxisRaw("Movement"));
    //     discreteActionsOut[1] = Mathf.RoundToInt(Input.GetAxisRaw("Rotation"));
    // }

    // public float movementSpeed = 10;
    // public float rotationSpeed = 40;
    // public override void OnActionReceived(ActionBuffers actionBuffers)
    // {

    //     float move = actionBuffers.DiscreteActions[0] * movementSpeed * Time.deltaTime;
    //     float rotate = actionBuffers.DiscreteActions[1] * rotationSpeed * Time.deltaTime;
    //     Vector3 movement = rBody.transform.forward * move;
    //     rBody.MovePosition(rBody.position + movement);
    //     rBody.transform.Rotate(0, rotate, 0);
    // }