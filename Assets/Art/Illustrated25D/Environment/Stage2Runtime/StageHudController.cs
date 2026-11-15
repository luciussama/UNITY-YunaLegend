using System;
using UnityEngine;
using YunaLegend.Dialogue;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Controller especialista de UI/HUD para o fluxo jogável (caixas de diálogo,
    /// cartões poéticos, cue pré-pulo, botões superiores e telas de pausa/derrota/conclusão).
    /// </summary>
    public sealed class StageHudController : MonoBehaviour
    {
        private Texture2D solidTex;

        public Func<StageFlowStateMachine.FlowPhase> GetPhase;
        public Func<bool> IsPaused;
        public Func<bool> IsMuted;
        public Func<bool> IsPlayerDead;
        public Func<string> GetFailure;
        public Func<float> GetStage2BannerTimer;
        public Func<Vector3> GetPlayerWorldPosition;
        public Func<Vector3> GetBearTopWorldPosition;

        public event Action OnAdvanceRequested;
        public event Action OnTogglePauseRequested;
        public event Action OnToggleMuteRequested;
        public event Action OnRetryFromBeginningRequested;
        public event Action OnRetryFromCheckpointRequested;

        private void Awake()
        {
            solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            solidTex.SetPixel(0, 0, Color.white);
            solidTex.Apply();
        }

        private void OnDestroy()
        {
            if (solidTex != null)
            {
                Destroy(solidTex);
            }
        }

        private void OnGUI()
        {
            if (GetPhase == null || solidTex == null)
            {
                return;
            }

            DrawTopRightButtons();

            string failure = GetFailure != null ? GetFailure() : null;
            if (!string.IsNullOrEmpty(failure))
            {
                DrawCenteredCard("✦ ERRO NA CENA ✦", failure, null, null, null);
                return;
            }

            if (IsPaused != null && IsPaused())
            {
                DrawCenteredCard(
                    "✦ PAUSADO ✦",
                    "Jogo pausado",
                    "Pressione P ou clique em Continuar para retomar.",
                    "Continuar",
                    () => OnTogglePauseRequested?.Invoke(),
                    "Voltar ao início",
                    () => OnRetryFromBeginningRequested?.Invoke());
                return;
            }

            if (IsPlayerDead != null && IsPlayerDead())
            {
                DrawCenteredCard(
                    "✦ QUARTO ESCURO ✦",
                    "Você não conseguiu sair do quarto.",
                    "A penumbra engoliu o caminho... Mas a luz da fadinha ainda chama por você.",
                    "Tentar de novo",
                    () => OnRetryFromCheckpointRequested?.Invoke(),
                    "Voltar ao início",
                    () => OnRetryFromBeginningRequested?.Invoke());
                return;
            }

            var phase = GetPhase();
            switch (phase)
            {
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake1:
                    DrawLegacyBottomDialogue(
                        "NANDA",
                        "Levanta, dorminhoca.\nTemos que brincar.\nVocê dormiu demais.",
                        "Avança sozinho · Clique ou Enter adianta",
                        boldBody: false);
                    break;

                case StageFlowStateMachine.FlowPhase.OpeningPoeticIntro:
                    DrawLegacyPoeticCard(
                        "Cada coisa guardava seu nome.\nEntre elas, nada dizia por onde ir.",
                        "Avança sozinho · Clique ou Enter adianta");
                    break;

                case StageFlowStateMachine.FlowPhase.OpeningNandaWake2:
                    DrawLegacyBottomDialogue(
                        "NANDA",
                        "SENÃO A GENTE NÃO SAI DAQUI NUNQUINHA!",
                        "Avança sozinho · Clique ou Enter adianta",
                        boldBody: true);
                    break;

                case StageFlowStateMachine.FlowPhase.PreJumpCue:
                    DrawPreJumpCue();
                    break;

                case StageFlowStateMachine.FlowPhase.CastleCutsceneStep0:
                    DrawLegacyPoeticCard(
                        DarkRoomDialogueScript.PoeticCastle,
                        "Clique ou pressione Espaço / Enter para continuar");
                    DrawLegacyBottomDialogue(
                        "NANDA",
                        DarkRoomDialogueScript.CastleNanda.text,
                        "Clique ou pressione Espaço / Enter para continuar",
                        boldBody: false);
                    break;

                case StageFlowStateMachine.FlowPhase.CastleCutsceneStep1:
                    DrawLegacyBottomDialogue(
                        "YUNA",
                        DarkRoomDialogueScript.CastleYuna.text,
                        "Clique ou pressione Espaço / Enter para iniciar A Fuga",
                        boldBody: false);
                    break;

                case StageFlowStateMachine.FlowPhase.Stage2Gameplay:
                    if (GetStage2BannerTimer != null && GetStage2BannerTimer() > 0f)
                    {
                        DrawStageBanner("FASE 2 — A FUGA");
                    }
                    break;

                case StageFlowStateMachine.FlowPhase.Stage2Completed:
                    DrawCenteredCard(
                        "✦ A FUGA — PLATAFORMA 10 ✦",
                        "Você alcançou a Pista Elevada do Trem!",
                        "Fim do trecho com plataformas desenhadas em sequência (00 → 10). As plataformas 11–13 ainda estão em produção.",
                        "Jogar novamente",
                        () => OnRetryFromBeginningRequested?.Invoke());
                    break;
            }
        }

        private void DrawTopRightButtons()
        {
            const float size = 38f;
            const float gap = 10f;
            const float top = 16f;
            float right = Screen.width - 18f;

            bool muted = IsMuted != null && IsMuted();
            bool paused = IsPaused != null && IsPaused();

            if (DrawRoundButton(new Rect(right - size, top, size, size), muted ? "OFF" : "SOM", false))
                OnToggleMuteRequested?.Invoke();
            if (DrawRoundButton(new Rect(right - size * 2f - gap, top, size, size), paused ? ">" : "||", paused))
                OnTogglePauseRequested?.Invoke();
            if (DrawRoundButton(new Rect(right - size * 3f - gap * 2f, top, size, size), "R", false))
                OnRetryFromBeginningRequested?.Invoke();
        }

        private bool DrawRoundButton(Rect rect, string label, bool active)
        {
            Color prev = GUI.color;
            GUI.color = active ? new Color(0.95f, 0.76f, 0.25f, 0.95f) : new Color(0.08f, 0.08f, 0.14f, 0.85f);
            GUI.DrawTexture(rect, solidTex);
            GUI.color = new Color(0.85f, 0.76f, 0.52f, 0.75f);
            DrawBorder(rect, 1.5f);
            GUI.color = prev;

            var style = CreateLabelStyle(
                TextAnchor.MiddleCenter,
                label.Length > 2 ? 11 : 15,
                active ? Color.black : new Color(0.96f, 0.92f, 0.84f),
                FontStyle.Bold);
            return GUI.Button(rect, label, style);
        }

        private void DrawLegacyBottomDialogue(string speaker, string body, string footer, bool boldBody)
        {
            float w = Mathf.Min(680f, Screen.width - 48f);
            const float h = 158f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 28f;

            Color prev = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.10f, 0.90f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.28f, 0.30f, 0.42f, 0.55f);
            DrawBorder(new Rect(x, y, w, h), 1.5f);
            GUI.color = prev;

            GUI.Label(
                new Rect(x + 20f, y + 14f, w - 40f, 20f),
                speaker,
                CreateLabelStyle(TextAnchor.UpperCenter, 12, new Color(0.75f, 0.78f, 0.86f), FontStyle.Bold));

            GUI.Label(
                new Rect(x + 28f, y + 36f, w - 56f, 84f),
                body,
                CreateLabelStyle(TextAnchor.MiddleCenter, 21, new Color(0.98f, 0.97f, 0.94f), boldBody ? FontStyle.Bold : FontStyle.Normal, wrap: true));

            GUI.Label(
                new Rect(x + 20f, y + h - 28f, w - 40f, 20f),
                footer,
                CreateLabelStyle(TextAnchor.LowerCenter, 11, new Color(0.68f, 0.70f, 0.78f)));

            if (GUI.Button(new Rect(x, y, w, h), GUIContent.none, GUIStyle.none))
            {
                OnAdvanceRequested?.Invoke();
            }
        }

        private void DrawLegacyPoeticCard(string text, string footer)
        {
            float w = Mathf.Min(640f, Screen.width - 64f);
            const float h = 118f;
            float x = (Screen.width - w) * 0.5f;
            const float y = 26f;

            Color prev = GUI.color;
            GUI.color = new Color(0.98f, 0.96f, 0.91f, 0.98f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.88f, 0.72f, 0.75f, 0.95f);
            DrawBorder(new Rect(x, y, w, h), 2f);
            DrawBorder(new Rect(x + 4f, y + 4f, w - 8f, h - 8f), 1f);
            GUI.color = prev;

            GUI.Label(
                new Rect(x + 24f, y + 14f, w - 48f, 68f),
                text,
                CreateLabelStyle(TextAnchor.MiddleCenter, 19, new Color(0.22f, 0.16f, 0.18f), FontStyle.Italic, wrap: true));

            GUI.Label(
                new Rect(x + 20f, y + h - 26f, w - 40f, 18f),
                footer,
                CreateLabelStyle(TextAnchor.LowerCenter, 11, new Color(0.45f, 0.36f, 0.38f)));

            if (GUI.Button(new Rect(x, y, w, h), GUIContent.none, GUIStyle.none))
            {
                OnAdvanceRequested?.Invoke();
            }
        }

        private void DrawPreJumpCue()
        {
            if (Camera.main == null || GetPlayerWorldPosition == null || GetBearTopWorldPosition == null)
            {
                return;
            }

            Vector3 yunaScreen = Camera.main.WorldToScreenPoint(GetPlayerWorldPosition() + Vector3.up * 2.15f);
            if (yunaScreen.z > 0f)
            {
                float sx = Mathf.Clamp(yunaScreen.x - 125f, 20f, Screen.width - 270f);
                float sy = Mathf.Clamp(Screen.height - yunaScreen.y - 44f, 60f, Screen.height - 100f);

                Color prev = GUI.color;
                GUI.color = new Color(0.06f, 0.06f, 0.10f, 0.88f);
                GUI.DrawTexture(new Rect(sx, sy, 250f, 40f), solidTex);
                GUI.color = new Color(0.95f, 0.82f, 0.55f, 0.75f);
                DrawBorder(new Rect(sx, sy, 250f, 40f), 1.5f);
                GUI.color = prev;

                GUI.Label(
                    new Rect(sx, sy, 250f, 40f),
                    "[ESPAÇO]  PRESSIONE ESPAÇO",
                    CreateLabelStyle(TextAnchor.MiddleCenter, 14, new Color(0.98f, 0.94f, 0.82f), FontStyle.Bold));
            }

            Vector3 bearScreen = Camera.main.WorldToScreenPoint(GetBearTopWorldPosition() + Vector3.up * 0.9f);
            if (bearScreen.z > 0f)
            {
                float bx = bearScreen.x;
                float by = Screen.height - bearScreen.y - 26f + Mathf.Sin(Time.time * 4.5f) * 5f;
                Color prev = GUI.color;
                GUI.color = new Color(0.98f, 0.88f, 0.45f, 0.98f);
                GUI.DrawTexture(new Rect(bx - 2f, by - 14f, 4f, 18f), solidTex);
                GUI.DrawTexture(new Rect(bx - 9f, by + 2f, 18f, 4f), solidTex);
                GUI.DrawTexture(new Rect(bx - 5f, by + 6f, 10f, 4f), solidTex);
                GUI.DrawTexture(new Rect(bx - 2f, by + 10f, 4f, 4f), solidTex);
                GUI.color = prev;
            }
        }

        private void DrawStageBanner(string title)
        {
            const float w = 320f;
            const float h = 44f;
            float x = (Screen.width - w) * 0.5f;
            const float y = 24f;

            Color prev = GUI.color;
            GUI.color = new Color(0.06f, 0.05f, 0.12f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.94f, 0.78f, 0.34f, 0.9f);
            DrawBorder(new Rect(x, y, w, h), 1.5f);
            GUI.color = prev;

            GUI.Label(
                new Rect(x, y, w, h),
                title,
                CreateLabelStyle(TextAnchor.MiddleCenter, 16, new Color(0.98f, 0.92f, 0.70f), FontStyle.Bold));
        }

        private void DrawCenteredCard(
            string header,
            string title,
            string subtitle,
            string primaryLabel,
            Action primaryAction,
            string secondaryLabel = null,
            Action secondaryAction = null)
        {
            float w = Mathf.Min(540f, Screen.width - 48f);
            const float h = 228f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            Color prev = GUI.color;
            GUI.color = new Color(0.05f, 0.04f, 0.10f, 0.95f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidTex);
            GUI.color = new Color(0.76f, 0.48f, 0.24f, 0.85f);
            DrawBorder(new Rect(x, y, w, h), 1.5f);
            GUI.color = prev;

            GUI.Label(
                new Rect(x + 20f, y + 18f, w - 40f, 22f),
                header,
                CreateLabelStyle(TextAnchor.MiddleCenter, 12, new Color(0.88f, 0.58f, 0.58f), FontStyle.Bold));

            GUI.Label(
                new Rect(x + 24f, y + 44f, w - 48f, 62f),
                title,
                CreateLabelStyle(TextAnchor.MiddleCenter, 24, new Color(0.98f, 0.94f, 0.68f), FontStyle.Bold, wrap: true));

            if (!string.IsNullOrEmpty(subtitle))
            {
                GUI.Label(
                    new Rect(x + 28f, y + 108f, w - 56f, 44f),
                    subtitle,
                    CreateLabelStyle(TextAnchor.MiddleCenter, 14, new Color(0.86f, 0.84f, 0.90f), FontStyle.Italic, wrap: true));
            }

            if (!string.IsNullOrEmpty(primaryLabel) && primaryAction != null)
            {
                bool hasTwo = !string.IsNullOrEmpty(secondaryLabel) && secondaryAction != null;
                float bw = hasTwo ? 180f : 220f;
                const float bh = 42f;
                float by = y + h - 58f;
                float bx1 = hasTwo ? x + w * 0.5f - bw - 10f : x + (w - bw) * 0.5f;

                GUI.color = new Color(0.96f, 0.76f, 0.24f, 1f);
                GUI.DrawTexture(new Rect(bx1, by, bw, bh), solidTex);
                GUI.color = prev;
                if (GUI.Button(
                    new Rect(bx1, by, bw, bh),
                    primaryLabel,
                    CreateLabelStyle(TextAnchor.MiddleCenter, 15, new Color(0.10f, 0.08f, 0.05f), FontStyle.Bold)))
                {
                    primaryAction.Invoke();
                }

                if (hasTwo)
                {
                    float bx2 = x + w * 0.5f + 10f;
                    GUI.color = new Color(0.12f, 0.12f, 0.18f, 1f);
                    GUI.DrawTexture(new Rect(bx2, by, bw, bh), solidTex);
                    GUI.color = new Color(0.55f, 0.55f, 0.65f, 0.8f);
                    DrawBorder(new Rect(bx2, by, bw, bh), 1f);
                    GUI.color = prev;
                    if (GUI.Button(
                        new Rect(bx2, by, bw, bh),
                        secondaryLabel,
                        CreateLabelStyle(TextAnchor.MiddleCenter, 15, new Color(0.94f, 0.94f, 0.96f), FontStyle.Bold)))
                    {
                        secondaryAction.Invoke();
                    }
                }
            }
        }

        private static GUIStyle CreateLabelStyle(
            TextAnchor alignment,
            int fontSize,
            Color textColor,
            FontStyle fontStyle = FontStyle.Normal,
            bool wrap = false)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = fontStyle,
                wordWrap = wrap
            };
            style.normal.textColor = textColor;
            return style;
        }

        private void DrawBorder(Rect r, float t)
        {
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, t), solidTex);
            GUI.DrawTexture(new Rect(r.x, r.yMax - t, r.width, t), solidTex);
            GUI.DrawTexture(new Rect(r.x, r.y, t, r.height), solidTex);
            GUI.DrawTexture(new Rect(r.xMax - t, r.y, t, r.height), solidTex);
        }
    }
}
