using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private Animator[] animators;
    [SerializeField] private GameObject pauseUI;

    [SerializeField] private Animator fadeAnim;

    [SerializeField] private GameObject setingsUI;

    [SerializeField] private PlayerController playerContoller;
    [SerializeField] private EscManager escManager;
    [SerializeField] private Settings settings;

    [SerializeField] private TaskManager taskManager;

    [SerializeField] private AudioSource buttonSound;
    private bool paused;

    private bool ableToPause;

    void Start()
    {
        escManager.AddWeight(999, gameObject, EscPressed);
    }

    public void SetAblePause(bool a)
    {
        ableToPause = a;
    }

    public void EscPressed()
    {
        if (!ableToPause) return;
        
        paused = !paused;
        pauseUI.SetActive(paused);

        taskManager.SetGamePaused(paused);

        if (paused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            playerContoller.FreezePlayer();
            settings.UpdateStats();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            playerContoller.GetSens();
            setingsUI.SetActive(false);

            playerContoller.UnfreezePlayer();
        }
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
        buttonSound.Play();
        animators[id].SetTrigger("Click");

        if (id == 0) EscPressed();
        else if (id == 1) OpenSettings();
        else if (id == 2) LoadMenu();
    }

    void OpenSettings()
    {
        setingsUI.SetActive(!setingsUI.activeSelf);
    }

    void LoadMenu()
    {
        fadeAnim.SetTrigger("FadeIn");
        Invoke("Menu", 1.5f);
    }

    void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}
