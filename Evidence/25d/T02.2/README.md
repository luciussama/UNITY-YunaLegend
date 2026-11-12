# T02.2a — projeção da câmera, em revisão

Sessão `2026-11-12-codex-camera-25d-b91f`, Codex (GPT-6), 12/11/2026.

**Estado: comparação implementada e verificada, aguardando escolha humana. T02 continua ativa.** A aprovação de T02.1 foi registrada em sua pasta reviews. Não aprovar T02 ou começar produção T03–T05 por causa desta captura.

## Material para revisão

`cameras-side-by-side.gif`: 46 pares de renders reais da Unity, 10 fps, 4,6 segundos de tempo simulado. Esquerda A: perspectiva suave, FOV 45°. Direita B: ortográfica. Cabeçalhos foram adicionados aos renders para identificá-los; nenhum frame recebeu pintura, correção de cor ou reconstrução. `02-bear-comparison.png` e `03-books-comparison.png` são pranchas de capturas reduzidas, também apenas com cabeçalhos.

Para ampliar sem redução: `02-bear-A-perspective.png`, `02-bear-B-orthographic.png`, `03-books-A-perspective.png`, `03-books-B-orthographic.png`. Os arquivos `-wide.png` têm formato 19,5:9, 1560×720, em vez de 16:9, 1280×720. São renders em outro formato, **não testes de celular**. O enquadramento vertical e a simulação são os mesmos; não representam todos os estados responsivos da câmera em aparelhos.

O cenário, luz e a esfera antiga de Nanda ainda são a base técnica anterior. Não são a nova arte ambiental nem a versão azul aprovada. Não recolorimos a esfera, nem integramos os brinquedos ilustrados, para não misturar mudanças ambientais com a comparação de câmera. A Yuna usa exatamente a pintura aprovada. Ela ainda está numa pose neutra, inclusive durante saltos; não julgar animação final por este material.

## Controle da comparação

`IllustratedCameraComparison.Run` abriu a Gameplay e salvou **outra cena**, `Assets/Scenes/Validation/IllustratedCameraComparison.unity`, trocando apenas o visual de Yuna para o sprite e sua compensação de orientação. A Gameplay fonte manteve SHA `d507921b1244c2a3186ba820bc54a3eae8daf9208dcde677f7eed438c9e358ed`.

Em Play Mode, entradas Space reais pelo Input System fizeram o percurso até o urso e os livros. Cada par foi renderizado antes da próxima atualização da simulação, com posição, rotação, iluminação, geometria e pose iguais. A única variável A/B é a projeção: `orthographicSize = abs(camera.z) * tan(22,5°)`. Isso iguala a escala exatamente no plano de corrida z0. O cenário com profundidade muda de aparência entre as projeções; a largura/altura da menina permanece igual entre A e B.

| Instante | Altura da menina A/B | Largura do retângulo do sprite A/B |
| --- | --- | --- |
| Entrada | 64,42 / 64,42 px | 44,90 / 44,90 px |
| Urso | 88,11 / 88,11 px | 61,42 / 61,42 px |
| Livros | 162,06 / 162,06 px | 112,97 / 112,97 px |

`camera-comparison.json` registra medições, apoio, posição física, câmera, limites horizontais projetados do próximo objeto e hashes. Os limites MinX/MaxX são apenas extensão global para diagnóstico, não criam apoios em lacunas entre regiões reais. A captura usa os apoios existentes, sem achatamento ou ajuste de tamanho dos objetos.

## Descoberta na integração

O root físico existente gira para ±90° durante a corrida, como orientação do antigo modelo 3D. No primeiro ensaio isso deixava o sprite de lado; a imagem estática T02.1 não havia exercitado esse caso. O ensaio falho visualmente está preservado em `attempt01`, mesmo que seu marcador de percurso tenha indicado sucesso. Não usar esse marcador isoladamente como validação de imagem.

`IllustratedCharacterFacing`, aplicado apenas à cena de comparação, compensa o yaw no filho Sprite_Painting e usa flipX para o sentido. O root e Body_Visual permanecem intactos. A fixture agora rejeita capturas com largura de sprite inferior a 40% da altura física projetada. Testes geométricos verificam os dois sentidos sem mudar o controlador. Isso não é animação final nem validação de tropeço.

## Diagnóstico e recomendação provisória

A perspectiva mantém visíveis os tampos dos objetos tridimensionais atuais. A ortográfica aproxima a leitura de um palco ilustrado, mas torna a capa dos livros e o contato dos pés menos claros nesta geometria. Há uma separação visual perceptível entre sola e capa na vista ortográfica: medir contato real/pivô/skin/superfície antes da integração final, sem deslocar o root nem alterar o colisor para maquiar a imagem. O contato estático no bloco da T02.1 não certifica todos os objetos em movimento.

**Preferência técnica provisória: A, pela leitura dos apoios atuais.** A decisão humana ainda está aberta; a geometria e arte finais devem ser reavaliadas na projeção escolhida. Nenhuma dessas imagens aprova a aparência do ambiente antigo para a nova versão.

As duas variantes preservam a mesma política de acompanhamento e zoom herdada. O crescimento de 64 para 162 px evidencia uma variação excessiva para avaliar conforto final. O próximo passo de câmera deve ser **T02.2b: estabilizar a escala/zoom e revisar uma travessia na projeção escolhida**, sem mudar física ou animação. Não avançar para T02.3 antes dessa revisão. O móvel seguinte também ultrapassa o limite direito no estado dos livros; investigar antecipação e enquadramento usando suas regiões de apoio, em vez de encolher o móvel.

## Reprodução e limites

```
Unity.exe -batchmode -projectPath <copia-isolada> -executeMethod YunaLegend.EditorScripts.IllustratedCameraComparison.Run -comparisonOutput <evidencias> -comparisonFrames <frames> -logFile <log>
```

O harness encerra a Unity ao terminar; não passar `-quit`. A cena salva contém a câmera original em perspectiva; a alternância A/B é feita pelo harness. Frames brutos: `Build/25d/CameraFrames/A` e `/B`. Log: `Build/25d-camera-comparison-facing.log`, marcador `ILLUSTRATED_CAMERA_COMPARISON_DONE success=True`. A exceção conhecida de inicialização do indexador Editor.Search permanece registrada; não impediu os renders. Capturas da câmera não incluem HUD IMGUI. Não houve build/publicação nova nem teste manual de navegador/dispositivo nesta microentrega.
