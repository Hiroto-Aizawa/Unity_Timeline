using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

// シーン実行外でも常に実行するように
[ExecuteAlways]
public class DisplayFrameCount : MonoBehaviour
{
    [SerializeField] PlayableDirector _playableDirector;

    [SerializeField] TextMeshProUGUI _text;

    [SerializeField] int _fps = 30;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        if (_playableDirector == null || _text == null)
        {
            return;
        }

        _text.text = CalculateFrame(_playableDirector.time);
    }

    // masterTimelineをHierarchyから取得して、PlayableDirectorとして扱う
    // Timelineのtime情報にアクセスして、fpsに応じてフレーム数を算出する
    // このスクリプトを持っているオブジェクトの子テキスト（TextMeshPro）にフレーム数を渡して表示する

    /// <summary>
    /// fpsに応じて現在のフレーム数を算出する関数
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private string CalculateFrame(double time)
    {
        double tmpFrameCount = _fps * time;
        // 小数を切り捨てて整数にする。　tmpFrameCountがdouble型なのでToSingle変換
        double frameCount = Mathf.FloorToInt(Convert.ToSingle(tmpFrameCount));
        _text.text = ($"Frame: {frameCount}");
        return _text.text;
    }
}
