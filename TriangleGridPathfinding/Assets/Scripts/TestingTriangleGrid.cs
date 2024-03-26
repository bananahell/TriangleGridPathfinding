using UnityEngine;

public class TestingTriangleGrid : MonoBehaviour {

  [SerializeField] private Transform trianglePrefab;
  [SerializeField] private Material materialUp;
  [SerializeField] private Material materialDown;
  [SerializeField] private Material materialUnwalkable;

  public Material closedTriangle;
  public Material openTriangle;
  public Material pathTriangle;

  private PathNodeTriangleXZ lastGridObject;
  private PathfindingTriangleXZ pathfindingTriangleXZ;

  public static TestingTriangleGrid Instance { get; private set; }

  private void Awake() {
    int width = 15;
    int height = 6;
    float triangleSide = 0.375f;
    _ = new GridTriangleXZ<PathNodeTriangleXZ>(width,
                                               height,
                                               triangleSide,
                                               Vector3.zero,
                                               (int x, int y) => new PathNodeTriangleXZ(x, y));
    Quaternion rotation;
    for (int x = 0; x < width; x++) {
      for (int z = 0; z < height; z++) {
        bool rotationDir = x % 2 == z % 2;
        rotation = rotationDir ? new Quaternion(0, 0, 0, 0) : new Quaternion(0, 180, 0, 0);
        Transform visualTransform = Instantiate(this.trianglePrefab,
                                                GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWorldPosition(x, z),
                                                rotation);
        Material material = rotationDir ? this.materialUp : this.materialDown;
        visualTransform.Find(Constants.UNSELECTED_STRING).gameObject.GetComponent<Renderer>().material = material;
        GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(x, z).visualTransform = visualTransform;
        GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(x, z).Hide();
      }
    }
    if (Instance != null) {
      Debug.LogError("Instance of PathfindingTriangleXZ already exists!");
    }
    Instance = this;
  }

  private void Start() {
    this.pathfindingTriangleXZ = new PathfindingTriangleXZ();
  }

  private void Update() {
    this.lastGridObject?.Hide();
    this.lastGridObject = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(Mouse3D.GetMouseWorldPosition());
    this.lastGridObject?.Show();
    if (Input.GetMouseButtonDown(0)) {
      _ = this.pathfindingTriangleXZ.FindPath(Vector3.zero, Mouse3D.GetMouseWorldPosition());
    }
    if (Input.GetMouseButtonDown(1)) {
      this.pathfindingTriangleXZ.GetGrid().GetGridObject(Mouse3D.GetMouseWorldPosition()).SetIsWalkable(false);
      GridTriangleXZ<PathNodeTriangleXZ>
        .Instance
        .GetGridObject(Mouse3D.GetMouseWorldPosition())
        .visualTransform
        .Find(Constants.UNSELECTED_STRING)
        .gameObject
        .GetComponent<Renderer>()
        .material = this.materialUnwalkable;
    }
  }

}
