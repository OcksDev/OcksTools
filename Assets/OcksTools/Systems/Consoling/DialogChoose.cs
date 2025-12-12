using UnityEngine;

public class DialogChoose : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void Clicky()
    {
        DialogLol.Instance.Choose(int.Parse(gameObject.name));
    }
}
