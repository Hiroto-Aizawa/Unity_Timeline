using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline;

public class TimelineTrackOrderEditor : EditorWindow
{
    [MenuItem("Tools/Timeline Track Order Editor")]
    public static void ShowWindow()
    {
        GetWindow<TimelineTrackOrderEditor>("Timeline Track Order Editor");
    }

    private TimelineAsset _timelineAsset;
    private List<string> _desiredTrackOrder = new List<string> { "track_A", "track_B", "track_C", "track_D", "track_E" };

    private void OnGUI()
    {
        GUILayout.Label("Timeline Track Order Editor", EditorStyles.boldLabel);

        _timelineAsset = (TimelineAsset)EditorGUILayout.ObjectField("Timeline Asset", _timelineAsset, typeof(TimelineAsset), true);

        if (GUILayout.Button("Check and Reorder Tracks"))
        {
            if (_timelineAsset != null)
            {
                CheckAndReorderTracks();
            }
            else
            {
                Debug.LogWarning("Please assign a Timeline Asset.");
            }
        }
    }

    private void CheckAndReorderTracks()
    {
        if (_timelineAsset == null)
        {
            Debug.LogWarning("Timeline Asset is not assigned.");
            return;
        }

        var tracks = _timelineAsset.GetOutputTracks().ToList();

        // Create a dictionary from track names to their current index
        dynamic trackDictionary = tracks.ToDictionary(track => track.name);

        // Check if the order matches the desired order
        bool needsReordering = !_desiredTrackOrder.SequenceEqual(tracks.Select(track => track.name));

        if (needsReordering)
        {
            Debug.Log("Reordering tracks...");
            ReorderTracks(tracks, trackDictionary);
        }
        else
        {
            Debug.Log("Tracks are already in the desired order.");
        }
    }

    private void ReorderTracks(List<TrackAsset> tracks, dynamic trackDictionary)
    {
        Undo.RecordObject(_timelineAsset, "Reorder Tracks");

        // Remove tracks from the timeline
        foreach (var track in tracks)
        {
            _timelineAsset.DeleteTrack(track);
        }

        //データ削除後にTimelineに変更内容を反映する
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

        // Add tracks in the desired order
        foreach (var trackName in _desiredTrackOrder)
        {
            if (trackDictionary.TryGetValue(trackName, out TrackAsset track))
            {
                _timelineAsset.CreateTrack(track.GetType(), null, trackName);
                CopyTrackProperties(track, _timelineAsset.GetOutputTracks().Last());
            }
        }

        EditorUtility.SetDirty(_timelineAsset);
        AssetDatabase.SaveAssets();
        //データ削除後にTimelineに変更内容を反映する
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
    }

    private void CopyTrackProperties(TrackAsset sourceTrack, TrackAsset targetTrack)
    {
        foreach (var clip in sourceTrack.GetClips())
        {
            // Duplicate clip in the new track
            var newClip = targetTrack.CreateClip<PlayableAsset>();//clip.asset.GetType()
            newClip.displayName = clip.displayName;
            newClip.start = clip.start;
            newClip.duration = clip.duration;

            // Copy properties (e.g., in case of custom properties or animation properties)
            // This is a placeholder; actual property copying may vary based on clip types
        }
    }
}