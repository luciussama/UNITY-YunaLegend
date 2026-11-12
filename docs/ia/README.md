# Diário compartilhado de IA

**Direção vigente:** personagens 2D ilustrados em cenário com profundidade na Unity. Antes de atuar na migração, leia [PLANO-25D-OBRIGATORIO.md](PLANO-25D-OBRIGATORIO.md) e consulte `python tools\plan_25d.py status`. Recomendações antigas de refinamento 3D final foram substituídas; os registros históricos permanecem válidos como evidência do que ocorreu.

> **Regra obrigatória do projeto:** o formato da plataforma manda no colisor — nunca o contrário. Leia o bloco “REGRA OBRIGATÓRIA — formato das plataformas” no [AGENTS.md](../../AGENTS.md) e a entrada **305** do diário antes de modelar qualquer objeto jogável.

O arquivo [diario.sqlite3](diario.sqlite3) é o histórico persistente de desenvolvimento do projeto, independente de fornecedor de IA. O [protocolo da raiz](../../AGENTS.md) orienta os agentes; o banco pode ser aberto por qualquer cliente SQLite. Não é um sistema de saves do jogo nem uma dependência de execução da Unity.

## Uso com Python 3

Execute na raiz do repositório; não há pacotes adicionais para instalar. A ferramenta resolve o banco em relação ao próprio script, não ao diretório de execução. Use UTF-8 no terminal.

```powershell
python tools\ai_diary.py list --limit 50
python tools\ai_diary.py add --session "2026-11-09-tarefa-id-unico" --agent "Nome da IA" --phase planned --action "Implementar movimento" --details "Objetivo e abordagem" --files "Assets\Scripts\Player.cs"
# Use o ID retornado no lugar de 123 abaixo.
python tools\ai_diary.py add --session "2026-11-09-tarefa-id-unico" --agent "Nome da IA" --phase completed --plan-id 123 --action "Movimento implementado" --details "Alteracoes realizadas" --files "Assets\Scripts\Player.cs" --verification "Comando executado e resultado observado"
python tools\ai_diary.py check
python -m unittest discover -s tests -p "test_ai_diary.py" -v
```

`list` retorna as últimas N entradas em ordem crescente de ID, em JSON, e aceita `--session`. `add` retorna o ID criado. `--db CAMINHO` antes do subcomando permite selecionar uma cópia existente; a ferramenta não cria bancos vazios silenciosamente.

Antes de cada passo, registre o plano. Depois, registre o resultado vinculando-o ao plano. Use `failed` para execução com erro, `blocked` para impedimentos e `info` para contexto/decisões/correções. Não declare testes aprovados sem executá-los. As entradas iniciais de bootstrap são retrospectivas e estão explicitamente identificadas.

## Esquema SQLite (versão 1)

A tabela `entries` contém:

| Coluna | Conteúdo |
| --- | --- |
| `id` | Sequência incremental: ordem canônica dos eventos |
| `created_at` | Data UTC ISO 8601 gerada pelo SQLite |
| `session` | Identificador da tarefa/sessão |
| `agent` | Nome do agente que registra a ação |
| `phase` | `planned`, `completed`, `failed`, `blocked` ou `info` |
| `action` | Resumo objetivo do passo |
| `details` | Ações, decisões técnicas e resultados, em texto |
| `files` | Caminhos relativos afetados (texto livre) |
| `verification` | Comandos de validação, resultados ou limitações |
| `plan_id` | ID do plano da mesma sessão, obrigatório para resultados |

O esquema é consultável em `sqlite_master`; `PRAGMA user_version` identifica sua versão. Triggers rejeitam UPDATE/DELETE e referências a planos inexistentes ou de outra sessão. A ferramenta também ativa chaves estrangeiras. Essas proteções previnem alterações acidentais; não são proteção criptográfica contra quem pode editar o arquivo ou remover triggers.

### Consultas para qualquer cliente SQLite

```sql
SELECT * FROM entries ORDER BY id;

-- Planos sem nenhum resultado registrado, inclusive após interrupção.
SELECT p.* FROM entries AS p
WHERE p.phase = 'planned'
AND NOT EXISTS (SELECT 1 FROM entries AS r WHERE r.plan_id = p.id)
ORDER BY p.id;

PRAGMA integrity_check;
PRAGMA foreign_key_check;
```

Para gravar sem Python, habilite `PRAGMA foreign_keys = ON`, utilize transações e INSERT parametrizado com as mesmas colunas/protocolo da ferramenta. Não altere o esquema para contornar validações.

## Persistência e limites

- Preserve o banco no Git junto às alterações e feche conexões antes de copiar. O formato é binário: diferenças não aparecem como texto e merges exigem coordenação.
- Não grave segredos, transcrições integrais ou raciocínio interno privado. Registre fatos observáveis e justificativas técnicas resumidas.
- O protocolo não garante que toda IA externa o descobrirá ou cumprirá. Ao iniciar uma ferramenta sem suporte a AGENTS.md, forneça explicitamente o protocolo e o caminho do banco.
- O diário não inventa passos anteriores à sua criação. A inicialização tem registros retrospectivos; passos futuros devem ser registrados antes e depois da execução.
