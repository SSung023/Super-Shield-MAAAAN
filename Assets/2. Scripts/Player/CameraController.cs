using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private Vector3 minCameraPos;
    private Vector3 maxCameraPos;
    private bool focusOut = false;

    private void FixedUpdate()
    {
        Vector3 targetPos = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
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
          // print(focusOut);
    }

    private void Isfocusout(Vector3 minPos, Vector3 maxPos, Vector3 cameraPos)
    {
        if(cameraPos.x < minPos.x )
        {
            focusOut = true;
            StartCoroutine(Focusreset());
            return;
        }
        else if (cameraPos.y < minPos.y)
        {
            focusOut = true;
            StartCoroutine(Focusreset());
            return;
        }
        else if (cameraPos.x > maxPos.x)
        {
            focusOut = true;
            StartCoroutine(Focusreset());
            return;

        }
        else if (cameraPos.y > maxPos.y)
        {
            focusOut = true;
            StartCoroutine(Focusreset());
            return;
        }
        focusOut = false;
        return;
    }
    IEnumerator Focusreset()
    {
        yield return new WaitForSeconds(1.0f);
        focusOut = false;
    }
}
