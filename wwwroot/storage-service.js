import * as _ from './node_modules/localforage/dist/localforage.min.js';
const dbName = 'TruliooExtApp';
export const setItem = async (instanceName, key, value) => {
    const instance = getInstances(instanceName);
    await instance.setItem(key, value);
};

export const getItem = async (instanceName, key) => {
    const instance = getInstances(instanceName);
    return await instance.getItem(key);
}

export const getAll = async (instanceName) => {
    const instance = getInstances(instanceName);
    const items = [];
    await instance.iterate((value, key, _) => {
        items.push(value);
    });
    
    return items;
}

export const getKeys = async (instanceName) => {
    const instance = getInstances(instanceName);
    return await instance.keys();
}

export const count = async (instanceName) => {
    const instance = getInstances(instanceName);
    return await instance.length();
}

export const deleteItem = async (instanceName, key) => {
    const instance = getInstances(instanceName);
    await instance.removeItem(key);
}


export const nextId = async (instanceName) => {
    const instance = getInstances(instanceName);
    const id = await instance.length();
    return id + 1;
}

export const getInstances =  (instanceName) => {
    return localforage.createInstance({
        name: dbName,
        storeName: instanceName,
    });
}

export const find = async (instanceName, predicate) => {
    const instance = getInstances(instanceName);
    await instance.iterate((value, key, _) => {
        if(predicate(value)){
            return value;
        }
    });

    return null;
}

export const filter = async (instanceName, predicate) => {
    const instance = getInstances(instanceName);
    const items = [];
    await instance.iterate((value, key, _) => {
        if(predicate(value)){
            items.push(value);
        }
    });

    return items;
}

export const clear = async (instanceName) => {
    const instance = getInstances(instanceName);
    await instance.clear();
}