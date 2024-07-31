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
    private List<string> _groupTrackOrder = new List<string>()
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
    private List<string> _stageLightOrder = new List<string>()
    {
        "StageAmbientLight",
        "StagePointLight0",
        "StagePointLight1",
        "StagePointLight2",
        "FlareLight",
    };

    private List<string> _characterLightOrder = new List<string>()
    {
        // 歌唱キャラによって要素数も番号も不規則になる...
        // CharacterRimLightは最後に固定されている
        "CharacterAmbientLight0",
        "CharacterAmbientLight1",
        "CharacterAmbientLight2",
        "CharacterAmbientLight3",
        "CharacterAmbientLight5",
        "CharacterRimLight",
    };

    private List<List<string>> _stageOrder = new List<List<string>>()
    {
        //二次元のリストを作成する？
        /*"NeonLight",
        "LED_Light",
        "Moving_Light",*/
    };

    private List<string> _effectOrder = new List<string>()
    {
        "通常エフェクト",
        "キャラ追従エフェクト",
        "ピンスポ",
    };

    private List<string> _soundOrder = new List<string>()
    {
        "Music",
        "Audience",
        "SE",
    };

    private List<string> _movieOrder = new List<string>()
    {
        "Movie0",
        "Movie1",
        "Movie2",
    };

    /// <summary>
    /// モブトラックグループの配置順
    /// </summary>
    private List<string> _mobOrder = new List<string>()
    {
        "MobAvatar",
        "MobAvatar",
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

        if (_timelineObj != null)
        {
            PlayableDirector timelineDirector = _timelineObj.GetComponent<PlayableDirector>();
            TimelineAsset timelineAsset = timelineDirector.playableAsset as TimelineAsset;

            //Timeline上のトラック数を返す
            _trackCount = timelineAsset.outputTrackCount;

            GUILayout.Label($"トラック数: {_trackCount}");
            GUILayout.Space(20);

            if (_trackCount > 0)
            {
                // ルートトラックの中でもGroupTrackのみをList<TrackAsset>にキャストして取得する
                //List<TrackAsset> groupTracks = timelineAsset.GetRootTracks().OfType<GroupTrack>() as List<TrackAsset>;
                List<TrackAsset> groupTracks = timelineAsset.GetRootTracks() as List<TrackAsset>;

                if (groupTracks.Count > 0)
                {
                    groupTracks = groupTracks.Where(e => _groupTrackOrder.Contains(e.name)).ToList();
                    int i = 0;

                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red;

                    // groupTracksリストのGroupTrackである要素だけを取り出す
                    foreach (TrackAsset groupTrack in groupTracks)
                    {
                        //GroupTrackがないTimelineで要検証
                        if (groupTrack.name != _groupTrackOrder[i])
                        {
                            GUILayout.Label($"trackName: {groupTrack.name}");
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"ここには、【{_groupTrackOrder[i]}】を配置してください");
                            GUILayout.Space(20);
                        }
                        else
                        {
                            switch (groupTrack.name)
                            {
                                case "全体設定":
                                    CheckGlobalSettingsTrack(groupTrack);
                                    break;
                                case "ステージ光源":
                                    CheckStageLightTrack(groupTrack);
                                    break;
                                case "キャラ光源":
                                    break;
                                case "ステージ":
                                    break;
                                case "エフェクト":
                                    CheckEffectTrack(groupTrack);
                                    break;
                                case "音声":
                                    CheckMusicTrack(groupTrack);
                                    break;
                                case "動画":
                                    CheckMovieTrack(groupTrack);
                                    break;
                                case "モブ":
                                    CheckMobAvatorTrack(groupTrack);
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

                    //明示的な変換（キャスト）(List<TrackAsset>)
                    /*var trackList = groupTracks.Select(groupTrack =>
                        new { Name = groupTrack.name, Index = ToIndex(groupTrack) }).
                            OrderBy(groupTrack => groupTrack.Index).Select(groupTrack => groupTrack.Name);*/

                }
            }
            else
            {
                GUILayout.Label("トラックが存在しないTimelineです。");
            }
        }

        EditorGUILayout.EndScrollView();
    }

    /* public int ToIndex(TrackAsset track)
     {
         return 0;
     }*/

    private void CheckGlobalSettingsTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "GlobalSettings").ToList();

            if (trackList.Count > 0)
            {
                List<TrackAsset> tmpTracks = childTracks.Where(e => e.name != "GlobalSettings").ToList();

                if (tmpTracks.Count > 0)
                {
                    GUILayout.Label($"【全体設定】");
                    GUILayout.Space(20);
                    foreach (TrackAsset tmpTrack in tmpTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
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
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckStageLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _soundOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _stageLightOrder.Count && regularTracks.Count == _stageLightOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【ステージ光源】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    //Debug.Log($"correctTrackName: {childTrack.name} || order: {_stageLightOrder[i]}");
                    if (childTrack.name != _stageLightOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【{_stageLightOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    i++;
                }
            }
            else
            {
                //トラック数に過不足がある
                GUILayout.Label($"【ステージ光源】");
                GUILayout.Space(20);

                if (childTracks.Count > _stageLightOrder.Count)
                {
                    //_stageLightOrderに含まれないトラックのリストを作成する
                    var irregularTracks = childTracks.Where(e => !_stageLightOrder.Contains(e.name)).ToList();
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
                else
                {
                    List<string> childNameList = childTracks.Select(e => e.name).ToList();
                    List<string> correctNameList = _stageLightOrder;

                    // 差集合を求める
                    var requiredTracks = correctNameList.Except(childNameList);
                    foreach (string requiredTrack in requiredTracks)
                    {
                        GUILayout.Label($"足りないトラックがあります。");
                        GUILayout.Label($" └トラック名: {requiredTrack}", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【ステージ光源】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckCharacterLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if (trackList.Count > 0 && trackList != null)
            {
                if (invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【キャラ光源】 \n 不要なトラックが含まれています。");
                    foreach (TrackAsset tmpTrack in invalidTracks)
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

                if (invalidTracks.Count > 0)
                {
                    foreach (TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【ステージ光源】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckStageTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if (trackList.Count > 0 && trackList != null)
            {
                if (invalidTracks.Count > 0)
                {
                    GUILayout.Label($"【ステージ】 \n 不要なトラックが含まれています。");
                    foreach (TrackAsset tmpTrack in invalidTracks)
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

                if (invalidTracks.Count > 0)
                {
                    foreach (TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【ステージ】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckEffectTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _soundOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _effectOrder.Count && regularTracks.Count == _effectOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【エフェクト】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _effectOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【 {_effectOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【エフェクト】");
                GUILayout.Space(20);

                if (childTracks.Count > _effectOrder.Count)
                {
                    var irregularTracks = childTracks.Where(e => !_effectOrder.Contains(e.name)).ToList();
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
                else
                {

                    List<string> childNameList = childTracks.Select(e => e.name).ToList();
                    List<string> correctNameList = _effectOrder;

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = correctNameList.Except(childNameList);
                    foreach (string requiredTrack in requiredTracks)
                    {
                        GUILayout.Label($"足りないトラックがあります。");
                        GUILayout.Label($"└トラック名: {requiredTrack}", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【エフェクト】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckMusicTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _soundOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _soundOrder.Count && regularTracks.Count == _soundOrder.Count)
            {
                int i = 0;

                GUILayout.Label($"【音声】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _soundOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【{_soundOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    i++;
                }
            }
            else
            {
                //トラック数に過不足がある
                GUILayout.Label($"【音声】");
                GUILayout.Space(20);

                if (childTracks.Count > _soundOrder.Count)
                {
                    //_soundOrderに含まれないトラックのリストを作成する
                    var irregularTracks = childTracks.Where(e => !_soundOrder.Contains(e.name)).ToList();
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
                else
                {
                    List<string> childNameList = childTracks.Select(e => e.name).ToList();
                    List<string> correctNameList = _soundOrder;

                    // 差集合を求める
                    var requiredTracks = correctNameList.Except(childNameList);
                    foreach (string requiredTrack in requiredTracks)
                    {
                        GUILayout.Label($"足りないトラックがあります。");
                        GUILayout.Label($" └トラック名: {requiredTrack}", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【音声】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckMovieTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _movieOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _movieOrder.Count && regularTracks.Count == _movieOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【動画】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _movieOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【 {_movieOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【動画】");
                GUILayout.Space(20);

                if (childTracks.Count > _movieOrder.Count)
                {
                    var irregularTracks = childTracks.Where(e => !_movieOrder.Contains(e.name)).ToList();
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
                else
                {

                    List<string> childNameList = childTracks.Select(e => e.name).ToList();
                    List<string> correctNameList = _movieOrder;

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = correctNameList.Except(childNameList);
                    foreach (string requiredTrack in requiredTracks)
                    {
                        GUILayout.Label($"足りないトラックがあります。");
                        GUILayout.Label($"└トラック名: {requiredTrack}", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【動画】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckMobAvatorTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _mobOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _mobOrder.Count && regularTracks.Count == _mobOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【モブ】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _mobOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【 {_mobOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【モブ】");
                GUILayout.Space(20);

                if (childTracks.Count > _mobOrder.Count)
                {
                    var irregularTracks = childTracks.Where(e => !_mobOrder.Contains(e.name)).ToList();
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
                else
                {

                    List<string> childNameList = childTracks.Select(e => e.name).ToList();
                    List<string> correctNameList = _mobOrder;

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = correctNameList.Except(childNameList);
                    foreach (string requiredTrack in requiredTracks)
                    {
                        GUILayout.Label($"足りないトラックがあります。");
                        GUILayout.Label($"└トラック名: {requiredTrack}", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【モブ】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

    private void CheckCallAndResponseTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // 仕様にあるトラックがあればリストに格納する
            List<TrackAsset> trackList = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> invalidTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if (trackList.Count > 0 && trackList != null)
            {
                GUILayout.Label($"【コーレス");
                GUILayout.Space(20);

                if (invalidTracks.Count > 0)
                {
                    GUILayout.Label($"不要なトラックが含まれています。");
                    foreach (TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"トラック名: {tmpTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【コーレス】");
                GUILayout.Space(20);
                GUILayout.Label($"ShoutTimeトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");

                if (invalidTracks.Count > 0)
                {
                    foreach (TrackAsset tmpTrack in invalidTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {tmpTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【コーレス】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
        }
    }

}