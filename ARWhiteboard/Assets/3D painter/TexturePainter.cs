using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter : MonoBehaviour {

    

    [Range(1, 2048)]
    public int canvasWidth;
    [Range(1, 2048)]
    public int canvasHeight;

    public Color foregroundColor = Color.black;
    public Color backgroundColor = Color.white;

    
    Texture2D canvas;
    
    //1024x768
    
    void Start()
    {
        CreateBackground();
        //Fill(backgroundColor, canvas);
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
        //canvas.filterMode = FilterMode.Point;
        //var filePath = "C:/Users/Jeff/Pictures/back.png";
        //if (System.IO.File.Exists(filePath))
        //{
        // Image file exists - load bytes into texture
        //var bytes = System.IO.File.ReadAllBytes(filePath);
        
        
        //tex.LoadImage(bytes);
            
        //GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
        //Fill(backgroundColor, tex.GetPixels(), canvas);
        //canvas.EncodeToPNG();
        canvas.SetPixels(tex.GetPixels());
        //canvas.LoadImage(bytes);


        //}
        
        //canvas.filterMode = FilterMode.Point;
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", canvas);
        canvas.Apply();
        

        // Load the Image from somewhere on disk

    }

    void Fill(Color color, Color[] image, Texture2D texture)
    {
        
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    void MousePressed()
    {
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo))
        {
            var pixelCoords = uv2PixelCoords(hitInfo.textureCoord);
            //800
            if (pixelCoords.x < 800)
            {
                Stroke(pixelCoords.x, pixelCoords.y, foregroundColor);
            }
            else if (pixelCoords.x > 837 && pixelCoords.x < 986 && pixelCoords.y  < 458 && pixelCoords.y > 341)
            {
                foregroundColor = canvas.GetPixel(pixelCoords.x, pixelCoords.y);
            }
        }
    }

    void Stroke(int x, int y, Color color)
    {
        
        //x left 837 x right 986 y top 458 y bottom 341
        //Debug.Log("x: " + x + ", y: " + y);
        for (int i = x-9; i <= x+9; i++)
        {
            for (int j = y-9; j <= y+9; j++)
            {
                if (j < canvasHeight && j > 0 && i < canvasWidth - 278 && i > 0)
                    canvas.SetPixel(i, j, color);
            }
        }
        
        canvas.Apply();
    }

    Vector2Int uv2PixelCoords(Vector2 uv)
    {
        int x = Mathf.FloorToInt(uv.x * canvasWidth);
        int y = Mathf.FloorToInt(uv.y * canvasHeight);
        return new Vector2Int(x, y);
    }



}
