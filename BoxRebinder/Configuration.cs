using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;

namespace BoxRebinder
{
    class Config
    {
        public string name;
        public int combinationArraySize;
        public string combination;
        public int pinA;
        public int pinB;
        public int pinX;
        public int pinY;
        public int pinL;
        public int pinR;
        public int pinZ;
        public int pinStart;
        public int pinUp;
        public int pinDown;
        public int pinLeft;
        public int pinRight;
        public int pinCUp;
        public int pinCDown;
        public int pinCLeft;
        public int pinCRight;
        public int pinSwitch;
        public int pinTilt;
        public int pinDUp;
        public int pinDDown;
        public int pinDLeft;
        public int pinDRight;
        public int pinLightShield;
        public int tiltValue;
        public int x1Value;
        public int x2Value;
        public int y1Value;
        public int y2Value;
        public int socdMode;
        public bool switchForDirections;

        [JsonIgnore]
        public Dictionary<int, string> PinDict = new Dictionary<int, string>();
        public void Connect()
        {
            FieldInfo[] fields = this.GetType().GetFields();
            foreach(FieldInfo prop in fields)
            {
                if (!prop.Name.StartsWith("pin"))
                    continue;
                int? value = this.GetType().GetField(prop.Name).GetValue(this) as int?;

                if (value == null)
                    continue;
                PinDict.Add((int)value, prop.Name);
            }
        }
    } 

    class Configs
    {
        public List<Config> configurations;
    }
}
