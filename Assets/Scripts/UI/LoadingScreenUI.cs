using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject viewHolder;

    public void Show()
    {
        viewHolder.SetActive(true);
    }

    public void Hide()
    {
        viewHolder.SetActive(false);
    }
}
