using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotAnimation : MonoBehaviour
{
    public float fadingSpeed;
    public float scalingSpeed = 1.5f;

    private bool isEnabled = false;
    private bool prvState = false;
    private SpriteRenderer sp;

    private const float startScale = 0.6f;
    private float scale;
    private float targetScale = 2f;

    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Color c = sp.color;
        c.a = 0f;
        sp.color = c;
    }

    public void setEnableAtPosition(bool enable, Vector3 pos)
    {
        prvState = isEnabled;

        transform.position = pos;
        scale = startScale;
        transform.localScale = new Vector3(scale, scale);

        isEnabled = enable;

        sp.color = ThemeChanger.current.dotColor;
        Color c = sp.color;

        if (isEnabled)
        {
            c.a = 1;
        }
        else
        {
            c.a = 0;
        }

        sp.color = c;
    }

    public void revertPrvState()
    {
        isEnabled = prvState;
        scale = startScale;
        transform.localScale = new Vector3(scale, scale);

        if (!isEnabled)
        {
            Color c = sp.color;
            c.a = 0f;
            sp.color = c;
        }
    }
    
    public void ResetAnimation()
    {
        // Reset to initial state
        isEnabled = false;
        prvState = false;
        scale = startScale;
        transform.localScale = new Vector3(scale, scale);
        
        // Reset color to transparent
        Color c = sp.color;
        c.a = 0f;
        sp.color = c;
    }

    void Update()
    {
        if (isEnabled)
        {
            scaleChange();
        }
    }

    public void setTargetScale(float target)
    {
        targetScale = target;
    }

    void scaleChange()
    {
        scale += Time.deltaTime * scalingSpeed;
        if (scale > targetScale) scale = startScale;

        float alpha = targetScale - scale;
        changeAlpha(alpha);

        transform.localScale = new Vector3(scale, scale);
    }

    void changeAlpha(float alpha)
    {
        alpha = alpha / targetScale;
        Color c = sp.color;
        c.a = alpha;
        sp.color = c;
    }
}
