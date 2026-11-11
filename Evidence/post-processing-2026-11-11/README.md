# Pós-processamento do Quarto Escuro — 11/11/2026

Sessão: `2026-11-11-codex-pos-processamento-4fa9`, Codex (GPT-6).

## Resultado

Pós-processamento habilitado e persistido na câmera, Volume ligado por `sharedProfile`, recursos do renderer verificados e grading em HDR. Aplicados tonemapping Neutral, Bloom discreto, vinheta leve, FXAA e dithering. Preservados o close do próximo salto, o panorama do castelo e o acompanhamento da queda.

Dois bloqueios foram corrigidos: a câmera serializava `m_RenderPostProcessing: 0` e o Volume serializava `sharedProfile: {fileID: 0}`. O gerador usava `volume.profile = profile`, que não persistia o vínculo compartilhado da cena. A configuração central agora é `DarkRoomPostProcessingSetup`; tanto o menu específico quanto os construtores existentes a utilizam. Não é necessário regenerar arte, áudio ou plataformas para aplicar este ajuste.

## Perfil final

| Controle | Valor |
|---|---:|
| Tonemapping | Neutral |
| Grading | HDR |
| Bloom: intensidade / limiar / dispersão | 0,18 / 1,15 / 0,55 |
| Bloom: clamp | 5 |
| Vinheta: intensidade / suavidade | 0,14 / 0,45 |
| Exposição / contraste / saturação | 0 / +2 / -2 |
| Anti-aliasing | FXAA |
| Dithering | Ativo |
| Motion Blur / Depth of Field / aberração cromática / grain / distorção | Desativados |

O perfil anterior tinha Bloom 1,25, vinheta 0,32, contraste +14, saturação +12 e nenhum tonemapping. Apenas ativá-lo produziu sombras muito mais fechadas e uma aura mais forte na fada; a opção calibrada preserva melhor a penumbra já aprovada.

## Captura HDR e correção metodológica

A URP 17.3 usa o formato do `camera.targetTexture` como buffer intermediário. Isso está documentado no próprio código instalado em `UniversalRenderPipelineCore.CreateRenderTextureDescriptor`, linhas 1592–1623. Um destino padrão de 8 bits cortava os valores HDR **antes** de Bloom e tonemapping. O primeiro ensaio foi preservado em `ldr-target-trial/` e não deve ser usado para julgar os realces HDR finais.

`DarkRoomStartCapture.RenderDisplayImage` agora renderiza para ARGBHalf linear e só depois converte para um destino SDR sRGB usado pelo PNG. O helper preserva/restaura os alvos e o estado gráfico. As capturas padrão e a galeria de parede também usam esse caminho. A captura de castelo reposiciona os atores e seleciona a plataforma 10; antes, deslocava somente a câmera enquanto Yuna continuava no chão inicial.

As auditorias anteriores mediram pixels de capturas LDR. Suas observações são válidas sobre aquelas imagens, mas não comprovam isoladamente que os mesmos realces fossem cortados no buffer HDR do GameView. Esta limitação e os resultados novos foram acrescentados ao diário sem apagar registros anteriores.

## Comparação e medidas

Foram capturados seis estados controlados de Play Mode, com três variantes consecutivas por estado: `off`, `legacy-enabled` e `calibrated`. Os três renders de cada estado usam as mesmas posições de atores e câmera. `legacy-enabled` é um ensaio do perfil antigo **ativado na cadeia corrigida**, não o estado anterior real, cujo pós-processamento estava desligado. Todas as variantes finais usam o caminho de captura HDR.

Os estados abrangem largada, close após castelo, aproximação da porta, portal, chão após queda e panorama do castelo. `Camera.Render` não inclui overlays OnGUI; os atores são posicionados pelo harness para comparação. Não é uma gravação da travessia nem uma auditoria do áudio ou de desempenho.

No close do castelo:

| Medida descritiva | Desligado | Perfil antigo ligado | Calibrado |
|---|---:|---:|---:|
| Luma média do quadro (0–255) | 11,405 | 5,940 | 10,541 |
| Pixels abaixo de luma 8 | 78,978% | 87,952% | 80,324% |
| Pixels totalmente brancos na caixa de Nanda (RGB ≥250) | 49,550% | 1,007% | 0% |

As caixas projetadas incluem fundo e não são máscaras exatas dos personagens. Os limites 8 e 250 são faixas descritivas, não critérios de acessibilidade. O vermelho ainda pode atingir 250 no brilho quente; o resultado não implica ausência de qualquer saturação de canal. `metrics.json` registra todos os estados. As pranchas não recebem correção de exposição posterior.

## Validação e preservação

- **17/17 EditMode e 20/20 PlayMode aprovados**, zero falhas. O novo teste valida o vínculo persistente câmera → Volume/perfil → recursos do renderer.
- Os 18 renders HDR finais foram gerados; a análise verifica que o pós-processamento altera efetivamente os pixels em todos os seis estados.
- `integration.json` comprova a alteração somente dos blocos de pós-processamento da câmera e do Volume na cena. O Transform da câmera e os demais blocos foram preservados.
- Nenhuma mudança em física, posição das plataformas, intensidade das luzes, materiais de personagens ou desenho do portal. Os demais itens P0/P1 continuam como próximos trabalhos.
- Aprovação visual do usuário ainda pendente. Não foi feita nova comparação com o Web nesta tarefa; a referência é a direção e o enquadramento aprovados na sessão anterior.
- O log de captura contém uma exceção do indexador `UnityEditor.Search.SearchDatabase`, já observada na cópia de validação em sessões anteriores. Os seis estados e os 18 renders concluíram normalmente, sem erro de compilação ou shader.
- Na conferência final apareceram também alterações em sete SFX e nas filas de renderização de dois materiais VFX, não editados por esta implementação. Sua origem não foi estabelecida; foram preservados e registrados no diário, sem atribuí-los a esta tarefa.

## Arquivos

- `comparison-close.png`: antes/depois no close.
- `comparison-contexts.png`: antes/depois no close, no chão após queda e no panorama.
- `comparison-profiles.png`: as três opções nesses contextos.
- `off-*.png`, `legacy-enabled-*.png`, `calibrated-*.png`: 18 renders finais em 1280×720.
- `unity-metadata.json`: posições, câmeras, materiais e luzes dos estados controlados.
- `metrics.json`, `analyze.py`: medidas e reprodução das pranchas; dependências Pillow/NumPy já utilizadas na auditoria anterior em `Build/VisualAuditTools`.
- `capture-harness.cs.txt`, `capture-hdr.log`, `editmode.xml`, `playmode.xml`: ferramenta e evidência das execuções.
- `integration.json`, `manifest.json`: preservação da cena e inventário com SHA-256.

Aplicação pelo menu: **YunaLegend → Configurar Pos-processamento do Quarto Escuro**. A configuração central também é chamada ao reconstruir a cena, evitando regressão para câmera desligada ou Volume sem perfil persistido.
