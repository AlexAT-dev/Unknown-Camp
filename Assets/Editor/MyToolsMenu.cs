using System.Linq;
using UnityEditor;
using UnityEngine;

public class MyToolsMenu : EditorWindow
{
    [MenuItem("MyTools/PlayerPrefs/Clear")]
    public static void PlayerPrefsClear()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("MyTools/PlayerPrefs/Add 1000 Matches")]
    public static void PlayerPrefsAdd()
    {
        PlayerPrefs.SetInt("matches", PlayerPrefs.GetInt("matches") + 1000);
    }


    [MenuItem("MyTools/Swap Transform YZ")]
    public static void SwapYZPositions()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject obj in selectedObjects)
        {
            Undo.RecordObject(obj.transform, "Swap YZ Positions");

            Vector3 position = obj.transform.position;
            position = new Vector3(position.x, position.z, position.y);
            obj.transform.position = position;
        }
    }

    [MenuItem("MyTools/SetXRotation")]
    public static void SetXRotation()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        Selection.objects = new Object[0];

        foreach (GameObject obj in allObjects)
        {
            if (Mathf.Approximately(obj.transform.localRotation.eulerAngles.x, 70f))
            {
                Selection.objects = Selection.objects.Append(obj).ToArray();
            }
        }
    }
}