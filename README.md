# ✈️ Fully Autonomous Aircraft AI
 Tubitak 2209-A

This project demonstrates a **fully autonomous takeoff–flight–landing aircraft simulation** powered by **reinforcement learning (RL)** in Unity.  
Using the **Bing Maps API**, the geographical area between **Atatürk Airport** and **Istanbul Airport** was recreated as a realistic Unity terrain with the **World Composer** package.  
The **Silantro Flight Simulator** was integrated to provide accurate aerodynamics and flight dynamics.  

Three specialized RL models were trained for **takeoff**, **cruising**, and **landing**, using **Unity ML-Agents** and the **Proximal Policy Optimization (PPO)** algorithm.  
The project runs inside **Unity Engine** with a focus on realism, dynamic training, and generalization across different conditions.

---

**Author:** Yusuf AĞAÇ, Dr. Eyüp Emre ÜLKÜ
yusufagacofficial@gmail.com

---

<img width="2210" height="1234" alt="Ekran görüntüsü 2025-09-28 132150" src="https://github.com/user-attachments/assets/c75dbbbe-de52-4cb1-a3d5-05d9f93ffb2b" />

---

## 🚀 Features

### ✅ Evaluation
- Autonomous **takeoff, flight, and landing** (individually or sequentially).  
- Handles **severe atmospheric conditions** (turbulence, wind).  
- Works with **different airports, aircraft, and routes**.  
- Supports **different aircraft physics models**.  
- constantly converges to the **optimal, safe path**.  
- Successfully performs flights from **Atatürk Airport → Istanbul Airport** under compelling weather.

### 🎓 Training
- **Airport/aircraft independent** → fully dynamic learning.  
- **No overfitting** → randomized conditions ensure robustness.  
- Routes are generated **randomly on each attempt**.  
- All values are **relative & normalized** → crucial for adaptive RL systems.  
- **Atmospheric conditions vary each attempt** via configurable settings.  
- Balanced **exploration vs. exploitation**.  

### 🛠️ Editor Tools
- Gizmos for **airport bounds, aircraft headings, and paths**.  
- **Agent observations, actions, and rewards** visualized on the canvas.  
- **Editor parameters easily configurable** (rewards, conditions, limits).  
- **Optimal paths configurable** via **Bezier curve weights**.  

### 🎨 Visuals
- **High-quality Istanbul environment** between Atatürk & Istanbul airports.  
- **Unity HDRP** for realistic lighting and atmosphere.  
- **Volumetric clouds** and **god rays**.

---

## 🛠️ Technologies Used
- **Silantro Flight** Simulator *(paid Unity package)*  
- **Unity HDRP** (High Definition Render Pipeline)  
- **Unity ML-Agents**
- **Deep Reinforcement Learning – PPO Algorithm**  
- **Bing Maps API**  
- **World Composer**

---

## ⚠️ Important Notes
- This project **cannot be run directly on another PC** because the **Silantro Flight Simulator** package is **paid** and modified for this project.  
- If you are interested in integrating this system into your own flight simulator, feel free to **contact me**.  

---

## Take-Off Model

The takeoff model is expected to guide the aircraft to the runway exit point without causing a crash, entering a dangerous rotation, or going off the runway area.

**Environment:** The environment is a runway. The runway has several functional roles. It determines the aircraft's relative position and rotation. Using a 5-point Bézier curve, it calculates the optimal path. It also calculates the aircraft's distance from the optimal path and the required direction for the aircraft to approach this path. Based on all these calculations, it applies a relative normalization process.

