# ✈️ Fully Autonomous Aircraft AI
 Tubitak 2209-A

This project demonstrates a **fully autonomous takeoff–flight–landing aircraft simulation** powered by **reinforcement learning (RL)** in Unity.  
Using the **Bing Maps API**, the geographical area between **Atatürk Airport** and **Istanbul Airport** was recreated as a realistic Unity terrain with the **World Composer** package.  
The **Silantro Flight Simulator** was integrated to provide accurate aerodynamics and flight dynamics.  

Three specialized RL models were trained for **takeoff**, **cruising**, and **landing**, using **Unity ML-Agents** and the **Proximal Policy Optimization (PPO)** algorithm.  
The project runs inside **Unity Engine** with a focus on realism, dynamic training, and generalization across different conditions.

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

## 📸 Preview
<img width="2203" height="1232" alt="Ekran görüntüsü 2025-09-28 131950" src="https://github.com/user-attachments/assets/a69e0463-b9cd-4f56-96c9-4c07126ed4f0" />
<img width="2214" height="1232" alt="Ekran görüntüsü 2025-09-28 131933" src="https://github.com/user-attachments/assets/58f07819-2222-45b1-8756-42636480308d" />
<img width="2197" height="1228" alt="Ekran görüntüsü 2025-09-28 132035" src="https://github.com/user-attachments/assets/6cec373d-cbd1-4b75-b785-82498a8d9066" />
<img width="2210" height="1234" alt="Ekran görüntüsü 2025-09-28 132150" src="https://github.com/user-attachments/assets/c75dbbbe-de52-4cb1-a3d5-05d9f93ffb2b" />
<img width="2205" height="1236" alt="Ekran görüntüsü 2025-09-28 132212" src="https://github.com/user-attachments/assets/57d81a73-dfd5-4e9c-a67b-79743a370cd4" />
<img width="2202" height="1234" alt="Ekran görüntüsü 2025-09-28 132225" src="https://github.com/user-attachments/assets/d93b3460-b082-4f0a-ad9f-cadd1c2d007e" />
<img width="2216" height="1235" alt="Ekran görüntüsü 2025-09-28 132003" src="https://github.com/user-attachments/assets/f877acc2-e29b-4bac-bdbc-90856a39b3b8" />


---

**Author:** Yusuf AĞAÇ, Dr. Eyüp Emre ÜLKÜ
yusufagacofficial@gmail.com

---
