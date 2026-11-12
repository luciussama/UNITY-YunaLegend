# Apoios compartilhados, camera 5% e Game Over silencioso

11/11/2026, Codex (GPT-6), sessao 2026-11-11-codex-contatos-camera-5pct-c6e4. Etapa base anterior ao novo movel02 solicitado durante a execucao.

## Implementacao

PlatformSupportProfile representa a superficie superior exposta no plano Z=0 por segmentos e regioes, incluindo curvas trianguladas, inclinacoes e areas separadas. Extrai caixas e malhas de contato reais, elimina superficies ocultas abaixo de outras, conserva lacunas e filtra normais de apoio conforme o criterio de pouso ja usado pelo controlador. A consulta de altura interpola o segmento local; nao usa um retangulo envolvente para inventar apoio no vao.

GetSupportShapes compartilha as mesmas formas de autoria entre visual/fisica/gerador. No runtime, DarkRoomPlatformMarker.Support consulta colliders de Contacto_Fisico e invalida o cache quando transform/box/malha muda. GetTopCenterWorld, GetContactRotation, alvo do salto, ponto de decolagem, limite de derrota e alcance da camera consomem essa base. PlatformSpec.contact segue como autoria do tampo simples dos livros, sem limitar a API a um plano.

O circuito usa ancoras de apoio da subida e vaos entre superficies das outras secoes. Removidos os modulos antigos0.7/4.2; primeira transicao usa o vao real2.115m e subida de contato0.295m, preservando as posicoes atuais. Defaults das secoes2/3 agora derivam da secao anterior, em vez de origens independentes. Tops e transformacoes dos objetos da cena existente foram preservados nesta etapa.

Camera: RouteDistanceScale1.05 aplicado depois do calculo do close; FOV45 e inclinacao fixa preservados. Panorama tutorial nao recebe esse multiplicador. Dados do A/B em camera-backoff.json: largura10.5->11.025m; Z-7.129474->-7.485948; razao1.05. Foco normalizado em Yuna e no proximo apoio preservado.

GameOver: removidos campo/atribuicao/reproducao do clip eletronico do controlador e do ScenePopulator. Evento de derrota, retry, musica e demaisSFX continuam. Arquivo antigo nao foi apagado; nao existe referencia dele na cena salva nem chamada de reproducao na derrota. Nenhum gerador de audio foi executado nesta etapa.

## Validacao

- EditMode34/34 e PlayMode24/24 aprovados (58 distintos). XMLs locais editmode-final.xml e playmode.xml.
- Testes novos: duas areas sem ponte; superficies superiores ocultando faces inferiores; inclinacao conferida por raycast real; cache acompanhando parte que desce/inclina; perfis de autoria versus cena para38objetos; pontos de urso/livros versus colliders; corrida nos dois sentidos a30/60/120fps; capsula caindo no vao entre areas inclinadas; distancia da camera exatamente1.05 mantendo alvos/FOV.
- Teste fisico de pares obrigatorios passou a buscar decolagens nas regioes e alturas reais, sem somar janelas separadas por lacunas. Analise balistica usa gap e altura dos apoios.
- support-geometry.json:38plataformas; urso5regioes/86segmentos, livros1regiao/2segmentos; legacyGameOverField=false.
- 11capturasHDR estaticas pelo capturador permanente DarkRoomPlatformArtCapture; as10/11 sao A/B da camera nas mesmas poses/luzes. Vistas de inspecao02-04/06-08 usam luzes temporarias; nao confundir com gameplay. Capturas nao sao video de travessia nem nova auditoriaWeb.
- Integracao com guarda de blocos:4blocos existentes alterados; transformacoes de arte/rota, luzes e Volume preservados. integration.json discrimina IDs. SemPopulate geral/audio.

## Limites e continuidade

Curvas sao aproximadas pelos triangulos reais das malhas; fronteiras de apoio sao medidas no planoZ0, coerente com o runner2.5D. A API implementa formasBox/Mesh atuais; nao certifica outros tipos deCollider ou futuras malhas importadas sem dados legiveis/bake. Testes de capsula tratam o contato fisico real; largura de volume nao certifica o tamanho do apoio.

O pedido posterior do usuario foi identificado como movel02 depois dos livros: superficie longa cedendo, estralo central, atraso local e tropeco. Isso sera implementado como etapa seguinte, utilizando este contrato dinamico, e tera sua propria recalibracao/validacao.

Proxima execucao PRIORITARIA apos esse escopo: modelar Yuna e Nanda, incluindo pes/rig/animacoes (corrida,salto,pouso,tropeco) e asas/voo. A pose procedural no proxy durante o trabalho do movel e transitoria, nao animacao final de personagem.

Sem commit/push nesta etapa. Diario registra plano337 e resultados.
