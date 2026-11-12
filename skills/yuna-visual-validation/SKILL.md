---
name: yuna-visual-validation
description: Produzir e revisar evidencia visual da apresentacao 2,5D de Yuna Legend na Unity, incluindo animacao, camera, contato, superficies reais e continuidade de clipes. Use para uma microentrega visual ou investigacao de movimento; nao substitui geracao de arte nem valida publicacao.
---

# Yuna — validação visual e de movimento

Use junto de `yuna-project`. Resolva caminhos pela raiz do checkout e consulte
o estado/checkpoint atual para saber qual microentrega está autorizada.

## Identidade e autoria

Leia `docs/ia/25D-ASSINATURA-E-MICROENTREGAS.md` e a direção aprovada em
`Evidence/25d/T01/DIRECAO-VISUAL-APROVADA.md`. Yuna mantém sua identidade pintada;
Nanda tem aparência/luz azulada. A assinatura visual não é autorização para
aprovar novas poses automaticamente.

Para criar/editar arte raster, use `imagegen` quando disponível no ambiente e
registre ferramenta, prompt, referências, arquivo bruto e hashes. Preserve a
pintura aprovada; crie versão irmã. Inspecione RGBA, dimensões reais, alpha e
registro dos quadros: o tamanho solicitado ao gerador não comprova o resultado.
RGB colorido em pixel quase transparente não é, sozinho, evidência de franja
opaca; examine o alpha e a renderização real em fundo claro/escuro.

Compartilhe canvas, escala e âncora de sola dos quadros; não iguale a altura de
cada pose pela caixa opaca. Inspecione NPOT, padding, pivôs, PPU, filtro, mipmaps,
compressão e Read/Write no importador. Não troque pipeline global para resolver
um asset. Preserve fontes/GUIDs e relações de sorting, Z e oclusão.

## Prova controlada

Uma microentrega responde a uma pergunta. Use cena/harness de avaliação com
escopo claro e altere uma variável por comparação: câmera, arte ou movimento.
Conserve física e Gameplay quando esse for o contrato. Capture ampliação e escala
real de gameplay, com configurações e hashes dos fontes efetivamente executados.

Distinga render conceitual, render Unity estático, Play Mode automatizado,
sessão manual e dispositivo real. Uma fixture com controlador desabilitado
prova apresentação estacionária, não gameplay. Um clipe autorun de chão técnico
não comprova travessia das plataformas ou entrada de menu.

Se controlar a UI, use a ferramenta disponível e as instruções próprias desse
ambiente. Observe a janela/projeto/cena corretos, preserve trabalho aberto e
salve evidência. Ter estas instruções em disco não instala controle de computador.

## Medir apoio e continuidade juntos

Para animação, observe pernas/pés/braços e o movimento do corpo no mundo e na
câmera. Registrar apenas o menor vértice da malha não verifica a sola opaca.
Um pé fixo pode coexistir com um salto indesejado do tronco.

- Identifique o ponto de contato pintado, a superfície física, a pose e sua fase.
  Separe pé de apoio de pé no ar. Considere rolagem/transição do contato.
- Meça deslizamento e separação vertical no referencial do apoio. Verifique
  também posição, velocidade e saltos do pivô visual quando muda a pose/âncora.
- Inspecione apoio→voo, voo→apoio, virada, entrada/saída de estados e reset.
  Não zere uma compensação do corpo sem conferir o efeito no vídeo.
- Compare os últimos quadros com os primeiros: pose/fase, deslocamento do corpo,
  câmera, padrão do chão, duração e pausa no GIF. Não corte o fim para ocultar
  um problema que também se repete no ciclo interno.
- Use normal e câmera lenta, sem retocar o render. Se reduzir fps, quantizar GIF
  ou repetir o clipe, registre as mudanças de apresentação e sua duração real.

Quando pertinente, teste a 30/60/120 Hz. Separe frequência de simulação da captura
e do arquivo de apresentação. Use tolerâncias dos contratos vigentes e reporte
falhas sem trocar a meta só para obter aprovação. Confirme qualidade visual após
correções numéricas; uma malha que fixa um marco mas distorce a bota falha na arte.

## Plataformas e móvel

Leia a regra das plataformas em `AGENTS.md` e os contratos pertinentes em
`Evidence/25d/T01/`. `PlatformSupportProfile` e os colliders reais governam o apoio,
inclusive superfícies inclinadas, regiões desconexas e bordas. Teste a coincidência
do topo visível com a superfície física na câmera final; não crie pontes em lacunas.

No móvel, visual e contato recebem a mesma pose de cedimento. Verifique estralo,
pausa local de 80 ms, reação/tropeço e reset no retry conforme o contrato ativo.
Não use pausa global como equivalente. Arte estática do móvel não valida a sequência.

## Desempenho, fontes e avaliação humana

Meça e identifique CPU/GPU, memória residente, aparelho e build quando disponíveis.
Tamanho PNG, pixels crus, memória de textura no Editor e orçamento de atlas
estimado são medidas distintas. Considere residência conjunta dos estados, não
apenas o atlas isolado. Editor/batch não é resultado de celular.

Em `Evidence/25d/<tarefa>/`, registre objetivo, versão, fontes/hashes, reprodução,
resultado real, limitações e a revisão humana. Preserve ensaios rejeitados como
histórico, claramente separados da candidata atual. Pergunte sobre identidade,
ritmo e leitura do apoio; não peça apenas uma aprovação genérica de testes.
Registre a resposta literal com referência e escopo. A consulta desta skill não
retoma automaticamente ajustes que o usuário tenha adiado.
