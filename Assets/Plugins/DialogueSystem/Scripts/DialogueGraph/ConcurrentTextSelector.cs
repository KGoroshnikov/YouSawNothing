using TMPro;
using UnityEngine;

public class ConcurrentTextSelector : MonoBehaviour
{
    [SerializeField] private TMP_Text[] texts;
    private bool[] locked;
    

    private void Start()
    {
        locked = new bool[texts.Length];
    }

    public TMP_Text LockText()
    {
        for (var i = 0; i < texts.Length; i++)
            if (!locked[i])
            {
                locked[i] = true;
                return texts[i];
            }
        return null;
    }

    public void ReleaseText(TMP_Text text)
    {
        for (var i = 0; i < texts.Length; i++)
            if (texts[i] == text)
            {
                locked[i] = false;
                return;
            }
    }
}