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

## Publica??o conclu?da

Branch enviada: `ae27f46`. Merge autorizado na main: `1e704ee`.
GitHub Actions [45691514983](https://github.com/luciussama/UNITY-YunaLegend/actions/runs/45691514983): success.
Manifesto p?blico id?ntico ao local; quatro arquivos da build HTTP200 com tamanhos corretos.
Conte?do do index e loader corresponde ao local ap?s normalizar CRLF/LF do Git.
Detalhes em `deployment.json`. O primeiro script de confer?ncia tentou buscar o index no manifesto,
que lista apenas os quatro arquivos Build; corrigido para comparar o arquivo local diretamente.

Rastreabilidade: o hash do harness no manifesto antigo de Yuna antecede a checagem GPU de conte?do.
O hash final est? em `deployment.json`; os outros dez hashes de fontes/atlas conferem.
Teste em aparelho f?sico e gameplay no navegador n?o foram realizados nesta publica??o.
