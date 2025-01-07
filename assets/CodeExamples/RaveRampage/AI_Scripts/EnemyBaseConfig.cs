using System;
using System.Collections.Generic;
using System.Diagnostics;
using DefaultNamespace;
using Player.Items;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace Cyber_Slicer_AI
{
    [Serializable]
    public struct EnemyStatModifierData : IEquatable<EnemyStatModifierData>
    {
        public StatModifierData StatModifierData;
        public float RoundMultiplier;
        [Range(0,1)]
        public float Chance;

        public int ActivationRound;

        public bool Equals(EnemyStatModifierData other)
        {
            return StatModifierData.Equals(other.StatModifierData) && RoundMultiplier.Equals(other.RoundMultiplier) && Chance.Equals(other.Chance);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyStatModifierData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StatModifierData, RoundMultiplier, Chance);
        }
    }
    
    [CreateAssetMenu(fileName = "Enemy Configuration", menuName = "AI Configuration/Enemy Base Config")]
    public class EnemyBaseConfig : EntityData
    {
        private List<Enemy> enemyList;
        
        public List<EnemyStatModifierData> EnemyStatModifierDatas;
        public List<StatModifier> StatModifiers { get; private set; } = new ();
        
        private void OnEnable()
        {
            GameEvents.OnRoundStart += OnRoundStart;
            GameEvents.OnGameStart += ClearModifiers;
            GameEvents.OnGameEnd += ClearModifiers;
        }

        private void OnDisable()
        {
            GameEvents.OnRoundStart -= OnRoundStart;
            GameEvents.OnGameStart -= ClearModifiers;
            GameEvents.OnGameEnd -= ClearModifiers;
        }

        private void OnRoundStart(int round)
        {
            if (round == 1)
                return;
            
            if (EnemyStatModifierDatas.Count == 0)
                return;
            
            // var randomModifierData = StatModifierDatas[Random.Range(0, StatModifierDatas.Count)];
            // StatModifiers.Add(randomModifierData.GetModifier());
            
            foreach (var modifierData in EnemyStatModifierDatas)
            {
                if (round < modifierData.ActivationRound)
                    continue;
                if (Random.value > modifierData.Chance)
                    continue;
                
                var statModifierData = modifierData.StatModifierData;
                var modifiedData = statModifierData;
                var valueMultiplier = Mathf.Pow(Mathf.Max(1, modifierData.RoundMultiplier), round - 2);
                modifiedData.Value = Mathf.RoundToInt(statModifierData.Value * valueMultiplier);
                StatModifiers.Add(modifiedData.GetModifier());
                Debug.Log("Enemy Stat Modifier Added to " + name + ": " + modifiedData.StatType + " " + modifiedData.Value);
            }
        }
        private void ClearModifiers()
        {
            StatModifiers.Clear();
        }
        
        [Conditional("UNITY_EDITOR")]
        public void AddEnemy(Enemy enemy)
        {
            if(enemyList == null) {  enemyList = new List<Enemy>(); }
            enemyList.Add(enemy); 
        }

        [Conditional("UNITY_EDITOR")]
        public void RemoveEnemy(Enemy enemy) 
        { 
            enemyList.Remove(enemy);
        }
        private void OnValidate()
        {
            if(enemyList == null) { return; }
            foreach (Enemy enemy in enemyList) 
            {
                enemy.SetEnemyFromConfig();
                
            }
        }
        [Header("Parameter Setter")]
        public bool IsMelee;
        public bool IsBoss;

        [Header("NavMesh Setup")]
        public float Acceleration = 8;
        public float AngularSpeed = 120;
        public int AreaMask = -1; // -1 means all.
        public int AvoidancePriority = 50;
        public int BaseOffset = 0;
        public float Height = 2f;
        public ObstacleAvoidanceType ObstacleAvoidanceType;
        public float Radius = 0.5f;
        public float Speed = 3f;
        public float StoppingDistance = 0.5f;

        [Header("Detector Setup")]
        public int DetectionAngle = 60;
        public int DetectionRadiusOuter = 10;
        public int DetectionRadiusInner = 5;
        public float DetectionCooldown = 1;
        public float AttackRange = 3f;
        public int RotationSpeed = 5;


        [Header("Melee Setup")]
        public float MinChargeDistance = 7f;
        public float ChargeDistance = 12f;
        public float ChargeSpeed = 0;
        public float ChargeAcceleration = 0;
        public AnimationCurve JumpHeightCurve;
        public int MinSpinDamage = 1;
        public int MaxSpinDamage = 3;
        public int SpinMoveSpeed = 10;
        public int MeleeDamage = 2;
        public int ChargeAttackDamage = 20;


        [Header("Ranged Setup")]
        public string test = "wheeeEEeeEEEEeee, why do these keep breaking!";
        public int RangedDamage = 10;
        public int ClipSize = 30;

        [Header("Sniper Setup")]
        public float CoverStoppingDistance = 0;
        public float SpeedInCover = 0;

        [Header("Boss Setup")]
        public AnimationCurve BellyFlopCurve;
        public float bellyFlopSpeed = 1;
        public AnimationCurve DashCurve;
        public float DashSpeed = 1;
        public AnimationCurve ElbowSmashCurve;
        public float ElbowSmashSpeed = 1;
        public float MinBellyFlopDistance = 15;
        public float BellyFlopDistance = 20;
        public float MinDashDistance = 10;
        public float DashDistance = 25;
        public int PunchDamage = 10;
        public int SlamDamage = 25;
        public int BellyFlopDamage = 40;
    }
}

