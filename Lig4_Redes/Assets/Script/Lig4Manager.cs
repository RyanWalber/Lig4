using UnityEngine;

public class Lig4Manager : MonoBehaviour
{
    private const int COLUNAS = 7;
    private const int LINHAS = 6;
    private int[,] tabuleiro = new int[COLUNAS, LINHAS];
    private int jogadorAtual = 1;
    private bool jogoFinalizado = false;

    [Header("Configurações Visuais")]
    public GameObject prefabJogador1; 
    public GameObject prefabJogador2; 
    public Transform[] colunasBotoes; 
    
    [Header("Ajuste de Posição")]
    public float yInicial; 
    public float espacamentoY; 

    [Header("Rede")]
    public Lig4Network scriptRede;

    void Start()
    {
        if (scriptRede == null)
        {
            scriptRede = Lig4Network.Instancia;
            if (scriptRede != null)
            {
                scriptRede.gameManager = this;
            }
        }
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
    }

    public void TentarJogar(int coluna)
    {
        if (jogoFinalizado || coluna < 0 || coluna >= COLUNAS) return;

        if (scriptRede != null && !scriptRede.MinhaVez())
        {
            return;
        }

        FazerJogada(coluna);

        if (scriptRede != null)
        {
            scriptRede.EnviarJogada(coluna);
        }
    }

    public void ReceberJogadaInimiga(int coluna)
    {
        if (jogoFinalizado || coluna < 0 || coluna >= COLUNAS) return;
        FazerJogada(coluna);
    }

    private void FazerJogada(int coluna)
    {
        for (int y = 0; y < LINHAS; y++)
        {
            if (tabuleiro[coluna, y] == 0)
            {
                tabuleiro[coluna, y] = jogadorAtual;
                
                CriarPecaNaTela(coluna, y);

                if (VerificarVitoria(coluna, y))
                {
                    jogoFinalizado = true;
                    return;
                }

                jogadorAtual = (jogadorAtual == 1) ? 2 : 1;
                return;
            }
        }
    }

    private void CriarPecaNaTela(int col, int lin)
    {
        GameObject prefabUsar = (jogadorAtual == 1) ? prefabJogador1 : prefabJogador2;
        
        float posX = colunasBotoes[col].position.x;
        float posY = yInicial + (lin * espacamentoY);
        
        Vector3 posicaoFinal = new Vector3(posX, posY, -1); 

        Instantiate(prefabUsar, posicaoFinal, Quaternion.identity);
    }

    public int ObterJogadorAtual()
    {
        return jogadorAtual;
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