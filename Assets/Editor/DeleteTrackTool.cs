using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using System.Linq;

public class DeleteTrackTool : EditorWindow
{
    private GameObject timelineObj;
    private PlayableDirector playableDirector;
    private Vector2 scrollPosition = Vector2.zero;

    private List<bool> isDeleteFlagList = new List<bool>(0);
    private bool isDeleteFlameTrack = true;
    private bool isNeedInit = true;
    private string description;

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

    /// <summary>
    /// 空 or ミュートされたトラックを検出して一覧表示する
    /// 一覧表示されたチェックボックスで有効になっているトラックをまとめて削除する。
    /// </summary>
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Space(20);
        //playable directorを含んだGameObjectを取得する
        timelineObj = EditorGUILayout.ObjectField("Playable Director", timelineObj, typeof(GameObject), true) as GameObject;
        GUILayout.Space(20);

        if (timelineObj != null)
        {
            PlayableDirector timelineDirector = timelineObj.GetComponent<PlayableDirector>();

            TimelineAsset timelineAsset = timelineDirector.playableAsset as TimelineAsset;

            //Timeline上にあるトラック数を取得する
            trackCount = timelineAsset.outputTrackCount;

            if (isNeedInit)
            {
                //初期のトラック数を記録する（削除されたトラックがあるか確認に使う）
                isNeedInit = false;
                initTrackCount = trackCount;
            }

            if (trackCount > 0)
            {
                // timelineにあるトラックをすべて取得する(GroupTrackは含まない)
                var tracks = timelineAsset.GetOutputTracks();

                // timelineにあるルートトラックのリストを取得する（GroupTrackも含む）
                var groupTracks = timelineAsset.GetRootTracks() as List<TrackAsset>;


                //削除するトラックを格納する配列
                List<TrackAsset> deleteTracks = new List<TrackAsset>(0);

                GUILayout.Label($"トラック数: {trackCount}");

                GUILayout.Space(20);

                int i = 0;

                //空 or ミュートされているトラックを削除用リストに格納する。
                //Makerトラックは空 or ミュートでもリストには含めない
                tracks = tracks.Where(e => (e.isEmpty || e.muted) && e.name != "Markers").ToList();

                foreach (TrackAsset track in tracks)
                {
                    if (isDeleteFlagList.Count < i + 1)
                    {
                        isDeleteFlagList.Add(false);
                    }

                    GUILayout.Label($"【グループ名】 \n ・{track.parent.name}");
                    GUILayout.Label($"【トラック名】 \n ・{track.name}");
                    description = track.isEmpty ? description = "空トラックを検知" : description = "ミュートされたトラックを検知";
                    GUILayout.Label(description);
                    isDeleteFlagList[i] = GUILayout.Toggle(isDeleteFlagList[i], "削除リストに追加する");

                    if (isDeleteFlagList[i])
                        deleteTracks.Add(track);
                    else
                        deleteTracks.Remove(track);

                    GUILayout.Space(20);

                    //ループ毎にindexを増加させる
                    i++;
                }

                //FLAMEトラックグループがあるか
                if (groupTracks.Find(track => track.name == "FLAME"))
                {
                    try
                    {
                        var groupTrack = groupTracks.Find(track => track.name == "FLAME");

                        GUILayout.Label($"【グループ名】\n ・{groupTrack.name} ");
                        isDeleteFlameTrack = GUILayout.Toggle(isDeleteFlameTrack, "FLAMEトラックを削除リストに追加する");
                        GUILayout.Space(20);

                        if (isDeleteFlameTrack)
                            deleteTracks.Add(groupTrack);
                        else
                            deleteTracks.Remove(groupTrack);
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log($"error: {e}");
                    }
                }

                //手動でTimelineのトラックが削除されたら、リスト作り直して
                //TimelineとEditorの要素が合うようにする
                if (initTrackCount != trackCount)
                {
                    //手動でトラックが削除されたら初期化フラグを有効にする
                    isNeedInit = true;
                    isDeleteFlameTrack = true;
                    isDeleteFlagList.Clear();
                }

                if (GUILayout.Button("トラックを削除する"))
                {
                    foreach (TrackAsset deleteTrack in deleteTracks)
                    {
                        timelineAsset.DeleteTrack(deleteTrack);
                        //データ削除後にTimelineに変更内容を反映する
                        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
                    }
                    //削除処理の後にトラック数の変動があるので初期化フラグを有効にする
                    isNeedInit = true;
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
