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

    public int GetPriceOf(int id)
    {
        return id switch
        {
            0 => staminaLevel < staminaPrices.Length ? staminaPrices[staminaLevel] : 0,
            1 => convictionLevel < convictionPrices.Length ? convictionPrices[convictionLevel] : 0,
            2 => timeLevel < timePrices.Length ? timePrices[timeLevel] : 0,
            _ => 0
        };
    }

    public void Upgrade(int id)
    {
        switch (id)
        {
            case 0:
                staminaLevel++;
                break;
            case 1:
                convictionLevel++;
                break;
            case 2:
                timeLevel++;
                break;
        }
    }
    
    public float Stamina => stamina[staminaLevel];
    public float Conviction => conviction[convictionLevel];
    public float Time => time[timeLevel];
}
