# T03.1 v1 — repouso, respiração e piscada

Estado: **aprovada** em 12/11/2026 ([revisão individual](reviews/T03.1-v1-aprovacao.md)).
12/11/2026, sessão `2026-11-12-codex-close-t02-c73a`,
Codex (GPT-6). T02 foi aceita como prova técnica/contratos; T03 segue ativa.

## Entrega

- [Loop ampliado](idle-detail.gif), 640×640, quatro segundos.
- [Loop na escala de gameplay](idle-gameplay.gif), 1280×720; Yuna neutra com
  altura derivada de 134,4 px, mesma projeção A 45° e distância 9,0533 m aprovada.
- PNGs `game-000/060/069` e `detail-000/060/069`: aberto, inspiração e piscada.

São renders reais da Unity 6000.3.28f1 em Play Mode, de uma cena estacionária
isolada. 120 amostras determinísticas a 30 Hz do mesmo avaliador que o componente
usa em LateUpdate. O controlador está desabilitado **apenas nesta fixture** para
separar o repouso visual de movimento/física. O apoio retangular é um suporte
técnico herdado de T02.1, não desenho de plataforma da fase.

Respiração de quatro segundos alarga suavemente o peito (até 1,8%), com inclinação
de tronco até 0,4° e contramovimento de cabeça até 0,5°. Pernas, sapatos e escala
global não respiram. Uma piscada de 180 ms fecha em 40 ms, mantém 80 ms e abre em
60 ms; na amostragem a 30 Hz há dois quadros inteiramente fechados. O ciclo repete
sem salto de pose no início/fim. Não há animação articulada de corrida nesta entrega.

## Arte e preservação da identidade

Original aprovado `Yuna_Profile_v1.png` intacto. A ferramenta nativa **imagegen**
gerou uma única variante de olhos fechados, com o original como alvo de edição.
Prompt integral em `prompt-blink-v1.txt`; saída bruta preservada como
`yuna-blink-generated-v1.png` e copiada em `Assets/.../Idle/Yuna_Blink_v1.png`.
Não é desenho manual nem conteúdo retirado do legado; não foram usados terceiros.

Para evitar mudanças de pincelada entre quadros, o shader usa a nova pintura
somente no retângulo do olho (x643–739/y351–468, coordenadas da imagem fonte com
origem superior esquerda), com borda suave de 10 px. Fora dele, RGB e alpha vêm
sempre do original. A respiração deforma a malha visual aprovada. O arquivo original
não foi recortado, recolorido ou sobrescrito; não houve edição artística por Python.
As GIFs apenas quantizam os renders para uma paleta fixa, sem recorte/redimensão,
com cadência 30/40 ms e duração total exata de 4000 ms.

## Verificações reais e limites

`idle-proof.json`: success=true, 120 quadros, root/rotação/escala e dimensões/centro
do colisor inalterados, pose inicial/final periódica, Gameplay com SHA idêntico
antes/depois. Textura importada como Texture2D 1024×1536, sRGB, alpha, clamp,
bilinear, sem mipmaps, NPOT None, Read/Write desligado. Duas texturas contabilizadas
em **24,0096 MiB no editor**; não é medição de GPU/dispositivo.

`preview-verification.json`: nos 120 PNGs ampliados, a região dos sapatos/solas
x260–384/y540–599 teve **zero pixels diferentes** do primeiro quadro. Isso comprova
estabilidade estacionária, não contato nos apoios reais. O mínimo de vértices da
malha +0,0130 m não é medição de sola opaca; a calibração do contato na travessia
continua pendente. A personagem em outros sentidos e os saltos ainda precisam de
revisão própria. Sem nova build, publicação, teste de aparelho ou aprovação do piloto.

Os 32 testes PlayMode da T02 são evidência anterior, não testes novos desta entrega.
Os novos contratos foram exercitados pelo harness de captura. Fontes, cena/material,
metas e evidências têm hashes em `manifest-v1.json`. Nenhum teste amplo foi repetido
para esta camada visual isolada após a captura final passar.

## Reprodução

Abrir `Assets/Art/Illustrated25D/Yuna/Idle/YunaIdleReview.unity` e entrar em Play
Mode para ver o repouso contínuo. A cena não está na lista de publicação.
Para captura automatizada, usar cópia isolada sem outro Editor aberto nesse projeto:

```text
Unity.exe -batchmode -projectPath <copia> -executeMethod YunaLegend.YunaIllustration.Editor.YunaIdleReviewCapture.Run -idleOutput <evidencias> -idleFrames <frames> -logFile <log>
```

Sem `-quit`: o harness entra/sai de Play Mode e encerra. Marcador final:
`YUNA_IDLE_REVIEW_DONE success=True`. Execução final: `Build/25d-t031-idle5.log`.
GIFs: `python tools/make_idle_review.py` (Pillow já instalado em
`Build/25d/PythonDeps`; esse diretório é dependência local ignorada).

Limitações de execução preservadas: primeira tentativa falhou por inicializador
de MaterialPropertyBlock, segunda por textura ausente, terceira identificou importação
como Cubemap. Corrigidos inicialização lazy e shape Texture2D explícito. A quarta
captura passou, mas revelou NPOT automático (28,0096 MiB); a quinta preserva o canvas
e usa 24,0096 MiB. Relatórios intermediários estão preservados. A exceção conhecida
do indexador Editor.Search aparece no log final, sem impedir os renders; não é
um log livre de exceções.

Avaliação humana em [reviews/T03.1-v1.md](reviews/T03.1-v1.md). Só após essa revisão
avançar para T03.2 (corrida). T03 inteira ainda não está concluída.
