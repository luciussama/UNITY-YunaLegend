# T05.2 - livros: apoio real e camera final

Estado: **T05.2 aprovada pelo usuario em 13/11/2026** (resposta literal
`"aprovado."`). A T05 agregada continua ativa: T05.3 movel, movimento e T06
permanecem pendentes.

Data: 13/11/2026. Sessao `2026-11-13-codex-t052-books-b7c4`, Codex (GPT-6.1).
Escopo: segundo objeto do percurso obrigatorio **urso -> livros -> movel**.
O movel nao foi iniciado nesta microentrega.

## Para revisar

- [Camera final, com Yuna sobre a capa superior](01-gameplay-books-contact.png)
- [Detalhe limpo da pilha ilustrada](02-books-detail.png)
- [Close do contato pes/capa](03-books-contact-close.png)
- [Proposta e perguntas de revisao](reviews/T05.2-livros-proposta.md)
- [Contrato fisico medido](CONTRATO.md)
- [Relatorio de contato e enquadramento](contact-report.json)

## Resultado

A prancha aprovada em T01.4 foi integrada sem nova geracao de arte nem alteracao
da imagem-fonte. A Yuna pousa na capa rigida do livro superior, inclinada
`-8,739244 graus` e deslocada `+0,134786 m` em relacao ao centro da pintura.
O ponto central de pouso fica em `y=-7,55 m`; a raiz da Yuna foi posicionada
`0,03 m` acima dele. O fundo do `CharacterController` termina em `-7,52 m`.

Nas capturas, a Yuna e a **pintura ilustrada oficial** (Idle aprovado em T01,
SHA-256 `0b9315b4dd525f23f2ef14db04682846b43b3ca7793f47670129afc88dfb9044`),
orientada para a direita. O harness oculta o `Body_Visual` 3D descartado e cria
um `SpriteRenderer` temporario apenas durante a revisao; a cena nao e salva.
A cena persistida ainda contem o corpo 3D ate a integracao permanente da T06.

O detalhe preserva os cinco volumes, capas, lombadas, paginas, ornamentos e a
fita lateral. Nao foi introduzido tampo reto, caixa envolvendo a pilha ou apoio
invisivel alem da profundidade fisica de `1,15 m`.

O aceite cobre exclusivamente a microentrega T05.2 dos livros. Ele nao aprova a
T05 agregada, o movel/T05.3, movimento, pouso, T04.2, T06 ou WebGL.

## Contrato de apoio

- Fonte: `Evidence/25d/T01/books-v1.png`, SHA-256
  `cdbf8f5b1fa6f8cc3dd92372d142d26a72325005f4e84a97d49bd14fd83b0bec`.
- Alpha bbox inclusivo: `(83, 51)..(1444, 926)`, textura `1620 x 971`.
- Escala: `0,001268571 m/px`; largura visivel `1,726526 m`; altura `1,11 m`.
- Capa superior: largura `1,184211 m`, inclinacao `-8,739244 graus`.
- Offset do contato: `+0,134786 m`; altura acima do centro: `0,363763 m`.
- Profundidade fisica: `1,15 m`; plano visual em `z=0`.
- Fisica: um unico `MeshCollider` nao convexo; 8 vertices; 1 regiao e 2 segmentos.

O topo visual mais alto da imagem pertence a lombada traseira, nao ao plano de
pouso. O contrato detalhado esta em [`CONTRATO.md`](CONTRATO.md).

## Integracao

- Marcador da cena: `index1` em `Assets/Scenes/DarkRoom_Gameplay.unity`.
- Textura: `Books_Platform_v1.png`; material: `Books_Platform_Painted.mat`.
- Malha visual: `Books_Platform_Visual.asset`; contato:
  `Books_Platform_Contact.asset`.
- Componente: `IllustratedBookStackPlatform`.
- Perfil de engenharia:
  `Assets/Art/Illustrated25D/Environment/Books/Resources/Books_Platform_Profile.json`.
