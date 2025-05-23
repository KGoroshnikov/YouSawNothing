using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animator[] animators;
    [SerializeField] private Animator camAnim;
    [SerializeField] private Animator fadeAnim;

    [SerializeField] private AudioSource buttonAudio;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HoverButton(int id)
    {
        animators[id].SetBool("Hovered", true);
        animators[id].SetTrigger("Hover");
    }

    public void LeaveButton(int id)
    {
        animators[id].SetBool("Hovered", false);
        animators[id].SetTrigger("Leave");
    }

    public void ClickButton(int id)
    {
        buttonAudio.Play();
        animators[id].SetTrigger("Click");

        if (id == 0) LoadGame();
        else if (id == 1) LoadSettings();
        else if (id == 2) LoadExit();
        else if (id == 3) LoadIdle();
    }

    void LoadGame()
    {
        fadeAnim.SetTrigger("FadeIn");
        Invoke("SetGameScene", 2f);
    }

    void SetGameScene()
    {
        SceneManager.LoadScene("GAME");
    }

    void LoadIdle()
    {
        camAnim.SetTrigger("Idle");
    }

    void LoadSettings()
    {
        camAnim.SetTrigger("Settings");
    }

    void LoadExit()
    {
        camAnim.SetTrigger("Exit");
        fadeAnim.SetTrigger("FadeIn");
        Invoke("Exit", 1);
    }

    void Exit()
    {
        Application.Quit();
    }
}
