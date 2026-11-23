# Handover — Cena de Finalização do Capítulo 1 (`Um Quarto Inteiro`)

- **Assunto / Job ID:** `151` — `bearchase/cena-finalizacao-capitulo-v1`
- **Estado da reserva:** **EM ABERTO (`em_execucao`)**, preparada para handover explícito
- **Sessão atual (dona da reserva):** `20261123-antigravity-chapter1-ending-v1` (`Antigravity`)
- **Branch Git:** `bear-chase-refinement`
- **Commit de código base:** `e287ee0` (`feat: add QA pilot modules and tests for all game stages and update AGENTS guidelines`)
- **Entradas relacionadas no diário (`docs/ia/diario.sqlite3`):** `#1543` (`planned`), `#1544` (`blocked` por indisponibilidade/cota de geração de imagem)
- **Motivo do handover:** Indisponibilidade/falta de cota de geração de imagens (`image-generator` / `gemini-3.1-flash-image` retornando 503/sem cota) nesta IA. O usuário solicitou transferir a tarefa para outra IA com cota disponível.

---

## 1. Como assumir a tarefa (Comando obrigatório de Handover)

Antes de editar qualquer arquivo, a próxima IA deve assumir oficialmente a reserva `151` usando uma sessão única própria:

```bash
python3 tools/agent_registry.py handover \
  --id 151 \
  --session <NOVA-SESSAO-UNICA> \
  --agent "<NOME-DO-MODELO>" \
  --from-session 20261123-antigravity-chapter1-ending-v1 \
  --reason "Decisao explicita do usuario: assumir geracao das 3 artes da cena final ('Um Quarto Inteiro') e concluir validacao EditMode/WebGL por falta de cota de imagegen na IA anterior."
```

Recursos já reservados no Job `151`:
- `assets/art/bearchase/resources/bearchaseending`
- `assets/scripts/runtime/bearchase/core`
- `assets/scripts/runtime/bearchase/runtime`
- `assets/scripts/tests/editmode/bearchase`
- `build/bearchase-v2-webgl-qa`

---

## 2. Tudo o que já foi feito passo a passo

1. **Inspeção da referência legada (`https://luciussama.github.io/legendGirl/`):**
   - Mapeadas as 5 batidas exatas da cena 2D ilustrada de encerramento do Capítulo 1 (`Um Quarto Inteiro`):
     1. **Batida 0 (`0.00s`–`4.60s`):** Cabeçalho `NANDA` — `"Ufa, conseguimos. Guardamos os brinquedos a tempo. Agora podemos brincar."` | Yuna ofegante (`CatchingBreath`), Nanda feliz (`Happy`), fade-in inicial (`0.45s`).
     2. **Batida 1 (`4.60s`–`9.00s`):** Cabeçalho `UM QUARTO INTEIRO` — `"A menina sorriu.\nDe canto a canto, passeou os olhos pelo quarto."` | Yuna sorrindo (`Smiling`), Nanda feliz (`Happy`).
     3. **Batida 2 (`9.00s`–`14.20s`):** Cabeçalho `UM QUARTO INTEIRO` — `"Onde antes havia brinquedos, agora havia espaço.\nO sorriso ficou pequeno. Uma pergunta cresceu no lugar."` | Yuna transita de `Smiling` (até 45% da batida) para `Confused`, Nanda feliz (`Happy`).
     4. **Batida 3 (`14.20s`–`19.60s`):** Cabeçalho `MENINA` — `"Mas... agora todos os brinquedos estão guardados.\nComo vamos brincar? Vamos pegar os brinquedos de novo?"` | Yuna confusa (`Confused`), Nanda feliz (`Happy`).
     5. **Batida 4 (`19.60s`–`24.00s`):** Punchline muda (sem caixa de diálogo) | Yuna confusa (`Confused`), **Nanda azul fazendo facepalm (`Facepalm`)**, zoom-in suave de `1.0x` até `1.88x` focado na reação.
     6. **Fade-out (`24.00s`–`25.00s`) e Conclusão (`>= 25.00s`):** Transição suave para o modal limpo de conclusão do Capítulo 1.

2. **Implementação da timeline pura em `Core` (sem dependências de engine):**
   - Criado `Assets/Scripts/Runtime/BearChase/Core/BearChaseEndingTimeline.cs` (e `.meta`), com enums `BearChaseEndingBeat`, `BearChaseEndingYunaPose`, `BearChaseEndingNandaPose`, struct `BearChaseEndingSample`, método `Evaluate(float elapsedSeconds)` e `GetNextAdvanceTime(float elapsedSeconds)` (para adiantar batida por clique/toque/Enter/Espaço).

