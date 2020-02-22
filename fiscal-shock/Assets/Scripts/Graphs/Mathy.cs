using System;
using UnityEngine;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Various math functions (can't name it `Math` because that collides with `System.Math`)
    /// </summary>
    public static class Mathy {
        public const double HALF_PI = Math.PI / 2;  // TODO remove if not using

        /// <summary>
        /// Find determinant of a 2x2 matrix by cross-multiplying.
        /// <para>`| a b |`</para><para/>
        /// <para>`| c d |`</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns>determinant of 2x2 matrix</returns>
        public static double determinant2(double a, double b, double c, double d) {
            return (a * d) - (b * c);
        }

        public static double determinant2(Vertex a, Vertex b) {
            return determinant2(a.x, b.x, a.y, b.y);
        }

        /// <summary>
        /// Euclidean distance between two Cartesian coordiates
        /// </summary>
        /// <returns>distance</returns>
        public static double getDistanceBetween(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        /// <summary>
        /// <para>atan2 range: [-π, π]</para>
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double getAngleOfRotation(double x1, double y1, double x2, double y2) {
            return Math.Atan2(y2 - y1, x2 - x1);
        }

        /// <summary>
        /// Find a point on the line drawn at the angle theta from site that is distance units away from site.
        /// </summary>
        /// <param name="x">x-coordinate of site</param>
        /// <param name="y">y-coordinate of site</param>
        /// <param name="theta">angle in radians</param>
        /// <param name="distance">desired length of line segment</param>
        /// <returns>2-element array representing the x- and y-coordinate of the point, respectively</returns>
        public static double[] getEndpointOfLineRotation(double x, double y, double theta, float distance) {
            double u = x + (distance * Math.Cos(theta));
            double v = y + (distance * Math.Sin(theta));
            return new double[] { u, v };
        }

        // TODO cite reference or remove if don't need
        public static Vector2 Rotate(this Vector2 v, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        /// <summary>
        /// Find intersection between the lines ab and cd
        /// http://www.cs.swan.ac.uk/~cssimon/line_intersection.html
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns>2-element double representing x- and y-coordinates, respectively</returns>
        public static double[] findIntersection(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy) {
            const float EPSILON = 1e-5f;  // Floating point correction

            // Find numerator/denominator for t_a.
            float ta_numer = ((cy - dy) * (ax - cx)) + ((dx - cx) * (ay - cy));
            float ta_denom = ((dx - cx) * (ay - by)) - ((ax - bx) * (dy - cy));

            if (ta_denom == 0 || Math.Abs(ta_denom) < EPSILON) {  // Collinear
                return null;
            }

            float ta = ta_numer / ta_denom;

            if (ta < 0 || ta > 1) {  // Does not intersect on the segments
                return null;
            }

            // -----------------------------------
            // Find numerator/denominator for t_b.
            float tb_numer = ((ay - by) * (ax - cx)) + ((bx - ax) * (ay - cy));
            float tb_denom = ((dx - cx) * (ay - by)) - ((ax - bx) * (dy - cy));

            if (tb_denom == 0 || Math.Abs(tb_denom) < EPSILON) {  // Collinear
                return null;
            }

            float tb = tb_numer / tb_denom;

            if (tb < 0 || tb > 1) {  // Does not intersect on the segments
                return null;
            }

            // -----------------------------------
            // At this point, we know they intersect, so plug ta or tb into equation
            float x = ax + (ta * (bx - ax));
            float y = ay + (ta * (by - ay));

            return new double[] { x, y };
        }

        // TODO cite reference or remove
        public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection) {
            float Ax,Bx,Cx,Ay,By,Cy,d,e,f,num/*,offset*/;
            float x1lo,x1hi,y1lo,y1hi;
            Ax = p2.x-p1.x;
            Bx = p3.x-p4.x;
            // X bound box test/
            if(Ax<0) {
                x1lo=p2.x; x1hi=p1.x;
            } else {
                x1hi=p2.x; x1lo=p1.x;
            }
            if(Bx>0) {
                if(x1hi < p4.x || p3.x < x1lo) return false;
            } else {
                if(x1hi < p3.x || p4.x < x1lo) return false;
            }
            Ay = p2.y-p1.y;
            By = p3.y-p4.y;
            // Y bound box test//
            if(Ay<0) {
                y1lo=p2.y; y1hi=p1.y;
            } else {
                y1hi=p2.y; y1lo=p1.y;
            }
            if(By>0) {
                if(y1hi < p4.y || p3.y < y1lo) return false;
            } else {
                if(y1hi < p3.y || p4.y < y1lo) return false;
            }
            Cx = p1.x-p3.x;
            Cy = p1.y-p3.y;
            d = By*Cx - Bx*Cy;  // alpha numerator//
            f = Ay*Bx - Ax*By;  // both denominator//

            // alpha tests//
            if(f>0) {
                if(d<0 || d>f) return false;
            } else {
                if(d>0 || d<f) return false;
            }
            e = Ax*Cy - Ay*Cx;  // beta numerator//

            // beta tests //
            if(f>0) {
                if(e<0 || e>f) return false;
            } else {
                if(e>0 || e<f) return false;
            }
            // check if they are parallel
            if(f==0) return false;

            // compute intersection coordinates //
            num = d*Ax; // numerator //
            intersection.x = p1.x + num / f;
            num = d*Ay;
            intersection.y = p1.y + num / f;
            return true;
        }
    }
}