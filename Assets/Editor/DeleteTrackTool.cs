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
    private GameObject _timelineObj;
    private PlayableDirector _playableDirector;
    private Vector2 _scrollPosition = Vector2.zero;

    private List<bool> _isDeleteFlagList = new List<bool>(0);
    // 特定のトラックグループを削除するか？
    private bool _isDeleteSpecificTrack = true;
    private bool _isNeedInit = true;
    private bool _isDeleteAllTrack = false;
    private bool _isToggleFirstChange = true;
    private string _description;

    //Playable Directorがセットされた初期のトラック数
    private int _initTrackCount;

    //現在のトラック数
    private int _trackCount;

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
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        GUILayout.Space(20);
        //playable directorを含んだGameObjectを取得する
        _timelineObj = EditorGUILayout.ObjectField("Playable Director", _timelineObj, typeof(GameObject), true) as GameObject;
        GUILayout.Space(20);

        if (_timelineObj != null)
        {
            PlayableDirector timelineDirector = _timelineObj.GetComponent<PlayableDirector>();

            TimelineAsset timelineAsset = timelineDirector.playableAsset as TimelineAsset;

            //Timeline上にあるトラック数を取得する
            _trackCount = timelineAsset.outputTrackCount;

            if (_isNeedInit)
            {
                //初期のトラック数を記録する（削除されたトラックがあるか確認に使う）
                _isNeedInit = false;
                _initTrackCount = _trackCount;
            }

            if (_trackCount > 0)
            {
                // timelineにあるトラックをすべて取得する(GroupTrackは含まない)
                var tracks = timelineAsset.GetOutputTracks();

                // timelineにあるルートトラックのリストを取得する（GroupTrackも含む）
                var groupTracks = timelineAsset.GetRootTracks() as List<TrackAsset>;


                //削除するトラックを格納する配列
                List<TrackAsset> deleteTracks = new List<TrackAsset>(0);

                GUILayout.Label($"トラック数: {_trackCount}");

                _isDeleteAllTrack = GUILayout.Toggle(_isDeleteAllTrack, "一覧にあるトラックをすべて削除する");

                //isDeleteAllTrackだとListのboolが常にtrueになり任意のトラックだけをfalseにできないため
                //isToggleFirstChangeでフラグが有効な時だけ処理を実行する
                if (_isDeleteAllTrack && _isToggleFirstChange)
                {
                    //削除用リスト内のフラグをすべてtrueに変更する
                    _isDeleteFlagList = _isDeleteFlagList.Select(e => e = true).ToList();
                    _isToggleFirstChange = false;
                }
                //こちらも同様にisToggleFirstChangeを利用し、isDeleteAllTrackが変更された瞬間のみ処理を実行する
                else if (!_isDeleteAllTrack && !_isToggleFirstChange)
                {
                    //削除用リスト内のフラグをすべてfalseに変更する
                    _isDeleteFlagList = _isDeleteFlagList.Select(e => e = false).ToList();
                    //フラグをリセットする
                    _isToggleFirstChange = true;
                }

                GUILayout.Space(20);

                int i = 0;

                //空 or ミュートされているトラックを削除用リストに格納する。
                //Makerトラックは空 or ミュートでもリストには含めない
                tracks = tracks.Where(e => (e.isEmpty || e.muted) && e.name != "Markers").ToList();

                foreach (TrackAsset track in tracks)
                {
                    if (_isDeleteFlagList.Count < i + 1)
                    {
                        _isDeleteFlagList.Add(false);
                    }

                    GUILayout.Label($"【グループ名】 \n ・{track.parent.name}");
                    GUILayout.Label($"【トラック名】 \n ・{track.name}");
                    _description = track.isEmpty ? _description = "空トラックを検知" : _description = "ミュートされたトラックを検知";
                    GUILayout.Label(_description);
                    _isDeleteFlagList[i] = GUILayout.Toggle(_isDeleteFlagList[i], "削除リストに追加する");

                    if (_isDeleteFlagList[i])
                        deleteTracks.Add(track);
                    else
                        deleteTracks.Remove(track);

                    GUILayout.Space(20);

                    //ループ毎にindexを増加させる
                    i++;
                }

                //〇〇トラックグループがあるか
                if (groupTracks.Find(track => track.name == "〇〇"))
                {
                    try
                    {
                        var groupTrack = groupTracks.Find(track => track.name == "〇〇");

                        GUILayout.Label($"【グループ名】\n ・{groupTrack.name} ");
                        _isDeleteSpecificTrack = GUILayout.Toggle(_isDeleteSpecificTrack, "〇〇トラックを削除リストに追加する");
                        GUILayout.Space(20);

                        if (_isDeleteSpecificTrack)
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
                if (_initTrackCount != _trackCount)
                {
                    //手動でトラックが削除されたら初期化フラグを有効にする
                    _isNeedInit = true;
                    _isDeleteSpecificTrack = true;
                    _isDeleteAllTrack = false;
                    _isToggleFirstChange = false;
                    _isDeleteFlagList.Clear();
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
                    _isNeedInit = true;
                    _isDeleteSpecificTrack = true;
                    _isDeleteAllTrack = false;
                    _isToggleFirstChange = false;
                    _isDeleteFlagList.Clear();
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

