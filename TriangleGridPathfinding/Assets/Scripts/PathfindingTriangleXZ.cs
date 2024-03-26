using System;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTriangleXZ {

  private readonly Material closedTriangle;
  private readonly Material openTriangle;
  private readonly Material pathTriangle;

  private List<PathNodeTriangleXZ> openList;
  private List<PathNodeTriangleXZ> closedList;

  public static PathfindingTriangleXZ Instance { get; private set; }

  public PathfindingTriangleXZ() {
    if (Instance != null) {
      Debug.LogError("Instance of PathfindingTriangleXZ already exists!");
    }
    Instance = this;
    this.closedTriangle = TestingTriangleGrid.Instance.closedTriangle;
    this.openTriangle = TestingTriangleGrid.Instance.openTriangle;
    this.pathTriangle = TestingTriangleGrid.Instance.pathTriangle;
  }

  public GridTriangleXZ<PathNodeTriangleXZ> GetGrid() {
    return GridTriangleXZ<PathNodeTriangleXZ>.Instance;
  }

  public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
    GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetXZ(startWorldPosition, out int startX, out int startY);
    GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetXZ(endWorldPosition, out int endX, out int endY);
    List<PathNodeTriangleXZ> path = this.FindPath(startX, startY, endX, endY);
    if (path == null) {
      return null;
    } else {
      List<Vector3> vectorPath = new();
      foreach (PathNodeTriangleXZ pathNode in path) {
        vectorPath.Add(GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWorldPosition(pathNode.x, pathNode.z));
      }
      return vectorPath;
    }
  }

  public List<PathNodeTriangleXZ> FindPath(int startX, int startY, int endX, int endY) {
    PathNodeTriangleXZ startNode = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(startX, startY);
    PathNodeTriangleXZ endNode = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(endX, endY);
    if (startNode == null || endNode == null) {
      return null;
    }
    this.openList = new List<PathNodeTriangleXZ> { startNode };
    GridTriangleXZ<PathNodeTriangleXZ>
      .Instance
      .GetGridObject(startNode.x, startNode.z)
      .visualTransform
      .Find(Constants.UNSELECTED_STRING)
      .gameObject
      .GetComponent<Renderer>()
      .material = this.openTriangle;
    this.closedList = new List<PathNodeTriangleXZ>();
    for (int x = 0; x < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWidth(); x++) {
      for (int y = 0; y < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetHeight(); y++) {
        PathNodeTriangleXZ pathNode = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(x, y);
        pathNode.gCost = 99999999f;
        pathNode.CalculateFCost();
        pathNode.cameFromNode = null;
      }
    }
    startNode.gCost = 0;
    startNode.hCost = this.CalculateDistanceCost(startNode, endNode);
    startNode.CalculateFCost();
    while (this.openList.Count > 0) {
      PathNodeTriangleXZ currentNode = this.GetLowestFCostNode(this.openList);
      if (currentNode == endNode) {
        return this.CalculatePath(endNode);
      }
      _ = this.openList.Remove(currentNode);
      this.closedList.Add(currentNode);
      GridTriangleXZ<PathNodeTriangleXZ>
        .Instance
        .GetGridObject(currentNode.x, currentNode.z)
        .visualTransform.Find(Constants.UNSELECTED_STRING)
        .gameObject
        .GetComponent<Renderer>()
        .material = this.closedTriangle;
      foreach (PathNodeTriangleXZ neighbourNode in this.GetNeighbourList(currentNode)) {
        if (this.closedList.Contains(neighbourNode)) {
          continue;
        }
        if (!neighbourNode.isWalkable) {
          this.closedList.Add(neighbourNode);
          continue;
        }
        float tentativeGCost = currentNode.gCost + this.CalculateDistanceCost(currentNode, neighbourNode);
        if (tentativeGCost < neighbourNode.gCost) {
          neighbourNode.cameFromNode = currentNode;
          neighbourNode.gCost = tentativeGCost;
          neighbourNode.hCost = this.CalculateDistanceCost(neighbourNode, endNode);
          neighbourNode.CalculateFCost();
          if (!this.openList.Contains(neighbourNode)) {
            this.openList.Add(neighbourNode);
            GridTriangleXZ<PathNodeTriangleXZ>
              .Instance
              .GetGridObject(neighbourNode.x, neighbourNode.z)
              .visualTransform
              .Find(Constants.UNSELECTED_STRING)
              .gameObject
              .GetComponent<Renderer>()
              .material = this.openTriangle;
          }
        }
      }
    }
    return null;
  }

  private List<PathNodeTriangleXZ> GetNeighbourList(PathNodeTriangleXZ currentNode) {
    List<PathNodeTriangleXZ> neighbourList = new();
    if (currentNode.x % 2 == currentNode.z % 2) {
      if (currentNode.x - 1 >= 0) {
        neighbourList.Add(this.GetNode(currentNode.x - 1, currentNode.z));
        if (currentNode.x - 2 >= 0 && currentNode.z - 1 >= 0) {
          neighbourList.Add(this.GetNode(currentNode.x - 2, currentNode.z - 1));
        }
      }
      if (currentNode.x + 1 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWidth()) {
        neighbourList.Add(this.GetNode(currentNode.x + 1, currentNode.z));
        if (currentNode.x + 2 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWidth() && currentNode.z - 1 >= 0) {
          neighbourList.Add(this.GetNode(currentNode.x + 2, currentNode.z - 1));
        }
      }
      if (currentNode.z - 1 >= 0) {
        neighbourList.Add(this.GetNode(currentNode.x, currentNode.z - 1));
      }
      if (currentNode.z + 1 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetHeight()) {
        neighbourList.Add(this.GetNode(currentNode.x, currentNode.z + 1));
      }
    } else {
      if (currentNode.x - 1 >= 0) {
        neighbourList.Add(this.GetNode(currentNode.x - 1, currentNode.z));
        if (currentNode.x - 2 >= 0 && currentNode.z + 1 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetHeight()) {
          neighbourList.Add(this.GetNode(currentNode.x - 2, currentNode.z + 1));
        }
      }
      if (currentNode.z + 1 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetHeight()) {
        neighbourList.Add(this.GetNode(currentNode.x, currentNode.z + 1));
      }
      if (currentNode.x + 1 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWidth()) {
        neighbourList.Add(this.GetNode(currentNode.x + 1, currentNode.z));
        if (currentNode.x + 2 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWidth()
              && currentNode.z + 1 < GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetHeight()) {
          neighbourList.Add(this.GetNode(currentNode.x + 2, currentNode.z + 1));
        }
      }
      if (currentNode.z - 1 >= 0) {
        neighbourList.Add(this.GetNode(currentNode.x, currentNode.z - 1));
      }
    }
    return neighbourList;
  }

  public PathNodeTriangleXZ GetNode(int x, int z) {
    return GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(x, z);
  }

  private List<PathNodeTriangleXZ> CalculatePath(PathNodeTriangleXZ endNode) {
    List<PathNodeTriangleXZ> path = new() {
      endNode
    };
    PathNodeTriangleXZ currentNode = endNode;
    GridTriangleXZ<PathNodeTriangleXZ>
      .Instance
      .GetGridObject(currentNode.x, currentNode.z)
      .visualTransform
      .Find(Constants.UNSELECTED_STRING)
      .gameObject
      .GetComponent<Renderer>()
      .material = this.pathTriangle;
    while (currentNode.cameFromNode != null) {
      path.Add(currentNode.cameFromNode);
      currentNode = currentNode.cameFromNode;
      GridTriangleXZ<PathNodeTriangleXZ>
        .Instance
        .GetGridObject(currentNode.x, currentNode.z)
        .visualTransform
        .Find(Constants.UNSELECTED_STRING)
        .gameObject
        .GetComponent<Renderer>()
        .material = this.pathTriangle;
    }
    path.Reverse();
    return path;
  }

  private float CalculateDistanceCost(PathNodeTriangleXZ a, PathNodeTriangleXZ b) {
    float sideOne = a.x - b.x;
    float sideTwo = a.z - b.z;
    return (float)(Math.Pow(sideOne, 2) + Math.Pow(sideTwo, 2));
  }

  private PathNodeTriangleXZ GetLowestFCostNode(List<PathNodeTriangleXZ> pathNodeList) {
    PathNodeTriangleXZ lowestFCostNode = pathNodeList[0];
    for (int i = 1; i < pathNodeList.Count; i++) {
      if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
        lowestFCostNode = pathNodeList[i];
      }
    }
    return lowestFCostNode;
  }

}
