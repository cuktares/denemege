using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StarterAssets
{
    [System.Serializable]
    public class ModernButtonAction
    {
        public GameObject targetObject;
        public enum ActionType { Move, Scale, Activate, Rotation }
        public ActionType actionType;

        // Hareket ayarları
        public Vector3 moveDirection;
        public float moveDistance;
        public float moveSpeed = 2f;
        public bool resetOnRelease = true;
        
        // Ölçek ayarları
        public Vector3 scaleChange = Vector3.one;
        public float scaleSpeed = 2f;
        public bool resetScaleOnRelease = true;
        
        // Rotasyon ayarları
        public Vector3 rotationAxis = Vector3.up;
        public float rotationAngle = 90f;
        public float rotationSpeed = 2f;
        public bool resetRotationOnRelease = true;
        
        [HideInInspector]
        public Vector3 originalPosition;
        [HideInInspector]
        public Vector3 originalScale;
        [HideInInspector]
        public Quaternion originalRotation;
    }

    public class ModernButtonController : MonoBehaviour
    {
        [Header("Buton Ayarları")]
        public List<ModernButtonAction> actions = new List<ModernButtonAction>();
        public float pressDepth = 0.1f;
        public float pressSpeed = 5f;
        public bool requiresPlayer = false; // Player gerekli mi, yoksa Pushable yeterli mi?
        
        [Header("Ses Ayarları")]
        public AudioClip pressSound;
        public AudioClip releaseSound;
        [Range(0f, 1f)]
        public float soundVolume = 0.5f;
        
        private bool isPressed = false;
        private bool isMoving = false;
        private Vector3 originalPosition;
        private Vector3 pressedPosition;
        private AudioSource audioSource;
        private float lastTriggerTime = 0f;
        private const float TRIGGER_COOLDOWN = 0.2f; // Trigger arası minimum süre
        private HashSet<GameObject> objectsOnButton = new HashSet<GameObject>();

        private void Start()
        {
            originalPosition = transform.position;
            pressedPosition = originalPosition - new Vector3(0, pressDepth, 0);
            
            // AudioSource ekle
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = soundVolume;
            }
            
            // Her bir action için orijinal değerleri kaydet
            foreach (ModernButtonAction action in actions)
            {
                if (action.targetObject != null)
                {
                    action.originalPosition = action.targetObject.transform.position;
                    action.originalScale = action.targetObject.transform.localScale;
                    action.originalRotation = action.targetObject.transform.rotation;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Trigger cooldown kontrolü
            if (Time.time - lastTriggerTime < TRIGGER_COOLDOWN)
                return;
                
            bool canPress = false;
            
            if (requiresPlayer)
            {
                canPress = other.CompareTag("Player");
            }
            else
            {
                canPress = other.CompareTag("Player") || other.CompareTag("Pushable");
            }
            
            if (canPress)
            {
                objectsOnButton.Add(other.gameObject);
                
                // İlk obje butonun üzerine geldiğinde bas
                if (!isPressed && objectsOnButton.Count == 1)
                {
                    isPressed = true;
                    lastTriggerTime = Time.time;
                    StartCoroutine(PressButton());
                    ExecuteActions(true);
                    
                    if (pressSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(pressSound, soundVolume);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"[{gameObject.name}] OnTriggerExit - Object: {other.name}, Tag: {other.tag}");
            
            bool canRelease = false;
            
            if (requiresPlayer)
            {
                canRelease = other.CompareTag("Player");
            }
            else
            {
                canRelease = other.CompareTag("Player") || other.CompareTag("Pushable");
            }
            
            Debug.Log($"[{gameObject.name}] canRelease: {canRelease}, objectsOnButton.Contains: {objectsOnButton.Contains(other.gameObject)}");
            
            if (canRelease && objectsOnButton.Contains(other.gameObject))
            {
                objectsOnButton.Remove(other.gameObject);
                Debug.Log($"[{gameObject.name}] Obje kaldırıldı - Remaining count: {objectsOnButton.Count}");
                
                // Tüm objeler butondan çıktığında bırak
                if (isPressed && objectsOnButton.Count == 0)
                {
                    Debug.Log($"[{gameObject.name}] ReleaseButtonWithDelay başlatılıyor");
                    lastTriggerTime = Time.time;
                    StartCoroutine(ReleaseButtonWithDelay());
                }
            }
        }

        private IEnumerator PressButton()
        {
            isMoving = true;
            float elapsedTime = 0f;
            Vector3 startPos = transform.position;
            
            // Press depth sıfırsa animasyon yapma
            if (pressDepth <= 0f)
            {
                isMoving = false;
                yield break;
            }
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * pressSpeed;
                transform.position = Vector3.Lerp(startPos, pressedPosition, elapsedTime);
                yield return null;
            }
            
            transform.position = pressedPosition;
            isMoving = false;
        }

        private IEnumerator ReleaseButtonWithDelay()
        {
            Debug.Log($"[{gameObject.name}] ReleaseButtonWithDelay başladı - objectsOnButton.Count: {objectsOnButton.Count}");
            
            // Kısa bir delay - obje tam olarak çıkmasını bekle
            yield return new WaitForSeconds(0.1f);
            
            Debug.Log($"[{gameObject.name}] Delay sonrası - objectsOnButton.Count: {objectsOnButton.Count}");
            
            // Hala butonun üzerinde obje varsa geri çıkma
            if (objectsOnButton.Count > 0)
            {
                Debug.Log($"[{gameObject.name}] Hala objeler var, release iptal edildi");
                yield break; // isPressed zaten true olarak kalacak
            }
            
            // Artık gerçekten release edebiliriz
            Debug.Log($"[{gameObject.name}] Release işlemi başlıyor - isPressed: {isPressed}");
            isPressed = false;
            ExecuteActions(false);
            
            if (releaseSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(releaseSound, soundVolume);
            }
            
            isMoving = true;
            float elapsedTime = 0f;
            Vector3 startPos = transform.position;
            
            // Press depth sıfırsa animasyon yapma
            if (pressDepth <= 0f)
            {
                isMoving = false;
                yield break;
            }
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * pressSpeed;
                transform.position = Vector3.Lerp(startPos, originalPosition, elapsedTime);
                yield return null;
            }
            
            transform.position = originalPosition;
            isMoving = false;
        }

        private void ExecuteActions(bool isPressing)
        {
            Debug.Log($"[{gameObject.name}] ExecuteActions çağrıldı - isPressing: {isPressing}, actions count: {actions.Count}");
            
            foreach (ModernButtonAction action in actions)
            {
                if (action.targetObject == null) 
                {
                    Debug.Log($"[{gameObject.name}] Action target object null!");
                    continue;
                }

                Debug.Log($"[{gameObject.name}] Action işleniyor - Type: {action.actionType}, Target: {action.targetObject.name}");

                switch (action.actionType)
                {
                    case ModernButtonAction.ActionType.Move:
                        StartCoroutine(MoveObject(action, isPressing));
                        break;

                    case ModernButtonAction.ActionType.Scale:
                        StartCoroutine(ScaleObject(action, isPressing));
                        break;

                    case ModernButtonAction.ActionType.Activate:
                        if (isPressing)
                        {
                            action.targetObject.SetActive(!action.targetObject.activeSelf);
                        }
                        break;
                    case ModernButtonAction.ActionType.Rotation:
                        StartCoroutine(RotateObject(action, isPressing));
                        break;
                }
            }
        }

        private IEnumerator MoveObject(ModernButtonAction action, bool moveUp)
        {
            Vector3 startPos = action.targetObject.transform.position;
            Vector3 targetPos;
            
            if (moveUp)
            {
                targetPos = action.originalPosition + action.moveDirection.normalized * action.moveDistance;
                Debug.Log($"[{gameObject.name}] MoveObject UP - Target: {action.targetObject.name}, From: {startPos}, To: {targetPos}");
            }
            else
            {
                if (action.resetOnRelease)
                {
                    targetPos = action.originalPosition;
                    Debug.Log($"[{gameObject.name}] MoveObject DOWN - Target: {action.targetObject.name}, From: {startPos}, To: {targetPos}");
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] MoveObject DOWN - resetOnRelease false, çıkılıyor");
                    yield break;
                }
            }

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * action.moveSpeed;
                action.targetObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            action.targetObject.transform.position = targetPos;
        }

        private IEnumerator ScaleObject(ModernButtonAction action, bool scaleUp)
        {
            Vector3 startScale = action.targetObject.transform.localScale;
            Vector3 targetScale;
            
            if (scaleUp)
            {
                targetScale = action.originalScale + action.scaleChange;
                Debug.Log($"[{gameObject.name}] ScaleObject UP - Target: {action.targetObject.name}, From: {startScale}, To: {targetScale}");
            }
            else
            {
                if (action.resetScaleOnRelease)
                {
                    targetScale = action.originalScale;
                    Debug.Log($"[{gameObject.name}] ScaleObject DOWN - Target: {action.targetObject.name}, From: {startScale}, To: {targetScale}");
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] ScaleObject DOWN - resetScaleOnRelease false, çıkılıyor");
                    yield break;
                }
            }

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * action.scaleSpeed;
                action.targetObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            action.targetObject.transform.localScale = targetScale;
        }

        private IEnumerator RotateObject(ModernButtonAction action, bool rotateUp)
        {
            Quaternion startRot = action.targetObject.transform.rotation;
            Quaternion targetRot;
            if (rotateUp)
            {
                targetRot = action.originalRotation * Quaternion.AngleAxis(action.rotationAngle, action.rotationAxis.normalized);
                Debug.Log($"[{gameObject.name}] RotateObject UP - Target: {action.targetObject.name}, From: {startRot}, To: {targetRot.eulerAngles}");
            }
            else
            {
                if (action.resetRotationOnRelease)
                {
                    targetRot = action.originalRotation;
                    Debug.Log($"[{gameObject.name}] RotateObject DOWN - Target: {action.targetObject.name}, From: {startRot}, To: {targetRot.eulerAngles}");
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] RotateObject DOWN - resetRotationOnRelease false, çıkılıyor");
                    yield break;
                }
            }
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * action.rotationSpeed;
                action.targetObject.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }
            action.targetObject.transform.rotation = targetRot;
        }

        private void OnDrawGizmosSelected()
        {
            // Button press görselleştirmesi
            Gizmos.color = isPressed ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
            
            if (pressDepth > 0)
            {
                Gizmos.color = Color.yellow;
                Vector3 pressedPos = transform.position - new Vector3(0, pressDepth, 0);
                Gizmos.DrawWireCube(pressedPos, transform.localScale);
            }
            
            // Action hedeflerini göster
            foreach (var action in actions)
            {
                if (action.targetObject != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, action.targetObject.transform.position);
                    
                    switch (action.actionType)
                    {
                        case ModernButtonAction.ActionType.Move:
                            Vector3 moveTarget = action.targetObject.transform.position + 
                                               action.moveDirection.normalized * action.moveDistance;
                            Gizmos.color = Color.blue;
                            Gizmos.DrawWireSphere(moveTarget, 0.3f);
                            break;
                    }
                }
            }
        }
    }
} 