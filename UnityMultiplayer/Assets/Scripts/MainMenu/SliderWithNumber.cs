using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class SliderWithNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI NumberText;
        [SerializeField] private Slider slider;
        
        public void SetTextNumber()
        {
            NumberText.text = slider.value.ToString();
        }

        
    }
}