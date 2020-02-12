using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using FiscalShock.Graphs;

/// <summary>
/// This script must be attached to a camera to work!
/// </summary>
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

    void renderLines(List<Edge> edges, Color color) {
        // Start immediate mode drawing for lines
        GL.PushMatrix();

        //foreach (Edge e in edges) {
        //    GL.Begin(GL.LINES);
        //    GL.Color(color);
        //    Vector3 a = e.vertices[0].toVector3AtHeight(renderHeight);
        //    Vector3 b = e.vertices[1].toVector3AtHeight(renderHeight);

        //    // Connect two vertices
        //    GL.Vertex3(a.x, a.y, a.z);
        //    GL.Vertex3(b.x, b.y, b.z);
        //    GL.End();
        //}

        // drawing every triangle
        for (int i = 0; i < dungen.dt.triangulation.triangles.Count; i += 3) {
            GL.Begin(GL.LINES);
            GL.Color(color);
            float[] verts = dungen.dt.getTriangleVertices(i);
            Vector3 a = new Vector3(verts[0], renderHeight, verts[1]);
            Vector3 b = new Vector3(verts[2], renderHeight, verts[3]);
            Vector3 c = new Vector3(verts[4], renderHeight, verts[5]);

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
        Debug.Log($"Drew {edges.Count} edges.");
    }

    void renderAllSelected() {
        if (renderDelaunay && dungen.dt != null) {
            renderLines(dungen.dt.edges, delaunayColor);
        }
    }

    // Display in game
    void OnPostRender() {
        renderAllSelected();
    }

    // Display in editor
    void OnDrawGizmos() {
        renderAllSelected();
    }
}
