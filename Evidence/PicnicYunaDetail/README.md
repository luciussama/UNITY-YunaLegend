# Yuna no Jardim — tamanho, resolução e sombra

## Atualização de publicação — 25/11/2026

**Piloto High Fidelity aprovado pelo usuário:** “Nossa, adorei”, seguido de pedido
para compilar WebGL, publicar no Git e fazer merge na main (diário1783).
O Upscayl High Fidelity foi aplicado aos **56 quadros**, mantendo os oito atlas,
as transparências, os pivôs e a sequência. Unity confirmou **56/56 fatias e pivôs**,
**10 capturas** e máscara da sombra com **alpha0 nos cantos /1 no centro**; saída0
em `unity-approved.log`. O aumento visual de15% e o corpo físico original foram preservados.

[Prancha final dos56quadros](high-fidelity-56-contact-sheet.png).
`hd-atlas-manifest.json` e `validation-manifest.json` identificam o processo final.
A publicação prossegue no registro196; registro194 concluído. Sem validação em aparelho físico presumida.

**As seções abaixo preservam o checkpoint anterior à aprovação e não representam
o estado final dos atlas.**

Pedido de 25/11/2026: aumentar a resolução, deixar a menina um pouco maior e corrigir a sombra quadrada.

## Já implementado

- Arte da Yuna **15% maior** no gameplay, com escala aplicada somente ao renderer.
- **56 quadros em 2×**, aproximadamente 512 px de altura, nos oito atlas normais/medo.
  Esta base usa reamostragem fiel Lanczos com alpha premultiplicado; não sintetiza detalhes novos.
- Atlas compactados por enquadramento, conservando a sequência, os pivôs e o tamanho em mundo
  antes do aumento visual. Oito texturas somam cerca de 69,8 MB de RGBA sem compressão.
- Sombra oval com máscara radial de transparência, borda suave, orientação adaptada à inclinação
  e posicionamento dentro da superfície do piso. O plano não acompanha a personagem no ar.
- Corpo físico original preservado: altura 0,85 m, raio 0,16 m. Nenhum Core ou regra de gameplay alterado.

## Piloto de recuperação de detalhes por IA — revisão humana pendente

O usuário autorizou pesquisar no catálogo e escolheu instalar **Upscayl**.
Foi instalado o pacote Windows portátil oficial **2.15.0**, em `Build/Tools/Upscayl/app`.
O executável de processamento é `resources/bin/upscayl-bin.exe`.

Foram comparados Digital Art, Standard e High Fidelity em **um único quadro**.
High Fidelity foi escolhido como candidato técnico por conservar melhor o acabamento pintado.

- [Comparação original / High Fidelity / Standard](upscayl-test/comparison-fidelity.png).
- [Comparação com Digital Art](upscayl-test/comparison.png).
- [Piloto real na Unity, celular](pilot-unity/idle-wood-390.png).
- [Piloto real na Unity, desktop](pilot-unity/idle-wood-1280.png).
- [Tamanho e sombra, antes/depois](before-after.png).

**A IA ainda não foi aplicada aos 56 quadros.** O piloto está somente nas evidências;
o checkout permanece com a ampliação fiel em 2×. A resposta humana à pergunta assíncrona
define a próxima etapa, conforme o protocolo de microentregas.

## Validação observada

Unity **6000.3.28f1**, compilação e capturas com saída **0**.
**56/56 quadros** com dimensões duplicadas, pivôs dos pés e escala em mundo conferidos.
Máscara da sombra lida de volta da GPU: **quatro cantos alpha 0, centro alpha 1**.
Dez capturas desktop/mobile cobrem repouso, corrida na grama, cascalho, colina e salto.
São enquadramentos estáticos reais da Unity; não representam travessia completa em Play Mode,
WebGL ou teste em aparelho físico. A assinatura e o acabamento do piloto aguardam revisão humana.

`validation-manifest.json` contém os hashes dos fontes, atlas e modelo High Fidelity.
`hd-atlas-manifest.json` registra o empacotamento da base fiel em 2×.
O harness `PicnicYunaDetailCapture.Capture` é executado na cópia isolada
`Build/PicnicVisualAuditProject`, com todos os fontes e texturas pertinentes sincronizados.

## Próximo passo

Após o aceite do piloto: recortar os 56 quadros originais, ampliar com o mesmo modelo
High Fidelity em 2×, manter alpha/pivôs, reempacotar nos mesmos oito atlas, sincronizar
a cópia isolada e repetir validações de dimensões, apoio e sequência visual.
Não considerar a comparação técnica como aprovação estética.

Registro **194 aberto**, sessão `codex-20261125-yuna-detail`, aguardando revisão.
Registro de instalação **195 concluído**. Branch `fix/picnic-editor-playmode`, sem commit/push.
Nenhum processo próprio de Unity/Upscayl ficou aberto; o Hunyuan iniciado pelo usuário foi preservado.

Fontes oficiais consultadas: [Upscayl](https://github.com/upscayl/upscayl),
[Clarity Refiners UI](https://github.com/pinokiofactory/clarity-refiners-ui).
