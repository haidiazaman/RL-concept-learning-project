using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// STAGE 0

public class stage1a : Agent
{
    private GameObject Target;
    private GameObject otherObject1;
    private GameObject otherObject2;
    private GameObject otherObject3;
    private Rigidbody rBody;
    private List<GameObject> prefabList;
    // private List<string> prefabNames;
    private List<Color> colorList;
    public Material material1;
    public Material material2;
    public Material material3;
    public Material material4;
    // private string target_name;
    // private int currentPrefabIndex;
    public float objectsScaleFactor=1.0f;
    public float leftRightLimits=14f;
    public float frontBackLimits=18f;
    public float bounceBackDistance=2f;
    private int target_prefab_index;

    public override void Initialize()
    {
        // Load all prefabs from the "Prefabs" folder
        // GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        prefabList = prefabs.OrderBy(prefab => prefab.name).ToList();
        // Debug.Log(prefabList[0].name);
        // Debug.Log(prefabList[1].name);
        // Debug.Log(prefabList[2].name);
        // Debug.Log(prefabList[3].name);
        // Debug.Log(prefabList[4].name);

        // Convert the array to a list
        // prefabList = new List<GameObject>(prefabs);
        // initialise an array and add names of the prefabs to it
        // prefabNames = new List<string>();
        // for (int i=0; i<prefabList.Count;i++)
        // {
        //     prefabNames.Add(prefabList[i].name);
        // }

        //get the RB component so we can access in the script. refers to the agent
        rBody = GetComponent<Rigidbody>(); 

        //initialise colours to use
        colorList = new List<Color>(new Color[] {new Color(1,0,0),new Color(0,1,0),new Color(0,0,1),new Color(1, 0.92f, 0.016f),new Color(0,0,0)});
    }

