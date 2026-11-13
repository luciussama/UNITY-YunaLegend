# T05.4 — captura de camadas na Unity

Sessão `2026-11-13-copilot-t053-b29a5ec7`. Primeira montagem real do quarto em camadas atrás do trecho. **Candidata para revisão humana; não aprovada.** A cena não foi salva.

## O que foi executado

`Unity 6000.3.28f1 -batchmode -executeMethod YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run`

- Importou as três PNGs com `spritePixelsToUnits: 72`, `alphaIsTransparency: 1`, mipmaps ligados.
- Instalou `RoomBackgroundT054` em memória e o móvel da T05.3 (para a composição ficar completa).
- **Ocultou 272 renderers da geometria 3D legada do quarto** — ela ficava à frente da arte pintada e escondia o piso. Plataformas, jogador, Nanda, móvel e quarto permaneceram.
- Câmera de inspeção própria: FOV 45°, 16:9, `z = −9,0526` (96 px/m no plano do jogo), sem a câmera de rolagem.
- Quatro enquadramentos a 1280×720, salvos em `capture/`.

## Medições reais (do `room-audit.json`)

| Grandeza | Valor |
| --- | --- |
| Piso físico medido (base do urso/livros) | `y = −8,8556` |
| Centro do trecho | `x = 8,6675` |
| Extensão do trecho | **15,289 m** (`x = 1,023 … 16,312`) |
| Distância da câmera | 9,053 m → 13,33 m de largura visível |
| Camada de fundo | 28,444 × 14,222 m, ordem −300, deslocamento 0,35 |
| Camada de meio | 28,444 × 14,222 m, ordem −290, deslocamento 0,45 |
| Primeiro plano | 28,444 × 14,222 m, ordem 40, fixo no mundo |
| Linha de piso pintada | 1,333 m acima da borda inferior |

O importador arredondou 1536×768 para potência de dois (2048×1024). Isso **não** alterou a escala aparente (72 px/m em ambos), apenas aumentou a cobertura de 21,33 para 28,44 m — sobra de margem para o parallax.

## Cobertura conferida por enquadramento

| Tomada | Câmera x | Fundo x | Visível |
| --- | --- | --- | --- |
| 01 início | 2,030 | 6,344 | −4,64 … 8,70 |
| 02 livros | 6,070 | 7,758 | −0,60 … 12,74 |
| 03 vão | 7,287 | 8,184 | 0,62 … 13,95 |
| 04 móvel | 12,112 | 9,873 | 5,44 … 18,78 |

Nenhum enquadramento expõe borda da arte.

## Checagens numéricas feitas

- Faixa de piso (88–100% da altura) deixou de ser preta: passou de `(9,3,1)` para `(118,116,133)` — o piso pintado aparece.
- Topo azul-violeta consistente: `(92,102,132)`.
- Primeiro plano na ordem correta: borda esquerda escura na tomada 01 `(57,46,47)` e borda direita escura na tomada 04 `(56,35,28)`.
- Cobertura horizontal sem lacunas (tabela acima).

## Limites e riscos que a revisão precisa julgar

1. **Escala do quarto.** Com 72 px/m, o rodapé de madeira mede ≈3,95 m de altura e a janela ≈6,6 m de largura. O móvel tem 2,3 m de altura. O quarto lê como salão, não como quarto infantil — é consequência de a câmera mostrar 13,3 m de largura. Corrigir exige arte mais larga (mais módulos de parede na mesma escala), não apenas reescalar: reduzir a escala deixaria a arte menor que o campo de visão.
2. **Iluminação.** Sprites são unlit; o luar pintado não ilumina as plataformas 3D. Elas continuam sob a luz da cena aprovada em T05.1/T05.2. Não adicionei luz nova para não duplicar o cone pintado. A coerência entre pintura e plataformas é o ponto a avaliar.
3. **Vidro/moldura lisos** na pintura, como já registrado na avaliação das camadas.
4. **Pausa local de 80 ms, estralo, tropeço e retry** continuam sem prova de runtime — pertencem à T06.2/3.

## Proximo passo decidido em 13/11/2026

O usuario pediu **arte mais larga em escala domestica**. A especificacao dos tres paineis (1920x1080 a 144 px/m, rodape 1,10 m, janela 2,40 x 1,40 m) esta em [PROMPTS-CAMADAS-V2.md](PROMPTS-CAMADAS-V2.md). O componente ja aceita `InstallPanels` com varios paineis continuos, entao a integracao nao exige reescrita.

## Como reproduzir

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\sergi\repos\UNITY-YunaLegend' -executeMethod YunaLegend.Illustrated25D.Environment.Editor.RoomT054Capture.Run -t054Output 'Evidence/25d/T05.4/capture' -logFile 'Evidence/25d/T05.4/capture.log'
```
