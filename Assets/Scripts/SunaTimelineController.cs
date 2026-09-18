using UnityEngine;
using UnityEngine.Playables;

public class SunaTimelineCycleController : MonoBehaviour
{
    [Header("Characters")]
    public GameObject suna1Root;
    public GameObject suna2Root;

    [Header("Timeline Directors")]
    public PlayableDirector timeline01Suna1;
    public PlayableDirector timeline02Suna1;
    public PlayableDirector timeline02Suna2;

    [Header("Input")]
    public KeyCode triggerKey = KeyCode.P;

    private Renderer[] suna1Renderers;
    private Renderer[] suna2Renderers;

    private int sequenceIndex = 0;
    private bool charactersVisible = false;
    private bool isPlaying = false;
    private bool performanceFinished = false;

    private int directorsFinished = 0;
    private int directorsExpected = 0;

    void Start()
    {
        // Get all renderers of both Sunas
        suna1Renderers = suna1Root.GetComponentsInChildren<Renderer>(true);
        suna2Renderers = suna2Root.GetComponentsInChildren<Renderer>(true);

        // Both Sunas invisible at game start
        SetSuna1Visible(false);
        SetSuna2Visible(false);

        // Prepare all Timeline Directors
        PrepareDirector(timeline01Suna1);
        PrepareDirector(timeline02Suna1);
        PrepareDirector(timeline02Suna2);

        // IMPORTANT:
        // Evaluate Timeline 01 at frame 0.
        // This makes the Asaf Activation Track take effect immediately,
        // so Asaf is invisible at game start.
        if (timeline01Suna1 != null)
        {
            timeline01Suna1.time = 0;
            timeline01Suna1.Evaluate();
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(triggerKey) || isPlaying)
            return;

        // Final P:
        // Hide both Sunas
        if (performanceFinished)
        {
            SetSuna1Visible(false);
            SetSuna2Visible(false);

            performanceFinished = false;
            charactersVisible = false;

            return;
        }

        // First P of a sequence:
        // Prepare positions and make characters visible
        if (!charactersVisible)
        {
            ShowCurrentSequenceCharacters();
        }
        // Second P:
        // Start choreography
        else
        {
            PlayCurrentSequence();
        }
    }

    void ShowCurrentSequenceCharacters()
    {
        if (sequenceIndex == 0)
        {
            // Prepare Timeline 01 at frame 0
            timeline01Suna1.time = 0;
            timeline01Suna1.Evaluate();

            // Only Suna 1 visible
            SetSuna1Visible(true);
            SetSuna2Visible(false);
        }
        else if (sequenceIndex == 1)
        {
            // Prepare BOTH Timeline 02 directors at frame 0
            timeline02Suna1.time = 0;
            timeline02Suna1.Evaluate();

            timeline02Suna2.time = 0;
            timeline02Suna2.Evaluate();

            // Both Sunas visible
            SetSuna1Visible(true);
            SetSuna2Visible(true);
        }

        charactersVisible = true;
    }

    void PlayCurrentSequence()
    {
        isPlaying = true;
        directorsFinished = 0;

        if (sequenceIndex == 0)
        {
            // First choreography:
            // only one Timeline Director
            directorsExpected = 1;

            ResetAndPlay(timeline01Suna1);
        }
        else if (sequenceIndex == 1)
        {
            // Second choreography:
            // two Sunas play the same choreography simultaneously
            directorsExpected = 2;

            ResetAndPlay(timeline02Suna1);
            ResetAndPlay(timeline02Suna2);
        }
    }

    void OnDirectorStopped(PlayableDirector director)
    {
        if (!isPlaying)
            return;

        directorsFinished++;

        // For choreography 02:
        // wait until BOTH directors have finished
        if (directorsFinished < directorsExpected)
            return;

        isPlaying = false;

        if (sequenceIndex == 0)
        {
            // Choreography 01 finished.
            // Suna 1 stays visible.
            // Next P prepares choreography 02.
            sequenceIndex = 1;
            charactersVisible = false;
        }
        else if (sequenceIndex == 1)
        {
            // Choreography 02 finished.
            // Both Sunas stay visible.
            // Next P hides them.
            performanceFinished = true;
            charactersVisible = true;
        }
    }

    void PrepareDirector(PlayableDirector director)
    {
        if (director == null)
            return;

        director.Stop();
        director.time = 0;

        director.stopped += OnDirectorStopped;
    }

    void ResetAndPlay(PlayableDirector director)
    {
        if (director == null)
            return;

        director.time = 0;
        director.Evaluate();
        director.Play();
    }

    void SetSuna1Visible(bool visible)
    {
        foreach (Renderer r in suna1Renderers)
        {
            r.enabled = visible;
        }
    }

    void SetSuna2Visible(bool visible)
    {
        foreach (Renderer r in suna2Renderers)
        {
            r.enabled = visible;
        }
    }

    void OnDestroy()
    {
        if (timeline01Suna1 != null)
            timeline01Suna1.stopped -= OnDirectorStopped;

        if (timeline02Suna1 != null)
            timeline02Suna1.stopped -= OnDirectorStopped;

        if (timeline02Suna2 != null)
            timeline02Suna2.stopped -= OnDirectorStopped;
    }
}