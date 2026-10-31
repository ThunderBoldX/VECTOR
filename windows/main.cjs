'use strict';
const {app,BrowserWindow,ipcMain,Tray,Menu,nativeImage,shell,dialog,safeStorage}=require('electron'),path=require('path'),fs=require('fs'),{Receiver}=require('./protocol.cjs'),{NativeInput}=require('./native.cjs'),Profiles=require('./profiles.cjs'),{TrustStore}=require('./trust.cjs');
let win,receiver,input,tray,quit=false,store,storeRevision=0,profileLoadError=null,profileFile,settings={startup:false,tray:false};
const smoke=process.env.VECTOR_SMOKE==='1';
if(smoke)app.setPath('userData',path.join(app.getPath('temp'),'vector-smoke-v2'));
if(!app.requestSingleInstanceLock())app.quit();else{
 app.on('second-instance',()=>{win?.show();win?.focus()});
 app.whenReady().then(async()=>{
  const file=path.join(app.getPath('userData'),'settings.json');profileFile=path.join(app.getPath('userData'),'profiles-v2.json');fs.mkdirSync(app.getPath('userData'),{recursive:true});try{store=Profiles.load(profileFile)}catch(e){profileLoadError='Не вдалося прочитати профілі: '+e.message+'. Файл збережено без змін.';store=Profiles.defaults();}
  try{const s=JSON.parse(fs.readFileSync(file,'utf8'));settings={startup:s.startup===true,tray:s.tray===true};}catch{}
  input=new NativeInput();input.on('error',e=>{if(receiver){receiver.enable(false);receiver.lastError=e;push()}});input.on('status',()=>{if(receiver?.enabled&&((receiver.mode==='xbox'&&!input.xbox)||(receiver.mode==='joystick'&&!input.vjoy)))receiver.enable(false);push()});
  receiver=new Receiver({port:smoke?0:26760,trust:new TrustStore(path.join(app.getPath('userData'),'trusted-devices.json'),safeStorage),native:input,canInput:()=>!smoke&&!!win&&!input.dead&&(!['keyboard','drone'].includes(receiver.mode)||!win.isFocused())});receiver.setProfile(store.profiles.find(p=>p.id===store.active));receiver.on('state',push);
  win=new BrowserWindow({title:'VECTOR Host',show:!smoke,width:1440,height:950,minWidth:1024,minHeight:700,backgroundColor:'#0b0810',autoHideMenuBar:true,webPreferences:{preload:path.join(__dirname,'preload.cjs'),nodeIntegration:false,contextIsolation:true,sandbox:true,backgroundThrottling:!smoke}});
  win.webContents.setWindowOpenHandler(()=>({action:'deny'}));win.webContents.on('will-navigate',e=>e.preventDefault());win.loadFile(path.join(__dirname,'ui/index.html'));
  win.on('focus',()=>{if(['keyboard','drone'].includes(receiver.mode))receiver.release()});win.on('close',e=>{receiver.enable(false);if(settings.tray&&!quit){e.preventDefault();win.hide()}});
  tray=new Tray(nativeImage.createFromPath(path.join(__dirname,'ui/assets/vector-mark.png')));tray.setToolTip('VECTOR Host');tray.setContextMenu(Menu.buildFromTemplate([{label:'Відкрити VECTOR Host',click:()=>win.show()},{label:'Зупинити керування',click:()=>receiver.enable(false)},{type:'separator'},{label:'Вийти',click:()=>{quit=true;app.quit()}}]));tray.on('double-click',()=>win.show());
  if(profileLoadError)receiver.lastError=profileLoadError;
  function saveProfiles(value){if(profileLoadError)throw Error(profileLoadError);if(JSON.stringify(value).length>131072)throw Error('Профілі завеликі');const next=Profiles.save(profileFile,value);store=next;storeRevision++;receiver.lastError=null;receiver.setProfile(store.profiles.find(p=>p.id===store.active),{preserveEnabled:true});push();return {ok:true,store,storeRevision};}
  ipcMain.handle('vector-save-profiles',(event,value)=>{if(event.sender!==win.webContents)return {ok:false,error:'Недозволений запит'};try{return saveProfiles(value)}catch(e){receiver.lastError=e.message;push();return {ok:false,error:e.message};}});
  ipcMain.on('vector-ready',push);ipcMain.on('vector-command',async(event,kind,value)=>{if(event.sender!==win.webContents)return;try{switch(kind){
   case'forget-device':if(typeof value==='string')receiver.forgetDevice(value);break;case'new-pin':receiver.renewPairing();break;case'approve':receiver.approve();break;case'reject':receiver.reject();break;case'enable':receiver.enable(value===true);break;case'disconnect':receiver.disconnect();break;
   case'listen':if(value===true)await receiver.start();else await receiver.stop();break;
   case'edit-begin':receiver.beginEdit();break;case'edit-end':receiver.endEdit();break;case'profiles':saveProfiles(value);break;
   case'export':{const p=store.profiles.find(p=>p.id===store.active);const out=await dialog.showSaveDialog(win,{defaultPath:p.name+'.vector.json',filters:[{name:'VECTOR profile',extensions:['json']}]});if(!out.canceled)fs.writeFileSync(out.filePath,JSON.stringify(p,null,2));break;}
   case'import':{receiver.enable(false);const chosen=await dialog.showOpenDialog(win,{properties:['openFile'],filters:[{name:'VECTOR profile',extensions:['json']}]});if(chosen.canceled)break;const file=chosen.filePaths[0];if(fs.statSync(file).size>65536)throw Error('Профіль завеликий');const p=JSON.parse(fs.readFileSync(file,'utf8'));p.id='p'+Date.now();saveProfiles({profiles:[...store.profiles,p],active:p.id});break;}
   case'driver':{receiver.enable(false);const driver=path.join(__dirname,'vendor/ViGEmBus_1.22.0.exe');const result=await shell.openPath(driver);if(result)throw Error(result);break;}
   case'restart':receiver.enable(false);if(!smoke){app.relaunch();app.quit()}break;
   case'settings':settings={startup:value?.startup===true,tray:value?.tray===true};fs.writeFileSync(file,JSON.stringify(settings));if(!smoke)app.setLoginItemSettings({openAtLogin:settings.startup});push();break;
  }}catch(e){receiver.lastError=e.message;push()}});
  try{await receiver.start()}catch(e){receiver.lastError='Не вдалося відкрити UDP 26760: '+e.message;push()}
  setInterval(push,100).unref();
 });
 function push(){if(win&&!win.isDestroyed()&&receiver)win.webContents.send('vector-state',{...receiver.snapshot(),...settings,store,storeRevision})}
 app.on('before-quit',()=>{quit=true;receiver?.release();input?.close();receiver?.close();tray?.destroy()});app.on('window-all-closed',()=>{if(!settings.tray)app.quit()});
}
