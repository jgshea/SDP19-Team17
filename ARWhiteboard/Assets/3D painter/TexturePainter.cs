using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter : MonoBehaviour {

    

    [Range(1, 2048)]
    private int canvasWidth;
    [Range(1, 2048)]
    private int canvasHeight;
    private int radius;

    private bool marker;

    private Color foregroundColor;
    private Color backgroundColor;
    private Color prevColor;

    Texture2D canvas;
    
    //1024x768 is the IR input space
    
    void Start()
    {
        foregroundColor = Color.black;
        backgroundColor = Color.white;
        radius = 6;
        marker = true;
        CreateBackground();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
            MousePressed();            
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo))
        {
            var pixelCoords = uv2PixelCoords(hitInfo.textureCoord);
            //800
            //draw
            if (pixelCoords.x < 800)
            {
                Stroke(pixelCoords.x, pixelCoords.y, foregroundColor);
            }
            //change color
            else if (pixelCoords.x > 837 && pixelCoords.x < 986 && 
                pixelCoords.y  < 946 && pixelCoords.y > 737 && canvas.GetPixel(pixelCoords.x, pixelCoords.y) != backgroundColor)
            {
                foregroundColor = canvas.GetPixel(pixelCoords.x, pixelCoords.y);
                prevColor = canvas.GetPixel(pixelCoords.x, pixelCoords.y);
            }
            //change size1
            else if (pixelCoords.x > 814 && pixelCoords.x < 852 && 
                pixelCoords.y < 641 && pixelCoords.y > 449 && radius > 2)
            {
                radius--;
                /*
                GetComponent<MeshCollider>().enabled = false;
                StartCoroutine(Wait(1));
                GetComponent<MeshCollider>().enabled = true;*/
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
                for (int i = 0; i <= 800; i++)
                {
                    for (int j = 0; j <= canvasHeight; j++) 
                        canvas.SetPixel(i, j, backgroundColor);
                }
                canvas.Apply();
            }
        }
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
                if(y + yn < canvasHeight && y + yn > 0 && x + xn < 800 && x + xn > 0)
                {
                    if (xn * xn + yn * yn <= radius * radius && marker)
                        canvas.SetPixel(x + xn, y + yn, color);
                    else if(!marker) 
                        canvas.SetPixel(x + xn, y + yn, color);
                }
            }
        }
        canvas.Apply();
    }
    /*
    IEnumerator Wait(float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
    }*/
}
