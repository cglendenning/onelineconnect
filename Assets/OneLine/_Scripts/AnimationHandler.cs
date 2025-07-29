using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{

    private bool isVisble = false;

    private SpriteRenderer spr;
    private Dictionary<int, Vector3> posforAnimation;
    private Dictionary<int, TextM> posWithText;

    private float scalingSpeed = 1.5f;
    private float targetScale = 1.7f;
    private int targetAnimationToShow = 0;
    private bool callOnced = false;

    private float scale;
    private const float startScale = 0.6f;

    // Use this for initialization
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        posforAnimation = new Dictionary<int, Vector3>();
        posWithText = new Dictionary<int, TextM>();
        makeVisblityChange(0f);
    }

    void FixedUpdate()
    {
        if (isVisble)
        {
            scaleChange();
        }
    }

    public void resetAnimation()
    {
        posforAnimation.Clear();
    }

    public void addAnimationToRun(Vector3 pos, int gridPoint, TextM textM)
    {
        if (!posforAnimation.ContainsKey(gridPoint))
        {
            posforAnimation.Add(gridPoint, pos);
            posWithText.Add(gridPoint, textM);
        }
    }

    private void runAnimationAtPosition()
    {
        if (posforAnimation.Count > 0)
        {
            isVisble = false;
            targetAnimationToShow = 0;

            scale = startScale;
            Vector3 pos = findAndDeleteLowestPosition();
            transform.position = pos;

            transform.localScale = new Vector3(scale, scale);
            isVisble = true;
            callOnced = false;
        }
        else
        {
            makeVisblityChange(0f);
            isVisble = false;
        }
    }

    public Vector3 findAndDeleteLowestPosition()
    {
        int lowest = 20000;
        foreach (int key in posforAnimation.Keys)
        {
            if (key < lowest)
            {

                lowest = key;
            }
        }

        Vector3 pos = posforAnimation[lowest];
        posforAnimation.Remove(lowest);

        posWithText[lowest].addString("" + lowest);
        posWithText.Remove(lowest);

        pos.z = 2;

        return pos;
    }

    public void runAnimations()
    {
        isVisble = true;
        makeVisblityChange(1f);

        runAnimationAtPosition();
    }

    void makeVisblityChange(float alpha)
    {
        Color cp = Color.blue;
        cp.a = alpha;
        spr.color = cp;
    }

    void scaleChange()
    {
        if (callOnced)
            return;

        scale += Time.deltaTime * scalingSpeed; // Mathf.Repeat (scaleLen, 5f);

        float alpha = targetScale - scale;
        changeAlpha(alpha);
        if (scale >= targetScale)
        {
            targetAnimationToShow++;
        }

        transform.localScale = new Vector3(scale, scale);

        if (!callOnced && targetAnimationToShow == 1)
        {
            scale = startScale;
            callOnced = true;
            targetAnimationToShow++;

            runAnimationAtPosition();
        }
    }

    void changeAlpha(float alpha)
    {
        alpha = alpha / targetScale;
        Color c = spr.color;
        c.a = alpha;
        spr.color = c;
    }
}
