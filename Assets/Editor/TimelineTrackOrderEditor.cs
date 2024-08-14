using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Linq;
using System.Collections.Generic;
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
        var tracks = _timelineAsset.GetOutputTracks().ToList();

        // Create a dictionary from track names to their current index
        var trackIndexMap = new Dictionary<string, int>();
        for (int i = 0; i < tracks.Count; i++)
        {
            trackIndexMap[tracks[i].name] = i;
        }

        // Check if the order matches the desired order
        bool needsReordering = false;
        for (int i = 0; i < _desiredTrackOrder.Count; i++)
        {
            if (!trackIndexMap.ContainsKey(_desiredTrackOrder[i]) || trackIndexMap[_desiredTrackOrder[i]] != i)
            {
                needsReordering = true;
                break;
            }
        }

        if (needsReordering)
        {
            Debug.Log("Reordering tracks...");
            ReorderTracks(tracks);
        }
        else
        {
            Debug.Log("Tracks are already in the desired order.");
        }
    }

    private void ReorderTracks(List<TrackAsset> tracks)
    {
        Undo.RecordObject(_timelineAsset, "Reorder Tracks");

        // Remove all tracks
        foreach (var track in tracks)
        {
            _timelineAsset.DeleteTrack(track);
        }

        // データ削除後にTiemlineの変更内容を反映する
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

        // Add tracks in the desired order
        foreach (var trackName in _desiredTrackOrder)
        {
            var track = tracks.FirstOrDefault(t => t.name == trackName);
            if (track != null)
            {
                //_timelineAsset.AddTrack(track.GetType(), track.parent, trackName);
                _timelineAsset.CreateTrack(track.GetType(), null, trackName);
            }
        }

        EditorUtility.SetDirty(_timelineAsset);
        AssetDatabase.SaveAssets();
    }
}