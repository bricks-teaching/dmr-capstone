#pragma warning disable
using NUnit.Framework;
using UnityEngine;
using FiscalShock.Graphs;

namespace Tests {
    public class MathyTest {
        [Test]
        public void testGetAngleOfRotation90Deg() {
            double x1 = 0;
            double y1 = 0;
            double x2 = 0;
            double y2 = 5;

            float result = (float)Mathy.getAngleOfRotation(x1, y1, x2, y2) * Mathf.Rad2Deg;
            Assert.AreEqual(90.0f, result);
        }

        [Test]
        public void testGetAngleOfRotationInQuadrant1() {
            double x1 = 0;
            double y1 = 0;
            double x2 = 5;
            double y2 = 5;

            float result = (float)Mathy.getAngleOfRotation(x1, y1, x2, y2) * Mathf.Rad2Deg;
            Assert.GreaterOrEqual(result, 0);
            Assert.LessOrEqual(result, 90);
            Assert.AreEqual(45.0f, result);
        }

        [Test]
        public void testGetAngleOfRotationInQuadrant2() {
            double x1 = 0;
            double y1 = 0;
            double x2 = -5;
            double y2 = 5;

            float result = (float)Mathy.getAngleOfRotation(x1, y1, x2, y2) * Mathf.Rad2Deg;
            Assert.GreaterOrEqual(result, 90);
            Assert.LessOrEqual(result, 180);
            Assert.AreEqual(135.0f, result);
        }

        [Test]
        public void testGetAngleOfRotationInQuadrant3() {
            double x1 = 0;
            double y1 = 0;
            double x2 = -5;
            double y2 = -5;

            float result = (float)Mathy.getAngleOfRotation(x1, y1, x2, y2) * Mathf.Rad2Deg;
            Assert.GreaterOrEqual(result, -180);
            Assert.LessOrEqual(result, -90);
            Assert.AreEqual(-135.0f, result);
        }

        [Test]
        public void testGetAngleOfRotationInQuadrant4() {
            double x1 = 0;
            double y1 = 0;
            double x2 = 5;
            double y2 = -5;

            float result = (float)Mathy.getAngleOfRotation(x1, y1, x2, y2) * Mathf.Rad2Deg;
            Assert.GreaterOrEqual(result, -90);
            Assert.LessOrEqual(result, 0);
            Assert.AreEqual(-45.0f, result);
        }

        [Test]
        public void testFindIntersectionParallel() {
            float ax = 0;
            float ay = 0;
            float bx = 5;
            float by = 0;

            float cx = 0;
            float cy = 5;
            float dx = 5;
            float dy = 5;

            double[] result = Mathy.findIntersection(ax, ay, bx, by, cx, cy, dx, dy);
            Assert.IsNull(result);
        }

        [Test]
        public void testFindIntersectionCross() {
            // Q3 -> Q1 across origin
            float ax = -5;
            float ay = -5;
            float bx = 5;
            float by = 5;

            // Q2 -> Q4 across origin
            float cx = -5;
            float cy = 5;
            float dx = 5;
            float dy = -5;

            double[] result = Mathy.findIntersection(ax, ay, bx, by, cx, cy, dx, dy);
            Assert.IsNotNull(result);
            Assert.AreEqual(new double[] { 0, 0 }, result);
        }

        [Test]
        public void testFindIntersectionAtPoint() {
            // Q3 -> Q1 across origin
            float ax = -5;
            float ay = -5;
            float bx = 0;
            float by = 0;

            // Q2 -> Q4 across origin
            float cx = 0;
            float cy = 0;
            float dx = 5;
            float dy = -5;

            double[] result = Mathy.findIntersection(ax, ay, bx, by, cx, cy, dx, dy);
            Assert.IsNotNull(result);
            Assert.AreEqual(new double[] { 0, 0 }, result);
        }

        [Test]
        public void testFindIntersectionNotOnLineSegments() {
            float ax = -5;
            float ay = -5;
            float bx = -1;
            float by = -1;

            float cx = 0;
            float cy = 0;
            float dx = 5;
            float dy = -5;

            double[] result = Mathy.findIntersection(ax, ay, bx, by, cx, cy, dx, dy);
            Assert.IsNull(result);
        }

        [Test]
        public void testGetEndpointOfLineRotationNinety() {
            double theta = 90.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(0.0, result[0], 5);
            Assert.AreEqual(5.0, result[1], 5);
        }

        [Test]
        public void testGetEndpointOfLineRotationThreeSixty() {
            double theta = 360.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(0.0, result[0], 5);
            Assert.AreEqual(0.0, result[1], 5);
        }

        [Test]
        public void testGetEndpointOfLineRotationQuadrant1() {
            double theta = 45.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(5.0, result[0], 5);
            Assert.AreEqual(5.0, result[1], 5);
        }

        [Test]
        public void testGetEndpointOfLineRotationQuadrant2() {
            double theta = 135.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(-5.0, result[0], 5);
            Assert.AreEqual(5.0, result[1], 5);
        }

        [Test]
        public void testGetEndpointOfLineRotationQuadrant3() {
            double theta = -135.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(-5.0, result[0], 5);
            Assert.AreEqual(-5.0, result[1], 5);
        }

        [Test]
        public void testGetEndpointOfLineRotationQuadrant3Unsigned() {
            double theta = 225.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(-5.0, result[0], 5);
            Assert.AreEqual(-5.0, result[1], 5);
        }

        [Test]
        public void testGetEndpointOfLineRotationQuadrant4() {
            double theta = -45.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(5.0, result[0], 5);
            Assert.AreEqual(-5.0, result[1], 5);
        }

        [Test]
        public void testGetEndpointOfLineRotationQuadrant4Unsigned() {
            double theta = 315.0 * Mathf.Deg2Rad;
            double x = 0;
            double y = 0;
            float distance = 5;

            double[] result = Mathy.getEndpointOfLineRotation(x, y, theta, distance);
            // Check separately due to floating point precision, within 5 points
            Assert.AreEqual(5.0, result[0], 5);
            Assert.AreEqual(-5.0, result[1], 5);
        }
    }
}
