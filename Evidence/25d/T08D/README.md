# T08D — candidata individual dos blocos ABC04

Estado: em revisão visual, sem integração Unity. A build aprovada até a cômoda permanece como estava.

A consulta visual ao legado mostrou uma pilha escalonada com topo estreito. Candidata original: seis blocos3-2-1, A no topo, B/C abaixo, madeira pintada com acabamento da cômoda aprovada. Não houve extração de assets ou código legado.

Arquivo: `Assets/Art/Illustrated25D/Environment/ABC/abc-stack-v1.png`.
Ferramenta: image_gen integrada, fundo transparente; referência local da cômoda apenas para estilo. Prompt completo em `prompt-v1.txt`. Fonte preservada em `C:/Users/sergi/.codex/generated_images/01a1d6c5-873f-7641-a8c8-db4035243db5/exec-363fb2b5-c641-42dd-9c64-a3cae2f8a3e1.png`.
SHA256: `75af893ca38895c3d3a0984685783064eca5e747ef68bce36b829a42390aa968`.

Inspeção: imagem completa1278x1230RGBA, sem recorte do objeto;599899 pixels totalmente transparentes. Alpha medido por ffmpeg e Python stdlib; sem redimensionamento ou edição da pintura. A maioria do interior usa alpha254, próximo de opaco. Importador proposto2048, NPOTNone, sem mipmaps, transparência ativada. Não foi executado Editor/build nesta etapa.

O bloco superior mede cerca37,4% da largura inteira; a borda frontal horizontal mede aproximadamente31%. São estimativas visuais da candidata. A observação não demonstra largura exata de colisor no legado. Ver `CONTRATO-APOIO.md` e `asset-measurements.json`.

Revisão proposta: a silhueta escalonada corresponde ao objeto que você lembrava? O topo único parece estreito na medida desejada? Esta aprovação é da peça; contato dos pés, escala em metros, piso, perfil3D, vão e dificuldade serão verificados na candidata jogável seguinte.

Próximo passo após revisão: medir lip/baseline, definir escala uniforme e apoio próprio antes de instalar; integrar somente04 em uma nova candidata preservando câmera aprovada, cedimento/tropeço e peças anteriores. Validar salto/erro de borda/retry em execução. Estante continua adiada; T06/T07 globais e validações de aparelho/WebGL/áudio/desempenho não mudam.