    public override void OnEpisodeBegin()
    {
        if (Target != null) // Destroy the old target, if any, need this because i am setting a max step in the scene
        {
            Destroy(Target);
        }
        if (otherObject1 != null) 
        {
            Destroy(otherObject1);
        }
        if (otherObject2 != null) 
        {
            Destroy(otherObject2);
        }
        if (otherObject3 != null) 
        {
            Destroy(otherObject3);
        }

        this.transform.localPosition = new Vector3(0,1,-15); // move the agent back to its original location

        // generate the prefab for the target object first
        // int target_prefab_index = Random.Range(0, prefabList.Count);
        target_prefab_index = Random.Range(0, 5);
        GameObject target_prefab = prefabList[target_prefab_index];
        Debug.Log("target prefab is "+target_prefab.name); // print out the name of the target_prefab, might need to save to a json file

        // generate the prefabs for the other objects, need to ensure they are not the same as the target prefab
        // other object 1
        int other_object_prefab_index1 = Random.Range(0, prefabList.Count);
        while (other_object_prefab_index1==target_prefab_index)
        {
            other_object_prefab_index1 = Random.Range(0, prefabList.Count);
        }
        GameObject other_object_prefab1 = prefabList[other_object_prefab_index1];

        int other_object_prefab_index2 = Random.Range(0, prefabList.Count);
        while (other_object_prefab_index2==target_prefab_index)
        {
            other_object_prefab_index2 = Random.Range(0, prefabList.Count);
        }
        GameObject other_object_prefab2 = prefabList[other_object_prefab_index2];

        int other_object_prefab_index3 = Random.Range(0, prefabList.Count);
        while (other_object_prefab_index3==target_prefab_index)
        {
            other_object_prefab_index3 = Random.Range(0, prefabList.Count);
        }
        GameObject other_object_prefab3 = prefabList[other_object_prefab_index3];

        GameObject[] prefabs =  { target_prefab, other_object_prefab1, other_object_prefab2, other_object_prefab3 };
        Vector3[] positions = {new Vector3(-4,1*objectsScaleFactor/2,10),new Vector3(4,1*objectsScaleFactor/2,10),new Vector3(-8,1*objectsScaleFactor/2,1),new Vector3(8,1*objectsScaleFactor/2,1)};  // initialise the 4 fixed positions
        // Vector3[] positions = {new Vector3(-5,1*objectsScaleFactor/2,5),new Vector3(5,1*objectsScaleFactor/2,5),new Vector3(-5,1*objectsScaleFactor/2,-5),new Vector3(5,1*objectsScaleFactor/2,-5)};  // initialise the 4 fixed positions
        List<int> assigned_pos = new List<int> { 1, 2, 3, 4 };
        assigned_pos = assigned_pos.OrderBy( x => Random.value ).ToList();

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

        Target.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject1.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject2.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject3.transform.localScale = Vector3.one * objectsScaleFactor;

        Renderer renderer1 = Target.GetComponent<Renderer>();
        renderer1.material = material1;
        Renderer renderer2 = otherObject1.GetComponent<Renderer>();
        renderer2.material = material2;
        Renderer renderer3 = otherObject2.GetComponent<Renderer>();
        renderer3.material = material3;
        Renderer renderer4 = otherObject3.GetComponent<Renderer>();
        renderer4.material = material4;

        Material[] material_list = {material1,material2,material3,material4};
        for (int i=0;i<4;i++)
        {
            Material current_material = material_list[i];
            int random_color = Random.Range(0,5);
            current_material.color = colorList[random_color];
            // float r = Random.Range(0f, 1f);
            // float g = Random.Range(0f, 1f);
            // float b = Random.Range(0f, 1f);
            // current_material.color = new Color(r, g, b);
        }

        // target_name = Target.name.Substring(0,Target.name.Length-7);
        // currentPrefabIndex = prefabNames.IndexOf(target_name);    
        Debug.Log(target_prefab_index);    

        // move up the capsule and cylinder by the objectsScaleFactor to avoid them being stuck in the floor
        // Get all 4 objects in the scene
        GameObject[] prefabs_ =  { Target, otherObject1, otherObject2, otherObject3 };

        foreach (GameObject obj in prefabs_)
        {
            // Check if the object name starts with "Capsule" or "Cylinder"
            if (obj.name.StartsWith("Capsule") || obj.name.StartsWith("Cylinder"))
            {
                // Set the y coordinate to 3
                Vector3 newPosition = new Vector3(obj.transform.localPosition.x, objectsScaleFactor, obj.transform.localPosition.z);
                obj.transform.localPosition = newPosition;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddOneHotObservation((int)currentPrefabIndex,prefabNames.Count);
        sensor.AddObservation(target_prefab_index);
        // 0-capsule,1-cube,2-cylinder,3-prism,4-sphere
    }

    public float movementSpeed = 150;
    public float rotationSpeed = 200;
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

        if (distance < 2) // if next action will enter the radius of ref man, then dont take action, rotation is fine
        {
            SetReward(-1.0f);
            Vector3 backwardVector = -rBody.transform.forward;
            backwardVector.Normalize();
            Vector3 newPosition = current_position + (backwardVector * bounceBackDistance);
            rBody.MovePosition(newPosition);
        }

        else
        {
            rBody.MovePosition(rBody.position + movement);
            rBody.transform.Rotate(0, rotate, 0);
        }

        // this entire block is activated when the OnCollisionEnter block fails, and the agent can still somehow  break through the wall
        if (current_position.x <-leftRightLimits) // hit the left wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x+bounceBackDistance,current_position.y,current_position.z);
            SetReward(-1.0f);
        }
        else if (current_position.x > leftRightLimits) // hit the right wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x-bounceBackDistance,current_position.y,current_position.z);
            SetReward(-1.0f);
        }
        else if (current_position.z < -frontBackLimits) // hit the back wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z+bounceBackDistance);
            SetReward(-1.0f);
        }
        else if (current_position.z > frontBackLimits) // hit the front wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z-bounceBackDistance);
            SetReward(-1.0f);
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
        if(collided_object.gameObject.name == "wall" || collided_object.gameObject.name == otherObject1.name || collided_object.gameObject.name == otherObject2.name || collided_object.gameObject.name == otherObject3.name)
        {
            SetReward(-2.5f);
            // Debug.Log(collided_object.gameObject.name);
        }

        else if (collided_object.gameObject.name == Target.name)
        {
            SetReward(10f); // SetReward is generally used when you finalise the task (before EndEpisode)
             // you can instead use AddReward() but this is used more for intermediate task, for example, need hit the target 5 times in a row before considerered complete, then maybe for loop then AddReward(0.2f), then SetReward(1.0f) then finally EndEpisode()
            GameObject.Destroy(Target);
            GameObject.Destroy(otherObject1);
            GameObject.Destroy(otherObject2);
            GameObject.Destroy(otherObject3);
            EndEpisode(); // end of task     
        }
    }
}