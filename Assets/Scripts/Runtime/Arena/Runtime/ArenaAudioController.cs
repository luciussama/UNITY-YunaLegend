using UnityEngine;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Audio da Arena Imaginaria: mantem Circular Dissonance (1).mp3 em loop e dispara
    /// os efeitos sonoros canonicos de espada, defesa, especial e golpes dos 4 brinquedos.
    /// </summary>
    public sealed class ArenaAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Musica")]
        [SerializeField] private AudioClip battleMusic;

        [Header("Menina")]
        [SerializeField] private AudioClip girlAttackClip;
        [SerializeField] private AudioClip girlDefenceClip;
        [SerializeField] private AudioClip girlSpecialClip;

        [Header("Inimigos")]
        [SerializeField] private AudioClip trainHornClip;
        [SerializeField] private AudioClip trainSteamClip;
        [SerializeField] private AudioClip robotNoisesClip;
        [SerializeField] private AudioClip robotLaserClip;
        [SerializeField] private AudioClip robotPunchClip;
        [SerializeField] private AudioClip dinosaurAttackClip;
        [SerializeField] private AudioClip bearAttackClip;

        public bool IsMuted { get; private set; }

        public void Configure(
            AudioClip music,
            AudioClip girlAttack,
            AudioClip girlDefence,
            AudioClip girlSpecial,
            AudioClip trainHorn,
            AudioClip trainSteam,
            AudioClip robotNoises,
            AudioClip robotLaser,
            AudioClip robotPunch,
            AudioClip dinosaurAttack,
            AudioClip bearAttack)
        {
            battleMusic = music;
            girlAttackClip = girlAttack;
            girlDefenceClip = girlDefence;
            girlSpecialClip = girlSpecial;
            trainHornClip = trainHorn;
            trainSteamClip = trainSteam;
            robotNoisesClip = robotNoises;
            robotLaserClip = robotLaser;
            robotPunchClip = robotPunch;
            dinosaurAttackClip = dinosaurAttack;
            bearAttackClip = bearAttack;
        }

        private void Start()
        {
            EnsureSources();
            if (battleMusic != null && !musicSource.isPlaying)
            {
                musicSource.clip = battleMusic;
                musicSource.loop = true;
                musicSource.volume = 0.48f;
                musicSource.Play();
            }
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            EnsureSources();
            musicSource.mute = IsMuted;
            sfxSource.mute = IsMuted;
        }

        public void SetPaused(bool paused)
        {
            if (musicSource == null)
            {
                return;
            }

            if (paused) musicSource.Pause();
            else musicSource.UnPause();
        }

        public void PlaySwordSwing() => PlayOneShot(girlAttackClip, 0.78f);
        public void PlayBlock() => PlayOneShot(girlDefenceClip, 0.85f);
        public void PlaySpecialLaunch() => PlayOneShot(girlSpecialClip, 0.92f);

        public void PlayEnemyAttack(ArenaEnemyId enemyId, ArenaAttackKind kind)
        {
            AudioClip clip = enemyId switch
            {
                ArenaEnemyId.Train => kind == ArenaAttackKind.SteamBlast ? trainSteamClip : trainHornClip,
                ArenaEnemyId.Robot => kind == ArenaAttackKind.LaserBeam ? robotLaserClip : robotPunchClip,
                ArenaEnemyId.Dinosaur => dinosaurAttackClip,
                ArenaEnemyId.Bear => bearAttackClip,
                _ => null
            };

            PlayOneShot(clip, 0.82f);
        }

        public void PlayRobotStep() => PlayOneShot(robotNoisesClip, 0.35f);

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null || IsMuted)
            {
                return;
            }

            EnsureSources();
            sfxSource.PlayOneShot(clip, volume);
        }

        private void EnsureSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }
    }
}
