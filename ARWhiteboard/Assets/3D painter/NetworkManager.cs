using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SocketIO;
using System;

namespace Project.Networking
{
    public class NetworkManager : SocketIOComponent
    {
     
        [SerializeField] public struct ColorArrayWrapper { public Color[] colors; }

        List<Color> colors;
        GUIStyle guiStyle = new GUIStyle();
        
        [Serializable]
        public struct PixelUpdate
        {
            public int x;
            public int y;
            public Color color;
        }
        
        [Serializable]
        public struct UpdateMessage
        {
            public int sessionID;
            public List<PixelUpdate> changes;
        }

        [Serializable]
        public struct DrawCircle
        {
            public int sessionID;
            public int x;
            public int y;
            public int radius;
            public Color color;
        }

        [Serializable]
        public struct DrawLine
        {
            public int sessionID;
            public int x0;
            public int y0;
            public int x1;
            public int y1;
            public int radius;
            public Color color;
        }

        public static IEnumerable<JSONObject> prepareMessage(List<PixelUpdate> updates)
        {
            int payloadSize = 2000;
            int messages = (updates.Count - 1) / payloadSize + 1;
            
            for(int i = 0; i < messages; i++)
            {
                int start = i * payloadSize;
                int count = payloadSize;
                if (start + payloadSize >= updates.Count)
                    count = updates.Count - start;

                List<PixelUpdate> payload = updates.GetRange(start, count);
                UpdateMessage message = new UpdateMessage { sessionID = 0, changes = payload };

                yield return new JSONObject(JsonUtility.ToJson(message));
            }
        }

        public void sendUpdates(List<PixelUpdate> updates)
        {
            foreach(JSONObject message in prepareMessage(updates))
            {
                Debug.Log(message.ToString());
                Emit("image update", message);
                Debug.Log("Sent message");
            }
        }

        public void drawCircle(DrawCircle circle)
        {
            Emit("draw circle", new JSONObject(JsonUtility.ToJson(circle)));
        }

        public void drawLine(DrawLine line)
        {
            Emit("draw line", new JSONObject(JsonUtility.ToJson(line)));
        }

        public void Clear()
        {
            Emit("clear");
        }

        public override void Start()
        {
            base.Start();
            colors = new List<Color>(new Color[100]);
            //TexturePainter.canvas.GetPixels();//{ Color.black, Color.white };
            //Debug.Log((new JSONObject(JsonUtility.ToJson(mine))).ToString());
            Connect();
            SetupEvents();
            
        }

        public override void Update()
        {
            base.Update(); 
        }

        private TexturePainter_RC getTexturePainter()
        {
            return gameObject.GetComponent<TexturePainter_RC>();
        }

        private void SetupEvents()
        {
            On("open", (E) =>
            {
                //Debug.Log("connected to server");
                //Emit("test", new JSONObject(JsonUtility.ToJson(mine)));
            });
            On("init", (E) =>
            {
                Debug.Log("connected to server");
            });
            On("testback", (E) =>
            {
                Debug.Log("recieved response");
                
                //Debug.Log(E.data["id"].ToString());
            });

            On("image update", (E) =>
            {
                Debug.Log("Received update");

                UpdateMessage message= JsonUtility.FromJson<UpdateMessage>(E.data.ToString());
                getTexturePainter().applyUpdates(message.changes);
            });

            On("draw circle", (E) =>
            {
                Debug.Log("Draw circle received");

                DrawCircle message = JsonUtility.FromJson<DrawCircle>(E.data.ToString());
                getTexturePainter().DrawCircle(message.x, message.y, message.radius, message.color, false);
            });

            On("draw line", (E) =>
            {
                Debug.Log("Draw line received");

                DrawLine message = JsonUtility.FromJson<DrawLine>(E.data.ToString());
                getTexturePainter().DrawLine(message.x0, message.y0, message.x1, message.y1,
                    message.radius, message.color, false);
            });

            On("clear", (E) =>
            {
                Debug.Log("clear received");
                getTexturePainter().Clear(false);
            });
        }

    }
}