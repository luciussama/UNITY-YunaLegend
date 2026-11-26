# Cartas discretas no mobile: valida??o final

Unity6000.3.28f1, PlayMode v4:2/2Passed, exit0 confirmado pelo Root. Fontes executadas conferidas com o checkout central em sources.json.

O teste usa CharacterController3D real, largada no apoio30 real, somente plataformas de retorno ativas e velocidade5.2. Compara baseline sem colapso e sequ?ncia com colapso frame a frame a30/60/120Hz, fixando Time.captureDeltaTime em1/hz e restaurando ao terminar. Em todos os casos: influ?ncia do colapso na c?mera0m e diferen?a da trajet?ria f?sica0m. CSV e metrics.json preservam26/52/103amostras por tentativa. A c?mera n?o ganhou foco/solavanco por causa do evento.

O material exclusivo mobile mant?m a pintura, quatro camadas de apoio e quinze cartas em penumbra fixa, sem reagir ? lua ou Nanda. Teste validou20materiais e restaura??o dos materiais desktop. PNGs antes/durante/depois mostram Yuna e cartas com Nanda pr?xima; revis?o visual realizada pelo Root. O m?todo atualiza apenas apresenta??o ilustrada/ilumina??o antes do render; n?o altera f?sica, posi??o da c?mera ou trajet?ria.

Ramo desktop ? preservado por Application.isMobilePlatform estrito; testes for?am override expl?cito. Sem altera??o da tela inicial, seletor de fases ou ordemWebGL.

Limites: fixture controlada de trecho real de gameplay, sem percurso completo, combate, navegadorWebGL ou aparelho f?sico. Os testes n?o aprovam desempenho/ergonomia de um celular espec?fico. A evid?ncia v3 foi preservada separadamente; seus PNGs iniciais eram inv?lidos porque a abertura mantinha Yuna suprimida, problema restrito ao harness de captura e corrigido emv4.
