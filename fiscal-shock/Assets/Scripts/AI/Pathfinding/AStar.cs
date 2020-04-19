using UnityEngine;
using System;
using System.Collections.Generic;
using FiscalShock.Graphs;
using System.IO;

namespace FiscalShock.Pathfinding {
    public class AStar {
        private Voronoi navGraph;

        // Will have to change the script execution order again.
        public AStar(Voronoi graph) {
            navGraph = graph;

            // DEBUG: Remove, or set debugging code.
            Debug.Log("AStar is working. Graph: " + navGraph);
            StreamWriter writer = new StreamWriter("/home/ybautista/Desktop/UnityOutput/cell_vertices.txt");

            foreach (Cell cell in navGraph.cells) {
                writer.Write("CELL " + cell.id + ": " + cell.site.vector + "\n");

                foreach (Vertex vertex in cell.vertices) {
                    writer.Write("\tVERTEX: " + vertex.vector + " " + vertex.walkable + "\n");
                }
            }

            writer.Close();
        }

        // Don't need to return a list of vertex nodes because each vertex node is added with new()
        public Stack<Vertex> findPath(Vertex lastVisitedNode, Vertex destination) {
            Stack<Vertex> path = new Stack<Vertex>();

            if (lastVisitedNode.Equals(destination)) {
                return path;
            }

            // Currently don't have to resort since only added first node.
            LinkedList<VertexNode> open = new LinkedList<VertexNode>();
            open.AddFirst(new VertexNode(lastVisitedNode, 0, destination.getDistanceTo(lastVisitedNode)));
            open.First.Value.setLinkToPrevious(null);

            // Create the closed set with a custom comparison operator.
            LinkedList<VertexNode> closed = new LinkedList<VertexNode>();

            // Set current node to only element in list.
            LinkedListNode<VertexNode> currentNode = open.Last;

            double cost;
            LinkedListNode<VertexNode> openTemp, closedTemp;

            // Approximate equality should help avoid bugs with floating point values.
            while (!currentNode.Value.associatedLocation.Equals(destination)) {
                // DEBUG: Debugging code. Remove or refactor for debugging mode.
                // Debug.Log("Current: " + currentNode.Value.associatedLocation.vector);
                // Debug.Log("Desination: " + destination.vector);

                // TODO: Will still need to account for when this gets done in a loop
                // NOTE: Perhaps this accounted for already, since the smallest value should always be last?
                open.RemoveLast();

                // TODO: Need to add check to make sure not trying to insert into empty list
                // Can just add that yourself.

                closed.AddFirst(currentNode);
                foreach (Vertex neighbor in currentNode.Value.associatedLocation.neighborhood) {
                    cost = currentNode.Value.gCost + currentNode.Value.associatedLocation.getDistanceTo(neighbor);
                    VertexNode neighborNode = new VertexNode(neighbor);

                    // Find returns null in any case where the node doesn't exist.
                    // openTemp should never be filled at the same time as closedTemp
                    openTemp = open.Find(neighborNode);
                    // GCost should never be empty.
                    if (openTemp != null && openTemp.Value.gCost > cost) {
                        open.Remove(openTemp);
                    }

                    closedTemp = closed.Find(neighborNode);
                    if (closedTemp != null && closedTemp.Value.gCost > cost) {
                        closed.Remove(closedTemp);
                    }

                    if (!open.Contains(neighborNode) && !closed.Contains(neighborNode)) {
                        // NeighborNode is a different instance of VertexNode and, thus,
                        // needs to be set to instance that already existed in lists.
                        if (openTemp != null) {
                            neighborNode = openTemp.Value;
                            neighborNode.changeGCost(cost);
                        }

                        else if (closedTemp != null) {
                            neighborNode = closedTemp.Value;
                            neighborNode.changeGCost(cost);
                        }

                        else {
                            neighborNode.setCosts(cost, destination.getDistanceTo(neighborNode.associatedLocation));
                        }

                        neighborNode.setLinkToPrevious(currentNode.Value);
                        sortedAdd(open, neighborNode);
                    }
                }

                currentNode = open.Last;

                // DEBUG: Debug code. Remove or refactor for debugging mode.
                // Debug.Log("CLOSED COUNT: " + closed.Count);
                // Debug.Log("OPEN COUNT: " + open.Count);
            }

            VertexNode node = currentNode.Value;

            int i = 0;
            while (node.previousOnPath != null) {
                path.Push(node.associatedLocation);
                node = node.previousOnPath;
                i++;
            }

            // DEBUG: Debug code. Remove or refactor for debugging mode.
            Debug.Log("TOTAL NODES PASSED: " + i);

            return path;
        }

        // Will take the element to add it via insertion sort to correct position.
        // Yeah, this is an anti-pattern. But it was the best way to generalize it last minute.
        private void sortedAdd(LinkedList<VertexNode> list, VertexNode node) {
            LinkedListNode<VertexNode> currentNode = list.First;

            if (list.Count == 0) {
                list.AddFirst(node);
                return;
            }

            while(true) {
                if(currentNode.Value.fCost == node.fCost) {
                    if(node.hCost >= currentNode.Value.hCost) {
                        list.AddBefore(currentNode, node);
                        break;
                    }
                }

                if(node.fCost > currentNode.Value.fCost) {
                    list.AddBefore(currentNode, node);
                    break;
                }

                if(currentNode.Next == null) {
                    list.AddAfter(currentNode, node);
                    break;
                }

                currentNode = currentNode.Next;
            }
        }
    }
}