3. **Implementação do adaptador uGUI em `Runtime`:**
   - Criado `Assets/Scripts/Runtime/BearChase/Runtime/BearChaseEndingController.cs` (e `.meta`):
     - Cria `BearChase_EndingCanvas` (`ScreenSpaceOverlay`, `sortingOrder = 40`, `1920x1080`).
     - Configura container de zoom (`EndingZoomContainer` com pivô `(0.555, 0.375)`), fundo do quarto, sombra da Yuna, sprite de Yuna, aura azul-ciano de Nanda (`Color(0.22, 0.85, 1.0, 0.42)`), sprite de Nanda azul, cartão de diálogo creme/lilás e overlay de fade.
     - Carrega automaticamente as 3 texturas de `Assets/Art/BearChase/Resources/BearChaseEnding/` via `Resources.Load<Texture2D>` (com fallback temporário para sprites da entrada caso os arquivos ainda não existam).
     - Emite eventos de telemetria QA (`[QA_EVENT] bearchase_ending_started`, `bearchase_ending_beat`, `bearchase_ending_completed`) via `WebQaRuntime.LogEvent`.

4. **Integração na fase e correção do modal final:**
   - Atualizado `Assets/Scripts/Runtime/BearChase/Runtime/BearChaseStageBootstrap.cs`: ao vencer a perseguição (`HandleVictory`), dispara `BeginChapterEndingCinematic()` e, no evento `OnEndingCompleted`, chama `hudController.ShowChapterCompletedModal()`.
   - Atualizado `Assets/Scripts/Runtime/BearChase/Runtime/BearChaseHudController.cs`:
     - Na fase `BearChasePhase.Victory`, oculta o modal imediato para deixar a cena `"Um Quarto Inteiro"` rodar primeiro.
     - Adicionado `ShowChapterCompletedModal()` com título `"✦ CAPÍTULO 1 CONCLUÍDO! ✦"`, botão único `"Voltar ao início"` e ocultação do `titleButton` para **eliminar o botão duplicado**.

5. **Testes unitários `EditMode` escritos:**
   - Criado `Assets/Scripts/Tests/EditMode/BearChase/BearChaseEndingTimelineTests.cs` (e `.meta`) cobrindo progressão das 5 batidas, textos literais, troca de poses, zoom crescente na batida 4, conclusão e avanço monotônico por `GetNextAdvanceTime`.

6. **Ponto exato de interrupção:**
   - Todo o código C# acima já está commitado em `e287ee0`.
   - A geração das **3 imagens finais** em `Assets/Art/BearChase/Resources/BearChaseEnding/` falhou por falta de cota / HTTP 503 no gerador de imagens. **Nenhuma das 3 imagens foi criada ainda** (a pasta `Assets/Art/BearChase/Resources/BearChaseEnding/` ainda não existe).

---

## 3. O que está pendente (Passo a passo para a próxima IA)

### Passo 1 — Gerar e importar as 3 artes em `Assets/Art/BearChase/Resources/BearChaseEnding/`

Os nomes e formatos abaixo devem bater **exatamente** com o que `BearChaseEndingController.LoadAssets()` espera:

1. **`Assets/Art/BearChase/Resources/BearChaseEnding/ending-room-bg-v1.png`**
   - **Conteúdo:** Fundo 2D ilustrado (16:9, ex.: `1920x1080` ou `1536x864`) do quarto da menina completamente **arrumado e limpo**, com os brinquedos guardados nas prateleiras/baú e amplo espaço vazio no centro do chão onde antes havia bagunça.
   - **Estilo:** Ilustração 2,5D coerente com `ToyRoom` / `BearChaseEntrance`, paleta aconchegante e iluminação suave, sem personagens desenhados no fundo.

