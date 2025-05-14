using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tablet : MonoBehaviour
{
    [System.Serializable]
    public class TaskNote{
        public GameObject obj;
        public TMP_Text noteTex;
        public TMP_Text descriptionText;
        public GameObject completedObj;
    }

    [SerializeField] private TaskNote[] taskNotes;

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInput playerInput;
    private bool isOpened;

    private InputAction openAction;

    void Awake()
    {
        openAction = playerInput.actions["Tab"];
        openAction.performed += _ => TabPressed();
    }

    void OnEnable()
    {
        openAction.Enable();
    }

    void OnDisable()
    {
        openAction.Disable();
    }

    void TabPressed(){
        isOpened = !isOpened;
        animator.SetTrigger(isOpened ? "OpenTablet" : "CloseTablet");
    }

    public void SetTasks(List<Task> newTasks){
        for(int i = 0; i < newTasks.Count; i++){
            taskNotes[i].obj.SetActive(true);
            taskNotes[i].noteTex.text = newTasks[i].taskName;
            taskNotes[i].noteTex.fontSize = newTasks[i].nameFontSize;
            taskNotes[i].descriptionText.text = newTasks[i].taskDescription;
            taskNotes[i].descriptionText.fontSize = newTasks[i].descriptionFontSize;
            taskNotes[i].completedObj.SetActive(false);
        }
        for(int i = newTasks.Count; i < 3; i++){
            taskNotes[i].obj.SetActive(false);
        }
    }

    public void UpdateStateTask(int id, bool completed){
        taskNotes[id].completedObj.SetActive(completed);
    }
}
