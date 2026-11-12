# Investigação independente do handoff DeepSeek

11/11/2026 · Codex (GPT-6) · sessão `2026-11-11-codex-revisao-handoff-6d81`.

**Conclusão:** as lacunas de integração do contato são reais. Recomendo corrigir esse contrato antes de expandir a modelagem de 02–37, preservando urso e livros. Duas cobranças precisam de ressalva: o colisor dos livros está contido na capa superior e coincide com ela; vários IDs de sessão no mesmo dia são permitidos quando representam tarefas distintas.

Escopo: investigação e reprodução, sem correções de gameplay, modelos, materiais ou cena principal. Li AGENTS, handoff, regra 305, entradas 296–310, scripts e evidências; executei sondas e testes em projeto isolado. Não atribuo a mim a modelagem feita pelo DeepSeek. Não houve nova comparação Web ou certificação de paridade.

## Verificação dos sete pontos principais

### 1. Regra 305 parcialmente implementada — confirmado

`PlatformContactSurface` é uma estrutura singular com offset X/Y, inclinação Z, largura e profundidade. Não representa múltiplas áreas, degraus ou uma superfície curva. O enum `PlatformSurfaceKind` documenta a intenção, mas não cria essas superfícies físicas.

O colisor separado e o centro/rotação do contato são avanços reais. Ainda faltam consumidores consistentes: bordas da rota, alvo horizontal do salto, extensão da câmera e testes continuam usando volume em vários pontos. Referências exatas em `Evidence/handoff-review-2026-11-11/code-references.json`.

### 2. Vão/pulo por volume e módulo antigo — confirmado, inclusive no runtime

- Gerador: posição X da Etapa 1 usa larguras antigas 3,0/2,6/4,2 m; `Stage2StartX/Y` retém 4,2/0,7 m.
- `GenerateStage1Platforms` devolve specs com contato default; o builder só injeta o contato customizado depois. Consultar o gerador e consultar a cena não produz hoje a mesma descrição completa da plataforma.
- `Yuna25DPlayerController`, linha 134: derrota por ultrapassagem usa centro do root ± `spec.width/2`.
- Linhas 459–460: alcance horizontal mira o X do root e usa `spec.width`, embora a altura já venha de `GetTopCenterWorld`.
- `SideScroll25DCamera`, linha 128: alcance do enquadramento usa centro do root e largura do volume. Os centros verticais já usam contato.
- Testes analíticos: largura do volume; na Etapa 1, deltaY ainda é entre centros de volume. O teste físico usa colisão real, mas sua busca de posições de decolagem também percorre `spec.width` e não a superfície inclinada.

**Medição da cena persistida:**

| Par | Vão entre volumes | Vão entre extremos de contato projetados em X |
| --- | ---: | ---: |
| 00 → 01 | 1,560 m | 2,277 m |
| 01 → 02 | 1,010 m | 1,247 m |

A primeira diferença é 0,717 m. Essa conta mede separação geométrica de contatos declarados; não desconta raio da cápsula nem representa sozinha o alcance necessário do centro dos pés. A área do urso ainda precisa ser corrigida para representar a malha. Não há demonstração de que a transição atual já quebrou: os testes físicos passam. Há dependência real de constantes antigas, que não acompanha mudanças futuras de forma/contato.

### 3. Apoio invisível — confirmado no urso; cobrança dos livros precisa ser corrigida

Sonda na cena: malhas visuais receberam MeshColliders temporários apenas no projeto isolado; raios na normal da face verificaram uma grade interior 21×21 sobre cada colisor físico. Tolerância descritiva: 2 cm. Os objetos temporários foram removidos e a cena não foi salva. Isso é amostragem de geometria, não prova matemática de toda a superfície nem simulação da área dos pés.

| Resultado dos 441 pontos | Urso 00 | Livros 01 |
| --- | ---: | ---: |
| Malha coincide com plano | 197 | 441 |
| Sem malha sob o ponto | 136 | 0 |
| Malha abaixo do apoio | 92 | 0 |
| Malha acima do apoio | 16 | 0 |

