# T03.4 — tropeço e recuperação

Estado: T03.4 v2 aprovada individualmente em 12/11/2026; conjunto T03 e integração pendentes. Codex (GPT-6), 12/11/2026.

T03.3 foi aprovada pelo usuário. Próxima microentrega: reação ilustrada ao
BeginStumble existente, mantendo pausa local de 80 ms e recuperação de 280 ms.

Preparar poses de perda de equilíbrio, braços buscando apoio, ajuste dos pés e
retomada, com vestido/cabelo acompanhando. Não simular apenas inclinando a pintura.
Conservar física, velocidade, colisor e apresentações já aprovadas.

Prova isolada deve medir evento único, duração local, contato e continuidade em
30/60/120 Hz, além do reset/repetição. Entregar normal/lento e escala de gameplay
antes de aprovação. O móvel ilustrado cedendo pertence a T05.3/T06.2; este contrato
não declara essa integração feita. Na criação deste contrato ainda não havia arte; a candidata atual e as provas estão no README.md.


## Registro da candidata v1, sessão a71c

Apoio definido no topo Y=0 do colisor técnico real da fixture Run, preservado.
PPU330 único; grade arredondada 4×2 em 1774×887; nenhuma pose reescalada por bbox.
Baseline top-left: [430,429,428,428,429,430,431,431].
SoleX: [219,229.5,233.5,273.5,327,177,180,157.5], todos em alpha opaco ≥128.
Ponto ancorado enquanto cada pose é mostrada; troca de desenho muda apoio.
Importador: NPOT None, clamp, bilinear, sem mipmap/compressão/ReadWrite, max2048,
sRGB e alphaIsTransparency. Sprite.Create FullRect; GUIDs preservados.
ResetPresentation limpa só a apresentação; ResetPlayer governa lógica e passos.
Integração desse hook no retry real e móvel continua pendente em T06.


QA v2: material Stumble exclusivo suprime névoa de alpha com smoothstep(.075,.18), sem alterar o PNG original. Capturas finais-v2 e review-v2 substituem ensaios visuais v1 como candidata atual.
