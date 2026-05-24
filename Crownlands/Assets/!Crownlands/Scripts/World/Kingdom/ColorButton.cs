using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private Image swatch;
    [SerializeField] private KingdomMenuUI menu;

    public void OnClick()
    {
        menu.SelectColor(index);  
    }
}
