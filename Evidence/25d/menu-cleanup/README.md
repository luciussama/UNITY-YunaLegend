# Limpeza do menu Unity — 13/11/2026

Sessão `2026-11-13-codex-menu-cleanup`. Solicitação explícita do usuário após
aprovar a build de acompanhamento.

O menu YunaLegend tinha19 atalhos, incluindo geração de modelos3D, reconstrução
de cena legada, calibração de luz/URP, geradores de assets, provas isoladas e builds
Web que ainda apontavam à Gameplay anterior. Retirados os19 registros MenuItem de
17 arquivos, mantendo todos os métodos para referências de código e reprodução
histórica. Nenhum asset, cena ou componente runtime foi removido.

Menu atual, confirmado na própria janela Unity por acessibilidade e screenshot:

1. Abrir cena de acompanhamento.
2. Gerar build Windows de acompanhamento.
3. Abrir pasta da build.

Os três usam `IllustratedPreviewBuild`. O comando interativo de build protege
alterações de cena pelo diálogo padrão de salvamento, restaura a configuração de
cenas ao terminar e não encerra o Editor. `Run` permanece como entrada batch com
encerramento automático; a rotina de exportação é compartilhada.

Validação: inventário estático confirma exatamente3 registros; alterações dos
17 arquivos anteriores limitadas à remoção de19 linhas. Compilação Unity6000.3.28f1
em cópia técnica, exit0, sem erros C#. Menu atualizado observado no Editor original,
sem abrir/salvar a cena Untitled do usuário ou executar geradores/build.
Não foi necessário recompilar o jogo para esta mudança exclusiva de Editor.
Log: `Build/25d/menu-cleanup-compile.log`.

A aprovação literal da build (“perfeito. Aprovado.”) foi registrada em
`Evidence/25d/T06-acompanhamento/reviews/acompanhamento-v6-aprovacao.md`, com hash
do manifesto entregue e escopo. Nenhum critério de aparelhos/expansão foi inferido.
