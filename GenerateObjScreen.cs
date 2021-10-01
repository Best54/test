using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GenerateObjScreen : MonoBehaviour
{
    const int timeAlarm = 15;

    [SerializeField] private GameObject[] figurePrefab = null; //поле доступно в инспекторе, но недоступно для других скриптов    
    [SerializeField] private Text textScore = null;
    [SerializeField] private Text textPCoin = null;
    [SerializeField] private Text textTimer = null;
    [SerializeField] private QuestLoad questLoad = null;

    public int playerCoin;
    public float timeNMax { get; private set; } = 2.5f;
    public float timeNMin { get; private set; } = .5f;
    public int maxFigurePerWidth = 13;
    public int minFigurePerWidth = 2;
    public int sizeFigurePrefab = 650;
    public int maxChislo { get; private set; } = 10;
    public int zagadanChislo { get; private set; }
    public float timeGame { get; private set; } = 0;

    public Camera cam;
    public int countFigure;    

    private List<GameObject> _figure = new List<GameObject>();
    private Color[] _colorFig = new Color[] { Color.white, Color.red, Color.blue, Color.green, Color.cyan, Color.yellow, Color.magenta};
    private float _sizeFigureX, _sizeFigureY, _maxScaleFigure, _minScaleFigure;
    private Vector2 _sizeScreen = new Vector2();
    private float _tekWidthScreen = 0;
    private float _sizeGUITop = 1.3f;
    private bool _isStarted = false;
    private bool _startTimer = false;
    private int _score = 0;
    private int _tekLevelGame = 0;    

    private void Awake()
    {
        Messenger<int>.AddListener(MyEvent.ACTION_GET, ActionGet);
        Messenger<int>.AddListener(MyEvent.START, ReStart);
        Messenger.AddListener(MyEvent.PAUSE, PauseGame);
    }
    private void OnDestroy()
    {
        Messenger<int>.RemoveListener(MyEvent.ACTION_GET, ActionGet);
        Messenger<int>.RemoveListener(MyEvent.START, ReStart);
        Messenger.RemoveListener(MyEvent.PAUSE, PauseGame);
    }

    private void ActionGet(int value)
    {        
    //if (value == zagadanChislo)
    //{
        _score++;
        //}        
        textScore.text = "Очки: " + _score;
        if (_score >= questLoad.quest[_tekLevelGame].score)
        {
            Messenger<bool>.Broadcast(MyEvent.WIN, true);
        }
    }

    private void WinLose()
    {
        if (_score >= questLoad.quest[_tekLevelGame].score)
        {
            Messenger<bool>.Broadcast(MyEvent.WIN, true);
        }
        else
        {
            Messenger<bool>.Broadcast(MyEvent.WIN, false);
        }
    }

    public void ReStart(int i)
    {
        _isStarted = true;
        _startTimer = false;
        timeGame = 0;
        _score = 0;
        _tekLevelGame = i;
        textScore.text = "Очки: " + _score;
        textPCoin.text = playerCoin.ToString();
        textTimer.text = "";
        if (questLoad.quest[_tekLevelGame].time > 0)
        {
            StartTimer(questLoad.quest[_tekLevelGame].time);
        }
    }

    private void PauseGame()
    {
        _isStarted = !_isStarted;
    }

    private void Start()
    {
        playerCoin = 0; //загружать
        SizeScreen();
        //CreateLevel();
        zagadanChislo = Random.Range(0, maxChislo);
    }

    private void CreateLevel()
    {
        while (_figure.Count < countFigure)
        {
            _figure.Add(CreateFigure());
        }
    }

    private void SizeScreen()
    {
        _sizeScreen = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight));
        _tekWidthScreen = cam.pixelWidth;
        _maxScaleFigure = _tekWidthScreen / (minFigurePerWidth * sizeFigurePrefab);
        _minScaleFigure = _tekWidthScreen / (maxFigurePerWidth * sizeFigurePrefab);
    }
    
    private void StartTimer(int time)
    {
        timeGame = time;
        _startTimer = true;
    }

    private void TicTimer()
    {
        timeGame -= Time.deltaTime;        

        int minutes = (int)timeGame / 60;
        int seconds = (int)timeGame % 60;

        if (timeGame < timeAlarm)
        {
            if (textTimer.color != Color.red)
            {
                textTimer.color = Color.red;
                textTimer.fontSize += 10;
            }            
        }
        if (seconds < 10)
        {            
            textTimer.text = minutes + ":0" + seconds;
        }
        else
        {
            textTimer.text = minutes + ":" + seconds;
        }
        if (timeGame <= 0)
        {
            _isStarted = false;
            WinLose();
        }
    }

    void Update()
    {
        if (!_isStarted)
        {
            DeleteFigure();
            return;
        }
        if (cam.pixelWidth != _tekWidthScreen)
        {
            SizeScreen();
        }
        _figure.RemoveAll(x => x == null);
        while (_figure.Count < countFigure)
        {
            _figure.Add(CreateFigure());
        }
        if (_startTimer) TicTimer();
    }    

    private GameObject ScaleTransform(GameObject temp)
    {
        float scaleFigure;

        scaleFigure = Random.Range(_minScaleFigure, _maxScaleFigure);
        temp.transform.localScale = new Vector3(scaleFigure, scaleFigure, scaleFigure);
        var bounds = temp.GetComponent<Renderer>().bounds.size;
        if (bounds != null)
        {
            _sizeFigureX = bounds.x / 2.2f;
            _sizeFigureY = bounds.y / 2.2f;
        }
        temp.transform.position = new Vector2(Random.Range(-_sizeScreen.x + _sizeFigureX, _sizeScreen.x - _sizeFigureX), Random.Range(-_sizeScreen.y + _sizeFigureY, _sizeScreen.y - _sizeGUITop - _sizeFigureY)); //случайное расположение
        return temp;
    }

    private GameObject Peresechenie(GameObject temp)
    {
        bool peresek = true;
        int whileExit = 0; //для выхода из бесконечного цикла
        float dist, radius;

        while (peresek)
        {
            whileExit++;
            foreach (GameObject _circ in _figure)
            {
                radius = Mathf.Abs(_circ.GetComponent<Renderer>().bounds.size.x / 2);
                dist = Vector3.Distance(_circ.transform.position, temp.transform.position);
                if (dist < (radius + Mathf.Abs(_sizeFigureX)))
                {
                    peresek = true;
                    temp = ScaleTransform(temp);
                    break;
                }
                else { peresek = false; }
            }
            if (whileExit >= 10000) { countFigure = _figure.Count; Debug.Log("Невозможно создать с данными условиями: установлено количество объектов" + countFigure); break; }
        }
        return temp;
    }

    private GameObject CreateFigure()
    {
        GameObject temp;
        temp = Instantiate(figurePrefab[Random.Range(0,figurePrefab.Length)]) as GameObject; //случайная фигура из префабов
        temp.GetComponent<SpriteRenderer>().color = _colorFig[Random.Range(0, _colorFig.Length)]; //случайный цвет
        temp = ScaleTransform(temp); //случайный размер        
        if (_figure.Count > 0) { temp = Peresechenie(temp); }
        return temp;
    }

    public void DeleteFigure()
    {
        foreach (GameObject del in _figure)
        {
            Destroy(del);
        }
        _figure.Clear();
    }
}
