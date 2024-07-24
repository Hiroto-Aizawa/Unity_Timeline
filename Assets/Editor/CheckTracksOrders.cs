using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using System.Linq;
using Sandbox.Project3D.SDFGenerate.Scripts;

public class CheckTracksOrders : EditorWindow
{
    private Vector2 _scrollPositon = Vector2.zero;
    private PlayableDirector _playableDirector;
    private GameObject _timelineObj;
    private int _trackCount;
    private List<string> _trackOrderList = new List<string>
    {
        "トラック01",
        "トラック02",
        "トラック03",
        "トラック04",
        "トラック05",
        "トラック06",
        "トラック07",
        "トラック08",
        "トラック09",
        "トラック10",
    };

    [MenuItem("TimelineTools/CheckTracksOrders")]

    private static void Init()
    {
        var window = GetWindow<CheckTracksOrders>("CheckTracksOrders");
        window.Show();
    }

    /// <summary>
    /// Timelineのトラックが順番通りに配置されているかをチェックする
    /// </summary>
    private void OnGUI()
    {
        _scrollPositon = EditorGUILayout.BeginScrollView(_scrollPositon);
        GUILayout.Space(20);
        _timelineObj = EditorGUILayout.ObjectField("Timeline Object", _timelineObj, typeof(GameObject), true) as GameObject;
        GUILayout.Space(20);

        if (_timelineObj != null)
        {
            PlayableDirector timelineDirector = _timelineObj.GetComponent<PlayableDirector>();
            TimelineAsset timelineAsset = timelineDirector.playableAsset as TimelineAsset;

            //Timeline上のトラック数を返す
            _trackCount = timelineAsset.outputTrackCount;

            GUILayout.Label($"トラック数: {_trackCount}");

            if (_trackCount > 0)
            {
                // GroupTrackをList<TrackAsset>にキャストして取得する
                List<TrackAsset> groupTracks = timelineAsset.GetRootTracks() as List<TrackAsset>;

                //_trackOrderListと一致するトラックだけのリストを作成
                groupTracks = groupTracks.Where(e => _trackOrderList.Contains(e.name)).ToList();
                //groupTracksからトラック名のみのリストを作成する
                List<string> trackNameList = groupTracks.Select(e => e.name).ToList();

                if (groupTracks.Count > 0)
                {
                    int i = 0;


                    foreach (string trackName in trackNameList)
                    {
                        //トラックの配置順が間違っているものをツールに表示する
                        if (trackName != _trackOrderList[i])
                        {
                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = Color.red;

                            GUILayout.Label($"trackName: {trackName}");
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"ここには、【{_trackOrderList[i]}】を配置してください");
                            GUILayout.Space(20);
                        }

                        i++;
                    }
                }
            }
            else
            {
                GUILayout.Label("トラックが存在しないTimelineです。");
            }
        }

        EditorGUILayout.EndScrollView();
    }
}