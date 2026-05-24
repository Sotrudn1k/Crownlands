using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KingdomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image color;
    private KingdomMenuUI menu;
    private uint kingdomId;

    public void Init(uint id, uint colorId, string name, KingdomMenuUI menuUI)
    {
        nameText.text = name;
        kingdomId = id;
        menu = menuUI;
        color.color = menu.palette[colorId];
    }

    public void OnClick()
    {
        menu.JoinKingdom(kingdomId);
    }
}
