# T04.2 — candidata de acompanhamento da Nanda

Sessão `2026-11-13-copilot-t042-b29a5ec7`, Copilot, 13/11/2026.
**Candidata capturada na Unity a 30/60/120Hz; ainda não aceita por humano.**
As três provas finais têm `success=true`, 150 frames por câmera, fontes protegidas intactas e reset da trilha confirmado. Não fecha T04 nem libera T06.

## Entrega concreta

- `Assets/Art/Illustrated25D/Nanda/Accompaniment/NandaAccompaniment.cs`: apresentação sobre `NandaFairyGuide` existente. Não calcula destino nem move root/Yuna. O guia conserva antecipação, separação e SmoothDamp.
- Atlas, oito poses, ciclo 0,8s, hover e orientação T04.1 preservados. O rosto continua exatamente a pintura aprovada; expressão adicional é **gestual**, inclinação máxima 4° para Happy e 2,4° para Annoyed/Facepalm. Não há nova expressão facial desenhada nem recoloração da Yuna.
- Luz pontual azul `(0,32;0,58;1)`, intensidade nominal 0,65, pulsação 6%, alegria +8%, faixa 2,2m, sem sombras. Trilha azul curta 0,32s, largura 0,028m, alpha máximo 0,24, sem partículas/halo opaco. Fonte, rosto e asas devem continuar protagonistas.
- Reset explícito limpa trilha/relógio/pose; disable limpa trilha. Nenhum asset/material aprovado é regravado. O único material novo é runtime e pertence à candidata.
- Harness abre fixture aceita T03 **sem salvar**, monta a Nanda em memória com guia real e controlador real da Yuna: repouso 0–1s, corrida direita 1–5s, volta esquerda 5–9s, repouso 9–10s. Normal/Happy/Annoyed/Normal exercitam gesto e luz. Duas câmeras de prova, 150 frames a 15fps, simulação 30/60/120Hz; não equivale à câmera final ou travessia de plataformas.

## Reprodução serial (não executar Editors simultâneos)

Guarda mínima **aplicada com autorização explícita** em `NandaFlightReviewCapture.cs`: primeira linha do callback é `if (!SessionState.GetBool(Key, false)) return;`. A ausência foi observada por leitura; a interferência era risco inferido, não falha reproduzida. Idle/Run/Jump/Stumble/Joint e o novo harness possuem guardas. Nenhum Editor do projeto estava ativo antes das execuções; PID32980 era apenas UnityHub `serve`. Cada execução foi serial e aguardada com limite de600s, sem `-quit`; o harness encerra PlayMode e Editor com status0/1.

Unity 6000.3.28f1, sem `-quit`, renderização disponível (não usar `-nographics`). Substitua caminho do executável pelo Editor instalado:

```powershell
& '<Unity6000.3.28f1>\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\sergi\repos\UNITY-YunaLegend' -executeMethod YunaLegend.NandaIllustration.Editor.NandaAccompanimentCapture.Run -t042Hz 60 -t042Output Evidence/25d/T04.2/capture-60 -logFile Evidence/25d/T04.2/capture-60.log
# Repetir serialmente com 30 e 120, saídas/logs capture-30 e capture-120.
& .\.venv\Scripts\python.exe tools\t042_review.py --capture Evidence/25d/T04.2/capture-60
```

Empacotador usa Pillow. Nesta execução faltava `PIL` no venv; após o erro demonstrado, instalado Pillow13.0.0 com `python -m pip install Pillow`, sem alterar manifests de dependências. Gera GIF normal 10s (67/67/66ms), lento 15s, folhas e manifesto. Recusa relatório falho ou contagem diferente de150. PNGs são renderizações Unity sem retoque; GIF é redução/quantização para revisão, não fonte de medição.

## Critérios e limites a validar

O JSON falha se fonte protegida mudar, azul/orientação divergirem, intensidade >0,8, alcance >2,5m, reset deixar trilha, direções/humores faltarem ou separação dianteira <1,35m após transição. Registra posições, pose, flip, gesto, luz, quantidade de vértices da trilha e hashes de antes/depois. A reversão permite 1s de transição do guia legado; inspecionar cruzamento/oclusão visualmente, não confundir tolerância temporal com aprovação.

## Resultado observado em 13/11/2026

Unity6000.3.28f1/D3D12/RTX5070 compilou e capturou todas as taxas com exit0: 30Hz separação mínima1,41642m; 60Hz1,40752m; 120Hz1,39745m. Intensidade máxima0,741, alcance2,2m; azul, flip, humores e reset passam em todas. [Prova60](capture-60/accompaniment-proof.json), [30](capture-30/accompaniment-proof.json), [120](capture-120/accompaniment-proof.json). [Gameplay](review/game-normal.gif), [detalhe](review/detail-normal.gif) e folhas em `review/`.

Falhas reais preservadas: `attempt-01-lag` com smoothTime legado0,35 produziu atraso/separação0,764m; `attempt-02-*` com0,02 passou30/60 mas falhou120 por2mm (1,347916m). Configuração final **0,01s apenas na instância do harness**; não relaxou a meta1,35m nem alterou `NandaFairyGuide.cs`. A integração posterior deve levar essa configuração e rever reversão; o guia legado não passa automaticamente sem ela.

Inspeção visual das folhas gameplay/detalhe: identidades pintadas preservadas, Nanda legível e menor que Yuna, acompanhando à frente nos dois lados; fundo técnico cinza e marcadores permanecem. Efeito azul muito discreto nas amostras; não há evidência visual convincente de luz iluminando superfície. Folhas estáticas não certificam fluidez da reversão, pulso ou gesto, mesmo com GIFs empacotados. Rever clipes normal/lento humanamente, especialmente5s.

Ainda por comprovar: visibilidade satisfatória da trilha no pipeline real, efeito da luz sobre materiais (sprites padrão podem ser unlit), foco da composição, antecipação, leitura do gesto, memória/CPU/GPU/dispositivos e câmera de produção. Intensidade/faixa numéricas não comprovam luz bonita nem iluminação visível no cenário. Harness não fabrica superfície iluminada para disfarçar limitação do material. Nenhuma integração na Gameplay, prefabs compartilhados ou ambiente foi feita; essa integração pertence ao piloto após revisão.

Perguntas humanas após captura real: Nanda acompanha sem disputar foco com Yuna? Azul/trilha são discretos? A troca de lado é confortável? Gesto expressa alegria/contrariedade preservando rosto/asas? Registrar resposta literal e escopo; não presumir aceite agregado.


## Aceite humano ? 13/11/2026

Resposta literal: `voo da nada aprovado. Ficou muito bom.`

Escopo: candidata de acompanhamento em voo apresentada nesta conversa (GIFs/folhas de `review/`). N?o inclui T04 agregada, integra??o na Gameplay nem ilumina??o efetiva sobre superf?cies.
