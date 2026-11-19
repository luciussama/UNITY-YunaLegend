using System;
using UnityEngine;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    [Serializable]
    public struct ArenaEnemySpriteSet
    {
        public ArenaEnemyId EnemyId;
        public Sprite[] Idle;
        public Sprite[] Walk;
        public Sprite[] PrimaryAttack;
        public Sprite[] SecondaryAttack;
        public Sprite[] TertiaryAttack;
        public Sprite[] Enrage;
        public Sprite[] Hit;
        public Sprite[] Death;
    }

    /// <summary>
    /// Catalogo de sprites fatiados da Arena (menina e os 4 brinquedos adversarios).
    /// </summary>
    public sealed class ArenaSpriteCatalog : MonoBehaviour
    {
        [Header("Yuna (Arena)")]
        [SerializeField] private Sprite[] girlIdle;
        [SerializeField] private Sprite[] girlRun;
        [SerializeField] private Sprite[] girlJump;
        [SerializeField] private Sprite[] girlCrouch;
        [SerializeField] private Sprite[] girlAttack;
        [SerializeField] private Sprite[] girlGuard;
        [SerializeField] private Sprite[] girlDamage;
        [SerializeField] private Sprite eyesPowerCutin;

        [Header("Inimigos")]
        [SerializeField] private ArenaEnemySpriteSet[] enemies;

        public Sprite[] GirlIdle => girlIdle;
        public Sprite[] GirlRun => girlRun;
        public Sprite[] GirlJump => girlJump;
        public Sprite[] GirlCrouch => girlCrouch;
        public Sprite[] GirlAttack => girlAttack;
        public Sprite[] GirlGuard => girlGuard;
        public Sprite[] GirlDamage => girlDamage;
        public Sprite EyesPowerCutin => eyesPowerCutin;

        public void ConfigureGirl(
            Sprite[] idle,
            Sprite[] run,
            Sprite[] jump,
            Sprite[] crouch,
            Sprite[] attack,
            Sprite[] guard,
            Sprite[] damage,
            Sprite eyesCutin)
        {
            girlIdle = idle;
            girlRun = run;
            girlJump = jump;
            girlCrouch = crouch;
            girlAttack = attack;
            girlGuard = guard;
            girlDamage = damage;
            eyesPowerCutin = eyesCutin;
        }

        public void ConfigureEnemies(ArenaEnemySpriteSet[] enemySets)
        {
            enemies = enemySets;
        }

        public bool TryGetEnemySet(ArenaEnemyId id, out ArenaEnemySpriteSet set)
        {
            if (enemies != null)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i].EnemyId == id)
                    {
                        set = enemies[i];
                        return true;
                    }
                }
            }

            set = default;
            return false;
        }
    }
}
