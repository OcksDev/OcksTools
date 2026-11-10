using UnityEngine;

public class TransformReferences : MonoBehaviour
{
    public CompileableDictionary<Transform> Transforms = new CompileableDictionary<Transform>();
    private void Awake()
    {
        Transforms.Compile();
    }
}
