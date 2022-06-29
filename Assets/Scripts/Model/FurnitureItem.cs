using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using UnityEngine;
using static Utils.Utils;
using Object = UnityEngine.Object;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Random = UnityEngine.Random;

namespace Model
{
    public class FurnitureItem
    {
        public readonly FurnitureModel Model;
        public Bounds Bounds;
        public Bounds ClearanceBounds;
        public Vector3 Position;
        public float Angle;

        public FurnitureItem(FurnitureModel model)
        {
            Model = model;
            Bounds = GetModelBounds(Model.Object);
            ClearanceBounds = GetClearanceBounds(this);
        }

        public FurnitureItem Copy()
        {
            return new FurnitureItem(new FurnitureModel(Model.Object.name, Model.Type))
                {Position = Position, Angle = Angle};
        }

        public Transform Instantiate()
        {
            return Object.Instantiate(Model.Object.transform, Position, Quaternion.Euler(new Vector3(-90, Angle, 0)));
        }

        // generates a random position and angle for this item
        public void Randomize()
        {
            Angle = Random.Range(0f, 90f);
            var margins = GetMargins(Bounds.size.x, Bounds.size.z, Angle);
            var x = Random.Range(-Settings.Width / 2 + 0.1f + margins.x / 2,
                Settings.Width / 2 - 0.1f - margins.x / 2);
            var z = Random.Range(-Settings.Depth / 2 + 0.1f + margins.y / 2,
                Settings.Depth / 2 - 0.1f - margins.y / 2);
            Position = new Vector3(x, 0, z);

            if (Model.Type.Equals(FurnitureType.Bed) || Model.Type.Equals(FurnitureType.Cabinet))
                SnapToClosestWall();
        }

        // snaps position and angle to closest wall
        public void SnapToClosestWall()
        {
            var margins = GetMargins(Bounds.size.x, Bounds.size.z, 0);
            var distances = GetWallDistances();
            var x = 0f;
            var z = 0f;
            switch (distances.FindIndex(distance => distance.Equals(distances.Min())))
            {
                case 0: // left
                    x = -Settings.Width / 2 + 0.1f + margins.x / 2;
                    z = Random.Range(-Settings.Depth / 2 + 0.1f + margins.y / 2,
                        Settings.Depth / 2 - 0.1f - margins.y / 2);
                    Angle = -90;
                    break;
                case 1: // right
                    x = Settings.Width / 2 - 0.1f - margins.x / 2;
                    z = Random.Range(-Settings.Depth / 2 + 0.1f + margins.y / 2,
                        Settings.Depth / 2 - 0.1f - margins.y / 2);
                    Angle = 90;
                    break;
                case 2: // front
                    x = Random.Range(-Settings.Width / 2 + 0.1f + margins.x / 2,
                        Settings.Width / 2 - 0.1f - margins.x / 2);
                    z = -Settings.Depth / 2 + 0.1f + margins.y / 2;
                    Angle = 180;
                    break;
                case 3: // back
                    x = Random.Range(-Settings.Width / 2 + 0.1f + margins.x / 2,
                        Settings.Width / 2 - 0.1f - margins.x / 2);
                    z = Settings.Depth / 2 - 0.1f - margins.y / 2;
                    Angle = 0;
                    break;
            }

            Position = new Vector3(x, 0, z);
        }

        public void AlignToClosestWall()
        {
            var distances = GetWallDistances();
            switch (distances.FindIndex(distance => distance.Equals(distances.Min())))
            {
                case 0: // left
                    Angle = -90;
                    break;
                case 1: // right
                    Angle = 90;
                    break;
                case 2: // front
                    Angle = 180;
                    break;
                case 3: // back
                    Angle = 0;
                    break;
            }
        }

        public List<float> GetWallDistances()
        {
            return new List<float>()
            {
                Math.Abs(Position.x + Settings.Width / 2), // left
                Math.Abs(Position.x - Settings.Width / 2), // right
                Math.Abs(Position.x + Settings.Depth / 2), // front
                Math.Abs(Position.x - Settings.Depth / 2) // back
            };
        }

        // overlap area between ground projection of 2 furniture items
        public double GetOverlapArea(FurnitureItem b, bool shouldIncludeClearance = false)
        {
            Bounds boundsA = shouldIncludeClearance ? this.ClearanceBounds : this.Bounds;
            Bounds boundsB = shouldIncludeClearance ? b.ClearanceBounds : b.Bounds;
            Rect rectangleA = new Rect(this.Position.x - boundsA.extents.x, this.Position.z - boundsA.extents.z,
                boundsA.size.x, boundsA.size.z);
            Rect rectangleB = new Rect(b.Position.x - boundsB.extents.x, b.Position.z - boundsB.extents.z,
                boundsB.size.x, boundsB.size.z);
            Polygon subjects = GetRotatedRectangle(rectangleA, this.Angle);
            Polygon clips = GetRotatedRectangle(rectangleB, b.Angle);

            Clipper c = new Clipper();
            c.AddPolygon(subjects, PolyType.ptSubject);
            c.AddPolygon(clips, PolyType.ptClip);
            Polygons solution = new Polygons();
            c.Execute(ClipType.ctIntersection, solution);

            return solution.Count == 0 ? 0 : GetPolygonArea(solution);
        }
    }
}