// PlayerController.cs
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Animatin related
    public Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;

    //Physics related
    public float speed = 1f; // Horizontal speed
    public float glideSpeed = -0.2f; // Vertical speed when not boosting
    public float boostSpeed = 5f; // Vertical speed during boost
    public AudioClip boostSound; // Assign the ExampleSFX here
    public float fuel = 100f; // Starting fuel amount
    public float fuelConsumptionRate = 10f; // Fuel consumption rate per second during boost
    public Image fuelBar; // UI image for fuel bar
    public TextMeshProUGUI fuelText; // UI text for fuel display

    //Turbo (Shift): schneller nach vorne, kostet mehr Sprit
    public float turboSpeedMultiplier = 1.75f;
    public float turboFuelMultiplier = 2f; // Verbrauch relativ zu fuelConsumptionRate

    //Sterne-Wertung im Finish-Screen
    [Tooltip("Stern für Zeit: schneller als (Zeit ohne Turbo * Faktor)")]
    public float starTimeFactor = 0.85f;
    [Tooltip("Stern für Sparsamkeit: mindestens so viel Sprit übrig")]
    public float starFuelThreshold = 25f;
    private float parTime; // Zeit bis zum Ziel ohne Turbo

    //Audio n stuff
    private AudioSource audioSource; // To play the sound effect
    public AudioClip crashSound;
    public AudioClip fuelPickupSound;
    private Rigidbody2D rb;
    private bool boosting = false;
    private bool turbo = false;
    private bool finished = false; // Ziel erreicht

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        InvokeRepeating("SwapSprites", 0.1f, 0.1f);

        // If there's no AudioSource component attached, create one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        PlaceCharacterAtPosition(-5f, 0f);

        // Referenzzeit: wie lange man ohne Turbo bis zum Ziel braucht
        GameObject goal = GameObject.FindWithTag("Goal");
        parTime = goal != null ? Mathf.Max(1f, (goal.transform.position.x - transform.position.x) / speed) : 30f;

        ControlsHint.Show("LEERTASTE = Steigen      SHIFT = Turbo");
    }

    void Update()
    {
        if (finished) return; // Finish-Screen übernimmt die Eingaben
        // Check if spacebar is pressed and there is enough fuel
        if (Input.GetKey(KeyCode.Space) && fuel > 0)
        {
            boosting = true;
            if (!audioSource.isPlaying)
            {
                audioSource.clip = boostSound;
                audioSource.pitch = 1.2f; // Increase the pitch
                audioSource.Play();
            }
        }
        else
        {
            boosting = false;
        }

        turbo = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && fuel > 0;

        if (Input.GetKeyDown(KeyCode.Space) && !this.enabled)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Display the remaining fuel
        fuelBar.fillAmount = fuel / 100f;
        fuelText.text = fuel > 0 ? Mathf.Round(fuel).ToString() : "Empty";
    }

    void FixedUpdate()
    {
        // Fly to the right and slightly downwards when doing nothing
        float forward = turbo ? speed * turboSpeedMultiplier : speed;
        rb.linearVelocity = new Vector2(forward, boosting ? boostSpeed : glideSpeed);

        // Consume fuel (Boost und Turbo addieren sich)
        float consumption = 0f;
        if (boosting) consumption += fuelConsumptionRate;
        if (turbo) consumption += fuelConsumptionRate * turboFuelMultiplier;
        fuel = Mathf.Max(0f, fuel - consumption * Time.fixedDeltaTime);

        // Rotate in moving direction, clamping between -45 and 45 degrees
        float desiredRotation = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        desiredRotation = Mathf.Clamp(desiredRotation, -45f, 45f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, desiredRotation), 100 * Time.deltaTime);
    }

    private void PlaceCharacterAtPosition(float x, float y)
    {
        transform.position = new Vector3(x, y, transform.position.z);
    }

    private void SwapSprites()
    {
        currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length;
        spriteRenderer.sprite = sprites[currentSpriteIndex];
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Ground"))
        {
            // Play crash sound
            audioSource.clip = crashSound;
            audioSource.Play();

            // Deactivate controls
            this.enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("FuelItem"))
        { 
            // Play fuel pickup sound
            audioSource.clip = fuelPickupSound;
            audioSource.Play();

            // Increase fuel amount
            fuel = Mathf.Min(100, fuel + 20);

            // Update fuel UI
            fuelBar.fillAmount = fuel / 100f;
            fuelText.text = (fuel > 0) ? fuel.ToString() : "Empty";

            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("Goal") && !finished && this.enabled)
        {
            finished = true;
            if (audioSource.clip == boostSound) audioSource.Stop();
            Time.timeScale = 0; // This freezes the game
            FinishScreen.Show(Time.timeSinceLevelLoad, fuel, parTime * starTimeFactor, starFuelThreshold);
        }
    }

    void LateUpdate()
    {
        // Keep the sprite on screen
        Vector3 position = Camera.main.WorldToViewportPoint(transform.position);
        position.x = Mathf.Clamp(position.x, 0.04f, 0.95f);
        position.y = Mathf.Clamp(position.y, 0.04f, 0.95f);
        transform.position = Camera.main.ViewportToWorldPoint(position);
    }
}
