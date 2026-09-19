using UnityEngine;
using UnityEngine.Playables;

public class SunaTimelineController : MonoBehaviour
{
    [Header("Characters")]
    public GameObject suna1Root;
    public GameObject suna2Root;
    public GameObject asaf1Root;
    public GameObject asaf2Root;

    [Header("Timeline Directors")]
    public PlayableDirector timeline01;
    public PlayableDirector timeline02A;
    public PlayableDirector timeline02B;

    [Header("Input")]
    public KeyCode triggerKey = KeyCode.P;

    private Renderer[] suna1Renderers;
    private Renderer[] suna2Renderers;
    private Renderer[] asaf1Renderers;
    private Renderer[] asaf2Renderers;

    /*
        STATE 0
        Start
        -> Everyone invisible
        P -> Suna 1 appears

        STATE 1
        -> Suna 1 visible
        P -> Choreo 1 starts
        -> Asafs appear through Activation Tracks

        STATE 2
        -> Choreo 1 finished
        P -> Suna 2 appears AND Choreo 2 starts

        STATE 3
        -> Choreo 2 running

        STATE 4
        -> Choreo 2 finished
        P -> Everyone disappears

        STATE 5
        -> Finished
    */

    private int state = 0;
    private bool isPlaying = false;

    private int directorsFinished = 0;
    private int directorsExpected = 0;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // Get renderers
        suna1Renderers =
            suna1Root.GetComponentsInChildren<Renderer>(true);

        suna2Renderers =
            suna2Root.GetComponentsInChildren<Renderer>(true);

        asaf1Renderers =
            asaf1Root.GetComponentsInChildren<Renderer>(true);

        asaf2Renderers =
            asaf2Root.GetComponentsInChildren<Renderer>(true);


        // Sunas invisible at game start
        SetSuna1Visible(false);
        SetSuna2Visible(false);


        // Asafs invisible at game start.
        // Their Activation Tracks will activate them later.
        asaf1Root.SetActive(false);
        asaf2Root.SetActive(false);


        // Prepare Timeline Directors
        PrepareDirector(timeline01);
        PrepareDirector(timeline02A);
        PrepareDirector(timeline02B);

        state = 0;
    }


    // =========================================================
    // INPUT
    // =========================================================

    void Update()
    {
        if (!Input.GetKeyDown(triggerKey))
            return;

        // Ignore P while a choreography is running
        if (isPlaying)
            return;


        // =====================================================
        // P1
        // Show ONLY Suna 1
        // =====================================================

        if (state == 0)
        {
            // Put Timeline 01 at frame 0.
            // This also prepares the Asaf Activation Tracks.
            if (timeline01 != null)
            {
                timeline01.time = 0;
                timeline01.Evaluate();
            }

            SetSuna1Visible(true);
            SetSuna2Visible(false);

            state = 1;
            return;
        }


        // =====================================================
        // P2
        // Start Choreography 1
        // =====================================================

        if (state == 1)
        {
            isPlaying = true;

            directorsFinished = 0;
            directorsExpected = 1;

            ResetAndPlay(timeline01);

            return;
        }


        // =====================================================
        // P3
        // Show Suna 2 AND start Choreography 2
        // =====================================================

        if (state == 2)
        {
            // Prepare Choreo 2
            if (timeline02A != null)
            {
                timeline02A.time = 0;
                timeline02A.Evaluate();
            }

            if (timeline02B != null)
            {
                timeline02B.time = 0;
                timeline02B.Evaluate();
            }


            // Both Sunas visible
            SetSuna1Visible(true);
            SetSuna2Visible(true);


            // Start Choreo 2
            isPlaying = true;

            directorsFinished = 0;
            directorsExpected = 2;

            state = 3;

            ResetAndPlay(timeline02A);
            ResetAndPlay(timeline02B);

            return;
        }


        // =====================================================
        // P4
        // Hide EVERYONE
        // =====================================================

        if (state == 4)
        {
            SetSuna1Visible(false);
            SetSuna2Visible(false);

            SetAsaf1Visible(false);
            SetAsaf2Visible(false);

            state = 5;
            return;
        }
    }


    // =========================================================
    // TIMELINE FINISHED
    // =========================================================

    void OnDirectorStopped(PlayableDirector director)
    {
        if (!isPlaying)
            return;

        directorsFinished++;


        // Choreo 2 has two Directors.
        // Wait until BOTH have finished.
        if (directorsFinished < directorsExpected)
            return;


        isPlaying = false;


        // Choreo 1 finished
        if (state == 1)
        {
            // Everyone stays exactly where Timeline left them.
            // Next P starts Choreo 2.

            state = 2;
            return;
        }


        // Choreo 2 finished
        if (state == 3)
        {
            // Everyone stays visible.
            // Next P hides everyone.

            state = 4;
            return;
        }
    }


    // =========================================================
    // DIRECTOR SETUP
    // =========================================================

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


    // =========================================================
    // VISIBILITY
    // =========================================================

    void SetSuna1Visible(bool visible)
    {
        if (suna1Renderers == null)
            return;

        foreach (Renderer r in suna1Renderers)
            r.enabled = visible;
    }


    void SetSuna2Visible(bool visible)
    {
        if (suna2Renderers == null)
            return;

        foreach (Renderer r in suna2Renderers)
            r.enabled = visible;
    }


    void SetAsaf1Visible(bool visible)
    {
        if (asaf1Renderers == null)
            return;

        foreach (Renderer r in asaf1Renderers)
            r.enabled = visible;
    }


    void SetAsaf2Visible(bool visible)
    {
        if (asaf2Renderers == null)
            return;

        foreach (Renderer r in asaf2Renderers)
            r.enabled = visible;
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    void OnDestroy()
    {
        if (timeline01 != null)
            timeline01.stopped -= OnDirectorStopped;

        if (timeline02A != null)
            timeline02A.stopped -= OnDirectorStopped;

        if (timeline02B != null)
            timeline02B.stopped -= OnDirectorStopped;
    }
}