using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderControl : MonoBehaviour
{
    private Slider min, max;
    private Text text;

    private void Start()
    {
        // Get components to store
        max = transform.GetChild(0).GetComponent<Slider>();
        min = transform.GetChild(1).GetComponent<Slider>();
        text = transform.GetChild(2).GetChild(0).GetComponent<Text>();
    }

    public void MinThrottle()
    {
        // Stop min going above max slider
        if (min.value > max.value)
            min.value = max.value;
        UpdateText();
    }

    public void MaxThrottle()
    {
        // Stop max going below min slider
        if (max.value < min.value)
            max.value = min.value;
        UpdateText();
    }

    private void UpdateText()
    {
        // Update the label
        text.text = min.value + " - " + max.value;
    }
}
