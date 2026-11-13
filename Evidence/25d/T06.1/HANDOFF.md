# Handoff — T06 (piloto em movimento), 13/11/2026

Sessão que encerra: `2026-11-13-dsh-pilot-7b3e` (DeepSeek).
Tarefa liberada no coordenador para outra IA continuar. Leia este arquivo antes de
mexer em cena, build ou evidências.

## Estado das tarefas

| Tarefa | Estado | Observação |
| --- | --- | --- |
| T05 | **aceita** | Consolidação em [Evidence/25d/T05/README.md](../T05/README.md); seis aprovações individuais |
| T06 | **liberada** (sem dono) | T06.1 em revisão humana; T06.2/T06.3 pendentes |
| T07 | pendente | Depende de T06 inteira e aprovação explícita do piloto |

T06.1 está **em revisão**: [proposta](reviews/T06.1-proposta.md) e [README](README.md).
Não houve resposta humana; ausência de resposta não é aprovação.

## O que já existe

1. **Harness da travessia** `Assets/Art/Illustrated25D/Yuna/Joint/Editor/YunaT061TraversalCapture.cs`:
   abre a `DarkRoom_Gameplay`, monta Idle/Run/Jump/Stumble aprovados + resolvedor
   sobre o controlador real, dirige chão → urso → livros por `InputSystem` e
   captura na câmera final. Não salva a cena. Aceita `-t061Hz 30|60|120` e
   `-t061Output`.
2. **Instalador runtime** `Assets/Art/Illustrated25D/Yuna/Joint/Runtime/YunaIllustrationInstaller.cs`
   (setupe por `YunaIllustrationInstallerSetup.Run`): ficou **salvo na cena**
   `DarkRoom_Gameplay`, esconde só os renderers do corpo 3D e monta o visual
   ilustrado. O harness reaproveita esse instalador quando ele existe.
3. **Build Windows** `Assets/Scripts/Editor/WindowsPilotBuild.cs`, saída em
   `Build/Windows/YunaLegend-Pilot/YunaLegend.exe` (183 MB, x64). A pasta `Build/`
   é ignorada pelo Git: quem continuar precisa recompilar.
4. **Evidências**: `capture-30/60/120` (harness isolado), `capture-60-integrated`
   (caminho do instalador persistido), GIFs e `summary.json` em `review/`.

## Resultados medidos

- 60 Hz e 120 Hz: travessia completa, 2 saltos, 1 fonte visível, 0 conflitos,
  SHA da cena e do controlador intactos.
- 30 Hz: falha real — o salto do urso parte em `x=2,96`, passa por cima da pilha
  e cai em `x≈5,97`. Decidir se corrige a temporização do salto nesta microentrega.
- Build: smoke test abre a tela de entrada; após corrigir o rótulo de integridade
  da pasta (ver abaixo), o input chega ao `EventSystem`.

## Pendências em ordem

1. **Revisão humana da T06.1** (`reviews/T06.1-proposta.md`): arco do salto,
   frenagem no pouso e o que fazer com o 30 Hz.
2. **Composição visual**: instalar a T05.4 (panorama, penumbra, sombras) e o voo
   ilustrado da T04.1 na captura e na build. Hoje o quarto aparece com o visual
   3D legado, escuro.
3. **30 Hz**: se for corrigir, o ponto é a temporização/altura do salto do urso
   (o harness já usa durações em segundos, não em quadros).
4. **T06.2**: travessia do móvel com cedimento, pausa local de 80 ms, estralo e
   tropeço — exige runtime real, não a fixture estática da T05.3.
5. **T06.3**: falhar e tentar de novo, com reset e retry sem eventos duplicados.
6. **T07**: build + vídeo do piloto contínuo, teclado e toque, com aprovação
   humana explícita antes de expandir a fase.

## Comandos de retomada

```powershell
$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe'
$root = (Get-Location).Path

# 1. Reinstalar/atualizar o instalador na cena (salva a cena)
& $Unity -batchmode -quit -projectPath $root `
  -executeMethod YunaLegend.YunaIllustration.Editor.YunaIllustrationInstallerSetup.Run `
  -logFile 'Build\25d\t061-installer.log'

# 2. Capturar a travessia (não salva a cena)
& $Unity -batchmode -projectPath $root `
  -executeMethod YunaLegend.YunaIllustration.Editor.YunaT061TraversalCapture.Run `
  -t061Hz 60 -t061Output "$root\Evidence\25d\T06.1\capture-60" `
  -logFile 'Build\25d\t061-60.log'

# 3. Recompilar a build Windows de validação
& $Unity -batchmode -projectPath $root `
  -executeMethod YunaLegend.EditorScripts.WindowsPilotBuild.Run `
  -logFile 'Build\25d\pilot-build.log'

# 4. Pacote de revisão (GIFs + resumo)
python tools\make_t061_review.py
```

## Armadilhas já encontradas (não repetir)

- **Rótulo de integridade**: a pasta `Build/` herdou `Nível Obrigatório Baixo` do
  sandbox; o player abre, mas **não recebe mouse/teclado**. Corrigir com
  `icacls 'Build\Windows\YunaLegend-Pilot' /setintegritylevel Medium /T` e
  `icacls 'Build' /setintegritylevel Medium`.
- **Churn do URP no build**: cada compilação marca `active: 1` em MotionBlur,
  LensDistortion, DepthOfField e FilmGrain do `DefaultVolumeProfile` e normaliza
  `DarkRoomPipeline`. Valores em zero; reverter e recompilar não adianta.
- **Harness × instalador**: o harness detecta o resolvedor e **não reinstala**.
  Se a cena for limpa, ele volta a instalar sozinho.
- **Cena modificada**: `DarkRoom_Gameplay.unity` está salva com o instalador;
  qualquer re-captura antiga (T05.x) deve ser conferida antes de rodar.
- **Não usar o corpo 3D como visual**: só os renderers são ocultados; os
  transforms continuam ativos porque o evento do móvel os procura.

## Onde está o histórico

Diário `docs/ia/diario.sqlite3`, entradas `883`–`892`, sessão
`2026-11-13-dsh-pilot-7b3e`. Estado resumido em
[ESTADO-ATUAL-25D.md](../../../docs/ia/ESTADO-ATUAL-25D.md).

## Atualização v3 (13/11/2026, sessão `2026-11-13-dsh-pilot-t061v2-8a1f`)

- `RoomCompositionRuntimeInstaller` instala em runtime a composição **aprovada**
  completa: panorama v4, sombras de contato, **móvel ilustrado** (T05.3,
  `FurnitureIllustrationCandidate`), **Nanda ilustrada** (T04.1,
  `NandaFlightIllustration`) e **penumbra** (luar, luz azul de Nanda,
  fosforescência). Salvo na cena `DarkRoom_Gameplay`.
- Captura [capture-v3-60](capture-v3-60) e revisão [T06.1-v3](reviews/T06.1-v3.md);
  build regravada (193 MB, 0 erros).
- A v2 fica marcada como **superada**: ela desligou a penumbra por engano. O tom
  azulado da Yuna no aprovado é intencional; sem a penumbra o quarto fica claro e
  o móvel 3D legado aparece como silhueta preta.
- Pendente: T06.2 (travessia do móvel com quebra/tropeço) e T06.3 (retry).
