# Plano de Desenvolvimento — Fase 2: Toy Room (Sala de Brinquedos)

> **Status:** Planejamento aprovado para produção (após limpeza da Fase 1 e pesquisa de direção artística).
> **Branch:** `feature/toyroom`
> **Nota de Pesquisa Legada:** A execução ao vivo da URL legada (`https://luciussama.github.io/legendGirl/`) via `chrome_devtools` não pôde ser concluída nesta sessão porque o Chrome não estava com `DevToolsActivePort` ativo (`chrome://inspect/#remote-debugging`). Os detalhes de mecânica e narrativa legada abaixo foram extraídos do roteiro oficial já consolidado no repositório ([02-CAPITULO-1-PARTE-2-SALA-DE-BRINQUEDOS-EXPLORACAO.md](../roteiro-unity/02-CAPITULO-1-PARTE-2-SALA-DE-BRINQUEDOS-EXPLORACAO.md)) e devem ser conferidos visualmente ao vivo assim que o debug remoto do Chrome for habilitado.

---

## 1. Síntese da Direção Artística (Diablo IV × Legend of Mana)

| Pilar | Referência Principal: **Diablo IV** | Referência Estética: **Legend of Mana** | Aplicação na **Toy Room (Unity URP 3D Isométrico)** |
| :--- | :--- | :--- | :--- |
| **Estrutura & Câmera** | Perspectiva isométrica fixa, leitura imediata de silhuetas, enquadramento com *foreground framing* e *pathing* claro. | Diorama de "livro ilustrado vivo", proporções fantásticas e escala lúdica. | Câmera 3D isométrica fixa (~35°–40° de inclinação), com enquadramento amplo da sala, bordas em *vignette* suave e *Depth of Field* sutil para efeito maquete. |
| **Composição & Props** | "Culture kits", objetos com história/desgaste e distribuição natural que guia o olhar; oclusão limpa no primeiro plano. | Texturas pintadas à mão (aquarela/acrílico), sombras de borda marcada, contornos coloridos sutis. | 7 móveis principais + micro-props de brinquedoteca (almofadas, lápis, livros caídos, tapete artesanal) com albedos pintados à mão e *Ambient Occlusion* estilizado. |
| **Iluminação & Atmosfera** | Alto contraste, feixes de luz direcionados (*light shafts*), poeira volumétrica e sombras profundas. | Paleta saturada e calorosa (âmbares, cremes, sálvia, turquesa, coral), luz mágica vibrante. | Luz solar matinal dourada entrando pelas janelas norte com *god rays* suaves + poeira flutuante; sombras tintadas em índigo/violeta suave (nunca preto puro); luz azulada contínua da **Nanda** (`AGENTS.md` §3). |
| **Shading (URP)** | Separação clara entre plano jogável e fundo via contraste luminoso e névoa de profundidade. | *Ramp cel-shading* com textura 1D de gradiente + *MatCap* pictórico + *rim light* colorido. | Shader URP customizado (*Painterly Ramp + Rim*) com mapas de normal pictóricos sutis e contorno *screen-space* fino e colorido. |

---

## 2. Mecânicas da Fase

1. **Movimentação Livre em 3D Isométrico (`X` e `Z`):**
   - Yuna caminha em 360° pelo piso da sala (eixos X e Z), com colisão física 3D contra paredes e os 7 móveis principais.
   - Rotação suave da personagem para a direção do deslocamento, preservando leitura clara em perspectiva isométrica.
2. **Coleta e Transporte de Brinquedos (8 Brinquedos + Peso):**
   - Ao se aproximar de um brinquedo no chão (sem estar carregando outro) e acionar interação (ou coleta por proximidade configurável), Yuna pega o objeto com as duas mãos à frente do corpo (`isCarried = true`).
   - Cada brinquedo possui um **fator de peso (`weight`)** que multiplica a velocidade de caminhada da Yuna (ex.: `0.85` na Torre de Blocos até `0.96` no Coelhinho) e ajusta o balanço da animação.
3. **Entrega no Baú de Brinquedos (`toy-chest`):**
   - Enquanto Yuna carrega um brinquedo, **Nanda (azul)** voa até acima do baú, servindo de farol diegético.
   - Ao alcançar a zona frontal do baú, a tampa articulada abre com brilho mágico dourado, o brinquedo entra em arco (`isOrganized = true`) e a tampa fecha com feedback sonoro cristalino.
