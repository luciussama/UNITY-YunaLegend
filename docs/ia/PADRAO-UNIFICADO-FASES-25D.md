# Padrão Unificado Obrigatório das Fases — Yuna Legend (2,5D)

**Vigência:** 26/11/2026 (obrigatório para todas as fases novas e para qualquer refatoração/evolução das fases existentes).  
**Objetivo:** Eliminar quebras perceptíveis de jogabilidade, qualidade visual, animação, câmera, controles e interface entre uma fase e outra do jogo.

---

## 1. Princípio Fundamental: Experiência Contínua Entre Fases

O jogador deve sentir que **todas as fases pertencem ao mesmo jogo**, mesmo quando a temática ou o gênero secundário muda (plataforma no quarto escuro, exploração nos brinquedos, combate na arena, perseguição do urso, travessia no jardim).  
Qualquer técnica nova validada e aprovada em uma fase torna-se **padrão transversal do projeto** e não pode ficar isolada em um único estágio.

---

## 2. Animação de Personagens — Padrão Unity 2D Animation (Obrigatório)

Para personagens 2D ilustrados (Yuna, Nanda e futuros atores 2D), fica **proibido** entregar ciclos de corrida/movimento baseados em spritesheets que repitam a mesma perna na frente ou apresentem *flicker* de traço/estampa.

### 2.1 Estrutura Obrigatória do Rig 2D
- **Pacote oficial:** `com.unity.feature.2d` (`com.unity.2d.animation`, `com.unity.2d.sprite`, `com.unity.2d.psdimporter`).
- **Camadas modulares com contorno fechado e fundo RGBA transparente:**
  - `HeadHappy` e `HeadScared` (ou expressões equivalentes com dimensões idênticas para troca limpa);
  - `PigtailBack` e `PigtailFar` (elementos secundários separados para *follow-through*);
  - `TorsoDress` (sem braços nem pernas);
  - `ArmFront` / `ArmBack` e `LegFront` / `LegBack` (membros traseiros com sombreado quente mais escuro para leitura imediata de profundidade e alternância).
- **Hierarquia anatômica mínima:**
  `RigRoot (SortingGroup)` → `Bone_Hips` → `Bone_LegFront`, `Bone_LegBack`, `Bone_Torso` → `Bone_ArmFront`, `Bone_ArmBack`, `Bone_Head` → `Bone_PigtailBack`, `Bone_PigtailFar`.
- **Regras de movimento obrigatórias:**
  1. **Alternância real de passada (`180°`):** perna frontal e perna traseira devem operar em fases opostas durante corrida/caminhada.
  2. **Contra-balanço de braços:** braço frontal oscila em oposição à perna frontal.
  3. **Movimento secundário:** maria-chiquinhas/cabelo e inclinação do tronco respondem com atraso de fase ao passo, subida de pulo e queda.
  4. **Troca de expressão:** estados de desconforto sensorial, pânico ou dano ativam automaticamente a expressão facial correspondente.
  5. **Editabilidade na Unity:** cada rig deve gerar `.prefab`, `.controller` (`AnimatorController`) e clipes `.anim` em **60 FPS** via Editor builder (`*RigBuilder.cs`), permitindo ajuste fino direto nas janelas *Animation* e *2D Animation*.
- **Referência canônica:** `PicnicYunaRigState` (`Core`), `PicnicYunaRigView` (`Runtime`), `PicnicYunaRigBuilder` (`Editor`) e `Assets/Art/Picnic/YunaRig2D/`.

---

## 3. Câmera Unificada (Desktop + Mobile Retrato/Paisagem)

Toda fase jogável deve possuir um **perfil puro de câmera mobile em `Core`** (`*MobileCameraFraming.cs`), ativado exclusivamente por `Application.isMobilePlatform` ou override explícito de QA.

### Regras Obrigatórias
1. **Desktop intocado:** nenhuma regra mobile pode alterar a câmera desktop já aprovada.
2. **Planejamento de ação visível:**
   - Em **plataforma** (ex.: Jardim, Quarto Escuro, Brinquedos): a câmera mobile deve enquadrar Yuna e o **próximo apoio relevante à frente** tanto em paisagem quanto em retrato, sem cortes bruscos durante saltos (âncora vertical no último pouso + suavização exponencial).
   - Em **combate/perseguição** (ex.: Arena, BearChase): ambos os atores (Yuna e ameaça/inimigo) devem permanecer integralmente dentro dos limites horizontais do viewport nas poses de proximidade, média distância e afastamento.
3. **Independência de framerate:** toda interpolação de câmera deve usar suavização exponencial (`1 - exp(-k * dt)`) e ser validada em `30/60/120 Hz`.
4. **Proibição de acoplamento visual destrutivo:** eventos cênicos (como desabamento do castelo de cartas) nunca podem deslocar ou tremer a câmera a ponto de fazer o jogador perder a referência da personagem.

---

## 4. Controles, Física e Sensação de Jogo Consistentes

