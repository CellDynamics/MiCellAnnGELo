using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaSliderLabel : MonoBehaviour
{
    // Label of the alpha slider
    private Text label;
    // Slider object of the alpha slider
    private Slider slider;

    private void Start()
    {
        // Store text component
        label = transform.GetChild(3).GetChild(0).GetComponent<Text>();
        // Store the slider component
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Update the value of the slider label to the percentage of the slider
        var val = 100 * slider.value / 255;
        label.text = (int) val + "%";
    }
}
