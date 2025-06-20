using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathScreener : MonoBehaviour
{
    [Header("Screamer Settings")]
    public VideoPlayer screamerVideoPlayer;
    public VideoClip screamerVideo;
    public GameObject screamerCanvas; // UI Canvas with RawImage for video
    public RawImage videoDisplay; // RawImage component to show video
    public AudioSource screamerAudio;
    public AudioClip screamerSound;

    [Header("Death Settings")]
    public float videoDelay = 0.2f; // Small delay before screamer
    public float deathDuration = 3f; // How long screamer plays
    public string gameOverSceneName = "GameOver"; // Scene to load after death
    public bool reloadCurrentScene = true; // If true, reloads current scene instead

    [Header("Player References")]
    public GameObject playerCamera;
    public PlayerController playerController; // Your player controller script
    public AudioListener playerAudioListener;

    [Header("Visual Effects")]
    public Image fadeToBlack; // Optional fade effect
    public float fadeSpeed = 2f;

    [HideInInspector]
    public bool isDead = false;
    private float deathTimer = 0f;
    private RenderTexture videoRenderTexture;

    void Start()
    {
        // Create render texture for video
        videoRenderTexture = new RenderTexture(1920, 1080, 24);

        // Setup video player
        if (screamerVideoPlayer == null)
        {
            screamerVideoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        if (screamerCanvas != null)
        {
            screamerCanvas.SetActive(false);
        }

        // Configure video player
        if (screamerVideoPlayer != null && screamerVideo != null)
        {
            screamerVideoPlayer.clip = screamerVideo;
            screamerVideoPlayer.targetTexture = videoRenderTexture;
            screamerVideoPlayer.isLooping = true;
            screamerVideoPlayer.playOnAwake = false;
            screamerVideoPlayer.Prepare();

            // Set render texture to RawImage
            if (videoDisplay != null)
            {
                videoDisplay.texture = videoRenderTexture;
            }
        }

        // Setup fade overlay
        if (fadeToBlack != null)
        {
            fadeToBlack.color = new Color(0, 0, 0, 0);
            fadeToBlack.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if spider touches player
        if (other.CompareTag("Spider") && !isDead)
        {
            SpiderAI spider = other.GetComponent<SpiderAI>();
            if (spider != null && spider.animator.GetBool("isAttacking"))
            {
                TriggerDeath();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Also check during stay for continuous attack
        if (other.CompareTag("Spider") && !isDead)
        {
            SpiderAI spider = other.GetComponent<SpiderAI>();
            if (spider != null && spider.animator.GetBool("isAttacking"))
            {
                TriggerDeath();
            }
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;

        // Disable player controls immediately
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Freeze player movement
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // Start death sequence
        Invoke("ShowScreamer", videoDelay);
    }

    void ShowScreamer()
    {
        // Show screamer canvas
        if (screamerCanvas != null)
        {
            screamerCanvas.SetActive(true);
        }

        // Play screamer video
        if (screamerVideoPlayer != null)
        {
            screamerVideoPlayer.Play();
        }

        // Play screamer audio
        if (screamerAudio != null && screamerSound != null)
        {
            screamerAudio.PlayOneShot(screamerSound);
        }
        else if (screamerAudio != null)
        {
            screamerAudio.Play();
        }

        // Disable game audio
        if (playerAudioListener != null)
        {
            playerAudioListener.enabled = false;
        }

        // Optional: Add camera shake
        if (playerCamera != null)
        {
            StartCoroutine(CameraShake());
        }

        // Start fade to black
        if (fadeToBlack != null)
        {
            fadeToBlack.gameObject.SetActive(true);
            StartCoroutine(FadeOut());
        }
    }

    void Update()
    {
        if (isDead)
        {
            deathTimer += Time.deltaTime;

            if (deathTimer >= deathDuration)
            {
                // Load game over or restart
                if (reloadCurrentScene)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                else if (!string.IsNullOrEmpty(gameOverSceneName))
                {
                    SceneManager.LoadScene(gameOverSceneName);
                }
            }
        }
    }

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
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeToBlack.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    void OnDestroy()
    {
        // Clean up render texture
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
        }
    }
}