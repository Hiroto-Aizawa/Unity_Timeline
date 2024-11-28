#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;

/// <summary>
/// 【エディタ拡張】
/// 親オブジェクトが持つ子オブジェクト整列させる
/// </summary>
public class ModelAutoAlignment : EditorWindow
{
    private Vector2 _scrollPosition = Vector2.zero;
    private GameObject _parentObj;
    private GameObject _effectObj;
    private Vector3 _vec3;

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
        _parentObj = EditorGUILayout.ObjectField("Parent Object", _parentObj, typeof(GameObject), true) as GameObject;

        if(_parentObj == null)
        {
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("キャラモデルの親オブジェクトが指定されていません", MessageType.Error);
        }
        else
        {
            ChangeCharaChildrenPos(_parentObj);
        }
        #endregion

        GUILayout.Space(20);

        #region PARTICLE_OBJ
        EditorGUILayout.LabelField("エフェクトの親オブジェクト", EditorStyles.boldLabel);
        _effectObj = EditorGUILayout.ObjectField("particle Object", _effectObj, typeof(GameObject), true) as GameObject;
        _vec3 = EditorGUILayout.Vector3Field("Position", _vec3);

        GUILayout.Space(20);

        if(_effectObj != null)
        {
            if(GUILayout.Button("座標の更新"))
            {
                ChangeEffectChildrenPos(_effectObj, _vec3);
            }
        }
        #endregion

        GUILayout.Space(20);

        EditorGUILayout.EndScrollView();
    }


    /// <summary>
    /// 子オブジェクトの数によって整列させるためのX座標の配列を返す関数
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private float[] GetXPositions(int count)
    {
        switch(count)
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
                for(int i = 0; i < count; i++)
                {
                    defaultPositions[i] = 0f;
                }
                return defaultPositions;
        }
    }

    /// <summary>
    /// 子オブジェクトの数に応じて整列させる関数
    /// </summary>
    /// <param name="parent"></param>
    private void ChangeCharaChildrenPos(GameObject parent)
    {
        // 親＆孫以下のオブジェクトを含まない、子配列を作成
        Transform[] children = new Transform[parent.transform.childCount];
        int childCount = children.Length;
        //子の要素数に応じて整列させるx座標を返す
        float[] xPositions = GetXPositions(childCount);
        // z座標は1.25で固定する
        float zPosition = 1.25f;

        for(int i = 0; i < childCount; i++)
        {
            // 配列に子を格納する
            children[i] = parent.transform.GetChild(i);

            // 順番に整列させる
            Vector3 newPosition = children[i].position;
            newPosition.x = xPositions[i];
            newPosition.z = zPosition;
            children[i].position = newPosition;
        }
    }

    /// <summary>
    /// 入力されたVector3を元に子オブジェクトを整列させる関数
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="vec3"></param>
    private void ChangeEffectChildrenPos(GameObject parent, Vector3 vec3)
    {
        Transform[] effectChildren = new Transform[parent.transform.childCount];
        int effectChildCount = effectChildren.Length;

        for(int i = 0; i < effectChildCount; i++)
        {
            effectChildren[i] = parent.transform.GetChild(i);

            Vector3 tmpPosition = effectChildren[i].position;
            if(vec3.x != 0)
                tmpPosition.x = vec3.x * i;
            if(vec3.y != 0)
                tmpPosition.y = vec3.y * i;
            if(vec3.z != 0)
                tmpPosition.z = vec3.z * i;

            effectChildren[i].position = tmpPosition;
        }
    }
}

#endif