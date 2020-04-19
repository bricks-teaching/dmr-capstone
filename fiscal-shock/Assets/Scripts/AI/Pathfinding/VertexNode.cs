using UnityEngine;
using System;
using FiscalShock.Graphs;

namespace FiscalShock.Pathfinding {
    public class VertexNode : IEquatable<VertexNode> {
        public Vertex associatedLocation { get; }
        public VertexNode previousOnPath { get; private set; }
        public double gCost { get; private set; }
        public double hCost { get; private set; }
        public double fCost { get; private set; }

        public VertexNode(Vertex associatedVertex) {
            associatedLocation = associatedVertex;
            previousOnPath = null;
        }

        public VertexNode(Vertex associatedVertex, double g, double h) {
            associatedLocation = associatedVertex;
            gCost = g;
            hCost = h;
            fCost = g + h;
            previousOnPath = null;
        }

        public void setLinkToPrevious(VertexNode parent) {
            previousOnPath = parent;
        }

        public void setCosts(double g, double h) {
            gCost = g;
            hCost = h;
            fCost = g+h;
        }

        public void changeGCost(double newG) {
            gCost = newG;
            fCost = newG + hCost;
        }

        public bool Equals(VertexNode other) {
            return associatedLocation.Equals(other.associatedLocation);
        }

        public override bool Equals(object obj) {
            if (obj is VertexNode other) {
                return associatedLocation.Equals(other.associatedLocation);
            }

            return false;
        }

        // Source:
        // https://codereview.stackexchange.com/questions/164970/using-gethashcode-in-equals
        // https://stackoverflow.com/questions/720177/default-implementation-for-object-gethashcode/720282#720282
        // This should work -- only VertexNodes being stored in any data structure -- but proceed with caution.
        public override int GetHashCode() {
            return associatedLocation.GetHashCode();
        }
    }
}