4. **Mecânica de Estranhamento / Recorrência (Brinquedos 7 e 8):**
   - Os **6 primeiros brinquedos** são guardados normalmente.
   - Ao restarem **2 ou menos brinquedos** na sala (`remaining <= 2`, isto é, na guarda do 7º e do 8º), o sistema ativa silenciosamente a **recorrência** (`recurrenceActive = true`):
     - Após guardar o 7º (e o 8º) brinquedo no baú, após um atraso curto (~1,5s / 90 frames), o brinquedo desaparece do baú e **reaparece em um ponto livre fora do enquadramento principal da câmera** (ou em ângulo cego distante de Yuna, mantendo a câmera fixa isométrica ou janela de enquadramento fora do foco direto).
     - O objetivo emocional é provocar *"Ué... eu já não tinha guardado isso?"* sem revelar de imediato a causa sobrenatural/imaginária.
   - Ao guardar novamente brinquedos retornados (`returnedToyCount == 2`), dispara diálogo curto de estranhamento entre Nanda e Yuna; ao atingir `returnedToyCount >= 4` (com mãos vazias), dispara o evento de **Investigação e Revelação da Espada de Brinquedo**.

---

## 3. Fluxo de Gameplay

```text
[Transição da Fase 1 (Portal)]
        │
        ▼
[Introdução da Sala (~23s)] ── Yuna e Nanda chegam ao tapete central; falas + narração poética
        │
        ▼
[Exploração: Coleta 1..6] ── Pegar brinquedo ──► Levar ao Baú ──► Guardar (sucesso normal)
        │
        ▼ (restam <= 2 brinquedos)
[Fase de Estranhamento (7º e 8º)]
   ├── Guarda 7º/8º ──► Timer 1.5s ──► Reaparece fora do foco principal da câmera
   ├── 2º retorno guardado ──► Diálogo curto ("Estranho... / Acho que já tinha guardado.")
   └── 4º retorno guardado ──► Cutscene de Investigação
                                    │
                                    ▼
                        [Revelação da Espada de Brinquedo]
                                    │
                                    ▼
                        [Busca e Coleta da Espada] ── Pose heroica + tema musical
                                    │
                                    ▼
                        [Gatilho para Fase 3: Arena Imaginária]
```

---

## 4. Layout do Cenário (Planta Isométrica ~16m × 12m)

- **Orientação Isométrica:** Parede Norte/Fundo com duas grandes janelas ensolaradas (feixes de luz matinal em diagonal sobre o piso de madeira envernizada); área central coberta por um grande tapete artesanal pastel; área Sul/Frente com molduras sutis de primeiro plano (cortina/beiral desfocado ou quina de brinquedos fora da zona de colisão).
- **7 Móveis Principais (Colisão 3D):**
  1. `toy-chest` — **Baú de Brinquedos** (canto Norte-Direito, alvo principal com tampa articulada e brilho interno).
  2. `bookshelf` — **Estante de Livros** (parede Norte-Esquerda, livros coloridos e miniaturas).
  3. `wardrobe` — **Guarda-Roupa** (parede Norte-Centro, madeira clara entalhada).
  4. `rocking-horse` — **Cavalinho de Pau** (setor Centro-Oeste).
  5. `table` — **Mesinha Infantil** (setor Centro-Sul).
  6. `armchair` — **Poltrona Aconchegante** (canto Sudoeste, cria ponto de oclusão/esconderijo natural para reaparecimento).
  7. `block-fortress` — **Fortaleza de Blocos** (canto Sudeste, torres coloridas que bloqueiam linha de visão para reaparecimento do 7º/8º brinquedo).
- **Distribuição dos 8 Brinquedos:** Espalhados entre as zonas abertas e cantos periféricos (Ursinho Pipoca, Trenzinho Real, Robô Estelar Faísca, Coelhinho de Algodão, Patinho Imperial, Torre de Blocos, Tamborzinho Encantado, Caixinha de Surpresa), garantindo que os pontos de *respawn* da recorrência fiquem em zonas periféricas fora do enquadramento/foco imediato.

---

## 5. Assets Necessários

### 5.1. Cenário e Arquitetura 3D (Estilo Diablo IV × Legend of Mana)
- Piso modular de tábuas de madeira envernizada + tapete central com textura aquarelada.
- Paredes com lambris, papel de parede creme-rosado pintado à mão e 2 janelas com vitral/moldura e *light shafts*.
- 7 móveis 3D otimizados (*low/mid-poly* com bordas chanfradas e textura hand-painted).
- Micro-props decorativos sem colisão pesada (almofadas, lápis, livros caídos, quadros na parede) para riqueza visual de diorama.

