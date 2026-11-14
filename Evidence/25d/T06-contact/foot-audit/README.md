# Auditoria dos pés de Yuna nas superfícies — 13/11/2026

Registro 8, sessão `2026-11-13-codex-contact-penumbra`.
**Diagnóstico confirmado; nenhuma correção de animação aplicada.**

O usuário apontou afastamento da sola no móvel na [imagem marcada](user-reference.png).
A análise confirma afastamento também no chão, nos livros e no urso do piloto.
É uma questão da integração da personagem com os apoios, adicional ao alinhamento
do fundo/friso tratado pela v4. Não encerrar T06/T07 com o resultado funcional verde.

## Resultado observado

| Superfície / estado | Separação vertical observada |
| --- | --- |
| Chão inicial, repouso | 7,95 cm; corrida com pose de apoio 7,87–8,13 cm |
| Móvel, repouso final | 7,91 cm, aproximadamente 7,59 px em 1280×720 |
| Móvel, pouso/recuperação | 8,15–8,21 cm |
| Móvel, corrida com pose de apoio | 7,35–8,60 cm |
| Livros, pouso | 6,86–7,78 cm |
| Urso, pouso/recuperação | 5,28–8,56 cm no ponto inferior da sola amostrado |
| Urso, corrida | 9,25–21,31 cm nesse ponto; a curva requer análise da envoltória completa de apoio para corrigir |

No tropeço, a maioria das poses fica próxima do apoio (aproximadamente −5,5 mm
a +0,1 mm), mas a pose 1 chegou a +8,11 cm. Uma compensação uniforme aplicada
também ao tropeço causaria penetração nas poses que já encostam.

O valor do urso **não é** a menor distância de toda a sola curva, nem prova que
os dois pés estejam afastados por 21 cm: é o ponto mais baixo opaco da pintura
comparado ao apoio diretamente sob esse ponto. As capturas confirmam a leitura
de afastamento; a correção precisa avaliar o pé de apoio e sua região inteira.

## Causa encontrada

O `CharacterController.skinWidth` é **0,08 m**. Na prática, o root fica cerca de
8 cm acima do apoio. A pintura foi registrada junto ao root, sem compensar isso
na apresentação Idle. Run e Jump possuem compensação vertical, mas ela depende
de `reviewSupport`, preenchido nas provas isoladas e vazio no instalador real.
O tropeço já tem fallback para raycast de cena, explicando seu comportamento
diferente. A sondagem pelo root também não equivale ao apoio sob a sola numa
curva, rampa ou abertura do móvel.

Os [192 arquivos de personagem/controlador conferidos](baseline-check.json)
continuam idênticos aos listados na entrega T06.3. A folga já vinha dessa
integração; as mudanças recentes do panorama não alteraram as animações.

A correção indicada é ajustar **somente a apresentação** ao apoio real do pé
ativo, incluindo Idle, Run e pouso, respeitando curva/inclinação, a pose de
cedimento e a transição do tropeço. Preservar a cápsula, a margem de colisão,
`PlatformSupportProfile`, os vãos e a física. Não baixar a personagem toda por
uma constante, nem mover o tampo para esconder o problema.

## Evidência e reprodução

Instrumentação somente opt-in: `PreviewFootContactAudit.cs`, instalada por
`-previewFootAudit <pasta>`. Mede a textura original: pixel inferior vermelho do
sapato com alfa ≥128, transformado pelo sprite ou por interpolação UV dos
triângulos deformados do Idle. Raycast no collider real sob a sola, separado
da âncora de autoria e do root. Valores positivos significam afastamento.

- [Amostras completas](run-60/foot-contact.json): **1.316 frames grounded**.
- [Resumo](summary.json), gerado por `python summarize.py`.
- **33 capturas do executável** em `run-60/`, sem retoque; o nome identifica frame,
  índice da plataforma, fonte visível e pose. [Detalhe do móvel em repouso](furniture-idle-detail.png).
- Móvel: [frame Idle](run-60/frame-1349_2_Idle_0.png).
- Urso: [frame Run](run-60/frame-271_0_Run_1.png).
- Livros: [frame Land](run-60/frame-327_1_Jump_6.png).
- [Teste de travessia/retry](retry-60/verification.json): cinco tentativas,
  success=true, zero erros. Isso confirma execução funcional, **não contato visual correto**.

Unity 6000.3.28f1, exportador oficial na cópia técnica, 60 Hz e janela em primeiro
plano. Argumentos: `-previewRetryVerify <pasta-retry> -previewFootAudit <pasta-audit>
-previewHz 60 -screen-width 1280 -screen-height 720 -windowed`.
Build diagnóstica apenas na cópia `UNITY-YunaLegend-preview-20261113`;
a build normal central v4 e a T06.3 aprovada foram preservadas.
Fontes, executável e provas vinculados em [manifest.json](manifest.json).

Cobertura: chão, urso, livros e móvel ativos do piloto; pouso, corrida e repouso
onde ocorreram naturalmente, cedimento e recuperação. O trecho dos livros só
ofereceu frames de pouso, sem certificação de corrida/repouso prolongado ali.
Frames de voo das poses Run 3/7 foram excluídos do resumo de contato.
Sem teste adicional 30/120 Hz, aparelho, WebGL, áudio ou desempenho.
Editor do usuário e player normal preservados; nenhum commit/push.
