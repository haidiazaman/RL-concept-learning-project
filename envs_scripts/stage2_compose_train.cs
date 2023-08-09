using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// STAGE 0

public class stage2_compose_train : Agent
{
    private GameObject TargetA;
    private GameObject TargetB;
    private GameObject otherObject1A;
    private GameObject otherObject1B;
    private GameObject otherObject2A;
    private GameObject otherObject2B;
    private GameObject otherObject3A;
    private GameObject otherObject3B;
    private Rigidbody rBody;
    private List<GameObject> prefabList;
    private List<Color> colorList;
    private List<AttributeTuple> trainAttributesList;
    private List<AttributeTuple> testAttributesList;
    private List<PositionTuple> positionsList;
    public Material material1;
    public Material material2;
    public Material material3;
    public Material material4;
    public float objectsScaleFactor=1.5f;
    public float leftRightLimits=11f;
    public float frontBackLimits=16f;
    public float bounceBackDistance=3f;
    public float movementSpeed = 50;
    public float rotationSpeed = 200;
    private int targetAttributesIndex;
    private int targetColorIndex;
    private int targetPrefabIndex1;
    private int targetPrefabIndex2;

    // define a class to hold color and pair of gameobjects tgt
    public class AttributeTuple
    {
        public Color color;
        public GameObject obj1;
        public GameObject obj2;

