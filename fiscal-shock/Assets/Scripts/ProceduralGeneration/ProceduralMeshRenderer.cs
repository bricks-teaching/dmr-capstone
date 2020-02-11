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
        GL.Begin(GL.LINES);
        GL.Color(color);

        foreach (Edge e in edges) {
            Vector3 a = e.vertices[0].toVector3AtHeight(renderHeight);
            Vector3 b = e.vertices[1].toVector3AtHeight(renderHeight);

            // Connect two vertices
            GL.Vertex3(a.x, a.y, a.z);
            GL.Vertex3(b.x, b.y, b.z);
        }

        GL.End();
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
