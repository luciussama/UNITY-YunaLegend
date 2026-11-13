# T06.1 — travessia urso → livros em movimento (candidata)

Data: 13/11/2026. Sessão `2026-11-13-dsh-pilot-7b3e` (DeepSeek).
Estado: **candidata para revisão humana**. Nada aqui foi aprovado, e a cena
`DarkRoom_Gameplay` **não foi salva**: o visual ilustrado existe só durante a
execução do harness.

Escopo desta microentrega: uma travessia real do urso (`index0`) até os livros
(`index1`) com a Yuna ilustrada aprovada na T03, câmera final e física preservada.
O móvel (T06.2) e o retry (T06.3) não foram tocados.

## O que foi executado

- Harness novo [YunaT061TraversalCapture.cs](../../../Assets/Art/Illustrated25D/Yuna/Joint/Editor/YunaT061TraversalCapture.cs):
  abre a `DarkRoom_Gameplay`, monta Idle/Run/Jump/Stumble aprovados + o resolvedor
  de visibilidade sobre o `Yuna25DPlayerController` real, oculta os renderers do
  corpo 3D (`Body_Visual`) e dirige a travessia com eventos reais de `InputSystem`.
- Os dois saltos são reais: chão → urso e urso → livros, com o controlador
  físico 3D, câmera `SideScroll25DCamera` e plataformas dos T05.1/T05.2.
- Captura em 30, 60 e 120 Hz, com PNGs de momentos-chave, GIFs e
  `traversal-report.json` por taxa.

## Resultados medidos

| Taxa | Travessia | Saltos | Fonte visível | Conflitos | Física | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| 30 Hz | urso ✓, livros ✓ | 2/2 | máx. 1 | 0 | intacta | **falhou**: o salto do urso parte em `x=2,96` e passa por cima da pilha, caindo em `x≈5,97` |
| 60 Hz | urso ✓, livros ✓ | 2/2 | máx. 1 | 0 | intacta | **passou** |
| 120 Hz | urso ✓, livros ✓ | 2/2 | máx. 1 | 0 | intacta | **passou** |

- Folga de contato medida no urso: `0,082–0,168 m`; nos livros: `0,084 m`
  (compatível com a `skinWidth` de 0,08 do `CharacterController`).
- `DarkRoom_Gameplay` e `Yuna25DPlayerController.cs`: SHA-256 idênticos antes e
  depois nas três execuções; `CharacterController` (centro/altura/raio) e escala
  do root inalterados.
- Resolvedor: no máximo uma fonte visível por quadro e zero conflitos em todas
  as taxas (ordem Stumble > Jump > Run > Idle preservada).

## Pacote para revisão

1. [GIF 60 Hz](review/traversal-60.gif) e [GIF 120 Hz](review/traversal-120.gif):
   corrida, salto do urso, pouso nos livros e frenagem.
2. [GIF 30 Hz](review/traversal-30.gif): mostra a falha descrita acima.
3. Momentos-chave em `capture-60/stills/`: `02-bear-contact`, `03-bear-takeoff`,
   `04-rise`, `05-fall`, `06-books-land`, `07-books-contact`, `09-books-stop`.
4. `capture-*/traversal-report.json` e `review/summary.json` com as amostras.

## Limites desta candidata

- A composição da T05.4 (panorama pintado, penumbra e sombras de contato) **não**
  está instalada nesta captura: o fundo é o quarto 3D legado, ainda escuro.
  Instalar a composição aprovada é o próximo passo natural da T06.1.
- Nanda aparece como o guia existente, sem o voo ilustrado da T04.1.
- Sem móvel, sem cedimento/tropeço, sem morte/retry, sem UI e sem aparelhos.
- O 30 Hz não conclui a travessia com esta temporização de entrada. É uma
  limitação real da candidata, não um artefato escondido: o JSON registra a
  posição de decolagem e o ponto de queda.

## Reprodução

```powershell
$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe'
& $Unity -batchmode -projectPath (Get-Location).Path `
  -executeMethod YunaLegend.YunaIllustration.Editor.YunaT061TraversalCapture.Run `
  -t061Hz 60 -t061Output (Join-Path (Get-Location) 'Evidence\25d\T06.1\capture-60') `
  -logFile 'Build\25d\t061-60.log'
python tools\make_t061_review.py
```

A pasta `frames/` é intermediária e foi descartada após gerar os GIFs; o harness
a recria. O `-t061Hz` aceita 30, 60 ou 120.

## Perguntas para a revisão humana

1. A leitura do salto do urso para os livros convence em movimento?
2. A frenagem ao pousar parece natural ou abrupta?
3. O 30 Hz precisa ser corrigido nesta microentrega (temporização do salto) ou
   pode ser tratado junto com a composição da T05.4 na próxima iteração?
