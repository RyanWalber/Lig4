using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Lig4Network : MonoBehaviour
{
    public static Lig4Network Instancia { get; private set; }

    [Header("UI")]
    public GameObject painelConexao;
    public TMP_InputField inputIP;
    public Button botaoHost;
    public Button botaoCliente;
    public TMP_Text textoStatus;

    [Header("Cena")]
    public string nomeCenaJogo = "SampleScene";

    private TcpListener servidor;
    private TcpClient cliente;
    private NetworkStream fluxo;
    private Thread threadEscuta;
    private bool rodandoThread = false;

    private bool souHost = false;
    private bool conectado = false;
    private int meuIdJogador = 0;
    public Lig4Manager gameManager;

    // TRAVA DE MEMÓRIA PARA A UNITY ATUALIZAR A TELA IMEDIATAMENTE
    private readonly object travaMemoria = new object();
    private bool temJogadaPendente = false;
    private int colunaPendente = -1;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        botaoHost.onClick.AddListener(ConfigurarHost);
        botaoCliente.onClick.AddListener(ConfigurarCliente);
        textoStatus.text = "Escolha Host ou Cliente";
        painelConexao.SetActive(true);
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
        FecharConexoes();
    }

    void Update()
    {
        if (souHost && !conectado && servidor != null)
        {
            if (servidor.Pending())
            {
                cliente = servidor.AcceptTcpClient();
                cliente.NoDelay = true;
                fluxo = cliente.GetStream();
                conectado = true;
                meuIdJogador = 1;

                IniciarThreadEscuta();

                textoStatus.text = "Conectado! Carregando jogo...";
                SceneManager.LoadScene(nomeCenaJogo);
            }
        }

        bool processarAgora = false;
        int col = -1;

        // Verifica de forma segura se a Thread recebeu algo
        lock (travaMemoria)
        {
            if (temJogadaPendente)
            {
                processarAgora = true;
                col = colunaPendente;
                temJogadaPendente = false; // Limpa a fila
            }
        }

        // Aplica na Unity
        if (processarAgora && gameManager != null)
        {
            gameManager.ReceberJogadaInimiga(col);
        }
    }

    void ConfigurarHost()
    {
        try
        {
            servidor = new TcpListener(IPAddress.Any, 7777);
            servidor.Start();
            souHost = true;
            textoStatus.text = "Aguardando jogador...";
            botaoHost.interactable = false;
            botaoCliente.interactable = false;
        }
        catch (Exception e)
        {
            textoStatus.text = "Erro: " + e.Message;
        }
    }

    void ConfigurarCliente()
    {
        try
        {
            string ip = string.IsNullOrEmpty(inputIP.text) ? "127.0.0.1" : inputIP.text;
            cliente = new TcpClient();
            cliente.NoDelay = true;
            cliente.Connect(ip, 7777);
            fluxo = cliente.GetStream();
            conectado = true;
            meuIdJogador = 2;

            IniciarThreadEscuta();

            textoStatus.text = "Conectado! Carregando jogo...";
            SceneManager.LoadScene(nomeCenaJogo);
        }
        catch (Exception e)
        {
            textoStatus.text = "Erro: " + e.Message;
        }
    }

    void IniciarThreadEscuta()
    {
        rodandoThread = true;
        threadEscuta = new Thread(EscutarRede);
        threadEscuta.IsBackground = true;
        threadEscuta.Start();
    }

    void EscutarRede()
    {
        byte[] buffer = new byte[1];
        while (rodandoThread && conectado && fluxo != null)
        {
            try
            {
                if (fluxo.CanRead)
                {
                    int lidos = fluxo.Read(buffer, 0, buffer.Length);
                    if (lidos > 0)
                    {
                        // Salva na memória de forma blindada para o Update ler
                        lock (travaMemoria)
                        {
                            colunaPendente = buffer[0];
                            temJogadaPendente = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                break;
            }
            Thread.Sleep(10);
        }
    }

    void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (cena.name == nomeCenaJogo)
        {
            gameManager = FindFirstObjectByType<Lig4Manager>();
            if (gameManager != null)
            {
                gameManager.scriptRede = this;
                gameManager.IniciarNovoJogo();
            }
        }
    }

    public void EnviarJogada(int coluna)
    {
        if (conectado && fluxo != null)
        {
            try
            {
                byte[] dado = new byte[] { (byte)coluna };
                fluxo.Write(dado, 0, dado.Length);
                fluxo.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError("Erro ao enviar: " + e.Message);
            }
        }
    }

    public bool MinhaVez()
    {
        if (!conectado || gameManager == null) return false;
        return gameManager.ObterJogadorAtual() == meuIdJogador;
    }

    void FecharConexoes()
    {
        rodandoThread = false;
        if (threadEscuta != null && threadEscuta.IsAlive) threadEscuta.Abort();
        if (fluxo != null) fluxo.Close();
        if (cliente != null) cliente.Close();
        if (servidor != null) servidor.Stop();
    }
}