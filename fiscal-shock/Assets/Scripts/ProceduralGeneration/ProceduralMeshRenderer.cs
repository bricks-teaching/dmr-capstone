using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FiscalShock.Graphs;
using FiscalShock.Procedural;

/// <summary>
/// This script must be attached to a camera to work!
/// </summary>
namespace FiscalShock.Demo {
    public class ProceduralMeshRenderer : MonoBehaviour {
        public Dungeoneer dungen { get; set; }

        [Tooltip("Prefab object used for rendering points.")]
        public GameObject pointPrefab;

        [Tooltip("Whether to render the main Delaunay triangulation.")]
        public bool renderDelaunay = true;

        [Tooltip("Render Delaunay vertices.")]
        public bool renderDelaunayVertices = true;

        public bool renderDelaunayHull = true;

        [Tooltip("Color used to draw the main Delaunay triangulation.")]
        public Color delaunayColor = new Color(1, 0, 1);

        [Tooltip("Height at which to render the Delaunay triangulation.")]
        public float delaunayRenderHeight = 1.1f;

        [Tooltip("Whether to render the Voronoi diagram that is the dual of the main Delaunay triangulation.")]
        public bool renderVoronoi = true;

        [Tooltip("Color used to draw the Voronoi diagram.")]
        public Color voronoiColor = new Color(0, 1, 1);

        [Tooltip("Height at which to render the Voronoi diagram.")]
        public float voronoiRenderHeight = 1.6f;

        [Tooltip("Whether to render the Delaunay triangulation of the master points.")]
        public bool renderMasterDelaunay = true;

        [Tooltip("Color used to draw the master points' Delaunay triangulation.")]
        public Color masterDelaunayColor = new Color(1, 0, 0);

        [Tooltip("Height at which to render the master points' Delaunay triangulation.")]
        public float masterDelaunayRenderHeight = 1.3f;

        [Tooltip("Render master vertices.")]
        public bool renderMasterVertices = true;

        [Tooltip("Whether to render the spanning tree of the master cells. The spanning tree is used to create corridors along any Voronoi cell it intersects.")]
        public bool renderSpanningTree = true;

        [Tooltip("Color used to draw the spanning tree.")]
        public Color spanningTreeColor = new Color(0, 0, 1);

        [Tooltip("Height at which to render the spanning tree.")]
        public float spanningTreeRenderHeight = 1.4f;

        [Tooltip("Whether to render the Voronoi cells representing rooms.")]
        public bool renderRooms = true;

        [Tooltip("Color used to draw the room Voronoi.")]
        public Color roomColor = new Color(0, 0, 1);

        [Tooltip("Height at which to render the room Voronoi.")]
        public float roomRenderHeight = 3f;

        [Tooltip("Material with a specific shader to color lines properly in game view. Don't change it unless you have a good reason!")]
        public Material edgeMat;

        [Tooltip("Whether to render the Voronoi diagram used for A* navigation.")]
        public bool renderNavigableGraph;

        [Tooltip("Color of the navigable graph vertices.")]
        public Color navigableGraphColor = new Color(215, 35, 187);

        [Tooltip("Height at which the navigable Voronoi should render.")]
        public float navigableRenderHeight = 2.4f;

        public TextMesh label { get; private set; } = new TextMesh();
        public bool alreadyDrew { get; set; }

        private void Start() {
            label = GameObject.Find("ProceduralMeshLabel").GetComponent<TextMesh>();
            dungen = GameObject.Find("DungeonSummoner").GetComponent<Dungeoneer>();
        }

        private void renderDelaunayTriangulation(Delaunay del, Color color, float renderHeight) {
            // Start immediate mode drawing for lines
            GL.PushMatrix();

            foreach (Triangle tri in del.triangles) {
                GL.Begin(GL.LINES);
                setGraphColors(color);

                Vector3 a = tri.a.toVector3AtHeight(renderHeight);
                Vector3 b = tri.b.toVector3AtHeight(renderHeight);
                Vector3 c = tri.c.toVector3AtHeight(renderHeight);

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

                Vector3 a = e.p.toVector3AtHeight(renderHeight);
                Vector3 b = e.q.toVector3AtHeight(renderHeight);

                // ab
                GL.Vertex3(a.x, a.y, a.z);
                GL.Vertex3(b.x, b.y, b.z);

                GL.End();
            }

            GL.PopMatrix();
        }

