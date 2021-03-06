using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Roto.Control;

public class TestController : MonoBehaviour
{
    RotoController controller;
    RotoCalibration calibration;
    bool flag = true;
    float timer = 5;
    public int rotation;
    public string path;

    /**
    bool isFirstTime = true;
    int randomAngle = 0;
    int randomSpeed = 80;
    **/


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
            controller.SetCurrentPositionToZero();

            Debug.Log("Route 1 - Motorway Drive");
            path =  "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive1.txt";
            StartCoroutine(Drive());


            //Debug.Log("Performing calibration");
            //calibration.CalibrateChairZero(100);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            controller.SetCurrentPositionToZero();

            Debug.Log("Route 2 - Main Roads Drive");
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive2.txt";
            StartCoroutine(Drive());

            //controller.MoveChairToZero(50);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            controller.SetCurrentPositionToZero();

            Debug.Log("Route 3 - Housing Estate Drive");
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive3.txt";
            StartCoroutine(Drive());

            //sidetosiderotation();
            //ReadText();
        }

        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            controller.SetCurrentPositionToZero();

            Debug.Log("Route 4 - West End Drive");
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive4.txt";
            StartCoroutine(Drive());
        }

        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            controller.SetCurrentPositionToZero();

            Debug.Log("Route 5 - City Centre Drive");
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive5.txt";
            StartCoroutine(Drive());
        }

        if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            controller.SetCurrentPositionToZero();

            Debug.Log("Route 6 - Loch Lommond Country Drive");
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/drive6.txt";
            StartCoroutine(Drive());
        }

        if (Input.GetKeyUp(KeyCode.Alpha7))
        {
            controller.SetCurrentPositionToZero();

            //Debug.Log("Hard Route - Loch Lommond");
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/mediumRoute.txt";
            StartCoroutine(Drive());
        }




        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            controller.TurnLeftAtSpeed(90, 20);
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            controller.TurnRightAtSpeed(90, 20);
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            Debug.Log("Current angle " + controller.GetOutputRotation());
            //Debug.Log("Current angle " + controller.currentAngle);

        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            Debug.Log("Performing calibration");
            calibration.CalibrateChairZero(100);

        }

        if (Input.GetKey(KeyCode.Alpha0))
        {
            controller.MoveChairToZero(50);
        }
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
     * The 3 main functions 
    **/
    public string[] ReadText()
    {
        string[] values;
        
        string text = System.IO.File.ReadAllText(@path);

        //string text = System.IO.File.ReadAllText(@RandomRoute());
        
        values = text.Split(char.Parse("\n"), char.Parse(","));

        return values;
    }


    private bool CalcRotatingRight(int angle)
    {
        if (rotation >= angle - 3 && rotation <= angle + 3)
        {
            //Debug.Log("Rotation calc right: " + rotation);

            controller.SetCurrentPositionToZero();
            //Debug.Log("Setting to zero");

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CalcRotatingLeft(int angle)
    {
        if (rotation >= (360-angle) - 3 && rotation <= (360-angle) + 3)
        {
            //Debug.Log("Rotation calc left: " + rotation);

            controller.SetCurrentPositionToZero();
            //Debug.Log("Setting to zero");

            return true;
        }
        else
        {
            return false;
        }
    }


    /**
    * This code is my attempt to try stop the chair from continually spinning or excuting the rest 
    * of the loop before waiting the 5s by having it wait untill the angle is correct, however this hasnt worked so far.
    **/
    public IEnumerator Drive()
    {
        string[] values;
        values = ReadText();

        //for each rotation in the text file, excecute rotation and then wait 5s, value[i] = angle, value[i+1] = speed
        for (int i = 0; i < values.Length-1; i += 4)
        {
            if (char.Parse(values[i]).Equals('R'))
            {
                controller.TurnRightAtSpeed(int.Parse(values[i+1]), int.Parse(values[i+2]));
                Debug.Log("TURNING RIGHT: " + values[i+1] + "   SPEED: " + values[i+2]);

                //Debug.Log("Waiting until true");
                yield return new WaitUntil(() => CalcRotatingRight(int.Parse(values[i+1])));

                Debug.Log("Waiting " + values[i+3] + " seconds");
                yield return new WaitForSecondsRealtime(int.Parse(values[i+3]));
            }

            else if (char.Parse(values[i]).Equals('L'))
            {
                controller.TurnLeftAtSpeed(int.Parse(values[i+1]), int.Parse(values[i+2]));
                Debug.Log("TURNING LEFT: " + values[i+1] + "   SPEED: " + values[i+2]);

                //Debug.Log("Waiting until true");
                yield return new WaitUntil(() => CalcRotatingLeft(int.Parse(values[i+1])));

                Debug.Log("Waiting " + values[i+3] + " seconds");
                yield return new WaitForSecondsRealtime(int.Parse(values[i+3]));
            }
            else
            {
                Debug.Log(values[i]);
                Debug.Log("Something has gone wrong");
            }
        }
        Debug.Log("DONE ROTATING");
    }








    /**
     * The following is different attempts of trying to get the chair to wait, 
     * not used in the Drive function right now but could be used if you think they would work better
     **/
    public IEnumerator Wait(int second)
    {
        yield return new WaitForSecondsRealtime(second);
        Debug.Log("Waiting");

        controller.SetCurrentPositionToZero();
        Debug.Log("Setting to zero");
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

/**
 * To Do:
 * Calibrate the chair to check if its at zero after every turn
 * Add into the text file R or L to tell the chair if it should turn left or right, need an if statement
 * Add into the text file waiting time
 **/
