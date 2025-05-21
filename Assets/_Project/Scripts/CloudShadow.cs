using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CloudShadow : MonoBehaviour
{
    [SerializeField] private UniversalAdditionalLightData lightData;
    [SerializeField] private Vector2 speedClouds;

    void Update()
    {
        lightData.lightCookieOffset += speedClouds * Time.deltaTime;
    }
}