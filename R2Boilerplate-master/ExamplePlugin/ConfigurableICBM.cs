using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using ExamplePlugin;

namespace ConfigurableICBM
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ConfigurableICBM : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "TaranDev";
        public const string PluginName = "ConfigurableICBM";
        public const string PluginVersion = "1.0.0";

        public static ConfigEntry<float> extraMissilesPerStack;

        public static ConfigEntry<bool> removeExtraMissileStackLimit;

        public static ConfigEntry<float> extraDamagePerStack;

        public void Awake() {
            configs();
        }

        private void OnEnable()
        {
            On.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool += OnFireMissile;
        }

        private void OnDisable()
        {
            On.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool -= OnFireMissile;

        }

        private static void OnFireMissile(On.RoR2.MissileUtils.orig_FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool orig, Vector3 position, CharacterBody attackerBody, ProcChainMask procChainMask, GameObject victim, float missileDamage, bool isCrit, GameObject projectilePrefab, DamageColorIndex damageColorIndex, Vector3 initialDirection, float force, bool addMissileProc) {
            int num = attackerBody?.inventory?.GetItemCount(DLC1Content.Items.MoreMissile) ?? 0;
            float num2 = Mathf.Max(1f, 1f + extraDamagePerStack.Value * (float)(num - 1));
            InputBankTest component = attackerBody.GetComponent<InputBankTest>();
            ProcChainMask procChainMask2 = procChainMask;
            if (addMissileProc) {
                procChainMask2.AddProc(ProcType.Missile);
            }
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.projectilePrefab = projectilePrefab;
            fireProjectileInfo.position = position;
            fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(initialDirection);
            fireProjectileInfo.procChainMask = procChainMask2;
            fireProjectileInfo.target = victim;
            fireProjectileInfo.owner = attackerBody.gameObject;
            fireProjectileInfo.damage = missileDamage * num2;
            fireProjectileInfo.crit = isCrit;
            fireProjectileInfo.force = force;
            fireProjectileInfo.damageColorIndex = damageColorIndex;
            FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
            ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
            if (num > 0) {
                Vector3 axis = (component ? component.aimDirection : attackerBody.transform.position);

                int numExtraMissilesToFire;
                if (removeExtraMissileStackLimit.Value) {
                    int extraMissilesPerStackInt = Mathf.RoundToInt(extraMissilesPerStack.Value);

                    if(extraMissilesPerStackInt > 0) {
                        numExtraMissilesToFire = num * extraMissilesPerStackInt;
                    } else {
                        numExtraMissilesToFire = 0;
                    }
                    
                } else {
                    numExtraMissilesToFire = 2;
                }
                
                float angleIncrement = 90f / numExtraMissilesToFire;
                float fireAngle = -45;
                for (int i = 0; i < numExtraMissilesToFire; i++) {
                    FireProjectileInfo fireProjectileInfo3 = fireProjectileInfo2;
                    fireProjectileInfo3.rotation = Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(fireAngle, axis) * initialDirection);
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo3);
                    fireAngle += angleIncrement;
                    if(Mathf.Abs(fireAngle) < 0.01) {
                        // Already fired centre missile, so skip this angle
                        fireAngle += angleIncrement;
                    }

                }
            }
        }

        private void configs() {

            extraMissilesPerStack = Config.Bind("General", "Extra Missiles Granted", 2f, "Number of extra missiles ICBM fires.\nDefault is 2.");
            ModSettingsManager.AddOption(new StepSliderOption(extraMissilesPerStack,
                new StepSliderConfig
                {
                    min = 0f,
                    max = 50f,
                    increment = 1f
                }));

            removeExtraMissileStackLimit = Config.Bind("General", "Extra Missile Scaling", true, "If each ICBM stack should fire extra missiles rather than just the first.\nMod default is true, base game default is false.");
            ModSettingsManager.AddOption(new CheckBoxOption(removeExtraMissileStackLimit));

            extraDamagePerStack = Config.Bind("General", "Additional Missile Damage Per Stack", 0.5f, "How much extra damage should be granted to missiles for each ICBM.\nEach increment of 1 is +100% damage, so 1 is +100% damage and 5 is +500% base damage.\nDefault is 0.5 (i.e. 50%).");
            ModSettingsManager.AddOption(new StepSliderOption(extraDamagePerStack,
                new StepSliderConfig
                {
                    min = 0f,
                    max = 50f,
                    increment = 0.05f
                }));
        }
    }
}
