using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEngine.Playables;

/// <summary>
/// 1つのクリップを基準に他のクリップの長さを調節する拡張エディタ
/// Description: BaseClipよりもclip.endが長いものをすべてBaseClipのclip.endと合わせる
/// </summary>
public class TimelineClipDurationAdjuster : EditorWindow
{
    private GameObject timelineObj;

    private static string baseClipName = "baseClip";

    private bool isChangeBaseClipName = false;

    [MenuItem("Tools/Adjust Timeline Clip Durations")]
    public static void ShowWindow()
    {
        GetWindow<TimelineClipDurationAdjuster>("Adjust Timeline Clip Durations");
    }

    private void OnGUI()
    {
        GUILayout.Space(20);
        timelineObj = EditorGUILayout.ObjectField("Playable Director", timelineObj, typeof(GameObject), true) as GameObject;
        GUILayout.Space(20);
        using (var inputGroup = new EditorGUILayout.ToggleGroupScope("基準となるクリップを変更する", isChangeBaseClipName))
        {
            isChangeBaseClipName = inputGroup.enabled;
            baseClipName = EditorGUILayout.TextField("Clip Name : ", baseClipName);
        }
        GUILayout.Space(20);



        if (timelineObj != null)
        {
            PlayableDirector timelineDirector = timelineObj.GetComponent<PlayableDirector>();
            TimelineAsset timelineAsset = timelineDirector.playableAsset as TimelineAsset;

            if (GUILayout.Button("Adjust Durations"))
            {
                AdjustClipDurations(timelineAsset);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("HierarchyビューからTimelineを持つGameObjectをアタッチしてください", MessageType.Error);
            return;
        }
    }

    // TimelineのクリップのDurationを調整する関数
    private static void AdjustClipDurations(TimelineAsset timeline)
    {
        // TimelineAssetを取得
        if (timeline == null)
        {
            Debug.LogError("Please select a Timeline Asset.");
            return;
        }

        TimelineClip baseClip = null;
        List<TimelineClip> clips;

        // Timelineのトラックを洗い出す
        foreach (var track in timeline.GetOutputTracks())
        {
            // 各トラックのクリップを洗い出す
            clips = new List<TimelineClip>(track.GetClips());
            // TimelineClip baseClip = null;

            // baseClipを見つける（ここでは名前で指定する）
            foreach (var clip in clips)
            {
                Debug.Log($"TrackName: {clip.displayName}");

                // ここでbaseClipの名前を指定する
                // 指定した名前を元にTimelineのclipを探す
                if (clip.displayName == baseClipName)
                {
                    Debug.LogWarning("Found base clip");
                    baseClip = clip;
                    Debug.LogWarning($"base clip type: {baseClip.GetType()} \n base clip: {baseClip.displayName}");
                    break;
                }
            }
        }

        // baseClipがあれば、それを基準に他クリップの長さを調整する
        if (baseClip != null)
        {
            // baseClipのDurationを取得
            double baseClipDuration = baseClip.duration;
            // baseClipのEndを取得
            double baseClipEnd = baseClip.end;

            foreach (var track in timeline.GetOutputTracks())
            {
                clips = new List<TimelineClip>(track.GetClips());
                // baseClipよりもDurationが長いクリップを調整
                foreach (var clip in clips)
                {
                    if (clip != baseClip && clip.duration > baseClipDuration)
                    {
                        // DurationをbaseClipと同じに変更
                        clip.duration = baseClipDuration;
                        Debug.Log($"Before Duration: {clip.duration} \n Clip End: {clip.end} \n Changed duration of {clip.displayName} to {baseClipDuration}");
                    }

                    // track内に複数クリップがあり、最後のクリップがbaseClipのendよりも後にあったら調整する
                    if (clip != baseClip && clip.end > baseClipEnd)
                    {
                        // baseClipEndとの差分を計算して、その長さ分をclipから引いて調整する
                        double diffDuration = clip.end - baseClipEnd;
                        clip.duration = clip.duration - diffDuration;

                    }
                }
            }
        }
        else
        {
            Debug.LogError("選択されたTimelineの中にbaseClipがありません");
        }
        EditorUtility.SetDirty(timeline);
    }
}
