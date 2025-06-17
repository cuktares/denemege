using UnityEngine;
using System.Collections.Generic;
using System.Collections;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PositionTracker))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class EnhancedThirdPersonController : MonoBehaviour
    {
        [Header("Karakter Hareket")]
        [Tooltip("Karakterin hareket hızı m/s")]
        public float MoveSpeed = 2.0f;
        [Tooltip("Karakterin koşu hızı m/s")]
        public float SprintSpeed = 5.335f;
        [Tooltip("Karakterin dönüş hızı")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        [Tooltip("Hızlanma ve yavaşlama")]
        public float SpeedChangeRate = 10.0f;

        [Header("Zıplama ve Yerçekimi")]
        [Tooltip("Zıplama yüksekliği")]
        public float JumpHeight = 1.2f;
        [Tooltip("Yerçekimi değeri")]
        public float Gravity = -15.0f;
        [Tooltip("Zıplama timeout süresi")]
        public float JumpTimeout = 0.50f;
        [Tooltip("Düşme timeout süresi")]
        public float FallTimeout = 0.15f;

        [Header("Zemin Kontrolü")]
        [Tooltip("Karakter yerde mi?")]
        public bool Grounded = true;
        [Tooltip("Zemin kontrol için offset")]
        public float GroundedOffset = -0.14f;
        [Tooltip("Zemin kontrol yarıçapı - CharacterController radius ile aynı olmalı")]
        public float GroundedRadius = 0.28f;
        [Tooltip("Zemin olarak kabul edilen layer'lar")]
        public LayerMask GroundLayers = -1;



        [Header("Işınlanma Sistemi")]
        [Tooltip("Klon işaretçisi prefab")]
        public Transform CloneMarkerPrefab;
        [Tooltip("Klon yaşam süresi")]
        public float cloneLifetime = 5f;
        [Tooltip("Geri sarma hızı")]
        public float rewindSpeed = 10f;
        [Tooltip("Geri sarma penceresi")]
        public float rewindWindow = 3f;

        [Header("Nesne Tutma Sistemi")]
        [Tooltip("Tutma noktası")]
        public Transform grabPoint;
        [Tooltip("Tutma menzili")]
        public float grabRange = 2f;
        [Tooltip("İtilebilir nesne layer")]
        public LayerMask pushableLayer;



        [Header("Ses Ayarları")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
        public AudioClip CloneCreateSound;
        public AudioClip RewindSound;
        [Range(0, 1)] public float CloneSoundVolume = 0.5f;



        // Temel bileşenler
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private PositionTracker _positionTracker;
        private AudioSource _audioSource;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif

        // Hareket değişkenleri
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;



        // Işınlanma değişkenleri
        private List<GameObject> activeMarkers = new List<GameObject>();
        private bool isRewinding = false;
        private bool hasActiveClone = false;
        private float cloneCreationTime;

        // Nesne tutma değişkenleri
        private PushableObject heldObject;
        private bool isHoldingObject = false;





        // Animasyon ID'leri
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDCrouching;

        private bool _hasAnimator;
        private const float _threshold = 0.01f;



        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _positionTracker = GetComponent<PositionTracker>();
            _audioSource = GetComponent<AudioSource>();

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.volume = CloneSoundVolume;
            }

#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();
            CreateGrabPoint();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;


        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            if (isRewinding) return;

            // Klon süresini kontrol et
            if (hasActiveClone && Time.time - cloneCreationTime > cloneLifetime)
            {
                DestroyClone();
            }
            JumpAndGravity();
            GroundedCheck();
            Move();
            HandleCloneAndRewind();
            HandleGrab();
        }



        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDCrouching = Animator.StringToHash("Crouching");
        }

        private void CreateGrabPoint()
        {
            if (grabPoint == null)
            {
                GameObject grabPointObj = new GameObject("GrabPoint");
                grabPointObj.transform.SetParent(transform);
                grabPointObj.transform.localPosition = new Vector3(0, 1f, 1f);
                grabPoint = grabPointObj.transform;
            }
        }



        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }



        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  (_mainCamera != null ? _mainCamera.transform.eulerAngles.y : 0f);
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void HandleCloneAndRewind()
        {
            if (_input.createClone)
            {
                if (!hasActiveClone)
                {
                    CreateCloneMarker();
                }
                else
                {
                    StartRewind();
                }
                _input.createClone = false;
            }
        }

        private void HandleGrab()
        {
            if (_input.interact)
            {
                if (!isHoldingObject)
                {
                    TryGrabObject();
                }
                else
                {
                    ReleaseObject();
                }
                _input.interact = false;
            }
        }

        private void TryGrabObject()
        {
            Collider[] colliders = Physics.OverlapSphere(grabPoint.position, grabRange, pushableLayer);
            
            if (colliders.Length > 0)
            {
                PushableObject closestObject = null;
                float closestDistance = float.MaxValue;

                foreach (Collider col in colliders)
                {
                    PushableObject pushable = col.GetComponent<PushableObject>();
                    if (pushable != null)
                    {
                        float distance = Vector3.Distance(grabPoint.position, col.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestObject = pushable;
                        }
                    }
                }

                if (closestObject != null)
                {
                    heldObject = closestObject;
                    heldObject.HoldObject(grabPoint);
                    isHoldingObject = true;
                }
            }
        }

        private void ReleaseObject()
        {
            if (heldObject != null)
            {
                heldObject.ReleaseObject();
                heldObject = null;
                isHoldingObject = false;
            }
        }

        private void CreateCloneMarker()
        {
            if (CloneMarkerPrefab == null || hasActiveClone) return;

            // Sadece mevcut pozisyonda tek bir marker oluştur
            GameObject marker = Instantiate(CloneMarkerPrefab.gameObject, transform.position, transform.rotation);
            activeMarkers.Add(marker);

            hasActiveClone = true;
            cloneCreationTime = Time.time;

            // Pozisyon takibini sıfırla ve yeniden başlat
            _positionTracker.ResetPositions();
            _positionTracker.StartRecording();

            if (_audioSource && CloneCreateSound)
            {
                _audioSource.PlayOneShot(CloneCreateSound);
            }
        }

        private void StartRewind()
        {
            if (!hasActiveClone || isRewinding) return;

            var positions = _positionTracker.GetRecordedPositions();
            if (positions.Count == 0) return;

            _positionTracker.StopRecording();
            StartCoroutine(RewindCoroutine());

            if (_audioSource && RewindSound)
            {
                _audioSource.PlayOneShot(RewindSound);
            }
        }

        private IEnumerator RewindCoroutine()
        {
            isRewinding = true;
            _controller.enabled = false;

            var positions = _positionTracker.GetRecordedPositions();
            positions.Reverse(); // En son pozisyondan başlayarak geri git

            foreach (var posData in positions)
            {
                float startTime = Time.time;
                Vector3 startPos = transform.position;
                Quaternion startRot = transform.rotation;
                Vector3 targetPos = posData.position;
                Quaternion targetRot = posData.rotation;

                float journeyLength = Vector3.Distance(startPos, targetPos);
                float distanceCovered = 0;

                while (distanceCovered < journeyLength)
                {
                    float fractionOfJourney = distanceCovered / journeyLength;
                    transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                    transform.rotation = Quaternion.Lerp(startRot, targetRot, fractionOfJourney);

                    distanceCovered = (Time.time - startTime) * rewindSpeed;
                    yield return null;
                }

                // Son pozisyonu tam olarak ayarla
                transform.position = targetPos;
                transform.rotation = targetRot;
            }

            ResetRewind();
        }

        private void ResetRewind()
        {
            isRewinding = false;
            _controller.enabled = true;
            _verticalVelocity = 0f;
            
            DestroyClone();
            _positionTracker.ResetPositions();
            _positionTracker.StartRecording();
        }

        private void DestroyClone()
        {
            foreach (GameObject marker in activeMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
            
            activeMarkers.Clear();
            hasActiveClone = false;
            
            _positionTracker.ResetPositions();
        }





        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);

            // Grab range
            if (grabPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(grabPoint.position, grabRange);
            }
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips != null && FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f && LandingAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}