- O antigo `Visual_3D` foi preservado como
  `Visual_3D_Referencia_Desativada` e permanece inativo.
- `DarkRoomPlatformGeometry` e `DarkRoomPlatformArt` usam o perfil ilustrado
  quando ele esta disponivel, mantendo fallback legado.
- O contato autorado dos livros passou para `Stage1ContactX[1] = 4,070 m` para
  preservar vaos dentro do alcance fisico.

## Validacao executada

| Prova | Resultado |
| --- | --- |
| Setup idempotente no marcador 1 | exit `0`; `T05_BOOKS_SETUP_SAVED width=1.726526 height=1.11 support=1.184211 tilt=-8.739244 offsetX=0.134786 depth=1.15 collider=MeshCollider` |
| EditMode | `52/52` testes passaram; [`validation/editmode-52.xml`](validation/editmode-52.xml) |
| Harness da camera final | exit `0`; erro de apoio `0`, zero apoios fora da profundidade, topo atingido |
| Camera final | 1600 x 900; Yuna com o sprite ilustrado oficial sobre a capa; urso e proxima plataforma visiveis |
| Detalhe | 1280 x 720; pintura e cinco volumes preservados |
| Close de contato | 1280 x 720; pes alinhados a capa inclinada |
| Visual de revisao | `Body_Visual` 3D inativo; sprite oficial ativo, hash T01 conferido, virado para a direita, erro de pes `0,000000 m` |

Logs reais: [`validation/books-setup.log`](validation/books-setup.log) e
[`validation/books-review.log`](validation/books-review.log).

## Reproducao

Executar a partir da raiz do repositorio, com o Unity Editor fechado:

```powershell
$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe'
$Project = (Get-Location).Path

python tools\measure_t05_books_platform.py

Start-Process -Wait -FilePath $Unity -ArgumentList @(
  '-batchmode', '-quit', '-projectPath', $Project,
  '-executeMethod', 'YunaLegend.EditorScripts.BooksPlatformSetup.Run',
  '-logFile', 'Build\25d\t05-books-setup.log'
)

Start-Process -Wait -FilePath $Unity -ArgumentList @(
  '-batchmode', '-quit', '-projectPath', $Project,
  '-executeMethod', 'YunaLegend.EditorScripts.BooksPlatformReviewCapture.Run',
  '-yunaT05BooksOutput', (Join-Path $Project 'Evidence\25d\T05.2'),
  '-logFile', 'Build\25d\t05-books-review.log'
)

Start-Process -FilePath $Unity -ArgumentList @(
  '-batchmode', '-projectPath', $Project,
  '-runTests', '-testPlatform', 'EditMode',
  '-testResults', (Join-Path $Project 'Evidence\25d\T05.2\validation\editmode-52.xml'),
  '-logFile', 'Build\25d\t05-editmode.log'
) -Wait

python tools\make_t05_books_manifest.py
```

O setup salva a cena; o harness de revisao nao salva a cena. O harness usa a
camera `SideScroll25DCamera` real da cena persistida, raycasts no `MeshCollider`
e gera o relatorio e as tres capturas.

## Limites

- Esta entrega valida apenas contato estatico, contrato fisico e enquadramento
  na camera final.
- Nao aprova movimento, pouso, sensacao de salto, entrada movel, desempenho em
  aparelhos ou WebGL.
- Nao aprova a T05 agregada, o movel, T06 ou o piloto em movimento.
- A Yuna das capturas usa o sprite ilustrado oficial apenas no harness; a
  integracao permanente T03 (run/jump/stumble) pertence a T06 e a cena
  persistida ainda contem o corpo 3D.

## Autoria e preservacao

Arte-fonte: prancha `books-v1.png` aprovada pelo usuario na T01.4, sem nova
chamada de geracao nesta entrega. A medicao, a malha de contato, a integracao e
o harness foram produzidos por Codex (GPT-6.1) nesta sessao. Nenhuma fonte
aprovada de Yuna, Nanda, urso ou movel foi alterada.
