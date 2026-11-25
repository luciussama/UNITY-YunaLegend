# WebGL — HUD mobile, gramado e Yuna HD

Publicação autorizada pelo usuário em25/11/2026: compilar WebGL, subir no Git e
fazer merge na `main` para teste em aparelho móvel.

## Conteúdo

- HUDs arredondados e responsivos no menu e nas cinco fases.
- Grama acompanhando a superfície, inclusive nas colinas.
- Yuna do Jardim15% maior,56 quadros High Fidelity2× e sombra oval suave.
- Correção do comando de reconstrução de cena durante Play Mode.
- Fontes e artes anteriores preservados; nenhuma mudança de Core/física/dificuldade nesta entrega.

## Build verificada

Unity6000.3.28f1, `WebCiBuild.Build`, saída0 e `YUNA_WEB_PAGES_OK`.
Seis cenas oficiais incluídas, com TitleScreen primeiro e Jardim pré-selecionado.
Gzip com fallback JavaScript, threads desativadas, memória inicial256MB/máxima1024MB.
Build gerada na cópia isolada e exportada para `WebGL/` do checkout central.

`build-manifest.json` e `WebGL/release.json` registram tamanho e SHA256 dos arquivos.
Verificação local por terminal: todos os arquivos referenciados existem e respondem
HTTP200; os três gzip descomprimem corretamente; `WebAssembly.validate` passou.

Antes da build, a validação Unity confirmou conteúdo visível dos56quadros na GPU,
dimensões/pivôs, dez enquadramentos e alpha da sombra0 nos cantos/1 no centro.
O processamento RGB separado da máscara original evitou a corrupção alpha do
Upscayl; dois recortes de origem do atlas de medo/cascalho foram corrigidos para
as colunas certas da primeira linha.

Os testes de layout/foco do menu e os eventos de UI já verificados estão em
`Evidence/RoundedHud`. O diário central registra as tentativas interrompidas e
as correções; elas não foram publicadas.

## Limites e teste móvel

O controle automatizado do Chrome no Windows foi bloqueado por não conseguir
identificar a URL atual. A publicação usa terminal/Git/HTTP, sem contornar esse
controle. Esta entrega não declara teste de gameplay no navegador nem em aparelho físico.
O usuário fará o teste mobile da versão publicada.

Destino: https://luciussama.github.io/UNITY-YunaLegend/
O status final do deploy e os commits são acrescentados após o GitHub Pages concluir.
