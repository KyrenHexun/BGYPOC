using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Data;
using System.Security.Cryptography;
using System.Net;

namespace MSCRM.CRM.WorkflowActivities.BGY
{
    /// <summary>
    /// 功能描述：插件工作流多语言实现公用方法
    /// </summary>
    public static class ProgrammeHelper
    {
        /// <summary>
        /// 获取当前组织语言ID
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static int GetOrganizationBaseLanguageCode(IOrganizationService service)
        {
            QueryExpression organizationEntityQuery = new QueryExpression("organization");
            organizationEntityQuery.ColumnSet.AddColumn("languagecode");
            organizationEntityQuery.NoLock = true;
            EntityCollection organizationEntities = service.RetrieveMultiple(organizationEntityQuery);
            return (int)organizationEntities[0].Attributes["languagecode"];
        }
        /// <summary>
        /// 获取指定用户语言ID
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userId">userid</param>
        /// <returns></returns>
        public static int GetUserLanguageCode(IOrganizationService service, Guid userId)
        {
            int code = 0;
            QueryExpression userSettingsQuery = new QueryExpression("usersettings");
            userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
            userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            userSettingsQuery.NoLock = true;
            EntityCollection userSettings = service.RetrieveMultiple(userSettingsQuery);
            if (userSettings.Entities.Count > 0)
            {
                code = (int)userSettings.Entities[0]["uilanguageid"];
            }
            if (code == 0)
            {
                return GetOrganizationBaseLanguageCode(service);
            }
            else
            {
                return code;
            }
        }
        /// <summary>
        /// 通过web资源唯一名称获取资源DOM
        /// </summary>
        /// <param name="service"></param>
        /// <param name="webresourceSchemaName"></param>
        /// <returns></returns>
        public static XmlDocument GegWebResourceByName(IOrganizationService service, string webresourceSchemaName)
        {
            QueryExpression webresourceQuery = new QueryExpression("webresource");
            webresourceQuery.ColumnSet.AddColumn("content");
            webresourceQuery.NoLock = true;
            webresourceQuery.Criteria.AddCondition("name", ConditionOperator.Equal, webresourceSchemaName);
            EntityCollection webresources = service.RetrieveMultiple(webresourceQuery);
            if (webresources.Entities.Count > 0)
            {
                byte[] bytes = Convert.FromBase64String((string)webresources.Entities[0]["content"]);
                XmlDocument document = new XmlDocument();
                document.XmlResolver = null;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        document.Load(sr);
                    }
                }
                return document;
            }
            else
            {
                throw new InvalidPluginExecutionException(String.Format("Unable to locate the web resource {0}.", webresourceSchemaName));
            }

        }
        /// <summary>
        /// 通过消息key获取消息文本
        /// </summary>
        /// <param name="doc"> xml document</param>
        /// <param name="LocallizedStringKey"> message key</param>
        /// <returns></returns>
        public static string GetLocallizedString(XmlDocument doc, string LocallizedStringKey)
        {
            XmlNode valueNode = doc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "./root/data[@name='{0}']/value", LocallizedStringKey));
            if (valueNode != null)
            {
                return valueNode.InnerText;
            }
            else
            {
                throw new InvalidPluginExecutionException(String.Format("ResourceID {0} was not found.", LocallizedStringKey));
            }
        }
        /// <summary>
        ///  通过语言ID获取多语言web资源名称
        /// </summary>
        /// <param name="languagecode">language code</param>
        /// <returns></returns>
        public static String GetLanguageSouceName(int languagecode)
        {
            return String.Format("hw_/ProgrammeHelper/xml/localizedStrings{0}.xml", languagecode);
        }
        /// <summary>
        /// 通过消息key，用户ID获取对应语言的消息文本
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userid">user guid</param>
        /// <param name="LocallizedStringKey">message key</param>
        /// <returns></returns>
        public static String GetLocallizedString(IOrganizationService service, Guid userid, String LocallizedStringKey)
        {
            //int userlanguagecode = GetUserLanguageCode(service, userid);
            //String webresourceSchemaName = GetLanguageSouceName(userlanguagecode);
            //XmlDocument doc = GegWebResourceByName(service, webresourceSchemaName);
            //return GetLocallizedString(doc, LocallizedStringKey);

            int userlanguagecode = GetUserLanguageCode(service, userid);
            QueryExpression q = new QueryExpression("hw_language");
            q.Criteria.AddCondition("hw_lc", ConditionOperator.Equal, userlanguagecode);
            q.Criteria.AddCondition("hw_langkey", ConditionOperator.Equal, LocallizedStringKey);
            q.NoLock = true;
            q.ColumnSet.AddColumn("hw_name");
            EntityCollection msg = service.RetrieveMultiple(q);
            if (msg.Entities.Count > 0)
            {
                return msg.Entities[0].GetAttributeValue<string>("hw_name");
            }
            else
            {

                QueryExpression qEnglish = new QueryExpression("hw_language");
                qEnglish.Criteria.AddCondition("hw_lc", ConditionOperator.Equal, 1033);
                qEnglish.Criteria.AddCondition("hw_langkey", ConditionOperator.Equal, LocallizedStringKey);
                qEnglish.ColumnSet.AddColumn("hw_name");
                qEnglish.NoLock = true;
                EntityCollection msgEnglish = service.RetrieveMultiple(qEnglish);
                if (msgEnglish.Entities.Count > 0)
                {
                    return msgEnglish.Entities[0].GetAttributeValue<string>("hw_name");
                }
                else
                {
                    return LocallizedStringKey;
                }
            }
        }
        /// <summary>
        /// 通过语言ID，消息key获取对应的消息文本
        /// </summary>
        /// <param name="service"></param>
        /// <param name="luanguagecode">language code</param>
        /// <param name="LocallizedStringKey">message key</param>
        /// <returns></returns>
        public static String GetLocallizedString(IOrganizationService service, int luanguagecode, String LocallizedStringKey)
        {
            String webresourceSchemaName = GetLanguageSouceName(luanguagecode);
            XmlDocument doc = GegWebResourceByName(service, webresourceSchemaName);
            return GetLocallizedString(doc, LocallizedStringKey);
        }
        /// <summary>
        ///  判断实体是否包含指定的属性
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entityname">实体logicalname</param>
        /// <param name="attributename">属性logicalname</param>
        /// <returns></returns>
        public static bool EntityHasAttribute(IOrganizationService service, String entityname, String attributename)
        {
            RetrieveAttributeRequest request = new RetrieveAttributeRequest() { EntityLogicalName = entityname, LogicalName = attributename };
            try
            {
                RetrieveAttributeResponse response = (RetrieveAttributeResponse)service.Execute(request);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        ///  判断实体是否包含指定的属性
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entityname">实体logicalname</param>
        /// <param name="attributename">属性logicalname</param>
        /// <returns></returns>
        public static bool EntityHastheAttribute(IOrganizationService service, String entityname, String attributename)
        {
            bool flag = false;
            RetrieveEntityRequest request = new RetrieveEntityRequest() { LogicalName = entityname, EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes };
            RetrieveEntityResponse response = (RetrieveEntityResponse)service.Execute(request);
            AttributeMetadata[] attributes = response.EntityMetadata.Attributes;
            foreach (AttributeMetadata attr in attributes)
            {
                if (attr.LogicalName == attributename)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        ///  create a condition node from fetchxml
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="attributename">entity attribute</param>
        /// <param name="oper">operator</param>
        /// <param name="tvalue">value</param>
        /// <returns></returns>
        public static XmlNode GetConditionNode(XmlDocument doc, String attributename, String oper, String tvalue)
        {
            XmlNode condition = doc.CreateElement("condition");
            XmlAttribute attribute = doc.CreateAttribute("attribute");
            attribute.Value = attributename;
            XmlAttribute toperator = doc.CreateAttribute("operator");
            toperator.Value = oper;
            XmlAttribute thevalue = doc.CreateAttribute("value");
            thevalue.Value = tvalue;
            condition.Attributes.Append(attribute);
            condition.Attributes.Append(toperator);
            condition.Attributes.Append(thevalue);
            return condition;
        }
        /// <summary>
        /// 获取optionset标签
        /// </summary>
        /// <param name="_service"></param>
        /// <param name="entityName">实体名称</param>
        /// <param name="attributeName">optionset属性名</param>
        /// <param name="optionsetValue">optionset 属性值</param>
        /// <returns></returns>
        public static string getOptionSetText(IOrganizationService _service, string entityName, string attributeName, int optionsetValue)
        {
            string optionsetText = string.Empty;
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;

            RetrieveAttributeResponse retrieveAttributeResponse =
              (RetrieveAttributeResponse)_service.Execute(retrieveAttributeRequest);
            PicklistAttributeMetadata picklistAttributeMetadata =
              (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            OptionSetMetadata optionsetMetadata = picklistAttributeMetadata.OptionSet;

            foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
            {
                if (optionMetadata.Value == optionsetValue)
                {
                    optionsetText = optionMetadata.Label.UserLocalizedLabel.Label;
                    return optionsetText;
                }

            }
            return optionsetText;
        }
        /// <summary>
        /// 插件中获取查找字段name属性
        /// </summary>
        /// <param name="service">服务</param>
        /// <param name="entity">触发插件的实体</param>
        /// <param name="attributename">查找字段在触发插件实体中的名称</param>
        /// <param name="rfentityname">查找字段实体名称</param>
        /// <param name="rfname">查找实体name属性名称</param>
        /// <returns></returns>
        public static String GetEntityReferenceName(IOrganizationService service, Entity entity, String attributename, String rfentityname, String rfname)
        {
            Entity retentity = service.Retrieve(rfentityname, entity.GetAttributeValue<EntityReference>(attributename).Id, new ColumnSet(new String[] { rfname }));
            return retentity.GetAttributeValue<String>(rfname);
        }

        /// <summary>
        /// 生成随机文本
        /// </summary>
        /// <param name="RandomChars">可以用来生成随机文本的字符列表</param>
        /// <param name="Length">生成随机文本的数量</param>
        /// <param name="Random">随机种子</param>
        /// <returns>生成的随机文本</returns>
        private static string GenerateCoupon(string RandomChars, int Length, Random Random)
        {
            StringBuilder result = new StringBuilder(Length);
            for (int i = 0; i < Length; i++)
            {
                result.Append(RandomChars[Random.Next(RandomChars.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// 创建查找是否存在相同手机号或者邮箱
        /// 更新查找是否存在相同手机号或者邮箱
        /// </summary>
        /// <param name="_service">组织服务</param>
        /// <param name="phoneNumber">电话号码</param>
        /// <param name="emailAddress">邮箱</param>
        /// <returns></returns>
        public static bool CheckExistPhoneAndEmail(IOrganizationService _service, string phoneNumber, string emailAddress)
        {
            string phoneNumberSubFetch = "";
            if (phoneNumber != null)
            {
                phoneNumberSubFetch = "<condition attribute='telephone1' operator='eq' value='10001' />";
            }

            string emailAddressSubFetch = "";
            if (emailAddress != "")
            {
                emailAddressSubFetch = "<condition attribute='hw_email' operator='eq' value='ttt@ttt.ttt' />";
            }

            //拼接查询字符串
            string isExistFetch = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' no-lock='true'>
                      <entity name='contact'>
                        <attribute name='fullname' />
                        <attribute name='telephone1' />
                        <attribute name='contactid' />
                        <filter type='and'>
                          <filter type='or'>
                            {0}
                            {1}
                          </filter>
                        </filter>
                      </entity>
                    </fetch>", phoneNumberSubFetch, emailAddressSubFetch);

            EntityCollection isExistColl = _service.RetrieveMultiple(new FetchExpression(isExistFetch));
            if (isExistColl.Entities.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 服务请求案例创建之后，根据服务客户Lookup字段填写服务请求上的客户信息（作最初的保存）
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity">incident entity</param>
        internal static void UpdateIncidentContactInfo(IOrganizationService service, Entity entity)
        {
            EntityReference contactRef = (EntityReference)entity[""];
            Entity contactEntity = service.Retrieve(contactRef.LogicalName, contactRef.Id, new ColumnSet("fullname", "emailaddress1", "gendercode", "hw_city", "telephone2", "telephone1"));
            if (contactEntity.Attributes.Contains("") && contactEntity[""] != null)
            {
                entity[""] = contactEntity.GetAttributeValue<string>("fullname");
            }
            //throw new NotImplementedException();
            service.Update(entity);
        }

        /// <summary>
        /// 获取当前用户所在部门的默认团队
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static EntityReference GetDefaultTeam(IOrganizationService service)
        {
            var fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1' no-lock='true'>
                              <entity name='team'>
                                <attribute name='name' />
                                <attribute name='teamid' />
                                <filter type='and'>
                                  <condition attribute='teamtype' operator='eq' value='0' />
                                  <condition attribute='businessunitid' operator='eq-businessid' />
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection teams = service.RetrieveMultiple(new FetchExpression(fetchxml));
            if (teams.Entities.Count > 0)
            {
                Entity team = teams.Entities.FirstOrDefault<Entity>();
                return new EntityReference("team", team.Id);
            }
            return null;
        }
        /// <summary>
        /// 判断IMEI是否符合表达式
        /// </summary>
        /// <param name="imei"></param>
        public static void CheckIMEI(IOrganizationService service, IPluginExecutionContext context, string imei)
        {
            Match match = Regex.Match(imei.Trim(), @"^[a-zA-Z0-9]{8,30}$");
            if (match.Success == false)
                throw new InvalidPluginExecutionException(ProgrammeHelper.GetLocallizedString(service, context.UserId, "IMEIOrSNIsERROR"));
        }
        /// <summary>
        /// 单个属性不存在抛出异常信息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="attributename"></param>
        /// <param name="messagename"></param>
        public static void ShowExceptionMessage(Entity entity, Entity preentity, IOrganizationService service, IPluginExecutionContext context, String attributename, String messagename)
        {
            if (entity != null && preentity == null)
            {
                if (!entity.Contains(attributename))
                {
                    throw new InvalidPluginExecutionException(ProgrammeHelper.GetLocallizedString(service, context.UserId, messagename));
                }
            }
            if (entity != null && preentity != null)
            {
                if (!entity.Contains(attributename) && !preentity.Contains(attributename))
                {
                    throw new InvalidPluginExecutionException(ProgrammeHelper.GetLocallizedString(service, context.UserId, messagename));
                }
            }
        }
        /// <summary>
        /// 多个属性不存在抛出异常
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="attributes"></param>
        /// <param name="messages"></param>
        public static void ShowExceptionMessage(Entity entity, Entity preentity, IOrganizationService service, IPluginExecutionContext context, String[] attributes, String[] messages)
        {
            if (attributes.Length != messages.Length)
            {
                return;
            }
            if (entity != null && preentity != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (!entity.Contains(attributes[i]) && !preentity.Contains(attributes[i]))
                    {
                        throw new InvalidPluginExecutionException(ProgrammeHelper.GetLocallizedString(service, context.UserId, messages[i]));
                    }
                }
            }
            if (entity != null && preentity == null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (!entity.Contains(attributes[i]) && entity[attributes[i]] != null)
                    {
                        throw new InvalidPluginExecutionException(ProgrammeHelper.GetLocallizedString(service, context.UserId, messages[i]));
                    }
                }
            }
        }

        /// <summary>
        /// 获取CCP配置项的值
        /// </summary>
        /// <param name="service">组织服务</param>
        /// <param name="ConfigureValue">配置项名称</param>
        /// <returns>配置项的值，如果不存在返回string.Empty</returns>
        public static string GetConfigValue(IOrganizationService service, string ConfigureValue)
        {
            var returnValue = string.Empty;
            QueryExpression qe = new QueryExpression("hw_config");
            qe.ColumnSet = new ColumnSet("hw_value");
            qe.NoLock = true;
            qe.TopCount = 1;
            qe.Distinct = false;
            qe.Criteria.AddCondition("hw_name", ConditionOperator.Equal, ConfigureValue);
            var results = service.RetrieveMultiple(qe);
            if (results.Entities.Count >= 1)
            {
                returnValue = results.Entities[0].GetAttributeValue<string>("hw_value");
            }
            return returnValue;
        }

        /// <summary>
        /// 将JSON字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T FromJsonTo<T>(this string jsonString)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                T jsonObject = (T)serializer.ReadObject(ms);
                return jsonObject;
            }
        }

        /// <summary>
        /// 对象序列化为JSON文本
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ToJsJson(this object item)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(item.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, item);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// json文本转对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static T JSONToObject<T>(string jsonText)
        {
             try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonText)))
                {
                    T jsonObject = (T)serializer.ReadObject(ms);
                    return jsonObject;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Json文本转换数据表数据
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Dictionary<string, object>>> TablesDataFormJSON(string jsonText)
        {
            return JSONToObject<Dictionary<string, List<Dictionary<string, object>>>>(jsonText);
        }

        /// <summary>
        /// 根据langkey获取语言标签
        /// </summary>
        /// <param name="service">IOrganizationService</param>
        /// <param name="userId">用户GUID</param>
        /// <param name="langKey">语言标签</param>
        /// <returns></returns>
        public static string GetLanguageLabelByKey(IOrganizationService service, Guid userId, string langKey)
        {
            int lcid = GetUserLanguageCode(service, userId);
            QueryExpression queryLangRes = new QueryExpression("hw_language")
            {
                TopCount = 1,
                Distinct = false,
                NoLock = true,
                ColumnSet = new ColumnSet("hw_name"),
                Criteria =
                {
                    Filters =
                            {
                                new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("hw_langkey", ConditionOperator.Equal, langKey),
                                        new ConditionExpression("hw_lc", ConditionOperator.Equal, lcid)
                                    },
                                }
                            }
                }
            };
            EntityCollection ecLangRes = service.RetrieveMultiple(queryLangRes);

            if (ecLangRes.Entities.Count > 0)
            {
                return ecLangRes.Entities[0].GetAttributeValue<string>("hw_name");
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static String GetCrmadminid(IOrganizationService service)
        {
            String userid = "";
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
                                  <entity name='mcs_configuration'>
                                    <attribute name='mcs_configurationid' />
                                    <attribute name='mcs_administrator' />
                                    <attribute name='mcs_name' />
                                    <filter type='and'>
                                      <condition attribute='mcs_name' operator='eq' value='SystemConfiguration' />
                                      <condition attribute='statecode' operator='eq' value='0' />
                                    </filter>
                                  </entity>
                                </fetch>";
            EntityCollection settings = service.RetrieveMultiple(new FetchExpression(fetchxml));
            if (settings.Entities.Count > 0)
            {
                Entity setting = settings.Entities[0];
                userid = setting.GetAttributeValue<EntityReference>("mcs_administrator").Id.ToString();
            }
            else
            {
                //配置信息中未能找到crmadminid，请联系管理员
                throw new InvalidPluginExecutionException("配置信息中未能找到crmadminid，请联系管理员");

            }
            return userid;
        }
        /// <summary>
        /// 获取当前用户所在门店
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static EntityReference GetUserSc(IOrganizationService service, IPluginExecutionContext context)
        {
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true' >
                                  <entity name='hw_sc'>
                                    <attribute name='hw_scid' />
                                    <attribute name='hw_name' />
                                    <link-entity name='businessunit' from='hw_sc' to='hw_scid' alias='ab'>
                                      <filter type='and'>
                                        <condition attribute='businessunitid' operator='eq-businessid' />
                                      </filter>
                                    </link-entity>
                                  </entity>
                                </fetch>";
            EntityCollection scs = service.RetrieveMultiple(new FetchExpression(fetchxml));
            if (scs.Entities.Count > 0)
            {
                return new EntityReference(scs.Entities[0].LogicalName, scs.Entities[0].Id);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据编码获取门店排队业务类型
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="typecode">01 预约</param>
        /// <returns></returns>
        public static EntityReference GetScNumberType(IOrganizationService service, IPluginExecutionContext context, String typecode)
        {
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' no-lock='true'>
                                  <entity name='hw_scnumberingbustype'>
                                    <attribute name='hw_scnumberingbustypeid' />
                                    <attribute name='hw_name' />
                                    <attribute name='createdon' />
                                    <attribute name='hw_code' />
                                    <filter type='and'>
                                      <condition attribute='hw_lc' operator='eq' value='{0}' />
                                      <condition attribute='hw_code' operator='eq' value='{1}' />
                                    </filter>
                                  </entity>
                                </fetch>";
            int lcid = ProgrammeHelper.GetUserLanguageCode(service, context.UserId);
            EntityCollection scnumbertypes = service.RetrieveMultiple(new FetchExpression(String.Format(fetchxml, new Object[] { lcid, typecode })));
            if (scnumbertypes.Entities.Count > 0)
            {
                return new EntityReference(scnumbertypes.Entities[0].LogicalName, scnumbertypes.Entities[0].Id);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 判断用户是否用用某个角色
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public static bool IsUserHasRole(IOrganizationService service, String rolename)
        {
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
                                  <entity name='systemuser'>
                                    <attribute name='fullname' />
                                    <attribute name='systemuserid' />
                                    <filter type='and'>
                                      <condition attribute='systemuserid' operator='eq-userid' />
                                    </filter>
                                    <link-entity name='systemuserroles' from='systemuserid' to='systemuserid' visible='false' intersect='true'>
                                      <link-entity name='role' from='roleid' to='roleid' alias='ab'>
                                        <filter type='and'>
                                          <condition attribute='name' operator='eq' value='{0}' />
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
            EntityCollection users = service.RetrieveMultiple(new FetchExpression(String.Format(fetchxml, rolename)));
            if (users.Entities.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 判断用户是否拥有数组中的某一个角色
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userid"></param>
        /// <param name="rolenames"></param>
        /// <returns></returns>
        public static bool IsUserHasRoles(IOrganizationService service, Guid userid, String[] rolenames)
        {
            bool flag = false;
            if (rolenames == null || (rolenames != null && rolenames.Length == 0))
            {
                return false;
            }
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
                              <entity name='role'>
                                <attribute name='name' />
                                <link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'>
                                  <link-entity name='systemuser' from='systemuserid' to='systemuserid' alias='ab'>
                                    <filter type='and'>
                                      <condition attribute='systemuserid' operator='eq'  uitype='systemuser' value='{0}' />
                                    </filter>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>";
            EntityCollection roles = service.RetrieveMultiple(new FetchExpression(String.Format(fetchxml, userid)));
            if (roles.Entities.Count > 0)
            {
                foreach (Entity role in roles.Entities)
                {
                    foreach (String rolename in rolenames)
                    {
                        if (role.GetAttributeValue<String>("name").Trim() == rolename.Trim())
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            return flag;
        }
        /// <summary>
        /// 判断用户是否用用某个角色
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public static bool IsUserHasRole(IOrganizationService service, Guid userid, String rolename)
        {
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' top='1' no-lock='true'>
                                  <entity name='systemuser'>
                                    <attribute name='fullname' />
                                    <attribute name='systemuserid' />
                                    <filter type='and'>
                                      <condition attribute='systemuserid' operator='eq' uiname='test1' uitype='systemuser' value='{0}' />
                                    </filter>
                                    <link-entity name='systemuserroles' from='systemuserid' to='systemuserid' visible='false' intersect='true'>
                                      <link-entity name='role' from='roleid' to='roleid' alias='af'>
                                        <filter type='and'>
                                          <condition attribute='name' operator='eq' value='{1}' />
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
            EntityCollection users = service.RetrieveMultiple(new FetchExpression(String.Format(fetchxml, new Object[] { userid, rolename })));
            if (users.Entities.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取自动编码
        /// </summary>
        /// <param name="code">单据类型（DT，DF，SR，RN，AS，CS，VR）</param>
        /// <returns></returns>
        public static String GetAutoNumber(String code, IOrganizationService service)
        {
            string reSiteAutoNumberCode;
            if (ProgrammeHelper.GetConfigValue(service, "SiteAutoNumberCode") == string.Empty)
            {
                reSiteAutoNumberCode = "1";
            }
            else
            {
                reSiteAutoNumberCode = ProgrammeHelper.GetConfigValue(service, "SiteAutoNumberCode");
            }
            return string.Format("{0}{1}{2}{3}", code, reSiteAutoNumberCode, DateTime.Now.ToString("yyyyMMddHHmmss"), GenerateCoupon("0123456789", 3, new Random()));
        }

        public static void createprocesslog(IOrganizationService service, ProcessLogEntity pe, int Uilanguageid)
        {
            Entity et = new Entity("hw_srprocesslog");
            et["hw_businessaction"] = pe.hw_businessaction; //业务操作
            et["hw_caseorigincode"] = pe.hw_caseorigincode;//请求来源
            et["hw_changerbu"] = pe.hw_changerbu;//所属部门（门店）
            et["hw_changerrole"] = changerroleIsChinese(Uilanguageid, pe.hw_changerrole);//操作者角色
            et["hw_changetime"] = DateTime.UtcNow;//受理时间
            et["hw_channel"] = pe.hw_channel;//受理渠道
            et["hw_postowner"] = pe.hw_postowner;//之后处理人
            et["hw_poststatus"] = pe.hw_poststatus;//之后处理状态
            et["hw_preowner"] = pe.hw_preowner;//之前处理人
            et["hw_prestatus"] = pe.hw_prestatus;//之后处理状态
            et["hw_repair"] = pe.hw_repair;//工单
            et["hw_sr"] = pe.hw_sr;//服务请求
            et["hw_srowner"] = pe.hw_srowner;//服务请求负责人
            et["hw_srcategory"] = pe.hw_srcategory;
            et["hw_requirementtype"] = pe.hw_requirementtype;
            et["hw_people"] = pe.hw_people;
            //et["hw_status"] = et.FormattedValues["hw_poststatus"];
            service.Create(et);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static Entity getUserinfo(IOrganizationService service, Guid userid)
        {

            Entity userinfo = service.Retrieve("systemuser", userid, new ColumnSet(new String[] { "fullname", "businessunitid" }));
            return userinfo;
        }
        /// <summary>
        /// 获取用户门店信息
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static EntityReference getScinfo(IOrganizationService service, Guid userid)
        {
            Entity userinfo = service.Retrieve("systemuser", userid, new ColumnSet(new String[] { "fullname", "businessunitid" }));
            Entity businessuinit = service.Retrieve("businessunit", userinfo.GetAttributeValue<EntityReference>("businessunitid").Id, new ColumnSet(new String[] { "hw_sc" }));
            return businessuinit.GetAttributeValue<EntityReference>("hw_sc");
        }
        #region CreateToken zhanghuang add 20161116

        #region Filed

        /// <summary>
        /// 密钥值
        /// </summary>
        private static readonly string keyVal = "A4aIT6Z68TPno4S9sWmDw9f6fNIa";

        /// <summary>
        /// 加密辅助向量
        /// </summary>
        private static readonly string ivVal = "HE(3@dkmnc012plkndiwnsijfuk12";

        /// <summary>
        /// 请求来源
        /// </summary>
        private static readonly string source = "CCP";

        #endregion

        #region BASE64 加密解密

        /// <summary>
        /// BASE64 加密
        /// </summary>
        /// <param name="source">待加密字段</param>
        /// <returns></returns>
        private static string Base64(string source)
        {
            var btArray = Encoding.UTF8.GetBytes(source);
            return Convert.ToBase64String(btArray, 0, btArray.Length);
        }

        #endregion

        #region AES 加密

        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="source">待加密字段</param>  
        /// <param name="keyVal">密钥值</param>  
        /// <param name="ivVal">加密辅助向量</param> 
        /// <returns></returns>  
        public static string AesStr(string source)
        {
            var encoding = Encoding.UTF8;
            byte[] btKey = FormatByte(keyVal, encoding);
            byte[] btIv = FormatByte(ivVal, encoding);
            byte[] byteArray = encoding.GetBytes(source);
            string encrypt;
            Rijndael aes = Rijndael.Create();
            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(btKey, btIv), CryptoStreamMode.Write))
                {
                    cStream.Write(byteArray, 0, byteArray.Length);
                    cStream.FlushFinalBlock();
                    encrypt = Convert.ToBase64String(mStream.ToArray());
                }
            }
            aes.Clear();
            return encrypt;
        }

        private static byte[] FormatByte(string strVal, Encoding encoding)
        {
            return encoding.GetBytes(Base64(strVal).Substring(0, 16).ToUpper());
        }

        #endregion

        /// <summary>
        /// 获取访问Access Token
        /// </summary>
        /// <returns></returns>
        public static string GetAccessApiOutToken()
        {
            long timeTicks = DateTime.UtcNow.Ticks;

            string waitEncryptStr = string.Format("{0}#{1}", source, timeTicks);

            return AesStr(waitEncryptStr);
        }


        public static void SetHttpClientAuthorization(System.Net.Http.HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", GetAccessApiOutToken());
        }

        #endregion

        //创建SRLog时保证SR类型等字段为最新数据
        public static void SetSRinfoToStatusshow(Entity entity, Entity preentity, Entity srProcessLog)
        {
            if (entity.Contains("caseorigincode"))
            {
                srProcessLog.Attributes["hw_caseorigincode"] = entity.Attributes["caseorigincode"];
            }
            else if (preentity.Contains("caseorigincode"))
            {
                srProcessLog.Attributes["hw_caseorigincode"] = preentity.Attributes["caseorigincode"];
            }
            if (entity.Contains("hw_channel"))
            {
                srProcessLog.Attributes["hw_channel"] = entity.Attributes["hw_channel"];
            }
            else if (preentity.Contains("hw_channel"))
            {
                srProcessLog.Attributes["hw_channel"] = preentity.Attributes["hw_channel"];
            }
            if (entity.Contains("hw_requirementtype"))
            {
                srProcessLog.Attributes["hw_requirementtype"] = entity.Attributes["hw_requirementtype"];
            }
            else if (preentity.Contains("hw_requirementtype"))
            {
                srProcessLog.Attributes["hw_requirementtype"] = preentity.Attributes["hw_requirementtype"];
            }
            if (entity.Contains("hw_srcategory"))
            {
                srProcessLog.Attributes["hw_srcategory"] = entity.Attributes["hw_srcategory"];
            }
            else if (preentity.Contains("hw_srcategory"))
            {
                srProcessLog.Attributes["hw_srcategory"] = preentity.Attributes["hw_srcategory"];
            }
        }

        public static void SetSRLogInfo(IOrganizationService _service, Entity entity, Entity preentity, Entity user, Entity srProcessLog, int Uilanguageid)
        {
            srProcessLog.Attributes["hw_changetime"] = DateTime.UtcNow;//操作时间
            srProcessLog.Attributes["hw_people"] = new EntityReference("systemuser", user.Id);//操作者
            EntityReference sc = ProgrammeHelper.getScinfo(_service, user.Id);
            if (sc != null)
            {
                srProcessLog.Attributes["hw_changerbu"] = sc.Name;//操作者所属BU
            }
            string role = "";
            int preStatusVale = preentity.GetAttributeValue<OptionSetValue>("statuscode").Value;
            if (preStatusVale == 3)
            {
                role = "华为服务工程师";
            }
            else if (preStatusVale == 100000000)
            {
                role = "高级客户经理";
            }
            else
            {
                role = user.FormattedValues.Contains("hw_position") ? user.FormattedValues["hw_position"].ToString() : "";
            }
            srProcessLog.Attributes["hw_changerrole"] = changerroleIsChinese(Uilanguageid, role);//操作者角色
            srProcessLog.Attributes["hw_preowner"] = preentity.GetAttributeValue<EntityReference>("ownerid").Name;//之前处理人
            srProcessLog.Attributes["hw_prestatus"] = preentity.GetAttributeValue<OptionSetValue>("statuscode");// getStatusText(service, entity.LogicalName, "statuscode", preImageSR.GetAttributeValue<OptionSetValue>("statuscode").Value);//之前状态
            srProcessLog.Attributes["hw_sr"] = new EntityReference("incident", entity.Id);//SR
        }
        /// <summary>
        /// 验证工单物料,如果存在数据错误的物料，则修复
        /// 示例1: ValidationItem(service, "RN20170222120039645", "hw_name,hw_repair");
        /// 示例2: ValidationItem(service, new Guid("da213d75-b3f8-e611-80cb-fb6b35d41b6c"), "hw_name,createdon");
        /// </summary>
        /// <param name="service">组织服务</param>
        /// <param name="RepairNoOrRepairGuid">工单号(字符串)或者工单的GUID(主键)如,"RN20170222120039645" 或者 Entity.Id</param> 
        ///  <param name="column">需要返回工单物料的字段用逗号分开,如 "hw_name,hw_type"</param> 
        /// <returns>返回工单对象的集合</returns> 
        public static EntityCollection ValidationItem(IOrganizationService service, object RepairNoOrRepairGuid, string column = null)
        {
            //自定义列
            string columnStr = "";
            if (!string.IsNullOrWhiteSpace(column))
            {
                var ColList = column.Split(',');
                for (int i = 0; i < ColList.Length; i++)
                {
                    string str = ColList[i].Trim().Replace("'", "");
                    if (str != "hw_repairitemid" && str != "hw_type" && str != "hw_replaceditem")
                    {
                        columnStr += " <attribute name='" + str + "' /> ";
                    }
                }
            }

            EntityCollection RetEntity = new EntityCollection();
            StringBuilder fetchxml = new StringBuilder();
            fetchxml.Append(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' no-lock='true' >
                              <entity name='hw_repairitem'>
                                <attribute name='hw_repairitemid' /> 
                                <attribute name='hw_type' /> 
                                <attribute name='hw_replaceditem' /> ");
            fetchxml.Append(columnStr);
            if (RepairNoOrRepairGuid.GetType() == typeof(string))
            {
                fetchxml.Append(@"<filter type='and'> 
                                  <condition attribute='hw_isestimated' operator='eq' value='0' />
                                </filter> 
<link-entity name='hw_repair' from='hw_repairid' to='hw_repair' alias='aa'>
                                                      <filter type='and'>
                                                        <condition attribute='hw_name' operator='eq' value='");
                fetchxml.Append(RepairNoOrRepairGuid);
                fetchxml.Append(@"' />
                                                      </filter>
                                                    </link-entity>
                                                  </entity>
                                                </fetch>");
            }
            else if (RepairNoOrRepairGuid.GetType() == typeof(Guid))
            {
                fetchxml.Append(@"    <filter type='and'>
                                  <condition attribute='hw_repair' operator='eq'  uitype='hw_repair' value='");
                fetchxml.Append(RepairNoOrRepairGuid);
                fetchxml.Append(@"' />
                                  <condition attribute='hw_isestimated' operator='eq' value='0' />
                                </filter>
                              </entity>
                            </fetch>");
            }
            else
            {
                return RetEntity;
            }


            EntityCollection repairitem = service.RetrieveMultiple(new FetchExpression(fetchxml.ToString()));

            var LinqItem = repairitem.Entities.AsQueryable();
            var replacedNull = LinqItem.Where(w => !w.Contains("hw_replaceditem")).Select(w => w.Id);

            if (replacedNull.Count() > 0)
            {
                var replacedNullList = replacedNull.ToList();
                for (int i = 0; i < repairitem.Entities.Count; i++)
                {
                    if (repairitem.Entities[i].Contains("hw_replaceditem") && replacedNullList.Contains(((EntityReference)repairitem.Entities[i].Attributes["hw_replaceditem"]).Id))
                    {
                        replacedNullList.Add(repairitem.Entities[i].Id);
                    }
                }
                //var replacedNull2 = LinqItem.Where(w => w.Contains("hw_replaceditem") && replacedNullList.Contains(((EntityReference)w.Attributes["hw_replaceditem"]).Id)); //对应的Item
                //if (replacedNull2.Count() > 0) //如果有工单物料引用了错误的数据，则将两条一起删除
                //{
                //    replacedNullList.AddRange(replacedNull2.Select(w => w.Id).ToList());
                //}
                for (int i = 0; i < replacedNullList.Count; i++)
                {
                    service.Delete("hw_repairitem", replacedNullList[i]);
                }
                //RetEntity.Entities.AddRange(LinqItem.Where(w => !replacedNullList.Contains(w.Id)).ToArray<Entity>());
            }
            else
            {
                RetEntity = repairitem;
            }

            return RetEntity;

        }

        /// <summary>
        /// 获取岗位信息
        /// </summary>
        /// <param name="_service"></param>
        /// <param name="ownerid"></param>
        /// <returns></returns>
        public static int GetUserPosition(IOrganizationService _service, Guid ownerid)
        {
            Entity user = _service.Retrieve("systemuser", ownerid, new ColumnSet("hw_position"));
            if (user.Contains("hw_position"))
            {
                return user.GetAttributeValue<OptionSetValue>("hw_position").Value;
            }
            else
            {
                return 0;
            }
        }

        public static string GetApiUrl(IOrganizationService service)
        {
            string apiUrl = string.Empty;
            QueryExpression query = new QueryExpression("hw_config");
            query.Criteria.AddCondition("hw_name", ConditionOperator.Equal, "apiurl");
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            query.ColumnSet.AddColumn("hw_value");
            query.NoLock = true;
            EntityCollection configs = service.RetrieveMultiple(query);
            if (configs.Entities.Count > 0)
            {
                apiUrl = configs.Entities[0].GetAttributeValue<string>("hw_value");
            }
            return apiUrl;

        }

        /// <summary>
        /// 判断当前操作语言是否中文,返回操作角色
        /// </summary>
        /// <param name="minLen"></param>
        /// <param name="maxLen"></param>
        /// <returns></returns>
        public static string changerroleIsChinese(int uilanguageid, string rolename)
        {
            string returnRoleName = "";
            if (uilanguageid == 2052)//中文 返回本身。
            {
                returnRoleName = rolename;
            }
            else
            {
                switch (rolename)
                {
                    case "系统管理员":
                        returnRoleName = "System Administrator";
                        break;
                    case "L1客服":
                        returnRoleName = "L1";
                        break;
                    case "L2客服":
                        returnRoleName = "L2";
                        break;
                    case "L1客服组长":
                        returnRoleName = "L1 Leader";
                        break;
                    case "L2客服组长":
                        returnRoleName = "L2 Leader";
                        break;
                    case "QA专员":
                        returnRoleName = "QA Specialist";
                        break;
                    case "代表处服务经理":
                        returnRoleName = "Rep Office Service MGR";
                        break;
                    case "店长":
                        returnRoleName = "SC MGR";
                        break;
                    case "调查专员":
                        returnRoleName = "Survey Specialist";
                        break;
                    case "调查组长":
                        returnRoleName = "Survey Leader";
                        break;
                    case "高级客户经理":
                        returnRoleName = "Sr Account MGR";
                        break;
                    case "高级客户经理组长":
                        returnRoleName = "Sr Account MGR Leader";
                        break;
                    case "供应商经理":
                        returnRoleName = "Supplier MGR";
                        break;
                    case "供应商运营经理":
                        returnRoleName = "Supplier Operation MGR";
                        break;
                    case "国家服务经理":
                        returnRoleName = "Country Service MGR";
                        break;
                    case "华为服务工程师":
                        returnRoleName = "HUAWEI Service Engineer";
                        break;
                    case "华为服务工程师组长":
                        returnRoleName = "HUAWEI Service Engineer Leader";
                        break;
                    case "热线服务经理":
                        returnRoleName = "Hotline Service MGR";
                        break;
                    case "受理员":
                        returnRoleName = "Representative";
                        break;
                    case "投诉专员":
                        returnRoleName = "Complaint Specialist";
                        break;
                    case "投诉组长":
                        returnRoleName = "Country Service MGR";
                        break;
                    case "维修工程师":
                        returnRoleName = "Repair Engineer";
                        break;
                    case "检测工程师":
                        returnRoleName = "Repair Engineer";
                        break;
                    case "物流代表":
                        returnRoleName = "Logistics Rep";
                        break;
                    case "线上服务代表":
                        returnRoleName = "Online Service Rep";
                        break;
                    case "业务管理员":
                        returnRoleName = "Business Administrator";
                        break;
                    case "CP系统":
                        returnRoleName = "CP System";
                        break;
                    case "CCP系统":
                        returnRoleName = "CCP System";
                        break;
                    case "物流专员":
                        returnRoleName = "Logistics Rep";
                        break;
                    default:
                        returnRoleName = rolename;   //不知道翻译的暂时用汉语
                        break;
                }
            }
            return returnRoleName;
        }
        /// <summary>
        /// 设置OwnerId为国家团队 created by kevinchow 20170713
        /// </summary>
        /// <param name="currentEntity"></param>
        /// <param name="adminservice"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        //public static string[] SetOwneridForCountry(Entity currentEntity, IOrganizationService adminservice, IOrganizationService service, Guid UserId)
        //{
        //    string[] resultString = new string[2];
        //    //判断服务用户的国家值是否为空，为空不做操作
        //    if (currentEntity.Contains("hw_country") && currentEntity.GetAttributeValue<EntityReference>("hw_country") != null)
        //    {
        //        var configName = "region";
        //        #region 获取系统配置文件实体的region参数
        //        #region 根据参数名称查找参数信息
        //        var refetchxml = string.Format(@"<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0 ' count='1' no-lock='true'>
        //                                                    <entity name='hw_config'>
        //                                                        <attribute name='hw_configid'/>
        //                                                        <attribute name='hw_name'/>
        //                                                        <attribute name='hw_value'/>
        //                                                        <filter type='and'>
        //                                                            <condition attribute='hw_name' value='{0}' operator='eq'/>
        //                                                        </filter>
        //                                                    </entity>
        //                                                   </fetch>", configName);
        //        EntityCollection returnLists = adminservice.RetrieveMultiple(new FetchExpression(refetchxml));
        //        #endregion
        //        #region 判断获取的参数信息
        //        //参数配置表是否找到参数信息
        //        if (returnLists.Entities.Count == 0)
        //        {
        //            resultString[0] = "false";
        //            resultString[1] = "can not found " + configName + " configure";
        //            return resultString;
        //        }
        //        String regionName = returnLists.Entities[0].GetAttributeValue<String>("hw_value");
        //        //判参数值是否为空
        //        if (regionName == null)
        //        {
        //            resultString[0] = "false";
        //            resultString[1] = configName + " configure value is null";
        //            return resultString;
        //        }
        //        //判断地区是否为亚太区
        //        if (regionName.ToString() == "1")
        //        {
        //            resultString[0] = "false";
        //            resultString[1] = configName + " configure value for 1(China)";
        //            return resultString;
        //        }
        //        #endregion
        //        #endregion
        //        #region 废弃-判断并获取服务用户的国家字段名称
        //        //string country = "";
        //        //if (currentEntity.GetAttributeValue<EntityReference>("hw_country").Name == null)
        //        //{
        //        //    Entity countryEntity = service.Retrieve("hw_country", currentEntity.GetAttributeValue<EntityReference>("hw_country").Id, new ColumnSet("hw_name"));
        //        //    if (countryEntity.Contains("hw_name") && countryEntity["hw_name"] != null)
        //        //    {
        //        //        country = countryEntity.GetAttributeValue<string>("hw_name");
        //        //    }
        //        //    else
        //        //    {
        //        //        resultString[0] = "false";
        //        //        resultString[1] = "can not found country name";
        //        //        return resultString;
        //        //    }
        //        //}
        //        //else country = currentEntity.GetAttributeValue<EntityReference>("hw_country").Name.ToString();
        //        ////国家名称+当前用户语言id 来获取对应的国家团队 created by kevinchow 20171123
        //        //int langCode = GetUserLanguageCode(service, UserId);
        //        //country += "(" + langCode.ToString() + ")";
        //        #endregion

        //        #region 根据国家id，查找对应的国家编码和国家语言
        //        string cLan = "";
        //        string countryCode = "";
        //        Entity countryEntity = service.Retrieve("hw_country", currentEntity.GetAttributeValue<EntityReference>("hw_country").Id, new ColumnSet("hw_alpha2code", "hw_lc"));
        //        if (countryEntity.Contains("hw_lc") && countryEntity.Contains("hw_alpha2code"))
        //        {
        //            countryCode = countryEntity.GetAttributeValue<string>("hw_alpha2code");
        //            cLan = countryEntity.GetAttributeValue<int>("hw_lc").ToString();
        //        }
        //        else
        //        {
        //            resultString[0] = "false";
        //            resultString[1] = "can not found country info";
        //            return resultString;
        //        }
        //        #endregion


        //        #region 根据国家编码去查找对应的国家团队，并根据国家的语言来找到相应的团队
        //        var teamxml = string.Format(@"<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0' no-lock='true'>
        //                                        <entity name='team'>
        //                                            <attribute name='name'/>
        //                                            <attribute name='teamid'/>
        //                                            <filter type='and'>
        //                                                <condition attribute='hw_countrycode' value='{0}' operator='eq'/>
        //                                            </filter>
        //                                        </entity>
        //                                      </fetch>", countryCode);
        //        EntityCollection teamLists = adminservice.RetrieveMultiple(new FetchExpression(teamxml));
        //        resultString[0] = "true";
        //        if (teamLists.Entities.Count > 0)
        //        {
        //            bool _sign = false;
        //            foreach (Entity _team in teamLists.Entities)
        //            {
        //                if (_team.Contains("name") && _team.GetAttributeValue<string>("name") != null)
        //                {
        //                    string[] _param = _team.GetAttributeValue<string>("name").Split(new char[2] { '(', ')' });
        //                    if (_param.Count() != 3 || _param[1] == "" || _param[1] != cLan)
        //                    {
        //                        continue;
        //                    }
        //                    AssignRequest incidentResqest = new AssignRequest()//服务请求
        //                    {
        //                        //实体
        //                        Target = new EntityReference(currentEntity.LogicalName, currentEntity.Id),
        //                        //用户
        //                        Assignee = new EntityReference("team", _team.Id)
        //                    };
        //                    adminservice.Execute(incidentResqest);//分派门店团队
        //                    resultString[1] = "setting success";
        //                    _sign = true;
        //                    break;
        //                }
        //            }
        //            if (!_sign) {
        //                AssignRequest incidentResqest = new AssignRequest()//服务请求
        //                {
        //                    //实体
        //                    Target = new EntityReference(currentEntity.LogicalName, currentEntity.Id),
        //                    //用户
        //                    Assignee = new EntityReference("team", teamLists.Entities[0].Id)
        //                };
        //                adminservice.Execute(incidentResqest);//分派门店团队
        //                resultString[1] = "setting success";
        //            }

        //        }
        //        else
        //        {
        //            resultString[1] = "unable to find a matching team";
        //        }
        //        return resultString;
        //        #endregion
        //    }
        //    else
        //    {
        //        resultString[0] = "false";
        //        resultString[1] = "contact country is null";
        //        return resultString;
        //    }
        //}

        /// <summary>
        /// 共享服务用户到SR国家团队
        /// </summary>
        /// <param name="adminservice"></param>
        /// <param name="service"></param>
        /// <param name="incidentId"></param>
        /// <returns></returns>
        //public static string ShareContactToCountryTeam(IOrganizationService adminservice, Guid incidentId)
        //{
        //    //成功信息 默认000：代表成功  其他信息具体显示
        //    string returnStr = "000";
        //    //获取国内外版本设置
        //    string overSeaVersion = GetOverSeaVersion(adminservice);
        //    //SR国家GUID                    服务用户国家GUID
        //    Guid srCountry = new Guid(); Guid contactCountry = new Guid();
        //    //国家名
        //    string srCountryName = "";
        //    //服务用户GUID
        //    Guid customerid = new Guid();
        //    //当前版本为海外版   1：海外版code
        //    if (overSeaVersion == "1")
        //    {
        //        #region //根据incidentid 获取 SR国家及SR服务用户的国家
        //        string incidentFetch = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        //                                  <entity name='incident'>
        //                                    <attribute name='incidentid' />
        //                                    <attribute name='hw_country' />
        //                                    <attribute name='customerid' />
        //                                    <filter type='and'>
        //                                      <condition attribute='incidentid' operator='eq' uitype='incident' value='{0}' />
        //                                    </filter>
        //                                    <link-entity name='contact' from='contactid' to='customerid' visible='false' link-type='outer'>
        //                                      <attribute name='hw_country' alias='contact_country' />
        //                                    </link-entity>
        //                                  </entity>
        //                                </fetch>", incidentId.ToString());
        //        EntityCollection incidentColl = adminservice.RetrieveMultiple(new FetchExpression(incidentFetch));
        //        if (incidentColl.Entities.Count > 0)
        //        {
        //            Entity incidentEntity = incidentColl.Entities[0];
        //            customerid = incidentEntity.GetAttributeValue<EntityReference>("customerid").Id;
        //            srCountry = incidentEntity.Contains("hw_country") ? incidentEntity.GetAttributeValue<EntityReference>("hw_country").Id : new Guid();
        //            srCountryName = incidentEntity.Contains("hw_country") ? incidentEntity.GetAttributeValue<EntityReference>("hw_country").Name : "";
        //            contactCountry = incidentEntity.Contains("contact_country") ? ((EntityReference)incidentEntity.GetAttributeValue<AliasedValue>("contact_country").Value).Id : new Guid();
        //        }
        //        #endregion
        //        #region //判断国家是否一致，不一致则share服务用户到SR国家
        //        //
        //        if (srCountry != new Guid() && contactCountry != new Guid() && srCountry != contactCountry)
        //        {
        //            //根据SR国家获取团队
        //            if (string.IsNullOrEmpty(srCountryName))
        //            {
        //                Entity countryEntity = adminservice.Retrieve("hw_country", srCountry, new ColumnSet("hw_name"));
        //                srCountryName = countryEntity.GetAttributeValue<string>("hw_name");
        //            }
        //            //查询国家团队fetch
        //            string teamFetch = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
        //                                                  <entity name='team'>
        //                                                    <attribute name='name' />
        //                                                    <attribute name='teamid' />
        //                                                    <filter type='and'>
        //                                                      <condition attribute='name' operator='eq' value='{0}' />
        //                                                    </filter>
        //                                                  </entity>
        //                                                </fetch>", srCountryName);
        //            EntityCollection teamColl = adminservice.RetrieveMultiple(new FetchExpression(teamFetch));
        //            if (teamColl.Entities.Count > 0)
        //            {
        //                Entity teamEntity = teamColl.Entities[0];
        //                #region 共享服务用户到国家团队
        //                var teamReference = new EntityReference("team", teamEntity.Id);
        //                //new share request
        //                var grantAccessRequest = new GrantAccessRequest
        //                {
        //                    PrincipalAccess = new PrincipalAccess
        //                    {
        //                        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess,
        //                        Principal = teamReference
        //                    },
        //                    Target = new EntityReference("contact", customerid)
        //                };
        //                //执行share
        //                adminservice.Execute(grantAccessRequest);
        //                #endregion
        //            }
        //            else
        //            {
        //                returnStr = "未找到对应的国家团队";
        //            }
        //        }
        //        #endregion
        //    }

        //    return returnStr;
        //}

        /// <summary>
        /// 获取是否是海外版
        /// </summary>
        /// <param name="adminservice"></param>
        /// <returns></returns>
        public static string GetOverSeaVersion(IOrganizationService adminservice)
        {
            //默认版本为国内版
            string version = "0";
            //是否海外版本，0代表否，也就是国内版本，1代表海外版本，程序中请统一使用是否等于0来判断是否海外版本
            var configName = "IsOverSeasVersion";
            #region 根据参数名称查找参数信息
            var refetchxml = string.Format(@"<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0 ' count='1' no-lock='true'>
                                                            <entity name='hw_config'>
                                                                <attribute name='hw_configid'/>
                                                                <attribute name='hw_name'/>
                                                                <attribute name='hw_value'/>
                                                                <filter type='and'>
                                                                    <condition attribute='hw_name' value='{0}' operator='eq'/>
                                                                </filter>
                                                            </entity>
                                                           </fetch>", configName);
            EntityCollection returnLists = adminservice.RetrieveMultiple(new FetchExpression(refetchxml));
            #endregion

            //参数配置表是否找到参数信息
            if (returnLists.Entities.Count == 0)
            {
                return version;
            }
            //获取第一条记录为
            Entity configEntity = returnLists.Entities[0];
            //获取当前版本
            version = configEntity.Contains("hw_value") ? configEntity.GetAttributeValue<string>("hw_value") : "0";

            return version;
        }

        /// <summary>
        /// online的生产环境证书需要指定SecurityProtocol by yuzelong 2017.9.9
        /// </summary>
        /// <param name="adminservice"></param>
        public static void SetSecurityProtocol(IOrganizationService service)
        {
            if (GetOverSeaVersion(service) == "1")
            {
                //为美西站点兼容证书版本 by yuzelong 2018.1.31
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }
        }

        /// <summary>
        /// 获取用户的时区
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userId">userid</param>
        /// <returns></returns>
        public static int GetUserTimeZoneCode(IOrganizationService service, Guid userId)
        {
            QueryExpression userSettingsQuery = new QueryExpression("usersettings");
            userSettingsQuery.NoLock = true;
            userSettingsQuery.ColumnSet.AddColumns("timezonebias", "systemuserid");
            userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            EntityCollection userSettings = service.RetrieveMultiple(userSettingsQuery);
            if (userSettings.Entities.Count > 0)
            {
                return (int)userSettings.Entities[0]["timezonebias"];
            }
            return 0;
        }

        /// <summary>
        /// 维修方法适用校验：适用产品族、适用国家、适用维修方案、门店授权维修等级 2017//11/02 李文涛
        /// </summary>
        /// <param name="_service"></param>
        /// <param name="flag"></param>
        /// <param name="entity">repair</param>
        public static void CheckThisRepairMethod(IOrganizationService _service, string flag, Entity entity)
        {

            if (entity.Contains("hw_repairmethod") && entity.GetAttributeValue<EntityReference>("hw_repairmethod") != null)
            {
                //维修方法id
                Guid repairmethodID = entity.GetAttributeValue<EntityReference>("hw_repairmethod").Id;

                //维修方法名称
                String methodName = "";
                if (entity.Contains("hw_incrementrepair") && entity.GetAttributeValue<EntityReference>("hw_incrementrepair") != null)
                    methodName = entity.GetAttributeValue<string>("hw_name").ToString();

                //工单ID
                Guid repairID = Guid.Empty;
                if (flag == "yes")
                {
                    //增值服务工单ID
                    if (entity.Contains("hw_incrementrepair") && entity.GetAttributeValue<EntityReference>("hw_incrementrepair") != null)
                        repairID = entity.GetAttributeValue<EntityReference>("hw_incrementrepair").Id;
                    else
                        throw new InvalidPluginExecutionException("'incrementrepair' does not exist.");
                }
                else
                {
                    //工单id
                    if (entity.Contains("hw_repair") && entity.GetAttributeValue<EntityReference>("hw_repair") != null)
                        repairID = entity.GetAttributeValue<EntityReference>("hw_repair").Id;
                    else
                        throw new InvalidPluginExecutionException("repair does not exist.");
                }


                //维修方法适用产品族
                string fetchGetProduct = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
                                                              <entity name='hw_repairmethodprofamily'>
                                                                <attribute name='hw_repairmethodprofamilyid' />
                                                                <attribute name='hw_name' />
                                                                <filter type='and'>
                                                                  <condition attribute='hw_repairmethod' operator='eq' value='{0}'/>
                                                                </filter>
                                                                {1}
                                                              </entity>
                                                            </fetch>", repairmethodID, repairID);
                if (flag == "yes")
                {//增值服务工单
                    fetchGetProduct = string.Format(fetchGetProduct, repairmethodID, @"<filter type='and'>
                                                                    <condition attribute='statecode' operator='eq' value='0' />
                                                                  </filter>
                                                                  <link-entity name='hw_productseries' from='hw_productfamily' to='hw_productfamilyid' alias='ah'>
                                                                    <filter type='and'>
                                                                      <condition attribute='statecode' operator='eq' value='0' />
                                                                    </filter>
                                                                    <link-entity name='hw_incrementrepair' from='hw_productseries' to='hw_productseriesid' alias='ai'>
                                                                      <filter type='and'>
                                                                        <condition attribute='hw_incrementrepairid' operator='eq'  value='{0}'/>
                                                                      </filter>
                                                                    </link-entity>
                                                                  </link-entity>
                                                                </link-entity>");

                }
                else
                {//工单
                    fetchGetProduct = string.Format(fetchGetProduct, repairmethodID, @"<link-entity name='hw_productfamily' from='hw_productfamilyid' to='hw_productfamily' alias='as'>
                                                                      <link-entity name='hw_repair' from='hw_productfamily' to='hw_productfamilyid' alias='at'>
                                                                      <filter type='and'>
                                                                      <condition attribute='hw_repairid' operator='eq' value='{0}' />
                                                                      </filter>
                                                                      </link-entity>
                                                                      </link-entity>");
                }

                fetchGetProduct = string.Format(fetchGetProduct, repairID);
                EntityCollection ProductGet = _service.RetrieveMultiple(new FetchExpression(fetchGetProduct));
                if (!(ProductGet.Entities.Count > 0))
                {
                    throw new InvalidPluginExecutionException(string.Format("Maintenance method {0},Not applicable to current product family.", methodName));
                }

                //维修方法适用国家
                string fetchGetcountry = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
  <entity name='hw_repairmethodcountry'>
    <attribute name='hw_repairmethodcountryid' />
    <filter type='and'>
      <condition attribute='hw_repairmethod' operator='eq' value='{0}'/>
    </filter>
    <link-entity name='hw_country' from='hw_countryid' to='hw_country' alias='aj'>
      <filter type='and'>
        <condition attribute='statecode' operator='eq' value='0' />
      </filter>
       {1}
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>", repairmethodID, repairID);
                if (flag == "yes")
                {//增值服务工单
                    fetchGetcountry = string.Format(fetchGetcountry, repairmethodID, @"<link-entity name='hw_incrementrepair' from='hw_country' to='hw_countryid' alias='ak'>
                                                                                <filter type='and'>
                                                                                 <condition attribute='hw_incrementrepairid' operator='eq' value='{0}'/>");

                }
                else
                {//工单
                    fetchGetcountry = string.Format(fetchGetcountry, repairmethodID, @"<link-entity name='hw_repair' from='hw_country' to='hw_countryid' alias='ak'>
                                                                                <filter type='and'>
                                                                                 <condition attribute='hw_repairid' operator='eq' value='{0}'/>");
                }

                fetchGetcountry = string.Format(fetchGetcountry, repairID);
                EntityCollection CountryGet = _service.RetrieveMultiple(new FetchExpression(fetchGetProduct.ToString()));
                if (!(CountryGet.Entities.Count > 0))
                {
                    throw new InvalidPluginExecutionException(string.Format("Maintenance method {0},Not applicable to current country.", methodName));
                }



                //维修方法对应维修方案
                string fetchGetmethod = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
  <entity name='hw_repairmethodplan'>
    <attribute name='hw_repairmethodplanid' />
    <attribute name='hw_name' />
    <attribute name='createdon' />
    <order attribute='hw_name' descending='false' />
    <filter type='and'>
      <condition attribute='hw_repairmethod' operator='eq'  value='{0}'/>
    </filter>
    <link-entity name='hw_repairplan' from='hw_repairplanid' to='hw_repairplan' alias='al'>
      <filter type='and'>
        <condition attribute='statecode' operator='eq' value='0' />
      </filter>
       {1}
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>", repairmethodID, repairID);
                if (flag == "yes")
                {//增值服务工单
                    fetchGetmethod = string.Format(fetchGetmethod, repairmethodID, @"<link-entity name='hw_incrementrepair' from='hw_finalrepairsolution' to='hw_repairplanid' alias='am'>
                                                                              <filter type='and'>
                                                                  <condition attribute='hw_incrementrepairid' operator='eq' value='{0}'/>");

                }
                else
                {//工单
                    fetchGetmethod = string.Format(fetchGetmethod, repairmethodID, @"<link-entity name='hw_repair' from='hw_repairsolution' to='hw_repairplanid' alias='am'>
                                                                                <filter type='and'>
                                                                                  <condition attribute='hw_repairid' operator='eq'  value='{0}' />");
                }

                fetchGetmethod = string.Format(fetchGetmethod, repairID);
                EntityCollection Getmethod = _service.RetrieveMultiple(new FetchExpression(fetchGetProduct.ToString()));
                if (!(Getmethod.Entities.Count > 0))
                {
                    throw new InvalidPluginExecutionException(string.Format("Maintenance method {0}, Not applicable to current maintenance program.", methodName));
                }



                //维修方法对应门店授权维修等级
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
  <entity name='hw_repairmethod'>
    <attribute name='hw_repairmethodid' />
    <attribute name='hw_name' />
    <attribute name='createdon' />
    <order attribute='hw_name' descending='false' />
    <filter type='and'>
      <condition attribute='hw_repairmethodid' operator='eq'  value='{0}' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
    <link-entity name='hw_repairlevel' from='hw_repairlevelid' to='hw_repairlevel' alias='ae'>
      <link-entity name='hw_screpairlevel' from='hw_repairlevel' to='hw_repairlevelid' alias='af'>
          {1}
            </filter>
          </link-entity>
        </link-entity>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";
                if (flag == "yes")
                {//增值服务工单
                    fetchXML = string.Format(fetchXML, repairmethodID, @"<link-entity name='hw_sc' from='hw_scid' to='hw_sc' alias='ag'>
                                     <link-entity name='hw_incrementrepair' from='hw_sc' to='hw_scid' alias='ah'>
                                       <filter type='and'>
                                         <condition attribute='hw_incrementrepairid' operator='eq'  value='{0}' />");

                }
                else
                {//工单
                    fetchXML = string.Format(fetchXML, repairmethodID, @"<link-entity name='hw_sc' from='hw_scid' to='hw_sc' alias='ag'>
                                         <link-entity name='hw_repair' from='hw_sc' to='hw_scid' alias='ah'>
                                           <filter type='and'>
                                             <condition attribute='hw_repairid' operator='eq'  value='{0}' />");
                }

                fetchXML = string.Format(fetchXML, repairID);
                EntityCollection repairlevel = _service.RetrieveMultiple(new FetchExpression(fetchXML.ToString()));
                if (!(repairlevel.Entities.Count > 0))
                {
                    throw new InvalidPluginExecutionException(string.Format("Maintenance method {0},Does not match with the current store maintenance level.", methodName));
                }
            }
            else
            {
                throw new InvalidPluginExecutionException("Maintenance method is empty.");
            }
        }
    }

    public class ProcessLogEntity
    {
        public OptionSetValue hw_businessaction { get; set; }
        public OptionSetValue hw_caseorigincode { get; set; }
        public String hw_changerbu { get; set; }
        public String hw_changerrole { get; set; }
        public OptionSetValue hw_channel { get; set; }
        public String hw_postowner { get; set; }
        public OptionSetValue hw_poststatus { get; set; }
        public String hw_preowner { get; set; }
        public OptionSetValue hw_prestatus { get; set; }
        public EntityReference hw_repair { get; set; }
        public EntityReference hw_sr { get; set; }
        public String hw_srowner { get; set; }
        //用户需求类型
        public OptionSetValue hw_requirementtype { get; set; }

        public EntityReference hw_people { get; set; }

        public EntityReference hw_srcategory { get; set; }
    }
}