### 5.2. Personagens e Interativos
- **Yuna 3D (ou 2.5D adaptado à câmera isométrica):** Rig/animações de `idle`, `walk_8dir`, `pickup`, `carry_light/medium/heavy`, `drop_in_chest`, `sword_raise`, `shocked`.
- **Nanda (Azul):** Fada guia com brilho azulado (`AGENTS.md` §3), asas translúcidas e partículas de poeira estelar azul-ciano.
- **8 Brinquedos 3D:** Ursinho, Trenzinho, Robô, Coelhinho, Patinho com coroa, Torre de blocos, Tamborzinho, Caixinha de surpresa.
- **Espada de Brinquedo:** Espada de madeira dourada com guarda-mão turquesa e cabo coral + aura luminosa.

---

## 6. Sistemas Necessários (Arquitetura Modular `Core` + `Runtime`)

Seguindo `AGENTS.md` §7 (SRP, Humble Object, Data-Driven, Observer/Events, Allman style):

1. **`ToyRoom.Core` (`.asmdef` puro C#, `noEngineReferences: true`):**
   - `ToyRoomStateMachine`: estados (`Intro`, `Exploring`, `RecurrenceActive`, `InvestigationCutscene`, `SeekingSword`, `SwordPickup`, `Completed`).
   - `ToySpec` & `ToyRoomCatalog`: descritor imutável dos 8 brinquedos (ID, nome, peso, posição inicial) e zonas de *respawn* válidas.
   - `ToyRecurrencePlanner`: lógica pura que controla a fila de retorno (delay 1,5s), seleção de ponto fora do frustum/foco da câmera e longe de Yuna, e contadores (`organizedCount`, `returnedToyCount`).
2. **Controllers `MonoBehaviour` Especialistas (adaptadores finos):**
   - `ToyRoomStageBootstrap`: *Composition Root* que conecta catálogo, controllers e eventos.
   - `IsometricToyRoomCamera`: câmera fixa em perspectiva isométrica, com enquadramento de sala inteira ou *pan* sutil controlado apenas em cutscenes (`Investigation`, `SwordReveal`).
   - `ToyRoomPlayerController`: movimentação 3D (`X/Z`) com aplicação do multiplicador de `weight` e interação de coleta/entrega.
   - `ToyChestController`: animação da tampa (`lidOpen`), *glow* interno e receptor de entrega via evento.
   - `ToyItemView`: representação 3D de cada brinquedo, *bounce* ao ser carregado e *respawn* silencioso.
   - `NandaToyRoomGuideController`: orbita Yuna na busca, voa sobre o baú quando Yuna carrega um brinquedo e guia até a Espada na etapa final.
   - `ToyRoomHudController`: reuso/extensão do `StageHudController` para caixa de diálogo vitoriana, narração poética manuscrita e contador discreto de brinquedos.

---

## 7. Efeitos Visuais (VFX) e Sonoros (SFX / Música)

- **VFX:**
  - *God rays* / feixes de luz matinal pelas janelas + partículas de poeira flutuante no ar.
  - Rastro de poeira estelar **azulada** da Nanda.
  - Brilho mágico dourado no interior do Baú ao abrir a tampa + partículas sutis de "guardado".
  - *Sparkle* discreto ao redor da Espada de Brinquedo na revelação.
  - *Desaparecimento/reaparecimento silencioso* no 7º e 8º brinquedos (sem explosão chamativa no baú, reforçando o mistério sutil).
- **Áudio (SFX & Música):**
  - Som de portal na chegada (`portalExitWhoosh`), passos em madeira vs tapete.
  - SFX de pegar brinquedo (toque macio/lúdico) e guardar no baú (tampa de madeira + chime cristalino).
  - *Stinger* sutil de mistério no diálogo de estranhamento (`returnedToyCount == 2`).
  - Música da Sala de Brinquedos (alegre/curiosa em fade-in após intro) → transição para o **Tema da Espada** na coleta final.

---

## 8. Eventos Especiais da Fase

1. **Introdução Cinematográfica (`ToyRoomIntroduction`):** ~23s com chegada pelo portal, 2 falas da menina, 2 quadros de narração poética e fala animada da Nanda apontando o baú.
2. **Fenômeno do 7º e 8º Brinquedo (Recorrência Silenciosa):** Desaparecimento do baú após 1,5s e reaparecimento em ponto fora do enquadramento principal da câmera.
3. **Diálogo de Estranhamento:** Disparo automático após guardar 2 brinquedos retornados (*"Estranho... / Acho que esse brinquedo já tinha sido guardado."*).
4. **Cutscene de Investigação e Revelação da Espada:** Pausa de controle após 4 retornos guardados, órbita da Nanda, narração poética e aparição brilhante da Espada de Brinquedo à direita do baú.
5. **Coleta Heroica e Transição:** Yuna recolhe a espada, ergue-a por 5s com narração poética e dispara a transição para a Fase 3 (Arena Imaginária).

---

## 9. Interface Necessária (UI / HUD)

- **Contador Discreto de Brinquedos:** Indicador limpo no canto (ex.: `Brinquedos: X / 8`) que reforça a estranheza quando o contador regride ao 7º/8º brinquedo reaparecer.
- **Prompt Contextual Diegético/Discreto:** Indicação leve de interação (`[E]` / `[Espaço]` / botão de ação) ao lado de um brinquedo ou do baú.
- **Caixa de Diálogo Vitoriana + Narração Poética Manuscrita:** Reaproveitar padrão visual e proteção contra vazamento de input da Fase 1.
- **Seletor de Fases (`TitlePhaseSelector`):** Registrar a cena `ToyRoom_Gameplay.unity` no `EditorBuildSettings` e no dropdown da tela inicial.

---

## 10. Critérios de Conclusão (Definition of Done)

- [ ] **Arquitetura & Testes:** `ToyRoom.Core` com cobertura de testes unitários **EditMode** (máquina de estados, cálculo de peso, lógica de recorrência nos brinquedos 7 e 8, seleção de *spawn* fora da câmera) e testes **PlayMode** de coleta, colisão e sequência completa até a espada.
- [ ] **Direção Visual:** Cenário 3D isométrico aprovado visualmente combinando profundidade/composição (*Diablo IV*) e pintura/paleta calorosa (*Legend of Mana*), com Nanda azulada.
- [ ] **Gameplay & Mistério:** Os 6 primeiros brinquedos guardam normalmente; o 7º e o 8º disparam o desaparecimento/reaparecimento fora do enquadramento principal causando estranheza genuína.
- [ ] **Performance & Zero Regressão:** 60 fps estáveis, zero erros de console, e cena da Fase 1 (`Fase-Jogavel-Completa.unity`) + testes da Fase 1 continuam 100% funcionais.

---

## 11. Lista de Tarefas de Implementação (Ordem de Prioridade)

1. **P1 — Setup de Cena, Câmera Isométrica e Pipeline URP:** Criar `ToyRoom_Gameplay.unity`, configurar câmera fixa isométrica, iluminação matinal + *Volume* (DoF, Vignette, Color Grading) e shader pictórico *Ramp/Rim*.
2. **P2 — Núcleo Puro (`YunaLegend.ToyRoom.Core.asmdef`) + Testes EditMode:** Implementar `ToyRoomStateMachine`, `ToyRoomCatalog`, `ToyRecurrencePlanner` e validar todos os estados/regras em NUnit.
3. **P3 — Blockout 3D da Sala, 7 Móveis e Movimentação (`X/Z`):** Montar piso, paredes, colisores dos 7 móveis e `ToyRoomPlayerController` com leitura de peso.
4. **P4 — Sistema de Coleta, Baú Articulado e Nanda Guia (Azul):** Implementar `ToyItemView`, `ToyChestController` e `NandaToyRoomGuideController`.
5. **P5 — Mecânica de Recorrência (Brinquedos 7 e 8) e Pontos Fora da Câmera:** Conectar o timer de 1,5s, filtro de visibilidade da câmera (`GeometryUtility.TestPlanesAABB` / margem de frustum) e eventos de estranhamento.
6. **P6 — Cutscenes, Diálogos, Narração Poética e Espada de Brinquedo:** Implementar introdução, investigação, revelação da espada, pose heroica e transição para Fase 3.
7. **P7 — Arte Final 3D (Diablo IV × Legend of Mana), VFX e Áudio:** Substituir blockout por modelos/texturas pintadas à mão, adicionar *god rays*, partículas e trilhas/SFX.
8. **P8 — Integração no Menu (`TitlePhaseSelector`) e Validação PlayMode Completa:** Registrar cena na build e validar fluxo ponta a ponta (1→6, recorrência 7→8, espada, retry/reset limpo).
