using UnityEngine;
using UnityEngine.UI;

public class CharacterPick : MonoBehaviour
    {
        [SerializeField] private Button pickButton;
        public int ID = 0;
        private bool isTaken = false;
    
        public bool IsTaken 
        {
            get { return isTaken; }
        }

        public void Take()
        {
            isTaken = true;
            pickButton.interactable = false;
        }

    }

   
