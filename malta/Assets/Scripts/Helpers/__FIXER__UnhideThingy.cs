#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// Tiny stub component for use in editor when I accidentally the hide flags.
/// Attach it to an object, ant it'll unhide it and destroy itself.
/// </summary>
[ExecuteInEditMode]
public class __FIXER__UnhideThingy : MonoBehaviour
{
    void Awake ()
    {
        gameObject.hideFlags = HideFlags.None;
        DestroyImmediate(this);
    }
}
#endif