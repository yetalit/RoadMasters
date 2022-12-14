using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VehiclePhysics;

public class GameManager : MonoBehaviour
{
    public static bool gameStarted = false;

    public Text TimeText;
    double MapTime = 0;
    bool timing = false;
    float lowestPoint = 0.0f;
    public Transform Car;
    bool gameover = false;
    public GameObject FinishPanel;
    public Text finishText;
    public Text finishTime;
    public Text bestTime;
    public Transform[] Triggers;
    public List<int> CPs = new List<int>();
    public GameObject Cons;
    public Text ConsText;
    public Object obs;

    public Image Fade;
    void Start()
    {
        VPVehicleController VC = Car.GetComponent<VPVehicleController>();
        VPCameraController CC = GetComponent<VPCameraController>();
        CC.smoothFollow.distance = 6.5f;
        CC.smoothFollow.height = 2.5f;
        CC.smoothFollow.heightMultiplier = 0.8f;
        gameStarted = true;
        if (MenuManager.PlayerBestTime != 0.0)
            bestTime.text = MenuManager.PlayerBestTime.ToString() + " s";
        else
            bestTime.text = "-";
        StartCoroutine(FadeImage(Fade, true, 0));
        if (MenuManager.settingVals[0] != 0.0f)
            Car.GetComponent<Rigidbody>().mass = MenuManager.settingVals[0];
        if (MenuManager.settingVals[1] != 0.0f)
            VC.steering.maxSteerAngle = MenuManager.settingVals[1];
        if (MenuManager.settingVals[2] != 0.0f)
        {
            VC.brakes.maxBrakeTorque = MenuManager.settingVals[2];
            VC.brakes.handbrakeTorque = MenuManager.settingVals[3];
        }
        if (MenuManager.settingVals[4] != 0.0f)
        {
            VC.engine.torqueCapLimit = MenuManager.settingVals[4];
            VC.engine.maxRpm = MenuManager.settingVals[5];
        }
        StartCoroutine(Design());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            if (!timing)
                timing = true;

        if (timing && !gameover)
        {
            MapTime += Time.deltaTime;
            MapTime = System.Math.Round(MapTime, 5);
            TimeText.text = MapTime.ToString() + " s";
        }

        if (!gameover)
            if (Car.position.y <= lowestPoint)
            {
                finishText.text = "You Failed!";
                GameOver();
            }
    }
    void GameOver ()
    {
        gameover = true;
        Car.GetComponent<Rigidbody>().isKinematic = true;
        finishTime.text = TimeText.text;
        FinishPanel.SetActive(true);
    }
    public void Win ()
    {
        finishText.text = "You Won!";
        GameOver();
        ConsText.text = "Please Wait...";
        Cons.SetActive(true);
        MenuManager.PlayerBestTime = SqlScript.getBestTime(MapTime);
        if (MenuManager.PlayerBestTime == 0.0)
        {
            ShowError("Couldn't connect to the database, Please Try again...");
        }
        else
        {
            bestTime.text = MenuManager.PlayerBestTime.ToString() + " s";
            Cons.SetActive(false);
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
    public void Finish (int scene)
    {
        Car.GetComponent<Rigidbody>().isKinematic = true;
        StartCoroutine(FadeImage(Fade, false, scene));
    }
    IEnumerator Design ()
    {
        BezierPath path = FindObjectOfType<PathCreator>().bezierPath;
        yield return new WaitForFixedUpdate();
        for (int i = 0; i < 20; i++)
        {
            path.MovePoint(i * 3, MenuManager.points[i]);
            if (MenuManager.points[i].y < lowestPoint)
                lowestPoint = MenuManager.points[i].y;
        }
        VertexPath Vpath = FindObjectOfType<PathCreator>().path;
        int count = 0;
        for (int i = 1; i < 20; i++)
        {
            Vector3 pos = new Vector3(MenuManager.points[i].x, MenuManager.points[i].y + 30.0f, MenuManager.points[i].z);
            while (!Approximately(MenuManager.points[i].x, Vpath.GetPointAtDistance(count).x) || !Approximately(MenuManager.points[i].y, Vpath.GetPointAtDistance(count).y) || !Approximately(MenuManager.points[i].z, Vpath.GetPointAtDistance(count).z))
            {
                count++;
            }
            Triggers[i - 1].position = pos;
            Triggers[i - 1].rotation = Quaternion.Euler(new Vector3(0.0f, Vpath.GetRotationAtDistance(count).eulerAngles.y, 0.0f));
            if (MenuManager.selstate[i] != 0 && i < 19)
            {
                GameObject obsObj;
                obsObj = (GameObject)Instantiate(obs);
                obsObj.transform.rotation = Triggers[i - 1].rotation;
                if (MenuManager.selstate[i] == 1)
                {
                    obsObj.transform.position = MenuManager.obsPos[i];
                }
                else
                {
                    pos.y -= 27.7f;
                    obsObj.transform.position = pos;
                    StartCoroutine(SmoothMove(obsObj.transform, obsObj.transform.position + (obsObj.transform.right * 9), MenuManager.speedstate[i] * 0.2f, -1));
                }
            }
        }
        lowestPoint -= 15.0f;
        yield return new WaitForSeconds (0.1f);
        GameObject.Find("Road Mesh Holder").AddComponent<MeshCollider>();
        gameStarted = false;
        StartCoroutine(CountDown());

    }
    IEnumerator CountDown ()
    {
        yield return new WaitForSeconds(5);
        timing = true;
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
    bool Approximately4(float valueA, float valueB)
    {
        return Mathf.Abs(valueA - valueB) < 0.04f;
    }
    bool Approximately(float valueA, float valueB)
    {
        return Mathf.Abs(valueA - valueB) < 1.0f;
    }

    IEnumerator SmoothMove(Transform target, Vector3 TargetPos, float Speed, int p)
    {
        float t = 0.0f;
        if (Speed == 0)
        {
            Speed = 0.00001f;
        }
        while (!Approximately4(target.position.x, TargetPos.x) || !Approximately4(target.position.z, TargetPos.z))
        {
            t += Time.deltaTime * Speed;
            target.position = Vector3.Lerp(target.position, TargetPos, Mathf.SmoothStep(0.0f, 1.0f, t));
            yield return null;
        }
        target.position = TargetPos;
        StartCoroutine(SmoothMove(target, target.position + (target.right * p * 18), Speed, p * -1));
    }
}
