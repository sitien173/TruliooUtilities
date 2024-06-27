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

export const loader = {
    __loader: null,
    show: function () {

        if (this.__loader == null) {
            const divContainer = document.createElement('div');
            divContainer.style.position = 'fixed';
            divContainer.style.left = '0';
            divContainer.style.top = '0';
            divContainer.style.width = '100%';
            divContainer.style.height = '100%';
            divContainer.style.zIndex = '9998';
            divContainer.style.backgroundColor = 'rgba(250, 250, 250, 0.80)';

            const div = document.createElement('div');
            div.style.position = 'absolute';
            div.style.left = '50%';
            div.style.top = '50%';
            div.style.zIndex = '9999';
            div.style.height = '64px';
            div.style.width = '64px';
            div.style.margin = '-76px 0 0 -76px';
            div.style.border = '8px solid #e1e1e1';
            div.style.borderRadius = '50%';
            div.style.borderTop = '8px solid #F36E21';
            div.animate([
                { transform: 'rotate(0deg)' },
                { transform: 'rotate(360deg)' }
            ], {
                duration: 2000,
                iterations: Infinity
            });
            divContainer.appendChild(div);
            this.__loader = divContainer
            document.body.appendChild(this.__loader);
        }
        this.__loader.style.display="";
    },
    hide: function(){
        if(this.__loader!=null)
        {
            this.__loader.style.display="none";
        }
    }
}