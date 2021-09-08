using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject target;
    
    [SerializeField] private Vector2 cameraDistrict_center;
    [SerializeField] private Vector2 cameraDistrict_size;
    private float camera_width;
    private float camera_height;
    
    private Vector3 minCameraPos;
    private Vector3 maxCameraPos;
    private bool focusOut = false;


    private void Start()
    {
        camera_height = Camera.main.orthographicSize;
        camera_width = camera_height * Screen.width / Screen.height;
    }
    private void FixedUpdate()
    {
        Vector3 targetPos = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
        //Vector3 targetPos = District_Camera();
        minCameraPos = new Vector3(targetPos.x - 6, targetPos.y - 6, 0);
        maxCameraPos = new Vector3(targetPos.x + 6, targetPos.y + 6, 0);

        if (!focusOut)
        {
            Isfocusout(minCameraPos, maxCameraPos, transform.position);
        }
        
        if(focusOut)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 3f);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 0.8f);
        }
    }

    private Vector3 District_Camera()
    {
        float lx = cameraDistrict_size.x * 0.5f / camera_width;
        float clampX = Mathf.Clamp(target.transform.position.x, -lx + cameraDistrict_center.x, lx + cameraDistrict_center.x);
        
        float ly = cameraDistrict_size.y * 0.5f / camera_height;
        float clampY = Mathf.Clamp(target.transform.position.y, -ly + cameraDistrict_center.y, ly + cameraDistrict_center.y);

        //transform.position = new Vector3(clampX, clampY, 0);
        Vector3 targetPos = new Vector3(clampX, clampY, transform.position.z);
        
        return targetPos;
    }
    
    private void Isfocusout(Vector3 minPos, Vector3 maxPos, Vector3 cameraPos)
    {
        if(cameraPos.x < minPos.x )
        {
            focusOut = true;
            StartCoroutine(ResetFocus());
            return;
        }
        else if (cameraPos.y < minPos.y)
        {
            focusOut = true;
            StartCoroutine(ResetFocus());
            return;
        }
        else if (cameraPos.x > maxPos.x)
        {
            focusOut = true;
            StartCoroutine(ResetFocus());
            return;

        }
        else if (cameraPos.y > maxPos.y)
        {
            focusOut = true;
            StartCoroutine(ResetFocus());
            return;
        }
        focusOut = false;
        return;
    }
    IEnumerator ResetFocus()
    {
        yield return new WaitForSeconds(1.0f);
        focusOut = false;
    }
}
