using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

/*
Even though the legacy Animation component is still supported...
I decided to switch to using the newer Animator component as it just felt appropriate :D (and also it's much simpler to work with)
*/

public class AIAnimation : MonoBehaviour
{
    public Animator animator;

    public string stateMovingName;
    public string stateSpottedName;
    public string walkSpeedMultiplierName;
    public string runSpeedMultiplierName;
    public float walkSpeedMultiplier;
    public float runSpeedMultiplier;

    public bool randomizeSpeeds;
    public float randomizeRange;

    public void UpdateAnimator(bool moving, bool spotted)
    {
        float newRunMultiplier = runSpeedMultiplier;
        float newWalkMultiplier = walkSpeedMultiplier;

        if (randomizeSpeeds)
        {
            float randomAmount = Random.Range(-randomizeRange, randomizeRange);

            newRunMultiplier += randomAmount;
            newWalkMultiplier += randomAmount;
        }

        animator.SetBool(stateMovingName, moving);
        animator.SetBool(stateSpottedName, spotted);

        animator.SetFloat(walkSpeedMultiplierName, newWalkMultiplier);
        animator.SetFloat(runSpeedMultiplierName, newRunMultiplier);
    }
}
