using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    public static class JudgeEntity
    {
        public delegate Entity CreateDelegate<T>(T item, Entity entity);
        /// <summary>
        /// 特殊字段标记
        /// </summary>
        public class EntitySpecialType : Attribute
        {
            /// <summary>
            /// 类型名称
            /// </summary>
            public string TypeName { get; set; }
            /// <summary>
            /// 表名称
            /// </summary>
            public string TableName { get; set; }
        }
        /// <summary>
        /// Entity转Model类型 【注：多表查询的“.”号Model中无法声明，请用双下划线代替“__”】
        /// 例如：mcs_member.mcs_userId 写成mcs_member__mcs_userId
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="TModel"></param>
        /// <returns></returns>
        public static T EntityToModel<T>(Entity entity, T TModel)
        {

            Type campaignInfoType = TModel.GetType();
            var properties = campaignInfoType.GetProperties();
            //利用反射循环Model字段
            foreach (var itemField in properties)
            {
                string fieldName = itemField.Name;
                if (fieldName.IndexOf("__") > 0)
                {
                    if (entity.Attributes.ContainsKey(fieldName.Replace("__", ".")))
                    {
                        fieldName = fieldName.Replace("__", ".");
                    }

                }
                //如果查出的接口中包含当前model的字段
                if (entity.Attributes.ContainsKey(fieldName))
                {
                    //获取当前字段的类型
                    var typeName = entity.Attributes[fieldName].GetType();
                    if (typeName == typeof(OptionSetValue))
                    {
                        //给model的字段赋值
                        itemField.SetValue(TModel, entity.GetAttributeValue<OptionSetValue>(fieldName).Value, null);
                    }
                    else if (typeName == typeof(EntityReference))
                    {
                        //给model的字段赋值
                        itemField.SetValue(TModel, entity.GetAttributeValue<EntityReference>(fieldName).Id.ToString(), null);
                    }
                    else if (typeName == typeof(Byte[]))
                    {
                        //给model的字段赋值
                        itemField.SetValue(TModel, entity.GetAttributeValue<byte[]>(fieldName), null);
                    }
                    else if (typeName == typeof(DateTime))
                    {
                        //时间从UTC转为本地时间
                        itemField.SetValue(TModel, entity.GetAttributeValue<DateTime>(fieldName).ToLocalTime().ToString(), null);
                    }
                    else if (typeName == typeof(Guid))
                    {
                        //给model的字段赋值
                        itemField.SetValue(TModel, entity[fieldName].ToString(), null);
                    }
                    else if (typeName == typeof(AliasedValue))
                    {
                        //给model的字段赋值
                        itemField.SetValue(TModel, entity.GetAttributeValue<AliasedValue>(fieldName).Value.ToString(), null);
                    }
                    else if (typeName == typeof(Money))
                    {
                        //给model的字段赋值
                        itemField.SetValue(TModel, entity.GetAttributeValue<Money>(fieldName).Value, null);
                    }
                    else
                    {
                        //给model的字段赋值
                        itemField.SetValue(TModel, entity[fieldName], null);
                    }

                }
                else//如果model不在查出的接口中，赋空值
                {
                    itemField.SetValue(TModel, null, null);
                }

            }

            return TModel;
        }

        /// <summary>
        /// Model转Entity 【注：本方法封装了特殊类型转化，在输出的字段名后面加双下划线“__”再所需要转化的类型即可 】
        /// 例如：字段名__OptionSetValue，字段名__EntityReference__表名，字段名__DateTime，字段名__Guid
        /// </summary>
        /// <param name="TModel">实体</param>
        /// <param name="entity">所需要返回的Entity,LogicalName和Id请自行赋值</param>
        /// <returns></returns>
        public static Entity ModelToEntityForPostfix<T>(T TModel, Entity entity)
        {
            Type type = TModel.GetType();
            foreach (var itemField in type.GetProperties())
            {
                if (itemField.GetValue(TModel, null) != null && itemField.GetValue(TModel, null).ToString() != "")
                {
                    var fieldType = itemField.PropertyType;
                    var fieldValue = itemField.GetValue(TModel, null).ToString();
                    var fieldName = itemField.Name;
                    //是否是OptionSetValue类型，双下划线
                    if (fieldName.Contains("__OptionSetValue"))
                    {
                        var keyName = fieldName.Substring(0, fieldName.IndexOf("__"));
                        entity.Attributes.Add(keyName, new OptionSetValue(int.Parse(fieldValue)));
                    }
                    else if (fieldName.Contains("__EntityReference"))
                    {
                        var keyName = fieldName.Substring(0, fieldName.IndexOf("__"));
                        var tableName = fieldName.Substring(fieldName.IndexOf("EntityReference__") + "EntityReference__".Length, fieldName.Length - fieldName.IndexOf("EntityReference__") - "EntityReference__".Length);
                        entity.Attributes.Add(keyName, new EntityReference(tableName, new Guid(fieldValue)));
                    }
                    else if (fieldName.Contains("__DateTime"))
                    {
                        var keyName = fieldName.Substring(0, fieldName.IndexOf("__"));
                        entity.Attributes.Add(keyName, DateTime.Parse(fieldValue));
                    }
                    else if (fieldName.Contains("__Guid"))
                    {
                        var keyName = fieldName.Substring(0, fieldName.IndexOf("__"));
                        entity.Attributes.Add(keyName, new Guid(fieldValue));
                    }
                    else if (fieldType == typeof(bool))
                    {
                        var keyName = fieldName.Substring(0, fieldName.IndexOf("__"));
                        entity.Attributes.Add(keyName, new Guid(fieldValue));
                    }
                    else
                    {
                        entity.Attributes.Add(fieldName, fieldValue);
                    }

                }


            }
            return entity;
        }
        /// <summary>
        /// ModelAttribute转Entity 【注：本方法封装了特殊类型转化，在输出的字段名后面加双下划线“__”再所需要转化的类型即可 】
        /// 例如：字段名__OptionSetValue，字段名__EntityReference__表名，字段名__DateTime，字段名__Guid
        /// </summary>
        /// <param name="TModelAttribute">实体继承ModelBase后的Attribute</param>
        /// <param name="entity">所需要返回的Entity,LogicalName和Id请自行赋值</param>
        /// <returns></returns>
        public static Entity ModelAttributeToEntity(Dictionary<string, object> TModelAttribute, Entity entity)
        {

            foreach (var itemField in TModelAttribute.Keys)
            {
                if (TModelAttribute[itemField] != null && TModelAttribute[itemField].ToString() != "")
                {
                    var fieldType = TModelAttribute[itemField].GetType();
                    var fieldValue = TModelAttribute[itemField].ToString();
                    //是否是OptionSetValue类型，双下划线
                    if (itemField.Contains("__OptionSetValue"))
                    {
                        var keyName = itemField.Substring(0, itemField.IndexOf("__"));
                        entity.Attributes.Add(keyName, new OptionSetValue(int.Parse(fieldValue)));
                    }
                    else if (itemField.Contains("__EntityReference"))
                    {
                        var tableName = itemField.Substring(itemField.IndexOf("EntityReference__") + "EntityReference__".Length, itemField.Length - itemField.IndexOf("EntityReference__") - "EntityReference__".Length);
                        entity.Attributes.Add(itemField, new EntityReference(tableName, new Guid(fieldValue)));
                    }
                    else if (itemField.Contains("__DateTime"))
                    {
                        var keyName = itemField.Substring(0, itemField.IndexOf("__"));
                        entity.Attributes.Add(keyName, DateTime.Parse(fieldValue));
                    }
                    else if (itemField.Contains("__Guid"))
                    {
                        var keyName = itemField.Substring(0, itemField.IndexOf("__"));
                        entity.Attributes.Add(keyName, new Guid(fieldValue));
                    }
                    else
                    {
                        entity.Attributes.Add(itemField, fieldValue);
                    }

                }


            }
            return entity;
        }
        /// <summary>
        /// 实体转Entity （需要在实体里面添加特性标签JudgeEntity.）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TModel"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Entity ModelToEntity<T>(T TModel, Entity entity)
        {
            Type type = TModel.GetType();
            foreach (var itemField in type.GetProperties())
            {
                if (itemField.GetValue(TModel, null) != null && itemField.GetValue(TModel, null).ToString() != "")
                {
                    //特殊字段类型
                    var fieldSpecialType = "";
                    //当前字段所属表名
                    var tableName = "";
                    //字段值
                    var fieldValue = itemField.GetValue(TModel, null).ToString();
                    //字段名
                    var fieldName = itemField.Name;
                    //字段类型
                    var fieldType = itemField.PropertyType;
                    var att = itemField.GetCustomAttributes(typeof(EntitySpecialType), true);
                    if (att.Length > 0)
                    {
                        var EntitySpecialType = att[0] as EntitySpecialType;
                        fieldSpecialType = EntitySpecialType.TypeName;
                        if (fieldSpecialType == "EntityReference")
                        {
                            tableName = EntitySpecialType.TableName;
                        }
                    }
                    //是否是OptionSetValue类型，双下划线
                    if (fieldSpecialType == "OptionSetValue")
                    {

                        entity.Attributes.Add(fieldName, new OptionSetValue(int.Parse(fieldValue)));
                    }
                    else if (fieldSpecialType == "EntityReference")
                    {
                        entity.Attributes.Add(fieldName, new EntityReference(tableName, new Guid(fieldValue)));
                    }
                    else if (fieldSpecialType == "DateTime")
                    {
                        entity.Attributes.Add(fieldName, DateTime.Parse(fieldValue));
                    }
                    else if (fieldSpecialType == "Guid")
                    {
                        entity.Attributes.Add(fieldName, new Guid(fieldValue));
                    }
                    else if (fieldType == typeof(bool))
                    {
                        entity.Attributes.Add(fieldName, bool.Parse(fieldValue));
                    }
                    else
                    {
                        entity.Attributes.Add(fieldName, fieldValue);
                    }

                }
            }
            return entity;
        }

        /// <summary>
        /// 委托后的ModelToEntity（测试后未提升速度还需要优化）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TModel"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Entity ModelToEntityDelegate<T>(T TModel, Entity entity)
        {
            CreateDelegate<T> testDelegate = new CreateDelegate<T>(ModelToEntity);
            testDelegate(TModel, entity);
            return entity;
        }
    }

    public static class XMLPagingHelp
    {
        /// <summary>
        /// FecthXML 分页
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="page">页码</param>
        /// <param name="count">每页个数</param>
        /// <param name="totalPagesEntity"></param>
        /// <param name="totalPagesXML"></param>
        /// <returns></returns>
        public static EntityCollection CreateXml(IOrganizationService orgService, string xml, int page, int count, out Entity totalPagesEntity,string totalPagesXML = "")
        {
            totalPagesEntity = null;
            StringReader stringReader = new StringReader(xml);
            XmlTextReader reader = new XmlTextReader(stringReader);

            // Load document
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            var resultXML = CreateXml(doc, page, count);
            RetrieveMultipleRequest fetchRequest = new RetrieveMultipleRequest
            {
                Query = new FetchExpression(resultXML)
            };
            var listData = ((RetrieveMultipleResponse)orgService.Execute(fetchRequest)).EntityCollection;
            //List<Entity> listData = new List<Entity>();
            //CRMEntityHelper.RetriveAll(resultXML, (entity) =>
            //{
            //    listData.Add(entity);
            //});
            if (totalPagesXML != "")
            {
                CRMEntityHelper CRMEntityHelper = new CRMEntityHelper(orgService);
                var totalPagesXMLResult = CRMEntityHelper.Retrive(totalPagesXML);
                //返回
                totalPagesEntity = totalPagesXMLResult == null ? null : totalPagesXMLResult;
            }
            return listData;

        }
        /// <summary>
        /// 分页查询构造
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string CreateXml(XmlDocument doc, int page, int count)
        {
            XmlAttributeCollection attrs = doc.DocumentElement.Attributes;

            //if (cookie != null)
            //{
            //    XmlAttribute pagingAttr = doc.CreateAttribute("paging-cookie");
            //    pagingAttr.Value = cookie;
            //    attrs.Append(pagingAttr);
            //}

            XmlAttribute pageAttr = doc.CreateAttribute("page");
            pageAttr.Value = System.Convert.ToString(page);
            attrs.Append(pageAttr);

            XmlAttribute countAttr = doc.CreateAttribute("count");
            countAttr.Value = System.Convert.ToString(count);
            attrs.Append(countAttr);

            StringBuilder sb = new StringBuilder(1024);
            StringWriter stringWriter = new StringWriter(sb);

            XmlTextWriter writer = new XmlTextWriter(stringWriter);
            doc.WriteTo(writer);
            writer.Close();

            return sb.ToString();
        }
    }
}
