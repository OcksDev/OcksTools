#if (UNITY_EDITOR)

using UnityEditor;
using UnityEngine;

public class ConsoleSetupWindow : EditorWindow
{
    private GameObject s;
    private GameObject s3;
    private int c = 0;
    private int c2 = 0;
    private string s_d = "";
    private bool gaming = false;
    [MenuItem("OcksTools/Console/Utils")]
    public static void ShowWindow()
    {
        var f = GetWindow<ConsoleSetupWindow>("Console Utils");
    }

    private void OnGUI()
    {
        GUILayout.Space(15);
        var f22 = new string[2] { "Setup", "Dialog" };
        c = GUILayout.Toolbar(c, f22);

        GUILayout.Space(15);
        switch (c)
        {
            case 0:
                s = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Parent Of UI Elements", "(required) The transform parents of any and all UI elements that are being created."), s, typeof(GameObject), true);
                if (GUILayout.Button(new GUIContent("Setup Console", "Creates the needed objects and references")))
                {
                    var f1 = "Console";
                    var f2 = "ConsoleHandler";

                    var f = (GameObject)Resources.Load(f1);
                    var t1 = (GameObject)PrefabUtility.InstantiatePrefab(f);
                    t1.transform.position = new Vector3(-5.23749303817749f, -2.491374969482422f, 0) + s.transform.position;
                    t1.transform.parent = s.transform;
                    t1.transform.localScale = Vector3.one;
                    var t2 = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)Resources.Load(f2));
                    t2.transform.position = Vector3.zero;
                    var a = t2.GetComponent<ConsoleLol>();
                    a.ConsoleObject = t1;

                    //ExecuteEvents.Execute(t1.GetComponent<ConsolRefs>().input.gameObject, null, ExecuteEvents.submitHandler);

                    t1.name = f1;
                    t2.name = f2;

                }
                if (GUILayout.Button(new GUIContent("Setup Dialog", "Creates the needed objects and references (need to manually make the text input call the correct function, sorry)")))
                {

                    var f1 = "DialogBox";
                    var f2 = "DialogManager";

                    var f = (GameObject)Resources.Load(f1);
                    var t1 = (GameObject)PrefabUtility.InstantiatePrefab(f);
                    t1.transform.position = new Vector3(0f, -2.783632516860962f, 0) + s.transform.position;
                    t1.transform.parent = s.transform;
                    t1.transform.localScale = Vector3.one;
                    var t2 = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)Resources.Load(f2));
                    t2.transform.position = Vector3.zero;
                    t2.GetComponent<DialogLol>().DialogBoxObject = t1;

                    t1.name = f1;
                    t2.name = f2;

                }
                break;
            case 1:

                var f222 = new string[2] { "Dialog", "Choice" };
                c2 = GUILayout.Toolbar(c2, f222);
                gaming = EditorGUILayout.Toggle(new GUIContent("Editor?", "Check this if your in the editor"), gaming);
                if (gaming) s3 = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Dialog Manager", "The Dialog Manager GameObject used for managing dialog."), s3, typeof(GameObject), true);
                GUILayout.Space(5);
                switch (c2)
                {
                    case 0:
                        s_d = EditorGUILayout.TextField(new GUIContent("Select Dialog", "The index of the dialog file within the Dialog Manager"), s_d);
                        var gg = (gaming ? s3.GetComponent<DialogLol>() : DialogLol.Instance);
                        string ss = "";
                        try
                        {
                            ss = gg.GetLineFrom(s_d, 0);
                        }
                        catch
                        {
                            ss = "Failed To Get File ";
                        }
                        GUILayout.Label("Currently Selected: " + ss.Substring(0, ss.Length - 1));
                        if (!gaming)
                        {

                            GUILayout.BeginHorizontal();
                            GUILayout.Space(5);
                            if (GUILayout.Button(new GUIContent("Start", "Starts the Dialog")))
                            {
                                gg.StartDialog(s_d);
                            }
                            if (GUILayout.Button(new GUIContent("End", "Ends the Dialog")))
                            {
                                gg.ResetDialog();
                            }

                            GUILayout.EndHorizontal();
                        }
                        break;
                    case 1:
                        s_d = EditorGUILayout.TextField(new GUIContent("Select Choice", "The index of the choice file within the Dialog Manager"), s_d);
                        var gg2 = (gaming ? s3.GetComponent<DialogLol>() : DialogLol.Instance);
                        string ss2 = "";
                        try
                        {
                            ss2 = gg2.GetLineFrom(s_d, 0, "Choose");
                        }
                        catch
                        {
                            ss2 = "Failed To Get File ";
                        }
                        GUILayout.Label("Currently Selected: " + ss2.Substring(0, ss2.Length - 1));
                        if (!gaming)
                        {

                            GUILayout.BeginHorizontal();
                            GUILayout.Space(5);
                            if (GUILayout.Button(new GUIContent("Start", "Starts the Choice Menu")))
                            {
                                gg2.StartDialog(s_d, "Choose");
                            }
                            if (GUILayout.Button(new GUIContent("End", "Ends the Choice Menu")))
                            {
                                gg2.ResetDialog();
                            }

                            GUILayout.EndHorizontal();
                        }
                        break;
                }

                break;
        }

    }
}
#endif