Urso: colisor 2,48×2,23 m, cabeça com profundidade 1,65 m. Há projeção traseira sem malha e pontos apoiados em um plano acima do volume visível. No corredor Z=0, os 21 pontos não tiveram buracos nem malha abaixo, mas oito atingiram malha acima do plano, ligada às orelhas. Portanto, o defeito de apoio invisível não equivale a uma falha comprovada no caminho atual travado em Z=0; existe também risco de interpenetração visual junto às orelhas. O teste atual só comparar alturas máximas não cobre isso.

Livros: colisor 1,85×1,15 m; capa superior com profundidade 1,30 m e largura projetada aproximada de 2,15 m. Todos os pontos amostrados coincidem com a capa. **A regra 305 permite contato mais estreito que o volume. Não devemos alargar esse colisor só para igualar a caixa da pilha.** Pode haver uma faixa visível sem apoio físico nas bordas; sua extensão deve ser uma escolha explícita, validada com os pés/cápsula em ambos os sentidos, e não presumida errada por comparação de AABB.

Dados e mapa: `probe.json` e `contact-support-map.png` na pasta de evidências desta investigação.

### 4. Altura do urso — duplicação de autoria confirmada

Catálogo: 40 px → 1,27 m após arredondamento. Resolver: exceção explícita `ConceptTeddyHeight=1.10` para índice 0. Em runtime existe um valor resolvido; o problema é haver dois lugares que aparentam mandar na autoria. As entradas 301–304 registram revisão por conceito/pedido do usuário. Não há fundamento para reverter automaticamente o modelo aprovado para 1,27 m.

Recomendação: uma definição autoritativa por asset, contendo dimensões e contatos aprovados. O catálogo histórico de design pode ser referência de origem/fallback, com exceção e motivo documentados. A altura física deve derivar dessa definição única; apagar a exceção sem preservar a decisão artística seria uma regressão.

### 5. Harness fora do versionamento — confirmado

`git ls-files '*Capture*'` não inclui Platform00Capture. `git check-ignore` confirma que sua cópia em Build/CameraFramingValidation é ignorada. O capturador padrão do projeto existe, mas não reproduz as vistas específicas do urso/livros.

Preservei uma cópia exata do harness encontrado em `Evidence/handoff-review-2026-11-11/tools/Platform00Capture.original.cs.txt`, junto às novas sondas. Esses arquivos estão no working tree e ainda precisam acompanhar um commit para existir em um checkout limpo. O harness original tem caminho absoluto local e adiciona luzes de inspeção em 02–04 e 06–08; a vista 05 também usa enquadramento próprio. Essas imagens são inspeções artísticas, não equivalentes ao close jogável aprovado. A futura ferramenta permanente deve receber caminho de saída/configuração e distinguir inspeção de gameplay.

### 6. Ruído de Git — causa parcialmente reproduzida; intenção humana não pode ser deduzida

O snapshot inicial tinha **104 entradas** em `git status --porcelain -uall`; a estimativa de ~78 não é contagem atual. HEAD era `72c9f87`; há trabalho anterior commitado, mas os novos modelos ainda estão pendentes. Não fiz fetch, commit, push ou reset.

**Áudio:** o ScenePopulator chama GenerateAllSfx em toda reconstrução. Esse gerador usa `new System.Random()` sem seed fixa. Executei duas gerações no projeto isolado: sete dos onze WAVs mudaram de hash — exatamente os sete apontados no status. Os WAVs do checkout também têm os mesmos formatos e durações das versões HEAD, com conteúdo diferente. Isso demonstra uma causa suficiente e reproduzível; não identifica quem disparou a geração histórica nem prova intenção artística.

**Volume:** o diff atual só desativa MotionBlur, LensDistortion, DepthOfField, FilmGrain e ChromaticAberration. Essa configuração corresponde ao configurador de pós-processamento aprovado, que também é chamado indiretamente pelo gerador de arte. Não é evidência de alteração arbitrária só porque o editor estava aberto.

**VFX:** duas filas 3000→3010 aparecem no checkout. Origem/intenção específica não comprovadas nesta investigação; preservar. O caminho correto é `Assets/Materials/M_Moonbeam_Shaft.mat`, não o caminho abreviado incorreto do handoff.

