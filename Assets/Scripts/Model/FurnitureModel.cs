﻿using UnityEngine;

namespace Model
{
    public class FurnitureModel
    {
        public readonly GameObject Object;
        public readonly FurnitureType Type;

        public FurnitureModel(string name, FurnitureType type)
        {
            this.Object = Resources.Load($"Prefabs/{name}") as GameObject;
            this.Type = type;
        }
    }
}