Para evitar quebra de *game feel* entre fases:
1. **Paridade Teclado + Touch Mobile:** todas as fases jogáveis devem responder a teclado (`A/D` ou setas + `Espaço/W/Seta Cima`) e controles touch mobile na tela.
2. **Multitouch obrigatório no mobile:** segurar `ESQ`/`DIR` e tocar em `PULAR` ou botão de ação simultaneamente **nunca** pode cancelar o movimento horizontal nem exigir soltar o direcional.
3. **Estabilidade física em `30/60/120 Hz`:** manter controlador 3D / simulação determinística em passo estável; proibida dependência de `Time.deltaTime` bruto em aceleração de pulo ou gravidade.
4. **Apoio real das plataformas:** preservar `PlatformSupportProfile` e superfícies reais de contato (conforme Regra 4 do `AGENTS.md`).

---

## 5. Diálogos, HUD e Proteção do Menu (`TitleScreen`)

1. **Diálogos unificados no mobile:** toda fase com falas/caixas de diálogo deve usar o adaptador compartilhado `MobileDialoguePresenter` (`YunaLegend.Presentation`):
   - posicionado acima da cabeça da falante (excluindo sistemas de partículas do cálculo de altura da cabeça);
   - fonte `14px` lógicos, `lineSpacing 1`, `padding 6px`, espaço `2px` e altura ajustada ao conteúdo;
   - textos originais e ações de avanço 100% preservados.
2. **Ergonomia de HUD mobile:** indicadores de vida/progresso e botão de pausa não podem sobrepor Yuna, o próximo apoio ou os botões de toque inferiores.
3. **Integridade do Menu:** `TitleScreen.unity`, `TitleScreenController.cs`, `TitlePhaseSelector.cs` e `EditorBuildSettings.asset` são arquivos protegidos em entregas de ajuste visual/mobile; qualquer nova fase cadastrada segue obrigatoriamente a Regra 8 do `AGENTS.md` (pré-selecionada em **Novo jogo** e no seletor de fases).

---

## 6. Arquitetura Obrigatória (`Core` + `Runtime` + `Editor` + QA Opt-In)

1. **Humble Object + `.asmdef` isolado:** toda lógica de simulação, câmera, pose de rig 2D e piloto QA deve residir em `*.Core.asmdef` com `noEngineReferences: true`.
2. **Adaptadores finos em `Runtime`:** `MonoBehaviour` apenas conecta entrada, física, áudio e renderização; proibido `OnGUI` de lógica de negócio ou bootstrap monolítico.
3. **Piloto QA Opt-In obrigatório:** toda fase deve expor um piloto determinístico puro em `Core` ativado apenas via query string (`?qa=1&qa_autoplay=1`), sem impacto algum no jogador comum.
4. **Testes obrigatórios antes de qualquer release:**
   - **EditMode (NUnit):** 100% de cobertura das regras puras (física/contato, câmera mobile, estados do rig 2D, piloto QA).
   - **PlayMode / Capturas Editor:** verificação em `30/60/120 Hz` quando houver física/colapso, capturas em `390×844` e `844×390` para câmera/diálogo mobile, e snapshot `TITLE_DESKTOP_NO_MUTATION_OK`.
   - **Integridade de fontes:** registro de hashes SHA256 em `Evidence/<Entrega>/sources.json` e `WebGL/release.json` para toda build WebGL.

---

## 7. Matriz de Adequação das Fases Existentes

Use esta matriz para planejar e auditar o alinhamento progressivo de todas as fases ao padrão unificado:

| Fase | Cena | Rig 2D Esqueletal | Câmera Mobile (`Core`) | Multitouch Mobile | `MobileDialoguePresenter` | Piloto QA (`Core`) |
|---|---|---|---|---|---|---|
| **Fase 1 — Quarto Escuro** | `Fase-Jogavel-Completa` | Pendente adequação | Validado (cartas + 30/60/120Hz) | Revisar paridade | Validado | `DarkRoomQaPilot` |
| **Fase 2 — Quarto dos Brinquedos** | `ToyRoom_Gameplay` | Avaliar paridade 2D/3D | Pendente perfil `Core` dedicado | Revisar paridade | Validado | `ToyRoomQaPilot` |
| **Fase 3 — Arena** | `Arena_Gameplay` | N/A (atores 3D) | Validado (`ArenaMobileCameraFraming`, 37/37) | Revisar paridade | Validado | `ArenaQaPilot` |
| **Fase 4 — Perseguição do Urso** | `BearChase_Gameplay` | Avaliar paridade | Pendente perfil `Core` dedicado | Revisar paridade | Validado | `BearChaseQaPilot` |
| **Fase 5 — Jardim (Picnic)** | `Picnic_Gameplay` | **Referência canônica** (`PicnicYunaRigState`/`View`) | Validado (`PicnicMobileCameraFraming`, 30/30) | Validado | Validado | Pendente piloto QA dedicado |

### Regra de Bloqueio (Definition of Done)
Nenhuma nova fase pode ser dada como concluída — e nenhuma fase existente pode receber selo de "alinhada ao padrão unificado" — sem:
1. Testes `EditMode` passando (`0` falhas) para simulação, câmera mobile e rig/animação;
2. Folha de revisão visual arquivada em `Evidence/` (ciclo de corrida/pulo/expressões + vistas `390×844` e `844×390`);
3. Declaração honesta do que foi validado no Editor/WebGL versus o que ainda depende de teste em aparelho físico.
