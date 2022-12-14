using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;

public class MapDesigner : MonoBehaviour
{
    public BezierPath path;
    VertexPath Vpath;
    public Transform CamRig;
    public static bool changing = false;
    int index = 0;
    float powX;
    float powZ;
    float powY;

    public Image Fade;
    public GameObject PublishPanel;
    public GameObject Settings;
    public InputField MapName;
    public GameObject Cons;
    public Text ConsText;
    public Dropdown ObsType;
    bool LockChange = false;
    float ObsPow = 0;
    public InputField[] settings;
    public float[] settingVals;
    int[] SelectState = new int[20];
    float[] SpeedState = new float[20];
    public InputField speed;
    public Object obs;
    GameObject obsObj = null;
    Vector3[] ObsPos = new Vector3[20];
    int count = -1;

    void Start()
    {
        path = FindObjectOfType<PathCreator>().bezierPath;
        for (int i = 0; i < 20; i++)
        {
            ObsPos[i] = path.GetPoint(i * 3);
            ObsPos[i].y += 2.3f;
        }
        changing = true;
        StartCoroutine(FadeImage(Fade, true, 0));
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            changing = true;
            powX = -1;
            powY = 0;
            powZ = 0;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            changing = true;
            powX = 1;
            powY = 0;
            powZ = 0;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            changing = true;
            powX = 0;
            powY = 0;
            powZ = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            changing = true;
            powX = 0;
            powY = 0;
            powZ = -1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            changing = true;
            powX = 0;
            powY = 1;
            powZ = 0;
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            changing = true;
            powX = 0;
            powY = -1;
            powZ = 0;
        }
        else
        {
            changing = false;
        }
        if (changing && index > 1 && !LockChange)
        {
            float newX = path.GetPoint(index * 3).x + powX;
            float newY = path.GetPoint(index * 3).y + powY;
            float newZ = path.GetPoint(index * 3).z + powZ;
            Vector3 pos = new Vector3(newX, newY, newZ);
            path.MovePoint(index * 3, pos);
            ObsPos[index] = new Vector3(newX, newY + 2.3f, newZ);
            if (obsObj != null)
            {
                obsObj.transform.position = pos;
            }
            count = 0;
        }
        if (ObsType.value == 1 && index >= 1 && index < 19)
        {
            if (obsObj == null)
            {
                obsObj = (GameObject)Instantiate(obs);
                obsObj.transform.position = ObsPos[index];
                count = 0;
                setRot();
            }
            if (!changing)
            {
                obsObj.transform.position += obsObj.transform.right * ObsPow;
                setRot();
            }
            ObsPos[index] = obsObj.transform.position;
        }
        else if (obsObj != null)
        {
            Destroy(obsObj);
            obsObj = null;
        }
    }

    public void ChangePoint(int op)
    {
        if (op == 1)
        {
            if (index == 19)
                return;
        }
        else
        {
            if (index == 0)
                return;
        }
        index += op;
        ObsType.value = SelectState[index];
        if (SpeedState[index] == 0)
            speed.text = "";
        else
            speed.text = SpeedState[index].ToString();
        CamRig.GetComponent<CameraController>().enabled = false;
        CamRig.position = new Vector3(CamRig.position.x, CamRig.position.y, path.GetPoint(index * 3).z);
        CamRig.GetComponent<CameraController>().newPosition = CamRig.position;
        CamRig.GetComponent<CameraController>().enabled = true;
    }
    void setRot ()
    {
        if (count != -1)
        {
            Vpath = FindObjectOfType<PathCreator>().path;
            count = 0;
            while (!Approximately(path.GetPoint(index * 3).x, Vpath.GetPointAtDistance(count).x) || !Approximately(path.GetPoint(index * 3).y, Vpath.GetPointAtDistance(count).y) || !Approximately(path.GetPoint(index * 3).z, Vpath.GetPointAtDistance(count).z))
            {
                count++;
            }
            obsObj.transform.rotation = Quaternion.Euler(new Vector3(0.0f, Vpath.GetRotationAtDistance(count).eulerAngles.y, 0.0f));
            count = -1;
        }
    }
    bool Approximately(float valueA, float valueB)
    {
        return Mathf.Abs(valueA - valueB) < 1.0f;
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

    public void Exit()
    {
        StartCoroutine(FadeImage(Fade, false, 0));
    }
    public void Publish()
    {
        if (MapName.text != "")
        {
            ConsText.text = "Please Wait...";
            Cons.SetActive(true);
            Vector3[] points = new Vector3[20];
            for (int i = 0; i < 20; i++)
            {
                points[i] = path.GetPoint(i * 3);
            }
            short result = SqlScript.publishMap(MapName.text, points, settingVals, SelectState, SpeedState, ObsPos);
            if (result == 0)
            {
                ShowError("Couldn't connect to the database, Please Try again...");
            }
            else
            {
                Cons.SetActive(false);
                Exit();
                ShowHidePanel(false);
            }
        }
    }
    public void ShowError(string msg)
    {
        ConsText.text = msg;
        StartCoroutine(err());
    }
    IEnumerator err()
    {
        yield return new WaitForSeconds(2);
        Cons.SetActive(false);
    }
    public void ShowHidePanel(bool state)
    {
        LockChange = state;
        PublishPanel.SetActive(state);
    }
    public void ShowHideSettings(bool state)
    {
        Settings.SetActive(state);
    }
    public void Done()
    {
        for (int i = 0; i < settings.Length; i++)
        {
            if (settings[i].text != "")
                settingVals[i] = float.Parse(settings[i].text);
        }
    }
    public void ValueChanged()
    {
        SelectState[index] = ObsType.value;
        if (ObsType.value == 2)
        {
            if (SpeedState[index] == 0.0f)
            {
                SpeedState[index] = 1.0f;
            }
            speed.text = SpeedState[index].ToString();
        }
    }
    public void SpeedChanged()
    {
        if (speed.text != "" && speed.text != ".")
            SpeedState[index] = float.Parse(speed.text);
        else
            SpeedState[index] = 1.0f;
    }
    public void ObsDown(float obspow)
    {
        if (ObsType.value == 1)
        {
            ObsPow = obspow;
        }
    }
    public void ObsUp()
    {
        ObsPow = 0;
    }
}
