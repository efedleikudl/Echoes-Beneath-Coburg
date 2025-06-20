using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class PlayVideoAndSwitchScene : MonoBehaviour
{
    [Tooltip("Scene name *or* build index to load when the video ends")]
    public string nextScene = "StartMenu";

    private VideoPlayer vp;

    // --------------------------------------------------
    void Awake()
    {
        vp = GetComponent<VideoPlayer>();

        // Make sure we control playback ourselves
        vp.playOnAwake = false;
        vp.isLooping = false;
    }

    void Start()
    {
        // Fire once when the last frame is reached
        vp.loopPointReached += HandleVideoFinished;

        // Optional: call vp.Prepare() and wait for prepared event if
        // you need a custom loading screen. For most clips, just play:
        vp.Play();
    }

    // --------------------------------------------------
    private void HandleVideoFinished(VideoPlayer source)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // Accept either a build-index string ("2") or a scene name
        if (int.TryParse(nextScene, out int buildIndex))
            SceneManager.LoadScene(buildIndex);
        else
            SceneManager.LoadScene(nextScene);
    }

    void OnDestroy()   // tidy up in case the object is destroyed early
    {
        if (vp != null)
            vp.loopPointReached -= HandleVideoFinished;
    }
}
