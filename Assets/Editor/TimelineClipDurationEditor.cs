using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

public class TimelineClipDurationEditor : MonoBehaviour, EditorWindow
{
    private TimelineAsset timelineAsset;

    [MenuItem("Tools/Adjust Clip Durations")]
    public static void ShowWindow(){
        GetWindow<TimelineClipDurationEditor>("Adjust Clip Durations");
    }

    private void OnGUI()
    {
        GUILayout.Label("Adjust Clip Durations", EditorStyles.boldLable);

        timelineAsset = (TimelineAsset)EditorGUILayout.ObjectField("Timeline Asset", timelineAsset, typeof(TimelineAsset), false);

        if (timelineAsset != null)
        {
            if (GUILayout.Button("Adjust Durations"))
            {
                AdjustClipDurations();
            }
        }
        else
        {
            GUILayout.Label("Please assign a Timeline Asset.");
        }
    }

        private void AdjustClipDurations()
    {
        // TimelineAssetのPlayableDirectorを取得
        var director = FindObjectOfType<PlayableDirector>();
        if (director == null || director.playableAsset != timelineAsset)
        {
            Debug.LogError("No PlayableDirector with the selected Timeline Asset found.");
            return;
        }

        // Timelineの全Trackを取得
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            // ClipAを探す (例: 1番目のClipと仮定)
            if (track.GetClips().Count > 0)
            {
                TimelineClip clipA = track.GetClips()[0];

                // ClipAのDurationを基準に
                double clipADuration = clipA.duration;

                // そのTrack内の他のクリップをチェック
                foreach (var clip in track.GetClips())
                {
                    if (clip.duration > clipADuration)
                    {
                        // DurationがClipAより長ければ、ClipAと同じDurationに変更
                        clip.duration = clipADuration;
                    }
                }

                Debug.Log($"Adjusted durations for clips in track: {track.name}");
            }
        }

        // タイムラインを更新して変更を反映
        EditorUtility.SetDirty(timelineAsset);
        AssetDatabase.SaveAssets();
    }
}
