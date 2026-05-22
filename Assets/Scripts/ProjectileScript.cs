using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private AnimationClip _projectileAnimation;
    PlayableGraph _playableGraph;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playableGraph = PlayableGraph.Create("ProjectileAnimation");
        _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", GetComponent<Animator>());

        var clipPlayable = AnimationClipPlayable.Create(_playableGraph, _projectileAnimation);

        output.SetSourcePlayable(clipPlayable);

        _playableGraph.Play();

        Destroy(gameObject, 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            Destroy(collision.gameObject); //test
        }
    }

    void OnDisable()
    {
        _playableGraph.Destroy();
    }

    private void OnDestroy()
    {
        _playableGraph.Destroy();
    }
}
