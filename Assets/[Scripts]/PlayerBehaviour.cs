using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehaviour : MonoBehaviour
{

    public Text scoreText;
    public Text scoreTextWin;
    public Text scoreTextLose;
    public GameObject heartOne;
    public GameObject heartTwo;
    public GameObject heartThree;
    public GameObject winScreen;
    public GameObject loseScreen;

    [Header("Touch Input")] 
    public Joystick joystick;
    [Range(0.01f, 1.0f)]
    public float sensitivity;
    
    [Header("Movement")] 
    public float horizontalForce;
    public float verticalForce;
    public bool isGrounded;
    public Transform groundOrigin;
    public float groundRadius;
    public LayerMask groundLayerMask;
    [Range(0.1f, 0.9f)]
    public float airControlFactor;

    [Header("Animation")] 
    public PlayerAnimationState state;

    [Header("Sound FX")] 
    public List<AudioSource> audioSources;
    public AudioSource jumpSound;
    public AudioSource hitSound;

    [Header("Dust Trail")] 
    public ParticleSystem dustTrail;
    public Color dustTrailColour;

    [Header("Screen Shake Properties")] 
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineBasicMultiChannelPerlin perlin;
    public float shakeIntensity;
    public float shakeDuration;
    public float shakeTimer;
    public bool isCameraShaking;
    public int score = 0;

    private int Lives = 3;
    private GameController gameController;
    private Rigidbody2D rigidbody;
    private Animator animatorController;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        isCameraShaking = false;
        shakeTimer = shakeDuration;

        rigidbody = GetComponent<Rigidbody2D>();
        animatorController = GetComponent<Animator>();
        gameController = GameObject.FindObjectOfType<GameController>();

        // Assign Sounds
        audioSources = GetComponents<AudioSource>().ToList();
        jumpSound = audioSources[0];
        hitSound = audioSources[1];

        scoreText.text = score + "";
        dustTrail = GetComponentInChildren<ParticleSystem>();

        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Lives == 3)
        {
            heartOne.SetActive(true);
            heartTwo.SetActive(true);
            heartThree.SetActive(true);
        }
        else if (Lives == 2)
        {
            heartOne.SetActive(true);
            heartTwo.SetActive(true);
            heartThree.SetActive(false);
        }
        else if (Lives == 1)
        {
            heartOne.SetActive(true);
            heartTwo.SetActive(false);
            heartThree.SetActive(false);
        }
        else if (Lives <= 1)
        {
            heartOne.SetActive(false);
            heartTwo.SetActive(false);
            heartThree.SetActive(false);
            GameOver();
        }
        Move();
        CheckIfGrounded();
        //OnDrawGizmos();
        // Camera Shake Control
        if (isCameraShaking)
        {
            //shakeTimer -= Time.deltaTime;
            //if (shakeTimer <= 0.0f) // timed out
            //{
            //    perlin.m_AmplitudeGain = 0.0f;
            //    shakeTimer = shakeDuration;
            //    isCameraShaking = false;
            //}
        }
    }

    private void Move()
    {
        float x = (Input.GetAxisRaw("Horizontal") + joystick.Horizontal) * sensitivity ;

        if (isGrounded)
        {
            // Keyboard Input
            float y = (Input.GetAxisRaw("Vertical") + joystick.Vertical) * sensitivity;
            float jump = Input.GetAxisRaw("Jump") + ((UIController.jumpButtonDown) ? 1.0f : 0.0f);

            // jump activated
            if (jump > 0)
            {
                jumpSound.Play();
            }

            // Check for Flip

            if (x != 0)
            {
                x = FlipAnimation(x);
                animatorController.SetInteger("AnimationState", (int) PlayerAnimationState.RUN); // RUN State
                state = PlayerAnimationState.RUN;
                CreateDustTrail();
            }
            else
            {
                animatorController.SetInteger("AnimationState", (int)PlayerAnimationState.IDLE); // IDLE State
                state = PlayerAnimationState.IDLE;
            }

            float horizontalMoveForce = x * horizontalForce;
            float jumpMoveForce = jump * verticalForce; 

            float mass = rigidbody.mass * rigidbody.gravityScale;


            rigidbody.AddForce(new Vector2(horizontalMoveForce, jumpMoveForce) * mass);
            rigidbody.velocity *= 0.99f; // scaling / stopping hack
        }
        else // Air Control
        {
            animatorController.SetInteger("AnimationState", (int)PlayerAnimationState.JUMP); // JUMP State
            state = PlayerAnimationState.JUMP;

            if (x != 0)
            {
                x = FlipAnimation(x);

                float horizontalMoveForce = x * horizontalForce * airControlFactor;
                float mass = rigidbody.mass * rigidbody.gravityScale;

                rigidbody.AddForce(new Vector2(horizontalMoveForce, 0.0f) * mass);
            }
            CreateDustTrail();
        }

    }

    private void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.CircleCast(groundOrigin.position, groundRadius, Vector2.down, groundRadius, groundLayerMask);

        isGrounded = (hit) ? true : false;
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        loseScreen.SetActive(true);
        scoreTextLose.text = ("Your Score: " + score);
    }
    private float FlipAnimation(float x)
    {
        // depending on direction scale across the x-axis either 1 or -1
        x = (x > 0) ? 1 : -1;

        transform.localScale = new Vector3(x * 0.5f, 0.50f);
        return x;
    }

    private void CreateDustTrail()
    {
        dustTrail.GetComponent<Renderer>().material.SetColor("_Color", dustTrailColour);
        dustTrail.Play();
    }

    private void ShakeCamera()
    {
        perlin.m_AmplitudeGain = shakeIntensity;
        isCameraShaking = true;
    }

    // EVENTS

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            transform.SetParent(other.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            transform.SetParent(null);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            hitSound.Play();
            Lives--;
            transform.position = gameController.currentSpawnPoint.position;
        }
        if (other.gameObject.CompareTag("Water"))
        {
            hitSound.Play();
            Lives--;
            transform.position = gameController.currentSpawnPoint.position;
        }
        if (other.gameObject.CompareTag("Finish"))
        {
            hitSound.Play();
        }
        if (other.gameObject.CompareTag("Bone"))
        {
            score += 50;
            scoreText.text = score + "";
            hitSound.Play();
        }
    }

   

    // UTILITIES

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundOrigin.position, groundRadius);
    }

}
