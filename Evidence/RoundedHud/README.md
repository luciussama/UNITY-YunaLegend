# HUDs arredondados — candidata para revisão humana

Pedido de 25/11/2026: avaliar e corrigir a apresentação do menu e dos HUDs de todo
o projeto, com cantos arredondados, controles grandes, boa leitura e alcance mobile.
Não adicionar nem remover funções.

## Resultado

- [Galeria dos cinco HUDs](hud-gallery.png).
- [Menu, configurações e seleção de fases](menu-gallery.png).
- [Comparação antes/depois](before-after.png).
- [Pausas e cartões narrativos](review-portrait-cards.png).
- [HUDs em paisagem](review-landscape-cards.png).
- `after/`: 41 PNGs completos renderizados na Unity; `before/`: 27 enquadramentos anteriores.
- `review-v2/`: candidata anterior com os problemas de 320 px identificados e corrigidos.

O tema compartilhado `YunaLegend.Presentation/HudTheme` usa sprites arredondados
de nove fatias, mantendo os cantos proporcionais ao tamanho dos painéis.
Cards de pergaminho, texto ameixa e botões com feedback de foco/pressão substituem
os blocos escuros de cantos retos. Os retratos, ícones e controles circulares
existentes foram preservados. Cinema, cortinas e sobreposições de escurecimento
continuam com sua função original.

| Tela | Correção principal |
| --- | --- |
| Menu inicial | Botões e painéis arredondados; slider com alça de 44 px; arte abaixo do menu nas telas estreitas; status com fundo contrastante. |
| Quarto Escuro | Cartões claros arredondados, texto maior e utilitários de 48 px. |
| Brinquedos | Contador e narrativa claros; controles maiores; GUARDAR em uma linha; layout desktop restaurado após rotação. |
| Arena | Cards ameixa arredondados; utilitários separados do status; ESPECIAL inteiro; modais claros. |
| Perseguição ao Urso | Texto funcional legível; modais claros; três ações empilhadas no celular; utilitários maiores. |
| Piquenique | Controles inferiores de 64 px; botões de diálogo de 52 px; safe area; layout de retrato/paisagem e troca correta entre canvases ativos. |

## Verificações

- Unity **6000.3.28f1**, compilação/capturas com saída **0**.
- **41 enquadramentos** em 320×568, 390×844, 844×390 e 1280×720.
- **2/2 testes EditMode** existentes de layout responsivo e preservação de foco do menu passaram.
- Harness do Piquenique confirmou os cinco eventos de ação existentes, pulo,
  direções ao manter pressionado e liberação do toque.
- `callback-audit.json` compara os listeners de clique com o HEAD anterior:
  mesmos callbacks nos seis arquivos de apresentação.
- `control-bounds.tsv` e `mobile-target-audit.json`: alvos mobile medidos nos
  estados adicionais com pelo menos 44 px e dentro da tela, com tolerância de arredondamento de 1 px.
- Os fontes usados na cópia de validação foram conferidos contra o checkout;
  hashes em `validation-manifest.json`.

Foram corrigidos durante a revisão: ESPECIAL e GUARDAR cortados, sobreposição do
status da Arena em 320 px, alça do volume esticada, modais do Urso estreitos e
botões da cinemática do Piquenique que encolhiam após rotação.

As imagens são capturas estáticas reais da Unity, em cópia isolada
`Build/PicnicVisualAuditProject`. As galerias contêm apenas composição e rótulos,
sem retoques nos HUDs. A verificação não representa um aparelho físico, um novo
percurso completo em Play Mode ou uma exportação WebGL. Não houve alteração de
Core, física, regras, dificuldade, progresso ou destinos do menu nesta entrega.

## Reprodução e estado

`YunaLegend.EditorScripts.RoundedHudReviewCapture.CaptureAll`, executado em batch
na cópia isolada, gera a matriz em `Logs/mobile_ui_audit`. O utilitário existente
`MobileUiCapture` fornece os enquadramentos originais; o novo harness acrescenta
Urso, Piquenique, desktop e celular estreito, além das medidas e verificações de eventos.

Para ver os HUDs atualizados no Editor do usuário, sair de Play e iniciar novamente.
O Editor aberto foi preservado; nenhum processo próprio de validação foi deixado ativo.

Branch **`fix/picnic-editor-playmode`**, sem commit/push nesta entrega.
Registro raiz **187**, revisão explícita do trabalho anterior **115**; registros
independentes de UI/UX por fase no diário central. Os arquivos anteriores de
grama e os demais trabalhos locais foram preservados.

**Estado visual: candidata em revisão.** Nenhuma aprovação estética humana foi presumida.
