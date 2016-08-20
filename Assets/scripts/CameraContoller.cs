// File: CameraContoller.cs
// Date: 2016-8-18
//
// COPYRIGHT (c) 2016 Callum Wilson callum.w@outlook.com
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System.Collections;

public class CameraContoller : MonoBehaviour
{


    public float moveSpeed = 60f;
    public float accelerateFactor = 3f;
    public float rotateSpeed = 90f;

    private float rotationX;    //Camera rotation
    private float rotationY;

    private bool haveFocus = false;
    private float lastFocusChangeTime;

    // Use this for initialization
    void Start()
    {
        rotationX = transform.localRotation.x;
        rotationY = transform.localRotation.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        haveFocus = true;
        lastFocusChangeTime = Time.fixedTime;
    }

    // Update is called once per frame
    void LateUpdate()
    {
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
            if (Time.fixedTime - lastFocusChangeTime > 0.5f)
            {
                lastFocusChangeTime = Time.fixedTime;
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
            if (Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift))
            {
                /** Up and Down **/
                if (Input.GetKey(KeyCode.Q))
                {
                    transform.position += transform.up * moveSpeed *
                        accelerateFactor * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    transform.position -= transform.up * moveSpeed *
                        accelerateFactor * Time.deltaTime;
                }

                /** Forwards and Backwards **/
                if (Input.GetKey(KeyCode.W))
                {
                    transform.position += transform.forward * moveSpeed *
                        accelerateFactor * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    transform.position -= transform.forward * moveSpeed *
                        accelerateFactor * Time.deltaTime;
                }

                /** Left and Right **/
                if (Input.GetKey(KeyCode.A))
                {
                    transform.position -= transform.right * moveSpeed *
                        accelerateFactor * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    transform.position += transform.right * moveSpeed *
                        accelerateFactor * Time.deltaTime;
                }
            }
            else
            {
                /** Up and Down **/
                if (Input.GetKey(KeyCode.Q))
                {
                    transform.position += transform.up * moveSpeed *
                        Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    transform.position -= transform.up * moveSpeed *
                        Time.deltaTime;
                }

                /** Forwards and Backwards **/
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

                /** Left and Right **/
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
}
