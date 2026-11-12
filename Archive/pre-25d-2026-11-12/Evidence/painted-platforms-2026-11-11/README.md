# Urso e livros — pintura e formas próximas às referências

11/11/2026 · Codex (GPT-6) · sessão `2026-11-11-codex-arte-urso-livros-2c49`.

Pedido: aproximar os materiais das duas plataformas já modeladas das imagens anexadas `giant-bear.png` e `open-books.png`, arredondando as laterais do urso-banqueta. Foram usadas as skills imagegen e audiovisual-parity-audit. As imagens anexadas serviram de referência visual; os sprites não foram importados nem o código legado acessado.

## Resultado

- **Urso:** textura nova de pelúcia pintada em caramelo/castanho, sem o normal map procedural agressivo anterior. Laterais mais curvas (expoente do superelipsoide 0,50→0,90), banqueta com 80 segmentos e bordas biseladas. Altura aprovada de 1,10 m preservada.
- **UVs:** topo mapeado por X/Z; frente e lados pelas respectivas projeções. A antiga projeção exclusivamente frontal reduzia o topo plano a uma faixa da textura e criava esticamento visível. Separações de UV preservam as normais suaves. Corrigido também o limite inferior da malha achatada para não ultrapassar a altura declarada.
- **Costuras:** reposicionadas e orientadas à normal da superfície curva; a coroa acompanha o contorno, as orelhas não carregam pontos flutuando no antigo plano frontal.
- **Livros:** atlas novo com letras serifadas douradas e ornamentos; capas superiores com moldura e filigrana próprias. A textura de capa é aplicada a uma superfície separada, evitando repetir o desenho nas laterais finas do cubo. Pequena margem entre faixas UV evita amostrar a lombada vizinha. Paleta dos cinco volumes ajustada para acompanhar as faixas.
- **Contato do urso:** o antigo retângulo de 2,23 m de profundidade foi substituído por cinco MeshColliders estáticos que compartilham as malhas da cabeça, orelhas e base. Rosto, laço e costuras são detalhes visuais. A mudança segue a forma nova e remove o apoio retangular invisível apontado pela investigação.
- **Detecção de derrota:** uma superfície curva pode ter normal vertical de apoio e componente horizontal contrária à corrida. A detecção antiga tratava isso como parede e também podia matar Yuna ao encostar em um relevo do apoio já alcançado. A condição foi corrigida para parede sem normal de apoio, pertencente a uma plataforma nova. Gaps, velocidade e altura dos saltos foram preservados.

## Assets e proveniência

Geração com a ferramenta embutida **image_gen**, sem CLI/API paga configurada separadamente. Três gerações novas orientadas por descrição de estilo; nenhum recorte do sprite legado foi usado como textura. Prompts completos, sem abreviação: `image-prompts.json`.

Arquivos finais no projeto:

- `Assets/Materials/T_Bear_Fur_Painted.png`
- `Assets/Materials/T_Book_Spines_Painted.png`
- `Assets/Materials/T_Book_Cover_Painted.png`

Os três PNGs entregues pela ferramenta têm 1254×1254 pixels. A importação define explicitamente Texture2D, sRGB, mipmaps, filtro Trilinear, anisotropia 4 e limite 2048. Os originais da ferramenta permanecem em `$CODEX_HOME/generated_images`; cópias consumidas pelo projeto estão em Assets, com `.meta` persistidos.

Materiais: `M_Bear_Fur`, `M_Book_Spines` e novo `M_Book_Painted_Cover`. As texturas procedurais anteriores continuam no projeto como histórico/fallback de outros materiais. Madeira, páginas, focinho, laço e pequenos detalhes conservam parte de seus materiais anteriores; esta entrega não afirma reprodução integral da pincelada da referência em cada peça.

## Ferramentas permanentes e reprodução

- `Assets/Scripts/Editor/DarkRoomPaintedPlatformSetup.cs`: aplicação localizada, integrada também ao gerador de arte. Menu **YunaLegend/Aplicar arte pintada no urso e livros**. Reconstrói apenas os visuais 00/01 e o contato de 00, atualiza três materiais, sem Populate geral nem geração de áudio.
- `Assets/Scripts/Editor/DarkRoomPlatformArtCapture.cs`: fonte de captura no projeto, substituindo a dependência exclusiva do harness ignorado em Build. Saída parametrizada por `-yunaArtCaptureOutput`.

Para reproduzir, use um projeto isolado com cópias de Assets, Packages e ProjectSettings deste checkout e Unity 6000.3.28f1. O editor principal pode permanecer aberto em outro projectPath. Execute sequencialmente:

```powershell
# Ajuste os caminhos; execute no projeto isolado.
& 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe' -batchmode -quit `
  -projectPath CAMINHO_ISOLADO `
  -executeMethod YunaLegend.EditorScripts.DarkRoomPaintedPlatformSetup.ApplyAndSave `
  -logFile CAMINHO_SETUP_LOG

& 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe' -batchmode -quit `
  -projectPath CAMINHO_ISOLADO -executeMethod DarkRoomPlatformArtCapture.Run `
  -yunaArtCaptureOutput CAMINHO_ABSOLUTO_DAS_IMAGENS -logFile CAMINHO_CAPTURA_LOG
```

O capturador prepara posições/câmeras temporárias e não salva a cena; usá-lo em projeto isolado evita alterar uma sessão de edição em andamento. O menu de aplicação abre e salva a cena alvo: caso a cena principal já estivesse carregada antes desta entrega, reabra-a para carregar os valores salvos externamente.

## Evidências visuais

- `comparison-detail.png`: antes/depois com recortes idênticos, sem ajuste de exposição ou repintura dos renders.
- `before/`: imagens anteriores preservadas da entrega DeepSeek.
- `after/`: nove PNGs HDR 1280×720, inspecionados após as mudanças finais.
- Vistas 02–04 e 06–08 usam luzes temporárias de inspeção, com posições reproduzidas do harness anterior; não são o resultado exclusivo das luzes de gameplay.
- Vistas 01/05/09 usam apenas luzes da cena. A 05 é uma vista de aproximação preparada; a **09 usa o close aprovado**, com Yuna sobre o urso e Nanda sobre os livros em poses preparadas.
- São capturas estáticas de Camera.Render/HDR, sem OnGUI, não vídeo de travessia ou nova comparação Web. O render de inspeção facilita avaliar o acabamento, mas não substitui julgar a leitura no close escuro.

## Testes e recuperação

**EditMode 28/28 e PlayMode 21/21 aprovados**, XMLs `editmode-final.xml` e `playmode-final.xml`. Total: 49 testes distintos na versão final. A recuperação focada de input também passou 5/5, sem contá-la novamente no total.

Regressões relevantes verificadas: UVs não colapsam no topo achatado; materiais persistidos usam efetivamente as três texturas pintadas; não existe apoio do urso em Z=±1 m acima da malha; primeiros saltos/input em 30/60/120 fps; pares obrigatórios do percurso e corrida contínua existentes.

O primeiro ensaio aplicou a geometria, mas reteve materiais antigos por retorno silencioso quando uma textura não carregava. Metadados mínimos haviam permitido importação como Cubemap. O configurador agora força Texture2D e falha explicitamente se faltar a textura; o teste de referência de material cobre esse caso. Capturas do ensaio preservadas em `first-trial/geometry-only/`.

A primeira suíte física com a nova malha teve 19/21, revelando a condição de derrota em curvas. XML preservado em `first-trial/playmode-curved-contact-failed.xml`. A correção descrita acima recuperou os testes, sem deformar o modelo para voltar ao colisor antigo. Trechos de logs em `validation-excerpts.txt`; logs completos locais em Build/painted-*.log.

`git diff --check -- Assets/Scripts` aprovado. Fontes e materiais integrados comparados byte a byte com a cópia efetivamente testada. SHA-256 dos sete WAVs já alterados antes deste trabalho e do Volume foram preservados. Não foram modificados áudio, perfil de pós-processamento, enquadramento, luzes da cena ou rota nesta entrega.

## Integração e limites

`integration.json` registra a comparação da cena anterior com a validada: cinco blocos existentes alterados; 215 blocos antigos de visual/contato removidos, 252 adicionados. Nenhuma alteração fora do escopo permitido de visual 00/01, contato 00, referências de filhos, array de materiais do builder e malhas embutidas. Todos os demais blocos existentes permaneceram iguais. O diff Git global contém também trabalho anterior e não deve ser atribuído integralmente a esta sessão.

A auditoria anterior está em `docs/ia/INVESTIGACAO-HANDOFF-2026-11-11.md`. O defeito físico específico do urso foi tratado aqui como parte da alteração de forma. **O contrato geral de áreas múltiplas e a migração de cálculo de salto/bordas/câmera para a superfície real continuam pendentes.** Os 49 testes não certificam esse contrato completo.

As plataformas 02–37 seguem com seus visuais provisórios. Yuna/Nanda ainda usam proxies. Inclinação geral dos livros, borla e acabamento de pequenos detalhes ainda podem se aproximar mais da referência; não foram declarados finalizados. Sem nova medição de desempenho ou paridade completa com o legado. Nenhum commit/push foi realizado.

O manifest contém os hashes das evidências; prompts e esta descrição são espelhados no diário.
