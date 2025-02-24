using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Math
{
    public static class MathUtil
    {
        public static float EaseIn(float f)
        {
            return f * f;
        }

        public static float EaseOut(float f)
        {
            return 1.0f - (1.0f - f) * (1.0f - f);
        }

        public static float SmoothStep(float f)
        {
            return f * f * (3.0f - 2.0f * f);
        }

        public static float EaseOutElastic(float f)
        {
            float p = 0.5f;
            return Mathf.Pow(2, -10 * f) * Mathf.Sin((f - p / 4) * (2 * Mathf.PI) / p) + 1;
        }

        public static float OutBounce(float f)
        {
            float div = 2.75f;
            float mult = 7.5625f;

            if (f < 1 / div)
            {
                return mult * f * f;
            }
            else if (f < 2 / div)
            {
                f -= 1.5f / div;
                return mult * f * f + 0.75f;
            }
            else if (f < 2.5 / div)
            {
                f -= 2.25f / div;
                return mult * f * f + 0.9375f;
            }
            else
            {
                f -= 2.625f / div;
                return mult * f * f + 0.984375f;
            }
        }

        public static float DotProduct(Vector2 vA, Vector2 vB)
        {
            return (vA.x * vB.x) + (vA.y * vB.y);
        }

        public static float AngleBetween(Vector2 vA, Vector2 vB)
        {
            return Mathf.Acos(DotProduct(vA, vB) / (vA.magnitude * vB.magnitude)) * Mathf.Rad2Deg;
        }

        public static Vector3 CrossProduct(Vector3 vA, Vector3 vB)
        {
            return new Vector3((vA.y * vB.z) - (vA.z * vB.y),
                               (vA.x * vB.z) - (vA.z * vB.x),
                               (vA.x * vB.y) - (vA.y * vB.x));
        }

        public static Vector3 ClosestPointOnPlane(Plane p, Vector3 v)
        {
            float fDistance = Vector3.Dot(p.normal, v) + p.distance;
            return v - p.normal * fDistance;
        }

        public static bool GetSide(Plane p, Vector3 v)
        {
            return Vector3.Dot(p.normal, v) + p.distance > 0f;
        }

        public static Vector3 ClosestPointOnSegment(Vector3 v, Vector3 vA, Vector3 vB)
        {
            Vector3 vToP = v - vA;
            Vector3 vAB = vB - vA;

            float fDot1 = Vector3.Dot(vToP, vAB);
            if (fDot1 <= 0.0f)
            {
                return vA;
            }

            vToP = v - vB;
            Vector3 vBA = vA - vB;
            float fDot2 = Vector3.Dot(vToP, vBA);
            if (fDot2 <= 0.0f)
            {
                return vB;
            }

            return vA + vAB.normalized * (fDot1 / vAB.magnitude);
        }

        public static bool PointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2, out Vector3 vBaryCoords)
        {
            // Compute vectors        
            Vector3 v0 = p2 - p0;
            Vector3 v1 = p1 - p0;
            Vector3 v2 = p - p0;

            // Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            float w = 1.0f - (u + v);
            vBaryCoords = new Vector3(w, v, u);

            // Check if point is in triangle
            return u >= 0.0f && 
                   v >= 0.0f && 
                   u + v <= 1.0f;
        }

        public static bool RayTriangleIntersection(Ray ray, Vector3 p0, Vector3 p1, Vector3 p2, out Vector3 vHitPoint, out Vector3 vBaryCoord)
        {
            vHitPoint = Vector3.zero;
            Plane plane = new Plane(p0, p1, p2);
            float fEnter;
            if (!plane.Raycast(ray, out fEnter))
            {
                vBaryCoord = Vector3.zero;
                return false;
            }

            Vector3 vPointOnPlane = ray.origin + ray.direction.normalized * fEnter;
            if (PointInTriangle(vPointOnPlane, p0, p1, p2, out vBaryCoord))
            {
                vHitPoint = vPointOnPlane;
                return true;
            }

            return false;
        }

        public static IEnumerable<Vector3Int> Bresenham3D(Vector3Int vA, Vector3Int vB)
        {
            // https://www.geeksforgeeks.org/bresenhams-algorithm-for-3-d-line-drawing/

            yield return vA;

            int dx = Mathf.Abs(vB.x - vA.x);
            int dy = Mathf.Abs(vB.y - vA.y);
            int dz = Mathf.Abs(vB.z - vA.z);
            int xs;
            int ys;
            int zs;
            if (vB.x > vA.x)
                xs = 1;
            else
                xs = -1;
            if (vB.y > vA.y)
                ys = 1;
            else
                ys = -1;
            if (vB.z > vA.z)
                zs = 1;
            else
                zs = -1;

            // Driving axis is X-axis"
            if (dx >= dy && dx >= dz)
            {
                int p1 = 2 * dy - dx;
                int p2 = 2 * dz - dx;
                while (vA.x != vB.x)
                {
                    vA.x += xs;
                    if (p1 >= 0)
                    {
                        vA.y += ys;
                        p1 -= 2 * dx;
                    }
                    if (p2 >= 0)
                    {
                        vA.z += zs;
                        p2 -= 2 * dx;
                    }
                    p1 += 2 * dy;
                    p2 += 2 * dz;

                    yield return vA;
                }

                // Driving axis is Y-axis"
            }
            else if (dy >= dx && dy >= dz)
            {
                int p1 = 2 * dx - dy;
                int p2 = 2 * dz - dy;
                while (vA.y != vB.y)
                {
                    vA.y += ys;
                    if (p1 >= 0)
                    {
                        vA.x += xs;
                        p1 -= 2 * dy;
                    }
                    if (p2 >= 0)
                    {
                        vA.z += zs;
                        p2 -= 2 * dy;
                    }
                    p1 += 2 * dx;
                    p2 += 2 * dz;

                    yield return vA;
                }

                // Driving axis is Z-axis"
            }
            else
            {
                int p1 = 2 * dy - dz;
                int p2 = 2 * dx - dz;
                while (vA.z != vB.z)
                {
                    vA.z += zs;
                    if (p1 >= 0)
                    {
                        vA.y += ys;
                        p1 -= 2 * dz;
                    }
                    if (p2 >= 0)
                    {
                        vA.x += xs;
                        p2 -= 2 * dz;
                    }
                    p1 += 2 * dy;
                    p2 += 2 * dx;

                    yield return vA;
                }
            }
        }
    }

    public static class Math2D
    {
        public static Vector2 ToXZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 ToXYZ(this Vector2 v)
        {
            return new Vector3(v.x, 0.0f, v.y);
        }

        public static float GetDistance2D(Vector3 vA, Vector3 vB)
        {
            return Vector2.Distance(vA.ToXZ(), vB.ToXZ());
        }

        public static Vector3 GetDirection2D(Vector3 v)
        {
            v.y = 0.0f;
            return v.normalized;
        }
    }
}