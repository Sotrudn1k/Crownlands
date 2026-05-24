using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KingdomMenuUI : MonoBehaviour
{
    public static KingdomMenuUI Instance;
    public Color[] palette;

    [SerializeField] TMP_InputField kingdomNameField;
    [SerializeField] GameObject root;

    [SerializeField] int selectedColorIndex = 0;
    [SerializeField] KingdomListItem itemPrefab;
    [SerializeField] Transform scrollRectContent;
    PlayerKingdom localPlayerKingdom;
    bool isOpen;

    
    public void CreateKingdom() => localPlayerKingdom.CreateKingdom(kingdomNameField.text, (byte)selectedColorIndex);
    public void JoinKingdom(uint id) => localPlayerKingdom.JoinKingdom(id);
    public void LeaveKingdom() => localPlayerKingdom.LeaveKingdom();
    
    public void SetLocalPlayer(PlayerKingdom pk) => localPlayerKingdom = pk;
    
    public void SelectColor(int index)
    {
        selectedColorIndex = Mathf.Clamp(index, 0, palette.Length - 1);
    }
    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        root.SetActive(isOpen);

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
    public void RefreshList(KingdomInfo[] kingdoms)
    {
        foreach (Transform child in scrollRectContent)
            Destroy(child.gameObject);

        foreach (var k in kingdoms)
        {
            var item = Instantiate(itemPrefab, scrollRectContent);
            item.Init(k.id, k.colorId, k.name, this);
        }
    }
}
[System.Serializable]
public struct KingdomInfo
{
    public uint id;
    public string name;
    public uint colorId;
}