Recomendação: separar geração explícita de áudio/arte da reconstrução geométrica; tornar saídas regeneráveis determinísticas quando essa for a intenção. Não descartar diferenças existentes silenciosamente.

### 7. Desvios de processo — separar fatos históricos e estado atual

- Entradas 303/304 e 306–308 descrevem mudanças de implementação como info, sem novos pares operacionais planned/resultado. Lacuna confirmada; não há justificativa para inventar registros retroativos de execução.
- Identificador único **por tarefa**, e não um único por dia, é o que AGENTS exige. Várias sessões no mesmo dia não constituem infração por si só.
- O arquivo atual tem um único BOM UTF-8 no byte 0. Não encontrei BOM interno. O erro anterior é relato histórico no handoff; não é erro de compilação atual.
- O teste atual dos livros usa centro da face inclinada, corrigindo a comparação AABB. Ainda testa poucos atributos: não cobre toda a extensão nem superfícies múltiplas.
- Planos 72, 92, 97, 152, 164, 168, 178 e 185 continuam sem resultado. Foram consultados e preservados; não são pendências de implementação desta sessão.

## Respostas às dez perguntas finais

1. **Como calcular vão pelo contato?** Resolver superfícies mundiais uma vez: vértices, normal, extremos, altura local e área útil. Gerador, câmera, validação de rota, salto e testes devem consumir essa mesma API. Vão entre bordas voltadas ao movimento; alvo na área realmente alcançável, com margem da cápsula, inclinação e sentido. Não basta substituir um `spec.width` isolado.
2. **Lista ou degraus?** Lista de áreas de contato, cada uma com contorno e transformação local. Degraus são várias áreas; partes curvas podem exigir aproximações coerentes/collider apropriado. Uma plataforma plana continua com uma área. Não unir áreas separadas por um vão com um retângulo envolvente.
3. **Profundidade igual ao objeto?** Deve corresponder à região visível de apoio, não obrigatoriamente ao volume total. Corrigir o urso. Manter/validar a região dos livros, sem igualar arbitrariamente à pilha inteira. Testar bordas e pés, não só máximos Y.
4. **Catálogo ou arte?** A decisão artística aprovada define o asset; uma definição única registra a geometria/contatos resultantes. Histórico do catálogo permanece rastreável. Conservar 1,10 m até haver nova decisão, removendo a ambiguidade de autoria.
5. **Harness entra no repo?** Sim. Cópia resgatada como evidência nesta investigação; converter a ferramenta permanente para parâmetros portáveis e incluí-la com dependências/instruções no commit. Ainda não foi feito commit.
6. **Áudio/Volume intencionais?** Autoria histórica não comprovada. Áudio não determinístico foi reproduzido; Volume atual corresponde às regras aprovadas. Não tratar tudo como ruído descartável nem atribuir ao editor sem verificar o gerador.
7. **Quando modelar 02–37?** Depois de corrigir o contrato de contato, verificar 00/01 com ele e publicar a ferramenta reprodutível. Depois avançar por lotes pequenos; não há estimativa de duração validada. Manter a geometria visual existente durante a correção técnica.
8. **Moldura e borla?** Pendências artísticas reais nas imagens/código. Podem ser próximas melhorias do livro após a correção técnica, avaliadas no close. Não bloqueiam a representação correta do contato.
9. **Tipografia?** Fonte atual é bitmap 5×7. Para o estilo pintado descrito, recomendo atlas desenhado/composto em resolução adequada à câmera, com licença dos elementos documentada. Bitmap é placeholder aceitável se identificado; trocar técnica não dispensa validar leitura em tamanho real.
10. **Orçamento de performance?** Não encontrei orçamento aprovado para hardware/alvo e nenhuma captura de profiling foi fornecida. Contagem estática desta cena: urso **17.596 triângulos, 16 MeshRenderers, 7 materiais distintos**; livros **378 triângulos, 30 MeshRenderers, 8 materiais**. Renderer não é sinônimo de draw call. Medir build no hardware/browser de destino, CPU/GPU, memória, batches e piores momentos antes de fixar orçamento por família. Não inventei limite de polígonos ou FPS.

## Demais itens do handoff

