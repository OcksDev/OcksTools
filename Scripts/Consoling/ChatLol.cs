using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatLol : MonoBehaviour
{
    public GameObject ChatLog;
    public GameObject ChatText;
    public GameObject CameraL;
    public int amnt = 0;

    public static ChatLol instance;
    private VerticalLayoutGroup bonerspawn;
    public static ChatLol Instance
    {
        get { return instance; }

    }

    private void Awake()
    {
        if (Instance == null) instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        bonerspawn = ChatLog.GetComponentInChildren<VerticalLayoutGroup>();
    }

    public void Update()
    {
        /* chat test to see if it is working
        if (Input.GetKeyDown(KeyCode.Space))
        {
            WriteChat("bonesssssssssssssssssssssssr", "#" + Random.ColorHSV().ToHexString());
        }
        */
    }

    public void WriteChat(string text = "Logged", string hex = "\"white\"")
    {
        string s = "<color=" + hex + ">" + text;
        Vector3 pos = CameraL.transform.position;
        pos.z = 0;
        var f = GameObject.Instantiate(ChatText, pos, Quaternion.Euler(0, 0, 0), bonerspawn.transform);

        f.GetComponent<TextMeshProUGUI>().text = s;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        amnt = ChatLog.GetComponentsInChildren<ChatThing>().Length;
    }
}
