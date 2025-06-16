using UnityEngine;
using StarterAssets;

public class MobileUIController : MonoBehaviour
{
    [Header("References")]
    public StarterAssetsInputs starterAssetsInputs;
    public GameObject mobileUICanvas;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.M; // M tuşu ile mobile UI'ı aç/kapat
    public bool startWithMobileUI = true;
    
    private bool isMobileUIActive = false;
    
    void Start()
    {
        // Otomatik olarak StarterAssetsInputs'u bul
        if (starterAssetsInputs == null)
        {
            starterAssetsInputs = FindObjectOfType<StarterAssetsInputs>();
        }
        
        // Otomatik olarak Mobile UI Canvas'ı bul
        if (mobileUICanvas == null)
        {
            GameObject canvas = GameObject.Find("UI_Canvas_StarterAssetsInputs_Joysticks");
            if (canvas != null)
                mobileUICanvas = canvas;
        }
        
        // Başlangıçta mobile UI durumunu ayarla
        if (startWithMobileUI)
        {
            EnableMobileUI(true);
        }
    }
    
    void Update()
    {
        // M tuşu ile mobile UI'ı aç/kapat
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMobileUI();
        }
    }
    
    public void EnableMobileUI(bool enable)
    {
        isMobileUIActive = enable;
        
        // Mobile UI Canvas'ı aktif/pasif yap
        if (mobileUICanvas != null)
        {
            mobileUICanvas.SetActive(enable);
        }
        
        // StarterAssetsInputs'a mobile UI durumunu bildir
        if (starterAssetsInputs != null)
        {
            starterAssetsInputs.EnableMobileUI(enable);
        }
        
        // Cursor'ı ayarla
        if (enable)
        {
            // Mobile UI aktifken mouse görünür ve free
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Normal PC oyun modunda cursor gizli ve kilitli
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        Debug.Log($"Mobile UI {(enable ? "Aktif" : "Pasif")} - Mouse {(enable ? "Free" : "Locked")}");
    }
    
    public void ToggleMobileUI()
    {
        EnableMobileUI(!isMobileUIActive);
    }
    
    // UI Button'dan çağrılabilir
    public void OnToggleUIButton()
    {
        ToggleMobileUI();
    }
} 