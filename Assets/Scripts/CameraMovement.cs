using UnityEngine;

public class FlyThroughCamera : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 2f;

    void Start(){
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        // Camera movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        float upDown = Input.GetKey(KeyCode.Q) ? 1f : (Input.GetKey(KeyCode.E) ? -1f : 0f);

        Vector3 moveDirection = new Vector3(horizontal, upDown, vertical).normalized;
        transform.Translate(moveDirection * speed * Time.deltaTime);

        // Camera rotation

        if (Input.GetMouseButton(1)){
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            transform.Rotate(Vector3.up * mouseX * sensitivity);
            transform.Rotate(Vector3.left * mouseY * sensitivity);
        }
    }
}
