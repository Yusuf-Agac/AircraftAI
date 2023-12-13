using UnityEngine;
using System.Collections;
using System;

namespace WorldComposer
{
    [Serializable]
    public class TerrainNeighbors : MonoBehaviour
    {
        public Terrain left;
        public Terrain top;
        public Terrain right;
        public Terrain bottom;

        void Start()
        {
            Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
            terrain.SetNeighbors(left, top, right, bottom);
        }
    }
}