using Project.Networking;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TexturePainter_RC : MonoBehaviour
{
    public Camera IRCamera;
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

    private GUIStyle style = new GUIStyle();

    AndroidJavaObject serialHelper;

    static int x;
    static int y;

    public static Texture2D canvas;

    private NetworkManager network;

    //1024x768 is the IR input space

    void Start()
    {
        IRCamera = GameObject.FindWithTag("IRCamera").GetComponent<Camera>();
        IRCamera.aspect = 4f/ 3f;
        y = 0;
        foregroundColor = Color.black;
        backgroundColor = Color.clear;
        radius = 4;
        marker = true;
        CreateBackground();
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        serialHelper = new AndroidJavaObject("com.team17.serialtool1.SerialHelper", context, new AndroidSerialHelperCallback());
    }

    void Update()
    {
        MousePressed();          
    }

    public void applyUpdates(List<NetworkManager.PixelUpdate> updates)
    {
        foreach (NetworkManager.PixelUpdate update in updates)
        {
            canvas.SetPixel(update.x, update.y, update.color);
        }
    }

    private void updatePixels(List<NetworkManager.PixelUpdate> updates)
    {
        List<NetworkManager.PixelUpdate> trueUpdates = new List<NetworkManager.PixelUpdate>();
        foreach (NetworkManager.PixelUpdate update in updates)
        {
            if (!canvas.GetPixel(update.x, update.y).Equals(update.color))
            {
                canvas.SetPixel(update.x, update.y, update.color);
                trueUpdates.Add(update);
            }
        }

        if (trueUpdates.Count > 0)
            getNetwork().sendUpdates(trueUpdates);
    }

    private NetworkManager getNetwork()
    {
        return gameObject.GetComponent<NetworkManager>();
    }

    void CreateBackground()
    {
        Texture2D tex = Resources.Load("whiteBoardObject") as Texture2D;
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
        if((x == 1023 && y == 1023) || (x == 0 && y == 0))
        {
            prevX = -1;
            prevY = -1;
            return;
        }
        float nx = (float)x;
        float ny = (float)y;
        nx = nx / 1024;
        ny = ny / 768;
        Ray ray = IRCamera.ViewportPointToRay(new Vector3(nx,ny));
        //Vector3 origin = Camera.main.ViewportToWorldPoint(new Vector3(nx, ny));
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        //if (Physics.Raycast(origin,direction, out hitInfo))
        {
            var pixelCoords = uv2PixelCoords(hitInfo.textureCoord);
            //800
            //draw
            if (pixelCoords.x < 800)
            {
                //Stroke(pixelCoords.x, pixelCoords.y, foregroundColor);
                if (x < 800)
                {
                    if (prevX == -1 || prevY == -1)
                    {
                        prevX = pixelCoords.x;
                        prevY = pixelCoords.y;
                    }
                    DrawLine(prevX, prevY, pixelCoords.x, pixelCoords.y, radius, foregroundColor, true);
                    prevX = pixelCoords.x;
                    prevY = pixelCoords.y;
                }
            }
            //change color
            else if (pixelCoords.x > 837 && pixelCoords.x < 986 &&
                pixelCoords.y < 946 && pixelCoords.y > 737 && canvas.GetPixel(pixelCoords.x, pixelCoords.y) != backgroundColor)
            {
                foregroundColor = canvas.GetPixel(pixelCoords.x, pixelCoords.y);
                prevColor = canvas.GetPixel(pixelCoords.x, pixelCoords.y);
            }
            //change size1
            else if (pixelCoords.x > 814 && pixelCoords.x < 852 &&
                pixelCoords.y < 641 && pixelCoords.y > 449 && radius > 2)
            {
                radius--;
            }
            //change size2
            else if (pixelCoords.x > 852 && pixelCoords.x < 935 && pixelCoords.y < 641 && pixelCoords.y > 449)
            {
                radius = 6;
            }
            //change size3
            else if (pixelCoords.x > 935 && pixelCoords.x < 1009 &&
                pixelCoords.y < 641 && pixelCoords.y > 449 && radius < 50)
            {
                radius += 1;
            }
            //marker tool
            else if (pixelCoords.x > 814 && pixelCoords.x < 910 && pixelCoords.y < 382 && pixelCoords.y > 188)
            {
                marker = true;
                foregroundColor = prevColor;
            }
            //eraser tool
            else if (pixelCoords.x > 915 && pixelCoords.x < 1009 && pixelCoords.y < 382 && pixelCoords.y > 188)
            {
                marker = false;
                foregroundColor = backgroundColor;
            }
            //clear all
            else if (pixelCoords.x > 814 && pixelCoords.x < 910 && pixelCoords.y < 166 && pixelCoords.y > 13)
            {
                Clear(true);
            }
        }
    }

    public void Clear(bool broadcast)
    {
        /*for (int i = 0; i <= 800; i++)
        {
            for (int j = 0; j <= canvasHeight; j++)
                canvas.SetPixel(i, j, backgroundColor);
        }
        canvas.Apply();*/
        CreateBackground();
        if (broadcast) getNetwork().Clear();
    }

    Vector2Int uv2PixelCoords(Vector2 uv)
    {
        int x = Mathf.FloorToInt(uv.x * canvasWidth);
        int y = Mathf.FloorToInt(uv.y * canvasHeight);
        return new Vector2Int(x, y);
    }

    void Stroke(int x, int y, Color color)
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
        canvas.Apply();
    }


    public void DrawCircle(int x, int y, int radius, Color color, Boolean broadcast)
    {
        //Debug.Log("x: " + x + ", y: " + y);
        for (int yn = -radius; yn <= radius; yn++)
        {
            for (int xn = -radius; xn <= radius; xn++)
            {
                if (y + yn < canvasHeight && y + yn > 0 && x + xn < 800 && x + xn > 0)
                {
                    if (xn * xn + yn * yn <= radius * radius && marker)
                    {
                        canvas.SetPixel(x + xn, y + yn, color);
                    }
                    else if (!marker)
                    {
                        canvas.SetPixel(x + xn, y + yn, color);
                    }
                }
            }
        }

        if (broadcast)
            getNetwork().drawCircle(new NetworkManager.DrawCircle { x = x, y = y, radius = radius, color = color, sessionID = 0 });
    }

    void Bresenham(int x0, int y0, int x1, int y1, bool reversedAxis, int radius, Color color)
    {
        double deltax = x1 - x0;
        double deltay = y1 - y0;
        double deltaerr = Math.Abs(deltay / deltax);
        double error = 0.0f;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            if (reversedAxis)
                DrawCircle(y, x, radius, color, false);
            else
                DrawCircle(x, y, radius, color, false);
            error = error + deltaerr;
            if (error >= 0.5f)
            {
                y = y + Math.Sign(deltay) * 1;
                error = error - 1.0f;
            }
        }
        canvas.Apply();
    }

    public void DrawLine(int x0, int y0, int x1, int y1, int radius, Color color, Boolean broadcast)
    {
        if (Math.Abs(y1 - y0) < Math.Abs(x1 - x0))
            if (x0 <= x1)
                Bresenham(x0, y0, x1, y1, false, radius, color);
            else
                Bresenham(x1, y1, x0, y0, false, radius, color);
        else
            if (y0 <= y1)
            Bresenham(y0, x0, y1, x1, true, radius, color);
        else
            Bresenham(y1, x1, y0, x0, true, radius, color);

        if (broadcast)
        {
            getNetwork().drawLine(new NetworkManager.DrawLine
            {
                x0 = x0,
                y0 = y0,
                x1 = x1,
                y1 = y1,
                radius = radius,
                color = color
            });
        }
    }

    class AndroidSerialHelperCallback : AndroidJavaProxy
    {
        public AndroidSerialHelperCallback() : base("com.team17.serialtool1.SerialHelperCallback") { }
        public void receiveData(string data)
        {
            string[] values = data.Split(',');
            x = Convert.ToInt32(values[0]);
            y = Convert.ToInt32(values[1]);
        }

    }

    void OnGUI()
    {
        style.fontSize = 20;
        //string newString = "Connected: " + transform.rotation.x + ", " + transform.rotation.y + ", " + transform.rotation.z;
        //GUI.Label(new Rect(10,10,300,100), value); //Display new values
        GUI.Label(new Rect(40, 40, 300, 100), x + "",style); //Display new values
        GUI.Label(new Rect(40, 90, 300, 100), y + "",style); //Display new values
                                                       // Though, it seems that it outputs the value in percentage O-o I don't know why.
    }
}


