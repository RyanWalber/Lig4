using System;
using System.Net;
using System.Net.Sockets;
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

    private bool souHost = false;
    private bool conectado = false;
    private int meuIdJogador = 0;
    public Lig4Manager gameManager;

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
                fluxo = cliente.GetStream();
                conectado = true;
                meuIdJogador = 1;

                textoStatus.text = "Conectado! Carregando jogo...";
                SceneManager.LoadScene(nomeCenaJogo);
            }
        }

        if (conectado && fluxo != null && fluxo.DataAvailable)
        {
            byte[] buffer = new byte[1];
            int lidos = fluxo.Read(buffer, 0, buffer.Length);
            if (lidos > 0)
            {
                int col = buffer[0];
                if (gameManager != null)
                {
                    gameManager.ReceberJogadaInimiga(col);
                }
            }
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
            cliente.Connect(ip, 7777);
            fluxo = cliente.GetStream();
            conectado = true;
            meuIdJogador = 2;

            textoStatus.text = "Conectado! Carregando jogo...";
            SceneManager.LoadScene(nomeCenaJogo);
        }
        catch (Exception e)
        {
            textoStatus.text = "Erro: " + e.Message;
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
        if (fluxo != null) fluxo.Close();
        if (cliente != null) cliente.Close();
        if (servidor != null) servidor.Stop();
    }
}