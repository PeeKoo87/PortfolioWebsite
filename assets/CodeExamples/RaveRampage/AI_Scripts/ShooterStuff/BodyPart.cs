using System;
using UnityEngine;
using UnityUtils;

public enum BodyPartType
{
    Head,
    Torso,
    Leg,
    Arm
}

public class BodyPart : MonoBehaviour, IDamageable, ISurfaceTypeProvider
{
    [SerializeField] private Unit unit;
    [SerializeField] private float damageMultiplier = 1f;
    
    [SerializeField] private BodyPartType bodyPartType = BodyPartType.Torso;
    public BodyPartType Type => bodyPartType;
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Flesh;

    private void Start()
    {
        unit.OnDeath += () => gameObject.layer = LayerMask.NameToLayer("GroundCollision");
    }

    public bool Damage(ref Damage damage, bool handlehealthLoweringManually = false)
    {
        var damageAmount = (damage.Type is DamageType.PercentageCurrent or DamageType.PercentageMax)
            ? damage.Amount
            : Mathf.RoundToInt(damage.Amount * damageMultiplier);
        
        damage.Amount = damageAmount;
        
        return unit.Damage(ref damage);
    }
    
    public Unit GetUnit() => unit;

    public SurfaceType GetSurfaceType() => surfaceType;
}
