'use strict';
const fs=require('fs'),path=require('path'),C=require(fs.existsSync(path.join(__dirname,'ui/core.js'))?'./ui/core.js':'../ui/core.js');
function cleanStore(value){if(!value||!Array.isArray(value.profiles)||!value.profiles.length||value.profiles.length>40)throw Error('Потрібно 1–40 профілів');const profiles=value.profiles.map(C.sanitizeProfile);if(new Set(profiles.map(p=>p.id)).size!==profiles.length)throw Error('Повторюються ідентифікатори профілів');return {presetVersion:5,profiles,active:profiles.some(p=>p.id===value.active)?value.active:profiles[0].id};}
function defaults(){return {presetVersion:5,active:'racing',profiles:[C.defaults(),C.dronePreset(),{...C.defaults(),id:'universal',name:'Універсальний',type:'universal',buttons:C.xboxButtons.slice(0,6).map((b,i)=>({id:'b'+i,label:b.name,joy:b.value,key:32,behavior:'hold',x:i<3?.04:.84,y:.16+(i%3)*.25,w:.12,h:.2,side:i<3?'left':'right'}))}]};}
function migrate(raw){
 // Version 0.5 could create a second legacy backup with the same ID. Keep both copies.
 if(Array.isArray(raw?.profiles)){const ids=new Set(raw.profiles.map(p=>p.id));const seen=new Set();raw={...raw,profiles:raw.profiles.map(p=>{if(seen.has(p.id)&&/^drone-v0[34]-backup(?:-recovered-\d+)?$/.test(p.id)){let n=1,id;do{id=p.id+'-recovered-'+n++}while(ids.has(id));ids.add(id);p={...p,id};}seen.add(p.id);return p;})};}
 const s=cleanStore(raw);if(raw.presetVersion>=5)return s;const i=s.profiles.findIndex(p=>p.id==='drone');if(i<0){if(s.profiles.length<40)s.profiles.push(C.dronePreset());return s;}
 const old=s.profiles[i],preset=C.dronePreset(),backupId=raw.presetVersion<4?'drone-v03-backup':'drone-v04-backup';if(s.profiles.length<40&&!s.profiles.some(p=>p.id===backupId))s.profiles.push(C.sanitizeProfile({...old,id:backupId,name:'Дрон — налаштування до 0.5'}));
 const buttons=[...old.buttons];for(const b of preset.buttons.filter(b=>b.joy===13||b.joy===14))if(!buttons.some(existing=>existing.joy===b.joy)&&buttons.length<12)buttons.push(b);
 s.profiles[i]=C.sanitizeProfile({...old,heightOutput:'xbox',mouseMode:'velocity',mouseGain:1280,mouseSpeed:64000,mouseDead:1,mouseInvertX:false,mouseInvertY:true,mouseSenseX:old.mouseSenseX,mouseSenseY:old.mouseSenseY,buttons});return s;
}
function load(file){let text;try{text=fs.readFileSync(file,'utf8')}catch(e){if(e.code==='ENOENT')return defaults();throw e;}const raw=JSON.parse(text),store=migrate(raw);if(JSON.stringify(raw)!==JSON.stringify(store)){const backup=file+'.before-0.6.bak';if(!fs.existsSync(backup))fs.copyFileSync(file,backup);save(file,store);}return store;}
function save(file,value){const store=cleanStore(value),tmp=file+'.tmp';fs.writeFileSync(tmp,JSON.stringify(store,null,2));fs.renameSync(tmp,file);return store;}
module.exports={cleanStore,defaults,load,save,migrate};
