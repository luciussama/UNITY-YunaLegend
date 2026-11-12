# Investigacao do handoff

Relatorio integral: docs/ia/INVESTIGACAO-HANDOFF-2026-11-11.md.

## Reproducao

Use Unity6000.3.28f1 e um projeto isolado com copias de Assets, Packages e ProjectSettings do checkout. Nao execute geradores na copia principal aberta. O editor principal pode permanecer aberto se a validacao usar outro projectPath.

Copie tools/HandoffProbe.cs.txt e tools/HandoffNoiseProbe.cs.txt desta pasta para Assets/Scripts/Editor no projeto isolado, com extensao .cs. Defina a variavel de ambiente YUNA_HANDOFF_AUDIT_OUTPUT para uma pasta absoluta de saida e inicie Unity com -batchmode -quit -projectPath CAMINHO_ISOLADO -executeMethod HandoffNoiseProbe.Run -logFile CAMINHO_LOG. Essa sonda regrava WAVs SOMENTE no projeto isolado e depois inspeciona a cena sem salva-la. HandoffProbe.Run sozinho nao gera audio.

Rode testes separadamente com -batchmode -projectPath CAMINHO_ISOLADO -runTests -testPlatform EditMode (depois PlayMode) -testResults CAMINHO_XML -logFile CAMINHO_LOG. Evite -quit nesse comando de testes.

render_support_map.py precisa Pillow; nesta maquina usa Build/VisualAuditTools. Arquivos .cs.txt preservam o codigo exato sem importar ferramentas de auditoria no projeto principal. Platform00Capture.original.cs.txt foi resgatado do clone, tem Output absoluto que deve ser adaptado se for reproduzido e inclui luzes/cameras de inspecao. Nao se trata de captura identica a gameplay.

Dados: probe.json (882amostras), audio-regeneration.json (7WAVs diferentes em2geracoes), audio-diff.json, source-snapshot.json, code-references.json, XMLs novos26+21Passed e trechos de logs. O mapa usa grade interior .998 da extensao do colisor, tolerancia2cm; branco indicaZ0. A amostragem nao certifica todosospontos da superficie nem area dos pes. Contagem de triangulos e renderers e estatica, nao profiling de GPU.

Nenhum arquivo critico de fonte/cena do snapshot mudou na investigacao. Evidencias e sondas novas estao pendentes de commit; nao alegar que um checkout anterior ja as contem.

## Estado anterior a alteracao artistica subsequente

A pedido do usuario, o urso recebeu contato por malha depois desta auditoria. Para reproduzir a medicao antiga, descompacte scene-before-art.unity.gz em Assets/Scenes/HandoffBaseline.unity SOMENTE no projeto isolado e defina YUNA_HANDOFF_SCENE=Assets/Scenes/HandoffBaseline.unity. O SHA256 descompactado corresponde ao snapshot original. As duas versoes anteriores dos materiais estao em tools/*.before.mat.txt para reproduzir o aspecto antigo no clone; as texturas procedurais anteriores permanecem no projeto. O capturador de arte passou tambem a fonte permanente Assets/Scripts/Editor/DarkRoomPlatformArtCapture.cs durante o trabalho artistico posterior.
