using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Suitcase : MonoBehaviour
{
    [System.Serializable]
    public class StatInfo{
        public GameObject[] checks;
        public TMP_Text priceText;
    }

    [SerializeField] private StatInfo[] statInfos; // 0 - выносливость

    private PlayerStats playerStats;
    private PlayerController playerController;

    [SerializeField] private Transform itemSpawn;
    [SerializeField] private GameObject[] itemsInCase;
    [SerializeField] private GameObject[] itemPrefs;

    [SerializeField] private AudioSource buyButtonSound;

    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        playerStats = playerController.GetPlayerStats();

        UpdateUI();
    }

    public void SetupCase(){
        for(int i = 0; i < itemsInCase.Length; i++)
            itemsInCase[i].SetActive(true);
    }

    void UpdateUI(){
        List<int> stats = playerStats.GetAllStats();
        for(int s = 0; s < stats.Count; s++){
            for(int i = 0; i < statInfos[s].checks.Length; i++){
                if (i >= stats[s]) statInfos[s].checks[i].SetActive(false);
                else statInfos[s].checks[i].SetActive(true);
            }
            if (playerStats.GetPriceOf(s) == 0)
                statInfos[s].priceText.text = "";
            else 
                statInfos[s].priceText.text = $"{playerStats.GetPriceOf(s)}$";
        }
    }

    public bool CanUpgrade(int id){
        return playerController.GetInventory().GetMoney() >= playerStats.GetPriceOf(id);
    }

    public bool CanBuy(int id, int price){
        return playerController.GetInventory().GetMoney() >= price;
    }

    public void Upgrade(int id){
        buyButtonSound.Play();
        playerController.GetInventory().RemoveMoney(playerStats.GetPriceOf(id));
        playerStats.Upgrade(id);
        UpdateUI();
    }

    public void BuyItem(int price, int id){
        buyButtonSound.Play();
        playerController.GetInventory().RemoveMoney(price);
        Instantiate(itemPrefs[id], itemSpawn.position, itemSpawn.rotation);
        itemsInCase[id].SetActive(false);
    }
}
