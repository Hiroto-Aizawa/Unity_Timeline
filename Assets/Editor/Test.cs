using System;
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

    #region _groupTrackOrder グループトラックの正しい順番を記載したリスト
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
    #endregion

    #region _stageLightOrder ステージ光源の正しい順番を記載したリスト
    private List<string> _stageLightOrder = new List<string>()
    {
        "StageAmbientLight",
        "StagePointLight0",
        "StagePointLight1",
        "StagePointLight2",
        "FlareLight",
    };
    #endregion

    #region _characterLightOrder キャラ光源の正しい順番を記載したリスト
    private List<string> _characterLightOrder = new List<string>()
    {
        // 歌唱キャラによって要素数も番号も不規則になる...
        // CharacterRimLightは最後に固定されている
        "CharacterAmbientLight0",
        "CharacterAmbientLight1",
        "CharacterAmbientLight2",
        "CharacterAmbientLight3",
        "CharacterAmbientLight4",
        "CharacterAmbientLight5",
        "CharacterAmbientLight6",
        "CharacterAmbientLight7",
        "CharacterAmbientLight8",
        "CharacterAmbientLight9",
        "CharacterRimLight",
    };
    #endregion

    #region _stageOrder ステージの正しい順番を記載したリスト
    private List<string> _stageOrder = new List<string>()
    {
        // 最低この3つはある？
        "Neon_Light",
        "LED_Light",
        "Moving_Light",
        // ない場合もある
        "Decoration"
    };
    #endregion

    #region _neonOrder Neon_Lightの正しい順番を記載したリスト
    private List<string> _neonLightOrder = new List<string>()
    {
        "Neon_A",
        "Neon_B",
        "Neon_C",
        // ステージ反対側の2Fにあがる階段についているネオン
        // 演出をつける必要がないのでそもそもTimelineに配置してないトラック 
        /*"Neon_D",*/
        "Neon_E",
        "Neon_F",
        "Neon_G"
    };
    #endregion

    #region _ledOrder LED_Lightの正しい順番を記載したリスト
    private List<string> _ledLightOrder = new List<string>()
    {
        "LED_A",
        "LED_B",
        "LED_C",
    };
    #endregion

    #region _movingLight MovingLightの正しい順番を記載したリスト
    private List<string> _movingLightOrder = new List<string>()
    {
        "Stage",
        "Base_A",
        "Base_B",
        "Base_C",
        "Base_D",
        "Base_E",
        "Material",
    };
    #endregion

    #region _effectOrder エフェクトの正しい順番を記載したリスト
    private List<string> _effectOrder = new List<string>()
    {
        "通常エフェクト",
        "キャラ追従エフェクト",
        "ピンスポ",
    };
    #endregion

    #region _soundOrder 音声の正しい順番を記載したリスト
    private List<string> _soundOrder = new List<string>()
    {
        "Music",
        "Audience",
        "SE",
    };
    #endregion

    #region _movieOrderd 動画の正しい順番を記載したリスト
    private List<string> _movieOrder = new List<string>()
    {
        "Movie0",
        "Movie1",
        "Movie2",
    };
    #endregion

    #region _mobOrder モブの正しい順番を記載したリスト
    private List<string> _mobOrder = new List<string>()
    {
        // トラック名はMobAvatarだが、これはカスタムトラックのため複数あると名前だけで判別できない
        // そのためトラックのタイプを見て判別可能にする 例 : MobAvatar(MobAvatarMotionTrack)
        "MobAvatarMotionTrack",
        "MobAvatarColorTrack",
    };
    #endregion

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
                                    CheckCharacterLightTrack(groupTrack);
                                    break;
                                case "ステージ":
                                    CheckStageTrack(groupTrack);
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
            List<TrackAsset> regularTracks = childTracks.Where(e => e.name == "GlobalSettings").ToList();

            List<TrackAsset> irregularTracks = childTracks.Where(e => e.name != "GlobalSettings").ToList();

            if (regularTracks != null)
            {
                GUILayout.Label($"【全体設定】");
                GUILayout.Space(20);

                if (irregularTracks.Count > 0)
                {
                    GUILayout.Label($"不要なトラックが含まれています。");

                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
            }
            else
            {
                GUILayout.Label($"【全体設定】");
                GUILayout.Space(20);
                GUILayout.Label($"GlobalSettingsトラックがありません", style);
                GUILayout.Label($"トラックの作成をしてください");
                GUILayout.Space(20);

                if (irregularTracks.Count > 0)
                {
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($"トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【全体設定】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
            GUILayout.Space(20);
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
            List<TrackAsset> regularTracks = childTracks.Where(e => _stageLightOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _stageLightOrder.Count && regularTracks.Count == _stageLightOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【ステージ光源】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
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

                    // 差集合を求める
                    var requiredTracks = _stageLightOrder.Except(childNameList);

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
            GUILayout.Space(20);
        }
    }

    private void CheckCharacterLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _characterLightOrder.Contains(e.name)).ToList();

            // 最小のトラック数
            // 歌唱キャラ5人分のAmbientLight + RimLight = 6トラック
            int minTrackCount = 6;

            // irregularTracksがあるかどうかを最初に見る
            // 無ければ、regularTracsの処理に入る
            // 仕様を満たすトラック数が最低限あり、
            if (regularTracks.Count >= minTrackCount && regularTracks.Count == childTracks.Count)
            {
                GUILayout.Label($"【キャラ光源】");
                GUILayout.Space(20);

                // _charactreLightOrderの最後の要素（CharacterRimLight）がリスト内に存在するか
                if (regularTracks.Exists(e => e.name == _characterLightOrder.Last()))
                {
                    // 最後のトラックが_charactreLightOrderの最後の要素（CharacterRimLight）一致しているか
                    if (regularTracks.Last().name != _characterLightOrder.Last())
                    {
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【CharacterRimLight】を配置してください");
                        GUILayout.Space(20);
                    }
                }
                else
                {
                    GUILayout.Label($"足りないトラックがあります。");
                    GUILayout.Label($"└トラック名: {_characterLightOrder.Last()}", style);
                    GUILayout.Space(20);
                }

            }
            else
            {
                GUILayout.Label($"【キャラ光源】");
                GUILayout.Space(20);

                // 仕様に沿っていないトラックを抽出
                var irregularTracks = childTracks.Where(e => !_characterLightOrder.Contains(e.name)).ToList();

                if (irregularTracks.Count > 0)
                {
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                        GUILayout.Space(20);
                    }
                }
                else
                {
                    if (regularTracks.Count < minTrackCount)
                    {
                        GUILayout.Label("トラック数が足りません");
                        GUILayout.Label("最低でも【CharacterAmbientLight】5つと \n【CharacterRimLight】を配置してください", style);
                        GUILayout.Space(20);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label($"【キャラ光源】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
            GUILayout.Space(20);
        }
    }

    private void CheckStageTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _stageOrder.Contains(e.name)).ToList();


            // 最小のトラック数
            // Neon、LED、Movingの3つのライトは必須、楽曲によってはDecorationトラックが入る
            int minTrackCount = 2;

            if (regularTracks.Count >= minTrackCount && regularTracks.Count == childTracks.Count)

            //if(childTracks.Count == _stageOrder.Count && regularTracks.Count == _stageOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【ステージ】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _stageOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【 {_stageOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    else
                    {
                        //TrackGroupの子トラックリストの順番が合っているかを確認する
                        switch (childTrack.name)
                        {
                            case "Neon_Light":
                                CheckNeonLightTrack(childTrack);
                                break;
                            case "LED_Light":
                                CheckLedLightTrack(childTrack);
                                break;
                            case "Moving_Light":
                                CheckMovingLightTrack(childTrack);
                                break;
                            default:
                                break;
                        }
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【ステージ】");
                GUILayout.Space(20);
                // regularTracks.Count >= childTracks.Count　regularTracks
                if (childTracks.Count > regularTracks.Count)//
                {
                    var irregularTracks = childTracks.Where(e => !_stageOrder.Contains(e.name)).ToList();

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

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = _stageOrder.Except(childNameList);

                    //トラック4個でDecoration以外があるとDecorationが足りないトラック判定される
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
            GUILayout.Label($"【ステージ】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
            GUILayout.Space(20);
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
            List<TrackAsset> regularTracks = childTracks.Where(e => _effectOrder.Contains(e.name)).ToList();

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

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = _effectOrder.Except(childNameList);

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
            GUILayout.Space(20);
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

                    // 差集合を求める
                    var requiredTracks = _soundOrder.Except(childNameList);

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
            GUILayout.Space(20);
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

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = _movieOrder.Except(childNameList);

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
            GUILayout.Space(20);
        }
    }

    private void CheckMobAvatorTrack(TrackAsset track)
    {
        List<TrackAsset> tmpTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (tmpTracks != null)
        {
            // トラック名はMobAvatarだが、これはカスタムトラックのため複数あると名前だけで判別できない
            // そのためトラックのタイプを見て判別可能にする 例 : MobAvatar(MobAvatarMotionTrack)
            List<string> childTracks = tmpTracks.Select(e => e.GetType().Name).ToList();// as List<string>;

            // childTracksから仕様に合っているトラックだけを抽出
            List<string> regularTracks = childTracks.Where(e => _mobOrder.Contains(e)).ToList();

            if (childTracks.Count == _mobOrder.Count && regularTracks.Count == _mobOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【モブ】");
                GUILayout.Space(20);

                foreach (string childTrack in childTracks)
                {
                    if (childTrack != _mobOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack}");
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
                    var irregularTracks = childTracks.Where(e => !_mobOrder.Contains(e)).ToList();

                    foreach (string irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack}", style);
                        GUILayout.Space(20);
                    }
                }
                else
                {
                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = _mobOrder.Except(childTracks);

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
            GUILayout.Space(20);
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
            List<TrackAsset> regularTracks = childTracks.Where(e => e.name == "ShoutTime").ToList();

            // 仕様にないトラックがあればリストに格納する
            List<TrackAsset> irregularTracks = childTracks.Where(e => e.name != "ShoutTime").ToList();

            if (regularTracks.Count > 0 && regularTracks != null)
            {
                GUILayout.Label($"【コーレス】");
                GUILayout.Space(20);

                if (irregularTracks.Count > 0)
                {
                    GUILayout.Label($"不要なトラックが含まれています。");
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"トラック名: {irregularTrack.name}", style);
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
                GUILayout.Space(20);

                if (irregularTracks.Count > 0)
                {
                    foreach (TrackAsset irregularTrack in irregularTracks)
                    {
                        GUILayout.Label($"不要なトラックが含まれています。");
                        GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
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
            GUILayout.Space(20);
        }
    }

    private void CheckNeonLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _neonLightOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _neonLightOrder.Count && regularTracks.Count == _neonLightOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【ステージ】\n └【Neon_Light】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _neonLightOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【 {_neonLightOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    else
                    {
                        CheckNeonChildGroupTrack(childTrack);
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【ステージ】\n └【Neon_Light】");
                GUILayout.Space(20);

                if (childTracks.Count > _neonLightOrder.Count)
                {
                    var irregularTracks = childTracks.Where(e => !_neonLightOrder.Contains(e.name)).ToList();

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

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = _neonLightOrder.Except(childNameList);

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
            GUILayout.Label($"【ステージ】\n └【Neon_Light】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
            GUILayout.Space(20);
        }
    }

    private void CheckNeonChildGroupTrack(TrackAsset track)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        switch (track.name)
        {
            case "Neon_A":

                GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_A】");
                GUILayout.Space(20);

                // ステージ > Neon_Light > Neon_Aグループトラックを取得する
                List<TrackAsset> neonAChildTracks = track.GetChildTracks() as List<TrackAsset>;

                // irregularなトラックをリストに格納して、foreachでメッセージを表示する
                // List<TrackAsset> neonAChildTracks = track.GetChildTracks().Where(e => !e.name.Contains("Neon_A_All_Meta")) as List<TrackAsset>;

                if (neonAChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in neonAChildTracks)
                    {
                        if (grandChildTrack.name != "Neon_A_All_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_A】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }


                break;
            case "Neon_B":

                GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_B】");
                GUILayout.Space(20);

                // ステージ > Neon_Light > Neon_Bグループトラックを取得する
                List<TrackAsset> neonBChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (neonBChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in neonBChildTracks)
                    {
                        if (grandChildTrack.name != "Neon_B_All_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_B】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Neon_C":

                GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_C】");
                GUILayout.Space(20);

                // ステージ > Neon_Light > Neon_Cグループトラックを取得する
                List<TrackAsset> neonCChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (neonCChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in neonCChildTracks)
                    {
                        if (grandChildTrack.name != "Neon_C_All_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_C】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Neon_E":

                GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_E】");
                GUILayout.Space(20);

                // ステージ > Neon_Light > Neon_Eグループトラックを取得する
                List<TrackAsset> neonDChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (neonDChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in neonDChildTracks)
                    {
                        if (grandChildTrack.name != "Neon_E_All_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_E】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Neon_F":

                GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_F】");
                GUILayout.Space(20);

                //ステージトラックの孫にあたるトラックグループを取得する
                List<TrackAsset> neonFChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (neonFChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in neonFChildTracks)
                    {
                        if (grandChildTrack.name != "Neon_F_All_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_F】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Neon_G":

                GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_G】");
                GUILayout.Space(20);

                // ステージ > Neon_Light > Neon_Eグループトラックを取得する
                List<TrackAsset> neonGChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (neonGChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in neonGChildTracks)
                    {
                        if (grandChildTrack.name != "Neon_G_All_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Neon_Light】\n      └【Neon_F】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            default:
                break;
        }
    }

    private void CheckLedLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _ledLightOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _ledLightOrder.Count && regularTracks.Count == _ledLightOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【ステージ】\n └【LED_Light】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _ledLightOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【 {_ledLightOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    else
                    {
                        CheckLedChildGroupTrack(childTrack);
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【ステージ】\n └【LED_Light】");
                GUILayout.Space(20);

                if (childTracks.Count > _ledLightOrder.Count)
                {
                    var irregularTracks = childTracks.Where(e => !_ledLightOrder.Contains(e.name)).ToList();

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

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = _ledLightOrder.Except(childNameList);

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
            GUILayout.Label($"【ステージ】\n └【LED_Light】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
            GUILayout.Space(20);
        }
    }

    private void CheckLedChildGroupTrack(TrackAsset track)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        switch (track.name)
        {
            case "LED_A":

                GUILayout.Label($"【ステージ】\n └【LED_Light】\n      └【LED_A】");
                GUILayout.Space(20);

                // ステージ > LED_Light > LED_Aグループトラックを取得する
                List<TrackAsset> ledAChildTracks = track.GetChildTracks() as List<TrackAsset>;

                // irregularなトラックをリストに格納して、foreachでメッセージを表示する
                // List<TrackAsset> ledAChildTracks = track.GetChildTracks().Where(e => !e.name.Contains("LED_A_All_Meta")) as List<TrackAsset>;

                if (ledAChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in ledAChildTracks)
                    {
                        if (grandChildTrack.name != "LED_A_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【LED_Light】\n      └【LED_A】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }


                break;
            case "LED_B":

                GUILayout.Label($"【ステージ】\n └【LED_Light】\n      └【LED_B】");
                GUILayout.Space(20);

                // ステージ > LED_Light > Neon_Bグループトラックを取得する
                List<TrackAsset> ledBChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (ledBChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in ledBChildTracks)
                    {
                        if (grandChildTrack.name != "LED_B_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【LED_Light】\n      └【LED_B】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "LED_C":

                GUILayout.Label($"【ステージ】\n └【LED_Light】\n      └【LED_C】");
                GUILayout.Space(20);

                // ステージ > LED_Light > Neon_Cグループトラックを取得する
                List<TrackAsset> ledCChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (ledCChildTracks != null)
                {
                    foreach (TrackAsset grandChildTrack in ledCChildTracks)
                    {
                        if (grandChildTrack.name != "LED_C_Meta")
                        {
                            GUILayout.Label($"不要なトラックが含まれています。");
                            GUILayout.Label($" └トラック名: {grandChildTrack.name}", style);
                            GUILayout.Space(20);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【LED_Light】\n      └【LED_C】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            default:
                break;
        }
    }

    private void CheckMovingLightTrack(TrackAsset track)
    {
        List<TrackAsset> childTracks = track.GetChildTracks() as List<TrackAsset>;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        if (childTracks != null)
        {
            // childTracksから仕様に合っているトラックだけを抽出
            List<TrackAsset> regularTracks = childTracks.Where(e => _movingLightOrder.Contains(e.name)).ToList();

            if (childTracks.Count == _movingLightOrder.Count && regularTracks.Count == _movingLightOrder.Count)
            {
                int i = 0;
                GUILayout.Label($"【ステージ】\n └【Moving_Light】");
                GUILayout.Space(20);

                foreach (TrackAsset childTrack in childTracks)
                {
                    if (childTrack.name != _movingLightOrder[i])
                    {
                        GUILayout.Label($"trackName: {childTrack.name}");
                        GUILayout.Label($"トラックの順番が間違っています", style);
                        GUILayout.Label($"ここには、【 {_movingLightOrder[i]}】を配置してください");
                        GUILayout.Space(20);
                    }
                    else
                    {
                        CheckMovingChildGroupTrack(childTrack);
                    }
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【ステージ】\n └【Moving_Light】");
                GUILayout.Space(20);

                if (childTracks.Count > _movingLightOrder.Count)
                {
                    var irregularTracks = childTracks.Where(e => !_movingLightOrder.Contains(e.name)).ToList();

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

                    // 差集合(足りないトラック)を求める
                    // 仕様通りのトラックリスト - 現在のトラックリスト = 足りないトラックのリスト
                    var requiredTracks = _movingLightOrder.Except(childNameList);

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
            GUILayout.Label($"【ステージ】\n └【Moving_Light】");
            GUILayout.Space(20);
            GUILayout.Label($"トラックグループ内にトラックがありません", style);
            GUILayout.Space(20);
        }
    }

    private void CheckMovingChildGroupTrack(TrackAsset track)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;

        switch (track.name)
        {
            case "Stage":
                //グループではないので特に処理は不要
                break;
            case "Base_A":

                GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_A】");
                GUILayout.Space(20);

                // ステージ > Moving_Light > Base_Aグループトラックを取得する
                List<TrackAsset> baseAChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (baseAChildTracks != null)
                {
                    List<string> baseAOrder = new List<string>()
                    {
                        "Stage", "Base_A_Meta", "Base_A_SpotLight",
                    };

                    List<TrackAsset> baseARegularTracks = baseAChildTracks.Where(e => baseAOrder.Contains(e.name)).ToList();
                    //groupTracks = groupTracks.Where(e => _groupTrackOrder.Contains(e.name)).ToList();

                    if (baseARegularTracks.Count >= baseAOrder.Count && baseARegularTracks.Count == baseAChildTracks.Count)
                    {
                        if (baseARegularTracks.First().name != baseAOrder.First())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Stage】は1番目に配置してください");
                            GUILayout.Space(20);
                        }

                        if (baseARegularTracks.Last().name != baseAOrder.Last())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Base_A_SpotLight】最後に配置してください");
                            GUILayout.Space(20);
                        }
                    }
                    else
                    {
                        var baseAIrregularTracks = baseAChildTracks.Where(e => !baseAOrder.Contains(e.name)).ToList();

                        if (baseAIrregularTracks.Count > 0)
                        {
                            foreach (TrackAsset irregularTrack in baseAIrregularTracks)
                            {
                                GUILayout.Label($"不要なトラックが含まれています。");
                                GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                                GUILayout.Space(20);
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_A】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Base_B":

                GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_B】");
                GUILayout.Space(20);

                // ステージ > Moving_Light > Base_Bグループトラックを取得する
                List<TrackAsset> baseBChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (baseBChildTracks != null)
                {
                    List<string> baseBOrder = new List<string>()
                    {
                        "Stage", "Base_B_Meta", "Base_B_SpotLight",
                    };

                    List<TrackAsset> baseBRegularTracks = baseBChildTracks.Where(e => baseBOrder.Contains(e.name)).ToList();
                    //groupTracks = groupTracks.Where(e => _groupTrackOrder.Contains(e.name)).ToList();

                    if (baseBRegularTracks.Count >= baseBOrder.Count && baseBRegularTracks.Count == baseBChildTracks.Count)
                    {
                        if (baseBRegularTracks.First().name != baseBOrder.First())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Stage】は1番目に配置してください");
                            GUILayout.Space(20);
                        }

                        if (baseBRegularTracks.Last().name != baseBOrder.Last())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Base_B_SpotLight】最後に配置してください");
                            GUILayout.Space(20);
                        }
                    }
                    else
                    {
                        var baseBIrregularTracks = baseBChildTracks.Where(e => !baseBOrder.Contains(e.name)).ToList();

                        if (baseBIrregularTracks.Count > 0)
                        {
                            foreach (TrackAsset irregularTrack in baseBIrregularTracks)
                            {
                                GUILayout.Label($"不要なトラックが含まれています。");
                                GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                                GUILayout.Space(20);
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_B】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Base_C":

                GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_C】");
                GUILayout.Space(20);

                // ステージ > Moving_Light > Base_Cグループトラックを取得する
                List<TrackAsset> baseCChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (baseCChildTracks != null)
                {
                    List<string> baseCOrder = new List<string>()
                    {
                        "Stage", "Base_C_Meta", "Base_C_SpotLight",
                    };

                    List<TrackAsset> baseCRegularTracks = baseCChildTracks.Where(e => baseCOrder.Contains(e.name)).ToList();
                    //groupTracks = groupTracks.Where(e => _groupTrackOrder.Contains(e.name)).ToList();

                    if (baseCRegularTracks.Count >= baseCOrder.Count && baseCRegularTracks.Count == baseCChildTracks.Count)
                    {
                        if (baseCRegularTracks.First().name != baseCOrder.First())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Stage】は1番目に配置してください");
                            GUILayout.Space(20);
                        }

                        if (baseCRegularTracks.Last().name != baseCOrder.Last())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Base_C_SpotLight】最後に配置してください");
                            GUILayout.Space(20);
                        }
                    }
                    else
                    {
                        var baseCIrregularTracks = baseCChildTracks.Where(e => !baseCOrder.Contains(e.name)).ToList();

                        if (baseCIrregularTracks.Count > 0)
                        {
                            foreach (TrackAsset irregularTrack in baseCIrregularTracks)
                            {
                                GUILayout.Label($"不要なトラックが含まれています。");
                                GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                                GUILayout.Space(20);
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_C】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Base_D":

                GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_D】");
                GUILayout.Space(20);

                // ステージ > Moving_Light > Base_Dグループトラックを取得する
                List<TrackAsset> baseDChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (baseDChildTracks != null)
                {
                    List<string> baseCOrder = new List<string>()
                    {
                        "Stage", "Base_D_Meta", "Base_D_SpotLight",
                    };

                    List<TrackAsset> baseDRegularTracks = baseDChildTracks.Where(e => baseCOrder.Contains(e.name)).ToList();
                    //groupTracks = groupTracks.Where(e => _groupTrackOrder.Contains(e.name)).ToList();

                    if (baseDRegularTracks.Count >= baseCOrder.Count && baseDRegularTracks.Count == baseDChildTracks.Count)
                    {
                        if (baseDRegularTracks.First().name != baseCOrder.First())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Stage】は1番目に配置してください");
                            GUILayout.Space(20);
                        }

                        if (baseDRegularTracks.Last().name != baseCOrder.Last())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Base_D_SpotLight】最後に配置してください");
                            GUILayout.Space(20);
                        }
                    }
                    else
                    {
                        var baseDIrregularTracks = baseDChildTracks.Where(e => !baseCOrder.Contains(e.name)).ToList();

                        if (baseDIrregularTracks.Count > 0)
                        {
                            foreach (TrackAsset irregularTrack in baseDIrregularTracks)
                            {
                                GUILayout.Label($"不要なトラックが含まれています。");
                                GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                                GUILayout.Space(20);
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_D】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;
            case "Base_E":

                GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_E】");
                GUILayout.Space(20);

                // ステージ > Moving_Light > Base_Eグループトラックを取得する
                List<TrackAsset> baseEChildTracks = track.GetChildTracks() as List<TrackAsset>;

                if (baseEChildTracks != null)
                {
                    List<string> baseCOrder = new List<string>()
                    {
                        "Stage", "Base_E_Meta", "Base_E_SpotLight",
                    };

                    List<TrackAsset> baseERegularTracks = baseEChildTracks.Where(e => baseCOrder.Contains(e.name)).ToList();
                    //groupTracks = groupTracks.Where(e => _groupTrackOrder.Contains(e.name)).ToList();

                    if (baseERegularTracks.Count >= baseCOrder.Count && baseERegularTracks.Count == baseEChildTracks.Count)
                    {
                        if (baseERegularTracks.First().name != baseCOrder.First())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Stage】は1番目に配置してください");
                            GUILayout.Space(20);
                        }

                        if (baseERegularTracks.Last().name != baseCOrder.Last())
                        {
                            GUILayout.Label($"トラックの順番が間違っています", style);
                            GUILayout.Label($"【Base_E_SpotLight】最後に配置してください");
                            GUILayout.Space(20);
                        }
                    }
                    else
                    {
                        var baseEIrregularTracks = baseEChildTracks.Where(e => !baseCOrder.Contains(e.name)).ToList();

                        if (baseEIrregularTracks.Count > 0)
                        {
                            foreach (TrackAsset irregularTrack in baseEIrregularTracks)
                            {
                                GUILayout.Label($"不要なトラックが含まれています。");
                                GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                                GUILayout.Space(20);
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Base_E】");
                    GUILayout.Space(20);
                    GUILayout.Label($"トラックグループ内にトラックがありません", style);
                    GUILayout.Space(20);
                }

                break;

            case "Material":

                GUILayout.Label($"【ステージ】\n └【Moving_Light】\n      └【Material】");
                GUILayout.Space(20);

                // ステージ > Moving_Light > Materialグループトラックを取得する
                List<TrackAsset> materialTracks = track.GetChildTracks() as List<TrackAsset>;

                //List<TrackAsset> regularTracks = materialTracks.Where(e => e.name == "Stage").ToList();

                if (materialTracks != null)
                {
                    List<TrackAsset> regularTracks = materialTracks.Where(e => e.name == "Stage").ToList();

                    var materialIrregularTracks = materialTracks.Where(e => e.name != "Stage").ToList();

                    if (regularTracks != null)
                    {
                        if (materialIrregularTracks.Count > 0)
                        {
                            foreach (TrackAsset irregularTrack in materialIrregularTracks)
                            {
                                GUILayout.Label($"不要なトラックが含まれています。");
                                GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                                GUILayout.Space(20);
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label($"Stageトラックがありません", style);
                        GUILayout.Label($"トラックの作成をしてください");
                        GUILayout.Space(20);

                        if (materialIrregularTracks.Count > 0)
                        {
                            foreach (TrackAsset irregularTrack in materialIrregularTracks)
                            {
                                GUILayout.Label($"不要なトラックが含まれています。");
                                GUILayout.Label($" └トラック名: {irregularTrack.name}", style);
                                GUILayout.Space(20);
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"GlobalSettingsトラックがありません", style);
                    GUILayout.Label($"トラックの作成をしてください");
                    GUILayout.Space(20);
                }


                break;
            default:
                break;
        }
    }
}