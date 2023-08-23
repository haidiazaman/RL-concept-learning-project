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


## Images from Our Submitted Paper
Here are some examples of the finalised three environments we created. For each environment, we created train and test environments, according to a predetermined train-test split.
The first, second and third rows are examples from environments C/S , C+S and C+S+S respectively. The first column shows the top view of the environment, where the agent is spawned at the default starting position. The second column shows the first-person view (128x128) of the RL agent in the default starting position. The third column shows the top view of the environment, where the agent has moved around the environment. The fourth column shows the first-person view (128x128) of the RL agent in the third column. In the top view images, the agent is the small cube. For example, in the first column, the agent is the small cube located at the back of the room. For the first row (C/S environment), the target instruction is “yellow cylinder”. For the second row (C+S environment), the target instruction is “green cylinder”. For the third row (C+S+S environment), the target instruction is “black cube capsule”.

![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/supp%20fig%201.jpg)

Scenarios with Overlaps in Shape or Color
These images show various scenarios of the three different environments. Of these scenarios, there are some where there could be overlaps in one characteristic, either Shape or Color.
  
![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/supp%20fig%202.jpg)


## Example movement by the Agent 
![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/s1a_example_movement.gif)
![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/bird%20eye%20view%20gif.gif)

This GIF is tiny because it represents the agent's actual POV, which is only 128 x 128 pixels. For reference, the previous GIF is 2048 x 2048 pixels.

![alt text](https://github.com/haidiazaman/RL-concept-learning-project/blob/main/imgs/pov%20gif.gif)
