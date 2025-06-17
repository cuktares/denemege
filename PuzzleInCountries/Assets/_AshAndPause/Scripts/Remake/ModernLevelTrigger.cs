using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace StarterAssets
{
    public class ModernLevelTrigger : MonoBehaviour
    {
        [Header("Level Ayarları")]
        [Tooltip("Sonraki level'in adı (boş bırakılırsa otomatik index kullanılır)")]
        public string nextLevelName = "";
        [Tooltip("Trigger'a temas etmek için gerekli süre")]
        public float requiredContactTime = 0.5f;
        [Tooltip("Level geçişinde fade efekti kullanılsın mı?")]
        public bool useFadeEffect = true;
        
        [Header("Görsel Efektler")]
        public GameObject completionEffect;
        public AudioClip completionSound;
        [Range(0f, 1f)]
        public float soundVolume = 0.7f;
        
        [Header("Debug")]
        public bool debugMode = false;
        
        private bool isPlayerInContact = false;
        private float contactTimer = 0f;
        private bool isTriggered = false;
        private AudioSource audioSource;

        private void Start()
        {
            // AudioSource ekle
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = soundVolume;
            }
            
            // Trigger kontrolü
            Collider triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null && !triggerCollider.isTrigger)
            {
                if (debugMode) Debug.LogWarning($"{gameObject.name}: Collider trigger değil! Otomatik olarak trigger yapılıyor.");
                triggerCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (isPlayerInContact && !isTriggered)
            {
                contactTimer += Time.deltaTime;
                
                if (debugMode)
                {
                    Debug.Log($"Player contact timer: {contactTimer:F1}/{requiredContactTime:F1}");
                }
                
                if (contactTimer >= requiredContactTime)
                {
                    TriggerLevelCompletion();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isTriggered)
            {
                isPlayerInContact = true;
                contactTimer = 0f;
                
                if (debugMode)
                {
                    Debug.Log($"{gameObject.name}: Player trigger alanına girdi");
                }
                
                // Eğer süre gerekliliği yoksa hemen tetikle
                if (requiredContactTime <= 0f)
                {
                    TriggerLevelCompletion();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInContact = false;
                contactTimer = 0f;
                
                if (debugMode)
                {
                    Debug.Log($"{gameObject.name}: Player trigger alanından çıktı");
                }
            }
        }

        private void TriggerLevelCompletion()
        {
            if (isTriggered) return;
            
            isTriggered = true;
            
            if (debugMode)
            {
                Debug.Log($"{gameObject.name}: Level completion triggered!");
            }
            
            // Ses efekti çal
            if (completionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(completionSound, soundVolume);
            }
            
            // Görsel efekt göster
            if (completionEffect != null)
            {
                if (completionEffect.activeSelf)
                {
                    completionEffect.SetActive(true);
                }
                else
                {
                    Instantiate(completionEffect, transform.position, transform.rotation);
                }
            }
            
            // Level geçişini başlat
            StartCoroutine(LoadNextLevel());
        }

        private IEnumerator LoadNextLevel()
        {
            // Ses çalmasını bekle
            if (completionSound != null)
            {
                yield return new WaitForSeconds(completionSound.length);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            // Sonraki level'i belirle
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                // İsimle yükle
                if (debugMode)
                {
                    Debug.Log($"Loading level by name: {nextLevelName}");
                }
                
                LoadLevel(nextLevelName);
            }
            else
            {
                // Index ile yükle
                int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
                int nextSceneIndex = currentSceneIndex + 1;
                
                if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    if (debugMode)
                    {
                        Debug.Log($"Loading next level by index: {nextSceneIndex}");
                    }
                    
                    LoadLevel(nextSceneIndex);
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.Log("Bu son level! Oyun tamamlandı.");
                    }
                    
                    OnGameCompleted();
                }
            }
        }

        private void LoadLevel(string levelName)
        {
            if (useFadeEffect)
            {
                // Fade controller arayalım
                var fadeController = FindObjectOfType<FadeController>();
                if (fadeController != null)
                {
                    // Fade ile yükle (bu scripti remake'den alacağız)
                    fadeController.gameObject.SetActive(true);
                    // fadeController.FadeOutAndLoadLevel(levelName);
                }
                else
                {
                    SceneManager.LoadScene(levelName);
                }
            }
            else
            {
                SceneManager.LoadScene(levelName);
            }
        }

        private void LoadLevel(int sceneIndex)
        {
            if (useFadeEffect)
            {
                var fadeController = FindObjectOfType<FadeController>();
                if (fadeController != null)
                {
                    fadeController.gameObject.SetActive(true);
                    // fadeController.FadeOutAndLoadNextLevel();
                }
                else
                {
                    SceneManager.LoadScene(sceneIndex);
                }
            }
            else
            {
                SceneManager.LoadScene(sceneIndex);
            }
        }

        private void OnGameCompleted()
        {
            // Oyun tamamlandığında çağrılır
            Debug.Log("Oyun tamamlandı!");
            
            // Burada ana menüye dönüş veya krediler gösterilebilir
            // SceneManager.LoadScene("MainMenu");
        }

        private void OnDrawGizmosSelected()
        {
            // Trigger alanını görselleştir
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = isTriggered ? Color.green : (isPlayerInContact ? Color.yellow : Color.cyan);
                Gizmos.matrix = transform.localToWorldMatrix;
                
                // Box collider için
                BoxCollider boxCol = col as BoxCollider;
                if (boxCol != null)
                {
                    Gizmos.DrawCube(boxCol.center, boxCol.size);
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                    return;
                }
                
                // Sphere collider için
                SphereCollider sphereCol = col as SphereCollider;
                if (sphereCol != null)
                {
                    Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
                    return;
                }
            }
            
            // Contact timer görselleştirme
            if (isPlayerInContact && requiredContactTime > 0f)
            {
                float progress = contactTimer / requiredContactTime;
                Gizmos.color = Color.Lerp(Color.red, Color.green, progress);
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
            }
        }
    }
} 