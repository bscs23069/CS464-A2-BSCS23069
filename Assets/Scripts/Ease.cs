using UnityEngine;

// easing curves from Lecture 05, t goes from 0 to 1
public static class Ease
{
    public static float Linear(float t) => t;

    public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

    public static float InOutCubic(float t) =>
        t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

    // overshoots to about 1.1, then settles on 1
    public static float OutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
