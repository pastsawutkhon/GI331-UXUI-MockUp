using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour
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
        touchPressAction = playerInput.actions["Touchpress"];
        touchPositionAction = playerInput.actions["TouchPosition"];
        touchMap = playerInput.actions.FindActionMap("Touch");
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
        //Debug.Log("Pressed");
        Vector3 position = touchPositionAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider != null && hit.collider.CompareTag("Draggable"))
            {
                StartCoroutine(DragUpdate(hit.collider.gameObject));
            }
        }
    }

    private IEnumerator DragUpdate(GameObject clickedObject)
    {
        //Debug.Log("Drag");
        float initialDistance = Vector3.Distance(clickedObject.transform.position, mainCamera.transform.position);
        clickedObject.TryGetComponent<Rigidbody>(out var rb);
        while(touchPressAction.ReadValue<float>() != 0)
        {
            Vector3 position = touchPositionAction.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(position);
            if (rb != null)
            {
                Vector3 direction = ray.GetPoint(initialDistance) - clickedObject.transform.position;
                rb.linearVelocity = direction * touchDragPhisicsSpeed;
                yield return waitForFixedUpdate;
            }
            else
            {
                clickedObject.transform.position = Vector3.SmoothDamp(clickedObject.transform.position, ray.GetPoint(initialDistance), ref velocity, touchDragSpeed);
                yield return null;
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
