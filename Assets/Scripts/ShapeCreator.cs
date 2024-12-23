using System.Collections.Generic;
using UnityEngine;

public class ShapeCreator : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public GameObject cylinderPrefab;
    public GameObject capsulePrefab;
    public GameObject planePrefab;

    private Stack<GameObject> createdShapes = new Stack<GameObject>(); // Stack to track created shapes

    public void CreateCube()
    {
        Debug.Log("Creating Cube");
        GameObject newCube = Instantiate(cubePrefab, new Vector3(0, 1, 0), Quaternion.identity);
        EnsureMeshCollider(newCube);
        createdShapes.Push(newCube); // Add to undo stack
    }

    public void CreateSphere()
    {
        Debug.Log("Creating Sphere");
        GameObject newSphere = Instantiate(spherePrefab, new Vector3(0, 1, 0), Quaternion.identity);
        EnsureMeshCollider(newSphere);
        createdShapes.Push(newSphere); // Add to undo stack
    }

    public void CreateCylinder()
    {
        Debug.Log("Creating Cylinder");
        GameObject newCylinder = Instantiate(cylinderPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        EnsureMeshCollider(newCylinder);
        createdShapes.Push(newCylinder); // Add to undo stack
    }

    public void CreateCapsule()
    {
        Debug.Log("Creating Capsule");
        GameObject newCapsule = Instantiate(capsulePrefab, new Vector3(0, 1, 0), Quaternion.identity);
        EnsureMeshCollider(newCapsule);
        createdShapes.Push(newCapsule); // Add to undo stack
    }

    public void CreatePlane()
    {
        Debug.Log("Creating Plane");
        GameObject newPlane = Instantiate(planePrefab, new Vector3(0, 1, 0), Quaternion.identity);
        EnsureMeshCollider(newPlane);
        createdShapes.Push(newPlane); // Add to undo stack
    }

    private void EnsureMeshCollider(GameObject obj)
    {
        // Check if a MeshCollider is already present; if not, add one.
        if (obj.GetComponent<MeshCollider>() == null)
        {
            obj.AddComponent<MeshCollider>();
        }
    }

    public SculptingTool sculptingTool;

    public void ToggleSculptingMode(bool isEnabled)
    {
        sculptingTool.enabled = isEnabled;
    }
}