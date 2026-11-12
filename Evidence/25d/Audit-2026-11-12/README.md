# Auditoria das entregas recentes — 12/11/2026

Sessão `2026-11-12-codex-audit-commit-ce9d12c6`, Codex (GPT-6).
Identidade real do executor; não corresponde à identidade Copilot sugerida no pedido.
Reserva documental no diário 611, sem assumir T03 ou transferir o coordenador.

**Conclusão:** as entregas existem e estão registradas, mas a rastreabilidade da
T03.2 v2 tem uma inconsistência entre prova e verificação. Commit desta auditoria
**bloqueado**: não foi localizada liberação explícita do responsável por T03 para
esta integração; há também as divergências abaixo a encaminhar. Nenhum staging,
commit, push, publicação, implementação ou aprovação estética feito nesta sessão.

## Baseline e concorrência

- HEAD efetivo: `5b582ab1c544cb0e8590f2bb5359309634f0a8ca`, branch `codex/dark-room-2d`.
- O baseline preliminar `8e094f0` foi sucedido por `8cdc476` (328 caminhos:
  implementações, evidências, documentação, skills e solução) e `5b582ab`
  (registro do checkpoint/push no diário). Git estava limpo antes desta auditoria.
- Diário 604 já tem resultado 605; 606/607 registram aceite T03.2 v2;
  608/609 registram commit/push da sessão DeepSeek. Esses eventos não foram
  executados por esta auditoria. O push é informação do diário, sem nova consulta
  de rede ou julgamento sobre autorização em conversa de outro agente.
- T00/T00B/T01/T02 aceitas; T03 ativa, dona
  `2026-11-12-deepseek-run-tail-4b7e`, DeepSeek (deepseek-flash).
  T04–T10 pendentes. Nenhuma tarefa ativa usa o recurso `integration`.
- Checkpoint T03 existe e reconhece T03.1/T03.2 aprovadas com limites;
  não contém liberação explícita para o commit condicionado desta auditoria.
- Processos Unity observados: 17936, 18128, 30392 e 31900. Nenhum iniciado,
  encerrado ou controlado por esta sessão; não foi aberto outro Editor.

## Entregas confirmadas

| Entrega | Confronto com disco, diário e estado | Limite |
| --- | --- | --- |
| T02 | Contratos, auditoria e aceite estruturado presentes; auditoria recalculada 23/23; 24/24 hashes de evidências do estado conferem | Prova técnica, não animação final nem piloto |
| T03.1 | Driver visual, shader de piscada, cena isolada, arte e evidências presentes; manifesto 35/35; aprovação individual em `../T03.1/reviews/T03.1-v1-aprovacao.md`; diário 567/571 | Repouso isolado; não prova contatos em movimento |
| T03.2 v1 | Oito poses, driver SpriteRenderer, cena e evidências presentes; diagnóstico e ajuste registrados em 589/591/593/595 | Apoio numérico não eliminava o passo corporal; histórico preservado |
| T03.2 v2 | Captura parametrizada em 128 amostras, grade re-espaçada e GIFs presentes; resultados 605/607; aprovação individual registrada | Fechamento do clipe; passo corporal e dependência da taxa permanecem |
| Skills/documentação | `SKILLS.md`, três skills próprias, snapshot imagegen com licença e exportador presentes; diário 602/603; snapshot 12/12 hashes | Pacote de instruções; não instala ferramentas nem aprova arte |
| Solução Unity | `.slnx` adiciona `Assembly-CSharp.csproj` e `Assembly-CSharp-Editor.csproj`; ambos existem localmente; diário 589 lista `.slnx` entre afetados | Motivo específico das duas inclusões não está explicado no registro; não compilado nesta auditoria |

Não há alteração integrada entre `8e094f0..HEAD` em `Assets/Scripts`,
`Assets/Scenes`, `Packages` ou `ProjectSettings`. Os novos scripts/cenas estão
sob `Assets/Art/Illustrated25D/Yuna/Idle` e `Run`. O driver Run usa o controlador
3D existente como relógio, ancora um ponto da sola e desloca apenas o visual.
Não integra `PlatformSupportProfile` ao piloto: usa o chão técnico explícito.
A cena salva mantém `footPlant: 1` e velocidade 5,2 m/s.

## Divergências e encaminhamento

1. **Prova principal v2 de outra taxa (rastreabilidade, prioridade alta).**
   `../T03.2/review-v2/run-proof.json` declara `simulationHz: 60` e tem o mesmo SHA256
   de `final-v2-60/run-proof.json`:
   `91b0e265f3280ecd597e67691e85a2327814ae173d6a31b8c3c055bcfa687b1f`.
   Já `review-v2/verification.json` declara 120 Hz; seus 256 hashes conferem
   integralmente com `Build/25d/RunV2_120`. A prova correspondente disponível em
   `final-v2-120/run-proof.json` tem SHA256
   `f4a355dd654ea09968d907c4bbabfa75d17b1fda04c6221f4d1037f0d3887197`.
   Isso mistura a prova de 60 Hz com a verificação dos quadros de 120 Hz.
   Encaminhar ao responsável para registrar correção de proveniência e vincular
   a prova correta sem apagar o histórico. A existência da prova 120 Hz não
   corrige automaticamente o pacote entregue.
2. **Manifesto v1 não descreve o checkout v2 completo.** 55/58 hashes conferem;
   divergências: `Run/Editor/YunaRunReviewCapture.cs`, `Run/YunaRunReview.unity`
   e `T03.2/README.md`. A evolução é compatível com a v2 documentada, não prova
   corrupção da arte. Porém os bytes antigos desses três arquivos não são
   recuperáveis pelo commit-base v1 declarado (`8e094f0`, anterior à inclusão).
   Não alegar reprodução integral da v1 pelo manifesto. Falta manifesto v2 que
   una fontes/cena/provas/GIFs e a aprovação a hashes explícitos. O snapshot desta
   auditoria registra hashes atuais, sem certificar retroativamente os fontes
   usados nas capturas anteriores.
