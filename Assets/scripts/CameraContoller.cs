using UnityEngine;
using System.Collections;

public class CameraContoller : MonoBehaviour {


    public float moveSpeed = 60f;
    public float rotateSpeed = 90f;

    private float rotationX;
    private float rotationY;
    private bool haveFocus = false;
    private float lastFocusChange;
	// Use this for initialization
	void Start () {
        rotationX = transform.localRotation.x;
        rotationY = transform.localRotation.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        haveFocus = true;
        lastFocusChange = Time.fixedTime;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (haveFocus)
        {
            rotationX += Input.GetAxis("Mouse X") * Time.deltaTime * 
                rotateSpeed;
            rotationY += Input.GetAxis("Mouse Y") * Time.deltaTime * 
                rotateSpeed;

            transform.localRotation = Quaternion.AngleAxis(rotationX, 
                Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY,
                Vector3.left);
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            if (Time.fixedTime - lastFocusChange > 0.5f)
            {
                lastFocusChange = Time.fixedTime;
                if (haveFocus)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                haveFocus = !haveFocus;
            }
        }

        if (haveFocus)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                transform.position += transform.up * moveSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                transform.position -= transform.up * moveSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * moveSpeed * 
                    Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * moveSpeed * 
                    Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * moveSpeed * 
                    Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * moveSpeed * 
                    Time.deltaTime;
            }
        }
	}


}
