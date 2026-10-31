const {contextBridge,ipcRenderer}=require('electron');
contextBridge.exposeInMainWorld('Host',{saveProfiles:value=>ipcRenderer.invoke('vector-save-profiles',value),command:(kind,value)=>ipcRenderer.send('vector-command',kind,value),onState:fn=>ipcRenderer.on('vector-state',(_,state)=>fn(state)),ready:()=>ipcRenderer.send('vector-ready')});
