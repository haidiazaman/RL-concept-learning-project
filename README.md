# RL-concept-learning-project
Consolidation of work for the RL concept learning project targeted for AAAI '23.
This work is inspired by the "Simulating Early Word Learning in Situated Connectionist Agents" paper by Deepmind: https://cognitivesciencesociety.org/cogsci20/papers/0155/0155.pdf

We use vision-language models to learn various spatial relationships in a simulated RL environment. The environment is generated using Unity 3D and writing of several C# scripts. The Unity ML-Agents package is used to interface between the models and the agent in the Unity environment.


Using the Unity 3D UI to manually create custom scenes using the ML-Agents package

![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/mlagents_s1a_unity_ui.jpg)


## Stage 0 examples
example of scene from top view

![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/s0_topcamera.jpg)

RL agent's first person view 
- the inputs to the RL model are these RGB camera inputs of the agents first person camera view

![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/s0_agentcamera.jpg)


## Stage 1a examples
example of scene from top view

![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/s1a_topcamera.jpg)

RL agent's first person view 
- the inputs to the RL model are these RGB camera inputs of the agents first person camera view
  
![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/s1a_agentcamera.jpg)


check out this GIF for an example on how the agent interacts with the env
- 4 discrete actions: move forward, move backward, rotate right and rotate left
- scene auto renews upon hitting the Target object

![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/s1a_example_movement.gif)
