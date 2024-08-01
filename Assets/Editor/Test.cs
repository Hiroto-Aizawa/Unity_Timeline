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
        //二次元のリストを作成する？
        "NeonLight",
        "LED_Light",
        "Moving_Light",
    };
    #endregion

    #region _neonOrder Neon_Lightの正しい順番を記載したリスト
    private List<string> _neonOrder = new List<string>()
    {
        "Neon_A",
        "Neon_B",
        "Neon_C",
        "Neon_D",
        "Neon_E",
        "Neon_F",
        "Neon_G"
    };
    #endregion

    #region _ledOrder LED_Lightの正しい順番を記載したリスト
    private List<string> _ledOrder = new List<string>()
    {
        "LED_A_Meta",
        "LED_B_Meta",
        "LED_C_Meta",
    };
    #endregion

    #region _movingLight MovingLightの正しい順番を記載したリスト
    private List<string> _movingLight = new List<string>()
    {
        "Stage",
        "Base_A_Meta",
        "Base_B_Meta",
        "Base_C_Meta",
        "Base_D_Meta",
        "Base_E_Meta",
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

            if (regularTracks.Count > 0 && regularTracks != null)
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
                int i = 0;
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

            if (childTracks.Count == _stageOrder.Count && regularTracks.Count == _stageOrder.Count)
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
                    i++;
                }
            }
            else
            {
                GUILayout.Label($"【ステージ】");
                GUILayout.Space(20);

                if (childTracks.Count > _stageOrder.Count)
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

}