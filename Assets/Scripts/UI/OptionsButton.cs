using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OptionsButton : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] bool selected = false;
    [SerializeField] Color selectedTextColor = Color.white;
    Color unselectedTextColor;
    TextMeshProUGUI text;
    Button btn;

    private Color unselectedNormalColor;
    private Color unselectedHightlightedColor;
    private Color unselectedPressedColor;
    private Color unselectedSelectedColor;

    [Header("Selected Colors")]
    [SerializeField] Color selectedNormalColor = Color.white;
    [SerializeField] Color selectedHightlightedColor = Color.white;
    [SerializeField] Color selectedPressedColor = Color.white;
    [SerializeField] Color selectedSelectedColor = Color.white;

    void Start()
    {
        btn = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        unselectedTextColor = text.color;
        unselectedNormalColor = btn.colors.normalColor;
        unselectedHightlightedColor = btn.colors.highlightedColor;
        unselectedPressedColor = btn.colors.pressedColor;
        unselectedSelectedColor = btn.colors.selectedColor;
        OnSelected();
    }

    public void Select()
    {
        selected = !selected;
        OnSelected();
    }

    private void OnSelected()
    {
        panel.SetActive(selected);
        if (selected) panel.transform.SetAsFirstSibling();
        text.color = selected ? selectedTextColor : unselectedTextColor;
        var colors = btn.colors;
        colors.normalColor = selected ? selectedNormalColor : unselectedNormalColor;
        colors.highlightedColor = selected ? selectedHightlightedColor : unselectedHightlightedColor;
        colors.pressedColor = selected ? selectedPressedColor : unselectedPressedColor;
        colors.selectedColor = selected ? selectedSelectedColor : unselectedSelectedColor;
        btn.colors = colors;
    }
}