        /// <summary>
        /// Unity doesn't have GL.POINTS `¯\_(ツ)_/¯`
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        /// <param name="renderHeight"></param>
        private void renderPoints(List<Vertex> points, Color color, float renderHeight) {
            if (alreadyDrew) {
                return;
            }
            GameObject holder = new GameObject();
            holder.name = "Vertices Display";
            foreach (Vertex v in points) {
                GameObject tmp = Instantiate(pointPrefab);
                // TODO set scale here in case it should be fatter
                Material pointMat = tmp.GetComponent<Renderer>().material;
                pointMat.SetColor(Shader.PropertyToID("_Color"), color);
                tmp.transform.position = v.toVector3AtHeight(renderHeight);
                tmp.name = $"Delaunay #{v.id}";
                tmp.transform.parent = holder.transform;

                if (label == null) {
                    continue;
                }

                float offsetPosY = tmp.transform.position.y + 1.5f;
                Vector3 offsetPos = new Vector3(tmp.transform.position.x, offsetPosY, tmp.transform.position.z);

                TextMesh texto = Instantiate(label);
                texto.transform.position = offsetPos;
                texto.text = v.id.ToString();
                texto.name = $"Label for #{v.id}";
                texto.transform.parent = holder.transform;
            }
            alreadyDrew = true;
        }

        private void renderAllSelected() {
            if (dungen == null) {
                return;
            }
            if (renderDelaunay && dungen.dt != null) {
                renderDelaunayTriangulation(dungen.dt, delaunayColor, delaunayRenderHeight);
            }
            if (renderDelaunayVertices && dungen.dt != null) {
                renderPoints(dungen.dt.vertices, delaunayColor, delaunayRenderHeight);
            }
            if (renderDelaunayHull && dungen.dt != null) {
                // TODO not same color as triangulation
                renderEdges(dungen.dt.convexHullEdges, delaunayColor, delaunayRenderHeight + 15f);
            }
            if (renderVoronoi && dungen.vd != null) {
                renderEdges(dungen.vd.edges, voronoiColor, voronoiRenderHeight);
                // Voronoi cells
                /*
                List<Edge> es = dungen.vd.cells.SelectMany(c => c.sides).ToList();
                var ef = es.Distinct().ToList();
                renderEdges(ef, spanningTreeColor, voronoiRenderHeight + 0.5f);
                */
            }
            if (renderMasterDelaunay && dungen.masterDt != null) {
                renderDelaunayTriangulation(dungen.masterDt, masterDelaunayColor, masterDelaunayRenderHeight);
            }
            if (renderMasterVertices && dungen.masterDt != null) {
                renderPoints(dungen.masterDt.vertices, masterDelaunayColor, masterDelaunayRenderHeight);
            }
            if (renderSpanningTree && dungen.spanningTree != null) {
                renderEdges(dungen.spanningTree, spanningTreeColor, spanningTreeRenderHeight);
            }
            if (renderRooms && dungen.roomVoronoi != null) {
                List<Edge> es = dungen.roomVoronoi.SelectMany(c => c.allEdges).ToList();

                //renderEdges(es, roomColor, roomRenderHeight);
                // room edges only
                List<Edge> ext = dungen.roomVoronoi.SelectMany(c => c.exterior.sides).ToList();
                renderEdges(ext, delaunayColor, roomRenderHeight+ 5f);
            }

            if (renderNavigableGraph && dungen.navigableGraph != null) {
                renderEdges(dungen.navigableGraph.edges, navigableGraphColor, navigableRenderHeight);
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
