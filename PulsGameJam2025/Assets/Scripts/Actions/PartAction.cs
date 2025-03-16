using System.Collections;
using UnityEngine;

public abstract class PartAction : MonoBehaviour
{
    public abstract int Execute();
    public abstract void Reset();

    public abstract IEnumerator ExecuteCoroutine();
}
