# T05.1 - urso: apoio real e camera final

Estado: **T05.1 aprovada pelo usuario em 13/11/2026**. A T05 agregada continua
ativa: livros, movel, movimento e T06 permanecem pendentes.

Data: 13/11/2026. Sessao `2026-11-13-codex-t05-urso-6d2e`, Codex (GPT-6.1).
Escopo: primeiro objeto do percurso obrigatorio **urso -> livros -> movel**.
Livros e movel nao foram iniciados nesta microentrega.

## Para revisar

- [Camera final, com Yuna sobre a coroa](../T05.1/01-gameplay-bear-contact.png)
- [Detalhe limpo do urso integrado](../T05.1/02-bear-detail.png)
- [Proposta e perguntas de revisao](reviews/T05.1-urso-proposta.md)
- [Relatorio de contato e enquadramento](contact-report.json)

## Resultado

A prancha aprovada em T01.3 foi integrada sem nova geracao de arte nem alteracao
da imagem-fonte. O urso usa a coroa curva entre orelhas como apoio principal, e
o topo fisico acompanha o perfil superior medido da pintura. A Yuna aparece na
camera final com a raiz `0,03 m` acima da coroa; o fundo do
`CharacterController` termina em `-7,97 m` para uma coroa em `y=-8,00 m`.

O detalhe preserva pelucia, costuras, remendo, laco e silhueta arredondada. Nao
foi introduzido tampo reto ou superficie visivel que substitua a forma pintada.

## Contrato de apoio

O contrato foi medido da fonte aprovada antes da integracao:

- Fonte: `Evidence/25d/T01/bear-v1.png`, SHA-256
  `708108204dda6a64294eb19c0e2567129e0d4a092926d78c47ebd0192f9ff833`.
- Alpha: bbox inclusivo `(88, 86)..(1526, 913)`, textura `1619 x 971`.
- Coroa central na textura: `(800, 129)`; piso em `y=913`.
- Escala fixada pela altura da coroa: `0,001403061 m/px`.
- Largura visivel: `2,017602 m`; altura visivel: `1,160332 m`.
- Altura da coroa acima do piso: `1,10 m`; orelhas chegam a `1,160332 m`.
- Offset horizontal do contato: `-0,009821 m`.
- Profundidade fisica: `1,20 m`; plano visual em `z=0`.
- Perfil superior: 360 amostras; apoio dividido em 7 regioes e 656 segmentos.
- Física: um unico `MeshCollider` nao convexo extrudado, com 2872 vertices.

No mundo, a coroa fica em `(2,012, -8,000, 0)`. O erro maximo entre o perfil e
o `MeshCollider` e `0,000001 m`; o raycast central na coroa tem erro `0`.
Raycasts em `z=+/-0,55 m` atingem o apoio e, em `z=+/-0,62 m`, nao encontram
apoio. O vao urso -> livros e `2,490840 m`, abaixo do alcance fisico validado de
`2,491969 m`.

## Integracao

- Marcador da cena: `index0` em `Assets/Scenes/DarkRoom_Gameplay.unity`.
- Textura: `Bear_Platform_v1.png`; material: `Bear_Platform_Painted.mat`.
- Malha visual: `Bear_Platform_Visual.asset`; contato:
  `Bear_Platform_Contact.asset`.
- Componente: `IllustratedBearPlatform`.
- Perfil de engenharia:
  `Assets/Art/Illustrated25D/Environment/Bear/Resources/Bear_Platform_Profile.json`.
- O antigo `Visual_3D` foi preservado como
  `Visual_3D_Referencia_Desativada` e permanece inativo.
- `DarkRoomPlatformGeometry` e `DarkRoomPlatformArt` usam o perfil ilustrado
  quando ele esta disponivel, mantendo fallback legado.
- O gerador do circuito deslocou o contato inicial em `0,012 m` para preservar
  o vao do salto dentro do alcance fisico.

## Validacao executada

| Prova | Resultado |
| --- | --- |
| Setup idempotente no marcador 0 | exit `0`; `T05_BEAR_SETUP_SAVED width=2.017602 depth=1.2 crownY=-8` |
| EditMode | `51/51` testes passaram; [`validation/editmode-51.xml`](validation/editmode-51.xml) |
| Harness da camera final | exit `0`; apoio maximo `0,000001 m`, zero apoios fora da profundidade, coroa atingida |
| Camera final | 1600 x 900; Yuna alinhada sobre a coroa; livro seguinte visivel a direita |
| Detalhe | 1280 x 720; pintura e silhueta preservadas |

Logs reais: [`validation/bear-setup.log`](validation/bear-setup.log) e
[`validation/bear-review.log`](validation/bear-review.log).

## Reproducao

Executar a partir da raiz do repositorio, com o Unity Editor fechado:

```powershell
$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe'
$Project = (Get-Location).Path

Start-Process -Wait -FilePath $Unity -ArgumentList @(
  '-batchmode', '-quit', '-projectPath', $Project,
  '-executeMethod', 'YunaLegend.EditorScripts.BearPlatformSetup.Run',
  '-logFile', 'Build\25d\t05-bear-setup.log'
)

Start-Process -Wait -FilePath $Unity -ArgumentList @(
  '-batchmode', '-quit', '-projectPath', $Project,
  '-executeMethod', 'YunaLegend.EditorScripts.BearPlatformReviewCapture.Run',
  '-yunaT05Output', (Join-Path $Project 'Evidence\25d\T05.1'),
  '-logFile', 'Build\25d\t05-bear-review.log'
)

Start-Process -Wait -FilePath $Unity -ArgumentList @(
  '-batchmode', '-quit', '-projectPath', $Project,
  '-runTests', '-testPlatform', 'EditMode',
  '-testResults', (Join-Path $Project 'Evidence\25d\T05.1\validation\editmode-51.xml'),
  '-logFile', 'Build\25d\t05-editmode.log'
)

python tools\make_t05_bear_manifest.py
```

O setup salva a cena; o harness de revisao nao salva a cena. O harness usa a
camera `SideScroll25DCamera` real da cena persistida, raycasts no `MeshCollider`
e gera o relatorio e as duas capturas.

## Limites

- Esta entrega valida apenas contato estatico, contrato fisico e enquadramento
  na camera final.
- Nao aprova movimento, pouso, sensacao de salto, entrada movel, desempenho em
  aparelhos ou WebGL.
- Nao aprova a T05 agregada, livros, movel, T06 ou o piloto em movimento.
- A Yuna no quadro e o visual ainda presente na cena principal; a integracao
  T03 pertence a T06.

## Autoria e preservacao

Arte-fonte: prancha `bear-v1.png` aprovada pelo usuario na T01.3, sem nova
chamada de geracao nesta entrega. A medicao, a malha de contato, a integracao e
o harness foram produzidos por Codex (GPT-6.1) nesta sessao. Nenhuma fonte
aprovada de Yuna, Nanda, livros ou movel foi alterada.

