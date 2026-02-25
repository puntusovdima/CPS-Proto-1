using System;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform leftEnd;
    public Transform rightEnd;
    public Transform leftGuide;
    public Transform rightGuide;
    public LineRenderer lineRenderer;
    public float leftLenght, rightLenght;
    public float length;

    private void Awake()
    {
        // Calculate lengths directly from the Transforms for accuracy
        leftLenght = Vector2.Distance(leftEnd.position, leftGuide.position);
        rightLenght = Vector2.Distance(rightEnd.position, rightGuide.position);
        length = leftLenght + rightLenght;
        
        // // Ensure the LineRenderer actually has at least 4 points before we check them
        // if (lineRenderer.positionCount >= 4)
        // {
        //     // Grab the positions from the LineRenderer
        //     Vector2 p0 = lineRenderer.GetPosition(0);
        //     Vector2 p1 = lineRenderer.GetPosition(1);
        //     Vector2 p2 = lineRenderer.GetPosition(2);
        //     Vector2 p3 = lineRenderer.GetPosition(3);
        //
        //     // Calculate the sum of the distances of the outer segments
        //     // leftLenght = Vector2.Distance(p0, p1);
        //     // rightLenght = Vector2.Distance(p2, p3);
        // }
        // else
        // {
        //     Debug.LogWarning("LineRenderer needs at least 4 positions for this calculation.");
        // }
    }

    private void Update()
    {
        lineRenderer.SetPosition(0, leftEnd.position);
        lineRenderer.SetPosition(1, leftGuide.position);
        lineRenderer.SetPosition(2, rightGuide.position);
        lineRenderer.SetPosition(3, rightEnd.position);
        
        // Note: You might also want to update positions 1 and 2 here 
        // to keep the rope looking connected as the ends move.
    }

    public float getMyRopeEnd(bool iAmOnLeftSide)
    {
        return iAmOnLeftSide ?  leftLenght : rightLenght; 
    }

    public float getOpRopeEnd(bool iAmOnLeftSide)
    {
        return iAmOnLeftSide ? rightLenght : leftLenght;
    }
}