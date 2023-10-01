using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedFramework.Core;
using EnhancedEditor;

namespace LudumDare54
{
    public class LevelManager : EnhancedSingleton<LevelManager>
    {
        [SerializeField]
        public int weight;
        [SerializeField]
        public int height;
        [SerializeField]
        public List<int> Grid;
        [SerializeField]
        public List<Vector2> GridBlockMap;
        [SerializeField]
        public GameObject Empty;
        [SerializeField]
        public GameObject Block;



        [Button(ActivationMode.Play, SuperColor.Green, IsDrawnOnTop=false)]
        public void InitLevel(int w, int h)
        {
            this.weight = w;
            this.height = h;
            this.Grid = new List<int>();
            this.GridBlockMap = new List<Vector2>();
            int cpt = 0;
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++)
                {
                    Instantiate(Empty, new Vector3(0.5f*i, 0.5f*j, 0), Quaternion.identity);
                    Grid.Add(cpt);
                    cpt++;
                }   
            }
        }

        [Button(ActivationMode.Play, SuperColor.Blue, IsDrawnOnTop = false)]
        public void PlaceBlock(Vector2 position)
        {
            Instantiate(Block, new Vector3(0.5f * position.x, 0.5f * position.y, 0), Quaternion.identity);
            this.GridBlockMap.Add(position);
        }

        [Button(ActivationMode.Play, SuperColor.Red, IsDrawnOnTop = false)]
        public void PlaceMultiblock(Multiblock m, Vector2 position)
        {
            foreach (Vector2 v in m.BlockStructure)
            {
                Vector2 blockPos = position + v;
                if(!IsOutOfGrid(blockPos))
                {
                    PlaceBlock(blockPos);
                }
            }
        }

        public bool IsOutOfGrid(Vector2 position)
        {
            return position.x < 0 || position.y < 0 || position.x > this.weight-1 || position.y > this.height-1;
        }

        public int ToGridCoordinate(Vector2 position)
        {
            return (int)position.y*this.weight + (int)position.x;
        }
    }
}
