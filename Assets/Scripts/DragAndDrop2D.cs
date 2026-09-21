using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop2D : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    private InputAction touchPressAction;   
    private InputAction touchPositionAction;

    [SerializeField] private float touchDragPhisicsSpeed = 10f;
    [SerializeField] private float touchDragSpeed = .1f;

    private InputActionMap touchMap;
    private Camera mainCamera;
    private Vector3 velocity = Vector3.zero;

    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    void Awake()
    {
        mainCamera = Camera.main;
        touchMap = playerInput.actions.FindActionMap("Touch");
        touchPressAction = touchMap.FindAction("Click"); 
        touchPositionAction = touchMap.FindAction("PointerPosition");
    }

    void OnEnable()
    {
        touchMap.Enable();
        touchPressAction.performed += TouchPressed;
    }

    void OnDisable()
    {
        touchPressAction.performed -= TouchPressed;
        touchMap.Disable();
    }

    private void TouchPressed(InputAction.CallbackContext context)
    {
        Vector2 position = touchPositionAction.ReadValue<Vector2>();

        if (float.IsInfinity(position.x) || float.IsInfinity(position.y)) return;

        float camDistance = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, camDistance));
        worldPos.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if(hit != null && hit.CompareTag("Draggable"))
        {
            StartCoroutine(DragUpdate(hit.gameObject));
        }
    }

    private IEnumerator DragUpdate(GameObject clickedObject)
    {
        clickedObject.TryGetComponent<Rigidbody2D>(out var rb);
        
        while(touchPressAction.ReadValue<float>() != 0)
        {
            Vector2 position = touchPositionAction.ReadValue<Vector2>();
            
            if (float.IsInfinity(position.x) || float.IsInfinity(position.y))
            {
                yield return null;
                continue;
            }

            float camDistance = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 targetPos = mainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, camDistance));
            targetPos.z = 0f;

            if (rb != null)
            {
                Vector2 direction = targetPos - clickedObject.transform.position;
                rb.linearVelocity = direction * touchDragPhisicsSpeed;
                yield return waitForFixedUpdate;
            }
            else
            {
                clickedObject.transform.position = Vector3.SmoothDamp(clickedObject.transform.position, targetPos, ref velocity, touchDragSpeed);
                yield return null;
            }
        }
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}