        public AttributeTuple(GameObject obj1,GameObject obj2,Color color)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
            this.color = color;
        }
    }

    public class PositionTuple
    {
        public Vector3 pos1;
        public Vector3 pos2;

        public PositionTuple(Vector3 pos1,Vector3 pos2)
        {
            this.pos1 = pos1;
            this.pos2 = pos2;
        }
    }

    public override void Initialize()
    {
        //get the RB component so we can access in the script. refers to the agent
        rBody = GetComponent<Rigidbody>(); 

        // Load all prefabs from the "Prefabs" folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        prefabList = prefabs.OrderBy(prefab => prefab.name).ToList(); 
        // 0-capsule,1-cube,2-cylinder,3-prism,4-sphere

        //initialise colours to use
        colorList = new List<Color>(new Color[] {new Color(1,0,0),new Color(0,1,0),new Color(0,0,1),new Color(1, 0.92f, 0.016f),new Color(0,0,0)});
        // 0-red,1-green,2-blue,3-yellow,4-black

        // hard code the unique test triplets as planned in train test split
        testAttributesList = new List<AttributeTuple> // 10 combinations
        {
            new AttributeTuple(prefabList[0],prefabList[1],colorList[0]),
            new AttributeTuple(prefabList[0],prefabList[2],colorList[0]),
            new AttributeTuple(prefabList[1],prefabList[2],colorList[1]),
            new AttributeTuple(prefabList[1],prefabList[3],colorList[1]),
            new AttributeTuple(prefabList[2],prefabList[3],colorList[2]),
            new AttributeTuple(prefabList[2],prefabList[4],colorList[2]),
            new AttributeTuple(prefabList[0],prefabList[3],colorList[3]),
            new AttributeTuple(prefabList[3],prefabList[4],colorList[3]),
            new AttributeTuple(prefabList[0],prefabList[4],colorList[4]),
            new AttributeTuple(prefabList[1],prefabList[4],colorList[4]),
        };

        // loop thru to get all possible combinations and then dont add those triplets alr in test
        trainAttributesList = new List<AttributeTuple>{}; 
        for (int i=0; i<colorList.Count; i++)
        {
            for (int j = 0; j < prefabList.Count; j++)
            {
                for (int k = j + 1; k < prefabList.Count; k++)
                {
                    AttributeTuple triplet = new AttributeTuple(prefabList[j], prefabList[k],colorList[i]);
                    // this doesnt work so need to hard code the 10 triplets in test - if (!testAttributesList.Contains(triplet)) // Check both lists 
                    if (
                        j==0 && k==1 && i==0 ||
                        j==0 && k==2 && i==0 ||
                        j==1 && k==2 && i==1 ||
                        j==1 && k==3 && i==1 ||
                        j==2 && k==3 && i==2 ||
                        j==2 && k==4 && i==2 ||
                        j==0 && k==3 && i==3 ||
                        j==3 && k==4 && i==3 ||
                        j==0 && k==4 && i==4 ||
                        j==1 && k==4 && i==4 
                        )                    
                    {
                        // Debug.Log(testAttributeSet.Contains(triplet));
                        continue;
                    }
                    else
                    {
                        trainAttributesList.Add(triplet);
                    }
                }
            }
        }
        // Debug.Log(trainAttributesList.Count);
        // Debug.Log(testAttributesList.Count);
    }

    public override void OnEpisodeBegin()
    {
        // destroy any remaining objects at start of episode - reqd if agent hits max steps in prev eps
        List<GameObject> previousGameObjects = new List<GameObject>{TargetA,TargetB,otherObject1A,otherObject1B,otherObject2A,otherObject2B,otherObject3A,otherObject3B};
        foreach (GameObject previousGO in previousGameObjects)
        {
            if (previousGO != null)
            {
                Destroy(previousGO);
            }
        }

        // reset agent position and rotation
        this.transform.localPosition = new Vector3(0,1,-15); // move the agent back to its original location
        this.transform.localRotation = Quaternion.identity; // make sure the rotation of the agent is also reset at the beginning of each episode

        // select AttributeTuple for target
        targetAttributesIndex = Random.Range(0,trainAttributesList.Count);

        // select AttributeTuple for other objects
        int otherObject1_AttributesIndex = Random.Range(0,trainAttributesList.Count);
        while (otherObject1_AttributesIndex==targetAttributesIndex)
        {
            otherObject1_AttributesIndex = Random.Range(0,trainAttributesList.Count);
        }
        int otherObject2_AttributesIndex = Random.Range(0,trainAttributesList.Count);
        while (otherObject2_AttributesIndex==targetAttributesIndex)
        {
            otherObject2_AttributesIndex = Random.Range(0,trainAttributesList.Count);
        }
        int otherObject3_AttributesIndex = Random.Range(0,trainAttributesList.Count);
        while (otherObject3_AttributesIndex==targetAttributesIndex)
        {
            otherObject3_AttributesIndex = Random.Range(0,trainAttributesList.Count);
        }

        Debug.Log("target index: " + targetAttributesIndex);
        Debug.Log(otherObject1_AttributesIndex);
        Debug.Log(otherObject2_AttributesIndex);
        Debug.Log(otherObject3_AttributesIndex);

        // call the attributetuple for all 4 objects-pairs
        AttributeTuple targetAttributes = trainAttributesList[targetAttributesIndex];
        AttributeTuple otherObject1_Attributes = trainAttributesList[otherObject1_AttributesIndex];
        AttributeTuple otherObject2_Attributes = trainAttributesList[otherObject2_AttributesIndex];
        AttributeTuple otherObject3_Attributes = trainAttributesList[otherObject3_AttributesIndex];

        // list down the all the pairs of positions 
        positionsList = new List<PositionTuple>{
            new PositionTuple(new Vector3(-6,objectsScaleFactor/2,-5), new Vector3(-4,objectsScaleFactor/2,-5)),
            new PositionTuple(new Vector3(-4,objectsScaleFactor/2,5), new Vector3(-2,objectsScaleFactor/2,5)),
            new PositionTuple(new Vector3(2,objectsScaleFactor/2,5), new Vector3(4,objectsScaleFactor/2,5)),
            new PositionTuple(new Vector3(4,objectsScaleFactor/2,-5), new Vector3(6,objectsScaleFactor/2,-5)),
        };
        
        // set random values to randomise the positions of gameobjects later
        List<int> assigned_pos = new List<int> {0, 1, 2, 3}; //also make sure the layout of our environment is the same like that
        assigned_pos = assigned_pos.OrderBy( x => Random.value ).ToList();

        // Instantiate the prefabs at their assigned positions
            // targetAttributes.obj1 gives ObjectA, targetAttributes.obj2 gives ObjectB
            // positiontuple.pos1 gives pos1, positiontuple.pos2 gives pos2
        TargetA = Instantiate(targetAttributes.obj1, positionsList[assigned_pos[0]].pos1, Quaternion.identity);
        TargetB = Instantiate(targetAttributes.obj2, positionsList[assigned_pos[0]].pos2, Quaternion.identity);
        otherObject1A = Instantiate(otherObject1_Attributes.obj1, positionsList[assigned_pos[1]].pos1, Quaternion.identity);
        otherObject1B = Instantiate(otherObject1_Attributes.obj2, positionsList[assigned_pos[1]].pos2, Quaternion.identity);
        otherObject2A = Instantiate(otherObject2_Attributes.obj1, positionsList[assigned_pos[2]].pos1, Quaternion.identity);
        otherObject2B = Instantiate(otherObject2_Attributes.obj2, positionsList[assigned_pos[2]].pos2, Quaternion.identity);
        otherObject3A = Instantiate(otherObject3_Attributes.obj1, positionsList[assigned_pos[3]].pos1, Quaternion.identity);
        otherObject3B = Instantiate(otherObject3_Attributes.obj2, positionsList[assigned_pos[3]].pos2, Quaternion.identity);

        // adjust scale of all 8 objects accordingly
        TargetA.transform.localScale = Vector3.one * objectsScaleFactor;
        TargetB.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject1A.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject1B.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject2A.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject2B.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject3A.transform.localScale = Vector3.one * objectsScaleFactor;
        otherObject3B.transform.localScale = Vector3.one * objectsScaleFactor;

        // get the materials for objects 
        Renderer renderer1 = TargetA.GetComponent<Renderer>();
        renderer1.material = material1;
        Renderer renderer2 = TargetB.GetComponent<Renderer>();
        renderer2.material = material1;
        Renderer renderer3 = otherObject1A.GetComponent<Renderer>();
        renderer3.material = material2;
        Renderer renderer4 = otherObject1B.GetComponent<Renderer>();
        renderer4.material = material2;
        Renderer renderer5 = otherObject2A.GetComponent<Renderer>();
        renderer5.material = material3;
        Renderer renderer6 = otherObject2B.GetComponent<Renderer>();
        renderer6.material = material3;
        Renderer renderer7 = otherObject3A.GetComponent<Renderer>();
        renderer7.material = material3;
        Renderer renderer8 = otherObject3B.GetComponent<Renderer>();
        renderer8.material = material3;

        // set colours
        material1.color=targetAttributes.color;
        material2.color=otherObject1_Attributes.color;
        material3.color=otherObject2_Attributes.color;
        material4.color=otherObject3_Attributes.color;

        Debug.Log("target color is " + material1.color + ", target prefab are " + TargetA.name + " and " + TargetB.name); // print out the name of the target_prefab, might need to save to a json file

        // move up the capsule and cylinder by the objectsScaleFactor to avoid them being stuck in the floor
        // Get all 4 objects in the scene
        List<GameObject> currentGameObjects = new List<GameObject>{TargetA,TargetB,otherObject1A,otherObject1B,otherObject2A,otherObject2B,otherObject3A,otherObject3B};
        // GameObject[] prefabs_ =  { Target, otherObject1, otherObject2, otherObject3 };
        foreach (GameObject obj in currentGameObjects)
        {
            // Check if the object name starts with "Capsule" or "Cylinder"
            if (obj.name.StartsWith("Capsule") || obj.name.StartsWith("Cylinder"))
            {
                // Set the y coordinate to 3
                Vector3 newPosition = new Vector3(obj.transform.localPosition.x, objectsScaleFactor, obj.transform.localPosition.z);
                obj.transform.localPosition = newPosition;
            }
        }

        // set the targetPrefabIndex1, targetPrefabIndex2 and targetColorIndex to be output in CollectObservations
        for (int i=0;i<prefabList.Count;i++)
        {
            if (targetAttributes.obj1==prefabList[i])
            {
                targetPrefabIndex1=i;
            }
        }

        for (int i=0;i<prefabList.Count;i++)
        {
            if (targetAttributes.obj2==prefabList[i])
            {
                targetPrefabIndex2=i;
            }
        }

        for (int i=0;i<colorList.Count;i++)
        {
            if (targetAttributes.color==colorList[i])
            {
                targetColorIndex=i;
            }        
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(targetPrefabIndex1);
        sensor.AddObservation(targetPrefabIndex2);
        // 0-capsule,1-cube,2-cylinder,3-prism,4-sphere
        sensor.AddObservation(targetColorIndex);
        // 0-red,1-green,2-blue,3-yellow,4-black
        Debug.Log("colour: " + targetColorIndex + ", prefab1: " + targetPrefabIndex1 + ", prefab2: " + targetPrefabIndex2);
    }

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
            SetReward(-1.0f);
            rBody.transform.localPosition = new Vector3(current_position.x+bounceBackDistance,current_position.y,current_position.z);
        }
        else if (current_position.x > leftRightLimits) // hit the right wall
        {
            SetReward(-1.0f);
            rBody.transform.localPosition = new Vector3(current_position.x-bounceBackDistance,current_position.y,current_position.z);
        }
        else if (current_position.z < -frontBackLimits) // hit the back wall
        {
            SetReward(-1.0f);
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z+bounceBackDistance);
        }
        else if (current_position.z > frontBackLimits) // hit the front wall
        {
            SetReward(-1.0f);
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z-bounceBackDistance);
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
        // get color of collided object to check for collision and give appropriate reward and endepisode accordingly
        Renderer renderer = collided_object.gameObject.GetComponent<Renderer>();
        Material collided_object_material = renderer.material;
        Color collided_object_color = collided_object_material.color;

        if (collided_object_color==material1.color && collided_object.gameObject.name == TargetA.name || collided_object_color==material1.color && collided_object.gameObject.name == TargetB.name) // this refers to the target object specifically since only the target is of the target object color
        {
            SetReward(10f); // SetReward is generally used when you finalise the task (before EndEpisode)
             // you can instead use AddReward() but this is used more for intermediate task, for example, need hit the target 5 times in a row before considerered complete, then maybe for loop then AddReward(0.2f), then SetReward(1.0f) then finally EndEpisode()
            GameObject.Destroy(TargetA);
            GameObject.Destroy(TargetB);
            GameObject.Destroy(otherObject1A);
            GameObject.Destroy(otherObject1B);
            GameObject.Destroy(otherObject2A);
            GameObject.Destroy(otherObject2B);
            GameObject.Destroy(otherObject3A);
            GameObject.Destroy(otherObject3B);
            EndEpisode(); // end of task     
        }

        if (collided_object_color!=material1.color && collided_object.gameObject.name != TargetA.name || collided_object_color!=material1.color && collided_object.gameObject.name != TargetB.name)
        // this is for the other objects, need the second part of checking not equal wall name because the wall is also not same color as target, so the first condition alone wont suffice
        {
            SetReward(-3f);
            // Debug.Log(collided_object.gameObject.name);
        }
    }
}