# Investigação do tranco no final — T03.2 v1

12/11/2026, sessão `2026-11-12-codex-run-tail-e42b`, Codex (GPT-6).
Pedido atual: investigar o tranco percebido no final do clipe. **Defeito confirmado;
esta investigação não altera nem declara corrigida a animação.**

## Causa principal: a compensação de apoio salta ao liberar o pé

Na última passada completa registrada, amostras **115→116**, poses **2→3**,
aproximadamente **3,833→3,867 segundos**, `bodyShift` passa diretamente de
−0,2208176 m para zero. Na câmera de gameplay isso equivale a **21,20 px a 720p**;
na ampliação, aproximadamente 78,5 px. O método limpa a âncora na pose de voo e
atribui `bodyShift = correction`, que começa em zero, sem transição de liberação.

O controlador continua avançando normalmente cerca de 0,17333 m por imagem.
Somado ao reset visual de +0,22082 m, o pivô visual avança cerca de **0,39415 m
em 33,3 ms** (11,82 m/s). Na amostra anterior, com a mesma pose de apoio,
o pivô visual ficava quase imóvel no mundo. A mudança imobilidade→avanço rápido
produz o tranco. A velocidade física segue 5,2 m/s, e seus fontes não mudaram.

O mesmo reset aparece a cada 12 imagens na sequência, incluindo a última passada.
Portanto o problema está presente no ciclo corporal, além do fim do arquivo.
O teste anterior verificava que o marco do pé não escorregava; não verificava a
continuidade do pivô do corpo e podia passar mesmo com esse defeito.

## Questão adicional: fechamento do GIF e do cenário

O GIF tem 120 quadros e duração correta de 4000 ms, sem pausa extra no último.
O fechamento passa da pose 5 para 6, uma transição normal da sequência; ele não
contém um quadro de repouso nem uma ordem de parar a corrida.

O deslocamento de um período completo é 20,8 m, enquanto as marcas do chão se
repetem a cada metro. A volta do GIF reinicia o cenário com diferença de fase de
0,8 m módulo 1 m (equivalente mínimo de 0,2 m na grade repetida). Isso acrescenta
uma descontinuidade do fundo na repetição. Não explica sozinho o reset corporal
115→116, que já ocorre dentro do clipe.

A diferença RGB média no fechamento não é maior que as transições internas dos
renders brutos. Essa média não certifica fluidez, mas não sustenta culpar apenas
a exportação do GIF por um quadro final grande ou uma pausa de codificação.

## Encaminhamento técnico

T03.2 permanece **ajustar**, sem aceite humano e sem iniciar salto T03.3.
A correção deve tratar continuidade corporal na passagem apoio→voo e voo→apoio,
além de produzir evidência com fechamento coerente do cenário ou clipe sem repetição.
Não apenas cortar os últimos quadros: o salto corporal se repete dentro do ciclo.

Uma simples suavização do corpo também pode reintroduzir deslizamento do pé.
Na próxima candidata, medir **apoio e continuidade do corpo juntos**, distinguir
passo discreto da pintura de deslocamento artificial instantâneo e revisar o vídeo.
Preservar pintura, física, velocidade e os registros atuais. Não deformar novamente
as botas para obter uma métrica de apoio verde; essa experiência já foi rejeitada.

## Reprodução

`python tools/investigate_run_tail.py` escreve `diagnosis.json`, com amostras,
saltos, tempos, linha/hash da fonte e análise de fechamento. Dados originais em
`../review-v1/run-proof-120.json`, GIFs e `Build/25d/RunFinal120/`.
Relatório numérico/tempos usa apenas arquivos existentes; nenhuma captura Unity
ou teste novo de gameplay foi executado nesta investigação. O manifesto v1 com
58 hashes continua válido; os artefatos anteriores não foram reescritos.
