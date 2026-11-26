# Arena: enquadramento mobile

Implementa??o somente mobile, ativada por Application.isMobilePlatform ou override expl?cito para QA. Desktop mant?m as f?rmulas e clamps anteriores, incluindo o fallback j? existente para aspect estreito/touchscreen. Tela de entrada, ordem das fases e gameplay n?o foram alterados por esta entrega.

## Valida??o

Unity6000.3.28f1 na c?pia isolada:37/37 testes Arena passaram, incluindo11testes novos de enquadramento mobile. Quatro fontes executados conferem SHA256 com o checkout central em sources.json. O root executou o preflight CaptureAllFinal com sa?da0 (mobile-final-provenance.log) e revisou os seis PNGs. A primeira execu??o usava um harness sem a linha de flushCSV intermedi?rio; foi repetida ap?s sincroniza??o para garantir identidade dos quatro fontes. O di?rio preserva a corre??o da primeira tentativa de arquivamento.

As imagens mostram fixtures Editor em390?844 e844?390 com separa??es2/5/9m. Foram atualizadas as posi??es e anima??es reais de ArenaActorView, com a c?mera percorrendo transi??es suaves por180frames entre poses. A fixture n?o executa a simula??o de combate e n?o equivale a um teste em aparelho f?sico.

CSV cont?m dist?ncia, altura dos meshes em pixels, maior deslocamento da c?mera por frame durante a transi??o e limites horizontais projetados de ambos os atores. O harness verifica os oito cantos dos bounds de cada mesh ativo e falha se algum ator sair horizontalmente do viewport. Todos os seis casos passaram.

## Resultado

No close retrato2m, Yuna ocupa142,84px de844; a locomotiva inteira termina emx371,27 numa tela390px. Em paisagem2m, Yuna ocupa203,94px de390 e o cen?rio permanece vis?vel. Conforme se afastam, a c?mera abre gradativamente: alturas de Yuna142,84?92,31?62,61px no retrato e203,94?175,28?125,20px na paisagem.

A revis?ov2 resolveu um corte da locomotiva no primeiro retrato: sua largura real normalizada ?3,966m. Apenas no retrato, a regra agora reserva4,2m de folga total e desloca8% da separa??o para o lado do inimigo. O enquadramento paisagem aprovado foi preservado.

O teste f?sico de toque, desempenho e jogabilidade mobile n?o foi realizado por este agente. Estas provas validam a apresenta??o da c?mera e a cobertura dos atores nas seis poses indicadas.
