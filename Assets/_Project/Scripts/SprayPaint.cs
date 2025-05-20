using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SprayPaint : MonoBehaviour
{
    [SerializeField] private GameObject decalPref;
    [SerializeField] private float maxDist;
    [SerializeField] private LayerMask lm;
    [SerializeField] private float surfaceOffset;
    [SerializeField] private float spawnTime;

    [SerializeField] private Material[] paints;

    public void SpawnPaint()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDist, lm))
        {
            Vector3 surf = hit.normal;
            DecalProjector decalProjector = Instantiate(decalPref, hit.point + surf * surfaceOffset, Quaternion.Euler(Vector3.zero))
                        .GetComponent<DecalProjector>();
            decalProjector.transform.forward = -surf;

            if (hit.collider.TryGetComponent<PaintHolder>(out PaintHolder paintHolder))
            {
                paintHolder.GetPainted(true);
            }

            var mat = Instantiate(paints[Random.Range(0, paints.Length)]);

            decalProjector.material = mat;

            StartCoroutine(FadeFloat(mat));
        }
    }

    IEnumerator FadeFloat(Material mat)
    {
        float elapsed = 0f;
        while (elapsed < spawnTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spawnTime);
            mat.SetFloat("_t", t);
            yield return null;
        }
        mat.SetFloat("_t", 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * maxDist);
    }
}
