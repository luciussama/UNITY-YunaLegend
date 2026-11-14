# Próximo entregável — arte conceitual ABC04 concluída

## ABC04: arte conceitual aprovada; construção da cena adiada

O usuário aprovou `abc-stack-v1.png` e definiu expressamente: **“o entregável vai ser só a arte conceitual”**.
[Aprovação e escopo literal](../../Evidence/25d/T08D/aprovacao-conceitual.md).
A pintura está salva em `Assets/Art/Illustrated25D/Environment/ABC/abc-stack-v1.png`. T08D concluída no escopo conceitual.

**Construção da cena ABC04: POR FAZER — ADIADA PELO USUÁRIO.**
Não iniciar agora posicionamento, integração, apoio físico, salto ou nova build. Retomar apenas mediante novo pedido do usuário. A arte aprovada não valida contato ou dificuldade.
Build aprovada até a cômoda T08C preservada. Estante permanece adiada, sem aceite global T06/T07. As seções abaixo registram estados anteriores e não autorizam iniciar a construção adiada.


Preparado em 13/11/2026, por solicitação do usuário. **Pronto para retomada;
implementação não iniciada por este registro.** Reserva T06 liberada ao encerrar.

## Ponto de partida aprovado

Build de acompanhamento com Yuna T03, Nanda azul T04, urso/livros/móvel e quarto
T05.4, sem fallback visual 3D. Aprovação literal: “perfeito. Aprovado.”, em
[registro da v6](../../Evidence/25d/T06-acompanhamento/reviews/acompanhamento-v6-aprovacao.md).
Menu Unity atual: abrir cena, gerar build Windows e abrir pasta da build.
Não repetir a revisão estética já concedida nem reconstruir o cenário antigo.

## Entrega concreta — T06.3

**Uma nova build de acompanhamento e um vídeo curto contínuo**, na câmera real,
mostrando falha/reinício no móvel e uma segunda tentativa completa. O vídeo deve
permitir ao usuário observar o estado anterior, o reset e o novo evento.

Sequência mínima:

1. Começar pelo trecho ilustrado e atravessar urso → livros → móvel.
2. Reiniciar durante a pausa local/tropeço do móvel e conferir o estado inicial.
3. Na nova tentativa, atravessar novamente: um único estralo/evento, cedimento,
   recuperação e conclusão do trecho.
4. Demonstrar também a falha de um salto e o botão de tentar novamente, além de
   reinício por R e pausa/retomada por P/Esc.

O teste atual já comprova travessia e reset **depois de concluir**. Falta comprovar
reinício **durante o evento** e a segunda travessia após esse reinício. Não apresentar
os mesmos flags de reset inicial como prova dessa sequência adicional.

## Critérios de conclusão

- Testes adequados em 30/60/120 Hz: sem exceções, estado residual ou evento duplicado;
  contador de um evento por tentativa e novo evento possível após o retry.
- Tampo/abertura, apoio físico, animação, trilha da Nanda, áudio e relógios locais
  retornam ao estado correto. Pausa local de 80 ms, recuperação de 280 ms e cedimento
  de 7,5 cm preservados; não usar pausa global para simular o tropeço.
- Zero renderers legados visíveis; física 3D e PlatformSupportProfile preservados.
- Captura do executável com UI; identificar taxa de simulação e de captura.
  Registrar áudio capturado/escutado e seus limites sem presumir validação.
- Build, vídeo, relatório, hashes e instruções de reprodução entregues juntos.
  Revisão humana da nova sequência antes de encerrar T06.

## Retomada técnica

1. Ler estado atual, diário e status; reservar T06 no coordenador antes de editar.
2. Abrir `Assets/Scenes/DarkRoom_IllustratedPreview.unity`. Não usar a Gameplay
   legada como saída de acompanhamento.
3. Partir de `Assets/Art/Illustrated25D/Preview/PreviewVerification.cs` e
   `IllustratedPreview.cs`, que já possuem travessia por input e recarga de cena.
4. Observar `YieldingWoodFurniture` e `YunaStumbleIllustration`; manter os
   transforms técnicos antigos usados pelo evento e ocultar somente seus visuais.
5. Usar menu **YunaLegend → Gerar build Windows de acompanhamento** ou entrada
   batch `YunaLegend.Illustrated25D.Preview.Editor.IllustratedPreviewBuild.Run`.
   O comando de menu mantém o Editor aberto; a entrada batch o encerra.
6. Se o Editor do usuário estiver aberto, preservá-lo e usar cópia técnica com
   hashes conferidos. O clone anterior está em
   `C:/Users/sergi/repos/UNITY-YunaLegend-preview-20261113`; sincronizar alterações
   atuais antes de reutilizar. Não compartilhar Library entre projetos.

Build local: `Build/Windows/YunaLegend-Acompanhamento/YunaLegend.exe`.
Evidências atuais: [T06-acompanhamento](../../Evidence/25d/T06-acompanhamento/README.md).
O manifesto da v6 é histórico e imutável; inventários posteriores devem ser novos.

## Depois

Fechar os critérios restantes do piloto T07 com evidência contínua e identificação
da cobertura de teclado/toque. Só então expandir a linguagem ilustrada para a
próxima plataforma. Esta preparação não inicia T08, não autoriza publicação e
não declara teste em aparelho móvel, WebGL ou oclusão dinâmica.
