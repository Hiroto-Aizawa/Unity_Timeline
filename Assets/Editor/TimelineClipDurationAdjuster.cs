using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using System.Collections.Generic;

public class TimelineClipDurationAdjuster : EditorWindow
{
    [MenuItem("Tools/Adjust Timeline Clip Durations")]
    public static void ShowWindow()
    {
        GetWindow<TimelineClipDurationAdjuster>("Adjust Timeline Durations");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Adjust Durations"))
        {
            AdjustClipDurations();
        }
    }

    // TimelineのクリップのDurationを調整する関数
    private static void AdjustClipDurations()
    {
        // TimelineAssetを取得
        TimelineAsset timeline = Selection.activeObject as TimelineAsset;
        
        if (timeline == null)
        {
            Debug.LogError("Please select a Timeline Asset.");
            return;
        }

        // Timelineのトラックを走査
        foreach (var track in timeline.GetOutputTracks())
        {
            // 各トラックのクリップを走査
            List<TimelineClip> clips = new List<TimelineClip>(track.GetClips());
            TimelineClip clipA = null;
            
            // ClipAを見つける（例えば名前で指定することもできる）
            foreach (var clip in clips)
            {
                Debug.Log($"Clip Name: {clip.displayName}");
                // ClipA
                if (clip.displayName == "ClipA")
                {
                    Debug.Log("Found Base Clip");
                    clipA = clip;
                    break;
                }
            }

            if (clipA != null)
            {
                // ClipAのDurationを取得
                double clipADuration = clipA.duration;
                
                // ClipAよりもDurationが長いクリップを調整
                foreach (var clip in clips)
                {
                    if (clip != clipA && clip.duration > clipADuration)
                    {
                        // DurationをClipAと同じに変更
                        clip.duration = clipADuration;
                        Debug.Log($"Changed duration of {clip.displayName} to {clipADuration}");
                    }
                }
            }
            else
            {
                Debug.LogError("ClipA not found in the selected Timeline.");
            }
        }
    }
}
