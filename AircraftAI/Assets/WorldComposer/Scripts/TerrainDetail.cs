using UnityEngine;
using System.Collections;
using System;

namespace WorldComposer
{
    [Serializable]
    public class TerrainDetail : MonoBehaviour
    {
        public int heightmapMaximumLOD;
        public float heightmapPixelError;
        public float basemapDistance;
        public bool castShadows;
        public bool draw;
        public float treeDistance;
        public float detailObjectDistance;
        public float detailObjectDensity;
        public float treeBillboardDistance;
        public float treeCrossFadeLength;
        public int treeMaximumFullLODCount;

        public TerrainDetail()
        {
            heightmapPixelError = 5;
            basemapDistance = 50000;
            draw = true;
            treeDistance = 20000;
            detailObjectDistance = 250;
            detailObjectDensity = 1;
            treeBillboardDistance = 200;
            treeCrossFadeLength = 50;
            treeMaximumFullLODCount = 50;
        }

        void Start()
        {
            Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
            terrain.heightmapPixelError = heightmapPixelError;
            terrain.heightmapMaximumLOD = heightmapMaximumLOD;
            if (terrain.GetComponent("ReliefTerrain") == null)
            {
                terrain.basemapDistance = basemapDistance;
            }
            //terrain.castShadows = castShadows;
            if (draw)
            {
                terrain.treeDistance = treeDistance;
                terrain.detailObjectDistance = detailObjectDistance;
            }
            else
            {
                terrain.treeDistance = 0;
                terrain.detailObjectDistance = 0;
            }
            terrain.detailObjectDensity = detailObjectDensity;
            terrain.treeMaximumFullLODCount = treeMaximumFullLODCount;
            terrain.treeBillboardDistance = treeBillboardDistance;
            terrain.treeCrossFadeLength = treeCrossFadeLength;
            terrain.treeMaximumFullLODCount = treeMaximumFullLODCount;
        }
    }
}