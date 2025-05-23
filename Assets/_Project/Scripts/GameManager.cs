using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Animator fade;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Animator deathScreen;

    [SerializeField] private Animator[] animatorsButtons;

    public void PlayerDied()
    {
        fade.SetTrigger("FadeIn");
        Invoke("LoadDeathScreen", 1.5f);
    }

    void LoadDeathScreen()
    {
        deathScreen.gameObject.SetActive(true);
        deathScreen.SetTrigger("Show");
        audioMixer.SetFloat("Volume", Mathf.Log10(0.0001f) * 20);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HoverButton(int id)
    {
        animatorsButtons[id].SetBool("Hovered", true);
        animatorsButtons[id].SetTrigger("Hover");
    }

    public void LeaveButton(int id)
    {
        animatorsButtons[id].SetBool("Hovered", false);
        animatorsButtons[id].SetTrigger("Leave");
    }

    public void ClickButton(int id)
    {
        animatorsButtons[id].SetTrigger("Click");

        deathScreen.SetTrigger("Hide");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (id == 0) Invoke("LoadGame", 2);
        else if (id == 1) Invoke("LoadMenu", 2);
    }

    void LoadGame()
    {
        SceneManager.LoadScene("GAME");
    }

    void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
