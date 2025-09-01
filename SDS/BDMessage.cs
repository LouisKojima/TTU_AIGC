using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


[Serializable]
public class BDMessage
{
    //{"err_no":0,"err_msg":"OK","log_id":1680598036,"sn":"8289623a-6f74-46f0-b9f6-633b6b589dc2_ws_0","type":"FIN_TEXT","result":"result","start_time":890,"end_time":4300,"product_id":15372,"product_line":"open"}
    // {"err_no":-3005,"err_msg":"asr server not find effective speech[info:-4]","log_id":3678430998,"sn":"62b8c944-14a3-4f0d-8ff6-f71540d2839a_ws_1","type":"FIN_TEXT","result":"","panting":0,"start_time":3490,"end_time":5730,"asmr":0,"product_id":15372,"product_line":"open"}
    //{"type":"HEARTBEAT"}
    public enum BDMessageType { INVALID, MID_TEXT, FIN_TEXT, HEARTBEAT }

    public int err_no = 0;
    public string err_msg = "";
    public long log_id = 0;
    public string sn = "";
    public BDMessageType type = BDMessageType.INVALID;
    public string result = "";
    public int start_time = 0;
    public int end_time = 0;
    public int product_id = 0;
    public string product_line = "";

    public BDMessage() { }
    public BDMessage(string toPrase)
    {
        PraseString(toPrase);
    }

    private Dictionary<string, FieldInfo> fields = typeof(BDMessage).GetFields().ToDictionary(x => x.Name);

    public void Clear()
    {
        err_no = 0;
        err_msg = "";
        log_id = 0;
        sn = "";
        type = BDMessageType.INVALID;
        result = "";
        start_time = 0;
        end_time = 0;
        product_id = 0;
        product_line = "";
    }

    public void PraseString(string toPrase)
    {
        Dictionary<string, string> input = toPrase.TrimStart('{').TrimEnd('}').Replace("\"", "").Split(',').Select(x => x.Split(':')).ToDictionary(x => x[0], x => x[1]);
        //Debug.Log(string.Join(";", input.Select(x => x.Key + " : " + x.Value)));
        //FieldInfo[] fields = typeof(BDMessage).GetFields();
        Dictionary<FieldInfo, string> joined = fields.Join(input,
            f => f.Key,
            i => i.Key,
            (f, i) => (f.Value, i.Value))
            .ToDictionary(x => x.Item1, x => x.Item2);
        //Debug.Log(string.Join(";", joined.Select(x => x.Key.Name + " : " + x.Value)));
        foreach ((FieldInfo f, string s) in joined)
        {
            Debug.Log("Prasing " + f + " = " + s);
            Type type = f.FieldType;
            if (type.IsPrimitive)
                f.SetValue(this, Convert.ChangeType(s, f.FieldType));
            else if (type.IsEnum)
                f.SetValue(this, Enum.Parse(f.FieldType, s, ignoreCase: true));
            else if (type == typeof(string))
                f.SetValue(this, s);
            else
                Debug.Log("type not found: " + f + " = " + s);
        }

        //string[] data = toPrase.TrimStart('{').TrimEnd('}').Replace("\"", "").Split(',');
        //foreach (string s in data)
        //{
        //    string[] ss = s.Split(':');
        //    switch (ss[0])
        //    {
        //        case "type":
        //            if (!Enum.TryParse(ss[1], out type)) type = BDMessageType.INVALID;
        //            break;
        //        case "err_no":
        //            err_no = int.Parse(ss[1]);
        //            break;
        //        case "err_msg":
        //            err_msg = ss[1];
        //            break;

        //        default:
        //            type = BDMessageType.INVALID;
        //            break;
        //    }
        //}
    }

    public static BDMessage Prase(string toPrase)
    {
        return new(toPrase);
    }
}
