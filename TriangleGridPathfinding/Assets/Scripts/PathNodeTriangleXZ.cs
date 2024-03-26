using UnityEngine;

public class PathNodeTriangleXZ {

  public int x;
  public int z;
  public float gCost;
  public float hCost;
  public float fCost;
  public bool isWalkable;
  public PathNodeTriangleXZ cameFromNode;
  public Transform visualTransform;

  public PathNodeTriangleXZ(int x, int z) {
    this.x = x;
    this.z = z;
    this.isWalkable = true;
  }

  public void CalculateFCost() {
    this.fCost = this.gCost + this.hCost;
  }

  public void SetIsWalkable(bool isWalkable) {
    this.isWalkable = isWalkable;
    GridTriangleXZ<PathNodeTriangleXZ>.Instance.TriggerGridObjectChanged(this.x, this.z);
  }

  public override string ToString() {
    return this.x + "," + this.z;
  }

  public void Show() {
    this.visualTransform.Find(Constants.SELECTED_STRING).gameObject.SetActive(true);
    this.visualTransform.Find(Constants.UNSELECTED_STRING).gameObject.SetActive(false);
  }

  public void Hide() {
    this.visualTransform.Find(Constants.SELECTED_STRING).gameObject.SetActive(false);
    this.visualTransform.Find(Constants.UNSELECTED_STRING).gameObject.SetActive(true);
  }

}
