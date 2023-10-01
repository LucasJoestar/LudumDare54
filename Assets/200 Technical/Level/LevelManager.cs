using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedFramework.Core;
using EnhancedEditor;
using UnityEngine.UIElements;

namespace LudumDare54
{
    public class LevelManager : EnhancedSingleton<LevelManager>
    {
        [SerializeField]
        public float width;
        [SerializeField]
        public float height;
        [SerializeField]
        public List<int> Grid;
        [SerializeField]
        public GameObject Empty;
        [SerializeField]
        public GameObject Block;
        public float UIOffsety = 3f;
        private float xscale = 1f;
        private float yscale = 1f;
        private float xoffset = 0.75f;
        private float yoffset = 0.75f;
        private Vector3 origin;

        public void InitLevel(float w, float h)
        {
            this.width = w;
            this.height = h;
            this.Grid = new List<int>();
            int cpt = 0;
             this.origin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
            this.origin.x = this.origin.x + xoffset;
            this.origin.y = this.origin.y + yoffset;
            for (int i = 0; i < w-1; i++) {
                for (int j = 0; j < h; j++)
                {
                    Instantiate(Empty, new Vector3(xscale * (float)i+this.origin.x, yscale*(float)j+this.origin.y, 0), Quaternion.identity);
                    Grid.Add(cpt);
                    cpt++;
                }   
            }
        }

        [Button(ActivationMode.Play, SuperColor.Green, IsDrawnOnTop = false)]
        public void InitLevel()
        {
            this.height = 2 * Camera.main.orthographicSize;
            this.width = height * Camera.main.aspect;
            this.height -= UIOffsety;
            InitLevel(this.width, this.height);
        }

        [Button(ActivationMode.Play, SuperColor.Blue, IsDrawnOnTop = false)]
        public void PlaceBlock(Vector2 position)
        {
            if(IsOutOfGrid(position)) {
                this.LogError("Placement out of the grid");
            } 
            else
            {
                Instantiate(Block, new Vector3(xscale * position.x + this.origin.x, yscale * position.y + this.origin.y, 0), Quaternion.identity);

            }
        }

        [Button(ActivationMode.Play, SuperColor.Red, IsDrawnOnTop = false)]
        public void PlaceMultiblock(Multiblock m, Vector2 position)
        {
            foreach (Vector2 v in m.BlockStructure)
            {
                Vector2 blockPos = position + v;
                Debug.Log(blockPos);
                if (!IsOutOfGrid(blockPos))
                {
                    PlaceBlock(blockPos);
                }
            }
        }

        public bool IsOutOfGrid(Vector2 position)
        {
            return position.x < 0 || position.y < 0 || position.x > this.width || position.y > this.height;
        }

        public float ToGridCoordinate(Vector2 position)
        {
            return position.y*this.width + position.x;
        }
    }
}
