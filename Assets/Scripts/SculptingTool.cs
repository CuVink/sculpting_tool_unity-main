using System.Collections.Generic;
using UnityEngine;

public class SculptingTool : MonoBehaviour
{
    public float sculptRadius = 1f;      // Radius of the sculpting effect
    public float sculptStrength = 0.1f; // Strength of the sculpting effect
    private GameObject targetObject;    // Object being sculpted
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    private enum SculptMode { Push, Pull, Grab, Pinch, Smooth }
    private SculptMode currentMode = SculptMode.Push; // Default mode

    private bool isSculpting = false;
    private bool isSculptModeActive = false; // Flag to track if sculpting mode is enabled

    private Stack<Vector3[]> undoStack = new Stack<Vector3[]>(); // Stack for undo functionality

    void Update()
    {
        if (isSculptModeActive) // Only handle input if sculpting mode is active
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button starts sculpting
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;

                    // If a new object is selected, initialize vertex data
                    if (targetObject != hitObject)
                    {
                        InitializeNewTarget(hitObject);
                    }

                    if (mesh != null)
                    {
                        SaveMeshState(); // Save state before sculpting
                        isSculpting = true;
                        Sculpt(hit.point);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) // Stop sculpting on mouse release
        {
            isSculpting = false;
        }

        if (isSculpting)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Sculpt(hit.point);
            }
        }

        // Undo action (Ctrl + Z)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            UndoLastAction();
        }
    }

    private void InitializeNewTarget(GameObject newTarget)
    {
        targetObject = newTarget;

        // Check if the object has a MeshFilter
        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            mesh = meshFilter.mesh;

            if (mesh != null)
            {
                // Initialize vertex data
                originalVertices = mesh.vertices;
                modifiedVertices = (Vector3[])originalVertices.Clone();
                Debug.Log("Initialized new target: " + targetObject.name);
            }
            else
            {
                Debug.LogWarning("Target object's mesh is null.");
                ClearVertexData();
            }
        }
        else
        {
            Debug.LogWarning("Target object does not have a MeshFilter.");
            ClearVertexData();
        }
    }

    private void ClearVertexData()
    {
        mesh = null;
        originalVertices = null;
        modifiedVertices = null;
    }

    public void SetSculptMode(string mode)
    {
        currentMode = (SculptMode)System.Enum.Parse(typeof(SculptMode), mode);
    }

    public void ToggleSculptMode(bool isActive)
    {
        isSculptModeActive = isActive; // Enable or disable sculpting mode
        Debug.Log("Sculpting mode " + (isActive ? "enabled" : "disabled"));
    }

    public void ToggleObjectMode(bool isActive)
    {
        // Enable or disable Object Mode
        if (isActive)
        {
            isSculptModeActive = false; // Disable Sculpting Mode if Object Mode is enabled
            Debug.Log("Sculpting mode disabled due to Object Mode activation");
        }
        Debug.Log("Object mode " + (isActive ? "enabled" : "disabled"));
    }

    private void Sculpt(Vector3 hitPoint)
    {
        if (modifiedVertices == null || mesh == null)
        {
            Debug.LogWarning("No valid mesh data to sculpt.");
            return;
        }

        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            Vector3 worldVertex = targetObject.transform.TransformPoint(modifiedVertices[i]);
            float distance = Vector3.Distance(worldVertex, hitPoint);

            if (distance < sculptRadius)
            {
                float influence = (1f - distance / sculptRadius) * sculptStrength;

                switch (currentMode)
                {
                    case SculptMode.Push:
                        modifiedVertices[i] -= (hitPoint - worldVertex).normalized * influence;
                        break;

                    case SculptMode.Pull:
                        modifiedVertices[i] += (hitPoint - worldVertex).normalized * influence;
                        break;

                    case SculptMode.Grab:
                        Vector3 grabDirection = (hitPoint - worldVertex).normalized;
                        modifiedVertices[i] += grabDirection * influence;
                        break;

                    case SculptMode.Pinch:
                        Vector3 pinchDirection = (hitPoint - worldVertex).normalized;
                        modifiedVertices[i] += pinchDirection * influence * -1;
                        break;

                    case SculptMode.Smooth:
                        SmoothVertex(i, hitPoint, influence);
                        break;
                }
            }
        }

        UpdateMesh();
    }

    private void SmoothVertex(int index, Vector3 hitPoint, float influence)
    {
        Vector3 average = Vector3.zero;
        int neighborCount = 0;

        // Calculate the average position of nearby vertices
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            if (i == index) continue;

            float distance = Vector3.Distance(
                targetObject.transform.TransformPoint(modifiedVertices[i]),
                targetObject.transform.TransformPoint(modifiedVertices[index])
            );

            if (distance < sculptRadius)
            {
                average += modifiedVertices[i];
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            average /= neighborCount;
            modifiedVertices[index] = Vector3.Lerp(modifiedVertices[index], average, influence);
        }
    }

    private void SaveMeshState()
    {
        if (modifiedVertices != null)
        {
            undoStack.Push((Vector3[])modifiedVertices.Clone());
        }
    }

    private void UndoLastAction()
    {
        if (undoStack.Count > 0)
        {
            modifiedVertices = undoStack.Pop();
            UpdateMesh();
            Debug.Log("Undo successful");
        }
        else
        {
            Debug.Log("Nothing to undo");
        }
    }

    private void UpdateMesh()
    {
        if (mesh == null || modifiedVertices == null)
        {
            Debug.LogWarning("No mesh data available for updating.");
            return;
        }

        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Update the collider to match the modified mesh
        MeshCollider collider = targetObject.GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = null;
            collider.sharedMesh = mesh;
        }
    }
}
