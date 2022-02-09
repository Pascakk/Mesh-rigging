using UnityEditor;
using UnityEngine;

public class MathUtility : ScriptableObject
{
    public static int ClampListIndex(int index, int listSize)
    {
        index = ((index % listSize) + listSize) % listSize;

        return index;
    }
}