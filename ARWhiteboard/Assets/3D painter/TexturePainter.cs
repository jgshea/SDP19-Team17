using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


public class TexturePainter : MonoBehaviour {

    [Range(1, 2048)]
    private int canvasWidth;
    [Range(1, 2048)]
    private int canvasHeight;
    private int radius;
    private int prevX;
    private int prevY;
    private bool marker;

    private Color foregroundColor;
    private Color backgroundColor;
    private Color prevColor;

    AndroidJavaObject serialHelper;

    static int x;
    static int y;

    Texture2D canvas;

    //1024x768 is the IR input space

    void Start()
    {
        x = 0;
        y = 0;
        foregroundColor = Color.black;
        backgroundColor = Color.white;
        radius = 6;
        marker = true;
        CreateBackground();
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        serialHelper = new AndroidJavaObject("com.team17.serialtool1.SerialHelper", context, new AndroidSerialHelperCallback());
    }

    void Update()
    {
        //value = stream.ReadLine(); //Read the information
        //values = value.Split(',');
        //stream.BaseStream.Flush();
        MousePressed();
        //if (Input.GetKey(KeyCode.Mouse0))
        //    MousePressed();            
    }

    void CreateBackground()
    {
        Texture2D tex = Resources.Load("back") as Texture2D;
        canvasWidth = tex.width;
        canvasHeight = tex.height;
        canvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.ARGB32, false);
        canvas.SetPixels(tex.GetPixels());
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", canvas);
        canvas.Apply();
        prevColor = foregroundColor;
    }

    void MousePressed()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // RaycastHit hitInfo;
        //int x = Convert.ToInt32(values[0]);
        //int y = Convert.ToInt32(values[1]);

        if (x == 1023 && y == 1023)
        {
            prevX = -1;
            prevY = -1;
        }

        // if(Physics.Raycast(ray, out hitInfo))
        //{
        //   var pixelCoords = uv2PixelCoords(hitInfo.textureCoord);
        //800
        //draw
        if (x < 800)
        {
            if (prevX == -1 || prevY == -1)
            {
                prevX = x;
                prevY = y;
                //Stroke(x, y, foregroundColor);
            }
            DrawLine(prevX, prevY, x, y, foregroundColor);
            prevX = x;
            prevY = y;
        }
        //change color
        else if (x > 837 && x < 986 && y < 461 && y > 340 && canvas.GetPixel(x, y) != backgroundColor)
        {
            foregroundColor = canvas.GetPixel(x, y);
            prevColor = canvas.GetPixel(x, y);
        }
        //change size1
        else if (x > 814 && x < 879 && y < 285 && y >= 169 && radius > 2)
        {
            radius--;
            /*
			GetComponent<MeshCollider>().enabled = false;
			StartCoroutine(Wait(1));
			GetComponent<MeshCollider>().enabled = true;*/
        }
        //change size2
        else if (x > 879 && x <= 942 && y < 285 && y >= 169)
        {
            radius = 6;
        }
        //change size3
        else if (x > 942 && x <= 1010 && y < 285 && y >= 169 && radius < 50)
        {
            radius += 1;
        }
        //marker tool
        else if (x > 911 && x <= 1010 && y < 139 && y >= 72)
        {
            marker = true;
            foregroundColor = prevColor;
        }
        //eraser tool
        else if (x > 814 && x <= 911 && y < 139 && y >= 72)
        {
            marker = false;
            foregroundColor = backgroundColor;
        }
        //clear all
        else if (x > 911 && x < 1010 && y < 72 && y >= 12)
        {
            for (int i = 0; i <= 800; i++)
            {
                for (int j = 0; j <= canvasHeight; j++)
                    canvas.SetPixel(i, j, backgroundColor);
            }
            canvas.Apply();
        }
    }

    //Vector2Int uv2PixelCoords(Vector2 uv)
    //{
    //    int x = Mathf.FloorToInt(uv.x * canvasWidth);
    //    int y = Mathf.FloorToInt(uv.y * canvasHeight);
    //    return new Vector2Int(x, y);
    //}


    void DrawCircle(int x, int y, Color color)
    {
        //Debug.Log("x: " + x + ", y: " + y);
        for (int yn = -radius; yn <= radius; yn++)
        {
            for (int xn = -radius; xn <= radius; xn++)
            {
                if (y + yn < canvasHeight && y + yn > 0 && x + xn < 800 && x + xn > 0)
                {
                    if (xn * xn + yn * yn <= radius * radius && marker)
                        canvas.SetPixel(x + xn, y + yn, color);
                    else if (!marker)
                        canvas.SetPixel(x + xn, y + yn, color);
                }
            }
        }

    }

    void Bresenham(int x0, int y0, int x1, int y1, bool reversedAxis, Color color)
    {
        double deltax = x1 - x0;
        double deltay = y1 - y0;
        double deltaerr = Math.Abs(deltay / deltax);
        double error = 0.0f;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            if (reversedAxis)
                DrawCircle(y, x, color);
            else
                DrawCircle(x, y, color);
            error = error + deltaerr;
            if (error >= 0.5f)
            {
                y = y + Math.Sign(deltay) * 1;
                error = error - 1.0f;
            }
        }
        canvas.Apply();
    }

    void DrawLine(int x0, int y0, int x1, int y1, Color color)
    {
        if (Math.Abs(y1 - y0) < Math.Abs(x1 - x0))
            if (x0 <= x1)
                Bresenham(x0, y0, x1, y1, false, color);
            else
                Bresenham(x1, y1, x0, y0, false, color);
        else
            if (y0 <= y1)
            Bresenham(y0, x0, y1, x1, true, color);
        else
            Bresenham(y1, x1, y0, x0, true, color);
    }

    class AndroidSerialHelperCallback : AndroidJavaProxy
    {
        public AndroidSerialHelperCallback() : base("com.team17.serialtool1.SerialHelperCallback") { }
        public void readData(string data)
        {
            string[] values = data.Split(',');
            x = Convert.ToInt32(values[0]);
            y = Convert.ToInt32(values[1]);
        }

    }

    void OnGUI(){
       //string newString = "Connected: " + transform.rotation.x + ", " + transform.rotation.y + ", " + transform.rotation.z;
       //GUI.Label(new Rect(10,10,300,100), value); //Display new values
       GUI.Label(new Rect(10,30,300,100), x+""); //Display new values
       GUI.Label(new Rect(10,50,300,100), y+""); //Display new values
       // Though, it seems that it outputs the value in percentage O-o I don't know why.
    }
}


