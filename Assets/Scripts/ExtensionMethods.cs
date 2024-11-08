using UnityEngine;

public static class ExtensionMethods
{
    public static bool IsOutOfScreen(this Transform transform, float offset)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        bool isOutOfScreen = viewportPos.x < -offset || viewportPos.x > 1 + offset ||
                             viewportPos.y < -offset || viewportPos.y > 1 + offset;
        return isOutOfScreen;
    }
}