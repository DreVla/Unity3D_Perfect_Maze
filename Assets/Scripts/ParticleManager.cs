using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private float enableTimer = 2f;
    private GameObject thunder;
    private ParticleSystem fireFlies;
    private ParticleSystemShapeType boxShape = ParticleSystemShapeType.Box;
    private AudioSource thunderAudioSource;
    public AudioClip[] audioClips;
    // Start is called before the first frame update
    void Start()
    {
        thunder = GameObject.Find("Thunder1");
        fireFlies = GameObject.Find("FireFlies").GetComponent<ParticleSystem>();
        thunderAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enableTimer <= 0.0f)
        {
            if (thunder.activeSelf) thunder.SetActive(false);
            else
            {
                thunder.SetActive(true);
                thunderAudioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                thunderAudioSource.Play();
            }
            enableTimer = Random.Range(0, 5.0f);

        }
        enableTimer -= Time.deltaTime;
    }

    public void SetupFireFliesParticles(Vector3 Size)
    {
        var shape = fireFlies.shape;
        shape.shapeType = boxShape;
        shape.scale = Size;
        shape.position = new Vector3(Size.x / 2, -Size.y / 2, 0);
    }
}

