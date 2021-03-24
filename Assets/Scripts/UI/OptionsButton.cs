using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OptionsButton : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] bool selected = false;

    [Header("Selected Colors")]
    [SerializeField] Color selectedTextColor = Color.white;
    [SerializeField] Color selectedNormalColor = Color.white;
    [SerializeField] Color selectedHightlightedColor = Color.white;
    [SerializeField] Color selectedPressedColor = Color.white;
    [SerializeField] Color selectedSelectedColor = Color.white;
    [SerializeField] bool lastChild;
    [SerializeField] bool firstChild;

    private Color unselectedTextColor;
    private Color unselectedNormalColor;
    private Color unselectedHightlightedColor;
    private Color unselectedPressedColor;
    private Color unselectedSelectedColor;

    private TextMeshProUGUI text;
    private Button btn;

    void Start()
    {
        btn = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        unselectedTextColor = text.color;
        unselectedNormalColor = btn.colors.normalColor;
        unselectedHightlightedColor = btn.colors.highlightedColor;
        unselectedPressedColor = btn.colors.pressedColor;
        unselectedSelectedColor = btn.colors.selectedColor;
        UpdateVisibility();
    }

    public void ChangeSelect()
    {
        selected = !selected;
        UpdateVisibility();
    }

    public void Select()
    {
        selected = true;
        UpdateVisibility();
    }

    public void Deselect()
    {
        selected = false;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        UpdatePanelVisibility();
        UpdateButtonColors();
    }

    private void UpdatePanelVisibility()
    {
        panel.SetActive(selected);
        if (selected)
        {
            if (lastChild)
            {
                panel.transform.SetAsLastSibling();
            }
            else if (firstChild)
            {
                panel.transform.SetAsFirstSibling();
            }
        }
    }

    private void UpdateButtonColors()
    {
        text.color = selected ? selectedTextColor : unselectedTextColor;

        var colors = btn.colors;
        colors.normalColor = selected ? selectedNormalColor : unselectedNormalColor;
        colors.highlightedColor = selected ? selectedHightlightedColor : unselectedHightlightedColor;
        colors.pressedColor = selected ? selectedPressedColor : unselectedPressedColor;
        colors.selectedColor = selected ? selectedSelectedColor : unselectedSelectedColor;
        btn.colors = colors;
    }
}
