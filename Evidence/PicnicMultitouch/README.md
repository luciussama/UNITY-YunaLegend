# Jardim: controles multitouch

Branch regress?o-jardim. Pulo inicia no pointerdown, permanece ativo enquanto segurado e respeita a altura vari?vel existente. Cada dedo tem estado independente; soltar o pulo n?o libera o direcional. Pequeno deslizamento fora da arte n?o cancela a dire??o. Perda de foco/desativa??o limpa as entradas.

Setas: 80?80 unidades l?gicas. Pulo: 108?80. ?rea segura e separa??o preservadas em320?568,390?844,844?390.

Unity6000.3.28f1, c?pia isolada com hashes conferidos:15/15 EditMode passaram. Inclui simula??o de avan?o e salto a30/60/120Hz, eventos PointerEventData com IDs distintos, gesto curto entre frames e limites de layout. CapturePicnic original: exit0, cinco callbacks gerais preservados, pulo/hold/release verificados.

PNGs s?o renders Unity est?ticos de layout; n?o comprovam toque f?sico. A primeira rodada12/15 falhou na fixture de bounds porque o CanvasScaler ainda n?o havia estabilizado; fixture corrigida, segunda rodada15/15 passou.

Ainda sem teste em aparelho f?sico ou execu??o multitouch em navegador real. Publica??o WebGL ser? registrada separadamente.
