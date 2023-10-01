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



        [Button(ActivationMode.Play, SuperColor.Green, IsDrawnOnTop=false)]
        public void InitLevel(int w, int h)
        {
            this.weight = w;
            this.height = h;
            this.Grid = new List<int>();
            this.GridBlockMap = new List<Vector2>();
            for (int i = 0; i < w*h; i++) { Grid.Add(i); }


        }

        [Button(ActivationMode.Play, SuperColor.Blue, IsDrawnOnTop = false)]
        public void PlaceBlock(Vector2 position)
        {
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
    }
}
