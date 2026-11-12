# T02.2b — câmera A com escala estável

Sessão `2026-11-12-codex-camera-stable-74ac`, Codex (GPT-6), 12/11/2026. Estado: implementada na cena de avaliação, aguardando revisão humana de conforto; não aceita T02 inteira.

`stable-camera.gif` contém 46 renders reais, a 10 fps, cobrindo 4,6 segundos de tempo simulado. A Yuna permanece na pose neutra aprovada; não há animação corporal nesta entrega. O cenário e a esfera antiga de Nanda continuam sendo a base técnica anterior, não a arte final 2,5D.

## Mudança isolada

A projeção A escolhida permanece perspectiva 45°, sem inclinação. O modo `illustratedPilotFraming` da `SideScroll25DCamera` fica desligado por padrão e foi ativado somente em `Assets/Scenes/Validation/IllustratedStableCamera.unity`. A cena Gameplay principal não foi salva/alterada. O Editor principal aberto pelo usuário foi preservado; o ensaio rodou na cópia `Build/Publish/Project`.

A distância em Z é constante: 9,0533 m, equivalente a 7,5 m de altura visível no plano z0. A Yuna de 1,4 m mede **134,3999–134,4001 px** a 720 px de altura, em vez da variação anterior de aproximadamente 64–162 px. Formatos 16:9 e 19,5:9 mantêm escala vertical, revelando mais largura no segundo formato; são renders, não cobertura de aparelho móvel.

A câmera acompanha X com SmoothDamp de 0,45 s e antecipa o atraso da câmera a partir da velocidade já existente. Não altera velocidade, gravidade ou posição física da criança. A altura usa a superfície real do apoio atual e a região de pouso seguinte, com transição suave, sem perseguir a altura do arco do salto. O primeiro ensaio de distância fixa, sem compensar o atraso horizontal, está preservado em `attempt01`.

## Leituras e limites

O percurso usa KeyboardState pelo Input System: dois saltos normais alcançam urso e livros. Não foi teletransportado o root nem desativada a progressão para produzir o GIF. O terceiro salto/travessia completa do móvel não faz parte desta captura.

O pouso seguinte ocupa x normalizado 0,563 antes do primeiro salto, 0,709 sobre o urso e 0,657 nos livros; a entrada do móvel está visível. No começo da corrida, o urso ainda fica fora da tela (x1,321), entrando no campo antes da decisão do salto. Não há garantia de mostrar todo o móvel longo a partir dos livros: a câmera antecipa a região real de entrada e acompanha o avanço, sem encolher o objeto.

`stable-camera.json` mede a separação entre sola/root e a superfície em z0: aproximadamente 0,08 m no urso e 0,0809 m nos livros enquanto grounded, compatível em ordem de grandeza com a skin 0,08 do CharacterController. Isso explica parte da impressão de flutuação exposta pela ortográfica. **Não foi corrigido deslocando personagem ou colisor.** A calibração visual de pés em movimento permanece requisito de integração/animação. O contato nominal 0 do bloco estático T02.1 não certifica todos os objetos.

Gameplay SHA antes/depois: `d507921b1244c2a3186ba820bc54a3eae8daf9208dcde677f7eed438c9e358ed`. Controlador físico SHA antes/depois: `d47aea951b8190c4a0557bb1521eaf4b37ccf57282b15e5daef07d94c0430d10`. Arte e parâmetros físicos preservados. Nenhuma publicação ou build nova. O modo ainda é prova do piloto; retorno, narrativa completa e composição retrato precisam de revisões próprias antes de expansão.

## Reprodução

Validação adicional: `camera-tests.xml`, **4/4 PlayMode tests passaram**. Três preservam os contratos da câmera anterior (panorama, salto e retorno/queda); o novo verifica modo opt-in desligado na Gameplay, escala fixa e ausência de perseguição do arco, inclusive ao mudar para 19,5:9. Não houve teste manual de aparelho ou navegador.

```
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.EditorScripts.IllustratedStableCameraCapture.Run -stableOutput <evidencias> -stableFrames <frames> -logFile <log>
```

Não passar `-quit`; o harness sai após Play Mode. Log da captura: `Build/25d-stable-camera-final.log`, marcador `ILLUSTRATED_STABLE_CAMERA_DONE success=True`. Frames brutos ignorados: `Build/25d/StableCameraFrames`. Os PNG 01–05 e variantes `-wide` estão nesta pasta. Captura da câmera exclui IMGUI. A exceção conhecida de indexação Editor.Search não impediu o ensaio; não confundir com erro de gameplay.

Revisar conforto, tamanho constante e antecedência do próximo apoio. Após aprovação desta microentrega, seguir T02.3 (prova de método de animação e custo), não produção completa da fase. O usuário também solicitou arquivar o material da direção anterior após concluir este trabalho; o arquivamento é uma tarefa de organização separada, sem equivaler a aceite visual.
