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

        [Tooltip("Height at which to render the graphs.")]
        public float renderHeight = 0.5f;

        [Tooltip("Whether to render the main Delaunay triangulation.")]
        public bool renderDelaunay = true;

        [Tooltip("Color used to draw the main Delaunay triangulation.")]
        public Color delaunayColor;

        [Tooltip("Whether to render the Voronoi diagram that is the dual of the main Delaunay triangulation.")]
        public bool renderVoronoi = true;

        [Tooltip("Color used to draw the Voronoi diagram.")]
        public Color voronoiColor;

        [Tooltip("Whether to render the Delaunay triangulation of the master points.")]
        public bool renderMasterDelaunay = true;

        [Tooltip("Color used to draw the master points' Delaunay triangulation.")]
        public Color masterDelaunayColor;

        [Tooltip("Whether to render the spanning tree of the master cells. The spanning tree is used to create corridors along any Voronoi cell it intersects.")]
        public bool renderSpanningTree = true;

        [Tooltip("Color used to draw the spanning tree.")]
        public Color spanningTreeColor;

        [Tooltip("Material with a specific shader to color lines properly in game view. Don't change it unless you have a good reason!")]
        public Material edgeMat;

        private void renderDelaunayTriangulation(Delaunay del, Color color) {
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

        private void renderEdges(List<Edge> edges, Color color) {
            GL.PushMatrix();

            foreach (Edge e in edges) {
                GL.Begin(GL.LINES);
                setGraphColors(color);

                // cheap hack to not display at same height as delaunay, it gets rendered weird
                // TODO make more configurable when more graphs displayed
                Vector3 a = e.vertices[0].toVector3AtHeight(renderHeight+0.5f);
                Vector3 b = e.vertices[1].toVector3AtHeight(renderHeight+0.5f);

                // ab
                GL.Vertex3(a.x, a.y, a.z);
                GL.Vertex3(b.x, b.y, b.z);

                GL.End();
            }

            GL.PopMatrix();
        }

        private void renderAllSelected() {
            if (renderDelaunay && dungen.dt != null) {
                renderDelaunayTriangulation(dungen.dt, delaunayColor);
            }
            if (renderVoronoi && dungen.vd != null) {
                renderEdges(dungen.vd.edges, voronoiColor);
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
