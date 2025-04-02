using System.Collections.Generic;
using UnityEngine;

public class ObjInteract : MonoBehaviour
{
    [SerializeField] private GameObject pointToTransform;
    private Transform examinedObject;
    private Vector3 lastMousePosition;
    private Dictionary<Transform, Vector3> originalPos = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> originalRot = new Dictionary<Transform, Quaternion>();
    private Camera m_Camera;
    Vector3 startPos;
    Quaternion startRot;
    private float rotationSpeed = 5f;
    private bool isExamining = false;
    private float lerpSpeed = 3f; // Speed for smooth transition

    void Start()
    {
        m_Camera = Camera.main;
        Vector3 startPos = examinedObject.position;
        Quaternion startRot = examinedObject.rotation;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            TrySelectObject();
        }

        if (isExamining && examinedObject != null)
        {
            RotateObject();
        }

        if (Input.GetMouseButtonDown(1)) // Right click to stop examining
        {
            NonExamine();
        }
    }

    void TrySelectObject()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = m_Camera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Object"))
            {
                examinedObject = hit.collider.transform;

                // Store original position & rotation
                if (!originalPos.ContainsKey(examinedObject))
                {
                    originalPos[examinedObject] = examinedObject.position;
                    originalRot[examinedObject] = examinedObject.rotation;
                }

                isExamining = true;
                lastMousePosition = Input.mousePosition;
                StartCoroutine(MoveToExaminePosition());
            }
            else
            {
                NonExamine();
            }
        }
    }

    System.Collections.IEnumerator MoveToExaminePosition()
    {
        float timeElapsed = 0f;
        Vector3 startPos = examinedObject.position;
        Quaternion startRot = examinedObject.rotation;

        while (timeElapsed < 1f)
        {
            if (examinedObject == null) yield break; // Exit if object is lost
            examinedObject.position = Vector3.Lerp(startPos, pointToTransform.transform.position, timeElapsed);
            examinedObject.rotation = Quaternion.Slerp(startRot, Quaternion.identity, timeElapsed);
            timeElapsed += Time.deltaTime * lerpSpeed;
            yield return null;
        }
    }

    void RotateObject()
    {
        if (Input.GetMouseButton(0)) // Rotate while dragging
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;
            examinedObject.Rotate(Vector3.up, -deltaMouse.x * rotationSpeed * Time.deltaTime, Space.World);
            examinedObject.Rotate(Vector3.right, deltaMouse.y * rotationSpeed * Time.deltaTime, Space.World);
            lastMousePosition = Input.mousePosition;
        }
    }

    void NonExamine()
    {
        if (examinedObject != null)
        {
            StartCoroutine(MoveBackToOriginalPosition());
            isExamining = false;
            examinedObject = null;
        }
    }

    System.Collections.IEnumerator MoveBackToOriginalPosition()
    {
        float duration = 1f; // Adjust if needed
        float timeElapsed = 0f;
        Vector3 targetPos = originalPos[examinedObject];
        Quaternion targetRot = originalRot[examinedObject];

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration; // Normalize time for smooth interpolation
            examinedObject.position = Vector3.Lerp(startPos, targetPos, t);
            examinedObject.rotation = Quaternion.Slerp(startRot, targetRot, t);
            timeElapsed += Time.deltaTime * lerpSpeed;
            yield return null;
        }

        // Ensure exact final position & rotation
        examinedObject.position = targetPos;
        examinedObject.rotation = targetRot;
    }

}
