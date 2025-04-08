using UnityEngine;
using UnityEngine.EventSystems;

public class SlotClickHandler : MonoBehaviour, IPointerClickHandler
{
    private PageSlotManager manager;
    private Transform slot;
    public int AssociatedIDPhoto = -1;

    public void Initialize(PageSlotManager mgr, Transform s)
    {
        manager = mgr;
        slot = s;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager.OnSlotClicked(slot, this);
    }
}