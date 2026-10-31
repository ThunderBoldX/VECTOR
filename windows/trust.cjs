'use strict';
const fs=require('fs'),crypto=require('crypto');
const validId=id=>typeof id==='string'&&/^[a-f0-9]{32}$/.test(id);
class TrustStore {
 constructor(file,secure){this.file=file;this.secure=secure;this.data={version:1,hostId:crypto.randomBytes(16).toString('hex'),devices:[]};try{const d=JSON.parse(fs.readFileSync(file,'utf8'));if(validId(d.hostId)&&Array.isArray(d.devices)){this.data={version:1,hostId:d.hostId,devices:d.devices.filter(v=>validId(v.id)&&typeof v.encrypted==='string').slice(0,32)};return}}catch{}this.write();}
 get hostId(){return this.data.hostId}
 write(){const tmp=this.file+'.tmp';fs.writeFileSync(tmp,JSON.stringify(this.data,null,2),{mode:0o600});fs.renameSync(tmp,this.file)}
 get(id){const row=this.data.devices.find(v=>v.id===id);if(!row)return null;try{const hex=this.secure.decryptString(Buffer.from(row.encrypted,'base64'));if(!/^[a-f0-9]{64}$/.test(hex))return null;return {...row,token:Buffer.from(hex,'hex')}}catch{return null}}
 enroll(id,name){if(!validId(id))throw Error('Неправильний ID телефона');if(!this.secure.isEncryptionAvailable())throw Error('Захищене сховище Windows недоступне');if(!this.data.devices.some(v=>v.id===id)&&this.data.devices.length>=32)throw Error('Збережено 32 телефони. Видали зайвий пристрій.');const token=crypto.randomBytes(32),row={id,name:String(name||'Android').slice(0,64),encrypted:this.secure.encryptString(token.toString('hex')).toString('base64'),seen:Date.now()};const before=this.data.devices;this.data.devices=[...before.filter(v=>v.id!==id),row];try{this.write()}catch(e){this.data.devices=before;throw e;}return token;}
 list(){return this.data.devices.map(v=>({id:v.id,name:v.name,seen:v.seen}))}
 forget(id){this.data.devices=this.data.devices.filter(v=>v.id!==id);this.write()}
}
module.exports={TrustStore,validId};
