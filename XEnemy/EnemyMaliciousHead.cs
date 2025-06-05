using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MaliciousHeads.XEnemy
{
    internal class EnemyMaliciousHead : MonoBehaviour
    {
        public enum State
        {
            Spawn = 0,
            Idle = 1,
            Despawn = 2,
        }

        public bool debugSpawn;

        [Space]
        public State currentState;

        private bool stateImpulse;

        internal float stateTimer;

        [Space]
        public Rigidbody rigidbody;

        [Space]
        public Enemy enemy;

        public PhysGrabObject physGrabObject;

        private PhotonView photonView;

        [Space]
        public MapCustom mapCustom;

        [Space]
        public float holdThreshold;

        private float _holdStartTime;

        private bool _hasSelfDestructed = false;

        private bool wasHoldingLastFrame;

        private AnimationCurve eyeFlashCurve;

        public Light eyeFlashLight;

        public float eyeFlashLightIntensity;

        private Material eyeMaterial;

        private int eyeMaterialAmount;

        private int eyeMaterialColor;

        private bool eyeFlash;

        private float eyeFlashLerp;

        [Space]
        public MeshRenderer headRenderer;

        [Space]
        public MeshRenderer[] eyeRenderers;

        [Space]
        public Color eyeFlashPositiveColor;

        public Color eyeFlashNegativeColor;

        [Space]
        public Sound eyeFlashPositiveSound;

        public Sound eyeFlashNegativeSound;

        [Space]
        public AudioClip seenSound;

        private float seenCooldownTime = 2f;

        private float seenCooldownTimer;

        private bool localSeen;

        private bool localSeenEffect;

        private float localSeenEffectTime = 2f;

        private float localSeenEffectTimer;

        public bool serverSeen;

        private bool setup;

        private void Awake()
        {
            holdThreshold = Settings.HoldThreshold.Value;
            photonView = GetComponent<PhotonView>();
            if (!Application.isEditor || (SemiFunc.IsMultiplayer() && !GameManager.instance.localTest))
            {
                debugSpawn = false;
            }

            foreach (MeshRenderer meshRenderer in eyeRenderers)
            {
                if (!eyeMaterial)
                {
                    eyeMaterial = meshRenderer.material;
                }
                meshRenderer.material = eyeMaterial;
            }
            eyeMaterialAmount = Shader.PropertyToID("_ColorOverlayAmount");
            eyeMaterialColor = Shader.PropertyToID("_ColorOverlay");
            eyeFlashCurve = AssetManager.instance.animationCurveImpact;

            localSeenEffectTimer = localSeenEffectTime;
        }

        private void Start()
        {
            StartCoroutine(Setup());
        }

        private void Update()
        {
            if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
            {
                switch (currentState)
                {
                    case State.Spawn:
                        StateSpawn();
                        break;
                    case State.Idle:
                        StateIdle();
                        break;
                    case State.Despawn:
                        StateDespawn();
                        break;
                }
            }

            if (!localSeen && !PlayerController.instance.playerAvatarScript.isDisabled)
            {
                if (seenCooldownTimer > 0f)
                {
                    seenCooldownTimer -= Time.deltaTime;
                }
                else
                {
                    Vector3 localCameraPosition = PlayerController.instance.playerAvatarScript.localCameraPosition;
                    float num = Vector3.Distance(base.transform.position, localCameraPosition);
                    if (num <= 10f && SemiFunc.OnScreen(base.transform.position, -0.15f, -0.15f))
                    {
                        Vector3 normalized = (localCameraPosition - base.transform.position).normalized;
                        if (!Physics.Raycast(physGrabObject.centerPoint, normalized, out var _, num, LayerMask.GetMask("Default")))
                        {
                            localSeen = true;
                            TutorialDirector.instance.playerSawHead = true;
                            if (!serverSeen && SemiFunc.RunIsLevel())
                            {
                                if (SemiFunc.IsMultiplayer())
                                {
                                    photonView.RPC("SeenSetRPC", RpcTarget.All, true);
                                }
                                else
                                {
                                    SeenSetRPC(true);
                                }
                                if (PlayerController.instance.deathSeenTimer <= 0f)
                                {
                                    localSeenEffect = true;
                                    PlayerController.instance.deathSeenTimer = 30f;
                                    GameDirector.instance.CameraImpact.Shake(2f, 0.5f);
                                    GameDirector.instance.CameraShake.Shake(2f, 1f);
                                    AudioScare.instance.PlayCustom(seenSound, 0.3f, 60f);
                                    ValuableDiscover.instance.New(physGrabObject, ValuableDiscoverGraphic.State.Bad);
                                }
                            }
                        }
                    }
                }
            }
            if (localSeenEffect)
            {
                localSeenEffectTimer -= Time.deltaTime;
                CameraZoom.Instance.OverrideZoomSet(75f, 0.1f, 0.25f, 0.25f, base.gameObject, 150);
                PostProcessing.Instance.VignetteOverride(Color.black, 0.4f, 1f, 1f, 0.5f, 0.1f, base.gameObject);
                PostProcessing.Instance.SaturationOverride(-50f, 1f, 0.5f, 0.1f, base.gameObject);
                PostProcessing.Instance.ContrastOverride(5f, 1f, 0.5f, 0.1f, base.gameObject);
                GameDirector.instance.CameraImpact.Shake(10f * Time.deltaTime, 0.1f);
                GameDirector.instance.CameraShake.Shake(10f * Time.deltaTime, 1f);
                if (localSeenEffectTimer <= 0f)
                {
                    localSeenEffect = false;
                }
            }

            bool isLocalPlayerHolding = false;

            foreach (var physGrabber in physGrabObject.playerGrabbing)
            {
                if (SemiFunc.PlayerGetSteamID(physGrabber.playerAvatar) == SemiFunc.PlayerGetSteamID(PlayerAvatar.instance))
                {
                    isLocalPlayerHolding = true;
                    break;
                }
            }

            if (eyeFlash)
            {
                eyeFlashLerp += holdThreshold * Time.deltaTime;
                eyeFlashLerp = Mathf.Clamp01(eyeFlashLerp);
                eyeMaterial.SetFloat(eyeMaterialAmount, eyeFlashCurve.Evaluate(eyeFlashLerp));
                eyeFlashLight.intensity = eyeFlashCurve.Evaluate(eyeFlashLerp) * eyeFlashLightIntensity;
                if (eyeFlashLerp > 1f)
                {
                    eyeFlash = false;
                    eyeMaterial.SetFloat(eyeMaterialAmount, 0f);
                    eyeFlashLight.gameObject.SetActive(value: false);
                }
            }

            if (isLocalPlayerHolding)
            {
                if (!wasHoldingLastFrame)
                {
                    _holdStartTime = Time.time;
                    _hasSelfDestructed = false;
                    FlashEye();
                }

                float heldDuration = Time.time - _holdStartTime;

                if (!_hasSelfDestructed && heldDuration >= holdThreshold)
                {
                    FlashEye();
                    ChatManager.instance.PossessSelfDestruction();
                    _hasSelfDestructed = true;
                }
            }
            else
            {

                _holdStartTime = -1f;
                _hasSelfDestructed = false;
            }

            wasHoldingLastFrame = isLocalPlayerHolding;
        }

        private IEnumerator Setup()
        {
            while (!LevelGenerator.Instance.Generated)
            {
                yield return new WaitForSeconds(0.1f);
            }
            if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
            {
                string randomPlayerSteamID = GetRandomPlayerSteamID();
                if (GameManager.Multiplayer())
                {
                    photonView.RPC("SetupRPC", RpcTarget.OthersBuffered, randomPlayerSteamID, Settings.HoldThreshold.Value);
                }
                SetupDone(randomPlayerSteamID, holdThreshold);
                if (SemiFunc.RunIsArena())
                {
                    physGrabObject.impactDetector.destroyDisable = false;
                }
                setup = true;
            }
        }

        public void UpdateState(State _state)
        {
            if (currentState != _state)
            {
                currentState = _state;
                stateImpulse = true;
                stateTimer = 0f;
                if (GameManager.Multiplayer())
                {
                    photonView.RPC("UpdateStateRPC", RpcTarget.All, currentState);
                }
                else
                {
                    UpdateStateRPC(currentState);
                }
            }
        }

        [PunRPC]
        public void SetupRPC(string steamID, float holdThreshold)
        {
            MaliciousHeads.Logger.LogDebug($"[RPC] SetupRPC called with parameter steamID: {steamID}");
            StartCoroutine(Wrap(SetupClient(steamID, holdThreshold)));
        }

        private IEnumerator SetupClient(string steamID, float holdThreshold)
        {
            while (!physGrabObject)
            {
                MaliciousHeads.Logger.LogWarning("No Phys Grab Object");
                yield return new WaitForSeconds(0.1f);
            }
            while (!physGrabObject.impactDetector)
            {
                MaliciousHeads.Logger.LogWarning("No Impact Detector");
                yield return new WaitForSeconds(0.1f);
            }
            while (!physGrabObject.impactDetector.particles)
            {
                MaliciousHeads.Logger.LogWarning("No Impact Detector Particles");
                yield return new WaitForSeconds(0.1f);
            }
            MaliciousHeads.Logger.LogDebug($"SetupClient finished: {steamID}");
            SetupDone(steamID, holdThreshold);
        }

        private string GetRandomPlayerSteamID()
        {
            PlayerAvatar player = GameDirector.instance.PlayerList[UnityEngine.Random.Range(0, GameDirector.instance.PlayerList.Count)];
            return SemiFunc.PlayerGetSteamID(player);
        }

        private void SetupDone(string steamID, float holdThreshold)
        {
            this.holdThreshold = holdThreshold;
            if (steamID == null)
            {
                Debug.LogError("Failed to set Malicious Head color");
                return;
            }

            PlayerAvatar playerAvatar = SemiFunc.PlayerGetFromSteamID(steamID);

            if (steamID == null)
            {
                Debug.LogError("Failed to set Malicious Head color: No player avatar");
                return;
            }

            headRenderer.material = playerAvatar.playerHealth.bodyMaterial;
            headRenderer.material.SetFloat(Shader.PropertyToID("_ColorOverlayAmount"), 0f);

            //if (SemiFunc.IsMultiplayer() && playerAvatar == SessionManager.instance.CrownedPlayerGet())
            //{
            //    arenaCrown.SetActive(value: true);
            //}
        }

        public void FlashEye()
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("FlashEyeRPC", RpcTarget.All, false);
            }
            FlashEyeRPC(false);
        }

        [PunRPC]
        public void FlashEyeRPC(bool _positive)
        {
            if (_positive)
            {
                eyeMaterial.SetColor(eyeMaterialColor, eyeFlashPositiveColor);
                eyeFlashPositiveSound.Play(base.transform.position);
                eyeFlashLight.color = eyeFlashPositiveColor;
            }
            else
            {
                eyeMaterial.SetColor(eyeMaterialColor, eyeFlashNegativeColor);
                eyeFlashNegativeSound.Play(base.transform.position);
                eyeFlashLight.color = eyeFlashNegativeColor;
            }
            eyeFlash = true;
            eyeFlashLerp = 0f;
            eyeFlashLight.gameObject.SetActive(value: true);
            GameDirector.instance.CameraImpact.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.25f);
            GameDirector.instance.CameraShake.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.5f);
        }

        private void StateSpawn()
        {
            if (stateImpulse)
            {
                stateImpulse = false;
                stateTimer = 1f;

                ApplyVelocity();
                ApplyAngularVelocity();

                seenCooldownTimer = seenCooldownTime;
                localSeen = false;
                localSeenEffect = false;
                serverSeen = false;
            }
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                UpdateState(State.Idle);
            }
        }

        private void StateIdle()
        {
            if (stateImpulse)
            {
                stateImpulse = false;
                stateTimer = 60f;
            }

            if (physGrabObject.playerGrabbing.Count == 0)
            {
                stateTimer -= Time.deltaTime;
            }
            else if (stateTimer < 2f)
            {
                stateTimer = 2f;
            }

            
            if (stateTimer <= 0f)
            {
                UpdateState(State.Despawn);
            }
        }

        private void StateDespawn()
        {
            if (stateImpulse)
            {
                stateImpulse = false;
                enemy.EnemyParent.Despawn();
            }
        }

        public void OnSpawn()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
            {
                UpdateState(State.Spawn);
            }
        }

        [PunRPC]
        private void UpdateStateRPC(State _state)
        {
            currentState = _state;
            if (currentState == State.Spawn)
            {
                // enemyHunterAnim.OnSpawn();
            }
        }

        [PunRPC]
        private void SeenSetRPC(bool seen)
        {
            MaliciousHeads.Logger.LogDebug($"[RPC] Setting server seen to {seen}");
            serverSeen = seen;
        }

        private static IEnumerator Wrap(IEnumerator ie)
        {
            while (true)
            {
                yield return ie.Current;
                try
                {
                    ie.MoveNext();
                }
                catch (Exception e)
                {
                    MaliciousHeads.Logger.LogError(e.ToString());
                }
            }
        }

        private void ApplyVelocity()
        {
            float xVelocity = UnityEngine.Random.Range(-3f, 3f);
            float yVelocity = UnityEngine.Random.Range(5f, 10f);
            float zVelocity = UnityEngine.Random.Range(-3f, 3f);
            rigidbody.velocity = new Vector3(xVelocity, yVelocity, zVelocity);
        }

        private void ApplyAngularVelocity()
        {
            float xVelocity = UnityEngine.Random.Range(-2f, 2f);
            float yVelocity = UnityEngine.Random.Range(-2f, 2f);
            float zVelocity = UnityEngine.Random.Range(-2f, 2f);
            rigidbody.angularVelocity = new Vector3(xVelocity, yVelocity, zVelocity);
        }
    }
}
