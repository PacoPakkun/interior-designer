using System;
using System.Collections.Generic;
using ClipperLib;
using Model;
using UnityEngine;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace Utils
{
    public static class Utils
    {
        // bounds of furniture model
        public static Bounds GetModelBounds(GameObject model)
        {
            if (model.transform.TryGetComponent<Renderer>(out var renderer))
                return renderer.bounds;

            var bounds = model.transform.GetChild(0).GetComponent<Renderer>().bounds;
            foreach (Transform child in model.transform)
            {
                bounds.Encapsulate(child.GetComponent<Renderer>().bounds);
            }

            return bounds;
        }

        // minimum margins to walls
        public static Vector2 GetMargins(float width, float length, float angle)
        {
            var x = Math.Abs(width * Math.Sin(DegToRad(90 - angle))) + Math.Abs(length * Math.Sin(DegToRad(angle)));
            var y = Math.Abs(length * Math.Sin(DegToRad(90 - angle))) + Math.Abs(width * Math.Sin(DegToRad(angle)));
            return new Vector2((float) x, (float) y);
        }

        // convert degrees to radians
        public static double DegToRad(double angle)
        {
            return angle * Math.PI / 180;
        }

        // convert radians to degrees
        public static double RadToDeg(double angle)
        {
            return angle * 180 / Math.PI;
        }


        // vertices of polygon after rotation by angle
        public static Polygon GetRotatedRectangle(Rect rectangle, float angle)
        {
            return new Polygon()
            {
                RotatePoint(rectangle.min, rectangle.center, angle),
                RotatePoint(new Vector2(rectangle.xMax, rectangle.yMin), rectangle.center, angle),
                RotatePoint(rectangle.max, rectangle.center, angle),
                RotatePoint(new Vector2(rectangle.xMin, rectangle.yMax), rectangle.center, angle),
            };
        }

        // rotate a point by the center of a rectangle
        public static IntPoint RotatePoint(Vector2 point, Vector2 center, float angle)
        {
            var width = point.x - center.x;
            var length = point.y - center.y;
            var rotatedX = width * Math.Cos(DegToRad(angle)) - length * Math.Sin(DegToRad(angle));
            var rotatedY = width * Math.Sin(DegToRad(angle)) + length * Math.Cos(DegToRad(angle));
            var x = rotatedX + center.x;
            var y = rotatedY + center.y;
            return new IntPoint((long) (x * 1000000), (long) (y * 1000000));
        }

        // area of polygon
        public static double GetPolygonArea(Polygons polygon)
        {
            double area = 0;
            int j = polygon[0].Count - 1;
            for (int i = 0; i < polygon[0].Count; i++)
            {
                area += (polygon[0][j].X / 1000000f + polygon[0][i].X / 1000000f) *
                        (polygon[0][j].Y / 1000000f - polygon[0][i].Y / 1000000f);
                j = i;
            }

            return Math.Abs(area / 2.0);
        }

        // specific clearance bounds for different furniture types
        public static Bounds GetClearanceBounds(FurnitureItem furnitureItem)
        {
            var clearanceBounds = furnitureItem.Bounds;
            switch (furnitureItem.Model.Type)
            {
                case FurnitureType.Bed:
                    clearanceBounds.Expand(new Vector3(0.92f * 2, 0, 0));
                    break;
                case FurnitureType.Seat:
                    clearanceBounds.Encapsulate(new Vector3(furnitureItem.Bounds.center.x,
                        furnitureItem.Bounds.center.y, furnitureItem.Bounds.max.z + 0.76f));
                    break;
                case FurnitureType.Cabinet:
                    clearanceBounds.Encapsulate(new Vector3(furnitureItem.Bounds.center.x,
                        furnitureItem.Bounds.center.y, furnitureItem.Bounds.max.z + 0.61f));
                    break;
                case FurnitureType.Table:
                    clearanceBounds.Expand(new Vector3(0.92f * 2, 0, 0.92f * 2));
                    break;
                default:
                    clearanceBounds.Expand(new Vector3(0.5f * 2, 0, 0.5f * 2));
                    break;
            }

            return clearanceBounds;
        }

        // number of distinct circulation areas within the room
        public static int GetCirculationComponents(List<FurnitureItem> furnitureItems)
        {
            // TODO add wall polygons

            Polygon roomPolygon = new Polygon()
            {
                new IntPoint((long) (-3.5f * 1000000), (long) (-2.5f * 1000000)),
                new IntPoint((long) (3.5f * 1000000), (long) (-2.5f * 1000000)),
                new IntPoint((long) (3.5f * 1000000), (long) (2.5f * 1000000)),
                new IntPoint((long) (-3.5f * 1000000), (long) (2.5f * 1000000)),
            };

            Clipper c = new Clipper();
            foreach (var furnitureItem in furnitureItems)
            {
                Bounds bounds = furnitureItem.Bounds;
                bounds.Expand(new Vector3(0.40f * 2, 0, 0.40f * 2));
                Rect rectangle = new Rect(furnitureItem.Position.x - bounds.extents.x,
                    furnitureItem.Position.z - bounds.extents.z,
                    bounds.size.x, bounds.size.z);
                Polygon polygon = GetRotatedRectangle(rectangle, furnitureItem.Angle);
                c.AddPolygon(polygon, PolyType.ptSubject);
            }

            Polygons union = new Polygons();
            c.Execute(ClipType.ctUnion, union);

            c.AddPolygon(roomPolygon, PolyType.ptSubject);
            c.AddPolygon(union[0], PolyType.ptClip);
            Polygons solution = new Polygons();
            c.Execute(ClipType.ctDifference, solution);

            return solution.Count;
        }

        // recommended distances between furniture items depending on their type
        public static Tuple<float, float> GetPairwiseDistance(this FurnitureItem a, FurnitureItem b)
        {
            if (a.Model.Type.Equals(FurnitureType.Seat) && b.Model.Type.Equals(FurnitureType.Table) ||
                b.Model.Type.Equals(FurnitureType.Seat) && a.Model.Type.Equals(FurnitureType.Table))
            {
                return new Tuple<float, float>(1f, 1.3f);
            }

            return null;
        }

        // recommended distances between seats to promote conversation
        public static Tuple<float, float> GetConversationDistance(this FurnitureItem a, FurnitureItem b)
        {
            if (a.Model.Type.Equals(FurnitureType.Seat) && b.Model.Type.Equals(FurnitureType.Seat))
            {
                return new Tuple<float, float>(1f, 2f);
            }

            return null;
        }

        // function that hax a max plateau value between the min and max parameters
        public static float PlateauFunction(float distance, float min, float max)
        {
            if (distance < min)
                return (distance / min) * (distance / min) * (distance / min) * (distance / min);
            if (distance > max)
                return (max / distance) * (max / distance) * (max / distance) * (max / distance);
            return 1;
        }
    }
}