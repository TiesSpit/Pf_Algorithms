using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class MouseClickController : MonoBehaviour
{
    public Vector3 clickPosition;
    public UnityEvent<Vector3> click;
    
    void Update() { 
        // Get the mouse click position in world space 
        if (Input.GetMouseButtonDown(0)) { 
            Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition ); 
            if (Physics.Raycast( mouseRay, out RaycastHit hitInfo )) { 
                Vector3 clickWorldPosition = hitInfo.point; 

                clickPosition = clickWorldPosition;

                click?.Invoke(clickPosition);
            }
        }
        DebugExtension.DebugWireSphere(clickPosition);
        Debug.DrawRay(transform.position, clickPosition - transform.position, Color.yellow);
    }
}
