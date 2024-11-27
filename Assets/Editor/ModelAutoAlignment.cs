using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ModelAutoAlignment : EditorWindow
{
    private Vector2 _scrollPosition = Vector2.zero;
    private GameObject parentObj;
    private GameObject particleObj;
    private Vector3 vec3;

    [MenuItem("Tools/ModelAutoAlignment")]
    private static void Init()
    {
        var window = GetWindow<ModelAutoAlignment>("ModelAutoAlignment");
        window.Show();
    }


    private void OnGUI()
    {
        // Windowを超えるUIがある場合はスクロールバーを表示する
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Space(20);
        #region PARENT_OBJ
        EditorGUILayout.LabelField("キャラモデルの親オブジェクト", EditorStyles.boldLabel);
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
        #endregion

        GUILayout.Space(20);

        #region PARTICLE_OBJ
        EditorGUILayout.LabelField("エフェクトの親オブジェクト", EditorStyles.boldLabel);
        particleObj = EditorGUILayout.ObjectField("particle Object", particleObj, typeof(GameObject), true) as GameObject;

        vec3 = EditorGUILayout.Vector3Field("Position", vec3);

        GUILayout.Space(20);

        if (particleObj != null)
        {
            Transform[] tmpChildren = particleObj.GetComponentsInChildren<Transform>();
            // tmpChildrenは親も含んだ配列なので、親の要素分を除いた長さの配列を作成
            Transform[] particleChildren = new Transform[tmpChildren.Length - 1];

            if (GUILayout.Button("座標の更新"))
            {
                // リストから親の要素を除外するために、particleChildrenに親以外の要素を入れていく
                // 親要素がtmpChildrenの0番目にいるので、1番目からparticleChildrenに入れる
                for (int i = 1; i < tmpChildren.Length; i++)
                {
                    particleChildren[i - 1] = tmpChildren[i];
                }

                for (int i = 0; i < particleChildren.Length; i++)
                {
                    Vector3 tmpPosition = particleChildren[i].position;
                    // indexを利用してvec3の値をかけて代入する
                    // これによって等間隔に整列させる
                    if (vec3.x != 0)
                        tmpPosition.x = vec3.x * i;
                    if (vec3.y != 0)
                        tmpPosition.y = vec3.y * i;
                    if (vec3.z != 0)
                        tmpPosition.z = vec3.z * i;

                    Debug.Log($"name: {particleChildren[i].name} \n position: {tmpPosition}");

                    particleChildren[i].position = tmpPosition;
                }
            }
        }
        #endregion

        GUILayout.Space(20);

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