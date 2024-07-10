using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    PlayerController player;

    private GameObject[] heartContainers;
    private Image[] heartFills;
    [SerializeField] private Transform heartParents;
    [SerializeField] private GameObject heartContainerPrefab;

    private void Start()
    {
        player = PlayerController.instance;
        heartContainers = new GameObject[PlayerController.instance.maxHealth];
        heartFills = new Image[PlayerController.instance.maxHealth];

        PlayerController.instance.onHealthChangeCallBack += UpdateHeartHUD;
        InstantiateHeartContainers();
        UpdateHeartHUD();
    }

    private void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < PlayerController.instance.maxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    private void SetFilledHearts()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < PlayerController.instance.Health)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }

    private void InstantiateHeartContainers()
    {
        for (int i = 0; i < PlayerController.instance.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartParents, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }

    private void UpdateHeartHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
    }
}
