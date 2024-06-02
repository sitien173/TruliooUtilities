namespace TruliooExtension.Common
{
    using System;
    using System.Text.Json.Serialization;
    using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;

    public class AppSettings
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("DbName")]                    public string DbName { get; set; }                   
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("AssemblyName")]              public string AssemblyName { get; set; }             
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("DefaultCulture")]            public string DefaultCulture { get; set; }           
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("CustomFieldMatchTemplate")]  public string CustomFieldMatchTemplate { get; set; } 
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("AdminPortalEndpoint")]       public Uri AdminPortalEndpoint { get; set; }         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("LastFetchDatasourceID")]       public string LastFetchDatasourceID { get; set; }         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("LastUpdatedDatasourceID")]       public string LastUpdatedDatasourceID { get; set; }         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("GetDatasourcePath")]       public string GetDatasourcePath { get; set; }         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("UpdateDatasourcePath")]       public string UpdateDatasourcePath { get; set; }         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("CustomFieldGroupGlobalKey")] public string CustomFieldGroupGlobalKey { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("Tables")]                    public Tables Tables { get; set; }                   
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("Command")]                   public Command Command { get; set; }                 
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("ContextMenusID")]            public ContextMenusId ContextMenusId { get; set; }   
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("MessageAction")]             public MessageAction MessageAction { get; set; }     
    }

    public class Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("FillInput")] public string FillInput { get; set; }
    }

    public class ContextMenusId
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("MainId")]                        public string MainId { get; set; }                       
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("PasteToCp")]                     public string PasteToCp { get; set; }                    
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("FillToCp")]                      public string FillToCp { get; set; }                     
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("PrintDsGroupVariantSetup")]      public string PrintDsGroupVariantSetup { get; set; }     
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("GenerateUnitTestsVariantSetup")] public string GenerateUnitTestsVariantSetup { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("JsonToClass")]                   public string JsonToClass { get; set; }                  
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("JsonToObjectInitializer")]       public string JsonToObjectInitializer { get; set; }      
    }

    public class MessageAction
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("PasteToCp")]                     public string PasteToCp { get; set; }                    
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("FillToCp")]                      public string FillToCp { get; set; }                     
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("PrintDsGroupVariantSetup")]      public string PrintDsGroupVariantSetup { get; set; }     
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("GenerateUnitTestsVariantSetup")] public string GenerateUnitTestsVariantSetup { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("JsonToClass")]                   public string JsonToClass { get; set; }                  
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("JsonToObjectInitializer")]       public string JsonToObjectInitializer { get; set; }      
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("GetSelectedText")]               public string GetSelectedText { get; set; }              
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("CreateKYBButton")]               public string CreateKybButton { get; set; }              
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("CreateKYCButton")]               public string CreateKycButton { get; set; }              
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("RefreshCustomFields")]           public string RefreshCustomFields { get; set; }          
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("CurrentCulture")]                public string CurrentCulture { get; set; }               
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("UpdateRule")]                    public string UpdateRule { get; set; }                   
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("RemoveRule")]                    public string RemoveRule { get; set; }                   
    }

    public class Tables
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("GlobalConfiguration")] public string GlobalConfiguration { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("CSPManager")]          public string CspManager { get; set; }         
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("CustomFieldGroup")]    public string CustomFieldGroup { get; set; }   
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)][J("Temp")]                public string Temp { get; set; }               
    }
}