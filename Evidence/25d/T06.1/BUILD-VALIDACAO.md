# Build de validação jogável — 13/11/2026

`Build/Windows/YunaLegend-Pilot/YunaLegend.exe` (183 MB, Windows x64, Unity
6000.3.28f1). Gerada pela sessão `2026-11-13-dsh-pilot-7b3e` para o usuário
testar a jogabilidade. **Não é a T07**: é uma build de validação, sem aceite.

## Como abrir

- Duplo clique em `YunaLegend.exe`, ou para forçar janela 1280×720:

```powershell
& '.\Build\Windows\YunaLegend-Pilot\YunaLegend.exe' -screen-width 1280 -screen-height 720 -screen-fullscreen 0
```

## Controles

| Ação | Teclas |
| --- | --- |
| Avançar menu / confirmar | `Espaço`, `Enter`, `↑`, `W` ou clique esquerdo |
| Pular | `Espaço`, `↑`, `W` ou clique esquerdo (segurar = salto mais alto) |
| Pausa | `P` ou `Esc` |

## O que a build contém

- Yuna ilustrada aprovada na T03 (idle/respiro, corrida, salto, tropeço) montada
  sobre o controlador 3D real por `YunaIllustrationInstaller`, salvo na cena.
- Urso (T05.1) e livros (T05.2) ilustrados com seus colisores medidos.
- Física, câmera `SideScroll25DCamera`, menu e progressão preservados.

## O que ela ainda não tem

- Composição da T05.4 (panorama pintado, penumbra, sombras de contato): o quarto
  aparece com o visual 3D legado, escuro.
- Móvel ilustrado da T05.3 (segue o móvel 3D legado) e Nanda com o voo da T04.1.
- UI nova, retry validado em runtime, áudio novo, WebGL e aparelhos reais.

## Como foi gerada

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe' -batchmode -quit `
  -projectPath (Get-Location).Path `
  -executeMethod YunaLegend.YunaIllustration.Editor.YunaIllustrationInstallerSetup.Run `
  -logFile 'Build\25d\t061-installer.log'

& 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe' -batchmode `
  -projectPath (Get-Location).Path `
  -executeMethod YunaLegend.EditorScripts.WindowsPilotBuild.Run `
  -logFile 'Build\25d\pilot-build.log'
```

O setup salva a cena `DarkRoom_Gameplay` com o componente `YunaIllustrationInstaller`.
O harness T06.1 passou a reaproveitar esse instalador persistido: a validação
`capture-60-integrated` confirma a mesma travessia (2 saltos, 1 fonte visível,
0 conflitos, SHA da cena/controlador intactos) pelo caminho usado na build.

## Efeito colateral observado no build

Cada compilação faz o URP marcar como `active: 1` alguns componentes do
`Assets/DefaultVolumeProfile.asset` (MotionBlur, LensDistortion, DepthOfField,
FilmGrain) e normaliza `Assets/Settings/DarkRoomPipeline.asset`. Reverti os dois
arquivos e recompilei: o efeito voltou, então é comportamento do pipeline, não da
entrega. Os valores desses componentes continuam em zero, portanto não há mudança
visual esperada; o ponto fica registrado para quem for commitar a build.


## Correção: janela abria mas não recebia cliques (13/11/2026)

O primeiro teste do usuário abriu a tela de entrada, mas nenhum clique ou atalho
funcionava. Causa: a pasta `Build/` herdou do sandbox o rótulo de integridade
**Baixo** (`Nível Obrigatório Baixo`), e o log do player registrava
`Process is running in low integrity mode...`. O Windows não entrega mouse/teclado
a processo de integridade baixa, então o `EventSystem` nunca recebia o clique.

Correção aplicada na pasta da build e na raiz `Build/`:

```powershell
icacls 'Build\Windows\YunaLegend-Pilot' /setintegritylevel Medium /T
icacls 'Build' /setintegritylevel Medium
```

Depois disso o log deixou de registrar o modo de integridade baixa e o player
iniciou com `<RI> Input initialized.`. Builds futuras herdam o rótulo Médio da
raiz `Build/`; se o sintoma voltar, repetir o comando acima.
