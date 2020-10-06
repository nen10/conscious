using ca.HenrySoftware.Rage;
using System.Collections;
using UnityEngine;
public class AnimationController : MonoBehaviour
{
	static int AnimatorWalk = Animator.StringToHash("Walk");
	static int AnimatorAttack = Animator.StringToHash("Attack");
	public Animator[] Controllers;
    bool frontIsRight = false;
    void Awake()
	{
	}
	void Start()
	{
        Begin();
    }
    public void Begin()
	{
        ContinueIdlingWalk();
    }
	public void Inverse()
	{
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].transform.localScale = Vector3.Scale(Controllers[i].transform.localScale, new Vector3(-1, 1, 1));
        
        frontIsRight = !frontIsRight;
    }
	public void turnRight()
	{
        if (!frontIsRight) { Inverse(); }
    }
	public void turnLeft()
	{
        if (frontIsRight) { Inverse(); }
    }
	public void BeginWalk()
	{
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].SetBool(AnimatorWalk, true);
	}
	public void StopWalk()
	{
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].SetBool(AnimatorWalk, false);
	}
	public void Attack()
	{
		StopAllCoroutines();
		StartCoroutine(AnimateAttack());
	}
	IEnumerator AnimateAttack()
	{
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].SetTrigger(AnimatorAttack);
		yield return new WaitForSeconds(2f);
	}
	public void ContinueAttack()
	{
		StopAllCoroutines();
		StartCoroutine(AnimateContinueAttack());
	}
	IEnumerator AnimateContinueAttack()
	{
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].SetTrigger(AnimatorAttack);
		yield return new WaitForSeconds(1f);
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].SetTrigger(AnimatorAttack);
		yield return new WaitForSeconds(1f);

		Ease.Go(this, 0f, 1f, 1f, (p) => {}, ContinueAttack, EaseType.Linear);
	}

	public void ContinueIdlingWalk()
	{
		StopAllCoroutines();
		StartCoroutine(AnimateContinueIdlingWalk());
	}
	IEnumerator AnimateContinueIdlingWalk()
	{
		yield return new WaitForSeconds(5f);
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].SetBool(AnimatorWalk, true);
		yield return new WaitForSeconds(1f);
        
		for (var i = 0; i < Controllers.Length; i++)
			Controllers[i].SetBool(AnimatorWalk, false);

		Ease.Go(this, 0f, 1f, 1f, (p) => {}, ContinueIdlingWalk, EaseType.Linear);
	}
}