**Observation:** Our agent has a total of 51 observations. The first two observations include the aircraft’s forward and upward directions. These observations provide the agent with information about the aircraft’s orientation in the scene, independent of the runway. Next, the scalar product of the aircraft’s forward direction and the scene’s upward vector is added to the observations. This shows how much the aircraft’s nose is pointing up or down. Similarly, the scalar product of the aircraft’s upward direction and the scene’s downward vector is included, informing the agent about the aircraft's roll angle and whether it is inverted. The aircraft’s speed, thrust, and speed direction are also included in the observations, providing the agent with information about the aircraft's momentum. Additionally, the distance to the optimal position and the direction toward the optimal path are part of the observations. This data informs the agent about the aircraft’s status on the optimal path and the required rotation to proceed toward the path. Another observation is the difference between the aircraft’s forward direction and its speed direction. This shows the necessary rotational change for the aircraft to fly straight. The difference between the optimal path direction and the speed direction is also included, indicating the speed direction adjustment needed to approach the optimal path. The scalar product between the aircraft’s forward direction and its speed is included in the observations, showing how straight the aircraft is flying and the risk of stalling. Similarly, the scalar product between the aircraft’s speed and the optimal path direction helps the agent determine whether it is moving at the correct angle. The scalar product between the aircraft’s body direction and the optimal path direction is also included to show the similarity between the body direction and the path. The agent’s previous actions, target actions, deflection values, and axial angles are also included. These observations guide the agent on which wing angles to use to achieve the desired rotation. The aircraft’s movable airfoils require time for their servo motors to reach the target deflection values, and all this data is necessary for the agent to perceive this delay. The direction, strength, and turbulence of the wind in the atmosphere are also included in the observations, helping the agent determine the necessary actions to stabilize against atmospheric conditions. The aircraft’s position and rotation on the runway are also included, allowing the agent to learn its position on the runway.Finally, distance information from the aircraft’s collision sensors is included, informing the agent about the proximity of surrounding objects detected by the sensors.All observations are subjected to a normalization process.

**Action:** The agent have 3 continuous actions. These actions control the aircraft's pitch, yaw, and roll values. All actions are subjected to a normalization process.

**Reward:** There are 2 sparse rewards. One sparse reward is for completing the task successfully, and the other is for failure. The condition for success is reaching the exit point. The condition for failure is going off the runway or putting the aircraft in dangerous positions. There are 4 different dense rewards. The first rewards the minimization of the distance to the optimal path. Another penalizes the difference between the previous action and the new action. A third reward incentivizes the similarity between the aircraft's nose direction and its velocity direction. The final dense reward encourages alignment between the aircraft's velocity direction and the optimal rotation.

---

## Flight Model

The flight model is expected to guide the aircraft from the departure airport to the landing airport without causing a crash, entering a dangerous rotation, or going outside the flight path's area.

**Environment:** The environment is a path between two runways. The optimal path is calculated using a 5-point Bézier curve. The distance from the aircraft to the optimal path and the required rotation for the aircraft to approach the path are calculated. A normalization process is applied to the result of all calculations.

**Observation:** Unlike the takeoff phase, observations do not include collision sensors or position and rotation information relative to the runway.

**Action:** The actions are the same as in the takeoff phase.

**Reward:** The rewards are the same as in the takeoff phase.

---

## Landing  Model

The landing model is expected to land the aircraft on the runway without causing a crash, entering a dangerous rotation, or going outside the runway area.

**Environment:** Unlike the takeoff phase, the Bézier control points that calculate the optimal path are positioned differently.

**Observation:** In addition to the takeoff phase, observations include acceleration, throttle, and whether the wheels have touched the ground.

**Action:** In addition to the takeoff phase actions, a throttle action is added. This action allows the agent to control the aircraft's thrust.

**Reward:** In addition to the takeoff phase rewards, 2 dense rewards are added. The first rewards the aircraft for slowing down, and the second rewards the aircraft for making contact with the ground.

---

## 📸 Preview
<img width="2203" height="1232" alt="Ekran görüntüsü 2025-09-28 131950" src="https://github.com/user-attachments/assets/a69e0463-b9cd-4f56-96c9-4c07126ed4f0" />

<img width="2214" height="1232" alt="Ekran görüntüsü 2025-09-28 131933" src="https://github.com/user-attachments/assets/58f07819-2222-45b1-8756-42636480308d" />

<img width="2197" height="1228" alt="Ekran görüntüsü 2025-09-28 132035" src="https://github.com/user-attachments/assets/6cec373d-cbd1-4b75-b785-82498a8d9066" />

<img width="2205" height="1236" alt="Ekran görüntüsü 2025-09-28 132212" src="https://github.com/user-attachments/assets/57d81a73-dfd5-4e9c-a67b-79743a370cd4" />

<img width="2202" height="1234" alt="Ekran görüntüsü 2025-09-28 132225" src="https://github.com/user-attachments/assets/d93b3460-b082-4f0a-ad9f-cadd1c2d007e" />

<img width="2216" height="1235" alt="Ekran görüntüsü 2025-09-28 132003" src="https://github.com/user-attachments/assets/f877acc2-e29b-4bac-bdbc-90856a39b3b8" />
