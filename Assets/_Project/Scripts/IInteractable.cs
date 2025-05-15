using UnityEngine;
using UnityEngine.VFX;

public class IInteractable : MonoBehaviour
{
    [Header("Interactable")]
    [SerializeField] protected Renderer renderer;
    [SerializeField] protected Animator animator;
    [SerializeField] protected VisualEffect vfxTip;
    protected MaterialPropertyBlock propBlock;
    public string I_Name = "ITEM";
    public string I_Action = "USE";
    public bool NeedTip = true;

    protected virtual void Start(){
        if (renderer == null) return;
        renderer.material = new Material(renderer.sharedMaterial);
        propBlock = new MaterialPropertyBlock();
    }

    protected void SetInteracted(float value) {
        if (renderer == null) return;

        if (propBlock == null){
            renderer.material = new Material(renderer.sharedMaterial);
            propBlock = new MaterialPropertyBlock();
        }

        renderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_Interacted", value);
        renderer.SetPropertyBlock(propBlock);
    }

    public virtual void GetUsed(Interaction player){
        //
    }

    public virtual bool CanInteractWithMe(Interaction player){
        return true;
    }
}