# WebGL multitouch do Jardim

Build Unity6000.3.28f1 exit0 e YUNA_WEB_PAGES_OK. Release20261125-jardim-multitouch. Branch regress?o-jardim; sem merge main.

Tr?s gzip descomprimidos por completo, assinatura wasm v?lida e valida??o WebAssembly no Node. Index com query de vers?o para loader/data/framework/wasm para evitar cache da build anterior. Manifesto SHA256 dos quatro arquivos Build.

Testes e renders Unity em ../. Publica??o por workflow_dispatch da pr?pria branch. Teste f?sico de dois dedos ainda depende do aparelho do usu?rio. N?o foi executado gameplay no navegador nesta publica??o.

## Publica??o aguardando decis?o

Fontes e WebGL enviados em74d8332 na branch regress?o-jardim. DispatchHTTP204 aceito, mas run45719737744 foi negado pelo ambiente: somente main est? na lista de branches autorizadas. Nenhuma prote??o foi alterada. Foi solicitada decis?o humana entre autorizar especificamente esta branch no Pages ou fazer merge na main. O endere?o p?blico continua servindo a release anterior at? a publica??o concluir.

## Publica??o conclu?da pela main

Usu?rio autorizou merge na main. Merge b659cffca4779c9ffc63b4671925eae9c8e012b3 enviado; Pages run45735150787 conclu?do com sucesso e associado a esse SHA. Manifesto p?blico corresponde ? release20261125-jardim-multitouch; quatro arquivos HTTP200 com tamanho correto; download wasm com SHA256 correspondente; index igual ap?s normalizar CRLF/LF. Detalhes em deployment-main.json. A falha anterior da branch foi preservada como hist?rico; nenhuma prote??o foi alterada. Teste f?sico de multitouch permanece com o usu?rio.
