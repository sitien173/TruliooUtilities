export const constantStrings = {
    DbName: 'TruliooExtApp',
    AssemblyName: 'TruliooExtension',
    DefaultCulture: 'en',
    CustomFieldMatchTemplate: '[id$=\"{0}\" i], [name$=\"{0}\" i], [class$=\"{0}\" i]',
    AdminPortalEndpoint: 'https://localhost:44331/',
    CustomFieldGroupGlobalKey: 'global',
    LastFetchDatasourceID: 'LastFetchDatasourceID',
    LastUpdatedDatasourceID: 'LastUpdatedDatasourceID',
    UpdateDatasourcePath: 'api-datasources/update',
    GetDatasourcePath: 'api-datasources/get/',
    Tables: {
        CSPManager: 'CSP',
        CustomFieldGroup: 'CustomFieldGroup',
        Config: 'Config'
    },
    Command: {
        FillInput: 'fill-input'
    },
    ContextMenusID: {
        MainId: 'TruExtMenu',
        PasteToCp: 'PasteToCp',
        FillToCp: 'FillToCp',
        PrintDsGroupVariantSetup: 'PrintDsGroupVariantSetup',
        GenerateUnitTestsVariantSetup: 'GenerateUnitTestsVariantSetup',
        JsonToClass: 'JsonToClass',
        JsonToObjectInitializer: 'JsonToObjectInitializer'
    },
    MessageAction: {
        PasteToCp: 'PasteToCp',
        FillToCp: 'FillToCp',
        PrintDsGroupVariantSetup: 'PrintDsGroupVariantSetup',
        GenerateUnitTestsVariantSetup: 'GenerateUnitTestsVariantSetup',
        JsonToClass: 'JsonToClass',
        JsonToObjectInitializer: 'JsonToObjectInitializer',
        GetSelectedText: 'GetSelectedText',
        CreateKYBButton: 'CreateKYBButton',
        CreateKYCButton: 'CreateKYCButton',
        RefreshCustomFields: 'RefreshCustomFields',
        CurrentCulture: 'CurrentCulture',
        UpdateRule: 'UpdateRule',
        RemoveRule: 'RemoveRule',
    }
}

export const appSettings = function () {
    return constantStrings;
}