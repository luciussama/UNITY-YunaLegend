# T02 — fechamento da prova técnica

12/11/2026, Codex (GPT-6), sessão `2026-11-12-codex-close-t02-c73a`.

As microentregas de Yuna estática, projeção A, câmera estável e prova de deformação
v2 receberam revisão humana. A decisão é quadros pintados para os membros,
com deformação secundária e física 3D preservada. As provas são cenas isoladas,
não a migração da Gameplay nem um piloto final aprovado.

[Contratos técnicos](CONTRATOS-TECNICOS.md) consolidam pivôs, importação, luz,
estados, câmera, contatos e orçamento. As tolerâncias e limites de produção são
metas explícitas para T03/T05/T06/T09; resultados já medidos estão separados.

[Auditoria reproduzível](technical-audit.json): `python tools/audit_25d_t02.py`.
Confere fontes atuais versus hashes e checkpoint `5d2e6db`, além da existência das
quatro revisões e dos 32 resultados PlayMode anteriores. A câmera tem comparação
de texto com finais de linha normalizados conforme `.gitattributes`; os hashes
dos arquivos em disco também são registrados. Não é nova execução de testes.

Limitações preservadas: sola visual precisa de calibração (amostras root/apoio
0,0800–0,1475 m), membros ainda sem animação final, orçamento de atlas estimado,
GPU/aparelhos não medidos, composição retrato pendente e ambiente/Nanda ainda da
base técnica. A validação do móvel na base física não aprova o móvel ilustrado.

Próxima entrega: T03.1, repouso/respiração de Yuna em loop; revisão individual
antes de corrida. Arte do projeto e ferramentas listadas nas pastas T02.1,
T02.2, T02.2b e T02.3 mantêm sua autoria original. Nenhuma publicação de build.
