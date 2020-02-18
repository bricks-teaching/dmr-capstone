using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FiscalShock.Graphs;
using FiscalShock.Procedural;

/// <summary>
/// This script must be attached to a camera to work!
/// </summary>
namespace FiscalShock.Demo {
    public class ProceduralMeshRenderer : MonoBehaviour {
        [Tooltip("Link a Dungeoneer script to render its graphs.")]
        public Dungeoneer dungen;

        [Tooltip("Prefab object used for rendering points.")]
        public GameObject pointPrefab;

        [Tooltip("Whether to render the main Delaunay triangulation.")]
        public bool renderDelaunay = true;

        [Tooltip("Render Delaunay vertices. Looks funny when you also render the triangulation, so don't do that.")]
        public bool renderDelaunayVertices = true;

        [Tooltip("Color used to draw the main Delaunay triangulation.")]
        public Color delaunayColor;

        [Tooltip("Height at which to render the Delaunay triangulation.")]
        public float delaunayRenderHeight = 1.1f;

        [Tooltip("Whether to render the Voronoi diagram that is the dual of the main Delaunay triangulation.")]
        public bool renderVoronoi = true;

        [Tooltip("Color used to draw the Voronoi diagram.")]
        public Color voronoiColor;

        [Tooltip("Height at which to render the Voronoi diagram.")]
        public float voronoiRenderHeight = 1.6f;

        [Tooltip("Whether to render the Delaunay triangulation of the master points.")]
        public bool renderMasterDelaunay = true;

        [Tooltip("Color used to draw the master points' Delaunay triangulation.")]
        public Color masterDelaunayColor;

        [Tooltip("Height at which to render the master points' Delaunay triangulation.")]
        public float masterDelaunayRenderHeight = 1.3f;

        [Tooltip("Whether to render the spanning tree of the master cells. The spanning tree is used to create corridors along any Voronoi cell it intersects.")]
        public bool renderSpanningTree = true;

        [Tooltip("Color used to draw the spanning tree.")]
        public Color spanningTreeColor;

        [Tooltip("Height at which to render the spanning tree.")]
        public float spanningTreeRenderHeight = 1.4f;

        [Tooltip("Material with a specific shader to color lines properly in game view. Don't change it unless you have a good reason!")]
        public Material edgeMat;

        private bool alreadyDrewPoints = false;

        private void renderDelaunayTriangulation(Delaunay del, Color color, float renderHeight) {
            // Start immediate mode drawing for lines
            GL.PushMatrix();

            foreach (Triangle t in del.triangles) {
                GL.Begin(GL.LINES);
                setGraphColors(color);

                Vector3 a = t.vertices[0].toVector3AtHeight(renderHeight);
                Vector3 b = t.vertices[1].toVector3AtHeight(renderHeight);
                Vector3 c = t.vertices[2].toVector3AtHeight(renderHeight);

                // ab
                GL.Vertex3(a.x, a.y, a.z);
                GL.Vertex3(b.x, b.y, b.z);
                // bc
                GL.Vertex3(c.x, c.y, c.z);
                // ca
                GL.Vertex3(a.x, a.y, a.z);

                GL.End();
            }

            GL.PopMatrix();
        }

        private void renderEdges(List<Edge> edges, Color color, float renderHeight) {
            GL.PushMatrix();

            foreach (Edge e in edges) {
                GL.Begin(GL.LINES);
                setGraphColors(color);

                Vector3 a = e.vertices[0].toVector3AtHeight(renderHeight);
                Vector3 b = e.vertices[1].toVector3AtHeight(renderHeight);

                // ab
                GL.Vertex3(a.x, a.y, a.z);
                GL.Vertex3(b.x, b.y, b.z);

                GL.End();
            }

            GL.PopMatrix();
        }

        /// <summary>
        /// Unity doesn't have GL.POINTS `¯\_(ツ)_/¯`
        /// <para>But hey, look at how easy it is to programmatically spawn stuff!
        /// *And* give its material a new color!</para>
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        /// <param name="renderHeight"></param>
        private void renderPoints(List<Vertex> points, Color color, float renderHeight) {
            foreach (Vertex v in points) {
                GameObject tmp = Instantiate(pointPrefab);
                // TODO set scale here in case it should be fatter
                Material pointMat = tmp.GetComponent<Renderer>().material;
                pointMat.SetColor(Shader.PropertyToID("_Color"), color);
                tmp.transform.position = v.toVector3AtHeight(renderHeight);
            }
            alreadyDrewPoints = true; // TODO one for each points to render I guess
        }

        private void renderAllSelected() {
            if (renderDelaunay && dungen.dt != null) {
                renderDelaunayTriangulation(dungen.dt, delaunayColor, delaunayRenderHeight);
            }
            if (renderDelaunayVertices && dungen.dt != null && !alreadyDrewPoints) {
               renderPoints(dungen.dt.vertices, delaunayColor, delaunayRenderHeight);
            }
            if (renderVoronoi && dungen.vd != null) {
                renderEdges(dungen.vd.edges, voronoiColor, voronoiRenderHeight);
                // TEMPORARY testing cells
                //List<Edge> es = dungen.vd.cells.SelectMany(c => c.sides).ToList();
                //renderEdges(es, spanningTreeColor, voronoiRenderHeight + 0.5f);
            }
        }

        private void setGraphColors(Color color) {
            edgeMat.SetPass(0);
            edgeMat.SetColor(Shader.PropertyToID("_Color"), color);  // set game view color
            GL.Color(color);  // set editor color
        }

        // Display in game
        public void OnPostRender() {
            renderAllSelected();
        }

        // Display in editor
        public void OnDrawGizmos() {
            renderAllSelected();
        }
    }
}
