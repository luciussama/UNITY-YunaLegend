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
        [SerializeField] private AudioSource enemyAttackSource;

        private AudioClip synthRobotAimClip;
        private AudioClip synthRobotLaserFireClip;
        private AudioClip synthRobotPunchWindupClip;
        private AudioClip synthRobotStepClip;

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
            enemyAttackSource.mute = IsMuted;
        }

        public void SetPaused(bool paused)
        {
            if (musicSource == null)
            {
                return;
            }

            if (paused)
            {
                musicSource.Pause();
                if (enemyAttackSource != null) enemyAttackSource.Pause();
            }
            else
            {
                musicSource.UnPause();
                if (enemyAttackSource != null) enemyAttackSource.UnPause();
            }
        }

        public void PlaySwordSwing() => PlayOneShot(girlAttackClip, 0.78f);
        public void PlayBlock() => PlayOneShot(girlDefenceClip, 0.85f);
        public void PlaySpecialLaunch() => PlayOneShot(girlSpecialClip, 0.92f);

        public void StopEnemyAttackAudio()
        {
            if (enemyAttackSource != null && enemyAttackSource.isPlaying)
            {
                enemyAttackSource.Stop();
            }
        }

        public void PlayEnemyAttack(ArenaEnemyId enemyId, ArenaAttackKind kind)
        {
            if (enemyId == ArenaEnemyId.Robot)
            {
                EnsureRobotCombatClips();
                AudioClip windupClip = kind == ArenaAttackKind.LaserBeam
                    ? synthRobotAimClip
                    : synthRobotPunchWindupClip;
                PlayEnemyChannel(windupClip, kind == ArenaAttackKind.LaserBeam ? 0.62f : 0.48f);
                return;
            }

            AudioClip clip = enemyId switch
            {
                ArenaEnemyId.Train => kind == ArenaAttackKind.SteamBlast ? trainSteamClip : trainHornClip,
                ArenaEnemyId.Dinosaur => dinosaurAttackClip,
                ArenaEnemyId.Bear => bearAttackClip,
                _ => null
            };

            PlayOneShot(clip, 0.82f);
        }

        public void PlayEnemyAttackActive(ArenaEnemyId enemyId, ArenaAttackKind kind)
        {
            if (enemyId != ArenaEnemyId.Robot)
            {
                return;
            }

            EnsureRobotCombatClips();
            if (kind == ArenaAttackKind.LaserBeam)
            {
                PlayEnemyChannel(synthRobotLaserFireClip, 0.85f);
            }
            else if (kind == ArenaAttackKind.RocketPunch)
            {
                PlayEnemyChannel(robotPunchClip != null ? robotPunchClip : synthRobotPunchWindupClip, 0.82f);
            }
        }

        public void PlayRobotStep()
        {
            EnsureRobotCombatClips();
            PlayOneShot(synthRobotStepClip, 0.25f);
        }

        private void PlayEnemyChannel(AudioClip clip, float volume)
        {
            if (clip == null || IsMuted)
            {
                return;
            }

            EnsureSources();
            enemyAttackSource.Stop();
            enemyAttackSource.clip = clip;
            enemyAttackSource.volume = volume;
            enemyAttackSource.Play();
        }

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

            if (enemyAttackSource == null)
            {
                enemyAttackSource = gameObject.AddComponent<AudioSource>();
                enemyAttackSource.playOnAwake = false;
            }
        }

        private void EnsureRobotCombatClips()
        {
            if (synthRobotAimClip != null)
            {
                return;
            }

            const int sr = 22050;

            // 1) Mira laser vermelha (0.88s): zumbido fino ascendente + bipes de trava de alvo
            int aimSamples = (int)(sr * 0.88f);
            float[] aimData = new float[aimSamples];
            for (int i = 0; i < aimSamples; i++)
            {
                float t = i / (float)sr;
                float u = i / (float)aimSamples;
                float freq = Mathf.Lerp(620f, 1480f, u * u);
                float hum = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.22f;
                // Pulsos curtos de trava de mira acelerando no final
                float beepRate = Mathf.Lerp(5f, 16f, u);
                float beepEnv = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * beepRate * t)), 6f);
                float beep = Mathf.Sin(2f * Mathf.PI * 1950f * t) * beepEnv * 0.30f;
                float env = Mathf.Clamp01(u * 12f) * Mathf.Clamp01((1f - u) * 14f);
                aimData[i] = (hum + beep) * env;
            }
            synthRobotAimClip = AudioClip.Create("Robot_LaserAim_Synth", aimSamples, 1, sr, false);
            synthRobotAimClip.SetData(aimData, 0);

            // 2) Disparo forte do laser (0.44s): ataque de plasma potente com grave + descarga energetica
            int fireSamples = (int)(sr * 0.44f);
            float[] fireData = new float[fireSamples];
            uint rng = 2463534242u;
            for (int i = 0; i < fireSamples; i++)
            {
                float t = i / (float)sr;
                float u = i / (float)fireSamples;
                float sweepFreq = Mathf.Lerp(1180f, 145f, Mathf.Pow(u, 0.55f));
                float subFreq = Mathf.Lerp(160f, 68f, u);
                float laserTone = Mathf.Sin(2f * Mathf.PI * sweepFreq * t) * 0.45f
                    + Mathf.Sin(2f * Mathf.PI * sweepFreq * 1.5f * t) * 0.18f;
                float punchSub = Mathf.Sin(2f * Mathf.PI * subFreq * t) * Mathf.Exp(-u * 5.5f) * 0.50f;
                rng ^= rng << 13; rng ^= rng >> 17; rng ^= rng << 5;
                float noise = ((rng & 0xFFFF) / 32768f - 1f) * Mathf.Exp(-u * 7f) * 0.28f;
                float env = Mathf.Clamp01(u * 60f) * Mathf.Pow(1f - u, 1.35f);
                fireData[i] = Mathf.Clamp((laserTone + punchSub + noise) * env, -0.95f, 0.95f);
            }
            synthRobotLaserFireClip = AudioClip.Create("Robot_LaserFire_Synth", fireSamples, 1, sr, false);
            synthRobotLaserFireClip.SetData(fireData, 0);

            // 3) Preparacao do RocketPunch (0.42s): servo mecanico + pressao de mola
            int windSamples = (int)(sr * 0.42f);
            float[] windData = new float[windSamples];
            for (int i = 0; i < windSamples; i++)
            {
                float t = i / (float)sr;
                float u = i / (float)windSamples;
                float freq = Mathf.Lerp(210f, 460f, u);
                float servo = (Mathf.Sin(2f * Mathf.PI * freq * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * freq * 2.02f * t)) * 0.30f;
                float env = Mathf.Clamp01(u * 10f) * Mathf.Clamp01((1f - u) * 8f);
                windData[i] = servo * env;
            }
            synthRobotPunchWindupClip = AudioClip.Create("Robot_PunchWindup_Synth", windSamples, 1, sr, false);
            synthRobotPunchWindupClip.SetData(windData, 0);

            // 4) Passo metalico curto (0.09s)
            int stepSamples = (int)(sr * 0.09f);
            float[] stepData = new float[stepSamples];
            for (int i = 0; i < stepSamples; i++)
            {
                float t = i / (float)sr;
                float u = i / (float)stepSamples;
                float clank = Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.45f + Mathf.Sin(2f * Mathf.PI * 520f * t) * 0.20f;
                stepData[i] = clank * Mathf.Exp(-u * 7f);
            }
            synthRobotStepClip = AudioClip.Create("Robot_Step_Synth", stepSamples, 1, sr, false);
            synthRobotStepClip.SetData(stepData, 0);
        }
    }
}
