# T02.1 — primeira Yuna ilustrada na Unity

Estado: **aprovada pelo usuário como microentrega estática**, conforme `reviews/T02.1-v1-aprovacao.md`, registrada na sessão `2026-11-12-codex-camera-25d-b91f`. T02 permanece ativa. Não equivale a animação, câmera final, piloto jogável ou aprovação da fase inteira.

Sessão `2026-11-12-codex-unity-proof-25d-a73e`, Codex (GPT-6), 12/11/2026.

Checkpoint Git posterior: `current-source-checkpoint.json` registra o hash da cena atual, cujos IDs internos Unity diferem da captura. A comparação canônica de todos os objetos/componentes não encontrou diferença semântica. A cena atual foi preservada e o manifesto original continua sendo o registro da captura; a imagem aprovada não mudou.

## O que avaliar

`01-yuna-unity-dark.png` é uma captura real de câmera em Play Mode Unity 6000.3.28f1, 1280×720. `02-yuna-unity-light.png` mostra o mesmo sprite em fundo claro, para expor contaminação nas bordas. O bloco é apenas um apoio técnico; não é proposta de plataforma ou mudança de assinatura visual. A personagem está parada e ampliada para inspeção; esse enquadramento não define seu tamanho na câmera de gameplay.

A pintura aprovada foi copiada integralmente para `Assets/Art/Illustrated25D/Yuna/Yuna_Profile_v1.png`, sem redesenho, recorte ou processamento externo. Hash original e importado idênticos: `0b9315b4dd525f23f2ef14db04682846b43b3ca7793f47670129afc88dfb9044`.

## Contrato verificado

- Cena separada: `Assets/Scenes/Validation/IllustratedYunaProof.unity`. Não adicionada ao build de publicação.
- Root copiado da Yuna na Gameplay, com o controlador existente; altura 1,4 m, raio 0,35 m, centro y 0,7 m, skin 0,08 m. Velocidade 5,2 e salto 2,6 preservados. Autorun permanece desligado nesta prova estática. Não foram exercitados gravidade, salto ou pouso em movimento.
- Imagem 1024×1536, importação Single Sprite, uncompressed, bilinear, alpha transparency, sem mipmaps. 1049,2858 pixels/unidade; pivô (536,29) em pixels a partir do canto inferior esquerdo. Altura definida pela silhueta opaca entre y38 e y1507 da imagem fonte, em vez da margem transparente.
- Shader URP Sprite-Unlit-Default, cor branca, ordem 0. A pintura mantém seu sombreado; não há uma segunda iluminação sobreposta. Interação final com a luz do quarto ainda precisa de definição.
- Apoio técnico tem topo y0; pivô da sola em y0, erro geométrico nominal 0. Raycast físico encontra esse apoio. A verificação é geométrica e visual estática; não certifica contato durante animação.
- Um sprite magenta mais próximo da câmera, mas ordem −10, permanece atrás da silhueta. Um bloco opaco mais próximo oculta a personagem. Capturas `03-sorting-rear.png`, `04-occlusion-foreground.png` e amostras de pixels no JSON comprovam os casos. Helpers ficam desativados na cena salva.
- Hash da Gameplay antes/depois idêntico: `d507921b1244c2a3186ba820bc54a3eae8daf9208dcde677f7eed438c9e358ed`. Sem mudança no controlador ou física da fase principal.

O primeiro ensaio está preservado em `attempt01/`: a amostra do teste de ordenação caiu fora da silhueta e falhou. A fixture foi centralizada sobre o corpo; a personagem e a tolerância não foram alteradas para passar. O segundo ensaio passou todos os contratos: `unity-proof.json`, marcador `ILLUSTRATED_YUNA_PROOF_OK` no log `Build/25d-yuna-proof-final.log`. O indexador Editor.Search apresenta uma exceção de inicialização no ambiente, sem impedir o ensaio; não declarar log sem exceções.

## Retomada

Abrir a cena de validação na Unity para revisão. Para regenerar, copiar o projeto e a imagem fonte `Evidence/25d/T01/yuna-profile-v1.png` para a mesma estrutura da cópia isolada e executar:

```
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.EditorScripts.IllustratedYunaProof.Run -proofOutput <evidencias> -logFile <log>
```

Sem `-quit`: o harness entra em Play Mode, aguarda 60 frames, captura, grava JSON, sai de Play Mode e encerra o Editor com código correspondente. O método BuildScene também está no menu YunaLegend/25D.

Próximo passo após avaliação desta microentrega: T02.2, comparação controlada de duas câmeras, alterando somente câmera. T02.3 ainda deve comparar método de animação, provar ciclo/salto provisórios e medir orçamento. Não iniciar T03/T04/T05 nem aceitar T02 com base nesta imagem estática. A nova apresentação ainda não foi publicada; a build WebGL T00B contém a base anterior, com o foco de menu corrigido.
