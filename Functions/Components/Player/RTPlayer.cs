using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using InControl;
using XInputDotNetPure;
using TMPro;
using DG.Tweening;

using LSFunctions;

using RTFunctions.Functions;
using RTFunctions.Functions.Data.Player;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Components.Player
{
    public class RTPlayer : MonoBehaviour
    {
        //Player Parent Tree (original):
        //player-complete (has Player component)
        //player-complete/Player
        //player-complete/Player/Player (has OnTriggerEnterPass component)
        //player-complete/Player/Player/death-explosion
        //player-complete/Player/Player/burst-explosion
        //player-complete/Player/Player/spawn-implosion
        //player-complete/Player/boost
        //player-complete/trail (has PlayerTrail component)
        //player-complete/trail/1
        //player-complete/trail/2
        //player-complete/trail/3

        public static bool ZenEditorIncludesSolid { get; set; }
        public static bool ShowNameTags { get; set; }

        public static bool PlayBoostSound { get; set; }
        public static bool PlayBoostRecoverSound { get; set; }

        public static bool PlayShootSound { get; set; }

        public static TailUpdateMode UpdateMode { get; set; } = TailUpdateMode.FixedUpdate;

        public static bool AssetsGlobal { get; set; }

        public static bool EvaluateCode { get; set; }

        public static bool AllowPlayersToTakeBulletDamage { get; set; }

        #region Base

        public MyGameActions Actions { get; set; }

        public FaceController faceController;

        public int playerIndex;

        public int initialHealthCount;

        public Coroutine boostCoroutine;

        public GameObject canvas;

        public TextMeshPro textMesh;
        public MeshRenderer healthBase;

        public GameObject health;

        private RectTransform barRT;
        private Image barIm;
        private Image barBaseIm;

        public ParticleSystem burst;
        public ParticleSystem death;
        public ParticleSystem spawn;

        public CustomPlayer CustomPlayer { get; set; }

        public PlayerModel PlayerModel { get; set; }

        #endregion

        #region Bool

        public bool canBoost = true;
        public bool canMove = true;
        public bool canRotate = true;
        public bool canTakeDamage;

        public bool isTakingHit;
        public bool isBoosting;
        public bool isBoostCancelled;
        public bool isDead = true;

        public bool isKeyboard;
        public bool animatingBoost;

        #endregion

        #region Velocities

        public Vector3 lastPos;
        public float lastMoveHorizontal;
        public float lastMoveVertical;
        public Vector3 lastVelocity;

        public Vector2 lastMovement;

        public float startHurtTime;
        public float startBoostTime;
        public float maxBoostTime = 0.18f;
        public float minBoostTime = 0.07f;
        public float boostCooldown = 0.1f;
        public float idleSpeed = 20f;
        public float boostSpeed = 85f;

        public bool includeNegativeZoom = false;
        public MovementMode movementMode = 0;

        public RotateMode rotateMode = RotateMode.RotateToDirection;

        public Vector2 lastMousePos;

        public bool stretch = true;
        public float stretchAmount = 0.1f;
        public int stretchEasing = 6;
        public Vector2 stretchVector = Vector2.zero;

        #endregion

        #region Enums

        public enum RotateMode
        {
            RotateToDirection,
            None,
            FlipX,
            FlipY
        }

        public enum MovementMode
        {
            KeyboardController,
            Mouse
        }

        #endregion

        #region Tail

        public bool tailGrows = false;
        public bool boostTail = false;
        public float tailDistance = 2f;
        public int tailMode;
        public enum TailUpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

        #endregion

        #region Properties

        //public bool CanTakeDamage
        //{
        //    get => (!(EditorManager.inst == null) || DataManager.inst.GetSettingEnum("ArcadeDifficulty", 1) != 0 || !DataManager.inst.GetSettingBool("IsArcade")) && (!(EditorManager.inst == null) || GameManager.inst.gameState != GameManager.State.Paused) && (!(EditorManager.inst != null) || !EditorManager.inst.isEditing) && canTakeDamage;
        //    set => canTakeDamage = value;
        //}

        //public bool CanMove
        //{
        //    get => canMove;
        //    set => canMove = value;
        //}

        //public bool CanBoost
        //{
        //    get => (!(EditorManager.inst != null) || !EditorManager.inst.isEditing) && (canBoost && !isBoosting) && (GameManager.inst == null || GameManager.inst.gameState != GameManager.State.Paused) && !LSHelpers.IsUsingInputField();
        //    set => canBoost = value;
        //}

        //public bool PlayerAlive => (!(InputDataManager.inst != null) || InputDataManager.inst.players.Count > 0) && (InputDataManager.inst != null && InputDataManager.inst.players[playerIndex].health > 0) && !isDead;

        public bool CanTakeDamage
        {
            get => DataManager.inst.GetSettingEnum("ArcadeDifficulty", 1) != 0 && !RTHelpers.Paused && (EditorManager.inst == null || !EditorManager.inst.isEditing) && canTakeDamage;
            set => canTakeDamage = value;
        }

        public bool CanMove
        {
            get => canMove;
            set => canMove = value;
        }

        public bool CanRotate
        {
            get => canRotate;
            set => canRotate = value;
        }

        public bool CanBoost
        {
            get => (EditorManager.inst == null || !EditorManager.inst.isEditing) && canBoost && !isBoosting && !RTHelpers.Paused && !LSHelpers.IsUsingInputField();
            set => canBoost = value;
        }

        public bool PlayerAlive => InputDataManager.inst != null && InputDataManager.inst.players.Count > 0 && InputDataManager.inst != null && CustomPlayer && CustomPlayer.Health > 0 && !isDead;

        #endregion

        #region Delegates

        public delegate void PlayerHitDelegate(int _health, Vector3 _pos);

        public delegate void PlayerHealDelegate(int _health, Vector3 _pos);

        public delegate void PlayerBoostDelegate();

        public delegate void PlayerDeathDelegate(Vector3 _pos);

        public event PlayerHitDelegate playerHitEvent;

        public event PlayerHealDelegate playerHealEvent;

        public event PlayerBoostDelegate playerBoostEvent;

        public event PlayerDeathDelegate playerDeathEvent;

        #endregion

        #region Spawn

        void Awake()
        {
            playerObjects.Add("Base", new PlayerObject("Base", gameObject));
            playerObjects["Base"].values.Add("Transform", gameObject.transform);
            var anim = gameObject.GetComponent<Animator>();
            anim.keepAnimatorControllerStateOnDisable = true;
            playerObjects["Base"].values.Add("Animator", anim);

            var rb = transform.Find("Player").gameObject;
            playerObjects.Add("RB Parent", new PlayerObject("RB Parent", rb));
            playerObjects["RB Parent"].values.Add("Transform", rb.transform);
            playerObjects["RB Parent"].values.Add("Rigidbody2D", rb.GetComponent<Rigidbody2D>());
            playerObjects["RB Parent"].values.Add("OnTriggerEnterPass", rb.GetComponent<OnTriggerEnterPass>());

            var circleCollider = rb.GetComponent<CircleCollider2D>();

            circleCollider.enabled = false;

            var polygonCollider = rb.AddComponent<PolygonCollider2D>();

            playerObjects["RB Parent"].values.Add("CircleCollider2D", circleCollider);
            playerObjects["RB Parent"].values.Add("PolygonCollider2D", polygonCollider);
            playerObjects["RB Parent"].values.Add("PlayerSelector", rb.AddComponent<PlayerSelector>());
            ((PlayerSelector)playerObjects["RB Parent"].values["PlayerSelector"]).id = playerIndex;

            var head = transform.Find("Player/Player").gameObject;
            playerObjects.Add("Head", new PlayerObject("Head", head));

            var headMesh = head.GetComponent<MeshFilter>();

            playerObjects["Head"].values.Add("MeshFilter", headMesh);

            polygonCollider.CreateCollider(headMesh);

            playerObjects["Head"].values.Add("MeshRenderer", head.GetComponent<MeshRenderer>());

            polygonCollider.isTrigger = EditorManager.inst != null && ZenEditorIncludesSolid;
            polygonCollider.enabled = false;
            circleCollider.enabled = true;

            circleCollider.isTrigger = EditorManager.inst != null && ZenEditorIncludesSolid;
            rb.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            DestroyImmediate(rb.GetComponent<OnTriggerEnterPass>());

            var playerCollision = rb.AddComponent<PlayerCollision>();
            playerCollision.player = this;

            var boost = transform.Find("Player/boost").gameObject;
            boost.transform.localScale = Vector3.zero;
            playerObjects.Add("Boost", new PlayerObject("Boost", transform.Find("Player/boost").gameObject));
            playerObjects["Boost"].values.Add("MeshFilter", boost.GetComponent<MeshFilter>());
            playerObjects["Boost"].values.Add("MeshRenderer", boost.GetComponent<MeshRenderer>());

            playerObjects.Add("Tail Parent", new PlayerObject("Tail Parent", transform.Find("trail").gameObject));
            var tail1 = transform.Find("trail/1").gameObject;
            playerObjects.Add("Tail 1", new PlayerObject("Tail 1", tail1));
            var tail2 = transform.Find("trail/2").gameObject;
            playerObjects.Add("Tail 2", new PlayerObject("Tail 2", tail2));
            var tail3 = transform.Find("trail/3").gameObject;
            playerObjects.Add("Tail 3", new PlayerObject("Tail 3", tail3));

            playerObjects["Tail 1"].values.Add("MeshFilter", tail1.GetComponent<MeshFilter>());
            playerObjects["Tail 2"].values.Add("MeshFilter", tail2.GetComponent<MeshFilter>());
            playerObjects["Tail 3"].values.Add("MeshFilter", tail3.GetComponent<MeshFilter>());
            playerObjects["Tail 1"].values.Add("MeshRenderer", tail1.GetComponent<MeshRenderer>());
            playerObjects["Tail 2"].values.Add("MeshRenderer", tail2.GetComponent<MeshRenderer>());
            playerObjects["Tail 3"].values.Add("MeshRenderer", tail3.GetComponent<MeshRenderer>());
            playerObjects["Tail 1"].values.Add("TrailRenderer", tail1.GetComponent<TrailRenderer>());
            playerObjects["Tail 2"].values.Add("TrailRenderer", tail2.GetComponent<TrailRenderer>());
            playerObjects["Tail 3"].values.Add("TrailRenderer", tail3.GetComponent<TrailRenderer>());

            tail1.transform.localPosition = new Vector3(0f, 0f, 0.1f);
            tail2.transform.localPosition = new Vector3(0f, 0f, 0.1f);
            tail3.transform.localPosition = new Vector3(0f, 0f, 0.1f);

            // Set new parents
            {
                // Tail 1
                var trail1Base = new GameObject("Tail 1 Base");
                trail1Base.layer = 8;
                trail1Base.transform.SetParent(transform.Find("trail"));
                transform.Find("trail/1").SetParent(trail1Base.transform);

                playerObjects.Add("Tail 1 Base", new PlayerObject("Tail 1 Base", trail1Base));

                // Tail 2
                var trail2Base = new GameObject("Tail 2 Base");
                trail2Base.layer = 8;
                trail2Base.transform.SetParent(transform.Find("trail"));
                transform.Find("trail/2").SetParent(trail2Base.transform);

                playerObjects.Add("Tail 2 Base", new PlayerObject("Tail 2 Base", trail2Base));

                // Tail 3
                var trail3Base = new GameObject("Tail 3 Base");
                trail3Base.layer = 8;
                trail3Base.transform.SetParent(transform.Find("trail"));
                transform.Find("trail/3").SetParent(trail3Base.transform);

                playerObjects.Add("Tail 3 Base", new PlayerObject("Tail 3 Base", trail3Base));

                // Boost
                var boostBase = new GameObject("Boost Base");
                boostBase.layer = 8;
                boostBase.transform.SetParent(transform.Find("Player"));
                boost.transform.SetParent(boostBase.transform);
                boost.transform.localPosition = Vector3.zero;
                boost.transform.localRotation = Quaternion.identity;

                playerObjects.Add("Boost Base", new PlayerObject("Boost Base", boostBase));

                var boostTail = Instantiate(boostBase);
                boostTail.name = "Boost Tail";
                boostTail.layer = 8;
                boostTail.transform.SetParent(transform.Find("trail"));

                playerObjects.Add("Boost Tail Base", new PlayerObject("Boost Tail Base", boostTail));
                var child = boostTail.transform.GetChild(0);

                bool showBoost = this.boostTail;

                playerObjects.Add("Boost Tail", new PlayerObject("Boost Tail", child.gameObject));
                playerObjects["Boost Tail"].values.Add("MeshRenderer", child.GetComponent<MeshRenderer>());
                playerObjects["Boost Tail"].values.Add("MeshFilter", child.GetComponent<MeshFilter>());

                path.Add(new MovementPath(Vector3.zero, Quaternion.identity, rb.transform));
                path.Add(new MovementPath(Vector3.zero, Quaternion.identity, boostTail.transform, showBoost));
                path.Add(new MovementPath(Vector3.zero, Quaternion.identity, trail1Base.transform));
                path.Add(new MovementPath(Vector3.zero, Quaternion.identity, trail2Base.transform));
                path.Add(new MovementPath(Vector3.zero, Quaternion.identity, trail3Base.transform));
                path.Add(new MovementPath(Vector3.zero, Quaternion.identity, null));
            }

            // Add new stuff
            {
                var delayTarget = new GameObject("tail-tracker");
                delayTarget.transform.SetParent(rb.transform);
                delayTarget.transform.localPosition = new Vector3(-0.5f, 0f, 0.1f);
                delayTarget.transform.localRotation = Quaternion.identity;
                playerObjects.Add("Tail Tracker", new PlayerObject("Tail Tracker", delayTarget));

                var faceBase = new GameObject("face-base");
                faceBase.transform.SetParent(rb.transform);
                faceBase.transform.localPosition = Vector3.zero;
                faceBase.transform.localScale = Vector3.one;

                playerObjects.Add("Face Base", new PlayerObject("Face Base", faceBase));

                var faceParent = new GameObject("face-parent");
                faceParent.transform.SetParent(faceBase.transform);
                faceParent.transform.localPosition = Vector3.zero;
                faceParent.transform.localScale = Vector3.one;
                faceParent.transform.localRotation = Quaternion.identity;
                playerObjects.Add("Face Parent", new PlayerObject("Face Parent", faceParent));

                // PlayerDelayTracker
                var boostDelay = playerObjects["Boost Tail Base"].gameObject.AddComponent<PlayerDelayTracker>();
                boostDelay.leader = delayTarget.transform;
                boostDelay.player = this;
                playerObjects["Boost Tail Base"].values.Add("PlayerDelayTracker", boostDelay);

                for (int i = 1; i < 4; i++)
                {
                    var tail = playerObjects[string.Format("Tail {0} Base", i)].gameObject;
                    var PlayerDelayTracker = tail.AddComponent<PlayerDelayTracker>();
                    PlayerDelayTracker.offset = -i * tailDistance / 2f;
                    PlayerDelayTracker.positionOffset *= (-i + 4);
                    PlayerDelayTracker.player = this;
                    PlayerDelayTracker.leader = delayTarget.transform;
                    playerObjects[string.Format("Tail {0} Base", i)].values.Add("PlayerDelayTracker", PlayerDelayTracker);
                }

                var mat = head.transform.Find("death-explosion").GetComponent<ParticleSystemRenderer>().trailMaterial;

                //Trail
                {
                    var superTrail = new GameObject("super-trail");
                    superTrail.transform.SetParent(head.transform);
                    superTrail.layer = 8;

                    var trailRenderer = superTrail.AddComponent<TrailRenderer>();

                    playerObjects.Add("Head Trail", new PlayerObject("Head Trail", superTrail));
                    playerObjects["Head Trail"].values.Add("TrailRenderer", trailRenderer);

                    trailRenderer.material = mat;
                }

                //Particles
                {
                    var superParticles = new GameObject("super-particles");
                    superParticles.transform.SetParent(head.transform);
                    superParticles.layer = 8;

                    var particleSystem = superParticles.AddComponent<ParticleSystem>();
                    if (!superParticles.GetComponent<ParticleSystemRenderer>())
                    {
                        superParticles.AddComponent<ParticleSystemRenderer>();
                    }

                    var particleSystemRenderer = superParticles.GetComponent<ParticleSystemRenderer>();

                    playerObjects.Add("Head Particles", new PlayerObject("Head Particles", superParticles));
                    playerObjects["Head Particles"].values.Add("ParticleSystem", particleSystem);
                    playerObjects["Head Particles"].values.Add("ParticleSystemRenderer", particleSystemRenderer);

                    var main = particleSystem.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    main.playOnAwake = false;
                    particleSystemRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                    particleSystemRenderer.alignment = ParticleSystemRenderSpace.View;

                    particleSystemRenderer.trailMaterial = mat;
                    particleSystemRenderer.material = mat;
                }

                //Trail
                {
                    var superTrail = new GameObject("boost-trail");
                    superTrail.transform.SetParent(boost.transform.parent);
                    superTrail.layer = 8;

                    var trailRenderer = superTrail.AddComponent<TrailRenderer>();

                    playerObjects.Add("Boost Trail", new PlayerObject("Boost Trail", superTrail));
                    playerObjects["Boost Trail"].values.Add("TrailRenderer", trailRenderer);

                    trailRenderer.material = mat;
                }

                //Boost Particles
                {
                    var superParticles = new GameObject("boost-particles");
                    superParticles.transform.SetParent(boost.transform.parent);
                    superParticles.layer = 8;

                    var particleSystem = superParticles.AddComponent<ParticleSystem>();
                    if (!superParticles.GetComponent<ParticleSystemRenderer>())
                    {
                        superParticles.AddComponent<ParticleSystemRenderer>();
                    }

                    var particleSystemRenderer = superParticles.GetComponent<ParticleSystemRenderer>();

                    playerObjects.Add("Boost Particles", new PlayerObject("Boost Particles", superParticles));
                    playerObjects["Boost Particles"].values.Add("ParticleSystem", particleSystem);
                    playerObjects["Boost Particles"].values.Add("ParticleSystemRenderer", particleSystemRenderer);

                    var main = particleSystem.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    main.loop = false;
                    main.playOnAwake = false;
                    particleSystemRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                    particleSystemRenderer.alignment = ParticleSystemRenderSpace.View;

                    particleSystemRenderer.trailMaterial = mat;
                    particleSystemRenderer.material = mat;
                }

                //Tail Particles
                {
                    for (int i = 1; i < 4; i++)
                    {
                        var superParticles = new GameObject("tail-particles");
                        superParticles.transform.SetParent(playerObjects[string.Format("Tail {0} Base", i)].gameObject.transform);
                        superParticles.layer = 8;

                        var particleSystem = superParticles.AddComponent<ParticleSystem>();
                        if (!superParticles.GetComponent<ParticleSystemRenderer>())
                        {
                            superParticles.AddComponent<ParticleSystemRenderer>();
                        }

                        var particleSystemRenderer = superParticles.GetComponent<ParticleSystemRenderer>();

                        playerObjects.Add(string.Format("Tail {0} Particles", i), new PlayerObject(string.Format("Tail {0} Particles", i), superParticles));
                        playerObjects[string.Format("Tail {0} Particles", i)].values.Add("ParticleSystem", particleSystem);
                        playerObjects[string.Format("Tail {0} Particles", i)].values.Add("ParticleSystemRenderer", particleSystemRenderer);

                        var main = particleSystem.main;
                        main.simulationSpace = ParticleSystemSimulationSpace.World;
                        main.playOnAwake = false;
                        particleSystemRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                        particleSystemRenderer.alignment = ParticleSystemRenderSpace.View;

                        particleSystemRenderer.trailMaterial = mat;
                        particleSystemRenderer.material = mat;
                    }
                }
            }

            health = PlayerManager.healthImages.Duplicate(PlayerManager.healthParent, $"Health {playerIndex}");

            for (int i = 0; i < 3; i++)
            {
                healthObjects.Add(new HealthObject(health.transform.GetChild(i).gameObject, health.transform.GetChild(i).GetComponent<Image>()));
            }

            var barBase = new GameObject("Bar Base");
            barBase.transform.SetParent(health.transform);
            barBase.transform.localScale = Vector3.one;

            var barBaseRT = barBase.AddComponent<RectTransform>();
            var barBaseLE = barBase.AddComponent<LayoutElement>();
            barBaseIm = barBase.AddComponent<Image>();

            barBaseLE.ignoreLayout = true;
            barBaseRT.anchoredPosition = new Vector2(-100f, 0f);
            barBaseRT.pivot = new Vector2(0f, 0.5f);
            barBaseRT.sizeDelta = new Vector2(200f, 32f);

            var bar = new GameObject("Bar");
            bar.transform.SetParent(barBase.transform);
            bar.transform.localScale = Vector3.one;

            barRT = bar.AddComponent<RectTransform>();
            barIm = bar.AddComponent<Image>();

            barRT.pivot = new Vector2(0f, 0.5f);
            barRT.anchoredPosition = new Vector2(-100f, 0f);

            health.SetActive(false);

            burst = playerObjects["Head"].gameObject.transform.Find("burst-explosion").GetComponent<ParticleSystem>();
            death = playerObjects["Head"].gameObject.transform.Find("death-explosion").GetComponent<ParticleSystem>();
            spawn = playerObjects["Head"].gameObject.transform.Find("spawn-implosion").GetComponent<ParticleSystem>();
        }

        public bool playerNeedsUpdating;
        void Start()
        {
            playerHealEvent += UpdateTail;
            playerHitEvent += UpdateTail;
            Spawn();

            if (playerNeedsUpdating)
                UpdatePlayer();
        }

        public void Spawn()
        {
            CanTakeDamage = false;
            CanBoost = false;
            CanMove = false;
            isDead = false;
            isBoosting = false;
            ((Animator)playerObjects["Base"].values["Animator"]).SetTrigger("spawn");
            PlaySpawnParticles();

            EvaluateSpawnCode();

            Debug.LogFormat("{0}Spawned Player {1}", FunctionsPlugin.className, playerIndex);
        }

        #endregion

        #region Update Methods

        void Update()
        {
            if (UpdateMode == TailUpdateMode.Update)
                UpdateTailDistance();

            UpdateCustomTheme(); UpdateBoostTheme(); UpdateSpeeds(); UpdateTrailLengths(); UpdateTheme();
            if (canvas != null)
            {
                bool act = InputDataManager.inst.players.Count > 1 && ShowNameTags;
                canvas.SetActive(act);

                if (act && textMesh != null)
                {
                    textMesh.text = "<#" + LSColors.ColorToHex(GameManager.inst.LiveTheme.playerColors[playerIndex % 4]) + ">Player " + (playerIndex + 1).ToString() + " " + FontManager.TextTranslater.ConvertHealthToEquals(CustomPlayer.Health, initialHealthCount);
                    healthBase.material.color = LSColors.fadeColor(GameManager.inst.LiveTheme.playerColors[playerIndex % 4], 0.3f);
                    healthBase.transform.localScale = new Vector3((float)initialHealthCount * 2.25f, 1.5f, 1f);
                }
            }

            //Anim
            {
                if (GameManager.inst.gameState == GameManager.State.Paused)
                {
                    ((Animator)playerObjects["Base"].values["Animator"]).speed = 0f;
                }
                else if (GameManager.inst.gameState == GameManager.State.Playing)
                {
                    ((Animator)playerObjects["Base"].values["Animator"]).speed = 1f / RTHelpers.Pitch;
                }
            }

            if (!PlayerModel)
                return;

            if (playerObjects["Boost Trail"].values["TrailRenderer"] != null && (bool)PlayerModel.values["Boost Trail Emitting"])
            {
                var tf = playerObjects["Boost"].gameObject.transform;
                Vector2 v = new Vector2(tf.localScale.x, tf.localScale.y);

                ((TrailRenderer)playerObjects["Boost Trail"].values["TrailRenderer"]).startWidth = (float)PlayerModel.values["Boost Trail Start Width"] * v.magnitude / 1.414213f;
                ((TrailRenderer)playerObjects["Boost Trail"].values["TrailRenderer"]).endWidth = (float)PlayerModel.values["Boost Trail End Width"] * v.magnitude / 1.414213f;
            }

            if (!RTHelpers.Paused)
            {
                if (!PlayerAlive && !isDead)
                    StartCoroutine(Kill());

                if (CanMove && PlayerAlive && Actions != null)
                {
                    if (Actions.Boost.WasPressed && CanBoost)
                    {
                        StartBoost();
                        return;
                    }
                    if (isBoosting && !isBoostCancelled && (Actions.Boost.WasReleased || startBoostTime + maxBoostTime <= Time.time))
                    {
                        InitMidBoost(true);
                    }
                }

                if (PlayerAlive && faceController != null && PlayerModel.values.ContainsKey("Bullet Active") && PlayerModel.values.ContainsKey("Bullet Constant"))
                {
                    if (!PlayerModel.values.Get<bool, string>("Bullet Constant") && faceController.Shoot.WasPressed && canShoot ||
                        PlayerModel.values.Get<bool, string>("Bullet Constant") && faceController.Shoot.IsPressed && canShoot)
                        CreateBullet();

                    //if (!(bool)currentModel.values["Bullet Constant"] && faceController.Shoot.WasPressed && canShoot)
                    //{
                    //    CreateBullet();
                    //}
                    //if ((bool)currentModel.values["Bullet Constant"] && faceController.Shoot.IsPressed && canShoot)
                    //{
                    //    CreateBullet();
                    //}
                }
            }
        }

        bool canShoot = true;

        void FixedUpdate()
        {
            if (UpdateMode == TailUpdateMode.FixedUpdate)
                UpdateTailDistance();

            health?.SetActive(PlayerModel && (bool)PlayerModel.values["GUI Health Active"] && GameManager.inst.timeline.activeSelf);
        }

        void LateUpdate()
        {
            if (UpdateMode == TailUpdateMode.LateUpdate)
                UpdateTailDistance();

            UpdateTailTransform(); UpdateTailDev(); UpdateTailSizes(); UpdateControls(); UpdateRotation();

            var player = playerObjects["RB Parent"].gameObject;

            // Here we handle the player's bounds to the camera. It is possible to include negative zoom in those bounds but it might not be a good idea since people have already utilized it.
            if ((!ModCompatibility.sharedFunctions.ContainsKey("EventsCoreEditorOffset") || !(bool)ModCompatibility.sharedFunctions["EventsCoreEditorOffset"]) && GameManager.inst.gameState == GameManager.State.Playing)
            {
                var cameraToViewportPoint = Camera.main.WorldToViewportPoint(player.transform.position);
                cameraToViewportPoint.x = Mathf.Clamp(cameraToViewportPoint.x, 0f, 1f);
                cameraToViewportPoint.y = Mathf.Clamp(cameraToViewportPoint.y, 0f, 1f);
                if (Camera.main.orthographicSize > 0f && (!includeNegativeZoom || Camera.main.orthographicSize < 0f))
                {
                    float maxDistanceDelta = Time.deltaTime * 1500f;
                    player.transform.position = Vector3.MoveTowards(lastPos, Camera.main.ViewportToWorldPoint(cameraToViewportPoint), maxDistanceDelta);
                }
            }

            if (!PlayerModel)
                return;

            if ((bool)PlayerModel.values["Face Control Active"] && faceController != null)
            {
                var vector = new Vector2(faceController.Move.Vector.x, faceController.Move.Vector.y);
                var fp = (Vector2)PlayerModel.values["Face Position"];
                if (vector.magnitude > 1f)
                {
                    vector = vector.normalized;
                }

                if (rotateMode == RotateMode.FlipX && lastMovement.x < 0f)
                    vector.x = -vector.x;
                if (rotateMode == RotateMode.FlipY && lastMovement.y < 0f)
                    vector.y = -vector.y;

                playerObjects["Face Parent"].gameObject.transform.localPosition = new Vector3(vector.x * 0.3f + fp.x, vector.y * 0.3f + fp.y, 0f);
            }

        }

        void UpdateSpeeds()
        {
            if (!PlayerModel)
                return;

            var idl = (float)PlayerModel.values["Base Move Speed"];
            var bst = (float)PlayerModel.values["Base Boost Speed"];
            var bstcldwn = (float)PlayerModel.values["Base Boost Cooldown"];
            var bstmin = (float)PlayerModel.values["Base Min Boost Time"];
            var bstmax = (float)PlayerModel.values["Base Max Boost Time"];

            float pitch = RTHelpers.Pitch;

            idleSpeed = idl;
            boostSpeed = bst;

            boostCooldown = bstcldwn / pitch;
            minBoostTime = bstmin / pitch;
            maxBoostTime = bstmax / pitch;

            var anim = (Animator)playerObjects["Base"].values["Animator"];

            if (GameManager.inst.gameState == GameManager.State.Paused)
                pitch = 0f;

            anim.speed = pitch;
        }

        void UpdateTailDistance()
        {
            path[0].pos = ((Transform)playerObjects["RB Parent"].values["Transform"]).position;
            path[0].rot = ((Transform)playerObjects["RB Parent"].values["Transform"]).rotation;
            for (int i = 1; i < path.Count; i++)
            {
                int num = i - 1;

                if (i == 2 && !path[1].active)
                {
                    num = i - 2;
                }

                if (Vector3.Distance(path[i].pos, path[num].pos) > tailDistance)
                {
                    Vector3 pos = Vector3.Lerp(path[i].pos, path[num].pos, Time.deltaTime * 12f);
                    Quaternion rot = Quaternion.Lerp(path[i].rot, path[num].rot, Time.deltaTime * 12f);

                    if (tailMode == 0)
                    {
                        path[i].pos = pos;
                        path[i].rot = rot;
                    }

                    if (tailMode > 1)
                    {
                        path[i].pos = new Vector3(RTMath.RoundToNearestDecimal(pos.x, 1), RTMath.RoundToNearestDecimal(pos.y, 1), RTMath.RoundToNearestDecimal(pos.z, 1));

                        var r = rot.eulerAngles;

                        path[i].rot = Quaternion.Euler((int)r.x, (int)r.y, (int)r.z);
                    }
                }
            }
        }

        void UpdateTailTransform()
        {
            if (tailMode == 1)
                return;
            if (!RTHelpers.Paused)
            {
                float num = Time.deltaTime * 200f;
                for (int i = 1; i < path.Count; i++)
                {
                    if (InputDataManager.inst.players.Count > 0 && path.Count >= i && path[i].transform != null && path[i].transform.gameObject.activeSelf)
                    {
                        num *= Vector3.Distance(path[i].lastPos, path[i].pos);
                        path[i].transform.position = Vector3.MoveTowards(path[i].lastPos, path[i].pos, num);
                        path[i].lastPos = path[i].transform.position;
                        path[i].transform.rotation = path[i].rot;
                    }
                }
            }
        }

        void UpdateTailDev()
        {
            if (tailMode != 1)
                return;
            if (!RTHelpers.Paused)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    int num = i;
                    if (boostTail && path[1].active)
                    {
                        num += 1;
                    }

                    if (i == 1)
                    {
                        var PlayerDelayTracker = (PlayerDelayTracker)playerObjects["Boost Tail Base"].values["PlayerDelayTracker"];
                        //if (rotateMode != RotateMode.FlipX || rotateMode == RotateMode.FlipX && lastMovement.x > 0f)
                        PlayerDelayTracker.offset = -i * tailDistance / 2f;
                        //else if (rotateMode == RotateMode.FlipX && lastMovement.x < 0)
                        //    PlayerDelayTracker.offset = -(-i * tailDistance / 2f);
                        PlayerDelayTracker.positionOffset = 0.1f * (-i + 5);
                        PlayerDelayTracker.rotationOffset = 0.1f * (-i + 5);
                    }

                    if (playerObjects.ContainsKey(string.Format("Tail {0} Base", i)))
                    {
                        var PlayerDelayTracker = (PlayerDelayTracker)playerObjects[string.Format("Tail {0} Base", i)].values["PlayerDelayTracker"];
                        //if (rotateMode != RotateMode.FlipX || rotateMode == RotateMode.FlipX && lastMovement.x > 0f)
                        PlayerDelayTracker.offset = -num * tailDistance / 2f;
                        //else if (rotateMode == RotateMode.FlipX && lastMovement.x < 0)
                        //    PlayerDelayTracker.offset = -(-num * tailDistance / 2f);
                        PlayerDelayTracker.positionOffset = 0.1f * (-num + 5);
                        PlayerDelayTracker.rotationOffset = 0.1f * (-num + 5);
                    }
                }
            }
        }

        void UpdateTailSizes()
        {
            if (!PlayerModel)
                return;

            for (int i = 1; i < 4; i++)
            {
                var t2 = (Vector2)PlayerModel.values[string.Format("Tail {0} Scale", i)];

                playerObjects[string.Format("Tail {0}", i)].gameObject.transform.localScale = new Vector3(t2.x, t2.y, 1f);
            }
        }

        void UpdateTrailLengths()
        {
            if (!PlayerModel)
                return;

            var headTrail = (TrailRenderer)playerObjects["Head Trail"].values["TrailRenderer"];
            var boostTrail = (TrailRenderer)playerObjects["Boost Trail"].values["TrailRenderer"];

            headTrail.time = (float)PlayerModel.values["Head Trail Time"] / RTHelpers.Pitch;
            boostTrail.time = (float)PlayerModel.values["Boost Trail Time"] / RTHelpers.Pitch;

            for (int i = 1; i < 4; i++)
            {
                var tailTrail = (TrailRenderer)playerObjects[string.Format("Tail {0}", i)].values["TrailRenderer"];

                tailTrail.time = (float)PlayerModel.values[string.Format("Tail {0} Trail Time", i)] / RTHelpers.Pitch;
            }
        }

        void UpdateControls()
        {
            if (!CustomPlayer || !PlayerModel)
                return;

            var anim = (Animator)playerObjects["Base"].values["Animator"];
            var player = playerObjects["RB Parent"].gameObject;
            var rb = (Rigidbody2D)playerObjects["RB Parent"].values["Rigidbody2D"];
            if (PlayerAlive && Actions != null && CustomPlayer.active && CanMove && !RTHelpers.Paused && !LSHelpers.IsUsingInputField() && movementMode == MovementMode.KeyboardController && (!ModCompatibility.sharedFunctions.ContainsKey("EventsCoreEditorOffset") || !(bool)ModCompatibility.sharedFunctions["EventsCoreEditorOffset"]))
            {
                float x = Actions.Move.Vector.x;
                float y = Actions.Move.Vector.y;
                if (x != 0f)
                {
                    lastMoveHorizontal = x;
                    if (y == 0f)
                        lastMoveVertical = 0f;
                }
                if (y != 0f)
                {
                    lastMoveVertical = y;
                    if (x == 0f)
                        lastMoveHorizontal = 0f;
                }

                var pitch = RTHelpers.Pitch;

                Vector3 vector;
                if (isBoosting)
                {
                    vector = new Vector3(lastMoveHorizontal, lastMoveVertical, 0f);
                    vector = vector.normalized;

                    rb.velocity = vector * boostSpeed * pitch;
                    if (stretch && rb.velocity.magnitude > 0f)
                    {
                        float e = 1f + rb.velocity.magnitude * stretchAmount / 20f;
                        player.transform.localScale = new Vector3(1f * e + stretchVector.x, 1f / e + stretchVector.y, 1f);
                    }
                }
                else
                {
                    vector = new Vector3(x, y, 0f);
                    if (vector.magnitude > 1f)
                        vector = vector.normalized;

                    var sp = (bool)PlayerModel.values["Base Sprint Sneak Active"] ? faceController.Sprint.IsPressed ? 1.3f : faceController.Sneak.IsPressed ? 0.1f : 1f : 1f;

                    rb.velocity = vector * idleSpeed * pitch * sp;
                    if (stretch && rb.velocity.magnitude > 0f)
                    {
                        if (rotateMode != RotateMode.None && rotateMode != RotateMode.FlipX)
                        {
                            float e = 1f + rb.velocity.magnitude * stretchAmount / 20f;
                            player.transform.localScale = new Vector3(1f * e + stretchVector.x, 1f / e + stretchVector.y, 1f);
                        }

                        // I really need to figure out how to get stretching to work with non-RotateMode.RotateToDirection. One solution is to setup an additional parent that can be used to stretch, but not sure about doing that atm.
                        if (rotateMode == RotateMode.None || rotateMode == RotateMode.FlipX || rotateMode == RotateMode.FlipY)
                        {
                            float e = 1f + rb.velocity.magnitude * stretchAmount / 20f;

                            float xm = lastMoveHorizontal;
                            if (xm > 0f)
                                xm = -xm;

                            float ym = lastMoveVertical;
                            if (ym > 0f)
                                ym = -ym;

                            float xt = 1f * e + ym + stretchVector.x;
                            float yt = 1f * e + xm + stretchVector.y;

                            if (rotateMode == RotateMode.FlipX)
                            {
                                if (lastMovement.x > 0f)
                                    player.transform.localScale = new Vector3(xt, yt, 1f);
                                if (lastMovement.x < 0f)
                                    player.transform.localScale = new Vector3(-xt, yt, 1f);
                            }

                            if (rotateMode == RotateMode.FlipY)
                            {
                                if (lastMovement.y > 0f)
                                    player.transform.localScale = new Vector3(xt, yt, 1f);
                                if (lastMovement.y < 0f)
                                    player.transform.localScale = new Vector3(xt, -yt, 1f);
                            }
                            if (rotateMode == RotateMode.None)
                                player.transform.localScale = new Vector3(xt, yt, 1f);
                        }
                    }
                    else if (stretch)
                    {
                        float xt = 1f + stretchVector.x;
                        float yt = 1f + stretchVector.y;

                        if (rotateMode == RotateMode.FlipX)
                        {
                            if (lastMovement.x > 0f)
                                player.transform.localScale = new Vector3(xt, yt, 1f);
                            if (lastMovement.x < 0f)
                                player.transform.localScale = new Vector3(-xt, yt, 1f);
                        }
                        if (rotateMode == RotateMode.FlipY)
                        {
                            if (lastMovement.y > 0f)
                                player.transform.localScale = new Vector3(xt, yt, 1f);
                            if (lastMovement.y < 0f)
                                player.transform.localScale = new Vector3(xt, -yt, 1f);
                        }
                        if (rotateMode == RotateMode.None)
                        {
                            player.transform.localScale = new Vector3(xt, yt, 1f);
                        }
                    }
                }
                anim.SetFloat("Speed", Mathf.Abs(vector.x + vector.y));

                if (rb.velocity != Vector2.zero)
                    lastVelocity = rb.velocity;
            }
            else if (CanMove)
            {
                rb.velocity = Vector3.zero;
            }

            if (PlayerAlive && CustomPlayer.active && CanMove && !RTHelpers.Paused && !LSHelpers.IsUsingInputField() && movementMode == MovementMode.Mouse && (EditorManager.inst == null || !EditorManager.inst.isEditing) && Application.isFocused && isKeyboard && (!ModCompatibility.sharedFunctions.ContainsKey("EventsCoreEditorOffset") || !(bool)ModCompatibility.sharedFunctions["EventsCoreEditorOffset"]))
            {
                Vector2 screenCenter = new Vector2(1920 / 2 * (int)EditorManager.inst.ScreenScale, 1080 / 2 * (int)EditorManager.inst.ScreenScale);
                Vector2 mousePos = new Vector2(System.Windows.Forms.Cursor.Position.X - screenCenter.x, -(System.Windows.Forms.Cursor.Position.Y - (screenCenter.y * 2)) - screenCenter.y);

                if (lastMousePos != new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y))
                {
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)screenCenter.x, (int)screenCenter.y);
                }

                var mousePosition = Input.mousePosition;
                mousePosition = Camera.main.WorldToScreenPoint(mousePosition);

                float num = idleSpeed * 0.00025f;
                if (isBoosting)
                    num = boostSpeed * 0.0001f;

                //player.transform.position += new Vector3(mousePos.x * num, mousePos.y * num, 0f);
                player.transform.localPosition = new Vector3(mousePosition.x, mousePosition.y, 0f);
                lastMousePos = new Vector2(mousePosition.x, mousePosition.y);
            }

        }

        void UpdateRotation()
        {
            var player = playerObjects["RB Parent"].gameObject;

            if (CanRotate)
            {
                if (rotateMode != RotateMode.RotateToDirection)
                {
                    var b = Quaternion.AngleAxis(Mathf.Atan2(lastMovement.y, lastMovement.x) * 57.29578f, player.transform.forward);

                    var c = Quaternion.Slerp(player.transform.rotation, b, 720f * Time.deltaTime).eulerAngles;

                    if (rotateMode == RotateMode.FlipX && c.z > 90f && c.z < 270f)
                    {
                        c.z = -c.z + 180f;
                    }
                    if (rotateMode == RotateMode.FlipY && c.z > 0f && c.z < 180f)
                    {
                        c.z = -c.z + 90f;
                    }

                    playerObjects["Face Base"].gameObject.transform.rotation = Quaternion.Euler(c);
                }

                if (rotateMode == RotateMode.RotateToDirection)
                {
                    Quaternion b = Quaternion.AngleAxis(Mathf.Atan2(lastMovement.y, lastMovement.x) * 57.29578f, player.transform.forward);
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, b, 720f * Time.deltaTime);

                    playerObjects["Face Base"].gameObject.transform.localRotation = Quaternion.identity;
                }

                if (rotateMode == RotateMode.FlipX)
                {
                    player.transform.rotation = Quaternion.identity;
                    if (lastMovement.x > 0.001f)
                    {
                        if (!stretch)
                            player.transform.localScale = Vector3.one;
                        if (!animatingBoost)
                            playerObjects["Boost Tail Base"].gameObject.transform.localScale = Vector3.one;
                        playerObjects["Tail 1 Base"].gameObject.transform.localScale = Vector3.one;
                        playerObjects["Tail 2 Base"].gameObject.transform.localScale = Vector3.one;
                        playerObjects["Tail 3 Base"].gameObject.transform.localScale = Vector3.one;
                    }
                    if (lastMovement.x < -0.001f)
                    {
                        var c = new Vector3(-1f, 1f, 1f);
                        if (!stretch)
                            player.transform.localScale = c;
                        if (!animatingBoost)
                            playerObjects["Boost Tail Base"].gameObject.transform.localScale = c;
                        playerObjects["Tail 1 Base"].gameObject.transform.localScale = c;
                        playerObjects["Tail 2 Base"].gameObject.transform.localScale = c;
                        playerObjects["Tail 3 Base"].gameObject.transform.localScale = c;
                    }
                }
                else if (rotateMode == RotateMode.FlipY)
                {
                    player.transform.rotation = Quaternion.identity;
                    if (lastMovement.y > 0.001f)
                    {
                        if (!stretch)
                            player.transform.localScale = Vector3.one;
                        if (!animatingBoost)
                            playerObjects["Boost Tail Base"].gameObject.transform.localScale = Vector3.one;
                        playerObjects["Tail 1 Base"].gameObject.transform.localScale = Vector3.one;
                        playerObjects["Tail 2 Base"].gameObject.transform.localScale = Vector3.one;
                        playerObjects["Tail 3 Base"].gameObject.transform.localScale = Vector3.one;
                    }
                    if (lastMovement.y < -0.001f)
                    {
                        var c = new Vector3(1f, -1f, 1f);
                        if (!stretch)
                            player.transform.localScale = c;
                        if (!animatingBoost)
                            playerObjects["Boost Tail Base"].gameObject.transform.localScale = c;
                        playerObjects["Tail 1 Base"].gameObject.transform.localScale = c;
                        playerObjects["Tail 2 Base"].gameObject.transform.localScale = c;
                        playerObjects["Tail 3 Base"].gameObject.transform.localScale = c;
                    }
                }

                if (rotateMode == RotateMode.None)
                {
                    player.transform.rotation = Quaternion.identity;
                }
            }

            //var posCalc = (player.transform.position - lastPos) * 50.2008f;
            var posCalc = (player.transform.position - lastPos);

            if (posCalc.x < -0.001f || posCalc.x > 0.001f || posCalc.y < -0.001f || posCalc.y > 0.001f)
            {
                lastMovement = posCalc;
            }

            lastPos = player.transform.position;

            var dfs = player.transform.localPosition;
            dfs.z = 0f;
            player.transform.localPosition = dfs;
        }

        void UpdateTheme()
        {
            if (!PlayerModel)
                return;

            if (playerObjects.ContainsKey("Head") && playerObjects["Head"].values["MeshRenderer"] != null)
            {
                int col = (int)PlayerModel.values["Head Color"];
                var colHex = (string)PlayerModel.values["Head Custom Color"];
                float alpha = (float)PlayerModel.values["Head Opacity"];

                ((MeshRenderer)playerObjects["Head"].values["MeshRenderer"]).material.color = GetColor(col, alpha, colHex);
            }

            try
            {
                int colStart = (int)PlayerModel.values["Head Color"];
                var colStartHex = (string)PlayerModel.values["Head Custom Color"];
                float alphaStart = (float)PlayerModel.values["Head Opacity"];

                var main1 = playerObjects["Head"].gameObject.transform.Find("burst-explosion").GetComponent<ParticleSystem>().main;
                var main2 = playerObjects["Head"].gameObject.transform.Find("death-explosion").GetComponent<ParticleSystem>().main;
                var main3 = playerObjects["Head"].gameObject.transform.Find("spawn-implosion").GetComponent<ParticleSystem>().main;
                main1.startColor = new ParticleSystem.MinMaxGradient(GetColor(colStart, alphaStart, colStartHex));
                main2.startColor = new ParticleSystem.MinMaxGradient(GetColor(colStart, alphaStart, colStartHex));
                main3.startColor = new ParticleSystem.MinMaxGradient(GetColor(colStart, alphaStart, colStartHex));
            }
            catch
            {

            }

            if (playerObjects.ContainsKey("Boost") && playerObjects["Boost"].values["MeshRenderer"] != null)
            {
                int colStart = (int)PlayerModel.values["Boost Color"];
                var colStartHex = (string)PlayerModel.values["Boost Custom Color"];
                float alphaStart = (float)PlayerModel.values["Boost Opacity"];

                ((MeshRenderer)playerObjects["Boost"].values["MeshRenderer"]).material.color = GetColor(colStart, alphaStart, colStartHex);
            }

            if (playerObjects.ContainsKey("Boost Tail") && playerObjects["Boost Tail"].values["MeshRenderer"] != null)
            {
                int colStart = (int)PlayerModel.values["Tail Boost Color"];
                var colStartHex = (string)PlayerModel.values["Tail Boost Custom Color"];
                float alphaStart = (float)PlayerModel.values["Tail Boost Opacity"];

                ((MeshRenderer)playerObjects["Boost Tail"].values["MeshRenderer"]).material.color = GetColor(colStart, alphaStart, colStartHex);
            }

            //GUI Bar
            {
                int baseCol = (int)PlayerModel.values["GUI Health Base Color"];
                int topCol = (int)PlayerModel.values["GUI Health Top Color"];
                string baseColHex = (string)PlayerModel.values["GUI Health Base Custom Color"];
                string topColHex = (string)PlayerModel.values["GUI Health Top Custom Color"];
                float baseAlpha = (float)PlayerModel.values["GUI Health Base Opacity"];
                float topAlpha = (float)PlayerModel.values["GUI Health Top Opacity"];

                for (int i = 0; i < healthObjects.Count; i++)
                {
                    if (healthObjects[i].image != null)
                    {
                        healthObjects[i].image.color = GetColor(topCol, topAlpha, topColHex);
                    }
                }

                barBaseIm.color = GetColor(baseCol, baseAlpha, baseColHex);
                barIm.color = GetColor(topCol, topAlpha, topColHex);
            }

            for (int i = 1; i < 4; i++)
            {
                int col = (int)PlayerModel.values[$"Tail {i} Color"];
                var colHex = (string)PlayerModel.values[$"Tail {i} Custom Color"];
                float alpha = (float)PlayerModel.values[$"Tail {i} Opacity"];

                int colStart = (int)PlayerModel.values[string.Format("Tail {0} Trail Start Color", i)];
                var colStartHex = (string)PlayerModel.values[string.Format("Tail {0} Trail Start Custom Color", i)];
                float alphaStart = (float)PlayerModel.values[string.Format("Tail {0} Trail Start Opacity", i)];
                int colEnd = (int)PlayerModel.values[string.Format("Tail {0} Trail End Color", i)];
                var colEndHex = (string)PlayerModel.values[string.Format("Tail {0} Trail End Custom Color", i)];
                float alphaEnd = (float)PlayerModel.values[string.Format("Tail {0} Trail End Opacity", i)];

                var psCol = (int)PlayerModel.values[string.Format("Tail {0} Particles Color", i)];
                var psColHex = (string)PlayerModel.values[string.Format("Tail {0} Particles Custom Color", i)];

                var ps = playerObjects[string.Format("Tail {0} Particles", i)].values.Get<ParticleSystem, string>("ParticleSystem");
                var main = ps.main;

                main.startColor = GetColor(psCol, 1f, psColHex);

                ((MeshRenderer)playerObjects[string.Format("Tail {0}", i)].values["MeshRenderer"]).material.color = GetColor(col, alpha, colHex);
                var trailRenderer = playerObjects[string.Format("Tail {0}", i)].values.Get<TrailRenderer, string>("TrailRenderer");

                trailRenderer.startColor = GetColor(colStart, alphaStart, colStartHex);
                trailRenderer.endColor = GetColor(colEnd, alphaEnd, colEndHex);
            }

            if (playerObjects["Head Trail"].values["TrailRenderer"] != null && (bool)PlayerModel.values["Head Trail Emitting"])
            {
                int colStart = (int)PlayerModel.values["Head Trail Start Color"];
                var colStartHex = (string)PlayerModel.values["Head Trail Start Custom Color"];
                float alphaStart = (float)PlayerModel.values["Head Trail Start Opacity"];
                int colEnd = (int)PlayerModel.values["Head Trail End Color"];
                var colEndHex = (string)PlayerModel.values["Head Trail End Custom Color"];
                float alphaEnd = (float)PlayerModel.values["Head Trail End Opacity"];

                var trailRenderer = (TrailRenderer)playerObjects["Head Trail"].values["TrailRenderer"];

                trailRenderer.startColor = GetColor(colStart, alphaStart, colStartHex);
                trailRenderer.endColor = GetColor(colEnd, alphaEnd, colEndHex);
            }
            if (playerObjects["Head Particles"].values["ParticleSystem"] != null && (bool)PlayerModel.values["Head Particles Emitting"])
            {
                var colStart = (int)PlayerModel.values["Head Particles Color"];
                var colStartHex = (string)PlayerModel.values["Head Particles Custom Color"];

                var ps = (ParticleSystem)playerObjects["Head Particles"].values["ParticleSystem"];
                var main = ps.main;

                main.startColor = GetColor(colStart, 1f, colStartHex);
            }
            if (playerObjects["Boost Trail"].values["TrailRenderer"] != null && (bool)PlayerModel.values["Boost Trail Emitting"])
            {
                var colStart = (int)PlayerModel.values["Boost Trail Start Color"];
                var colStartHex = (string)PlayerModel.values["Boost Trail Start Custom Color"];
                var alphaStart = (float)PlayerModel.values["Boost Trail Start Opacity"];
                var colEnd = (int)PlayerModel.values["Boost Trail End Color"];
                var colEndHex = (string)PlayerModel.values["Boost Trail End Custom Color"];
                var alphaEnd = (float)PlayerModel.values["Boost Trail End Opacity"];

                var trailRenderer = (TrailRenderer)playerObjects["Boost Trail"].values["TrailRenderer"];

                trailRenderer.startColor = GetColor(colStart, alphaStart, colStartHex);
                trailRenderer.endColor = GetColor(colEnd, alphaEnd, colEndHex);
            }
            if (playerObjects["Boost Particles"].values["ParticleSystem"] != null && (bool)PlayerModel.values["Boost Particles Emitting"])
            {
                var colStart = (int)PlayerModel.values["Boost Particles Color"];
                var colHex = (string)PlayerModel.values["Boost Particles Custom Color"];

                var ps = (ParticleSystem)playerObjects["Boost Particles"].values["ParticleSystem"];
                var main = ps.main;

                main.startColor = GetColor(colStart, 1f, colHex);
            }
        }

        #endregion

        #region Collision Handlers

        bool CollisionCheck(Collider2D _other) => _other.tag != "Helper" && _other.tag != "Player" && _other.name != $"bullet (Player {playerIndex + 1})";
        bool CollisionCheck(Collider _other) => _other.tag != "Helper" && _other.tag != "Player" && _other.name != $"bullet (Player {playerIndex + 1})";

        public void OnChildTriggerEnter(Collider2D _other)
        {
            if (CanTakeDamage && !isBoosting && CollisionCheck(_other))
                PlayerHit();
        }

        public void OnChildTriggerEnterMesh(Collider _other)
        {
            if (CanTakeDamage && !isBoosting && CollisionCheck(_other))
                PlayerHit();
        }

        public void OnChildTriggerStay(Collider2D _other)
        {
            if (CanTakeDamage && CollisionCheck(_other))
                PlayerHit();
        }

        public void OnChildTriggerStayMesh(Collider _other)
        {
            if (CanTakeDamage && CollisionCheck(_other))
                PlayerHit();
        }

        #endregion

        #region Init

        void PlayerHit()
        {
            var rb = (Rigidbody2D)playerObjects["RB Parent"].values["Rigidbody2D"];
            var anim = (Animator)playerObjects["Base"].values["Animator"];
            var player = playerObjects["RB Parent"].gameObject;

            if (CanTakeDamage && PlayerAlive)
            {
                InitBeforeHit();
                if (PlayerAlive)
                    anim.SetTrigger("hurt");
                if (CustomPlayer)
                {
                    CustomPlayer.Health--;
                    playerHitEvent?.Invoke(CustomPlayer.Health, rb.position);
                }

                EvaluateHitCode();
            }
        }

        IEnumerator BoostCooldownLoop()
        {
            var player = playerObjects["RB Parent"].gameObject;
            var headTrail = (TrailRenderer)playerObjects["Boost Trail"].values["TrailRenderer"];
            if (PlayerModel && (bool)PlayerModel.values["Boost Trail Emitting"])
            {
                headTrail.emitting = false;
            }

            DOTween.To(delegate (float x)
            {
                stretchVector = new Vector2(x, -x);
            }, stretchAmount * 1.5f, 0f, 1.5f).SetEase(DataManager.inst.AnimationList[stretchEasing].Animation);

            yield return new WaitForSeconds(boostCooldown / RTHelpers.Pitch);
            CanBoost = true;
            if (PlayBoostRecoverSound)
            {
                AudioManager.inst.PlaySound("boost_recover");
            }

            if (boostTail)
            {
                path[1].active = true;
                var tweener = playerObjects["Boost Tail Base"].gameObject.transform.DOScale(Vector3.one, 0.1f / RTHelpers.Pitch).SetEase(DataManager.inst.AnimationList[9].Animation);
                tweener.OnComplete(delegate ()
                {
                    animatingBoost = false;
                });
            }
            yield break;
        }

        IEnumerator Kill()
        {
            isDead = true;
            playerDeathEvent?.Invoke(((Rigidbody2D)playerObjects["RB Parent"].values["Rigidbody2D"]).position);
            CustomPlayer.active = false;
            CustomPlayer.health = 0;
            ((Animator)playerObjects["Base"].values["Animator"]).SetTrigger("kill");
            InputDataManager.inst.SetControllerRumble(playerIndex, 1f);
            EvaluateDeathCode();
            yield return new WaitForSecondsRealtime(0.2f);
            Destroy(health);
            Destroy(gameObject);
            InputDataManager.inst.StopControllerRumble(playerIndex);
            yield break;
        }

        public void InitMidSpawn()
        {
            CanMove = true;
            CanBoost = true;
        }

        public void InitAfterSpawn()
        {
            if (boostCoroutine != null)
                StopCoroutine(boostCoroutine);
            CanMove = true;
            CanBoost = true;
            CanTakeDamage = true;
        }

        void StartBoost()
        {
            if (CanBoost && !isBoosting)
            {
                var anim = (Animator)playerObjects["Base"].values["Animator"];

                startBoostTime = Time.time;
                InitBeforeBoost();
                anim.SetTrigger("boost");

                var ps = (ParticleSystem)playerObjects["Boost Particles"].values["ParticleSystem"];
                var emission = ps.emission;

                var headTrail = (TrailRenderer)playerObjects["Boost Trail"].values["TrailRenderer"];

                if (emission.enabled)
                {
                    ps.Play();
                }
                if (PlayerModel && (bool)PlayerModel.values["Boost Trail Emitting"])
                {
                    headTrail.emitting = true;
                }

                if (PlayBoostSound)
                {
                    AudioManager.inst.PlaySound("boost");
                }

                CreatePulse();

                stretchVector = new Vector2(stretchAmount * 1.5f, -(stretchAmount * 1.5f));

                if (boostTail)
                {
                    path[1].active = false;
                    animatingBoost = true;
                    playerObjects["Boost Tail Base"].gameObject.transform.DOScale(Vector3.zero, 0.05f / RTHelpers.Pitch).SetEase(DataManager.inst.AnimationList[2].Animation);
                }

                EvaluateBoostCode();
            }
        }

        public void InitBeforeBoost()
        {
            CanBoost = false;
            isBoosting = true;
            CanTakeDamage = false;
        }

        public void InitMidBoost(bool _forceToNormal = false)
        {
            if (_forceToNormal)
            {
                float num = Time.time - startBoostTime;
                StartCoroutine(BoostCancel((num < minBoostTime) ? (minBoostTime - num) : 0f));
                return;
            }
            isBoosting = false;
            CanTakeDamage = true;
        }

        public IEnumerator BoostCancel(float _offset)
        {
            var anim = (Animator)playerObjects["Base"].values["Animator"];

            isBoostCancelled = true;
            yield return new WaitForSeconds(_offset);
            isBoosting = false;
            if (!isTakingHit)
            {
                CanTakeDamage = true;
                anim.SetTrigger("boost_cancel");
                yield return new WaitForSeconds(0.1f);
                InitAfterBoost();
            }
            else
            {
                float num = (Time.time - startHurtTime) / 2.5f;
                if (num < 1f)
                {
                    anim.Play("Hurt", -1, num);
                }
                else
                {
                    anim.SetTrigger("boost_cancel");
                    InitAfterHit();
                }
                yield return new WaitForSeconds(0.1f);
                InitAfterBoost();
            }
            anim.SetTrigger("boost_cancel");
            yield break;
        }

        //Look into making custom damage offsets
        public IEnumerator DamageSetDelay(float _offset)
        {
            yield return new WaitForSeconds(_offset);
            Debug.LogFormat("{0}Player can now be damaged.", FunctionsPlugin.className);
            CanTakeDamage = true;
            yield break;
        }

        public void InitAfterBoost()
        {
            isBoosting = false;
            isBoostCancelled = false;
            boostCoroutine = StartCoroutine(BoostCooldownLoop());
        }

        public void InitBeforeHit()
        {
            startHurtTime = Time.time;
            CanBoost = true;
            isBoosting = false;
            isTakingHit = true;
            CanTakeDamage = false;

            AudioManager.inst.PlaySound(FunctionsPlugin.Language.Value == FunctionsPlugin.Lang.pirate ? "pirate_KillPlayer" : "HurtPlayer");
        }

        public void InitAfterHit()
        {
            isTakingHit = false;
            CanTakeDamage = true;
        }

        public void ResetMovement()
        {
            if (boostCoroutine != null)
                StopCoroutine(boostCoroutine);

            isBoosting = false;
            CanMove = true;
            CanBoost = true;
        }

        public void PlaySpawnParticles() => spawn?.Play();

        public void PlayDeathParticles() => death?.Play();

        public void PlayHitParticles() => burst?.Play();

        #endregion

        #region Update Values

        public bool updated;

        public void UpdatePlayer()
        {
            if (!PlayerModel)
                return;

            var currentModel = PlayerModel;
            var rb = (Rigidbody2D)playerObjects["RB Parent"].values["Rigidbody2D"];

            //New NameTag
            {
                Destroy(canvas);
                canvas = new GameObject("Name Tag Canvas" + (playerIndex + 1).ToString());
                canvas.transform.SetParent(transform);
                canvas.transform.localScale = Vector3.one;
                canvas.transform.localRotation = Quaternion.identity;

                var bae = Instantiate(ObjectManager.inst.objectPrefabs[0].options[0]);
                bae.transform.SetParent(canvas.transform);
                bae.transform.localScale = Vector3.one;
                bae.transform.localRotation = Quaternion.identity;

                bae.transform.GetChild(0).transform.localScale = new Vector3(6.5f, 1.5f, 1f);
                bae.transform.GetChild(0).transform.localPosition = new Vector3(0f, 2.5f, -0.3f);

                healthBase = bae.GetComponentInChildren<MeshRenderer>();
                healthBase.enabled = true;

                Destroy(bae.GetComponentInChildren<RTFunctions.Functions.Components.RTObject>());
                Destroy(bae.GetComponentInChildren<SelectObjectInEditor>());
                Destroy(bae.GetComponentInChildren<Collider2D>());

                var tae = Instantiate(ObjectManager.inst.objectPrefabs[4].options[0]);
                tae.transform.SetParent(canvas.transform);
                tae.transform.localScale = Vector3.one;
                tae.transform.localRotation = Quaternion.identity;

                tae.transform.GetChild(0).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                tae.transform.GetChild(0).transform.localPosition = new Vector3(0f, 2.5f, -0.3f);

                textMesh = tae.GetComponentInChildren<TextMeshPro>();

                var d = canvas.AddComponent<PlayerDelayTracker>();
                d.leader = playerObjects["RB Parent"].gameObject.transform;
                d.scaleParent = false;
                d.rotationParent = false;
                d.player = this;
                d.positionOffset = 0.9f;
            }

            //Set new transform values
            {
                //Head Shape
                {
                    int s = ((Vector2Int)currentModel.values["Head Shape"]).x;
                    int so = ((Vector2Int)currentModel.values["Head Shape"]).y;

                    s = Mathf.Clamp(s, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                    so = Mathf.Clamp(so, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                    if (s != 4 && s != 6)
                        ((MeshFilter)playerObjects["Head"].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[s].options[so].GetComponentInChildren<MeshFilter>().mesh;
                    else
                        ((MeshFilter)playerObjects["Head"].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[0].options[0].GetComponentInChildren<MeshFilter>().mesh;
                }

                //Boost Shape
                {
                    int s = ((Vector2Int)currentModel.values["Boost Shape"]).x;
                    int so = ((Vector2Int)currentModel.values["Boost Shape"]).y;

                    s = Mathf.Clamp(s, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                    so = Mathf.Clamp(so, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                    if (s != 4 && s != 6)
                        ((MeshFilter)playerObjects["Boost"].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[s].options[so].GetComponentInChildren<MeshFilter>().mesh;
                    else
                        ((MeshFilter)playerObjects["Boost"].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[0].options[0].GetComponentInChildren<MeshFilter>().mesh;
                }

                //Tail Boost Shape
                {
                    int s = ((Vector2Int)currentModel.values["Tail Boost Shape"]).x;
                    int so = ((Vector2Int)currentModel.values["Tail Boost Shape"]).y;

                    s = Mathf.Clamp(s, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                    so = Mathf.Clamp(so, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                    if (s != 4 && s != 6)
                        ((MeshFilter)playerObjects["Boost Tail"].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[s].options[so].GetComponentInChildren<MeshFilter>().mesh;
                    else
                        ((MeshFilter)playerObjects["Boost Tail"].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[0].options[0].GetComponentInChildren<MeshFilter>().mesh;
                }

                //Tail 1 Shape
                for (int i = 1; i < 4; i++)
                {
                    int s = ((Vector2Int)currentModel.values[string.Format("Tail {0} Shape", i)]).x;
                    int so = ((Vector2Int)currentModel.values[string.Format("Tail {0} Shape", i)]).y;

                    s = Mathf.Clamp(s, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                    so = Mathf.Clamp(so, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                    if (s != 4 && s != 6)
                        ((MeshFilter)playerObjects[string.Format("Tail {0}", i)].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[s].options[so].GetComponentInChildren<MeshFilter>().mesh;
                    else
                        ((MeshFilter)playerObjects[string.Format("Tail {0}", i)].values["MeshFilter"]).mesh = ObjectManager.inst.objectPrefabs[0].options[0].GetComponentInChildren<MeshFilter>().mesh;
                }

                var h1 = (Vector2)currentModel.values["Head Position"];
                var h2 = (Vector2)currentModel.values["Head Scale"];
                var h3 = (float)currentModel.values["Head Rotation"];

                playerObjects["Head"].gameObject.transform.localPosition = new Vector3(h1.x, h1.y, 0f);
                playerObjects["Head"].gameObject.transform.localScale = new Vector3(h2.x, h2.y, 1f);
                playerObjects["Head"].gameObject.transform.localEulerAngles = new Vector3(0f, 0f, h3);

                var b1 = (Vector2)currentModel.values["Boost Position"];
                var b2 = (Vector2)currentModel.values["Boost Scale"];
                var b3 = (float)currentModel.values["Boost Rotation"];

                ((MeshRenderer)playerObjects["Boost"].values["MeshRenderer"]).enabled = (bool)currentModel.values["Boost Active"];
                playerObjects["Boost Base"].gameObject.transform.localPosition = new Vector3(b1.x, b1.y, 0.1f);
                playerObjects["Boost Base"].gameObject.transform.localScale = new Vector3(b2.x, b2.y, 1f);
                playerObjects["Boost Base"].gameObject.transform.localEulerAngles = new Vector3(0f, 0f, b3);

                tailDistance = (float)currentModel.values["Tail Base Distance"];
                tailMode = (int)currentModel.values["Tail Base Mode"];

                tailGrows = (bool)currentModel.values["Tail Base Grows"];

                boostTail = (bool)currentModel.values["Tail Boost Active"];

                playerObjects["Boost Tail Base"].gameObject.SetActive(boostTail);

                var fp = (Vector2)currentModel.values["Face Position"];
                playerObjects["Face Parent"].gameObject.transform.localPosition = new Vector3(fp.x, fp.y, 0f);

                if (!isBoosting)
                {
                    path[1].active = boostTail;
                }

                //Stretch
                {
                    stretch = (bool)currentModel.values["Stretch Active"];
                    stretchAmount = (float)currentModel.values["Stretch Amount"];
                    stretchEasing = (int)currentModel.values["Stretch Easing"];
                }

                var bt1 = (Vector2)currentModel.values["Tail Boost Position"];
                var bt2 = (Vector2)currentModel.values["Tail Boost Scale"];
                var bt3 = (float)currentModel.values["Tail Boost Rotation"];

                playerObjects["Boost Tail"].gameObject.SetActive(boostTail);
                if (boostTail)
                {
                    playerObjects["Boost Tail"].gameObject.transform.localPosition = new Vector3(bt1.x, bt1.y, 0.1f);
                    playerObjects["Boost Tail"].gameObject.transform.localScale = new Vector3(bt2.x, bt2.y, 1f);
                    playerObjects["Boost Tail"].gameObject.transform.localEulerAngles = new Vector3(0f, 0f, bt3);
                }

                rotateMode = (RotateMode)(int)currentModel.values["Base Rotate Mode"];

                ((CircleCollider2D)playerObjects["RB Parent"].values["CircleCollider2D"]).isTrigger = EditorManager.inst != null && ZenEditorIncludesSolid;
                ((PolygonCollider2D)playerObjects["RB Parent"].values["PolygonCollider2D"]).isTrigger = EditorManager.inst != null && ZenEditorIncludesSolid;

                var colAcc = (bool)currentModel.values["Base Collision Accurate"];
                if (colAcc)
                {
                    ((CircleCollider2D)playerObjects["RB Parent"].values["CircleCollider2D"]).enabled = false;
                    ((PolygonCollider2D)playerObjects["RB Parent"].values["PolygonCollider2D"]).enabled = true;
                    ((PolygonCollider2D)playerObjects["RB Parent"].values["PolygonCollider2D"]).CreateCollider((MeshFilter)playerObjects["Head"].values["MeshFilter"]);
                }
                else
                {
                    ((PolygonCollider2D)playerObjects["RB Parent"].values["PolygonCollider2D"]).enabled = false;
                    ((CircleCollider2D)playerObjects["RB Parent"].values["CircleCollider2D"]).enabled = true;
                }

                for (int i = 1; i < 4; i++)
                {
                    //int num = i;
                    //if (boostTail)
                    //{
                    //    num += 1;
                    //}

                    //var PlayerDelayTracker = (PlayerDelayTracker)playerObjects[string.Format("Tail {0} Base", i)].values["PlayerDelayTracker"];
                    //PlayerDelayTracker.offset = -num * tailDistance / 2f;
                    //PlayerDelayTracker.followSharpness = 0.1f * (-num + 4);

                    var t1 = (Vector2)currentModel.values[string.Format("Tail {0} Position", i)];
                    var t2 = (Vector2)currentModel.values[string.Format("Tail {0} Scale", i)];
                    var t3 = (float)currentModel.values[string.Format("Tail {0} Rotation", i)];

                    ((MeshRenderer)playerObjects[string.Format("Tail {0}", i)].values["MeshRenderer"]).enabled = (bool)currentModel.values[string.Format("Tail {0} Active", i)];
                    playerObjects[string.Format("Tail {0}", i)].gameObject.transform.localPosition = new Vector3(t1.x, t1.y, 0.1f);
                    playerObjects[string.Format("Tail {0}", i)].gameObject.transform.localScale = new Vector3(t2.x, t2.y, 1f);
                    playerObjects[string.Format("Tail {0}", i)].gameObject.transform.localEulerAngles = new Vector3(0f, 0f, t3);
                }

                //Health
                {
                    if (EditorManager.inst == null && DataManager.inst.GetSettingEnum("ArcadeDifficulty", 0) == 3)
                    {
                        CustomPlayer.Health = 1;
                    }
                    else if (EditorManager.inst != null)
                    {
                        CustomPlayer.Health = (int)currentModel.values["Base Health"];
                    }

                    initialHealthCount = CustomPlayer.Health;
                }

                //Health Images
                {
                    foreach (var health in healthObjects)
                    {
                        if (health.image)
                        {
                            health.image.sprite = RTFile.FileExists(RTFile.BasePath + "health.png") && !AssetsGlobal ? SpriteManager.LoadSprite(RTFile.BasePath + "health.png") :
                                RTFile.FileExists(RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/health.png") ? SpriteManager.LoadSprite(RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/health.png") :
                                PlayerManager.healthSprite;
                        }
                    }
                }

                //Trail
                {
                    var headTrail = (TrailRenderer)playerObjects["Head Trail"].values["TrailRenderer"];

                    playerObjects["Head Trail"].gameObject.transform.localPosition = (Vector2)currentModel.values["Head Trail Position Offset"];

                    headTrail.enabled = (bool)currentModel.values["Head Trail Emitting"];
                    //headTrail.time = (float)currentModel.values["Head Trail Time"];
                    headTrail.startWidth = (float)currentModel.values["Head Trail Start Width"];
                    headTrail.endWidth = (float)currentModel.values["Head Trail End Width"];
                }

                //Particles
                {
                    var headParticles = (ParticleSystem)playerObjects["Head Particles"].values["ParticleSystem"];
                    var headParticlesRenderer = (ParticleSystemRenderer)playerObjects["Head Particles"].values["ParticleSystemRenderer"];

                    var s = ((Vector2Int)currentModel.values["Head Particles Shape"]).x;
                    var so = ((Vector2Int)currentModel.values["Head Particles Shape"]).y;

                    s = Mathf.Clamp(s, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                    so = Mathf.Clamp(so, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                    if (s != 4 && s != 6)
                    {
                        headParticlesRenderer.mesh = ObjectManager.inst.objectPrefabs[s].options[so].GetComponentInChildren<MeshFilter>().mesh;
                    }
                    var main = headParticles.main;
                    var emission = headParticles.emission;

                    main.startLifetime = (float)currentModel.values["Head Particles Lifetime"];
                    main.startSpeed = (float)currentModel.values["Head Particles Speed"];

                    emission.enabled = (bool)currentModel.values["Head Particles Emitting"];
                    headParticles.emissionRate = (float)currentModel.values["Head Particles Amount"];

                    var rotationOverLifetime = headParticles.rotationOverLifetime;
                    rotationOverLifetime.enabled = true;
                    rotationOverLifetime.separateAxes = true;
                    rotationOverLifetime.xMultiplier = 0f;
                    rotationOverLifetime.yMultiplier = 0f;
                    rotationOverLifetime.zMultiplier = (float)currentModel.values["Head Particles Rotation"];

                    var forceOverLifetime = headParticles.forceOverLifetime;
                    forceOverLifetime.enabled = true;
                    forceOverLifetime.space = ParticleSystemSimulationSpace.World;
                    forceOverLifetime.xMultiplier = ((Vector2)currentModel.values["Head Particles Force"]).x;
                    forceOverLifetime.yMultiplier = ((Vector2)currentModel.values["Head Particles Force"]).y;

                    var particlesTrail = headParticles.trails;
                    particlesTrail.enabled = (bool)currentModel.values["Head Particles Trail Emitting"];

                    var colorOverLifetime = headParticles.colorOverLifetime;
                    colorOverLifetime.enabled = true;
                    var psCol = colorOverLifetime.color;

                    float alphaStart = (float)currentModel.values["Head Particles Start Opacity"];
                    float alphaEnd = (float)currentModel.values["Head Particles End Opacity"];

                    var gradient = new Gradient();
                    gradient.alphaKeys = new GradientAlphaKey[2]
                    {
                        new GradientAlphaKey(alphaStart, 0f),
                        new GradientAlphaKey(alphaEnd, 1f)
                    };
                    gradient.colorKeys = new GradientColorKey[2]
                    {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(Color.white, 1f)
                    };

                    psCol.gradient = gradient;

                    colorOverLifetime.color = psCol;

                    var sizeOverLifetime = headParticles.sizeOverLifetime;
                    sizeOverLifetime.enabled = true;

                    var ssss = sizeOverLifetime.size;

                    var sizeStart = (float)currentModel.values["Head Particles Start Scale"];
                    var sizeEnd = (float)currentModel.values["Head Particles End Scale"];

                    var curve = new AnimationCurve(new Keyframe[2]
                    {
                        new Keyframe(0f, sizeStart),
                        new Keyframe(1f, sizeEnd)
                    });

                    ssss.curve = curve;

                    sizeOverLifetime.size = ssss;
                }

                //Boost Trail
                {
                    var headTrail = (TrailRenderer)playerObjects["Boost Trail"].values["TrailRenderer"];
                    headTrail.enabled = (bool)currentModel.values["Boost Trail Emitting"];
                    headTrail.emitting = (bool)currentModel.values["Boost Trail Emitting"];
                    //headTrail.time = (float)currentModel.values["Boost Trail Time"];
                }

                //Boost Particles
                {
                    var headParticles = (ParticleSystem)playerObjects["Boost Particles"].values["ParticleSystem"];
                    var headParticlesRenderer = (ParticleSystemRenderer)playerObjects["Boost Particles"].values["ParticleSystemRenderer"];

                    var s = ((Vector2Int)currentModel.values["Boost Particles Shape"]).x;
                    var so = ((Vector2Int)currentModel.values["Boost Particles Shape"]).y;

                    s = Mathf.Clamp(s, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                    so = Mathf.Clamp(so, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                    if (s != 4 && s != 6)
                    {
                        headParticlesRenderer.mesh = ObjectManager.inst.objectPrefabs[s].options[so].GetComponentInChildren<MeshFilter>().mesh;
                    }

                    var main = headParticles.main;
                    var emission = headParticles.emission;

                    main.startLifetime = (float)currentModel.values["Boost Particles Lifetime"];
                    main.startSpeed = (float)currentModel.values["Boost Particles Speed"];

                    emission.enabled = (bool)currentModel.values["Boost Particles Emitting"];
                    headParticles.emissionRate = 0f;
                    emission.burstCount = (int)currentModel.values["Boost Particles Amount"];
                    main.duration = (float)currentModel.values["Boost Particles Duration"];

                    var rotationOverLifetime = headParticles.rotationOverLifetime;
                    rotationOverLifetime.enabled = true;
                    rotationOverLifetime.separateAxes = true;
                    rotationOverLifetime.xMultiplier = 0f;
                    rotationOverLifetime.yMultiplier = 0f;
                    rotationOverLifetime.zMultiplier = (float)currentModel.values["Boost Particles Rotation"];

                    var forceOverLifetime = headParticles.forceOverLifetime;
                    forceOverLifetime.enabled = true;
                    forceOverLifetime.space = ParticleSystemSimulationSpace.World;
                    forceOverLifetime.xMultiplier = ((Vector2)currentModel.values["Boost Particles Force"]).x;
                    forceOverLifetime.yMultiplier = ((Vector2)currentModel.values["Boost Particles Force"]).y;

                    var particlesTrail = headParticles.trails;
                    particlesTrail.enabled = (bool)currentModel.values["Boost Particles Trail Emitting"];

                    var colorOverLifetime = headParticles.colorOverLifetime;
                    colorOverLifetime.enabled = true;
                    var psCol = colorOverLifetime.color;

                    float alphaStart = (float)currentModel.values["Boost Particles Start Opacity"];
                    float alphaEnd = (float)currentModel.values["Boost Particles End Opacity"];

                    var gradient = new Gradient();
                    gradient.alphaKeys = new GradientAlphaKey[2]
                    {
                        new GradientAlphaKey(alphaStart, 0f),
                        new GradientAlphaKey(alphaEnd, 1f)
                    };
                    gradient.colorKeys = new GradientColorKey[2]
                    {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(Color.white, 1f)
                    };

                    psCol.gradient = gradient;

                    colorOverLifetime.color = psCol;

                    var sizeOverLifetime = headParticles.sizeOverLifetime;
                    sizeOverLifetime.enabled = true;

                    var ssss = sizeOverLifetime.size;

                    var sizeStart = (float)currentModel.values["Boost Particles Start Scale"];
                    var sizeEnd = (float)currentModel.values["Boost Particles End Scale"];

                    var curve = new AnimationCurve(new Keyframe[2]
                    {
                        new Keyframe(0f, sizeStart),
                        new Keyframe(1f, sizeEnd)
                    });

                    ssss.curve = curve;

                    sizeOverLifetime.size = ssss;
                }

                //Tails Trail / Particles
                {
                    for (int i = 1; i < 4; i++)
                    {
                        var headTrail = (TrailRenderer)playerObjects[string.Format("Tail {0}", i)].values["TrailRenderer"];
                        headTrail.enabled = (bool)currentModel.values[string.Format("Tail {0} Trail Emitting", i)];
                        headTrail.emitting = (bool)currentModel.values[string.Format("Tail {0} Trail Emitting", i)];
                        //headTrail.time = (float)currentModel.values[string.Format("Tail {0} Trail Time", i)];
                        headTrail.startWidth = (float)currentModel.values[string.Format("Tail {0} Trail Start Width", i)];
                        headTrail.endWidth = (float)currentModel.values[string.Format("Tail {0} Trail End Width", i)];


                        var headParticles = (ParticleSystem)playerObjects[string.Format("Tail {0} Particles", i)].values["ParticleSystem"];
                        var headParticlesRenderer = (ParticleSystemRenderer)playerObjects[string.Format("Tail {0} Particles", i)].values["ParticleSystemRenderer"];

                        var s = ((Vector2Int)currentModel.values[string.Format("Tail {0} Particles Shape", i)]).x;
                        var so = ((Vector2Int)currentModel.values[string.Format("Tail {0} Particles Shape", i)]).y;

                        s = Mathf.Clamp(s, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                        so = Mathf.Clamp(so, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                        if (s != 4 && s != 6)
                        {
                            headParticlesRenderer.mesh = ObjectManager.inst.objectPrefabs[s].options[so].GetComponentInChildren<MeshFilter>().mesh;
                        }
                        var main = headParticles.main;
                        var emission = headParticles.emission;

                        main.startLifetime = (float)currentModel.values[string.Format("Tail {0} Particles Lifetime", i)];
                        main.startSpeed = (float)currentModel.values[string.Format("Tail {0} Particles Speed", i)];

                        emission.enabled = (bool)currentModel.values[string.Format("Tail {0} Particles Emitting", i)];
                        headParticles.emissionRate = (float)currentModel.values[string.Format("Tail {0} Particles Amount", i)];

                        var rotationOverLifetime = headParticles.rotationOverLifetime;
                        rotationOverLifetime.enabled = true;
                        rotationOverLifetime.separateAxes = true;
                        rotationOverLifetime.xMultiplier = 0f;
                        rotationOverLifetime.yMultiplier = 0f;
                        rotationOverLifetime.zMultiplier = (float)currentModel.values[string.Format("Tail {0} Particles Rotation", i)];

                        var forceOverLifetime = headParticles.forceOverLifetime;
                        forceOverLifetime.enabled = true;
                        forceOverLifetime.space = ParticleSystemSimulationSpace.World;
                        forceOverLifetime.xMultiplier = ((Vector2)currentModel.values[string.Format("Tail {0} Particles Force", i)]).x;
                        forceOverLifetime.yMultiplier = ((Vector2)currentModel.values[string.Format("Tail {0} Particles Force", i)]).y;

                        var particlesTrail = headParticles.trails;
                        particlesTrail.enabled = (bool)currentModel.values[string.Format("Tail {0} Particles Trail Emitting", i)];

                        var colorOverLifetime = headParticles.colorOverLifetime;
                        colorOverLifetime.enabled = true;
                        var psCol = colorOverLifetime.color;

                        float alphaStart = (float)currentModel.values[string.Format("Tail {0} Particles Start Opacity", i)];
                        float alphaEnd = (float)currentModel.values[string.Format("Tail {0} Particles End Opacity", i)];

                        var gradient = new Gradient();
                        gradient.alphaKeys = new GradientAlphaKey[2]
                        {
                            new GradientAlphaKey(alphaStart, 0f),
                            new GradientAlphaKey(alphaEnd, 1f)
                        };
                        gradient.colorKeys = new GradientColorKey[2]
                        {
                            new GradientColorKey(Color.white, 0f),
                            new GradientColorKey(Color.white, 1f)
                        };

                        psCol.gradient = gradient;

                        colorOverLifetime.color = psCol;

                        var sizeOverLifetime = headParticles.sizeOverLifetime;
                        sizeOverLifetime.enabled = true;

                        var ssss = sizeOverLifetime.size;

                        var sizeStart = (float)currentModel.values[string.Format("Tail {0} Particles Start Scale", i)];
                        var sizeEnd = (float)currentModel.values[string.Format("Tail {0} Particles End Scale", i)];

                        var curve = new AnimationCurve(new Keyframe[2]
                        {
                        new Keyframe(0f, sizeStart),
                        new Keyframe(1f, sizeEnd)
                        });

                        ssss.curve = curve;

                        sizeOverLifetime.size = ssss;
                    }
                }
            }

            CreateAll();

            updated = true;
        }

        void UpdateCustomObjects(string id = "")
        {
            if (customObjects.Count > 0)
            {
                foreach (var obj in customObjects)
                {
                    if (id != "" && obj.Key == id || id == "")
                    {
                        var customObj = obj.Value;

                        if (((Vector2Int)customObj.values["Shape"]).x != 4 && ((Vector2Int)customObj.values["Shape"]).x != 6)
                        {
                            var shape = (Vector2Int)customObj.values["Shape"];
                            var pos = (Vector2)customObj.values["Position"];
                            var sca = (Vector2)customObj.values["Scale"];
                            var rot = (float)customObj.values["Rotation"];

                            var depth = (float)customObj.values["Depth"];

                            int s = Mathf.Clamp(shape.x, 0, ObjectManager.inst.objectPrefabs.Count - 1);
                            int so = Mathf.Clamp(shape.y, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

                            customObj.gameObject = Instantiate(ObjectManager.inst.objectPrefabs[s].options[so]);
                            customObj.gameObject.transform.SetParent(transform);
                            customObj.gameObject.transform.localScale = Vector3.one;
                            customObj.gameObject.transform.localRotation = Quaternion.identity;

                            var PlayerDelayTracker = customObj.gameObject.AddComponent<PlayerDelayTracker>();
                            PlayerDelayTracker.offset = 0;
                            PlayerDelayTracker.positionOffset = (float)customObj.values["Parent Position Offset"];
                            PlayerDelayTracker.scaleOffset = (float)customObj.values["Parent Scale Offset"];
                            PlayerDelayTracker.rotationOffset = (float)customObj.values["Parent Rotation Offset"];
                            PlayerDelayTracker.scaleParent = (bool)customObj.values["Parent Scale Active"];
                            PlayerDelayTracker.rotationParent = (bool)customObj.values["Parent Rotation Active"];
                            PlayerDelayTracker.player = this;
                            
                            switch ((int)customObj.values["Parent"])
                            {
                                case 0:
                                    {
                                        PlayerDelayTracker.leader = playerObjects["RB Parent"].gameObject.transform;
                                        break;
                                    }
                                case 1:
                                    {
                                        PlayerDelayTracker.leader = playerObjects["Boost Base"].gameObject.transform;
                                        break;
                                    }
                                case 2:
                                    {
                                        PlayerDelayTracker.leader = playerObjects["Boost Tail Base"].gameObject.transform;
                                        break;
                                    }
                                case 3:
                                    {
                                        PlayerDelayTracker.leader = playerObjects["Tail 1 Base"].gameObject.transform;
                                        break;
                                    }
                                case 4:
                                    {
                                        PlayerDelayTracker.leader = playerObjects["Tail 2 Base"].gameObject.transform;
                                        break;
                                    }
                                case 5:
                                    {
                                        PlayerDelayTracker.leader = playerObjects["Tail 3 Base"].gameObject.transform;
                                        break;
                                    }
                                case 6:
                                    {
                                        PlayerDelayTracker.leader = playerObjects["Face Parent"].gameObject.transform;
                                        break;
                                    }
                            }

                            customObj.gameObject.transform.GetChild(0).localPosition = new Vector3(pos.x, pos.y, depth);
                            customObj.gameObject.transform.GetChild(0).localScale = new Vector3(sca.x, sca.y, 1f);
                            customObj.gameObject.transform.GetChild(0).localEulerAngles = new Vector3(0f, 0f, rot);

                            customObj.gameObject.tag = "Helper";
                            customObj.gameObject.transform.GetChild(0).tag = "Helper";

                            customObj.values["MeshRenderer"] = customObj.gameObject.GetComponentInChildren<MeshRenderer>();
                        }
                    }
                }
            }
        }

        void CreateAll()
        {
            var currentModel = PlayerModel;

            Dictionary<string, object> dictionary = (Dictionary<string, object>)currentModel.values["Custom Objects"];

            foreach (var obj in customObjects)
            {
                if (obj.Value.gameObject != null)
                {
                    Destroy(obj.Value.gameObject);
                }
            }

            customObjects.Clear();
            if (dictionary != null && dictionary.Count > 0)
                foreach (var obj in dictionary)
                {
                    var id = obj.Key;
                    var customObj = (Dictionary<string, object>)obj.Value;

                    var c = CreateCustomObject();
                    c.values["ID"] = customObj["ID"];
                    c.values["Shape"] = customObj["Shape"];
                    c.values["Position"] = customObj["Position"];
                    c.values["Scale"] = customObj["Scale"];
                    c.values["Rotation"] = customObj["Rotation"];
                    c.values["Color"] = customObj["Color"];
                    c.values["Custom Color"] = customObj["Custom Color"];
                    c.values["Opacity"] = customObj["Opacity"];
                    c.values["Parent"] = customObj["Parent"];
                    c.values["Parent Position Offset"] = customObj["Parent Position Offset"];
                    c.values["Parent Scale Offset"] = customObj["Parent Scale Offset"];
                    c.values["Parent Rotation Offset"] = customObj["Parent Rotation Offset"];
                    c.values["Parent Scale Active"] = customObj["Parent Scale Active"];
                    c.values["Parent Rotation Active"] = customObj["Parent Rotation Active"];
                    c.values["Depth"] = customObj["Depth"];
                    c.values["Visibility"] = customObj["Visibility"];
                    c.values["Visibility Value"] = customObj["Visibility Value"];
                    c.values["Visibility Not"] = customObj["Visibility Not"];

                    customObjects.Add((string)customObj["ID"], c);
                }

            UpdateCustomObjects();
        }

        public PlayerObject CreateCustomObject()
        {
            var obj = new PlayerObject();

            obj.name = "Object";
            obj.values = new Dictionary<string, object>();
            obj.values.Add("Shape", new Vector2Int(0, 0));
            obj.values.Add("Position", new Vector2(0f, 0f));
            obj.values.Add("Scale", new Vector2(1f, 1f));
            obj.values.Add("Rotation", 0f);
            obj.values.Add("Color", 0);
            obj.values.Add("Custom Color", "FFFFFF");
            obj.values.Add("Opacity", 0f);
            obj.values.Add("Parent", 0);
            obj.values.Add("Parent Position Offset", 1f);
            obj.values.Add("Parent Scale Offset", 1f);
            obj.values.Add("Parent Rotation Offset", 1f);
            obj.values.Add("Parent Scale Active", false);
            obj.values.Add("Parent Rotation Active", true);
            obj.values.Add("Depth", 0f);
            obj.values.Add("MeshRenderer", null);
            obj.values.Add("Visibility", 0);
            obj.values.Add("Visibility Value", 100f);
            obj.values.Add("Visibility Not", false);
            var id = LSText.randomNumString(16);
            obj.values.Add("ID", id);

            return obj;
        }

        void UpdateCustomTheme()
        {
            if (customObjects.Count > 0)
                foreach (var obj in customObjects.Values)
                {
                    int vis = (int)obj.values["Visibility"];
                    bool not = (bool)obj.values["Visibility Not"];
                    float value = (float)obj.values["Visibility Value"];
                    if (obj.gameObject != null)
                    {
                        switch (vis)
                        {
                            case 0:
                                {
                                    obj.gameObject.SetActive(true);
                                    break;
                                }
                            case 1:
                                {
                                    if (!not)
                                        obj.gameObject.SetActive(isBoosting);
                                    else
                                        obj.gameObject.SetActive(!isBoosting);
                                    break;
                                }
                            case 2:
                                {
                                    if (!not)
                                        obj.gameObject.SetActive(isTakingHit);
                                    else
                                        obj.gameObject.SetActive(!isTakingHit);
                                    break;
                                }
                            case 3:
                                {
                                    bool zen = DataManager.inst.GetSettingEnum("ArcadeDifficulty", 1) == 0;
                                    if (!not)
                                        obj.gameObject.SetActive(zen);
                                    else
                                        obj.gameObject.SetActive(!zen);
                                    break;
                                }
                            case 4:
                                {
                                    if (!not)
                                        obj.gameObject.SetActive((float)CustomPlayer.health / (float)initialHealthCount * 100f >= value);
                                    else
                                        obj.gameObject.SetActive(!((float)CustomPlayer.health / (float)initialHealthCount * 100f >= value));
                                    break;
                                }
                            case 5:
                                {
                                    if (!not)
                                        obj.gameObject.SetActive(CustomPlayer.health >= value);
                                    else
                                        obj.gameObject.SetActive(!(CustomPlayer.health >= value));
                                    break;
                                }
                            case 6:
                                {
                                    if (!not)
                                        obj.gameObject.SetActive(CustomPlayer.health == value);
                                    else
                                        obj.gameObject.SetActive(!(CustomPlayer.health == value));
                                    break;
                                }
                            case 7:
                                {
                                    if (!not)
                                        obj.gameObject.SetActive(CustomPlayer.health > value);
                                    else
                                        obj.gameObject.SetActive(!(CustomPlayer.health > value));
                                    break;
                                }
                            case 8:
                                {
                                    bool active = Input.GetKey(GetKeyCode((int)value));
                                    if (!not)
                                        obj.gameObject.SetActive(active);
                                    else
                                        obj.gameObject.SetActive(!active);
                                    break;
                                }
                        }
                    }

                    int col = (int)obj.values["Color"];
                    string hex = (string)obj.values["Custom Color"];
                    float alpha = (float)obj.values["Opacity"];
                    if (((MeshRenderer)obj.values["MeshRenderer"]) != null && obj.gameObject.activeSelf)
                    {
                        ((MeshRenderer)obj.values["MeshRenderer"]).material.color = GetColor(col, alpha, hex);
                    }
                }
        }

        public void UpdateTail(int _health, Vector3 _pos)
        {
            if (_health > initialHealthCount)
            {
                initialHealthCount = _health;

                if (tailGrows)
                {
                    var t = path[path.Count - 2].transform.gameObject.Duplicate(playerObjects["Tail Parent"].gameObject.transform);
                    t.transform.SetParent(playerObjects["Tail Parent"].gameObject.transform);
                    t.transform.localScale = Vector3.one;
                    t.name = string.Format("Tail {0}", path.Count - 2);

                    path.Insert(path.Count - 2, new MovementPath(t.transform.localPosition, t.transform.localRotation, t.transform));
                }
            }

            for (int i = 2; i < path.Count; i++)
            {
                if (path[i].transform != null)
                {
                    if (i - 1 > _health)
                    {
                        if (path[i].transform.childCount != 0)
                            path[i].transform.GetChild(0).gameObject.SetActive(false);
                        else
                            path[i].transform.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (path[i].transform.childCount != 0)
                            path[i].transform.GetChild(0).gameObject.SetActive(true);
                        else
                            path[i].transform.gameObject.SetActive(true);
                    }
                }
            }

            if (!PlayerModel)
                return;

            var currentModel = PlayerModel;

            if (healthObjects.Count > 0)
            {
                for (int i = 0; i < healthObjects.Count; i++)
                {
                    if (i < _health && (bool)currentModel.values["GUI Health Active"] && (int)currentModel.values["GUI Health Mode"] == 0)
                        healthObjects[i].gameObject.SetActive(true);
                    else
                        healthObjects[i].gameObject.SetActive(false);
                }
            }

            var text = health.GetComponent<Text>();
            if ((bool)currentModel.values["GUI Health Active"] && ((int)currentModel.values["GUI Health Mode"] == 1 || (int)currentModel.values["GUI Health Mode"] == 2))
            {
                text.enabled = true;
                if ((int)currentModel.values["GUI Health Mode"] == 1)
                    text.text = _health.ToString();
                else
                    text.text = FontManager.TextTranslater.ConvertHealthToEquals(_health, initialHealthCount);
            }
            else
            {
                text.enabled = false;
            }
            if ((bool)currentModel.values["GUI Health Active"] && (int)currentModel.values["GUI Health Mode"] == 3)
            {
                barBaseIm.gameObject.SetActive(true);
                var e = (float)_health / (float)initialHealthCount;
                barRT.sizeDelta = new Vector2(200f * e, 32f);
            }
            else
            {
                barBaseIm.gameObject.SetActive(false);
            }
            //for (int j = 1; j < _health + 1; j++)
            //{
            //    if (path.Count > _health + 1 && path[j].transform != null)
            //        path[j].transform.gameObject.SetActive(true);
            //}
        }

        public Color GetColor(int col, float alpha, string hex)
            => LSColors.fadeColor(col >= 0 && col < 4 ? RTHelpers.BeatmapTheme.playerColors[col] : col == 4 ? RTHelpers.BeatmapTheme.guiColor : col > 4 && col < 23 ? RTHelpers.BeatmapTheme.objectColors[col - 5] :
                col == 23 ? RTHelpers.BeatmapTheme.playerColors[playerIndex % 4] : col == 24 ? LSColors.HexToColor(hex) : col == 25 ? RTHelpers.BeatmapTheme.guiAccentColor : LSColors.pink500, alpha);
        
        #endregion

        #region Actions

        void CreatePulse()
        {
            if (!PlayerModel)
                return;

            var currentModel = PlayerModel;

            if (!currentModel.values.ContainsKey("Pulse Active") || !(bool)currentModel.values["Pulse Active"])
                return;

            var player = playerObjects["RB Parent"].gameObject;

            int s = Mathf.Clamp(((Vector2Int)currentModel.values["Pulse Shape"]).x, 0, ObjectManager.inst.objectPrefabs.Count - 1);
            int so = Mathf.Clamp(((Vector2Int)currentModel.values["Pulse Shape"]).y, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

            var objcopy = ObjectManager.inst.objectPrefabs[s].options[so];
            if (s == 4 || s == 6)
            {
                objcopy = ObjectManager.inst.objectPrefabs[0].options[0];
            }

            var pulse = Instantiate(objcopy);
            pulse.transform.SetParent(ObjectManager.inst.objectParent.transform);
            pulse.transform.localScale = new Vector3(((Vector2)currentModel.values["Pulse Start Scale"]).x, ((Vector2)currentModel.values["Pulse Start Scale"]).y, 1f);
            pulse.transform.position = player.transform.position;
            pulse.transform.GetChild(0).localPosition = new Vector3(((Vector2)currentModel.values["Pulse Start Position"]).x, ((Vector2)currentModel.values["Pulse Start Position"]).y, (float)currentModel.values["Pulse Depth"]);
            pulse.transform.GetChild(0).localRotation = Quaternion.Euler(new Vector3(0f, 0f, (float)currentModel.values["Pulse Start Rotation"]));

            if ((bool)currentModel.values["Pulse Rotate to Head"])
            {
                pulse.transform.localRotation = player.transform.localRotation;
            }

            //Destroy
            {
                if (pulse.transform.GetChild(0).GetComponent<SelectObjectInEditor>())
                {
                    Destroy(pulse.transform.GetChild(0).GetComponent<SelectObjectInEditor>());
                }
                if (pulse.transform.GetChild(0).GetComponent<BoxCollider2D>())
                {
                    Destroy(pulse.transform.GetChild(0).GetComponent<BoxCollider2D>());
                }
                if (pulse.transform.GetChild(0).GetComponent<PolygonCollider2D>())
                {
                    Destroy(pulse.transform.GetChild(0).GetComponent<PolygonCollider2D>());
                }
                if (pulse.transform.GetChild(0).gameObject.GetComponentByName("RTObject"))
                {
                    Destroy(pulse.transform.GetChild(0).gameObject.GetComponentByName("RTObject"));
                }
            }

            var obj = new PlayerObject("Pulse", pulse.transform.GetChild(0).gameObject);

            MeshRenderer pulseRenderer = pulse.transform.GetChild(0).GetComponent<MeshRenderer>();
            obj.values.Add("MeshRenderer", pulseRenderer);
            obj.values.Add("Opacity", 0f);
            obj.values.Add("ColorTween", 0f);
            obj.values.Add("StartColor", (int)currentModel.values["Pulse Start Color"]);
            obj.values.Add("EndColor", (int)currentModel.values["Pulse End Color"]);
            obj.values.Add("StartCustomColor", (string)currentModel.values["Pulse Start Custom Color"]);
            obj.values.Add("EndCustomColor", (string)currentModel.values["Pulse End Custom Color"]);

            boosts.Add(obj);

            pulseRenderer.enabled = true;
            pulseRenderer.material = ((MeshRenderer)playerObjects["Head"].values["MeshRenderer"]).material;
            pulseRenderer.material.shader = ((MeshRenderer)playerObjects["Head"].values["MeshRenderer"]).material.shader;
            Color colorBase = ((MeshRenderer)playerObjects["Head"].values["MeshRenderer"]).material.color;

            int easingPos = (int)currentModel.values["Pulse Easing Position"];
            int easingSca = (int)currentModel.values["Pulse Easing Scale"];
            int easingRot = (int)currentModel.values["Pulse Easing Rotation"];
            int easingOpa = (int)currentModel.values["Pulse Easing Opacity"];
            int easingCol = (int)currentModel.values["Pulse Easing Color"];

            float duration = Mathf.Clamp((float)currentModel.values["Pulse Duration"], 0.001f, 20f) / RTHelpers.Pitch;

            pulse.transform.GetChild(0).DOLocalMove(new Vector3(((Vector2)currentModel.values["Pulse End Position"]).x, ((Vector2)currentModel.values["Pulse End Position"]).y, (float)currentModel.values["Pulse Depth"]), duration).SetEase(DataManager.inst.AnimationList[easingPos].Animation);
            var tweenScale = pulse.transform.DOScale(new Vector3(((Vector2)currentModel.values["Pulse End Scale"]).x, ((Vector2)currentModel.values["Pulse End Scale"]).y, 1f), duration).SetEase(DataManager.inst.AnimationList[easingSca].Animation);
            pulse.transform.GetChild(0).DOLocalRotate(new Vector3(0f, 0f, (float)currentModel.values["Pulse End Rotation"]), duration).SetEase(DataManager.inst.AnimationList[easingRot].Animation);

            DOTween.To(delegate (float x)
            {
                obj.values["Opacity"] = x;
            }, (float)currentModel.values["Pulse Start Opacity"], (float)currentModel.values["Pulse End Opacity"], duration).SetEase(DataManager.inst.AnimationList[easingOpa].Animation);
            DOTween.To(delegate (float x)
            {
                obj.values["ColorTween"] = x;
            }, 0f, 1f, duration).SetEase(DataManager.inst.AnimationList[easingCol].Animation);

            tweenScale.OnComplete(delegate ()
            {
                Destroy(pulse);
                boosts.Remove(obj);
            });
        }

        void UpdateBoostTheme()
        {
            if (boosts.Count > 0)
            {
                foreach (var boost in boosts)
                {
                    if (boost != null)
                    {
                        int startCol = (int)boost.values["StartColor"];
                        int endCol = (int)boost.values["EndColor"];

                        var startHex = (string)boost.values["StartCustomColor"];
                        var endHex = (string)boost.values["EndCustomColor"];

                        float alpha = (float)boost.values["Opacity"];
                        float colorTween = (float)boost.values["ColorTween"];

                        Color startColor = GetColor(startCol, alpha, startHex);
                        Color endColor = GetColor(endCol, alpha, endHex);

                        if (((MeshRenderer)boost.values["MeshRenderer"]) != null)
                        {
                            ((MeshRenderer)boost.values["MeshRenderer"]).material.color = Color.Lerp(startColor, endColor, colorTween);
                        }
                    }
                }
            }
        }

        public List<PlayerObject> boosts = new List<PlayerObject>();

        void PlaySound(AudioClip _clip, float pitch = 1f)
        {
            float p = pitch * RTHelpers.Pitch;

            var audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
            audioSource.clip = _clip;
            audioSource.playOnAwake = true;
            audioSource.loop = false;
            audioSource.volume = AudioManager.inst.sfxVol;
            audioSource.pitch = pitch * AudioManager.inst.pitch;
            audioSource.Play();
            StartCoroutine(AudioManager.inst.DestroyWithDelay(audioSource, _clip.length / p));
        }

        void CreateBullet()
        {
            var currentModel = PlayerModel;

            if (currentModel == null || !currentModel.values.ContainsKey("Bullet Active") || !(bool)currentModel.values["Bullet Active"])
                return;

            if (PlayShootSound)
                PlaySound(AudioManager.inst.GetSound("boost"), 0.7f);

            canShoot = false;

            var player = playerObjects["RB Parent"].gameObject;

            int s = Mathf.Clamp(((Vector2Int)currentModel.values["Bullet Shape"]).x, 0, ObjectManager.inst.objectPrefabs.Count - 1);
            int so = Mathf.Clamp(((Vector2Int)currentModel.values["Bullet Shape"]).y, 0, ObjectManager.inst.objectPrefabs[s].options.Count - 1);

            var objcopy = ObjectManager.inst.objectPrefabs[s].options[so];
            if (s == 4 || s == 6)
            {
                objcopy = ObjectManager.inst.objectPrefabs[0].options[0];
            }

            var pulse = Instantiate(objcopy);
            pulse.transform.SetParent(ObjectManager.inst.objectParent.transform);
            pulse.transform.localScale = new Vector3(((Vector2)currentModel.values["Bullet Start Scale"]).x, ((Vector2)currentModel.values["Bullet Start Scale"]).y, 1f);

            var vec = new Vector3(((Vector2)currentModel.values["Bullet Origin"]).x, ((Vector2)currentModel.values["Bullet Origin"]).y, 0f);
            if (rotateMode == RotateMode.FlipX && lastMovement.x < 0f)
                vec.x = -vec.x;

            pulse.transform.position = player.transform.position + vec;
            pulse.transform.GetChild(0).localPosition = new Vector3(((Vector2)currentModel.values["Bullet Start Position"]).x, ((Vector2)currentModel.values["Bullet Start Position"]).y, (float)currentModel.values["Bullet Depth"]);
            pulse.transform.GetChild(0).localRotation = Quaternion.Euler(new Vector3(0f, 0f, (float)currentModel.values["Bullet Start Rotation"]));

            if (!AllowPlayersToTakeBulletDamage || !(bool)currentModel.values["Bullet Hurt Players"])
            {
                pulse.tag = "Helper";
                pulse.transform.GetChild(0).tag = "Helper";
            }

            pulse.transform.GetChild(0).gameObject.name = "bullet (Player " + (playerIndex + 1).ToString() + ")";

            float speed = Mathf.Clamp((float)currentModel.values["Bullet Speed Amount"], 0.001f, 20f) / RTHelpers.Pitch;
            var b = pulse.AddComponent<Bullet>();
            b.speed = speed;
            b.player = this;
            b.Assign();

            pulse.transform.localRotation = player.transform.localRotation;

            //Destroy
            {
                if (pulse.transform.GetChild(0).GetComponent<SelectObjectInEditor>())
                {
                    Destroy(pulse.transform.GetChild(0).GetComponent<SelectObjectInEditor>());
                }
                if (pulse.transform.GetChild(0).gameObject.GetComponentByName("RTObject"))
                {
                    Destroy(pulse.transform.GetChild(0).gameObject.GetComponentByName("RTObject"));
                }
            }

            var obj = new PlayerObject("Bullet", pulse.transform.GetChild(0).gameObject);

            MeshRenderer pulseRenderer = pulse.transform.GetChild(0).GetComponent<MeshRenderer>();
            obj.values.Add("MeshRenderer", pulseRenderer);
            obj.values.Add("Opacity", 0f);
            obj.values.Add("ColorTween", 0f);
            obj.values.Add("StartColor", (int)currentModel.values["Bullet Start Color"]);
            obj.values.Add("EndColor", (int)currentModel.values["Bullet End Color"]);
            obj.values.Add("StartCustomColor", (string)currentModel.values["Bullet Start Custom Color"]);
            obj.values.Add("EndCustomColor", (string)currentModel.values["Bullet End Custom Color"]);

            boosts.Add(obj);

            pulseRenderer.enabled = true;
            pulseRenderer.material = ((MeshRenderer)playerObjects["Head"].values["MeshRenderer"]).material;
            pulseRenderer.material.shader = ((MeshRenderer)playerObjects["Head"].values["MeshRenderer"]).material.shader;
            Color colorBase = ((MeshRenderer)playerObjects["Head"].values["MeshRenderer"]).material.color;

            var collider2D = pulse.transform.GetChild(0).GetComponent<Collider2D>();
            collider2D.enabled = true;
            //collider2D.isTrigger = false;

            var rb2D = pulse.transform.GetChild(0).gameObject.AddComponent<Rigidbody2D>();
            rb2D.gravityScale = 0f;

            var bulletCollider = pulse.transform.GetChild(0).gameObject.AddComponent<BulletCollider>();
            bulletCollider.rb = (Rigidbody2D)playerObjects["RB Parent"].values["Rigidbody2D"];
            if (currentModel.values.ContainsKey("Bullet AutoKill"))
            {
                bulletCollider.kill = (bool)currentModel.values["Bullet AutoKill"];
            }
            bulletCollider.player = this;
            bulletCollider.playerObject = obj;

            int easingPos = (int)currentModel.values["Bullet Easing Position"];
            int easingSca = (int)currentModel.values["Bullet Easing Scale"];
            int easingRot = (int)currentModel.values["Bullet Easing Rotation"];
            int easingOpa = (int)currentModel.values["Bullet Easing Opacity"];
            int easingCol = (int)currentModel.values["Bullet Easing Color"];

            float posDuration = Mathf.Clamp((float)currentModel.values["Bullet Duration Position"], 0.001f, 20f) / RTHelpers.Pitch;
            float scaDuration = Mathf.Clamp((float)currentModel.values["Bullet Duration Scale"], 0.001f, 20f) / RTHelpers.Pitch;
            float rotDuration = Mathf.Clamp((float)currentModel.values["Bullet Duration Rotation"], 0.001f, 20f) / RTHelpers.Pitch;
            float lifeTime = Mathf.Clamp((float)currentModel.values["Bullet Lifetime"], 0.001f, 20f) / RTHelpers.Pitch;

            pulse.transform.GetChild(0).DOLocalMove(new Vector3(((Vector2)currentModel.values["Bullet End Position"]).x, ((Vector2)currentModel.values["Bullet End Position"]).y, (float)currentModel.values["Bullet Depth"]), posDuration).SetEase(DataManager.inst.AnimationList[easingPos].Animation);
            pulse.transform.DOScale(new Vector3(((Vector2)currentModel.values["Bullet End Scale"]).x, ((Vector2)currentModel.values["Bullet End Scale"]).y, 1f), scaDuration).SetEase(DataManager.inst.AnimationList[easingSca].Animation);
            pulse.transform.GetChild(0).DOLocalRotate(new Vector3(0f, 0f, (float)currentModel.values["Bullet End Rotation"]), rotDuration).SetEase(DataManager.inst.AnimationList[easingRot].Animation);

            DOTween.To(delegate (float x)
            {
                obj.values["Opacity"] = x;
            }, (float)currentModel.values["Bullet Start Opacity"], (float)currentModel.values["Bullet End Opacity"], posDuration).SetEase(DataManager.inst.AnimationList[easingOpa].Animation);
            DOTween.To(delegate (float x)
            {
                obj.values["ColorTween"] = x;
            }, 0f, 1f, posDuration).SetEase(DataManager.inst.AnimationList[easingCol].Animation);

            StartCoroutine(CanShoot());

            var tweener = DOTween.To(delegate (float x) { }, 1f, 1f, lifeTime).SetEase(DataManager.inst.AnimationList[easingOpa].Animation);
            bulletCollider.tweener = tweener;

            tweener.OnComplete(delegate ()
            {
                var tweenScale = pulse.transform.GetChild(0).DOScale(Vector3.zero, 0.2f).SetEase(DataManager.inst.AnimationList[2].Animation);
                bulletCollider.tweener = tweenScale;

                tweenScale.OnComplete(delegate ()
                {
                    Destroy(pulse);
                    boosts.Remove(obj);
                    obj = null;
                });
            });
        }

        IEnumerator CanShoot()
        {
            var currentModel = PlayerModel;
            if (currentModel != null)
            {
                var delay = (float)currentModel.values["Bullet Delay Amount"];
                yield return new WaitForSeconds(delay);
            }
            canShoot = true;

            yield break;
        }

        #endregion

        #region Code

        public string SpawnCodePath => "player/spawn.cs";
        public string BoostCodePath => "player/boost.cs";
        public string HitCodePath => "player/hit.cs";
        public string DeathCodePath => "player/death.cs";

        void EvaluateSpawnCode()
        {
            if (!EvaluateCode)
                return;

            string path = RTFile.BasePath + SpawnCodePath;

            if (RTFile.FileExists(path))
            {
                var def = $"var playerIndex = {playerIndex};{Environment.NewLine}";

                string cs = FileManager.inst.LoadJSONFileRaw(path);

                if (!cs.Contains("System.IO.File.") && !cs.Contains("File."))
                    RTCode.Evaluate($"{def}{cs}");
            }
        }

        void EvaluateBoostCode()
        {
            if (!EvaluateCode)
                return;

            string path = RTFile.BasePath + BoostCodePath;

            if (RTFile.FileExists(path))
            {
                var def = $"var playerIndex = {playerIndex};{Environment.NewLine}";

                string cs = FileManager.inst.LoadJSONFileRaw(path);

                if (!cs.Contains("System.IO.File.") && !cs.Contains("File."))
                    RTCode.Evaluate($"{def}{cs}");
            }
        }

        void EvaluateHitCode()
        {
            if (!EvaluateCode)
                return;

            string path = RTFile.BasePath + HitCodePath;

            if (RTFile.FileExists(path))
            {
                var def = $"var playerIndex = {playerIndex};{Environment.NewLine}";

                string cs = FileManager.inst.LoadJSONFileRaw(path);

                if (!cs.Contains("System.IO.File.") && !cs.Contains("File."))
                    RTCode.Evaluate($"{def}{cs}");
            }
        }

        void EvaluateDeathCode()
        {
            if (!EvaluateCode)
                return;

            string path = RTFile.BasePath + DeathCodePath;

            if (RTFile.FileExists(path))
            {
                var def = $"var playerIndex = {playerIndex};{Environment.NewLine}";

                string cs = FileManager.inst.LoadJSONFileRaw(path);

                if (!cs.Contains("System.IO.File.") && !cs.Contains("File."))
                    RTCode.Evaluate($"{def}{cs}");
            }
        }
        
        #endregion

        public KeyCode GetKeyCode(int key)
        {
            if (key < 91)
            switch (key)
            {
                case 0: return KeyCode.None;
                case 1: return KeyCode.Backspace;
                case 2: return KeyCode.Tab;
                case 3: return KeyCode.Clear;
                case 4: return KeyCode.Return;
                case 5: return KeyCode.Pause;
                case 6: return KeyCode.Escape;
                case 7: return KeyCode.Space;
                case 8: return KeyCode.Quote;
                case 9: return KeyCode.Comma;
                case 10: return KeyCode.Minus;
                case 11: return KeyCode.Period;
                case 12: return KeyCode.Slash;
                case 13: return KeyCode.Alpha0;
                case 14: return KeyCode.Alpha1;
                case 15: return KeyCode.Alpha2;
                case 16: return KeyCode.Alpha3;
                case 17: return KeyCode.Alpha4;
                case 18: return KeyCode.Alpha5;
                case 19: return KeyCode.Alpha6;
                case 20: return KeyCode.Alpha7;
                case 21: return KeyCode.Alpha8;
                case 22: return KeyCode.Alpha9;
                case 23: return KeyCode.Semicolon;
                case 24: return KeyCode.Equals;
                case 25: return KeyCode.LeftBracket;
                case 26: return KeyCode.RightBracket;
                case 27: return KeyCode.Backslash;
                case 28: return KeyCode.A;
                case 29: return KeyCode.B;
                case 30: return KeyCode.C;
                case 31: return KeyCode.D;
                case 32: return KeyCode.E;
                case 33: return KeyCode.F;
                case 34: return KeyCode.G;
                case 35: return KeyCode.H;
                case 36: return KeyCode.I;
                case 37: return KeyCode.J;
                case 38: return KeyCode.K;
                case 39: return KeyCode.L;
                case 40: return KeyCode.M;
                case 41: return KeyCode.N;
                case 42: return KeyCode.O;
                case 43: return KeyCode.P;
                case 44: return KeyCode.Q;
                case 45: return KeyCode.R;
                case 46: return KeyCode.S;
                case 47: return KeyCode.T;
                case 48: return KeyCode.U;
                case 49: return KeyCode.V;
                case 50: return KeyCode.W;
                case 51: return KeyCode.X;
                case 52: return KeyCode.Y;
                case 53: return KeyCode.Z;
                case 54: return KeyCode.Keypad0;
                case 55: return KeyCode.Keypad1;
                case 56: return KeyCode.Keypad2;
                case 57: return KeyCode.Keypad3;
                case 58: return KeyCode.Keypad4;
                case 59: return KeyCode.Keypad5;
                case 60: return KeyCode.Keypad6;
                case 61: return KeyCode.Keypad7;
                case 62: return KeyCode.Keypad8;
                case 63: return KeyCode.Keypad9;
                case 64: return KeyCode.KeypadDivide;
                case 65: return KeyCode.KeypadMultiply;
                case 66: return KeyCode.KeypadMinus;
                case 67: return KeyCode.KeypadPlus;
                case 68: return KeyCode.KeypadEnter;
                case 69: return KeyCode.UpArrow;
                case 70: return KeyCode.DownArrow;
                case 71: return KeyCode.RightArrow;
                case 72: return KeyCode.LeftArrow;
                case 73: return KeyCode.Insert;
                case 74: return KeyCode.Home;
                case 75: return KeyCode.End;
                case 76: return KeyCode.PageUp;
                case 77: return KeyCode.PageDown;
                case 78: return KeyCode.RightShift;
                case 79: return KeyCode.LeftShift;
                case 80: return KeyCode.RightControl;
                case 81: return KeyCode.LeftControl;
                case 82: return KeyCode.RightAlt;
                case 83: return KeyCode.LeftAlt;
                case 84: return KeyCode.Mouse0;
                case 85: return KeyCode.Mouse1;
                case 86: return KeyCode.Mouse2;
                case 87: return KeyCode.Mouse3;
                case 88: return KeyCode.Mouse4;
                case 89: return KeyCode.Mouse5;
                case 90: return KeyCode.Mouse6;
            }

            if (key > 90)
            {
                int num = key + 259;

                if (IndexToInt(CustomPlayer.playerIndex) > 0)
                {
                    string str = (IndexToInt(CustomPlayer.playerIndex) * 2).ToString() + "0";
                    num += int.Parse(str);
                }

                return (KeyCode)num;
            }

            return KeyCode.None;
        }

        public int IndexToInt(PlayerIndex playerIndex) => (int)playerIndex;

        #region Objects

        public Dictionary<string, PlayerObject> customObjects = new Dictionary<string, PlayerObject>();
        public Dictionary<string, PlayerObject> playerObjects = new Dictionary<string, PlayerObject>();

        public class PlayerObject
        {
            public PlayerObject()
            {

            }

            public PlayerObject(string _name, GameObject _gm)
            {
                name = _name;
                gameObject = _gm;
                values = new Dictionary<string, object>();

                values.Add("Position", Vector3.zero);
                values.Add("Scale", Vector3.one);
                values.Add("Rotation", 0f);
                values.Add("Color", 0);
            }
            public PlayerObject(string _name, Dictionary<string, object> _values, GameObject _gm)
            {
                name = _name;
                values = _values;
                gameObject = _gm;
            }

            public string name;
            public GameObject gameObject;

            public Dictionary<string, object> values;
        }

        public List<MovementPath> path = new List<MovementPath>();

        public class MovementPath
        {
            public MovementPath(Vector3 _pos, Quaternion _rot, Transform _tf)
            {
                pos = _pos;
                rot = _rot;
                transform = _tf;
            }

            public MovementPath(Vector3 _pos, Quaternion _rot, Transform _tf, bool active)
            {
                pos = _pos;
                rot = _rot;
                transform = _tf;
                this.active = active;
            }

            public bool active = true;

            public Vector3 lastPos;
            public Vector3 pos;

            public Quaternion rot;

            public Transform transform;
        }

        public List<HealthObject> healthObjects = new List<HealthObject>();

        public class HealthObject
        {
            public HealthObject(GameObject gameObject, Image image)
            {
                this.gameObject = gameObject;
                this.image = image;
            }

            public GameObject gameObject;
            public Image image;
        }

        #endregion
    }
}
