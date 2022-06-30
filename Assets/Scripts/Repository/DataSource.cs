using System.Collections.Generic;
using System.IO;
using Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Repository
{
    public class DataSource
    {
        public List<Material> Materials;
        public List<FurnitureModel> FurnitureModels;
        public readonly List<int> Beds = new List<int>();
        public readonly List<int> Seats = new List<int>();
        public readonly List<int> Tables = new List<int>();
        public readonly List<int> Cabinets = new List<int>();
        public readonly List<int> Other = new List<int>();

        public DataSource()
        {
            LoadMaterials();
            LoadModels();
        }

        private void LoadMaterials()
        {
            Materials = new List<Material>();
            var materialPaths = new List<string>()
            {
                "base_material", "base_material 2", "cloth_1", "cloth_2", "cloth_3", "cloth_4", "floor", "wood_1",
                "wood_2", "wood_3", "wood_4"
            };
            foreach (var path in materialPaths)
            {
                Materials.Add(Resources.Load($"Materials/{path}", typeof(Material)) as Material);
            }
        }

        private void LoadModels()
        {
            using var r = new StreamReader("Assets/Scripts/Repository/FurnitureModels.json");
            var json = r.ReadToEnd();
            FurnitureModels = JsonConvert.DeserializeObject<List<FurnitureModel>>(json);
            for (var pos = 0; pos < FurnitureModels.Count; pos++)
                switch (FurnitureModels[pos].Type)
                {
                    case FurnitureType.Bed:
                        Beds.Add(pos);
                        break;
                    case FurnitureType.Seat:
                        Seats.Add(pos);
                        break;
                    case FurnitureType.Table:
                        Tables.Add(pos);
                        break;
                    case FurnitureType.Cabinet:
                        Cabinets.Add(pos);
                        break;
                    default:
                        Other.Add(pos);
                        break;
                }
        }
    }
}