Os itens 7–10 (tipografia, molduras, borla e inclinação -4°) estão visíveis no código/imagens, mas dependem de avaliação artística; não foram comparados de novo ao conceito original fora das pranchas existentes. Item 11: ausência de profiling permanece. Item 12: sem nova comparação Web. Item 14: os três PNGs 06/07/08 são cópias byte a byte entre as duas pastas; redundância confirmada.

Item 15 precisa correção factual: `BuildBookStack` hoje seleciona `{dark, green, brown, brown, ochre}`; o terceiro livro usa **brown**, não Burgundy. A variável burgundy está declarada, mas não é usada nesse array. Atlas e couro continuam definidos separadamente, portanto ainda convém unificar a paleta para evitar discordâncias.

Item 16: materiais M_Teddy_Muzzle/Dark/Bow não foram encontrados no projeto principal e permanecem no clone ignorado. São resíduos de validação, não dependências necessárias demonstradas. M_TeddyPlush continua registrado na resolução da plataforma 27. Nenhum material foi apagado nesta auditoria.

## Validação nova e limites

- **EditMode 26/26 e PlayMode 21/21 reexecutados e aprovados**, XMLs nesta pasta de evidências.
- A suíte verde não verifica integralmente a regra 305. A sonda encontrou divergência de superfície com os mesmos modelos que passam nos testes.
- Mesh raycasts: 882 amostras; a linha Z=0 analisada separadamente. Não houve simulação de novas áreas curvas ou múltiplas porque elas ainda não existem no esquema.
- Sete WAVs não determinísticos reproduzidos com duas chamadas do gerador no clone. Nenhum WAV principal foi regravado pela investigação.
- Snapshot de nove arquivos críticos (incluindo cena, perfil e scripts) conferido por SHA-256 ao final: nenhum alterado durante a auditoria. Novos arquivos são documentação, evidência e diário.
- Sem nova alegação de paridade Web, aprovação artística, desempenho ou autoria das alterações anteriores.

## Encaminhamento recomendado

**Primeiro:** unificar a definição de geometria/contato e seus consumidores; corrigir o apoio do urso preservando sua forma e cobrir limites visíveis nos testes. **Em seguida:** ferramenta de captura permanente e geração sem efeitos colaterais de áudio. **Depois:** retomar modelagem e acabamento dos livros por lotes, com profiling do piloto.

Preservar: modelos 00/01, câmera aprovada, fluxo narrativo e iluminação como base de avaliação. Reavaliar luz com os novos materiais e no enquadramento real. A recomendação anterior de iniciar um piloto continua válida; o piloto agora demonstrou uma lacuna de integração que precisa ser resolvida antes da expansão.

## Evidências e reprodução

Pasta `Evidence/handoff-review-2026-11-11/`: `probe.json`, mapa de apoio, XMLs, `audio-diff.json`, `audio-regeneration.json`, `source-snapshot.json`, referências de código, trechos de log e fontes das sondas. Instruções em `README.md`. O manifest relaciona caminhos, tamanhos e SHA-256.

## Atualização posterior: novo pedido artístico do usuário

Após concluir esta investigação, o usuário pediu texturas mais próximas das duas imagens anexadas e laterais arredondadas do urso-banqueta. A implementação subsequente substituiu o BoxCollider do urso por cinco MeshColliders das formas visíveis e corrigiu a condição de derrota ao tocar curvas/um apoio já alcançado. Também colocou o capturador de arte em Assets/Scripts/Editor, com saída parametrizada. Assim, o defeito físico específico do urso e a ausência dessa fonte de captura foram tratados depois do estado investigado acima; o contrato completo de superfícies e seus consumidores continua pendente.

O relatório e os 49 testes finais dessa alteração estão em `Evidence/painted-platforms-2026-11-11/README.md`. Para reproduzir as medições antigas desta investigação, foi arquivada a cena anterior comprimida em `scene-before-art.unity.gz`, com SHA-256 correspondente ao snapshot, e os dois materiais anteriores. A sonda aceita `YUNA_HANDOFF_SCENE` para abrir essa cena de baseline no projeto isolado. Não aplicar essa baseline sobre o projeto principal atualizado.
