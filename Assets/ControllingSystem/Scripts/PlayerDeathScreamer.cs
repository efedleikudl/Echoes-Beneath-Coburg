using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathScreener : MonoBehaviour
{
    [Header("Screamer Settings")]
    public VideoPlayer screamerVideoPlayer;
    public VideoClip screamerVideo;
    public GameObject screamerCanvas;          // UI Canvas with RawImage for video
    public RawImage videoDisplay;            // RawImage that shows the RenderTexture

    [Header("Death Settings")]
    public float videoDelay = 0.2f;    // Small delay before screamer starts
    public float deathDuration = 3f;      // Fallback timer (optional)
    public string gameOverSceneName = "GameOver";
    public bool reloadCurrentScene = true;   // If true, restarts the current scene

    [Header("Player References")]
    public GameObject playerCamera;
    public PlayerController playerController;  // Your player controller script

    [Header("Visual Effects")]
    public Image fadeToBlack;  // Optional fade overlay (UI Image)
    public float fadeSpeed = 2f;

    [HideInInspector] public bool isDead = false;

    private float deathTimer = 0f;
    private RenderTexture videoRenderTexture;

    /* ───────────────────────────────────────────────────────────── */
    /* INITIALISATION                                               */
    /* ───────────────────────────────────────────────────────────── */

    void Start()
    {
        // Create RenderTexture for the video
        videoRenderTexture = new RenderTexture(1920, 1080, 24);

        // Ensure we have a VideoPlayer component
        if (screamerVideoPlayer == null)
            screamerVideoPlayer = gameObject.AddComponent<VideoPlayer>();

        // Configure VideoPlayer
        screamerVideoPlayer.clip = screamerVideo;
        screamerVideoPlayer.targetTexture = videoRenderTexture;
        screamerVideoPlayer.isLooping = false;                       // ⇦ no looping
        screamerVideoPlayer.playOnAwake = false;
        screamerVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct; // ⇦ use clip’s own audio
        screamerVideoPlayer.Prepare();                                       // Start buffering

        // Assign RenderTexture to the RawImage
        if (videoDisplay != null)
            videoDisplay.texture = videoRenderTexture;

        // Hide screamer canvas at start
        if (screamerCanvas != null)
            screamerCanvas.SetActive(false);

        // Fade overlay starts transparent & inactive
        if (fadeToBlack != null)
        {
            fadeToBlack.color = new Color(0, 0, 0, 0);
            fadeToBlack.gameObject.SetActive(false);
        }
    }

    /* ───────────────────────────────────────────────────────────── */
    /* TRIGGER HANDLING                                             */
    /* ───────────────────────────────────────────────────────────── */

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spider") && !isDead)
        {
            SpiderAI spider = other.GetComponent<SpiderAI>();
            if (spider != null && spider.animator.GetBool("isAttacking"))
                TriggerDeath();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Spider") && !isDead)
        {
            SpiderAI spider = other.GetComponent<SpiderAI>();
            if (spider != null && spider.animator.GetBool("isAttacking"))
                TriggerDeath();
        }
    }

    /* ───────────────────────────────────────────────────────────── */
    /* DEATH SEQUENCE                                               */
    /* ───────────────────────────────────────────────────────────── */

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        // Disable player controls & movement
        if (playerController != null) playerController.enabled = false;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Unlock & hide cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // Start screamer after short delay
        Invoke(nameof(ShowScreamer), videoDelay);
    }

    void ShowScreamer()
    {
        // Display canvas
        if (screamerCanvas != null) screamerCanvas.SetActive(true);

        // Play video (includes its own audio)
        if (screamerVideoPlayer != null) screamerVideoPlayer.Play();

        // Optional camera shake
        if (playerCamera != null) StartCoroutine(CameraShake());

        // Fade to black over time
        if (fadeToBlack != null)
        {
            fadeToBlack.gameObject.SetActive(true);
            StartCoroutine(FadeOut());
        }
    }

    void Update()
    {
        if (!isDead) return;

        deathTimer += Time.deltaTime;

        // Safety fallback: if video finished earlier, you can hook into
        // screamerVideoPlayer.loopPointReached instead of using a timer.
        if (deathTimer >= deathDuration)
        {
            if (reloadCurrentScene)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            else if (!string.IsNullOrEmpty(gameOverSceneName))
                SceneManager.LoadScene(gameOverSceneName);
        }
    }

    /* ───────────────────────────────────────────────────────────── */
    /* HELPERS                                                      */
    /* ───────────────────────────────────────────────────────────── */

    System.Collections.IEnumerator CameraShake()
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        float shakeDuration = 0.5f;
        float shakeMagnitude = 0.3f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }

    System.Collections.IEnumerator FadeOut()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeToBlack.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    void OnDestroy()
    {
        if (videoRenderTexture != null)
            videoRenderTexture.Release();
    }
}
