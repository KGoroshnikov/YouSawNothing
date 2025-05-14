using System.Collections;
using UnityEngine;

public class Wheel : IInteractable
{
    [SerializeField] private Transform wheelTransform;

    [SerializeField] private float spinDuration;

    [SerializeField] private int minFullSpins;
    [SerializeField] private int maxFullSpins;

    [SerializeField] private AnimationCurve spinEasing;

    [SerializeField] private int dep;

    [SerializeField] private float biasRate;

    [SerializeField] private Transform moneySpawn;
    [SerializeField] private GameObject moneyPref;
    [SerializeField] private float throwForce;

    [SerializeField] private float[] payouts;
    private int sectionCount;
    private float sectionAngle;

    private bool isSpinning;

    private float[] weights;

    void Awake()
    {
        sectionCount = payouts.Length;
        sectionAngle = 360f / sectionCount;
        ComputeWeights();
    }

    public void Spin()
    {
        ComputeWeights();
        StartCoroutine(SpinRoutine());
    }

    public override void GetUsed(Interaction player){
        isSpinning = true;
        Spin();
        player.GetInventory().RemoveMoney(dep);
    }

    public override bool CanInteractWithMe(Interaction player)
    {
        return player.GetInventory().GetMoney() >= dep && !isSpinning;
    }

    void SpawnMoney(int mul){
        Item item = Instantiate(moneyPref, moneySpawn.position, Quaternion.Euler(Vector3.zero)).GetComponent<Item>();
        item.SetMoney(dep * mul);
        item.ThrowMe(moneySpawn.forward * throwForce, moneySpawn.position);
    }

    void ComputeWeights()
    {
        int lossCount = 0;
        float sumP = 0f, sumP2 = 0f;
        for (int i = 0; i < sectionCount; i++)
        {
            if (payouts[i] <= 0f) lossCount++;
            else { sumP += payouts[i]; sumP2 += payouts[i] * payouts[i]; }
        }
        float R = biasRate;
        float denom = sumP2 - R * sumP;
        float s = denom > 0f ? R * lossCount / denom : 0f;

        weights = new float[sectionCount];
        for (int i = 0; i < sectionCount; i++)
            weights[i] = payouts[i] > 0f ? payouts[i] * s : 1f;
    }

    IEnumerator SpinRoutine()
    {
        int targetIndex = ChooseSectionIndex();
        int fullSpins = Random.Range(minFullSpins, maxFullSpins + 1);

        float sectionCenter = 0 - targetIndex * sectionAngle;

        float startAngle = wheelTransform.localEulerAngles.z;
        float endAngle = fullSpins * 360f + sectionCenter;

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = spinEasing.Evaluate(Mathf.Clamp01(elapsed / spinDuration));
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            wheelTransform.localEulerAngles = new Vector3(0, 0, angle % 360f);
            yield return null;
        }

        wheelTransform.localEulerAngles = new Vector3(0, 0, endAngle % 360f);

        float result = payouts[targetIndex];
        if (result >= 1)
            SpawnMoney((int)result);
        isSpinning = false;
        Debug.Log("result " + result + " targetIndex " + targetIndex);
    }

    int ChooseSectionIndex()
    {
        float totalWeight = 0f;
        foreach (var w in weights) totalWeight += w;
        float r = Random.value * totalWeight;
        float a = 0f;
        for (int i = 0; i < sectionCount; i++)
        {
            a += weights[i]; if (r <= a) return i;
        }
        return sectionCount - 1;
    }
}
