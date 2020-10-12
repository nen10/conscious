using ca.HenrySoftware.Rage;
using MapSystem.HexCoordinateSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using MapSystem.SpriteManager;
using CharacterSystem.StatusParameter;
using System.Xml.Schema;

public class AnimationController : MonoBehaviour
{
	static int AnimatorWalk = Animator.StringToHash("Walk");
	static int AnimatorAttack = Animator.StringToHash("Attack");
    float moveSpeed = 2f;
    float paramSpeed = 4f;
    public Animator[] Controllers;
    bool frontIsRight = false;
    public Vector3 target;
    public List<HexUnit> targetList;
    public MapData map;
    public GameObject trapPrepared;
	public StatusCharacter status;
    public HexSprite pos;
    public bool valMax;
    public ParamVariable p;
    public float startP;
    public float endP;
    public float diffP;

    void Awake()
	{
	}
	void Start()
	{
        target = transform.position;
        Begin();
    }
	void Update () {
		if(!(targetList is null) && transform.position == target)
		{
            RecurciveMove();
        }
		if(!(p is null))
		{
			Move();
		}
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

	public void RecurciveMove()
	{
        if(targetList.Count == 0)
		{
            StopWalk();
            Begin();
            this.p = null;
            this.targetList = null;
            if(!(trapPrepared is null))
			{
                trapPrepared.SetActive(true);
                // TODO: trap process
                // ex. status.MOV.c -= 1;
                trapPrepared = null;
            }
            return;
        };

        pos += targetList[0];
		startP = endP;
		if(valMax) endP = startP - map.GetMoveCost(pos);
		else endP = startP - diffP;

		Vector3 v = map.HexUnit2World(targetList[0]);
        target += v;
		if(v.x>0) turnRight();
		if(v.x<0) turnLeft();

		targetList.RemoveAt(0);
	}
	void Move(){
		transform.position = Vector3.MoveTowards (transform.position, target, moveSpeed * Time.deltaTime);
        ParamaterControll();
    }
	public void ParamaterControllInit(ParamVariable param, bool valMax)
	{
        this.p = param;
        this.valMax = valMax;
		startP = valMax ? p.m : p.c;
		endP = startP;
    }
	private void ParamaterControll()
	{
		if(valMax) p.m = Mathf.MoveTowards(startP, endP, moveSpeed * Time.deltaTime);
		else p.c = Mathf.MoveTowards(startP, endP, moveSpeed * Time.deltaTime);
    }
}