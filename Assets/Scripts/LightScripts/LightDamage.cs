using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LightDamage : MonoBehaviour
{

    [Header("Light Damage Settings")]
    [SerializeField] private Slider healthBar;
    [SerializeField] bool inLight = false;
    [SerializeField] float timeInLight = 0f;
    [SerializeField] float timeAllowedInLight = 3f;
    [SerializeField]  Color normalColor = Color.green;
    [SerializeField]  Color cooldownColor = Color.red; 
    [SerializeField] Image barImage;
    [SerializeField] float cooldownTime = 3f; 
    [SerializeField] TMP_Text timeLabel;
    [SerializeField] bool isPaused = false;
    [SerializeField] private GameObject deathScreen;
    //[SerializeField] GameObject deathScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = timeAllowedInLight;
            healthBar.minValue = 0f;
            healthBar.value = timeAllowedInLight;
            healthBar.interactable = false;
        }
        if (barImage == null && healthBar != null && healthBar.fillRect != null)
        {
            barImage = healthBar.fillRect.GetComponent<Image>();
            barImage.color = normalColor;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused)
        {
            return; 
        }

        if (inLight)
        {         
            timeInLight += Time.deltaTime;
            timeInLight = Mathf.Clamp(timeInLight, 0f, timeAllowedInLight);
            if (timeInLight >= timeAllowedInLight)
            {
                Debug.Log("Player has been in light for too long!");
                /*

                PLAYER DIES LOGIC HERE

                */
                PauseGame();

            }

        }
        else
        {
            if (timeInLight > 0f)
            {
                timeInLight -= Time.deltaTime;
                timeInLight = Mathf.Clamp(timeInLight, 0f, timeAllowedInLight);
         
         
            }
        }

        if (healthBar != null)
        {
            healthBar.SetValueWithoutNotify(timeAllowedInLight - timeInLight);
        }

        
        if (barImage != null)
        {
            float progress = timeInLight / timeAllowedInLight; 
            barImage.color = Color.Lerp(normalColor, cooldownColor, progress);
        }

        // Update Text Label
        if (timeLabel != null)
        {
            float timeLeft = timeAllowedInLight - timeInLight;
            timeLabel.text = "Time in Light Left: " + Mathf.CeilToInt(Mathf.Max(timeLeft, 0f));
        }
    }

    public void PauseGame()
    {
        deathScreen.SetActive(true);
        Time.timeScale = 0f; 
        isPaused = true;
    }

    public void ResumeGame()
    {
        deathScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }


    //CALL THIS WITH ON TRGGER SCRIPTS
    public void SetInLight(bool value)
    {
        inLight = value;
    }

    



    
}
