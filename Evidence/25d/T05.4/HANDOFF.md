# Handoff T05.4 — quarto em camadas (para IA com imagegen)

Sessão que para aqui: `2026-11-13-copilot-t053-b29a5ec7`. Data: 13/11/2026.

**Nada está aprovado.** O usuário não aceitou a T05.4. Esta tarefa foi **liberada** no coordenador para outra IA continuar, porque os ajustes pendentes dependem de geração de imagem.

## Estado real

| Item | Situação |
| --- | --- |
| T05.1 urso | aprovada (13/11/2026) |
| T05.2 livros | aprovada |
| T05.3 móvel estático | aprovada (`reviews/T05.3-revisao.md`) |
| **T05.4 quarto** | **em aberto** — duas versões de arte, nenhuma aprovada |
| T05 agregada | bloqueada pela T05.4 |
| T06.1 → T06.3 | bloqueados por T05 |

## O que já está implementado e funciona

- `Assets/Art/Illustrated25D/Environment/Room/RoomBackgroundT054.cs`
  - `Install(...)` para uma arte única (v1).
  - `InstallPanels(Texture2D[] panels, float panelPixelsPerUnit, ...)` para N painéis contínuos (v2).
  - Parallax por fração de avanço da câmera; âncora de piso por fração `FloorInsetFraction = 0,09375`.
  - Compila sem erros (verificado com `-batchmode -nographics -quit`).
- `Assets/Art/Illustrated25D/Environment/Room/ContactShadowT054.cs`
  - Sombra de contato procedural (elipse gerada em memória, sem asset), duas manchas por plataforma, ordens −160/−150.
- `Assets/Art/Illustrated25D/Environment/Room/Editor/RoomT054Capture.cs`
  - Harness batch: abre `DarkRoom_Gameplay` sem salvar, instala o móvel T05.3 em memória, monta o quarto, **oculta a geometria 3D legada do quarto** (272 renderers — ela ficava à frente da arte e escondia o piso), captura quatro enquadramentos a 1280×720 / 96 px/m e grava `room-audit.json`.
  - Sai sozinho com `EditorApplication.Exit(0/1)`; **não usar `-quit`**.
- Texturas v1 importadas em `Assets/Art/Illustrated25D/Environment/Room/layer{0,1,2}-*.png` com PPU 72.

## Versões de arte

**v1 (em uso hoje)** — `source/layer0-far.png`, `layer1-mid.png`, `layer2-near.png` (1536×768). Medidas e hashes em `manifest.json`.
Montada a `roomScale = 0,70`: rodapé 2,77 m, janela 4,64 m. O usuário achou **ainda grande** e pediu arte mais larga.

**v2 (especificada, não gerada)** — `PROMPTS-CAMADAS-V2.md`: três painéis `room-left/center/right.png`, 1920×1080 a 144 px/m, piso em `y = 936`, rodapé 1,10 m (158 px), janela 2,40×1,40 m (346×202 px), continuidade de padrão nas emendas.

## Pendências concretas para a próxima IA

1. Gerar os três painéis conforme `PROMPTS-CAMADAS-V2.md` (o central primeiro, depois usar como referência para os laterais).
2. Verificar antes de importar: 1920×1080; piso `y = 936 ± 6`; topo do rodapé `y = 778 ± 8`; janela 346×202; emendas sem salto; sem personagens/plataformas. Registrar SHA-256.
3. Importar com PPU igual a **144** e criar `.meta` explícito (atenção: `nPOTScale: 1` faz o importador arredondar para 2048×2048 e muda a escala aparente; para arte 1920×1080 usar `nPOTScale: 0`).
4. Trocar o harness para `room.InstallPanels(new[]{left,center,right}, 144f, midTex, nearTex, floorY, centreX, material, 1f, nearScale)` e devolver o parallax do fundo para **0,35** (cabe: precisa de 13,33 + 15,29×0,65 = 23,3 m e haverá 40 m).
5. Recapturar e submeter à revisão humana. **Não** declarar aprovação.

## Feedback do usuário que motivou a v2

- “Estranhei o tamanho dos objetos em relação à plataforma de subida. Parece que os livros são pequenos demais para ser plataforma.”
- “Faltou um acabamento de iluminação. Parece que os objetos estão flutuando, sem contato com o solo.”
- “Os objetos estão sem sombra.”

Sombras de contato já foram implementadas; a escala ainda não foi resolvida à contento.

## Limites técnicos conhecidos

- A câmera mostra 13,333 × 7,5 m a 96 px/m; qualquer interior doméstico realista (≈2,7 m de pé-direito, 4 m de largura) **não preenche** esse campo. Daí a solução por painéis contínuos.
- Sprites são unlit: o luar pintado não ilumina as plataformas 3D. Não adicionar luz dinâmica sem antes remover/ajustar o cone pintado, para não duplicar.
- `mid` e `near` da v1 continuam servindo; reescalar quando a v2 entrar.
- Pausa local de 80 ms, estralo, tropeço e retry do móvel **não** foram validados em runtime — pertencem a T06.2/3.

## Como reproduzir a captura atual

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\sergi\repos\UNITY-YunaLegend' -executeMethod YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run -t054Output 'Evidence/25d/T05.4/capture' -logFile 'Evidence/25d/T05.4/capture.log'
```

Sentinela no log: `T054_ROOM_CAPTURE_OK` ou `T054_ROOM_CAPTURE_FAIL`.

## Evidências desta sessão

- `capture/01-quarto-inicio.png` … `04-quarto-movel.png`, `review-contact-sheet.png`, `room-audit.json`.
- `CAPTURA.md` — método, medições, limites.
- `manifest.json` — hashes da v1.
- Diário: entradas 805–809 (esta sessão), sem qualquer `accept`.
