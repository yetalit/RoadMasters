using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static string UserName = "";
    public GameObject[] Windows;
    public GameObject Cons;
    public Text ConsText;

    public InputField log_user;
    public InputField log_pass;
    public InputField sig_user;
    public InputField sig_pass;

    public Text NameText;

    public AudioSource sound;
    public AudioSource soundHover;

    public Image Fade;

    string[] mapNames;
    string[] timeNames;
    string[] MymapNames;
    bool showmaps = false;
    bool showtimes = false;
    bool showMymaps = false;

    public static List<Vector3> points = new List<Vector3>();
    public static List<Vector3> obsPos = new List<Vector3>();
    public static List<int> selstate = new List<int>();
    public static List<float> speedstate = new List<float>();
    public static List<float> settingVals = new List<float>();
    public static int MapIndex = 0;
    public static double PlayerBestTime = 0.0;

    short version = 1;
    void Start()
    {
        Screen.fullScreen = false;
        StartCoroutine(FadeImage(Fade, true, 0));
        if (UserName != "")
        {
            WinCloser(1);
            NameText.text = UserName;
            WinOpener(3);
        }
        ConsText.text = "Please Wait...";
        Cons.SetActive(true);
        short ver = SqlScript.getVersion();
        if (ver != 0)
        {
            if (ver != version)
                ConsText.text = "Please Update the game to the newest version...";
            else
                Cons.SetActive(false);
        }
        else
        {
            ConsText.text = "Couldn't connect to the database, Please Try again...";
        }
    }
    public void PlayGame()
    {
        ConsText.text = "Please Wait...";
        showmaps = false;
        points.Clear();
        obsPos.Clear();
        selstate.Clear();
        speedstate.Clear();
        settingVals.Clear();
        PlayerBestTime = 0.0;
        Cons.SetActive(true);
        short res = SqlScript.getMapData();
        if (res == 0)
        {
            ShowError("Couldn't connect to the database, Please Try again...", 1);
        } else
        {
            Cons.SetActive(false);
            StartCoroutine(FadeImage(Fade, false, 2));
        }
    }
    public void ShowMaps ()
    {
        ConsText.text = "Please Wait...";
        Cons.SetActive(true);
        mapNames = SqlScript.getMaps();
        if (mapNames.Length > 0 && mapNames[0] == "0")
        {
            ShowError("Couldn't connect to the database, Please Try again...", 0);
        }
        else
        {
            Cons.SetActive(false);
            Windows[3].SetActive(false);
            Windows[4].SetActive(true);
            showmaps = true;
        }
    }
    public void MapManage()
    {
        ConsText.text = "Please Wait...";
        Cons.SetActive(true);
        MymapNames = SqlScript.getMyMaps();
        if (MymapNames.Length > 0 && MymapNames[0] == "0")
        {
            ShowError("Couldn't connect to the database, Please Try again...", 0);
        }
        else
        {
            Cons.SetActive(false);
            Windows[5].SetActive(true);
            showMymaps = true;
        }
    }
    public void BackMap ()
    {
        showMymaps = false;
        Windows[5].SetActive(false);
    }
    void ShowTimes()
    {
        ConsText.text = "Please Wait...";
        showmaps = false;
        Cons.SetActive(true);
        timeNames = SqlScript.getTimes();
        if (timeNames.Length > 0 && timeNames[0] == "0")
        {
            ShowError("Couldn't connect to the database, Please Try again...", 1);
        }
        else
        {
            Cons.SetActive(false);
            showtimes = true;
            showmaps = false;
        }
    }
    public void Login()
    {
        if (log_user.text != "" && log_pass.text != "")
        {
            ConsText.text = "Please Wait...";
            Cons.SetActive(true);
            short log = SqlScript.LogUser(log_user.text, log_pass.text);
            if (log != 0)
            {
                if (log == 1)
                {
                    UserName = log_user.text;
                    log_user.text = log_pass.text = "";
                    Cons.SetActive(false);
                    WinCloser(1);
                    NameText.text = UserName;
                    WinOpener(3);
                }
                else
                {
                    ShowError("User Name or Password is wrong!", 0);
                }
            }
            else
                ShowError("Couldn't connect to the database, Please Try again...", 0);
        }
    }
    public void SignUp ()
    {
        if (sig_user.text != "" && sig_pass.text != "")
        {
            ConsText.text = "Please Wait...";
            Cons.SetActive(true);
            short sign = SqlScript.SignUser(sig_user.text, sig_pass.text);
            if (sign != 0)
            {
                if (sign == 1)
                {
                    UserName = sig_user.text;
                    sig_user.text = sig_pass.text = "";
                    Cons.SetActive(false);
                    WinCloser(2);
                    NameText.text = UserName;
                    WinOpener(3);
                }
                else
                {
                    ShowError("User Name is already taken!", 0);
                }
            }
            else
                ShowError("Couldn't connect to the database, Please Try again...", 0);
        }
    }

    public void LogOut ()
    {
        UserName = "";
        WinCloser(3);
        WinOpener(0);
    }
    public void ShowError (string msg, int state)
    {
        ConsText.text = msg;
        StartCoroutine(err(state));
    }
    IEnumerator err (int state)
    {
        yield return new WaitForSeconds(2);
        if (state == 1)
        {
            showmaps = true;
        }
        else if (state == 2)
        {
            showMymaps = true;
        }
        Cons.SetActive(false);
    }
    public void PlaySound(AudioClip audio)
    {
        sound.clip = audio;
        sound.Play();
    }
    public void PlayHover()
    {
        soundHover.Play();
    }
    public void WinOpener(int index)
    {
        Windows[0].SetActive(false);
        Windows[index].SetActive(true);
    }
    public void WinCloser (int index)
    {
        int mainMenu = 0;
        if (index == 4)
        {
            if (showmaps)
                showmaps = false;
            else
            {
                BackTimes();
                return;
            }
            mainMenu = 3;
        }
        Windows[index].SetActive(false);
        Windows[mainMenu].SetActive(true);
    }

    Vector2 scrollPosition;
    public Font font;

    void OnGUI()
    {
        GUI.skin.box.fontSize = Screen.width / 32;
        GUI.skin.box.font = font;
        GUI.skin.button.fontSize = Screen.width / 38;
        GUI.skin.button.font = font;
        GUI.skin.box.fixedHeight = Screen.height / 9.82f;

        GUILayout.BeginArea(new Rect(Screen.width / 2 - Screen.width / 2.64f, Screen.height / 2 - Screen.height / 2.8f, Screen.width / 1.32f, Screen.height / 1.35f));
        scrollPosition = GUILayout.BeginScrollView(
        scrollPosition, GUILayout.Width(Screen.width / 1.32f), GUILayout.Height(Screen.height / 1.35f));
        if (showmaps)
        {
            for (int i = 0; i < mapNames.Length; i++)
            {
                GUILayout.Box(mapNames[i]);
                if (GUILayout.Button("Best Times"))
                {
                    MapIndex = int.Parse(mapNames[i].Split('#')[1]);
                    ShowTimes();
                }
                if (GUILayout.Button("Play"))
                {
                    WinCloser(4);
                    MapIndex = int.Parse(mapNames[i].Split('#')[1]);
                    PlayGame();
                }
            }
        }
        if (showtimes)
        {
            for (int i = 0; i < timeNames.Length / 2; i++)
            {
                GUILayout.Box('[' + (i + 1).ToString() + "]  " + timeNames[i * 2]);
                GUILayout.Box(timeNames[i * 2 + 1] + " s");
            }
        }
        if (showMymaps)
        {
            if (GUILayout.Button("New Map"))
            {
                BackMap();
                StartCoroutine(FadeImage(Fade, false, 1));
            }
            for (int i = 0; i < MymapNames.Length; i++)
            {
                GUILayout.Box(MymapNames[i]);
                if (GUILayout.Button("Delete"))
                {
                    MapIndex = int.Parse(MymapNames[i].Split('#')[1]);
                    ConsText.text = "Please Wait...";
                    showMymaps = false;
                    Cons.SetActive(true);
                    short res = SqlScript.delMap();
                    if (res != 0)
                    {
                        if (res == 1)
                        {
                            ShowError("Selected map deleted successfully!", 0);
                            BackMap();
                        }
                        else
                        {
                            ShowError("You can't delete this map! It has been more than 24 hours", 2);
                        }
                    } 
                    else
                    {
                        ShowError("Couldn't connect to the database, Please Try again...", 2);
                    }
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void BackTimes ()
    {
        showtimes = false;
        showmaps = true;
    }

    IEnumerator FadeImage(Image img, bool fadeAway, int scene)
    {
        img.gameObject.SetActive(true);
        // fade from opaque to transparent
        if (fadeAway)
        {
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(0, 0, 0, i);
                yield return null;
            }
            img.gameObject.SetActive(false);
        }
        // fade from transparent to opaque
        else
        {
            // loop over 1 second
            for (float i = 0; i <= 1; i += Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(0, 0, 0, i);
                yield return null;
            }
            SceneManager.LoadScene(scene);
        }
    }
}