3. **Índices com estado antigo.** README T03.1 e `T03.2/review-v2/README.md`
   ainda dizem `em_revisao`; README T03.2 mantém trechos v1 em presente
   (“resultados atuais”, “aguarda revisão”, “nenhum commit”). O resumo canônico,
   checkpoint e diário registram aprovações posteriores. São divergências de
   navegação, não revogação desses registros de aprovação. O trecho “ajuste
   adiado” do resumo ainda aponta 595 sem reconciliar explicitamente 605/607.
4. **Link quebrado.** Em `T03.2/README.md`, `../review-v2/README.md` resolve fora
   de T03.2; destino pretendido existente: `review-v2/README.md`. Demais links
   locais no escopo verificado (SKILLS, skills próprias, resumo e Markdown
   T03.1/T03.2) existem. Não foi feita revisão global do arquivo histórico.
5. **Diagnósticos v1/v2 precisam de distinção.** A investigação v1 registra o
   salto de `bodyShift` de cerca de 0,22082 m na transição apoio→voo, repetido no
   fluxo. A página v2 atribui o “tranco no final” ao fechamento do clipe. A v2
   trata o fechamento; o driver ainda zera a compensação fora do apoio. Não
   declarar que a continuidade corporal inteira foi corrigida, nem usar a frase
   “não existe solução apenas de código” como teorema demonstrado pela auditoria.

Nenhum desses arquivos de outra sessão foi corrigido nesta tarefa. Os achados
não reclassificam a aprovação humana registrada e não aprovam novas entregas.

## Validações executadas agora

- `python -m unittest discover -s tests -v`: **14/14** aprovados, incluindo
  diário append-only, integridade, reservas, hashes editados e concorrência.
- `python tools/ai_diary.py check`: integrity `ok`, zero erros FK.
- `python tools/plan_25d.py check`: esquema/dependências/reservas/referências OK.
- Importação de `tools/audit_25d_t02.py` e chamada `audit()` sem executar seu
  gravador: **23/23**. Os **32 PlayMode** são resultados históricos lidos do XML,
  não testes Unity executados agora.
- Script [audit.py](audit.py): hashes de estado 24/24, T03.1 35/35,
  T03.2 v1 55/58, snapshot skills 12/12; GUIDs em `Assets/**/*.meta` sem duplicatas.
- Recalculo dos JSONs das capturas anteriores: 128 amostras em cada taxa;
  deriva de apoio 30/60/120 Hz = 0,000000954 / 0 / 0 m;
  maior passo corporal = 0,373335 / 0,286669 / 0,307487 m.
  Variante sem compensação a 60 Hz: deriva 0,242257 m, zero intervalos parados.
  SHA de Gameplay/controlador/atlas nas cinco provas confere com os arquivos atuais.
- **1.024/1.024** hashes dos quadros brutos conferem com as verificações v2
  (120/30/60 Hz e diagnóstico). Esses quadros estão em `Build/`, fora do Git.
- Leitura de metadados GIF com Pillow local: três GIFs principais com 128 quadros;
  normal/gameplay 4.270 ms, lento 8.540 ms. Não houve inspeção estética ou geração
  de GIF. Dados em [gif-metadata.json](gif-metadata.json).
- `python tools/package_project_skills.py --out Build/AuditExports/yuna-legend-skills-audit.zip`:
  17 fontes + manifesto, 55.962 bytes, CRC/SHA conferidos; inclui licença.
  Exportação separada, sem sobrescrever o ZIP anterior.
- `git diff --check` e `git diff --check 8e094f0..HEAD`: sem erros.

Dados detalhados, inventário dos 328 caminhos e hashes atuais:
[results.json](results.json). A consulta de planos nesse snapshot inclui o
plano 615 desta auditoria ainda em execução naquele instante; seu resultado é 616.

## Planos históricos sem resultado

Lista consultada, sem investigar encerramento, recuperar ou atribuir ações:
**72, 92, 97, 152, 164, 168, 178, 185, 534**.
604 saiu da lista preliminar por já ter resultado 605, registrado pelo dono.
Nenhuma pendência histórica foi encerrada por esta auditoria.

## Checkpoint documental e condição de integração

Feito: confronto de entregas/diário/estado, inventário, hashes, testes Python e
relatório com achados. Diário desta sessão: planos 610/613/615/617 e resultados
correspondentes. Aprovações humanas foram apenas lidas, sem nova autenticação
de conversas externas. Não houve teste Unity, captura, jogo, áudio, aparelho,
build WebGL ou avaliação estética nova.

Limites de produção preservados: contato final nas plataformas, orçamento de
atlas integrado (~36,45 MiB estimados juntos versus meta 32 MiB), salto,
tropeço, Nanda/ambiente, transições, retry, retrato/dispositivos e piloto/T07.
T03 continua ativa com seu responsável. Nenhuma reserva expira por tempo.

Próximo passo exato: responsável reconciliar prova/verificação v2 e registrar
checkpoint/liberação explícita para a integração pretendida; então o integrador
reconfere diário, reservas, HEAD e hashes e seleciona caminhos individualmente.
Esta auditoria não presume liberação a partir do commit/push anterior. Seus
arquivos e o banco permanecem locais, sem staging. A reserva documental desta
sessão será encerrada no diário, sem manter bloqueio sobre trabalho futuro.
