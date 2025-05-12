using UnityEngine;

public class Tips : MonoBehaviour
{
    [SerializeField] private GameObject wehicleTip;

    public void SetWehicleTip(bool a){
        wehicleTip.SetActive(a);
    }
}
