using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("ลาก Canvas ทั้งหมดมาใส่ในช่องนี้")]
    public GameObject[] targetCanvases;

    public void EnableCanvas(int index)
    {
        if (index >= 0 && index < targetCanvases.Length && targetCanvases[index] != null)
        {
            targetCanvases[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning("Canvas missing or index out of range!");
        }
    }

    public void DisableCanvas(int index)
    {
        if (index >= 0 && index < targetCanvases.Length && targetCanvases[index] != null)
        {
            targetCanvases[index].SetActive(false);
        }
    }

    public void SwitchToCanvas(int index)
    {
        for (int i = 0; i < targetCanvases.Length; i++)
        {
            if (targetCanvases[i] != null)
            {
                targetCanvases[i].SetActive(i == index);
            }
        }
    }
}