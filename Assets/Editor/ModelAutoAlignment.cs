using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;

public class ModelAutoAlignment : EditorWindow
{
    private Vector2 _scrollPosition = Vector2.zero;
    private GameObject parentObj;
    //[SerializeField]List<GameObject> parentList = new List<GameObject>();
    [SerializeField] List<GameObjectAndTransform> gameObjList = new List<GameObjectAndTransform>();
    GameObjectAndTransform gameObjectAndTransform = new GameObjectAndTransform();

    [MenuItem("Tools/ModelAutoAlignment")]
    private static void Init()
    {
        var window = GetWindow<ModelAutoAlignment>("ModelAutoAlignment");
        window.Show();
    }


    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        GUILayout.Space(20);
        parentObj = EditorGUILayout.ObjectField("Parent Object", parentObj, typeof(GameObject), true) as GameObject;

        if (parentObj == null)
        {
            EditorGUILayout.HelpBox("親オブジェクトが指定されていません", MessageType.Error);
            //return;
        }
        else
        {
            // 親自身も含めて子供を取得している点に注意
            Transform[] children = parentObj.GetComponentsInChildren<Transform>();
            // 親オブジェクトを含めない子だけのリストを作成する
            Transform[] actualChildren = new Transform[children.Length - 1];

            for (int i = 1; i < children.Length; i++)
            {
                actualChildren[i - 1] = children[i];
            }

            int childCount = actualChildren.Length;
            float[] xPositions = GetXPositions(childCount);
            // キャラモデルのz座標は1.25に固定する
            float zPosition = 1.25f;

            // 子オブジェクトの位置を変更する
            for (int i = 0; i < childCount; i++)
            {
                Vector3 newPosition = actualChildren[i].position;
                newPosition.x = xPositions[i];
                newPosition.z = zPosition;
                actualChildren[i].position = newPosition;

            }
        }

        GUILayout.Space(20);

        // このクラスのSerializedObjectを取得する
        var so = new SerializedObject(this);
        so.Update();
        // 第二引数をtrueにしたPropertyFieldで描画する
        EditorGUILayout.PropertyField(so.FindProperty("gameObjList"), true);
        so.ApplyModifiedProperties();
        GUILayout.Space(20);

        if (0 < gameObjList.Count && gameObjList[0].gameObject != null)
        {
            if (GUILayout.Button("Adjust Durations"))
            {
                gameObjectAndTransform.UpdateVector3(gameObjList);
                Debug.Log("Button clicked");
            }
        }

        EditorGUILayout.EndScrollView();
    }

    // 子オブジェクトの数によってX座標の配列を返す
    float[] GetXPositions(int count)
    {
        switch (count)
        {
            case 1:
                return new float[] { 0 };
            case 2:
                return new float[] { -1f, 1f };
            case 3:
                return new float[] { -2f, 0f, 2f };
            case 4:
                return new float[] { -2.25f, -0.75f, 0.75f, 2.25f };
            case 5:
                return new float[] { -3f, -1.5f, 0f, 1.5f, 3f };
            case 6:
                return new float[] { -3f, -1.8f, -0.6f, 0.6f, 1.8f, 3f };
            default:
                // それ以外の場合はX座標を0に指定する
                float[] defaultPositions = new float[count];
                for (int i = 0; i < count; i++)
                {
                    defaultPositions[i] = 0f;
                }
                return defaultPositions;
        }
    }
}

[System.Serializable]
public class GameObjectAndTransform
{
    public GameObject gameObject;
    // public Transform transform;
    public Vector3 position;

    // コンストラクタ：GameObjectを指定してインスタンスを作成する
    // public GameObjectAndTransform(GameObject obj)
    // {
    //     gameObject = obj;
    //     position = obj.transform.position;
    // }

    public void UpdateVector3(List<GameObjectAndTransform> gameObjList)
    {
        if (0 < gameObjList.Count)
        {
            for (int i = 0; i < gameObjList.Count; i++)
            {
                Debug.Log(
                    $"count: {i} \n gameObj: {gameObjList[i].gameObject.name} \n vector3: {gameObjList[i].position}"
                );
            }
        }
        Debug.Log("UpdateVector3 called");
    }
}