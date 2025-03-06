using UnityEngine;
using UnityEditor;
using System.IO;

public class TrackAndClipTemplateCreator : EditorWindow
{
    private string _namespaceName = "Flame";  // デフォルトnamespace
    private string _scriptName = "Template";  // デフォルトname
    private Color color = new(1f, 1f, 1f);
    private string _directoryPath = "Assets/Scripts";

    // ウィンドウを表示
    [MenuItem("Tools/Track and Clip Template Creator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TrackAndClipTemplateCreator));
    }

    private void OnGUI()
    {
        GUILayout.Label("Custom Track and Clip Template Creator", EditorStyles.boldLabel);
        GUILayout.Space(20);

        GUILayout.Label("【Common】", EditorStyles.boldLabel);
        // namespaceとnameの入力フィールドを作成
        _namespaceName = EditorGUILayout.TextField(" Namespace", _namespaceName);
        GUILayout.Space(5);
        _scriptName = EditorGUILayout.TextField(" Name", _scriptName);
        GUILayout.Space(5);
        _directoryPath = EditorGUILayout.TextField("Directory Path", _directoryPath);
        GUILayout.Space(5);

        GUILayout.Space(20);
        GUILayout.Label("【Track】", EditorStyles.boldLabel);
        color = EditorGUILayout.ColorField(" Track Color", color);
        GUILayout.Space(20);

        // GUILayout.Label("【Clip】", EditorStyles.boldLabel);
        // GUILayout.Space(20);

        // GUILayout.Label("【Behaviour】", EditorStyles.boldLabel);
        // GUILayout.Space(20);

        if (GUILayout.Button("Create Templates"))
        {
            CreateTemplates(_namespaceName, _scriptName);
        }
    }

    // テンプレートを作成するメソッド
    private void CreateTemplates(string namespaceName, string scriptName)
    {
        // ファイル名を決定
        string trackFileName = scriptName + "Track.cs";
        string clipFileName = scriptName + "Clip.cs";
        string behaviourFileName = scriptName + "Behaviour.cs";

        // カスタムトラックとカスタムクリップのスクリプトテンプレートを作成
        string trackScriptContent = GenerateTrackScript(namespaceName, scriptName, color);
        string clipScriptContent = GenerateClipScript(namespaceName, scriptName);
        string behaviourScriptContent = GenerateBehaviourScript(namespaceName, scriptName);

        // ファイルパスを作成
        string trackFilePath = Path.Combine(_directoryPath, trackFileName);
        string clipFilePath = Path.Combine(_directoryPath, clipFileName);
        string behaviourPath = Path.Combine(_directoryPath, behaviourFileName);

        // ファイルを保存
        File.WriteAllText(trackFilePath, trackScriptContent);
        File.WriteAllText(clipFilePath, clipScriptContent);
        File.WriteAllText(behaviourPath, behaviourScriptContent);

        // Unityエディタに新しいファイルを反映
        AssetDatabase.Refresh();

        // メッセージを表示
        EditorUtility.DisplayDialog("Template Created", $"Created {trackFileName} and {clipFileName} in {_directoryPath}", "OK");
    }

    // カスタムトラックのスクリプトテンプレートを生成
    private string GenerateTrackScript(string namespaceName, string scriptName, Color color)
    {
        return $@"
using UnityEngine;
using UnityEngine.Timeline;

namespace {namespaceName}
{{
    [TrackColor({color.r}f, {color.g}f, {color.b}f)]
    [TrackBindingType(typeof(UnityEngine.GameObject))]
    [TrackClipType(typeof({scriptName}Clip))]
    public class {scriptName}Track : TrackAsset
    {{
        // ここにカスタムトラックの処理を追加できます
    }}
}}
";
    }

    // カスタムクリップのスクリプトテンプレートを生成
    private string GenerateClipScript(string namespaceName, string scriptName)
    {
        return $@"
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace {namespaceName}
{{
    [System.Serializable]
    public class {scriptName}Clip : PlayableAsset, ITimelineClipAsset
    {{
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {{
            var playable = ScriptPlayable<TemplateBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            
            return playable;
        }}

        // このクリップがサポートする機能
        public ClipCaps clipCaps
        {{
            get {{ return ClipCaps.All; }}
        }}
    }}
}}
";
    }

    // カスタムビヘイビアーのスクリプトテンプレートを生きる成
    private string GenerateBehaviourScript(string namespaceName, string scriptName)
    {
        return $@"
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace {namespaceName}
{{
    [System.Serializable]
    public class {scriptName}Behaviour : PlayableBehaviour
    {{
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {{
            //処理
        }}
    }}
}}
";
    }
}
