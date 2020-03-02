using System;
using UnityEngine;

namespace FiscalShock.Graphs {
    /// <summary>
    /// Various math functions (can't name it `Math` because that collides with `System.Math`)
    /// </summary>
    public static class Mathy {
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
    }
}