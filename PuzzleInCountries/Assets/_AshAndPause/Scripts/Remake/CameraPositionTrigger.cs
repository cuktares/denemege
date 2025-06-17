using UnityEngine;
using System.Collections;

public class CameraPositionTrigger : MonoBehaviour
{
    [Header("Tetikleme Ayarları")]
    public float triggerDelay = 0.2f;
    
    [Header("Kamera Ayarları")]
    [Tooltip("Kamera X pozisyonunun yönü: Sol için negatif, sağ için pozitif değer girin")]
    public float cameraXOffset = 5f;
    [Tooltip("Kamera geçiş hızını belirler (daha düşük değer = daha hızlı geçiş)")]
    public float transitionSpeed = 1f;
    
    [Header("Debug")]
    public bool isDebugMode = false;
    
    private CinemachineStyleCamera cameraController;
    private Coroutine currentTriggerCoroutine;
    private bool isAlternatePosition = false; // Kamera alternatif pozisyonda mı?
    private float lastTriggerTime = 0; // Son tetikleme zamanı
    private float triggerCooldown = 0.5f; // Tetikleme bekleme süresi
    
    private void Start()
    {
        // Kamera kontrolcüsünü bul
        cameraController = FindObjectOfType<CinemachineStyleCamera>();
        if (cameraController == null)
        {
            Debug.LogError("CameraPositionTrigger: CinemachineStyleCamera bulunamadı!");
        }
        
        // Collider kontrolü
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning("CameraPositionTrigger: Collider bir trigger değil! Is Trigger özelliğini açın.");
            triggerCollider.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            if (isDebugMode) Debug.Log($"{gameObject.name}: Player olmayan nesne tetikledi - {other.name}");
            return;
        }
        
        // Aynı collider'ın çok hızlı tekrar tetiklenmesini önle
        if (Time.time - lastTriggerTime < triggerCooldown)
        {
            if (isDebugMode) Debug.Log($"{gameObject.name}: Tetikleme çok hızlı, bekleniyor...");
            return;
        }
        
        lastTriggerTime = Time.time;
        
        if (cameraController == null)
        {
            Debug.LogError($"{gameObject.name}: CameraController bulunamadı!");
            return;
        }
        
        if (isDebugMode) Debug.Log($"{gameObject.name}: Oyuncu trigger alanına girdi! Mevcut pozisyon durumu: {(isAlternatePosition ? "Alternatif" : "Varsayılan")}");
        
        // Önceki coroutine varsa durdur
        if (currentTriggerCoroutine != null)
        {
            StopCoroutine(currentTriggerCoroutine);
        }
        
        currentTriggerCoroutine = StartCoroutine(TriggerCameraChange());
    }
    
    private IEnumerator TriggerCameraChange()
    {
        if (isDebugMode) Debug.Log($"{gameObject.name}: TriggerCameraChange başlatıldı");
        
        yield return new WaitForSeconds(triggerDelay);
        
        ApplyPositionChange();
        
        if (isDebugMode) Debug.Log($"{gameObject.name}: TriggerCameraChange tamamlandı, kamera {(isAlternatePosition ? "alternatif" : "varsayılan")} pozisyona geçti");
    }
    
    // Pozisyon değişikliği işlemi
    private void ApplyPositionChange()
    {
        if (cameraController.defaultPosition != null)
        {
            if (!isAlternatePosition)
            {
                // Varsayılan pozisyondaysa offset değerine geç
                Vector3 currentOffset = cameraController.defaultOffset;
                Vector3 newOffset = new Vector3(cameraXOffset, currentOffset.y, currentOffset.z);
                
                if (isDebugMode) Debug.Log($"{gameObject.name}: Alternatif pozisyona geçiliyor. X offset: {cameraXOffset}");
                
                cameraController.UpdateCameraPosition(newOffset, transitionSpeed);
                isAlternatePosition = true;
            }
            else
            {
                // Alternatif pozisyondaysa varsayılan değere dön
                if (isDebugMode) Debug.Log($"{gameObject.name}: Varsayılan pozisyona dönülüyor.");
                
                cameraController.ResetToDefaultPosition(transitionSpeed);
                isAlternatePosition = false;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Tetikleme alanını görselleştir
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            // Box collider için
            BoxCollider boxCol = col as BoxCollider;
            if (boxCol != null)
            {
                Gizmos.DrawCube(boxCol.center, boxCol.size);
                return;
            }
            
            // Sphere collider için
            SphereCollider sphereCol = col as SphereCollider;
            if (sphereCol != null)
            {
                Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
                return;
            }
            
            // Capsule collider için
            CapsuleCollider capsuleCol = col as CapsuleCollider;
            if (capsuleCol != null)
            {
                // Basit bir kapsül görselleştirme
                Vector3 size = new Vector3(
                    capsuleCol.radius * 2,
                    capsuleCol.height,
                    capsuleCol.radius * 2
                );
                Gizmos.DrawCube(capsuleCol.center, size);
                return;
            }
        }
        
        // Hiç collider yoksa basit bir küp çiz
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(1f, 1f, 1f));
        
        // Kamera pozisyonlarını görselleştir
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
        
        // Kamera yönünü görselleştir
        if (Application.isPlaying && cameraController != null && cameraController.defaultTarget != null)
        {
            Vector3 targetPos = cameraController.defaultTarget.position;
            Vector3 currentOffset = cameraController.defaultOffset;
            Vector3 newOffset = new Vector3(cameraXOffset, currentOffset.y, currentOffset.z);
            
            // Varsayılan pozisyon
            Gizmos.color = isAlternatePosition ? Color.gray : Color.green;
            Vector3 defaultPos = targetPos + currentOffset;
            Gizmos.DrawLine(targetPos, defaultPos);
            Gizmos.DrawWireSphere(defaultPos, 0.3f);
            
            // Alternatif pozisyon
            Gizmos.color = isAlternatePosition ? Color.green : Color.gray;
            Vector3 altPos = targetPos + newOffset;
            Gizmos.DrawLine(targetPos, altPos);
            Gizmos.DrawWireSphere(altPos, 0.3f);
        }
    }
} 