2. **`Assets/Art/BearChase/Resources/BearChaseEnding/ending-yuna-poses-v1.png`**
   - **Layout obrigatório:** Spritesheet horizontal **3 colunas × 1 linha** (`cellW = width / 3f`, `cellH = height`), fundo **100% transparente (RGBA)**, sem bordas/linhas divisórias, pés alinhados na mesma linha de base (`pivot = (0.5, 0)`).
   - **Identidade obrigatória:** Yuna canônica (vestido creme/floral, cabelo e sapatos canônicos — não recolorir Yuna). Usar `Assets/Art/BearChase/Resources/BearChaseEntrance/yuna-approach-walk-atlas-v2.png` como referência de traço e proporção.
   - **Ordem das 3 colunas (da esquerda para a direita):**
     - **Coluna 0 (`CatchingBreath`):** Yuna ofegante/aliviada recuperando o fôlego.
     - **Coluna 1 (`Smiling`):** Yuna sorrindo satisfeita olhando ao redor do quarto.
     - **Coluna 2 (`Confused`):** Yuna confusa/intrigada (sorriso sumiu, expressão de dúvida inocente).

3. **`Assets/Art/BearChase/Resources/BearChaseEnding/ending-nanda-blue-poses-v1.png`**
   - **Layout obrigatório:** Spritesheet horizontal **2 colunas × 1 linha** (`cellW = width / 2f`, `cellH = height`), fundo **100% transparente (RGBA)**, sem linhas divisórias, centralizada em cada célula (`pivot = (0.5, 0.5)`).
   - **Identidade obrigatória:** **Nanda azul-ciano** (aparência e luz emanada azuladas — **proibido** usar paleta âmbar/dourada). Usar `Assets/Art/BearChase/Resources/BearChaseEntrance/illustrated-fairy-v1.png` como referência de identidade/traço.
   - **Ordem das 2 colunas (da esquerda para a direita):**
     - **Coluna 0 (`Happy`):** Nanda azul feliz/aliviada flutuando.
     - **Coluna 1 (`Facepalm`):** Nanda azul fazendo *facepalm* (mão no rosto/testa em reação cômica à pergunta de Yuna).

> **Dica de pós-processamento:** Usar script Python (`Pillow`) para garantir remoção limpa de fundo (sem ilhas brancas internas), alinhamento consistente dos pés/centros entre células e dimensões divisíveis por 3 (Yuna) e por 2 (Nanda).

### Passo 2 — Revisar importação Unity e ajuste visual fino

- Abrir/rodar Unity em `-batchmode` para gerar os `.meta` dos 3 PNGs (verificar que `textureType` e `alphaIsTransparency` estejam corretos; `BearChaseEndingController` carrega como `Texture2D` e cria `Sprite.Create` em runtime).
- Conferir se `BearChaseEndingController.cs` precisa de ajuste fino de âncoras, escala (`yunaRect`, `nandaRect`, `nandaGlowRect`) ou `zoomContainer` pivô `(0.555, 0.375)` após ver as artes reais.
- Verificar também se `endingController?.StopEnding()` deve ser chamado em eventual reinício/retry de cena se o jogador voltar ao início e jogar novamente.

### Passo 3 — Rodar testes `EditMode`

- Executar os testes `EditMode` de `YunaLegend.BearChase.Tests` (incluindo `BearChaseEndingTimelineTests`) via Unity CLI (`-runTests -testPlatform EditMode`) e confirmar **100% Passed**.

### Passo 4 — Compilar WebGL QA e validar no navegador (Modo QA opt-in)

- Compilar `Build/BearChase-V2-WebGL-QA` (via `WebCiBuild` ou script de build WebGL QA do projeto).
- Validar no Chrome usando **obrigatoriamente** o Modo QA opt-in (`?qa=1&qa_autoplay=1`), conforme Regra 9 do `AGENTS.md`.
- Conferir visualmente e via console (`[QA_EVENT]`):
  - `bearchase_victory` dispara `bearchase_ending_started` sem exibir o modal antigo antes da cena.
  - Todas as 5 batidas rodam com os textos literais, troca das 3 poses de Yuna e das 2 poses de Nanda azul.
  - Batida 4 oculta a caixa de diálogo e faz zoom-in suave na Nanda azul em `Facepalm`.
  - Fade-out conclui e exibe o modal `✦ CAPÍTULO 1 CONCLUÍDO! ✦` limpo com apenas um botão `"Voltar ao início"` (sem botão duplicado).
  - Zero erros no console do navegador.

### Passo 5 — Fechamento no Diário, Registro e Git

- Registrar conclusão no diário (`python3 tools/ai_diary.py add --phase completed --plan-id <ID> ...`).
- Encerrar a reserva `151` (`python3 tools/agent_registry.py finish --id 151 --session <SUA-SESSAO> --outcome concluido --details "..."`).
- Commitar e subir as alterações na branch `bear-chase-refinement`.
