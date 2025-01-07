using KBCore.Refs;
using System.Collections;
using Player.Combat.ComboSystem;
using UnityEngine;
using UnityEngine.AI;
using Player;
using AudioSystem;


namespace Cyber_Slicer_AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    public class Enemy : Unit { 
        #region Parameters
        [Space]
        [SerializeField] private Vector2Int bloodOnDeath = new Vector2Int(3, 7);
        [SerializeField] private float comboOnDeath = 10f;
        [Range(0, 100)]
        [SerializeField] private int itemDropChance = 20;
        [Header("Parameters")]
        [SerializeField, Self] NavMeshAgent agent;
        [SerializeField, Self] PlayerDetector playerDetector;
        [SerializeField, Child] Animator animator;
        //[SerializeField, Child] AI_Shooter_CS shooter;
        [SerializeField] AI_Shooter_CS shooter;
        [SerializeField] AI_Melee_Weapon melee;
        [SerializeField] AI_Boss boss;
        [SerializeField] float wanderRadius = 10f;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] float timeBetweenCharges = 3f; // Test
        [SerializeField] float combatCooldown = 5f;
        [SerializeField] float stunTime = 4f; // bad naming
        [SerializeField] float spinTime = 0.8f;
        public float despawnTime = 1f;
        public bool isAlive;
        [SerializeField] bool isMelee;
        [SerializeField] bool isSniper;
        //sniper stuff
        public Laser laser;
        //Sound
        public SoundData DeathSound;
        

        public bool isBoss;
        public string UniqueID { get; private set; }
        public string enemyID;
        public bool playerInSight = false;
        StateMachine stateMachine;

        CountdownTimer attackTimer;
        CountdownTimer chargeTimer;
        CountdownTimer combatTimer;
        CountdownTimer moveCooldownTimer;
        // Hyena attacks
        CountdownTimer spinAttackTimer;
        CountdownTimer spinAttackCooldownTimer;
        [SerializeField] float spinAttackCooldown = 1f;

        CountdownTimer chargeCooldownTimer;
        public float chargeCooldown = 1f;

        CountdownTimer despawnTimer;
        CountdownTimer bellyFlopAttackTimer;
        CountdownTimer bellyFlopCooldownTimer;
        [SerializeField] float timeBetweenBellyFlops = 10;
        [SerializeField] float bellyFlopCooldown = 3; 
        

        //dash
        CountdownTimer dashAttackTimer;
        public float timeBetweenDashes = 10;
        CountdownTimer dashCooldownTimer;
        public float dashCooldown = 1f;

        //Test shieeet
        CountdownTimer jumpAirTimer;
        [SerializeField] float airTime = 3f;
        //Melee jump curve
        public AnimationCurve HeightCurve { get; set; }

        //Boss curves
        public AnimationCurve bellyFlopCurve { get; set; }
        public AnimationCurve dashCurve { get; set; }
        

        AI_Ragdoll ragdoll;

        public AI_WeaponIK weaponIK;

        Health health;
        
        private AgentLinkMover LinkMover;
        
        public Collider Collider; // testi collider

        AI_Enemy_Positioning AiPositioning;

        CollectibleSpawnManager itemDrop;
        public EnemyBaseConfig enemyConfig;

        public SectionedAnimationSync animationSync;

        public ImpactDamageEffect impact;
        
        
        void OnValidate() => this.ValidateRefs();
        #endregion

        protected override void Awake()
        {
            base.Awake();
            UniqueID = System.Guid.NewGuid().ToString();
            SetEnemyFromConfig();
            enemyConfig.AddEnemy(this);
        }

        #region Start
        protected override void Start()
        {
            health = Stats.Health;
            
            foreach (var modifier in enemyConfig.StatModifiers)
            {
                Stats.Mediator.AddModifier(modifier);
            }
            
            base.Start();
            
            Debug.Log("Before setting, Current Health: " + Stats.Health.Current + " Max Health: " + Stats.Health.Max);
            health.SetCurrent(health.Max);
            Debug.Log("After setting, Current Health: " + Stats.Health.Current + " Max Health: " + Stats.Health.Max);
            impact = GetComponent<ImpactDamageEffect>();

            laser = GetComponentInChildren<Laser>();

            if (weaponIK !=null) { weaponIK.enabled = true; }
            if (laser != null) { laser.enabled = true; }

            #region Components    
            animationSync = GetComponentInChildren<SectionedAnimationSync>();

            AiPositioning = GetComponent<AI_Enemy_Positioning>();
            enemyID = UniqueID;
            //SetEnemyTypeData();
            melee = GetComponentInChildren<AI_Melee_Weapon>();

            itemDrop = GetComponentInChildren<CollectibleSpawnManager>();

            LinkMover = GetComponent<AgentLinkMover>();
            LinkMover.OnLinkEnd += HandleLinkEnd;
            LinkMover.OnLinkStart += HandleLinkStart;

            attackTimer = new CountdownTimer(timeBetweenAttacks);
            chargeTimer = new CountdownTimer(timeBetweenCharges); // Test
            combatTimer = new CountdownTimer(combatCooldown);
            moveCooldownTimer = new CountdownTimer(stunTime);
            //Hyena timers
            spinAttackTimer = new CountdownTimer(spinTime);
            spinAttackCooldownTimer = new CountdownTimer(spinAttackCooldown);
            chargeCooldownTimer = new CountdownTimer(chargeCooldown);

            despawnTimer = new CountdownTimer(despawnTime);
            jumpAirTimer = new CountdownTimer(airTime); //Test
            
            bellyFlopAttackTimer = new CountdownTimer(timeBetweenBellyFlops);
            bellyFlopCooldownTimer = new CountdownTimer(bellyFlopCooldown);

            dashAttackTimer = new CountdownTimer(timeBetweenDashes);
            dashCooldownTimer = new CountdownTimer(dashCooldown);

            isAlive = true;
            Collider = GetComponent<CapsuleCollider>(); // testi collider
            weaponIK = GetComponentInChildren<AI_WeaponIK>();
            ragdoll = GetComponentInChildren<AI_Ragdoll>();
            
            stateMachine = new StateMachine();

            CoverArea coverArea = FindObjectOfType<CoverArea>();
            #endregion
            
            #region Statemachine
            // general states
            var wanderState = new EnemyWanderState(this, animator, agent, wanderRadius);
            var chaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
            var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player, attackTimer, spinAttackTimer,moveCooldownTimer,AiPositioning);
            var reloadState = new EnemyReloadState(this, animator);
            var runToCoverState = new EnemyRunToCoverState(this, animator, coverArea, agent);
            var deathState = new EnemyDeathState(this, agent, animator, AiPositioning, despawnTimer);
            var chargeState = new EnemyChargeState(this, animator, agent, playerDetector.Player, playerDetector, chargeTimer);
            var newPositionState = new EnemyFindNewPositionState(this, animator, agent, AiPositioning);

            
            //At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            //At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));

            //Boss specific states
            var bellyFlopState = new BossBellyFlopState(this, animator, agent, bellyFlopAttackTimer);
            var dashState = new BossDashState(this, animator, agent, dashAttackTimer);

            if (!isMelee && !isBoss && !isSniper)
            {
                Any(newPositionState, new FuncPredicate(() => enemyDetected));
                At(newPositionState, chaseState, new FuncPredicate(() => !enemyDetected));

                //At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer() && !combatTimer.IsRunning));
                //At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer() && !combatTimer.IsRunning));

                At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer() && combatTimer.IsRunning && playerDetector.ConfirmLineOfSight()));
                At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer() && combatTimer.IsRunning && !playerDetector.ConfirmLineOfSight()));

                At(attackState, reloadState, new FuncPredicate(() => shooter.ShouldReload()));
                At(reloadState, attackState, new FuncPredicate(() => !shooter.ShouldReload()));
            }
            else if (!isMelee && !isBoss && isSniper)
            {
                //At(runToCoverState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer() && combatTimer.IsRunning && laser.IsTargetInSight()/* && AttackStateActive*/));
                //At(attackState, runToCoverState, new FuncPredicate(() => !laser.IsTargetInSight()&& !playerDetector.CanAttackPlayer()));
                Any(newPositionState, new FuncPredicate(() => enemyDetected));
                At(newPositionState, chaseState, new FuncPredicate(() => !enemyDetected));
                //At(newPositionState, runToCoverState, new FuncPredicate(() => !enemyDetected));

                //At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer() && !combatTimer.IsRunning));
                //At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer() && !combatTimer.IsRunning));

                At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer() && combatTimer.IsRunning && laser.IsTargetInSight()));
                At(attackState, chaseState, new FuncPredicate(() => /*!playerDetector.CanAttackPlayer() &&*/ combatTimer.IsRunning && !laser.IsTargetInSight()/* && attackTimer.IsFinished*/));

                At(attackState, reloadState, new FuncPredicate(() => shooter.ShouldReload()));
                At(reloadState, attackState, new FuncPredicate(() => !shooter.ShouldReload()));
            }
            else if (isMelee && !isBoss && !isSniper)
                {
                // chase -> charge -> chase
                At(chaseState, chargeState, new FuncPredicate(() => CanCharge() && combatTimer.IsRunning && !isAttacking && playerDetector.ConfirmLineOfSight()/* && playerDetector.CheckRoofCollision()*/));

                At(chargeState, chaseState, new FuncPredicate(() => !CanCharge() && combatTimer.IsRunning && !isCharging && !chargeCooldownTimer.IsRunning));

                //Death
                Any(deathState, new FuncPredicate(() => !isAlive));

                //wander -> chase -> wander
                //At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer() && !combatTimer.IsRunning));
                //At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer() && !combatTimer.IsRunning));

                //chase->attack->chase
                At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer() && combatTimer.IsRunning && !isCharging && !spinAttackTimer.IsRunning));
                //At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer() && combatTimer.IsRunning && !spinAttackCooldownTimer.IsRunning));
                At(attackState, chaseState, new FuncPredicate(() => !isAttacking && combatTimer.IsRunning && spinAttackCooldownTimer.IsFinished));


                //At(chargeState, chaseState, new FuncPredicate(() => !CanCharge() && !playerDetector.CanAttackPlayer()));
            }
            if (isBoss)
            {
                At(chaseState, bellyFlopState, new FuncPredicate(() => CanBellyFlop() && combatTimer.IsRunning));
                At(bellyFlopState, chaseState, new FuncPredicate(() => !CanBellyFlop() && combatTimer.IsRunning && !isBellyFlopping && !bellyFlopCooldownTimer.IsRunning));

                //At(chaseState, dashState, new FuncPredicate(() => CanBellyDash() && combatTimer.IsRunning));
                //At(dashState, chaseState, new FuncPredicate(() => !CanBellyDash() && combatTimer.IsRunning && !isDashing && !dashCooldownTimer.IsRunning));
                //chase->attack->chase
                At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer() && combatTimer.IsRunning && !attackTimer.IsRunning && !isBellyFlopping));
                At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer() && combatTimer.IsRunning && !isAttacking/* && !attackTimer.IsRunning*/));
            }
            Any(deathState, new FuncPredicate(() => !isAlive));
            //Any(runToCoverState, new FuncPredicate(() => TakeCover()));
            //At(runToCoverState, attackState, new FuncPredicate(() => ));

            if (isSniper)
            {
                stateMachine.SetState(chaseState);
            }
            else 
            {
                stateMachine.SetState(chaseState);
            }
            
            #endregion
        }
        #endregion

        #region Link Mover
        //Link mover
        private void HandleLinkStart(OffMeshLinkMoveMethod moveMethod)
        {
            // animations for jump
            animator.SetTrigger("LinkJump");
            agent.speed = 10f;
        }
        private void HandleLinkEnd(OffMeshLinkMoveMethod moveMethod) 
        {
            // animations for landed
            animator.SetTrigger("LinkLand");
            agent.speed = enemyConfig.Speed;
        }
        #endregion

        void At(IState_Interface from, IState_Interface to, IPredicate_Interface condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState_Interface to, IPredicate_Interface condition) => stateMachine.AddAnyTransition(to, condition);

        #region Update
        void Update()
        {
            if (isMelee) { GetDistance(); }
            if (isBoss) 
            {
                GetBellyFlopDistance();
                GetDashDistance();
                
            }
            SetEnemyFromConfig();

            CheckForEnemies();
            
            //if (!moveCooldownTimer.IsRunning)
            //{
            //    moveCooldownTimer.Start();

            //}
            //if (!moveCooldownTimer.IsRunning)
            //{
            //    enemyDetected = AiPositioning.DetectEnemies();
            //    AiPositioning.DetectEnemies(); //test
            //    moveCooldownTimer.Start();
            //}
            //enemyDetected = AiPositioning.DetectEnemies();
            
            MoveCooldownDebug = moveCooldownTimer.Progress;

            if (!moveCooldownTimer.IsRunning)
            {
                enemyDetected = AiPositioning.DetectEnemies();
                AiPositioning.DetectEnemies(); //test
                moveCooldownTimer.Start();
            }

            float deltaTime = Time.deltaTime;
            stateMachine.Update();
            attackTimer.Tick(deltaTime);
            moveCooldownTimer.Tick(deltaTime);
            chargeTimer.Tick(deltaTime);
            combatTimer.Tick(deltaTime);
            spinAttackTimer.Tick(deltaTime);
            spinAttackCooldownTimer.Tick(deltaTime);
            despawnTimer.Tick(deltaTime);
            jumpAirTimer.Tick(deltaTime);//test
            chargeCooldownTimer.Tick(deltaTime);
            bellyFlopAttackTimer.Tick(deltaTime);
            bellyFlopCooldownTimer.Tick(deltaTime);

            dashAttackTimer.Tick(deltaTime);
            dashCooldownTimer.Tick(deltaTime);

            float currentSpeed = agent.velocity.magnitude;
            float normalizedSpeed = currentSpeed / agent.speed;

            // Clamp the value to ensure it's between 0 and 1
            normalizedSpeed = Mathf.Clamp01(normalizedSpeed);
            animator.SetFloat("Speed", normalizedSpeed);

            if(playerDetector.CanDetectPlayer() && isAlive) { combatTimer.Start(); }

            if (playerDetector.CanDetectPlayer() && isAlive && !isMelee && !isBoss)
            {
                //combatTimer.Start();
                playerInSight = true;
                weaponIK.SetTargetTransform(playerDetector.Player);
                //Debug.Log(weaponIK.targetTransform + "From Enemy script");
                playerDetector.RotateTowardsPlayer(true);
            }
            else { playerInSight = false; weaponIK.SetTargetTransform(null); /*Debug.Log(weaponIK.targetTransform);*/ }

            if (isMelee && isAlive && !isBoss)
            {
                //combatTimer.Start();
                if (playerDetector.CanDetectPlayer())
                {
                    playerInSight = true;
                    if (!isCharging) 
                    {
                        playerDetector.RotateTowardsPlayer(true); 
                    }
                    else { playerDetector.RotateTowardsPlayer(false); }
                    
                }
                else { playerInSight = false; }
            }

            if (isBoss && isAlive && !isMelee)
            {
                //combatTimer.Start();
                if (playerDetector.CanDetectPlayer())
                {
                    playerInSight = true;

                    if (!isAttacking && !isBellyFlopping && !isDashing) 
                    {
                        playerDetector.RotateTowardsPlayer(true);
                    }
                    else { playerDetector.RotateTowardsPlayer(false); }
                    
                }
                else { playerInSight = false; }
            }

            isChargeTimerRunning(); // test debug
            isMoveCooldownTimerRunning(); // test debug
            isCombatTimerRunning();
            isSpinTimerRunning();
            isJumpAirTimerRunning();
            isChargeCooldownTimerRunning();
            isAttackTimerRunning();
            
            

            if (isMelee && inChargeRange) 
            {
                if (chargeTimer.IsRunning) { return; }
                CanCharge();
            }
        }

        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }
        #endregion
        
        #region Functions

        bool coverThresh;
        public bool TakeCover()
        {
            if (health.Current < 50)
            {
                coverThresh = true;
            }
            else
            {
                coverThresh = false;
            }
            return coverThresh;
        }

        public override bool Damage(ref Damage damage, bool handlehealthLoweringManually = false)
        {
            if (!isMelee) // Hyena needs a fix for this to work
            {
                ragdoll.ApplyForce(damage.KnockBackDirection * damage.KnockBackForce);
            }
            
            return base.Damage(ref damage);
        }

        protected override void Death()
        {
            if (!Application.isPlaying) { return; }
            if (!isAlive) { return; }
            
            base.Death();
            
            Combo.Increase(comboOnDeath);
            Blood.AddBlood(Random.Range(bloodOnDeath.x, bloodOnDeath.y));

            SoundManager.Instance.CreateSound()
                .WithSoundData(DeathSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();

            despawnTimer.Start();

            if (Random.Range(0, 100) < itemDropChance)
                itemDrop.enabled = true;

            animator.SetTrigger("IsDead");
            if (ragdoll != null) { ragdoll.ActivateRagdoll(); }

            AiPositioning.anchorPos.SetActive(false);
            Collider.enabled = false; // testi collider
            isAlive = false;
            if (isSniper) 
            {
                laser.DisableLineTrace();
                laser.enabled = false;
            }
            //if (laser != null) { laser.enabled = false; }
            weaponIK.enabled = false;
            agent.enabled = false;
            this.enabled = false;
            playerDetector.enabled = false;
        }

        private void OnDestroy()
        {
            enemyConfig.RemoveEnemy(this);
            Death();
        }

        private void GetDistance()
        {
            if (playerDetector.Player == null)
            {
                return;
            }
            float minChargeDist = enemyConfig.MinChargeDistance;
            float chargeDist = Vector3.Distance(playerDetector.Player.transform.position, this.transform.position);
            distanceToPlayer = chargeDist; // debug
            if (minChargeDist < chargeDist && chargeDist < (/*playerDetector.GetAttackRange() +*/ enemyConfig.ChargeDistance))
            {
                inChargeRange = true;

            }
            else if (minChargeDist > chargeDist) { inChargeRange = false; }
            else { inChargeRange = false; }
            
        }


        public bool CanCharge()
        {
            if(!chargeTimer.IsRunning && playerDetector.CanDetectPlayer() && inChargeRange && !isAttacking) { return true; }
            else { return false; }
        }

        private bool isAttacking = false;
        public void SpinAttack()
        {
            
            // Avoid starting a new spin attack if one is already in progress
            if (spinAttackCooldownTimer.IsRunning)
                return;

            // Start the spin attack
            isAttacking = true;
            
            animator.SetTrigger("Attack");
            spinAttackTimer.Start();

            // Start a coroutine to handle the spin attack asynchronously
            StartCoroutine(HandleSpinAttack());
        }

        private IEnumerator HandleSpinAttack()
        {
            // While the timer is running, set spinning state
            while (!spinAttackTimer.IsFinished)
            {
                animator.SetBool("IsSpinning", true);
                agent.speed = enemyConfig.SpinMoveSpeed;
                yield return null; // Wait until the next frame
            }

            // End the spin attack
            animator.SetBool("IsSpinning", false);
            isAttacking = false;
            agent.speed = enemyConfig.Speed;
            // Start the cooldown timer
            spinAttackCooldownTimer.Start();
        }

        public void StartCharge()
        {
            if (chargeTimer.IsRunning) { return; }
            chargeTimer.Start();
            //Debug.Log("StartCharge called");
            StartCoroutine(Charge());
            
        }

        private bool isCharging = false;
        public IEnumerator Charge()
        {
            isCharging = true;
            animator.SetTrigger("Charge");

            agent.enabled = false;

            playerDetector.RotateTowardsPlayer(false);
            Vector3 startingPosition = transform.position;
            Vector3 offset = new Vector3(0, 0, -2);
            Vector3 targetPosition = playerDetector.Player.position + offset;

            for (float time = 0; time < 1; time += Time.deltaTime * enemyConfig.ChargeSpeed)
            {
                animator.SetBool("InAir", true);
                transform.position = Vector3.Lerp(startingPosition, targetPosition, time)
                    + Vector3.up * HeightCurve.Evaluate(time);
                yield return null;
            }
            //Debug.Log("Charge air time loop over");
            animator.SetBool("InAir", false);
            //animator.SetTrigger("Landed");
            if (isAlive) { melee.PerformChargeAttack(); }
            agent.enabled = true;
            chargeCooldownTimer.Start();
            

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, agent.areaMask))
            {
                agent.Warp(hit.position);
            }
            isCharging = false;
        }

        // boss melee
        public void Punch()
        {
            
            if (attackTimer.IsRunning) { return; }
            //attackTimer.Start();
            agent.speed = 0;
            int attackIndex = Random.Range(0, 3);



            //Debug.Log($"Attackindex: {attackIndex}, Attack timer is running?: {attackTimer.Progress}");
            switch (attackIndex)
            {
                case 0:
                    LeftPunch();
                    
                    break;
                case 1:
                    RightPunch();
                    
                    break;
                case 2:
                    SlamAttack();
                    
                    break;
            }
        }

        public void LeftPunch()
        {
            ////un segmented
            //if (attackTimer.IsRunning) { return; }
            //isAttacking = true;
            //animator.SetBool("IsAttacking", true);
            //animator.SetTrigger("LeftPunch");
            //attackTimer.Start();
            //animator.SetBool("IsAttacking", false);
            //isAttacking = false;

            //Segmented
            if (attackTimer.IsRunning) { return; }
            attackTimer.Start();

            StartCoroutine(LeftPunchSegments());
        }
        public IEnumerator LeftPunchSegments()
        {
            if (isAttacking) yield break;
            isAttacking = true;
            animator.SetBool("IsAttacking", true);
            // Play the first section ()
            animationSync.PlayAnimation("Left_Punch"); // Ensure this matches your animation name
            //animationSync.SyncSectionProgress(0, 0); // Sync to the beginning of the first section
            //yield return new WaitForSeconds(animationSync.animations[2].animationSections[0].targetDuration);
            SectionedAnimationSync.Section windupSection = animationSync.animations[2].animationSections[0];
            float windupDuration = windupSection.targetDuration;
            for (float time = 0; time < windupDuration; time += Time.deltaTime)
            {
                float progress = time / windupDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(0, progress);
                yield return null;
            }

            // Play the second section () and sync with movement
            //animationSync.SyncSectionProgress(1, 0);
            //yield return new WaitForSeconds(animationSync.animations[2].animationSections[1].targetDuration);
            SectionedAnimationSync.Section punchSection = animationSync.animations[2].animationSections[1];
            float punchDuration = punchSection.targetDuration;
            for (float time = 0; time < punchDuration; time += Time.deltaTime)
            {
                float progress = time / punchDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(1, progress);
                yield return null;
            }
            
            // Play the third section ()
            //animationSync.SyncSectionProgress(2, 0);
            //yield return new WaitForSeconds(animationSync.animations[2].animationSections[2].targetDuration);
            SectionedAnimationSync.Section recoverySection = animationSync.animations[2].animationSections[2];
            float recoveryDuration = recoverySection.targetDuration;
            for (float time = 0; time < recoveryDuration; time += Time.deltaTime)
            {
                float progress = time / recoveryDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(2, progress);
                yield return null;
            }


            animator.SetBool("IsAttacking", false);
            animator.speed = 1f;
            isAttacking = false;
        }
        public void RightPunch()
        {
            //un Segmented
            //if (attackTimer.IsRunning) { return; }
            //isAttacking = true;
            //animator.SetBool("IsAttacking", true);
            //animator.SetTrigger("RightPunch");
            //attackTimer.Start();
            //animator.SetBool("IsAttacking", false);
            //isAttacking = false;

            //Segmented
            if (attackTimer.IsRunning) { return; }
            attackTimer.Start();
            StartCoroutine(RightPunchSegments());
        }

        public IEnumerator RightPunchSegments()
        {
            if (isAttacking) yield break;
            isAttacking = true;
            animator.SetBool("IsAttacking", true);

            // Play the first section ()
            animationSync.PlayAnimation("Right_Punch"); // Ensure this matches your animation name
            
            SectionedAnimationSync.Section windupSection = animationSync.animations[3].animationSections[0];
            float windupDuration = windupSection.targetDuration;
            for (float time = 0; time < windupDuration; time += Time.deltaTime)
            {
                float progress = time / windupDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(0, progress);
                yield return null;
            }

            // Play the second section () and sync with movement
            
            SectionedAnimationSync.Section punchSection = animationSync.animations[3].animationSections[1];
            float punchDuration = punchSection.targetDuration;
            for (float time = 0; time < punchDuration; time += Time.deltaTime)
            {
                float progress = time / punchDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(1, progress);
                yield return null;
            }
            
            // Play the third section ()
            //animationSync.SyncSectionProgress(2, 0);
            //yield return new WaitForSeconds(animationSync.animations[3].animationSections[2].targetDuration);
            SectionedAnimationSync.Section recoverySection = animationSync.animations[3].animationSections[2];
            float recoveryDuration = recoverySection.targetDuration;
            for (float time = 0; time < recoveryDuration; time += Time.deltaTime)
            {
                float progress = time / recoveryDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(2, progress);
                yield return null;
            }



            animator.SetBool("IsAttacking", false);
            animator.speed = 1f;
            isAttacking = false;
        }

        public void SlamAttack()
        {
            //if (attackTimer.IsRunning) { return; }
            //isAttacking = true;
            //animator.SetBool("IsAttacking", true);
            //animator.SetTrigger("SlamAttack");
            //attackTimer.Start();
            //animator.SetBool("IsAttacking", false);
            //isAttacking = false;
            if (attackTimer.IsRunning) { return; }
            StartCoroutine(SlamSegments());
        }

        public IEnumerator SlamSegments()
        {
            if (isAttacking) yield break;
            isAttacking = true;
            animator.SetBool("IsAttacking", true);

            // Play the first section ()
            animationSync.PlayAnimation("Slam_Attack"); // Ensure this matches your animation name

            SectionedAnimationSync.Section windupSection = animationSync.animations[4].animationSections[0];
            float windupDuration = windupSection.targetDuration;
            for (float time = 0; time < windupDuration; time += Time.deltaTime)
            {
                float progress = time / windupDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(0, progress);
                yield return null;
            }

            // Play the second section () and sync with movement

            SectionedAnimationSync.Section slamSection = animationSync.animations[4].animationSections[1];
            float slamDuration = slamSection.targetDuration;
            for (float time = 0; time < slamDuration; time += Time.deltaTime)
            {
                float progress = time / slamDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(1, progress);
                yield return null;
            }

            // Play the third section ()
            //animationSync.SyncSectionProgress(2, 0);
            //yield return new WaitForSeconds(animationSync.animations[3].animationSections[2].targetDuration);
            SectionedAnimationSync.Section recoverySection = animationSync.animations[4].animationSections[2];
            float recoveryDuration = recoverySection.targetDuration;
            for (float time = 0; time < recoveryDuration; time += Time.deltaTime)
            {
                float progress = time / recoveryDuration;


                // Sync animation progress
                animationSync.SyncSectionProgress(2, progress);
                yield return null;
            }

            animator.SetBool("IsAttacking", false);
            animator.speed = 1f;
            isAttacking = false;
        }
        //Boss BellyFlop

        private void GetBellyFlopDistance()
        {
            if (playerDetector.Player == null)
            {
                return;
            }
            float minBellyFlopDist = enemyConfig.MinBellyFlopDistance;
            float bellyFlopDist = Vector3.Distance(playerDetector.Player.transform.position, this.transform.position);
            
            if (minBellyFlopDist < bellyFlopDist && bellyFlopDist < (/*playerDetector.GetAttackRange() +*/ enemyConfig.BellyFlopDistance))
            {
                inBellyFlopRange = true;

            }
            else if (minBellyFlopDist > bellyFlopDist) { inBellyFlopRange = false; }
            else { inBellyFlopRange = false; }

        }
        public bool CanBellyFlop()
        {
            
            
            if (!bellyFlopAttackTimer.IsRunning && playerDetector.CanDetectPlayer() && inBellyFlopRange && !isAttacking && !isDashing) { return true; }
            else { return false; }
        }

        public void StartBellyFlop()
        {
            if (bellyFlopAttackTimer.IsRunning) { return; } // replace with new timer
            bellyFlopAttackTimer.Start();
            //Debug.Log("StartCharge called");
            
            StartCoroutine(BellyFlop());

        }
        
        private bool isBellyFlopping = false;

        public IEnumerator BellyFlop()
        {
            if (isBellyFlopping) yield break; // Prevent multiple belly flops at once

            isBellyFlopping = true;
            animator.SetBool("IsBellyFlopping", true);

            // Disable the NavMeshAgent during the belly flop
            agent.speed = 0;
            agent.enabled = false;

            //playerDetector.RotateTowardsPlayer(false);

            // Define starting and target positions
            Vector3 startingPosition = transform.position;
            Vector3 offset = new Vector3(0, 0, -2); // Adjust as needed
            Vector3 targetPosition = playerDetector.Player.position + offset;

            // Total distance to travel
            float totalDistance = Vector3.Distance(startingPosition, targetPosition);

            playerDetector.RotateTowardsPlayer(false);

            // Play the first section (jump animation)
            animationSync.PlayAnimation("BellyFlop"); // Ensure this matches your animation name
            animationSync.SyncSectionProgress(0, 0); // Sync to the beginning of the first section
            yield return new WaitForSeconds(animationSync.animations[0].animationSections[0].targetDuration);

            // Play the second section (in-air animation) and sync with movement
            SectionedAnimationSync.Section inAirSection = animationSync.animations[0].animationSections[1];
            float inAirDuration = inAirSection.targetDuration;

            for (float time = 0; time < inAirDuration; time += Time.deltaTime * enemyConfig.bellyFlopSpeed)
            {
                float progress = time / inAirDuration;

                // Update position based on progress
                transform.position = Vector3.Lerp(startingPosition, targetPosition, progress)
                    + Vector3.up * bellyFlopCurve.Evaluate(progress);

                // Sync animation progress
                animationSync.SyncSectionProgress(1, progress);
                yield return null;
            }

            // Play the third section (landing animation)
            animationSync.SyncSectionProgress(2, 0); // Start landing
            yield return new WaitForSeconds(animationSync.animations[0].animationSections[2].targetDuration);

            // Finalize the belly flop
            animator.SetBool("IsBellyFlopping", false);
            animator.speed = 1f;

            // Re-enable NavMeshAgent
            agent.speed = enemyConfig.Speed;
            agent.enabled = true;
            bellyFlopCooldownTimer.Start();
            isBellyFlopping = false;

            // Warp to the final position on the NavMesh
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2f, agent.areaMask))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("Unable to warp agent to the final position.");
            }

            Debug.Log("Belly flop complete.");
        }


        //Boss dash

        private void GetDashDistance()
        {
            if (playerDetector.Player == null)
            {
                return;
            }
            float minDashDist = enemyConfig.MinDashDistance;
            float dashDist = Vector3.Distance(playerDetector.Player.transform.position, this.transform.position);

            if (minDashDist < dashDist && dashDist < (/*playerDetector.GetAttackRange() +*/ enemyConfig.DashDistance))
            {
                inDashRange = true;

            }
            else if (minDashDist > dashDist) { inDashRange = false; }
            else { inDashRange = false; }
        }
        public bool CanBellyDash()
        {
            if (!dashAttackTimer.IsRunning && playerDetector.CanDetectPlayer() && inDashRange && !isAttacking && !isBellyFlopping) { return true; }
            else { return false; }
        }

        public void StartBellyDash()
        {
            if (dashAttackTimer.IsRunning) { return; } // replace with new timer
            dashAttackTimer.Start();
            //Debug.Log("StartCharge called");

            StartCoroutine(BellyDash());
        }

        
        private bool isDashing = false;

        public IEnumerator BellyDash()
        {
            if (isDashing) yield break; // Prevent multiple belly flops at once

            isDashing = true;
            animator.SetBool("IsDashing", true);

            
            agent.speed = 0;
            agent.enabled = false;

           playerDetector.RotateTowardsPlayer(false);

            // Define starting and target positions
            Vector3 startingPosition = transform.position;
            Vector3 offset = new Vector3(0, 0, -2); // Adjust as needed
            Vector3 targetPosition = playerDetector.Player.position + offset;
            
            // Play the first section (jump animation)
            animationSync.PlayAnimation("BellyDash"); // Ensure this matches your animation name
            //animationSync.SyncSectionProgress(0, 0); // Sync to the beginning of the first section
            //yield return new WaitForSeconds(animationSync.animations[1].animationSections[0].targetDuration);
            //test start
            SectionedAnimationSync.Section jumpSection = animationSync.animations[1].animationSections[0];
            float jumpDuration = jumpSection.targetDuration;
            for (float time = 0; time < jumpDuration; time += Time.deltaTime)
            {
                float progress = time / jumpDuration;

                // Sync animation progress
                animationSync.SyncSectionProgress(0, progress);
                yield return null;
            }
            //test end
            
            // Total distance to travel
            float totalDistance = Vector3.Distance(startingPosition, targetPosition);

            // Play the second section (in-air animation) and sync with movement
            SectionedAnimationSync.Section inAirSection = animationSync.animations[1].animationSections[1];
            float inAirDuration = inAirSection.targetDuration;

            for (float time = 0; time < inAirDuration; time += Time.deltaTime * enemyConfig.DashSpeed)
            {
                float progress = time / inAirDuration;

                // Update position based on progress
                transform.position = Vector3.Lerp(startingPosition, targetPosition, progress)
                    + Vector3.up * dashCurve.Evaluate(progress);

                // Sync animation progress
                animationSync.SyncSectionProgress(1, progress);
                yield return null;
            }

            // Play the third section (landing animation)
            animationSync.SyncSectionProgress(2, 0); // Start landing
            yield return new WaitForSeconds(animationSync.animations[1].animationSections[2].targetDuration);

            // Finalize the belly flop
            animator.SetBool("IsDashing", false);
            animator.speed = 1f;

            // Re-enable NavMeshAgent
            agent.speed = enemyConfig.Speed;
            agent.enabled = true;
            dashCooldownTimer.Start();
            

            // Warp to the final position on the NavMesh
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2f, agent.areaMask))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("Unable to warp agent to the final position.");
            }

            Debug.Log("Dash complete.");
            isDashing = false;
        }
        
        public void CheckForEnemies()
        {
            if (moveCooldownTimer.IsRunning) { return; }
            enemyDetected = AiPositioning.DetectEnemies();
            moveCooldownTimer.Start() ;
        }
        public bool IsEnemyMelee()
        {
            if (isMelee) { return true; }
            else { return false; }
        }
        public bool IsEnemyBoss()
        {
            if (isBoss) { return true; }
            else { return false; }
        }
        public bool IsEnemySniper()
        {
            if (isSniper) { return true; }
            else { return false; }
        }

        public bool isCombatTimerRunning()
        {
            if (combatTimer.IsRunning) 
            { 
                combatTimerIsRunning = true; 
                animator.SetBool("IsInCombat", true); 
                return true; 
            }
            else { 
                combatTimerIsRunning = false; 
                animator.SetBool("IsInCombat", false); 
                return false; 
            }
        }
        public bool isChargeTimerRunning()
        {
            if (chargeTimer.IsRunning) { chargeTimerIsRunning = true; return true; }
            else { chargeTimerIsRunning = false; return false; }
        }

        public bool isMoveCooldownTimerRunning()
        {
            if (moveCooldownTimer.IsRunning) { moveCooldownTimerIsRunning = true; return true; }
            else { moveCooldownTimerIsRunning = false; return false; }
        }

        public bool isSpinTimerRunning()
        {
            if (spinAttackTimer.IsRunning) { spinAttackTimerIsRunning = true; animator.SetBool("IsSpinning", true); return true; }
            else { spinAttackTimerIsRunning = false; animator.SetBool("IsSpinning", false); return false; }
        }

        public bool isJumpAirTimerRunning()
        {
            if (jumpAirTimer.IsRunning) { jumpAirTimerIsRunning = true; animator.SetBool("InAir", true); return true; }
            else { jumpAirTimerIsRunning = false; animator.SetBool("InAir", false); return false; }
        }

        public bool isChargeCooldownTimerRunning()
        {
            if (chargeCooldownTimer.IsRunning) { chargeCooldownTimerIsRunning = true;  return true; }
            else { chargeCooldownTimerIsRunning = false;  return false; }
        }
        public bool isAttackTimerRunning()
        {
            if (attackTimer.IsRunning) { attackTimerIsRunning = true; return true; }
            else { attackTimerIsRunning = false; return false; }
        }

        public void SetEnemyFromConfig()
        {
            //General
            isMelee = enemyConfig.IsMelee;
            isBoss = enemyConfig.IsBoss;
            //NavMesh Setup
            agent.acceleration = enemyConfig.Acceleration;
            agent.angularSpeed = enemyConfig.AngularSpeed;
            agent.areaMask = enemyConfig.AreaMask;
            agent.avoidancePriority = enemyConfig.AvoidancePriority;
            agent.baseOffset = enemyConfig.BaseOffset;
            agent.height = enemyConfig.Height;
            agent.obstacleAvoidanceType = enemyConfig.ObstacleAvoidanceType;
            agent.radius = enemyConfig.Radius;
            agent.speed = enemyConfig.Speed;
            agent.stoppingDistance = enemyConfig.StoppingDistance;

            //Player Detector Setup
            playerDetector.detectionAngle = enemyConfig.DetectionAngle;
            playerDetector.detectionRadius = enemyConfig.DetectionRadiusOuter;
            playerDetector.innerDetectionRadius = enemyConfig.DetectionRadiusInner;
            playerDetector.detectionCooldown = enemyConfig.DetectionCooldown;
            playerDetector.attackRange = enemyConfig.AttackRange;
            playerDetector.rotationSpeed = enemyConfig.RotationSpeed;

            //Melee setup
            HeightCurve = enemyConfig.JumpHeightCurve;

            //Boss Setup
            bellyFlopCurve = enemyConfig.BellyFlopCurve;
            dashCurve = enemyConfig.DashCurve;
            
        }
        #endregion

        //private void SetEnemyTypeData()
        //{
        //    if (!isMelee)
        //    {
        //        //ranged setup
        //        shooter.damage = enemyConfig.RangedDamage;
        //        shooter.ammo = enemyConfig.ClipSize;
        //    }

        //    if (isMelee)
        //    {
        //        //Melee setup
        //        int randomizedDamage = Random.Range(enemyConfig.MinSpinDamage, (enemyConfig.MaxSpinDamage + 1)); // Spin damage
        //        melee.damage = randomizedDamage;

        //        melee.chargeDamage = enemyConfig.ChargeAttackDamage;// Charge attack
        //    }
        //}

        #region Debug
        [Header("State debug")]
        public bool WanderStateActive;
        public bool ChaseStateActive;
        public bool AttackStateActive;
        public bool ChargeStateActive;
        public bool FindNewPositionStateActive;
        [Header("Parameter debug")]
        public bool attackTimerIsRunning;
        public bool chargeTimerIsRunning;
        public bool chargeCooldownTimerIsRunning;
        public bool combatTimerIsRunning;
        public bool moveCooldownTimerIsRunning;
        public float MoveCooldownDebug;
        public bool spinAttackTimerIsRunning;
        public bool jumpAirTimerIsRunning;
        public bool inChargeRange;
        public bool inBellyFlopRange;
        public bool inDashRange;
        public float distanceToPlayer;
        public bool enemyDetected;
        #endregion

        
    }
}