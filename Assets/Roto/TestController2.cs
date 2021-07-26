using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Roto.Control;

public class TestController2 : MonoBehaviour
{
    RotoController controller;
    RotoCalibration calibration;
    public string path;
    public int rotation;
    
    //The following are used in automation and psuedo random functions
    bool flag = false;
    public int log = 0;
    public int threshold_counter = 1;
    public int route_counter = 1;


    void Start()
    {
        controller = FindObjectOfType<RotoController>();
        calibration = FindObjectOfType<RotoCalibration>();
    }


    void Update()
    {
        //Gets the current angle of the chair every frame
        rotation = controller.GetOutputRotation();

        //Start the auto thresholding
        if (Input.GetKey(KeyCode.Alpha1))
        {
            //Path to get the text file with the route in it
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/thresholdRoute.txt";

            //Need to use StartCoroutine for these functions (not 100% why, what Unity tells me to do, i just do)
            StartCoroutine(Threshold());
        }

        //Start the auto 6 experimental puesdo routes
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/standardRoute.txt";
            StartCoroutine(RandomAuto());
        }

        //Start a the original route
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            path = "C:/Users/MIG/Documents/GitHub/RotoVR/Assets/Roto/Routes/standardRoute.txt";
            StartCoroutine(Drive());
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
            //Sets the chairs current angle to zero/default
            Debug.Log("Performing calibration");
            calibration.CalibrateChairZero(100);
        }

        if (Input.GetKey(KeyCode.Alpha0))
        {
            //Moves the chair back to angle 0 at a speed of 40
            controller.MoveChairToZero(40);
        }
    }





    /**
     * The 3 set up functions 
    **/
    public string[] ReadText()
    {
        string[] values;

        //Use this when you know the exact text file path you want to use
        string text = System.IO.File.ReadAllText(@path);

        //Use this when you want to use the random route function to pick a path
        //string text = System.IO.File.ReadAllText(@RandomRoute());

        values = text.Split(char.Parse("\n"), char.Parse(","));

        return values;
    }


    //Creates a dictionary with the instruction id being the key and the 4 char route instructions being the value
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


    //Creates a list of numbers from 1-31 in random order, 31 since there are 31 instructions in the text file
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
            //Example: Angle = 90 degrees, if the current angle is >= 87 or <= 93 then return true
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
     * This code reads in the text file route and instructs the chair on how to complete it
    **/
    public IEnumerator Drive()
    {
        string[] values;
        values = ReadText();

        //for each rotation in the text file, excecute rotation and then wait X seconds, value[i] = angle, value[i+1] = speed
        for (int i = 0; i < values.Length - 1; i += 4)
        {
            if (char.Parse(values[i]).Equals('R'))
            {
                controller.TurnRightAtSpeed(int.Parse(values[i + 1]), int.Parse(values[i + 2]));
                //Debug.Log("TURNING RIGHT: " + values[i + 1] + "   SPEED: " + values[i + 2]);

                yield return new WaitUntil(() => CalcRotatingRight(int.Parse(values[i + 1])));

                yield return new WaitForSecondsRealtime(int.Parse(values[i + 3]));
                //Debug.Log("Waiting " + values[i + 3] + " seconds");
            }

            else if (char.Parse(values[i]).Equals('L'))
            {
                controller.TurnLeftAtSpeed(int.Parse(values[i + 1]), int.Parse(values[i + 2]));
                //Debug.Log("TURNING LEFT: " + values[i + 1] + "   SPEED: " + values[i + 2]);

                yield return new WaitUntil(() => CalcRotatingLeft(int.Parse(values[i + 1])));

                yield return new WaitForSecondsRealtime(int.Parse(values[i + 3]));
                //Debug.Log("Waiting " + values[i + 3] + " seconds");
            }
            else
            {
                Debug.Log(values[i]);
                Debug.Log("Something has gone wrong");
            }
        }
        flag = true; //Needed for automation of routes
        Debug.Log("Done rotating");
    }


    /**
    * This code takes a text file route and randomises the order of the route.
    * The code then loops through the random route and turns the chair accordindly.
    * If the chair is going to pass 180 in one direction then it switches the instructions
    * to then turn in the opposite direction.
    **/
    public IEnumerator RandomDrive()
    {
        //Get the dictionary of ids and instruactions
        IDictionary<string, List<string>> dictionary = Dict(ReadText());

        //Get the list of randomised numbers
        List<int> ids = RandomOrder();

        //for each value in the random list, look up that value as an id in the dictionary to get the instructions.
        foreach (int i in ids)
        {
            string id = i.ToString();

            //id[0] = right or left, id[1] = angle, id[2] = speed, id[3] = wait time in seconds
            if (char.Parse(dictionary[id][0]).Equals('R'))
            {
                //log keeps track of the accumulation of the chairs angle, if turns right its negative
                log -= int.Parse(dictionary[id][1]);;

                if (log <= -180)
                {
                    controller.TurnLeftAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("OVER -180; CHANGE TO TURNING LEFT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    //in order to update the log value
                    log += int.Parse(dictionary[id][1])*2;

                    yield return new WaitUntil(() => CalcRotatingLeft(int.Parse(dictionary[id][1])));

                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                }

                else
                {
                    controller.TurnRightAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("TURNING RIGHT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    yield return new WaitUntil(() => CalcRotatingRight(int.Parse(dictionary[id][1])));

                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                }

            }

            else if (char.Parse(dictionary[id][0]).Equals('L'))
            {
                log += int.Parse(dictionary[id][1]);

                if (log >= 180)
                {
                    controller.TurnRightAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("OVER 180; CHANGE TO TURNING RIGHT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    log -= int.Parse(dictionary[id][1])*2;

                    yield return new WaitUntil(() => CalcRotatingRight(int.Parse(dictionary[id][1])));

                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                }

                else
                {
                    controller.TurnLeftAtSpeed(int.Parse(dictionary[id][1]), int.Parse(dictionary[id][2]));
                    //Debug.Log("TURNING LEFT: " + dictionary[id][1] + "   SPEED: " + dictionary[id][2]);

                    yield return new WaitUntil(() => CalcRotatingLeft(int.Parse(dictionary[id][1])));

                    yield return new WaitForSecondsRealtime(int.Parse(dictionary[id][3]));
                    //Debug.Log("Waiting " + dictionary[id][3] + " seconds");
                }

            }
            else
            {
                //print the whole instruction
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
     * These 2 fucntions are for automating the experiment, thresholding and the actual run
     **/
    public IEnumerator Threshold()
    {
        //Debug.Log("Threshold route");

        while (threshold_counter != 7)
        {
            Debug.Log("Threshold round: " + threshold_counter);

            if (flag == false)
            {
                //Do a route, always set current position to zero (not 100% sure why but it hates you if you dont)
                controller.SetCurrentPositionToZero();
                StartCoroutine(Drive());

                //Wait till the route is complete
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

        //This is the first round which runs the original route
        controller.SetCurrentPositionToZero();
        StartCoroutine(Drive());

        yield return new WaitUntil(() => flag);

        Debug.Log("Waiting 60s");
        yield return new WaitForSecondsRealtime(60);

        flag = false;
        route_counter += 1;

        Debug.Log("Puesdo randomised routes");
        while (route_counter != 7)
        {
            Debug.Log("Round: " + route_counter);

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
     * The following is different attempts of trying to get the chair to wait, turn, etc
     * Not that relevant right now but could be of use
     **/
    public IEnumerator Wait(int second)
    {

        yield return new WaitForSecondsRealtime(second);
        Debug.Log("Waiting 30s");

        flag = false;
        route_counter += 1;
    }


    private void turnLeft()
    {
        controller.TurnLeftAtSpeed(90, 25);
        Debug.Log("Turning left");
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


    //Used when there was 6 possible routes to choose from randomly
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
     * Gang's original spinning function that was a bit hardcore
     * 
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

