# T05.4 — camadas recebidas: avaliação técnica

Sessão `2026-11-13-copilot-t053-b29a5ec7`. Arquivos gerados externamente pelo usuário a partir de [PROMPTS-CAMADAS.md](PROMPTS-CAMADAS.md) e salvos em `source/`. **Recebidos e medidos; não importados na Unity e não aprovados.**

## Medições reais (não estimadas)

| Arquivo | Tamanho | Alpha | Medida-chave |
| --- | --- | --- | --- |
| `layer0-far.png` | 1536 × 768 | opaco (255–255) | **linha de piso em `y = 696`** — idêntica à especificada (696) |
| `layer1-mid.png` | 1536 × 768 | real 0–255, 5,09% visível | bases dos três objetos em `y = 695` (1 px do alvo 696) |
| `layer2-near.png` | 1536 × 768 | real 0–210, 12,91% visível | alpha **0** no corredor 14–86% |

Objetos da L1: estante **2,2–15,6%**, banquinho **51,0–56,5%**, caixa **86,0–98,6%** da largura — dentro das faixas pedidas. Cor média do opaco `(42,39,53)` — índigo escuro, atmosférico.

L2: cor média `(25,32,44)`, cortina 0–693 px, elemento direito 348–767 px. Opacidade máxima 210/255 = **0,82**, dentro do limite 0,85.

L0: janela detectada entre **25,0% e 56,1%** da largura (pedido: 18–39%). Céu/lua visíveis, cone de luar descendo para a direita, rodapé, piso de tábuas e tapete. Nenhum personagem, nenhuma plataforma, nenhum texto.

## Veredito de qualidade

**Aceitável como candidata.** Coerente com a assinatura pintada, sem artefatos de recorte, sem franja opaca, com alpha real e medido nas camadas que precisam dele. A linha de piso coincidir exatamente com o contrato é o ponto mais importante e ele passou.

### Desvios registrados

1. **Dimensão 1536 × 768** em vez de 2048 × 1024. Não afeta o mundo: importo com **PPU 72**, o que cobre exatamente 21,3333 × 10,6667 m sem reamostragem. Em 720p o fundo sobe 1,33× — perda leve de nitidez, aceitável para camada de fundo.
2. **Janela em 25–56%** em vez de 18–39%: deslocada ~7–17% para a direita e mais larga. A composição continua legível (janela à esquerda do centro, luar caindo para a direita), mas a poça de luar invade o corredor central.
3. **Paleta de L0** mais violeta/rosa (flores e bandeirinhas) do que os hex prescritos; mantém índigo + acentos carmim, sem sépia nem clarão.
4. **L2**: o elemento direito vai até a borda inferior e é mais alto que os 55% previstos; como é borrão de primeiro plano, não compromete.
5. **L0 é pintura lisa em algumas áreas** (vidro, madeira): a pincelada aparece mais no papel de parede e na cortina do que nos caixilhos. Só a captura em escala real dirá se lê como pintura ou como render suave.

## Decisão pendente

Nada aqui está importado. Antes de montar o rig, é preciso escolher:

- **seguir com estas camadas** (importar com PPU 72 e âncora em `y = 696`), ou
- **regerar apenas a L0** com a janela em 18–39%, mantendo L1/L2 que já passaram.

## Arquivos

- `source/layer0-far.png` — SHA-256 `b78eb11c…b6e1a0`
- `source/layer1-mid.png` — SHA-256 `2093e587…3fad`
- `source/layer2-near.png` — SHA-256 `1a60e8d8…2065`
- `manifest.json` — medidas, hashes, desvios e método de detecção da linha de piso


## Versao 2 em preparacao (13/11/2026)

O veredito humano pediu arte mais larga em escala domestica. Especificacao em [PROMPTS-CAMADAS-V2.md](PROMPTS-CAMADAS-V2.md): tres paineis 1920x1080 a 144 px/m formando 40 m, rodape 1,10 m, janela 2,40 x 1,40 m. `RoomBackgroundT054.InstallPanels` ja suporta os tres paineis.
