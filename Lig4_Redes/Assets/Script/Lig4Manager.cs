using UnityEngine;

public class Lig4Manager : MonoBehaviour
{
    private const int COLUNAS = 7;
    private const int LINHAS = 6;
    private int[,] tabuleiro = new int[COLUNAS, LINHAS];
    private int jogadorAtual = 1;
    private bool jogoFinalizado = false;

    [Header("Configurações Visuais")]
    public GameObject prefabJogador1; // Peça Vermelha
    public GameObject prefabJogador2; // Peça Amarela
    public Transform[] colunasBotoes; // Os 7 botões
    
    [Header("Ajuste de Posição")]
    public float yInicial; // Altura da primeira linha lá de baixo
    public float espacamentoY; // Distância entre uma linha e outra

    void Start()
    {
        IniciarNovoJogo();
    }

    public void IniciarNovoJogo()
    {
        for (int x = 0; x < COLUNAS; x++)
        {
            for (int y = 0; y < LINHAS; y++)
            {
                tabuleiro[x, y] = 0;
            }
        }
        jogadorAtual = 1;
        jogoFinalizado = false;
        Debug.Log("Jogo Iniciado! Turno do Jogador 1.");
    }

    public void TentarJogar(int coluna)
    {
        if (jogoFinalizado || coluna < 0 || coluna >= COLUNAS) return;

        for (int y = 0; y < LINHAS; y++)
        {
            if (tabuleiro[coluna, y] == 0)
            {
                tabuleiro[coluna, y] = jogadorAtual;
                
                // Cria a pecinha colorida na tela!
                CriarPecaNaTela(coluna, y);

                if (VerificarVitoria(coluna, y))
                {
                    jogoFinalizado = true;
                    Debug.Log($"VENCEDOR: Jogador {jogadorAtual}!");
                    return;
                }

                jogadorAtual = (jogadorAtual == 1) ? 2 : 1;
                return;
            }
        }
        Debug.Log("Coluna cheia!");
    }

    private void CriarPecaNaTela(int col, int lin)
    {
        GameObject prefabUsar = (jogadorAtual == 1) ? prefabJogador1 : prefabJogador2;

        // Pega a posição X do botão clicado e calcula a altura Y da linha
        float posX = colunasBotoes[col].position.x;
        float posY = yInicial + (lin * espacamentoY);
        
        // O -1 no Z serve para a peça ficar na frente do tabuleiro azul
        Vector3 posicaoFinal = new Vector3(posX, posY, -1); 

        Instantiate(prefabUsar, posicaoFinal, Quaternion.identity);
    }

    private bool VerificarVitoria(int col, int lin)
    {
        return ChecarDirecao(col, lin, 1, 0) || 
               ChecarDirecao(col, lin, 0, 1) || 
               ChecarDirecao(col, lin, 1, 1) || 
               ChecarDirecao(col, lin, 1, -1); 
    }

    private bool ChecarDirecao(int col, int lin, int dirX, int dirY)
    {
        int contagem = 1;
        for (int i = 1; i <= 3; i++)
        {
            int c = col + (dirX * i);
            int l = lin + (dirY * i);
            if (c >= 0 && c < COLUNAS && l >= 0 && l < LINHAS && tabuleiro[c, l] == jogadorAtual)
                contagem++;
            else
                break;
        }
        for (int i = 1; i <= 3; i++)
        {
            int c = col - (dirX * i);
            int l = lin - (dirY * i);
            if (c >= 0 && c < COLUNAS && l >= 0 && l < LINHAS && tabuleiro[c, l] == jogadorAtual)
                contagem++;
            else
                break;
        }
        return contagem >= 4;
    }
}