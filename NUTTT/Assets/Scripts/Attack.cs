using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private Animator anim;

    public int numOfClick;
    public float lastClickedTime = 0;
    public float maxComboDelay = 1; 

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f && anim.GetCurrentAnimatorStateInfo(0).IsName("anim1"))
        {
            anim.SetBool("hit1", false);
            numOfClick = 0;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f && anim.GetCurrentAnimatorStateInfo(0).IsName("anim2"))
        {
            anim.SetBool("hit2", false);
            numOfClick = 0;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f && anim.GetCurrentAnimatorStateInfo(0).IsName("anim3"))
        {
            anim.SetBool("hit3", false);
            numOfClick = 0;
        }


        if(lastClickedTime > maxComboDelay)
        {
            numOfClick = 0;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            OnClick();
            lastClickedTime *= Time.deltaTime;
        }
    }

    void OnClick()
    {
        numOfClick++;
        if(numOfClick == 1)
        {
            anim.SetBool("hit1", true); 
        }

        if(numOfClick >= 1 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("anim1"))
        {
            anim.SetBool("hit1", false);
            anim.SetBool("hit2", true);
        }

        if (numOfClick >= 1 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("anim2"))
        {
            anim.SetBool("hit2", false);
            anim.SetBool("hit3", true);
        }


        numOfClick = Mathf.Clamp(numOfClick, 0, numOfClick);
    }
}
