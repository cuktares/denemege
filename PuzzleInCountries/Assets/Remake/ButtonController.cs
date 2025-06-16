using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ButtonAction
{
    public GameObject targetObject;
    public enum ActionType { Move, Bridge }
    public ActionType actionType;

    // Hareket ayarları
    public Vector3 moveDirection;
    public float moveDistance;
    public float moveSpeed;
    public bool resetOnRelease = true;
    
    [HideInInspector]
    public Vector3 originalPosition;
}

public class ButtonController : MonoBehaviour
{
    public List<ButtonAction> actions = new List<ButtonAction>();
    private bool isPressed = false;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;

    private void Start()
    {
        originalPosition = transform.position;
        pressedPosition = originalPosition - new Vector3(0, 0.1f, 0);
        
        // Her bir action için orijinal konumu kaydet
        foreach (ButtonAction action in actions)
        {
            if (action.targetObject != null)
            {
                action.originalPosition = action.targetObject.transform.position;
            }
            
            if (action.actionType == ButtonAction.ActionType.Bridge)
            {
                HideBridgeObject(action.targetObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && !isMoving && other.CompareTag("Pushable"))
        {
            isPressed = true;
            ExecuteActions(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPressed && other.CompareTag("Pushable"))
        {
            isPressed = false;
            ExecuteActions(false);
        }
    }

    private void ExecuteActions(bool isPressing)
    {
        foreach (ButtonAction action in actions)
        {
            if (action.targetObject == null) continue;

            switch (action.actionType)
            {
                case ButtonAction.ActionType.Move:
                    StartCoroutine(MoveObject(action, isPressing));
                    break;

                case ButtonAction.ActionType.Bridge:
                    if (isPressing)
                    {
                        ShowBridgeObject(action.targetObject);
                    }
                    break;
            }
        }
    }

    private IEnumerator MoveObject(ButtonAction action, bool moveUp)
    {
        isMoving = true;
        Vector3 startPos = action.targetObject.transform.position;
        Vector3 targetPos;
        
        if (moveUp)
        {
            targetPos = action.originalPosition + action.moveDirection.normalized * action.moveDistance;
        }
        else
        {
            if (action.resetOnRelease)
            {
                targetPos = action.originalPosition;
            }
            else
            {
                isMoving = false;
                yield break;
            }
        }

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * action.moveSpeed;
            action.targetObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        isMoving = false;
    }

    private void HideBridgeObject(GameObject bridgeObject)
    {
        // Mesh Renderer'ı kapat
        MeshRenderer renderer = bridgeObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // Collider'ı kapat
        Collider collider = bridgeObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private void ShowBridgeObject(GameObject bridgeObject)
    {
        // Mesh Renderer'ı aç
        MeshRenderer renderer = bridgeObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }

        // Collider'ı aç
        Collider collider = bridgeObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }
} 