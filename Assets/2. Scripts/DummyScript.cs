using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DummyScript : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(turnScene());
    }
    IEnumerator turnScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(1);
    }
}
