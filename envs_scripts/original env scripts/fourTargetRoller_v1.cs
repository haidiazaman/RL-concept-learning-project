using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class fourTargetRoller : Agent
{
    Rigidbody rBody;
    void Start () {
        rBody = GetComponent<Rigidbody>(); //refers to the agent which is the sphere
    }

    public Transform Target1;
    public Transform Target2;
    public Transform Target3;
    public Transform Target4;
    public Material material1;
    public Material material2;
    public Material material3;
    public Material material4;


    public override void OnEpisodeBegin()
    {
        // position agent (0,1,-15)
        // position target1 (-5,1,5)
        // position target2 (5,1,5)
        // position target3 (-5,1,-5)
        // position target4 (5,1,-5)

       // If the Agent fell, zero its momentum, this is the sphere
       // * need to change the if condition to if it hits the walls or the objects falls off (in the case of back of the room), then spawn the agent rBody back at the original spawnLocation, can hardcode
        // if (this.transform.localPosition.y < 0)
        // {
        //     this.rBody.angularVelocity = Vector3.zero;
        //     this.rBody.velocity = Vector3.zero;
        //     this.transform.localPosition = new Vector3( 0, 0.5f, 0);
        // }

        this.transform.localPosition = new Vector3(0,1,-15);

        // Move the target to a new spot -  this is the cube
        // * for the targets, can change the code to randomise the objects at the respective locations
        // * need to hardcode 4 location on the play area

        // Target.localPosition = new Vector3(Random.value * 8 - 4,
        //                                    0.5f,
        //                                    Random.value * 8 - 4);

        material1.color = Random.ColorHSV();
        material2.color = Random.ColorHSV();
        material3.color = Random.ColorHSV();
        material4.color = Random.ColorHSV();

        // Debug.Log(Target1.localPosition);
        // Debug.Log(Target2.localPosition);
        // Debug.Log(Target3.localPosition);
        // Debug.Log(Target4.localPosition);
    }

    // this part of the code, we give the agent information to compute and then make decisions on what to do in the environment. input: the information from the world, output: the action taken by the agent
    // the type of input taken is the variable sensor that is imported from using Unity.MLAgents.Sensors; 
    // AddObservation tells the variable sensor what information to collect from the game
    // variable this refers to the gameobject you attach this script to, in this case is the sphere
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target1.localPosition); //cube
        sensor.AddObservation(Target2.localPosition); //cube
        sensor.AddObservation(Target3.localPosition); //cube
        sensor.AddObservation(Target4.localPosition); //cube
        sensor.AddObservation(this.transform.localPosition); //sphere

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x); //sphere since the rigidbody called in the script is from the GO the script is attached to, in this case: sphere
        sensor.AddObservation(rBody.velocity.z);
    }

    public float forceMultiplier = 10;
    // this part of the code outputs the action of the agent based on the data it received from the CollectObservations method
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 1. code for actions 
        // 2. code for rewards, we say the agent gets a reward when it runs into the target, we can do that by finding the distance between the agent and target

        // Actions, size = 2, 
        Vector3 controlSignal = Vector3.zero; // initialise all zero
        controlSignal.x = actionBuffers.ContinuousActions[0]; // ContinuousActions vs DiscreteActions, DiscreteActions mean its only a few possinble decisions, for e.g. move forward, yes or no? need to make certain number of branches
        controlSignal.z = actionBuffers.ContinuousActions[1]; // ContinuousActions means we can take any number or floating number and apply it to the behaviour for the agent 
        rBody.AddForce(controlSignal * forceMultiplier);

        // in gist, the CollectObservations method collects information from the world, and sends it to the NN for the NN to do its calculations on which action to take, this action is stored in actionBuffers in the OnActionReceived method
        // in the OnActionReceived method, the action from the NN which is stored in the actionBuffers variable, we then store in the controlSignal variable which will then combine with forceMultiplier to then use AddForce which then moves the GO in the direction and speed of the action from the NN
        // for example, we initialise the controlSignal to [0,0,0]
        // and then after the controlSignal.x =  and controlSignal.z = codes, the x and z components inherit the action output computed by the NN in actionBuffers and then
        // the controlSignal becomes e.g. [0.3,0,0.1]

        // Rewards
        float distanceToTarget1 = Vector3.Distance(this.transform.localPosition, Target1.localPosition); //calcualtes the distance between these 2 positions
        float distanceToTarget2 = Vector3.Distance(this.transform.localPosition, Target2.localPosition); //calcualtes the distance between these 2 positions
        float distanceToTarget3 = Vector3.Distance(this.transform.localPosition, Target3.localPosition); //calcualtes the distance between these 2 positions
        float distanceToTarget4 = Vector3.Distance(this.transform.localPosition, Target4.localPosition); //calcualtes the distance between these 2 positions

        // Reached target
        float lengthUnitCube = 1.42f; // The value 1.42f appears to be the square root of 2, which suggests that it's the distance between two points on a diagonal.
        float scaleTarget = 2; // change this based on the scale you are using in unity
        float reachedDistance = lengthUnitCube*scaleTarget;

        if (distanceToTarget1 < reachedDistance || distanceToTarget2 < reachedDistance || distanceToTarget3 < reachedDistance || distanceToTarget4 < reachedDistance)
        {
            SetReward(1.0f); // SetReward is generally used when you finalise the task (before EndEpisode)
             // you can instead use AddReward() but this is used more for intermediate task, for example, need hit the target 5 times in a row before considerered complete, then maybe for loop then AddReward(0.2f), then SetReward(1.0f) then finally EndEpisode()
            EndEpisode(); // end of task
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode(); // if the object falls off the plane, it has no way to get back on the plane so should EndEpisode here
        }
    }

    // this code enables you as the user to play the game instead of the computer automatically playing the game. this allows you to have more control on seeing how the custom env and your NN are working, to test that everything is working fine
    // actionsOut means that instead of going through the whole controlSignal thing in the OnActionsReceived, that entire thing is controlled by the user using the keyboard inputs 
    // to use this part of the code, select Heuristic only under Behavior type under Behaviour Parameters script in the Unity UI 
    public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Horizontal"); // file > build settings > players settings > input manager > axis
            continuousActionsOut[1] = Input.GetAxis("Vertical");
            // so now the continuousActionsOut will store the actions taken by the user, which will then be stored in the ActionBuffers. Next, the NN will then use the info from that ActionBuffers
        }
}