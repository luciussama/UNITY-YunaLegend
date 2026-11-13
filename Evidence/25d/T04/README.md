# T04 — Nanda ilustrada e animada (agregada)

Sessão `2026-11-13-copilot-visual-parallel-b29a5ec7`. Fecha T04.1 + T04.2 com os limites abaixo; **não** integra a fase.

## Microentregas e aceites

| Microentrega | Entrega | Aceite |
| --- | --- | --- |
| T04.1 | Oito poses de asas, atlas `2048×1184`, PPU 880, ciclo 0,8 s, hover 3,5 cm, flip por direção; provas 30/60/120 Hz | aprovada em 13/11/2026 (`Evidence/25d/T04.1/reviews/T04.1-revisao.md`) |
| T04.2 | Acompanhamento sobre `NandaFairyGuide`, luz azul discreta, trilha curta, gesto leve; 150 quadros/câmera em 30/60/120 Hz | aprovada em 13/11/2026 (`Evidence/25d/T04.2/reviews/T04.2-revisao.md`, resposta literal `voo da nada aprovado. Ficou muito bom.`) |

## O que está verificado

- Pose/atlas preservados byte a byte nas fontes protegidas de T04.1.
- Orientação e espelhamento: `mirrorError=0`, zero violações de orientação na T04.1.
- Separação dianteira mínima no acompanhamento: 1,416 / 1,408 / 1,397 m (30/60/120 Hz), acima do piso de 1,35 m do contrato.
- `NandaFairyGuide` não foi alterado; navegação, destino e trilha continuam no guia.
- Reset limpa trilha/relógio/pose; disable limpa trilha.
- Atlas residente T04.1: 4,8134 MiB CompressedHQ (limite 16).

## Limites registrados (não aprovados por este aceite)

1. **Luz sobre superfícies**: sprites padrão são unlit; a prova não demonstrou iluminação efetiva no cenário. Precisa de prova na cena integrada (T06).
2. **Integração na fase**: nada foi salvo em `DarkRoom_Gameplay`; pertence a T06.
3. **Câmera final**: as capturas usam câmeras de prova, não a câmera de produção.
4. **Dispositivos e memória conjunta**: não medidos; pertencem a T09.

## Reprodução

```powershell
& '<Unity6000.3.28f1>\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\sergi\repos\UNITY-YunaLegend' -executeMethod YunaLegend.NandaIllustration.Editor.NandaAccompanimentCapture.Run -t042Hz 60 -t042Output Evidence/25d/T04.2/capture-60 -logFile Evidence/25d/T04.2/capture-60.log
& .\.venv\Scripts\python.exe tools\t042_review.py --capture Evidence/25d/T04.2/capture-60
```
