
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
//using Unity.Mathematics;
using System;


[System.Serializable]
public class MaterialPropertyBehaviour : PlayableBehaviour
{
    public Material material;

    // Base Map
    public Color targetBaseMapColor;
    private Color initBaseMapColor;

    // Emission
    // public Color targetEmissionColor;
    // private Color initEmissionColor;

    public float startTime;
    public float endTime;

    private float lerpFactor = 0f; // 色補完の進行度（0~1）

    //元のマテリアルの色を保存する
    public override void OnGraphStart(Playable playable)
    {
        if (material != null)
        {
            initBaseMapColor = material.color;
            // initEmissionColor = material.GetColor("_EmissionColor");
            // Debug.Log($"initEmissionColor: {initEmissionColor}");
        }
    }

    // 停止時に元の色に戻す
    public override void OnGraphStop(Playable playable)
    {
        if (material != null)
        {
            material.color = initBaseMapColor;
            // material.SetColor("_EmissionColor", initEmissionColor);
        }
        base.OnGraphStop(playable);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //処理
        if (material != null)
        {
            float time = (float)playable.GetTime();

            if (startTime <= time && time <= endTime)
            {
                // 時間に応じて色を補完する
                float ratio = Mathf.InverseLerp(startTime, endTime, time);
                lerpFactor = ratio;
                Debug.Log($"currentTime: {time}");
            }

            material.color = Color.Lerp(initBaseMapColor, targetBaseMapColor, lerpFactor);
        }
    }
}
