using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuessUpperSide : MonoBehaviour
{
    public Enums.CubeSide GetSide()
    {
        // Get the world-space down vector of the cube
        Vector3 cubeDown = -transform.up;
        
        // Define a small threshold for float comparison
        float threshold = 0.1f;
        
        // Compare with main axes using dot product
        // When cubeDown points in the same direction as Vector3.up, the TOP face is DOWN
        if (Vector3.Dot(cubeDown, Vector3.up) > 1 - threshold)
            return Enums.CubeSide.Bottom;  // If down points up, bottom face is up
        if (Vector3.Dot(cubeDown, Vector3.down) > 1 - threshold)
            return Enums.CubeSide.Top;     // If down points down, top face is up
        if (Vector3.Dot(cubeDown, Vector3.right) > 1 - threshold)
            return Enums.CubeSide.Left;    // If down points right, left face is up
        if (Vector3.Dot(cubeDown, Vector3.left) > 1 - threshold)
            return Enums.CubeSide.Right;   // If down points left, right face is up
        if (Vector3.Dot(cubeDown, Vector3.forward) > 1 - threshold)
            return Enums.CubeSide.Back;    // If down points forward, back face is up
        if (Vector3.Dot(cubeDown, Vector3.back) > 1 - threshold)
            return Enums.CubeSide.Front;   // If down points back, front face is up
        
        // Default case if no clear side is up
        return Enums.CubeSide.Top;
    }
}