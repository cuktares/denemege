using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;

public class CinemachineStyleCamera : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public Transform defaultTarget; // Takip edilecek oyuncu
    public float defaultFollowDamping = 1f; // Takip yumuşatma faktörü
    public float defaultLookDamping = 1f; // Bakış yumuşatma faktörü
    public Vector3 defaultOffset = new Vector3(5f, 12f, -18f); // Varsayılan offset (çapraz pozisyon)
    public float fieldOfView = 60f; // Kamera FOV değeri
    public float collisionOffset = 0.2f; // Duvar çarpışma mesafesi
    public LayerMask collisionLayers; // Çarpışma katmanları
    
    [Header("Geçiş Ayarları")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug")]
    public bool debugMode = false;
    
    private Camera mainCamera;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 currentOffset;
    private Coroutine transitionCoroutine;
    private bool isTransitioning = false;
    private StarterAssetsInputs _input;
    
    // Varsayılan pozisyon referansını dışarıdan erişilebilir yapalım
    [HideInInspector]
    public CameraPosition defaultPosition;
    
    [System.Serializable]
    public class CameraPosition
    {
        public Transform target;
        public Vector3 offset;
        public float followDamping;
        public float lookDamping;
    }
    
    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        
        // Eğer hedef belirtilmediyse, Player tag'li objeyi bul
        if (defaultTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                defaultTarget = player.transform;
                Debug.Log("Kamera hedefi otomatik olarak Player tag'li objeye atandı");
            }
            else
            {
                Debug.LogError("Kamera için hedef bulunamadı! Lütfen bir hedef atayın.");
            }
        }
        
        // Varsayılan pozisyonu ayarla
        defaultPosition = new CameraPosition
        {
            target = defaultTarget,
            offset = defaultOffset,
            followDamping = defaultFollowDamping,
            lookDamping = defaultLookDamping
        };
        
        // Başlangıç offset değerini ayarla
        currentOffset = defaultOffset;
        
        // Input sistemini bul
        if (defaultTarget != null)
        {
            _input = defaultTarget.GetComponent<StarterAssetsInputs>();
        }
        
        // Kamerayı başlangıç konumuna yerleştir
        Vector3 targetPos = defaultTarget.position + defaultOffset;
        transform.position = targetPos;
        transform.LookAt(defaultTarget);
        
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = fieldOfView;
        }
    }
    
    private void LateUpdate()
    {
        if (isTransitioning || defaultTarget == null) 
            return;
            
        FollowTarget();
    }
    
    private void FollowTarget()
    {
        // Hedefin dünya konumu
        Vector3 targetWorldPos = defaultTarget.position;
        
        // Hedef pozisyonu, offset değerlerini kullanarak hesapla
        Vector3 targetPosition = targetWorldPos + currentOffset;
        
        // Duvar çarpışma kontrolü
        targetPosition = CheckWallCollision(targetWorldPos, targetPosition);
        
        // Yumuşak takip
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            defaultPosition.followDamping
        );
        
        // Hedefe bakış - Her zaman oyuncuya dönük
        Quaternion targetRotation = Quaternion.LookRotation(targetWorldPos - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * (1f / defaultPosition.lookDamping)
        );
    }
    
    private Vector3 CheckWallCollision(Vector3 targetLookPosition, Vector3 desiredCameraPosition)
    {
        Vector3 direction = (desiredCameraPosition - targetLookPosition).normalized;
        float targetDistance = Vector3.Distance(targetLookPosition, desiredCameraPosition);
        
        RaycastHit hit;
        if (Physics.Raycast(targetLookPosition, direction, out hit, targetDistance, collisionLayers))
        {
            // Duvar engeli tespit edildi, kamerayı engelin önüne getir
            return hit.point - (direction * collisionOffset);
        }
        
        return desiredCameraPosition;
    }
    
    // Yeni kamera pozisyonu için SADECE X offset ayarlama
    public void UpdateCameraPosition(Vector3 newOffset, float transitionTime)
    {
        // X eksenindeki değeri al, Y ve Z eksenindeki değerleri koru
        Vector3 targetOffset = new Vector3(
            newOffset.x,
            defaultOffset.y,
            defaultOffset.z
        );
        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        transitionCoroutine = StartCoroutine(TransitionOffset(targetOffset, transitionTime));
    }
    
    // Varsayılan pozisyona dönüş
    public void ResetToDefaultPosition(float transitionTime)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        transitionCoroutine = StartCoroutine(TransitionOffset(defaultOffset, transitionTime));
    }
    
    // Offset geçişi için coroutine
    private IEnumerator TransitionOffset(Vector3 newOffset, float transitionTime)
    {
        isTransitioning = true;
        
        // Geçiş başlangıç değerleri
        Vector3 startOffset = currentOffset;
        Vector3 targetOffset = newOffset;
        
        if (debugMode)
        {
            Debug.Log($"Kamera offset geçişi başlatılıyor: {startOffset} -> {targetOffset}");
            Debug.Log($"Geçiş süresi: {transitionTime} saniye");
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            // Animasyon eğrisini uygula
            float tCurve = transitionCurve.Evaluate(t);
            
            // Offset için yumuşak geçiş
            currentOffset = Vector3.Lerp(startOffset, targetOffset, tCurve);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Son offset değerini kesin şekilde ayarla
        currentOffset = targetOffset;
        
        if (debugMode)
        {
            Debug.Log($"Kamera offset geçişi tamamlandı. Yeni offset: {currentOffset}");
        }
        
        isTransitioning = false;
    }
    
    private void OnDrawGizmos()
    {
        // Debug görselleştirmeleri
        if (Application.isPlaying && defaultTarget != null)
        {
            // Takip edilen hedef
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(defaultTarget.position, 0.5f);
            
            // Kamera konumu
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, defaultTarget.position);
            
            // İleri vektörü
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
            
            // Offset pozisyonları (çapraz açılar)
            Gizmos.color = Color.cyan;
            Vector3 defaultPos = defaultTarget.position + defaultOffset;
            Vector3 currentPos = defaultTarget.position + currentOffset;
            
            Gizmos.DrawWireSphere(defaultPos, 0.3f);
            Gizmos.DrawWireSphere(currentPos, 0.3f);
        }
    }
} 