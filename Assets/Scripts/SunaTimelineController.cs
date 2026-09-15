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

    private int directorsFinished = 0;
    private int directorsExpected = 0;

    void Start()
    {
        suna1Renderers = suna1Root.GetComponentsInChildren<Renderer>(true);
        suna2Renderers = suna2Root.GetComponentsInChildren<Renderer>(true);

        SetSuna1Visible(false);
        SetSuna2Visible(false);

        PrepareDirector(timeline01Suna1);
        PrepareDirector(timeline02Suna1);
        PrepareDirector(timeline02Suna2);
    }

    void Update()
    {
        if (!Input.GetKeyDown(triggerKey) || isPlaying)
            return;

        if (!charactersVisible)
        {
            ShowCurrentSequenceCharacters();
        }
        else
        {
            PlayCurrentSequence();
        }
    }

    void ShowCurrentSequenceCharacters()
    {
        if (sequenceIndex == 0)
        {
            // Set Timeline 01 to its first frame
            timeline01Suna1.time = 0;
            timeline01Suna1.Evaluate();

            SetSuna1Visible(true);
            SetSuna2Visible(false);
        }
        else if (sequenceIndex == 1)
        {
            // Set BOTH Timeline 02 directors to their first frame
            // BEFORE making the characters visible
            timeline02Suna1.time = 0;
            timeline02Suna1.Evaluate();

            timeline02Suna2.time = 0;
            timeline02Suna2.Evaluate();

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
            directorsExpected = 1;

            ResetAndPlay(timeline01Suna1);
        }
        else if (sequenceIndex == 1)
        {
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

        // Wait until all directors in this sequence are finished
        if (directorsFinished < directorsExpected)
            return;

        SetSuna1Visible(false);
        SetSuna2Visible(false);

        charactersVisible = false;
        isPlaying = false;

        sequenceIndex++;

        // Loop back to sequence 1
        if (sequenceIndex > 1)
            sequenceIndex = 0;
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
        director.time = 0;
        director.Evaluate();
        director.Play();
    }

    void SetSuna1Visible(bool visible)
    {
        foreach (Renderer r in suna1Renderers)
            r.enabled = visible;
    }

    void SetSuna2Visible(bool visible)
    {
        foreach (Renderer r in suna2Renderers)
            r.enabled = visible;
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