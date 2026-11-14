# Análise Crítica — Desaparecimento do Fundo do Quarto após o Tambor (05)

14/11/2026. Sessão `2026-11-14-antigravity-08-09-visual`.

## 1. Diagnóstico técnico (por que o fundo some após o tambor)

A inspeção de [IllustratedPreview.cs](../../../Assets/Art/Illustrated25D/Preview/IllustratedPreview.cs), [RoomBackgroundT054.cs](../../../Assets/Art/Illustrated25D/Environment/Room/RoomBackgroundT054.cs), [PenumbraLightingT054.cs](../../../Assets/Art/Illustrated25D/Environment/Room/PenumbraLightingT054.cs) e [PenumbraT054.shader](../../../Assets/Art/Illustrated25D/Environment/Room/PenumbraT054.shader) revela **três causas acopladas**:

1. **Fim físico do painel único (`RoomBackgroundT054`):**
   - `IllustratedPreview.Install()` instala **apenas 1 painel** (`new[]{panorama}`, `room-panorama-v4.png`, `2172×724 px`) com `panelPixelsPerUnit = panorama.width / 32f`, fixando a largura total do quarto em **32,0 m** (`±16,0 m`).
   - O centro do painel é ancorado na média das **3 primeiras plataformas do piloto** (`pilot.Take(3)`: urso, livros, penteadeira, centro em ~10–11 m), com `travelFraction = 0.35f`.
   - Assim, a borda direita da parede termina em ~26–27 m no início e, mesmo com o deslocamento de paralaxe (`0,35 × Δx_cam`), ao chegar no **Tambor (05)** (`x = 28,25 m`, `drumLandingX = 27,33 m`), a borda direita da câmera (`FOV 45°`, `Z = -9,0533 m`, ~`x_cam + 6,66 m ≈ 34 m`) **ultrapassa a borda direita do panorama**. A partir do **Almofadão (06)** (`x = 33,80 m`) e das **Gavetas (07)** (`x = 38,80 m`), o fundo simplesmente acaba e expõe o `Camera.main.backgroundColor` (`(0.007, 0.009, 0.018)`).

2. **Apagamento da iluminação de penumbra (`PenumbraT054.shader`):**
   - A luz ambiente do quarto é quase nula (`float3(.018, .022, .035) * _MoonStrength`).
   - O feixe principal vem de uma única janela (`_MoonPosition` em `uv.x = 0.564`), cujo cone cai exponencialmente e zera antes de `x ≈ 25 m`.
   - `PenumbraLightingT054` envia apenas **3 slots** de luz de contato no piso (`_FloorContact0..2`), cobrindo só urso, livros e penteadeira. `06` e `07` nem sequer receberam `DresserFloorReceiver` em `IllustratedPreview.cs`.

3. **Incoerência se apenas repetíssemos o panorama atual:**
   - `PenumbraT054.shader` desenha a abertura da janela da lua por UV fixa (`float2(.564, .558)`).
   - Se `InstallPanels` repetisse `room-panorama-v4.png` lado a lado, **a mesma janela com lua e os dois quadrinhos se repetiriam a cada 32 m**, quebrando a verossimilhança arquitetônica de um quarto contínuo.

## 2. Proposta de solução coerente para a continuação da fase

Para que o quarto continue de forma orgânica até as plataformas `08` (`x ≈ 43,6 m`), `09` (`x ≈ 49,6 m`) e além:

1. **Painel de extensão contínuo sem janela (`room-wall-extension-v1.png`):**
   - Gerar/derivar um painel contínuo (mesma altura `724 px`, mesma linha de rodapé/lambris `64/724`, mesmo papel de parede floral azul-escuro e assoalho de madeira) **sem a janela e sem os quadrinhos**, opcionalmente com variação sutil de textura/madeira para emendar perfeitamente à direita (e à esquerda, se necessário) do `room-panorama-v4.png`.
   - No `RoomBackgroundT054.InstallPanels(...)`, manter `room-panorama-v4.png` como painel central da zona inicial (`0..2`) e encadear painéis de extensão à direita cobrindo com folga até além de `x = 60 m` (já considerando `08`, `09` e margem de câmera).

2. **Ajuste mínimo e limpo no `PenumbraT054.shader`:**
   - Adicionar uma propriedade `_HasMoonWindow` (padrão `1` no painel central, `0` nos painéis de extensão) para que apenas a janela real do quarto emita brilho de abertura lunar, preservando o feixe noturno coerente e deixando os painéis seguintes iluminados pela penumbra ambiente + halo azul de Nanda + reflexo local no piso.

3. **Reflexos de piso consistentes (`DresserFloorReceiver`):**
   - Garantir `DresserFloorReceiver` (ou expansão limpa de `_FloorContact`) também para `06`, `07`, `08` e `09`, mantendo o contato visual dos móveis altos com o assoalho.
