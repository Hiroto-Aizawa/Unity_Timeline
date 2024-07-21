using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using System.Linq;
using System.Reflection.Emit;

public class DeleteTrackTool : EditorWindow
{
    private GameObject timelineObj;
    private PlayableDirector playableDirector;
    private Vector2 scrollPosition = Vector2.zero;

    private List<bool> isDeleteFlagList = new List<bool>(0);
    private bool isDeleteFlameTrack = true;
    private bool isFirstLoop = true;

    //Playable Directorがセットされた初期のトラック数
    private int initTrackCount;

    //現在のトラック数
    private int trackCount;

    //ツールバーに表示する名前
    [MenuItem("TimelineTools/DeleteTrackTool")]

    private static void Init()
    {
        var window = GetWindow<DeleteTrackTool>("DeleteTrackTool");
        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Space(20);
        //playable directorを取得する
        timelineObj = EditorGUILayout.ObjectField("Playable Director", timelineObj, typeof(GameObject), true) as GameObject;
        GUILayout.Space(20);

        if(timelineObj != null)
        {
            PlayableDirector timelineDirector = timelineObj.GetComponent<PlayableDirector>();

            TimelineAsset timelineAsset = timelineDirector.playableAsset as TimelineAsset;

            //Timeline上にあるトラック数を取得する
            trackCount = timelineAsset.outputTrackCount;

            if(isFirstLoop)
            {
                //初期のトラック数を記録する（削除されたトラックがあるか確認に使う）
                isFirstLoop = false;
                initTrackCount = trackCount;
            }

            if(trackCount > 0)
            {
                // timelineにあるトラックをすべて取得する(GroupTrackは含まない)
                var tracks = timelineAsset.GetOutputTracks();

                // timelineにあるルートトラックのリストを取得する（GroupTrackも含む）
                var groupTracks = timelineAsset.GetRootTracks() as List<TrackAsset>;


                //削除するトラックを格納する配列
                //要素（トラック）の追加ができるように初期化する
                List<TrackAsset> deleteTracks = new List<TrackAsset>(0);

                GUILayout.Label($"トラック数: {trackCount}");

                GUILayout.Space(20);

                // 削除用のリストを動的に作成するためのindex
                int i = 0;

                foreach(TrackAsset track in tracks)
                {

                    // TimelineにあるMarkersトラックは削除対象にしない
                    if(track.isEmpty && track.name != "Markers")
                    {
                        Debug.Log($"Loop i: {i} \n isDeleteFlagList: {isDeleteFlagList.Count}");
                        //動的に削除用のリストの要素を増やす
                        if(isDeleteFlagList.Count < i + 1)
                        {
                            //Debug.Log($"i count: {i}");
                            isDeleteFlagList.Add(false);
                        }

                        GUILayout.Label($"【グループ名】 \n ・{track.parent.name}");
                        GUILayout.Label($"【トラック名】 \n ・{track.name}");
                        GUILayout.Label("空トラックを検知");
                        isDeleteFlagList[i] = GUILayout.Toggle(isDeleteFlagList[i], "削除リストに追加する");


                        //削除用のリストに追加する
                        if(isDeleteFlagList[i])
                        {
                            deleteTracks.Add(track);
                        }
                        else
                        {
                            deleteTracks.Remove(track);
                        }

                        GUILayout.Space(20);

                        //ループ毎にindexを増加させる
                        i++;
                    }
                    else if(track.muted && track.name != "Markers")
                    {
                        //動的に削除用のリストの要素を増やす
                        if(isDeleteFlagList.Count < i + 1)
                        {
                            isDeleteFlagList.Add(false);
                        }

                        GUILayout.Label($"【グループ名】 \n ・{track.parent.name}");
                        GUILayout.Label($"【トラック名】 \n ・{track.name}");
                        GUILayout.Label("ミュートされたトラックを検知");
                        isDeleteFlagList[i] = GUILayout.Toggle(isDeleteFlagList[i], "削除リストに追加する");

                        //削除用のリストに追加する
                        if(isDeleteFlagList[i])
                        {
                            deleteTracks.Add(track);

                        }
                        else
                        {
                            deleteTracks.Remove(track);
                        }

                        GUILayout.Space(20);

                        //ループ毎にindexを増加させる
                        i++;
                    }
                }

                Debug.Log($"loop count: {i}");

                //FLAMEトラックグループがあるか
                if(groupTracks.Count > 0 && groupTracks.Find(track => track.name == "FLAME"))
                {
                    try
                    {
                        var groupTrack = groupTracks.Find(track => track.name == "FLAME");

                        GUILayout.Label($"【グループ名】\n ・{groupTrack.name} ");
                        isDeleteFlameTrack = GUILayout.Toggle(isDeleteFlameTrack, "FLAMEトラックを削除リストに追加する");
                        GUILayout.Space(20);

                        if(isDeleteFlameTrack)
                        {
                            deleteTracks.Add(groupTrack);
                        }
                        else
                        {
                            deleteTracks.Remove(groupTrack);
                        }
                    }
                    catch(System.Exception e)
                    {
                        Debug.Log($"error: {e}");
                    }
                }

                //手動でTimelineのトラックが削除されたら、リスト作り直して
                //TimelineとEditorの要素が合うようにする
                if(initTrackCount != trackCount)
                {
                    //手動でトラックが削除されたら初回ループフラグを有効にする
                    isFirstLoop = true;
                    isDeleteFlameTrack = true;
                    isDeleteFlagList.Clear();
                }

                /*GUILayout.Label($"remove count: {deleteTracks.Count}");
                GUILayout.Space(10);
                GUILayout.Label("trackCount: " + trackCount);
                GUILayout.Space(10);
                GUILayout.Label("flag list: " + isDeleteFlagList.Count);
                GUILayout.Space(10);*/

                if(GUILayout.Button("トラックを削除する"))
                {
                    foreach(TrackAsset deleteTrack in deleteTracks)
                    {
                        timelineAsset.DeleteTrack(deleteTrack);
                        //データ削除後にTimelineに変更内容を反映する
                        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
                    }
                    //削除処理の後に初回ループのフラグを有効にする
                    isFirstLoop = true;
                    isDeleteFlameTrack = true;
                    isDeleteFlagList.Clear();
                }

                GUILayout.Space(20);
            }
            else
            {
                GUILayout.Label("トラックが存在しないTimelineです。");
            }
        }

        EditorGUILayout.EndScrollView();
    }

}
