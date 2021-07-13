using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Roto.Control;

public class TestController2 : MonoBehaviour
{
    RotoController controller;
    RotoCalibration calibration;
    bool flag = false;
    float timer = 5;
    public int rotation;
    public string path;
    public int log = 0;
    public int threshold_counter = 1;
    public int route_counter = 1;


    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<RotoController>();
        calibration = FindObjectOfType<RotoCalibration>();

        //calibration.CalibrateChairZero(100);
        //controller.SyncRotoToVirtualRoto(80);
        //controller.EnableFreeMode();
    }

    // Update is called once per frame
    void Update()
    {
        rotation = controller.GetOutputRotation();

        if (Input.GetKey(KeyCode.Alpha1))
        {
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/thresholdRoute.txt";
            StartCoroutine(Threshold());
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/standardRoute.txt";
            StartCoroutine(RandomAuto());
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {

        }





        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            controller.TurnLeftAtSpeed(90, 40);
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            controller.TurnRightAtSpeed(90, 40);
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            Debug.Log("Current angle " + controller.GetOutputRotation());
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            Debug.Log("Performing calibration");
            calibration.CalibrateChairZero(100);
        }

        if (Input.GetKey(KeyCode.Alpha0))
        {
            controller.MoveChairToZero(40);
        }
    }





    /**
     * The 3 set up functions 
    **/
    public string[] ReadText()
    {
        string[] values;

        string text = System.IO.File.ReadAllText(@path);

        //string text = System.IO.File.ReadAllText(@RandomRoute());

        values = text.Split(char.Parse("\n"), char.Parse(","));

        return values;
    }


    //Creates a dictionary with the route number being the key and the route instructions being the value
    public IDictionary<string, List<string>> Dict(string[] values)
    {
        IDictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
        int id = 1;

        for (int i = 0; i < values.Length - 1; i += 4)
        {
            List<string> list = new List<string>
            {
                values[i],
                values[i + 1],
                values[i + 2],
                values[i + 3]
            };

            dictionary.Add(id.ToString(), list);
            id += 1;
        }
        return dictionary;
    }


    //Creates a list of numbers from 1-21 in random order
    public List<int> RandomOrder()
    {
        List<int> ids = new List<int>();

        for (int j = 0; j < 31; j++)
        {
            int Rand = (int)Random.Range(1f, 32f);

            while (ids.Contains(Rand))
            {
                Rand = (int)Random.Range(1f, 32f);
            }

            ids.Add(Rand);
        }
        return ids;
    }



    /**
    * The 2 turning functions 
    **/
    private bool CalcRotatingRight(int angle)
        {
            if (rotation >= angle - 3 && rotation <= angle + 3)
            {
                controller.SetCurrentPositionToZero();
                return true;
            }
            else
            {
                return false;
            }
        }

    private bool CalcRotatingLeft(int angle)
    {
        if (rotation >= (360 - angle) - 3 && rotation <= (360 - angle) + 3)
        {
            controller.SetCurrentPositionToZero();
            return true;
        }
        else
        {
            return false;
        }
    }



    /**
    * This code takes a text file route and randomises the order of the route.
    * The code then loops through the random route and turns the chair accordindly.
    * If the chair is going to pass 180 in one direction then it switches the instructions
    * to then turn in the opposite direction.
    **/
    public IEnumerator RandomDrive()
    {
        IDictionary<string, List<string>> dictionary = Dict(ReadText());
        List<int> ids = RandomOrder();

        //for each value in the random list, look up that value as an id in the route dictionary to get the instructions.
        foreach (int i in ids)
        {
            string id = i.ToString();

            //id[0] = right or left, id[1] = angle, id[2] = speed, id[3] = wait time in seconds
            if (char.Parse(dictionary[id][0]).Equals('R'))
            {
                log -= int.Parse(dictionary[id][1]);
                //Debug.Log("Log: " + log);

                if (log <= -180)
                {
                    controller.TurnLeftAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("OVER -180; CHANGE TO TURNING LEFT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    log += int.Parse(dictionary[id][1])*2;
                    //Debug.Log("Over 180; minus log: " + log);

                    //Debug.Log("Waiting until true");
                    yield return new WaitUntil(() => CalcRotatingLeft(int.Parse(dictionary[id][1])));

                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                }

                else
                {
                    controller.TurnRightAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("TURNING RIGHT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    //Debug.Log("Waiting until true");
                    yield return new WaitUntil(() => CalcRotatingRight(int.Parse(dictionary[id][1])));

                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                }

            }

            else if (char.Parse(dictionary[id][0]).Equals('L'))
            {
                log += int.Parse(dictionary[id][1]);
                //Debug.Log("Log: " + log);

                if (log >= 180)
                {
                    controller.TurnRightAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("OVER 180; CHANGE TO TURNING RIGHT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    log -= int.Parse(dictionary[id][1])*2;
                    //Debug.Log("Over 180; minus log: " + log);

                    //Debug.Log("Waiting until true");
                    yield return new WaitUntil(() => CalcRotatingRight(int.Parse(dictionary[id][1])));

                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                }
               
                else
                {
                    controller.TurnLeftAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("TURNING LEFT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    //Debug.Log("Waiting until true");
                    yield return new WaitUntil(() => CalcRotatingLeft(int.Parse(dictionary[id][1])));

                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                }

            }
            else
            {
                Debug.Log(dictionary[id][0]);
                Debug.Log(dictionary[id][1]);
                Debug.Log(dictionary[id][2]);
                Debug.Log(dictionary[id][3]);
                Debug.Log("Something has gone wrong");
            }
        }
        flag = true;
        Debug.Log("Done rotating");
    }



    /**
     * This code reads in the text file route and instructs the chair on how to complete it
    **/
    public IEnumerator Drive()
    {
        string[] values;
        values = ReadText();

        //for each rotation in the text file, excecute rotation and then wait 5s, value[i] = angle, value[i+1] = speed
        for (int i = 0; i < values.Length - 1; i += 4)
        {
            if (char.Parse(values[i]).Equals('R'))
            {
                controller.TurnRightAtSpeed(int.Parse(values[i + 1]), int.Parse(values[i + 2]));
                //Debug.Log("TURNING RIGHT: " + values[i + 1] + "   SPEED: " + values[i + 2]);

                //Debug.Log("Waiting until true");
                yield return new WaitUntil(() => CalcRotatingRight(int.Parse(values[i + 1])));

                //Debug.Log("Waiting " + values[i + 3] + " seconds");
                yield return new WaitForSecondsRealtime(int.Parse(values[i + 3]));
            }

            else if (char.Parse(values[i]).Equals('L'))
            {
                controller.TurnLeftAtSpeed(int.Parse(values[i + 1]), int.Parse(values[i + 2]));
                //Debug.Log("TURNING LEFT: " + values[i + 1] + "   SPEED: " + values[i + 2]);

                //Debug.Log("Waiting until true");
                yield return new WaitUntil(() => CalcRotatingLeft(int.Parse(values[i + 1])));

                //Debug.Log("Waiting " + values[i + 3] + " seconds");
                yield return new WaitForSecondsRealtime(int.Parse(values[i + 3]));
            }
            else
            {
                Debug.Log(values[i]);
                Debug.Log("Something has gone wrong");
            }
        }
        flag = true;
        Debug.Log("Done rotating");
    }


    public IEnumerator Threshold()
    {
        Debug.Log("Threshold route");

        while (threshold_counter < 8)
        {
            Debug.Log("Threshold round: " + threshold_counter);

            //Debug.Log(flag);
            if (flag == false)
            {
                controller.SetCurrentPositionToZero();
                StartCoroutine(Drive());

                yield return new WaitUntil(() => flag);

                Debug.Log("Waiting 30s");
                yield return new WaitForSecondsRealtime(30);

                flag = false;
                threshold_counter += 1;
            }
        }
        Debug.Log("Done thresholding routes");
    }


    public IEnumerator RandomAuto()
    {
        Debug.Log("Original route");
        Debug.Log("Round: " + route_counter);
        controller.SetCurrentPositionToZero();
        StartCoroutine(Drive());

        yield return new WaitUntil(() => flag);

        Debug.Log("Waiting 60s");
        yield return new WaitForSecondsRealtime(60);

        flag = false;
        route_counter += 1;

        Debug.Log("Puesdo randomised routes");
        while (route_counter < 8)
        {
            Debug.Log("Round: " + route_counter);

            //Debug.Log(flag);
            if (flag == false)
            {
                controller.SetCurrentPositionToZero();
                StartCoroutine(RandomDrive());

                yield return new WaitUntil(() => flag);

                Debug.Log("Waiting 60s");
                yield return new WaitForSecondsRealtime(60);

                flag = false;
                route_counter += 1;
            }
        }
        Debug.Log("Done 6 routes");
    }









    /**
     * The following is different attempts of trying to get the chair to wait, 
     * not used in the Drive function right now but could be used if you think they would work better
     **/
    public IEnumerator Wait(int second)
    {

        yield return new WaitForSecondsRealtime(second);
        Debug.Log("Waiting 30s");

        flag = false;
        route_counter += 1;
    }

    /**
    public IEnumerator WaitRotating(int angle)
    {
        while (CalcRotating(angle) is false)
        {

        }

        Debug.Log("Waiting untill true");
        yield return new WaitUntil(() => CalcRotating(angle));
        //done = true;
        //Debug.Log("Done is true");

    }
    **/

    private void wait2(int seconds)
    {
        if (flag)
        {
            Debug.Log("Waiting");
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                flag = false;
            }
        }
    }


    private void turnLeft()
    {
        controller.TurnLeftAtSpeed(90, 25);
        Debug.Log("Turning left");
        flag = true;
    }


    private void turnRight()
    {
        controller.TurnRightAtSpeed(90, 25);
        Debug.Log("Turning right");
    }


    IEnumerator turnLeftWait()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("Waiting left");

        controller.TurnLeftAtSpeed(90, 25);
        Debug.Log("Turning left");
    }


    IEnumerator turnRightWait()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("Waiting right");

        controller.TurnRightAtSpeed(90, 25);
        Debug.Log("Turning right");
    }

    public string RandomRoute()
    {
        int route = (int)Random.Range(1f, 7f);

        switch (route)
        {
            case 1:
                Debug.Log("Route 1 - Motorway Drive");
                return "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive1.txt";

            case 2:
                Debug.Log("Route 2 - Main Roads Drive");
                return "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive2.txt";

            case 3:
                Debug.Log("Route 3 - Housing Estate Drive");
                return "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive3.txt";

            case 4:
                Debug.Log("Route 4 - West End Drive");
                return "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive4.txt";

            case 5:
                Debug.Log("Route 5 - City Centre Drive");
                return "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive5.txt";

            case 6:
                Debug.Log("Route 6 - Loch Lommond Country Drive");
                return "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive6.txt";

            default:
                return null;
        }
    }


    /**
    private void sidetosiderotation()
    {
        if (isFirstTime)
        {
            randomAngle = (int)Random.Range(180f, 270f);
            //randomSpeed = (int)Random.Range(30f, 80f);
            controller.TurnLeftAtSpeed(randomAngle / 2, randomSpeed);
            Debug.Log("First left turn");
            isFirstTime = false;
        }

        StartCoroutine(Delay(3f));

    }
    IEnumerator Delay(float second)
    {
        //to ensure the rotation fre <0.2Hz, the angle should be >=180, speed should be < 80
        yield return new WaitForSecondsRealtime(second);
        controller.TurnRightAtSpeed(randomAngle, randomSpeed);
        Debug.Log("Turning right");
        yield return new WaitForSecondsRealtime(second);
        Debug.Log("Set to zero");
        controller.MoveChairToZero(randomSpeed);


        //int randomizedinterval = (int)Random.Range(3f, 10f);
        int randomizedinterval = 5;
        StartCoroutine(Delay1(randomizedinterval));

    }
    IEnumerator Delay1(float second)
    {
        yield return new WaitForSecondsRealtime(second);
        isFirstTime = true;
        sidetosiderotation();
    }
    **/
}

