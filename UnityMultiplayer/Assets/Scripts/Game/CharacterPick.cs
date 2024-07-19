using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPick : MonoBehaviour
{
    public bool IsTaken { get; private set; } = false;
    public int ID = 0;
    
    [SerializeField] private Image imageButton;
    [SerializeField] private Button pickButton;
    [SerializeField] private Color playerColor;

    private void Start()
    {
        imageButton.color = playerColor;
    }


    public void Take()
    {
        IsTaken = true;
        pickButton.interactable = false;
    }
}