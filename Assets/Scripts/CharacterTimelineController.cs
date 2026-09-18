using UnityEngine;
using UnityEngine.Playables;

public class CharacterTimelineController : MonoBehaviour
{
    [Header("Character")]
    public GameObject characterRoot;

    [Header("Timeline")]
    public PlayableDirector timeline;

    [Header("Input")]
    public KeyCode triggerKey = KeyCode.O;

    private Renderer[] renderers;

    private int state = 0;
    private bool isPlaying = false;

    void Start()
    {
        renderers = characterRoot.GetComponentsInChildren<Renderer>(true);

        SetVisible(false);

        if (timeline != null)
        {
            timeline.Stop();
            timeline.time = 0;
            timeline.stopped += OnTimelineStopped;
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(triggerKey) || isPlaying)
            return;

        // O 1: appear
        if (state == 0)
        {
            timeline.time = 0;
            timeline.Evaluate();

            SetVisible(true);

            state = 1;
        }

        // O 2: play choreography
        else if (state == 1)
        {
            isPlaying = true;

            timeline.time = 0;
            timeline.Evaluate();
            timeline.Play();

            state = 2;
        }

        // O 3: disappear
        else if (state == 2)
        {
            SetVisible(false);
            state = 3;
        }
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        isPlaying = false;
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
            r.enabled = visible;
    }

    void OnDestroy()
    {
        if (timeline != null)
            timeline.stopped -= OnTimelineStopped;
    }
}