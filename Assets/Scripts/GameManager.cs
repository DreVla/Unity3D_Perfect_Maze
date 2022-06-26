using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private bool isGenerationDone, isFirstPersonView, mapView;
    private Camera mainCamera, playerCamera;
    private FirstPersonController playerController;
    private ParticleManager ParticleManager;
    private ParticleSystem fireFliesPS;
    public Button startPlayButton;
    // Start is called before the first frame update
    void Start()
    {
        // initialize the camera view for maze generation
        isFirstPersonView = false;
        mapView = true;
        mainCamera = Camera.main;
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        playerController = GameObject.Find("Player").GetComponent<FirstPersonController>();
        mainCamera.enabled = true;
        playerCamera.enabled = false;
        isGenerationDone = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        fireFliesPS = GameObject.Find("FireFlies").GetComponent<ParticleSystem>();
        playerCamera.GetComponent<AudioListener>().enabled = false;
        ParticleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGenerationDone)
        {
            // when generation is done, set the size of the fireflies particles shape so it covers the whole maze and start playing it.
            // Also enable the audio listener on the player, so sounds can only be heard after generation is done.
            playerCamera.GetComponent<AudioListener>().enabled = true;
            ParticleManager.SetupFireFliesParticles(new Vector3(GameObject.Find("Grid").GetComponent<GridScript>().Size.x, GameObject.Find("Grid").GetComponent<GridScript>().Size.z, 0));
            fireFliesPS.Play();
            if (!isFirstPersonView)
            {
                // Lock cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                mainCamera.enabled = false;
                playerCamera.enabled = true;
                mapView = false;
                isFirstPersonView = true;
            }
            // swap between map view and fps view
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (mapView)
                {
                    mainCamera.enabled = false;
                    playerCamera.enabled = true;
                    mapView = false;
                    playerController.enableMovement();
                }
                else
                {
                    mainCamera.enabled = true;
                    playerCamera.enabled = false;
                    mapView = true;
                    playerController.disableMovement();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

    }

    // called when start playing button is pressed
    public void SetIsGenerationDoneTrue()
    {
        isGenerationDone = true;
        startPlayButton.gameObject.SetActive(false);
    }
}
