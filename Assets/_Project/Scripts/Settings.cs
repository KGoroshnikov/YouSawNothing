using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private Slider[] sliders;
    [SerializeField] private TMP_Text[] slidersText;

    [SerializeField] private float[] sensRange;

    [SerializeField] private AudioMixer audioMixer;

    void Start()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        slidersText[1].text = "" + PlayerPrefs.GetFloat("PlayerSens", 0.3f).ToString("F1");
        sliders[1].value = InverseLerp(sensRange[0], sensRange[1], PlayerPrefs.GetFloat("PlayerSens", 0.3f));

        slidersText[0].text = "" + PlayerPrefs.GetFloat("PlayerVolume", 1).ToString("F1");
        sliders[0].value = PlayerPrefs.GetFloat("PlayerVolume", 1);
    }

    public static float InverseLerp(float a, float b, float value)
    {
        if (a != b)
            return Mathf.Clamp01((value - a) / (b - a));
        else
            return 0f;
    }

    public void ChangeSens(){
        float sens = Mathf.Lerp(sensRange[0], sensRange[1], sliders[1].value);
        PlayerPrefs.SetFloat("PlayerSens", sens);
        slidersText[1].text = "" + sens.ToString("F1");
    }

    public void ChangeVolume(){
        PlayerPrefs.SetFloat("PlayerVolume", sliders[0].value);
        slidersText[0].text = "" + sliders[0].value.ToString("F1");

        audioMixer.SetFloat("Volume", Mathf.Log10(sliders[0].value)*20);
    }
}