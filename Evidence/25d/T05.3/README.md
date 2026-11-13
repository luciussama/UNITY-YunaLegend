# T05.3 — móvel ilustrado, candidata v1 para captura serial

**Capturada estaticamente na Unity6000.3.28f1, não aprovada.** Sessão `2026-11-13-copilot-t053-b29a5ec7`. Contrato de apoio escrito antes da derivação em [CONTRATO.md](CONTRATO.md). Fonte aprovada e hashes dos recortes em [manifest.json](manifest.json). O PNG de preview é só inspeção da fonte em fundo escuro, não render do jogo.

## Entrega

- `Assets\Art\Illustrated25D\Environment\Furniture\FurnitureIllustrationCandidate.cs`: estrutura pintada separada, dois tampos mesh3D com fronteira irregular complementar em profundidade, fibras sem suporte. Sincroniza cada visual com a transformação do contato legado. Não substitui evento/pausa/retry nem modifica controlador3D/perfil/câmera.
- `Editor\FurnitureT053ReviewBatch.cs`: fixture temporária no marcador2 da cena carregada, captura intacto/quebrado/reset e audit de ausência de ponte na abertura, pose visual=contato e reset. Desativa somente renderers legados em memória; conserva transforms ativos para `YieldingWoodFurniture`. Não salva Gameplay.
- `tools\t053_prepare_furniture.ps1`: recortes determinísticos da prancha aprovada, nenhuma geração nova. RGB preservado; alpha251–255 do interior do patch de madeira normalizado a255 com rejeição se alpha<240. Corpo mantém alpha original. Não houve instalação.

## Comandos exatos para o integrador

Fechar/conferir Editor do checkout antes da chamada; não abrir segundo Editor. Unity é6000.3.28f1. Executar na raiz:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\t053_prepare_furniture.ps1
& 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\sergi\repos\UNITY-YunaLegend' -executeMethod YunaLegend.Illustrated25D.Furniture.Editor.FurnitureT053ReviewBatch.Run -logFile 'Evidence\25d\T05.3\unity-editmode.log'
```

O caminho padrão do Hub deve ser confirmado pelo integrador. Não passar `-nographics` (captura renderiza); `Run` encerra com0/1, não requer `-quit`. Verificar `T053_EDITMODE_INSPECTION_PASS`, quatro PNGs e `editmode-audit.json`. Frequência de simulação não se aplica a esta inspeção EditMode. Os arquivos novos não requerem patch no gerador compartilhado para executar a fixture.

## Integração posterior, sem executar nesta entrega

No ponto onde marcador2 já foi criado pelo gerador, após seus contatos e `YieldingWoodFurniture`, instalar componente com as duas texturas via `candidate.Install(bodyTexture,woodTexture)`. Fazer isto somente no piloto/harness aprovado; não copiar arte à fase inteira. O adaptador espera dimensões canônicas e rejeita outras. Não desativar `Visual_3D` inteiro: o evento legado procura os transforms filhos para funcionar. Ocultar renderers basta. O perfil existente lê as novas MeshColliders; caixas legadas são desabilitadas, não deletadas.

A fixture estática NÃO valida toda a geometria de contato sob CharacterController. A câmera de inspeção usa45° mas não é a câmera final acompanhando Yuna: integração/captura em câmera final ainda obrigatória. Fonte ilustrada frontal permanece recortada em escala uniforme; o topo usa UVs próprios sobre malha física, sem pintura única esticada para ceder. A parte baixa da prancha (detalhe quebrado) não é importada como segundo móvel. O vão irregular no plano do corredor aparece só ao romper; repouso toca as bordas complementares. Alteração visual de fibras é apenas candidata técnica, sujeita a revisão.

## Revisão pendente e critérios

Após render, inspecionar em escala de gameplay e detalhe: reconhece o mesmo móvel doméstico/latão/pintura T01.5? A fadiga central parece localizada, não corte reto? O topo e bordas móveis coincidem com apoio antes/depois? Ajustar se necessário. Nenhuma resposta humana registrada ainda.

Travessia real, estralo audível, duração LOCAL80ms, recuperação280ms, evento único e retry30/60/120Hz exigem execução posterior T06.2/3, com Yuna ilustrada oficial; não declarados realizados. T05.4 e aceites não iniciados. Observação da sessão irmã: há callback antigo de `NandaFlightReviewCapture` sem guarda de SessionState que pode interferir em PlayMode; este harness não entra PlayMode e não altera a fonte aprovada.

## Resultado real da captura serial —13/11/2026

- `unity-editmode-v3.log`: `T053_EDITMODE_INSPECTION_PASS`; sem erro de compilação/Exception. Editor encerrado.
- `editmode-audit.json`: íntegro1região; aberto2regiões; sem apoio no centro da abertura; pose visual=contato; sag0,075m. `ResetForRetry()` chamado diretamente restaurou flags/contador/sag e apoio; isto NÃO prova retry via fluxo real do jogo.
- Tentativa inicial preservada em `unity-editmode.log`: rejeitada porque interpretação das dimensões da referência inverteu altura/profundidade. Corrigidas somente candidata e contrato para a cena real8,4largura/2,4profundidade/2,3altura. V2/v3 passaram. Não confiar apenas no retorno do launcher Powershell: o primeiro retornou0 apesar de exceção. Conferir sentinela/audit.
- Fontes e PNGs finais listados com SHA256 em `review-hashes.json`. Fonte aprovada permaneceu byte-idêntica.

### Pacote para abrir

1. `01-intact-gameplay-scale.png` e `02-broken-gameplay-scale.png`: poses limpas,1280×720,45° na câmera de inspeção (não travessia/câmera final acompanhando).
2. `03-broken-detail.png` e `04-reset-detail.png`: detalhes limpos cedido/reset.
3. `05-intact-support-overlay.png` e `06-broken-support-overlay.png`: versões TÉCNICAS com segmentos reais do perfil no planoZ0 projetados em ciano. Não são linhas pintadas na arte; não usar os overlays no jogo.
4. `review-contact-sheet.png`: montagem reduzida480×270 por quadro com rótulos para comparação, sem retoque do render. Originais acima preservados.

### Inspeção e questões humanas

Imagens reais de repouso/detalhe/reset e folha comparativa foram abertas. O móvel preserva gavetas laterais/centro aberto/puxadores da pintura fonte; tampo ocupa uma malha própria que se separa localmente no centro. A abertura é pequena na escala de gameplay (5cm de spread somado à rotação) e pede julgamento no detalhe; o patch de topo repete/perspectiva de UV e transição corpo→tampo são candidatos técnicos, não acabamento aprovado. Overlay representa superfície no plano do corredor, não toda a largura visível em profundidade. Não há Yuna nesta captura; sem prova de sola/apoio.

A prova runtime adicional NÃO foi iniciada: a candidata visual e sua superfície diferente das caixas legadas ainda não receberam revisão humana. Executar o controlador sobre essa nova superfície seria uma fixture técnica, jamais aceite do piloto; a sequência real exigida continua pendente. Para futura prova isolada sem salvar cena, o ponto de partida já existente é `DarkRoomFurnitureCapture` (controlador real, jump buffer, evento YieldingWoodFurniture), mas ele mostra visual3D antigo e captura objetos do piloto; não foi reutilizado para fingir resultado desta candidata. Necessário novo harness exclusivo que instale candidata + visual T03 aprovado, observe evento único80ms local/áudio/reação/recuperação, e exerça morte/retry e segunda carga30/60/120Hz. T06.2/3 mantêm a prova integrada posterior. Nenhum sistema compartilhado foi alterado nesta sessão.
