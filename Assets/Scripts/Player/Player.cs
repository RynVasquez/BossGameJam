using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;

    
    private PlayerMovement playerMovement;
    
    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
    
        staminaSlider.value = 1f;
    }
    
    private void Update()
    {
        staminaSlider.value = playerMovement.GetStaminaPercentage();
    }
}