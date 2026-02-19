using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform leftEnd;
    public Transform rightEnd;
    public LineRenderer lineRenderer;
    private void Update()
    {
        lineRenderer.SetPosition(0, leftEnd.position);
        lineRenderer.SetPosition(3, rightEnd.position);
    }
}
