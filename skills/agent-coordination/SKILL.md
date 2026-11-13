---
name: agent-coordination
description: Projetar, validar ou operar o registro idempotente de assuntos dos agentes neste repositorio. Usar para reservas, concorrencia, retomada e alteracoes no CLI de coordenacao; preservar trabalho paralelo em assuntos independentes.
---

# Engenharia de coordenação de agentes

Resolver a raiz pelo `AGENTS.md` e usar o banco central `docs/ia/diario.sqlite3`.
Ler a seção 0 de `AGENTS.md` e [o contrato do registro](../../docs/ia/REGISTRO-AGENTES.md).
Não confundir exclusão por assunto com exclusão global: assuntos independentes
devem poder executar simultaneamente.

Antes de atuar, consultar reservas abertas, assuntos encerrados relevantes,
planos pendentes do diário e coordenador. Escolher chave semântica estável e
recursos mínimos. Reutilizar ID/chave/sessão na retomada; uma nova formulação do
pedido não justifica outra reserva. Exigir aquisição bem-sucedida antes de editar.

Ao alterar a implementação, preservar estes invariantes:

- Consulta + decisão + aquisição sob transação; nunca usar consulta seguida de
  gravação sem trava. Uma consulta de status é somente um retrato.
- Chave única permanente, repetição sem novo evento, transições ordenadas,
  proprietário validado e histórico append-only.
- Queda não libera reserva comprometida; ausência de heartbeat não autoriza posse.
- Reservas do coordenador antigo e do novo registro respeitam-se mutuamente.
- Exclusão por recurso cobre pasta/descendente, sem reservar o projeto inteiro.
- Falhas devem abortar sem escrita parcial; testes usam bancos descartáveis.

Testar disputa real entre processos pelo mesmo assunto, execução simultânea de
assuntos distintos, repetição, sessão indevida, transições inválidas, liberação,
imutabilidade e interoperabilidade com o coordenador. Executar também testes
existentes do diário/coordenador quando seus contratos forem afetados.

Registrar no diário o plano e seu resultado antes do próximo passo. Entregar
evidências observadas e limites: a semântica do assunto depende dos agentes e
cópias independentes do banco não coordenam entre si. Não prometer execução
exatamente uma vez para efeitos externos apenas com idempotência do registro.
