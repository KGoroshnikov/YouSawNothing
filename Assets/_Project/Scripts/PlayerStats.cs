using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int staminaLevel;
    [SerializeField] private int convictionLevel;
    [SerializeField] private int timeLevel;

    [SerializeField] private float[] stamina;
    [SerializeField] private int[] staminaPrices;

    [SerializeField] private float[] conviction;
    [SerializeField] private int[] convictionPrices;

    [SerializeField] private float[] time;
    [SerializeField] private int[] timePrices;

    public List<int> GetAllStats(){
        List<int> stats = new List<int>(){staminaLevel, convictionLevel, timeLevel};
        return stats;
    }

    public int GetPriceOf(int id){
        if (id == 0){
            return staminaLevel < staminaPrices.Length ? staminaPrices[staminaLevel] : 0;
        }
        else if (id == 1){
            return convictionLevel < convictionPrices.Length ? convictionPrices[convictionLevel] : 0;
        }
        else if (id == 2){
            return timeLevel < timePrices.Length ? timePrices[timeLevel] : 0;
        }
        return 0;
    }

    public void Upgrade(int id){
        if (id == 0) staminaLevel++;
        else if (id == 1) convictionLevel++;
        else if (id == 2) timeLevel++;
    }
}
