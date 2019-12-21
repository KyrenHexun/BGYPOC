using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    /// <summary>
    /// 通用DTO类
    /// 属性全部依赖Attributes
    /// 通常用于纯查询场景
    /// </summary>
    [DataContract]
    public class CommonModel : ModelBase
    {
    }

    public static class JsonSerializerHelper
    {
        static JsonSerializerHelper()
        {
            /*JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                return jsonSerializerSettings;
            };*/
        }
        public static string Serializer<T>(T obj)
        {
            return Serializer(obj, null);
        }

        public static string Serializer(object obj, Type type)
        {
            string strInfo = string.Empty;
            strInfo = JsonConvert.SerializeObject(obj, new ModelBaseJsonConverter());
            return strInfo;
        }


        public static T Deserialize<T>(string strInfo)
        {
            /*T obj = default(T);
            if (string.IsNullOrEmpty(strInfo))
            {
                return obj;
            }
            byte[] byteArray = new UTF8Encoding().GetBytes(strInfo);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)jsonSerializer.ReadObject(stream);
                stream.Close();
            }
            return obj;*/

            return JsonConvert.DeserializeObject<T>(strInfo);
        }

        public static object Deserialize(string strInfo, Type type)
        {
            return JsonConvert.DeserializeObject(strInfo, type);
        }
    }

    /// <summary>
    /// 针对所有继承自ModelBase的类型做特殊化序列化处理
    /// 在序列化时，检查Attributes属性，只有在Attributes中包含的属性才会被序列化到json字符串中
    /// </summary>
    public class ModelBaseJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(ModelBase));

        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JsonSerializer newSerialzer = new JsonSerializer();

            //针对CommonModel做特殊处理

            if (objectType == typeof(CommonModel))
            {
                var jObj=newSerialzer.Deserialize<Dictionary<string,object>>(reader);
                CommonModel model = new CommonModel();
                foreach(var pItem in jObj)
                {
                    model.Attributes[pItem.Key] = pItem.Value;
                }

                return model;
            }
            else
            {
                var obj = newSerialzer.Deserialize(reader, objectType);
                return obj;
            }
        }

        /*public override bool CanWrite
        {
            get
            {
                var canWrite = _canWrite;
                if (canWrite==false)
                {
                    _canWrite = true;
                }
                return canWrite;
            }
        }*/

        private JObject CreateJObject(ModelBase model)
        {
            JObject jObject = new JObject();
            foreach (var attributeItem in model.Attributes)
            {
                if (attributeItem.Value is ModelBase)
                {
                    jObject.Add(attributeItem.Key, CreateJObject((ModelBase)(attributeItem.Value)));
                }
                else
                {
                    if (attributeItem.Value == null)
                    {
                        jObject.Add(attributeItem.Key, null);
                    }
                    else
                    {
                        jObject.Add(attributeItem.Key, JToken.FromObject(attributeItem.Value));
                    }
                }
            }

            return jObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            var model = (ModelBase)(value);

            JObject jObject = CreateJObject(model);
            

            jObject.WriteTo(writer);

            /*string strInfo = string.Empty;
            using (StringWriter stringWriter= new StringWriter())
            {


                _canWrite = false;

                serializer.Serialize(stringWriter,value,value.GetType());
                
                strInfo =stringWriter.ToString();


                stringWriter.Close();
            }


            JObject jObj = JObject.Parse(strInfo);

             foreach (var attributeItem in jObj.Properties().ToList())
             {
                 if (!model.Attributes.ContainsKey(attributeItem.Name))
                 {
                     attributeItem.Remove();
                 }
             }
             jObj.WriteTo(writer);*/


        }
    }
}
