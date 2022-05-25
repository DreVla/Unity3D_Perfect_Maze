using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isGenerationDone;
    public Camera mainCamera, playerCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        mainCamera.enabled = true;
        playerCamera.enabled = false;
        isGenerationDone = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGenerationDone)
        {
            mainCamera.enabled = false;
            playerCamera.enabled = true;
        }
    }

    public void SetIsGenerationDone(bool value)
    {
        isGenerationDone = value;
    }
}
