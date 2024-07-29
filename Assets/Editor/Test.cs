using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class FlameCheckTracksOrder : EditorWindow
{
    private Vector2 _scrollPositon = Vector2.zero;
    private PlayableDirector _playableDirector;
    private GameObject _timelineObj;
    private int _trackCount;
    private List<string> _correctTrackOrder = new List<string>
    {
        "全体設定",
        "ステージ光源",
        "キャラ光源",
        "ステージ",
        "エフェクト",
        "音声",
        "動画",
        "モブ",
        "コーレス",
    };
    private List<string> _stageLightOrder = new List<string>
    {
        "StageAmbientLight",
        "StagePointLight0",
        "StagePointLight1",
        "StagePointLight2",
        "FlareLight",
    };

    [MenuItem("Flame/FlameCheckTracksOrder")]

    private static void Init()
    {
        var window = GetWindow<FlameCheckTracksOrder>("FlameCheckTracksOrder");
        window.Show();
    }

    private void OnGUI()
    {
        _scrollPositon = EditorGUILayout.BeginScrollView(_scrollPositon);
        GUILayout.Space(20);
        _timelineObj = EditorGUILayout.ObjectField("Timeline Object", _timelineObj, typeof(GameObject), true) as GameObject;
        GUILayout.Space(20);

        if(_timelineObj != null)
        {
            PlayableDirector timelineDirector = _timelineObj.GetComponent<PlayableDirector>();
            TimelineAsset timelineAsset = timelineDirector.playableAsset as TimelineAsset;

            //Timeline上のトラック数を返す
            _trackCount = timelineAsset.outputTrackCount;

            GUILayout.Label($"トラック数: {_trackCount}");
            GUILayout.Space(20);

            if(_trackCount > 0)
            {
                // ルートトラックの中でもGroupTrackのみをList<TrackAsset>にキャストして取得する
                //List<TrackAsset> groupTracks = timelineAsset.GetRootTracks().OfType<GroupTrack>() as List<TrackAsset>;
                List<TrackAsset> groupTracks = timelineAsset.GetRootTracks() as List<TrackAsset>;

                if(groupTracks.Count > 0)
                {
                    groupTracks = groupTracks.Where(e => _correctTrackOrder.Contains(e.name)).ToList();
                    List<string> tracksName = groupTracks.Select(e => e.name).ToList();

                    int i = 0;

                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red;

                    // groupTracksリストのGroupTrackである要素だけを取り出す
                    foreach(TrackAsset groupTrack in groupTracks.OfType<GroupTrack>())
                    {
                        //GroupTrackがないTimelineで要検証
                        if(groupTrack.name != _correctTrackOrder[i])
                        {
                            GUILayout.Label($"trackName: {groupTrack.name}");
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"ここには、【{_correctTrackOrder[i]}】を配置してください");
                            GUILayout.Space(20);
                        }
                        else
                        {

                            List<TrackAsset> childTracks;
                            switch(groupTrack.name)
                            {
                                case "全体設定":
                                    CheckGlobalSettingsTrack(groupTrack);
                                    break;
                                case "ステージ光源":
                                    CheckStageLightTrack(groupTrack);
                                    break;
                                case "キャラ光源":
                                    childTracks = groupTrack.GetChildTracks() as List<TrackAsset>;
                                    break;
                                case "ステージ":
                                    childTracks = groupTrack.GetChildTracks() as List<TrackAsset>;
                                    break;
                                case "エフェクト":
                                    childTracks = groupTrack.GetChildTracks() as List<TrackAsset>;
                                    break;
                                case "音声":
                                    childTracks = groupTrack.GetChildTracks() as List<TrackAsset>;
                                    break;
                                case "動画":
                                    childTracks = groupTrack.GetChildTracks() as List<TrackAsset>;
                                    break;
                                case "モブ":
                                    childTracks = groupTrack.GetChildTracks() as List<TrackAsset>;
                                    break;
                                case "コーレス":
                                    CheckCallAndResponseTrack(groupTrack);
                                    break;
                                default:
                                    break;
                            }
                        }

                        i++;
                    }

                    //明示的な変換（キャスト）
                    /*groupTracks = (List<TrackAsset>)groupTracks.Select(groupTrack =>
                        new { Name = groupTrack.name, Index = ToIndex(groupTrack) }).
                            OrderBy(groupTrack => groupTrack.Index).Select(groupTrack => groupTrack.Name);
                    */
                }
            }
            else
            {
                GUILayout.Label("トラックが存在しないTimelineです。");
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void CheckGlobalSettingsTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "GlobalSettings").ToList();

            if(trackList.Count > 0)
            {
                List<TrackAsset> tmpTracks = childTracks.Where(e => e.name != "GlobalSettings").ToList();

                if(tmpTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in tmpTracks)
                    {
                        GUILayout.Label($"【全体設定】 \n 不要なトラックが含まれています。");
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"GlobalSettingsトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");
            }
        }
        else
        {
            GUILayout.Label($"【全体設定】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckStageLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            //childTracksが_stageLightOrderの要素数より多い場合は不正データ

            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where( (e, index) => e.name == _stageLightOrder[index] ).ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where( (e, index) => e.name != _stageLightOrder[index] ).ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                int i = 0;
                GUILayout.Label($"【ステージ光源】 \n 不要なトラックが含まれています。");
                foreach(TrackAsset tmpTrack in invalidTracks)
                {
                    GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    if(tmpTrack.name != _stageLightOrder[i])
                    {
                        GUILayout.Label($"trackName: {tmpTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【{_correctTrackOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【ステージ光源】");
                GUILayout.Label($"トラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【ステージ光源】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckCharacterLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                if(invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【キャラ光源】 \n 不要なトラックが含まれています。");
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【ステージ光源】");
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【ステージ光源】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckStageTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                if(invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【ステージ】 \n 不要なトラックが含まれています。");
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【ステージ】");
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【ステージ】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckEffectTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                if(invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【エフェクト】 \n 不要なトラックが含まれています。");
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【エフェクト】");
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【エフェクト】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckMusicTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                if(invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【音声】 \n 不要なトラックが含まれています。");
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【音声】");
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【音声】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckMovieTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                if(invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【動画】 \n 不要なトラックが含まれています。");
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【動画】");
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【動画】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckMobAvatorTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                if(invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【モブ】 \n 不要なトラックが含まれています。");
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【モブ】");
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【モブ】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckCallAndResponseTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if(childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if(trackList.Count > 0 && trackList != null)
            {
                if(invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【コーレス】 \n 不要なトラックが含まれています。");
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【コーレス】");
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if(invalidTracks.Count > 0)
                {
                    foreach(TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"・不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【コーレス】");
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

}