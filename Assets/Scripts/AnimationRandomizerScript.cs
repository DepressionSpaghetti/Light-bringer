using UnityEngine;

public class DeityManagerScript : StateMachineBehaviour
{
    //Animation loop variables
    [SerializeField] private int minLoopsToFinish;
    [SerializeField] private int maxLoopsToFinish;
    public int loopsLeft;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        loopsLeft = Random.Range(minLoopsToFinish, maxLoopsToFinish + 1);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(stateInfo.normalizedTime >= 1f && !animator.IsInTransition(layerIndex))
        {
            loopsLeft--;

            if(loopsLeft <= 0)
            {
                animator.SetTrigger("nextIdle");
            }
        }
    }
}
