'use strict';
const {spawn}=require('child_process'),path=require('path'),{EventEmitter}=require('events');
class NativeInput extends EventEmitter{
 constructor(executable=path.join(__dirname,'VectorInput.exe')){super();this.vjoy=false;this.xbox=false;this.dead=false;this.last='';this.process=spawn(executable,[],{windowsHide:true,stdio:['pipe','pipe','pipe']});this.process.on('error',e=>{this.dead=true;this.emit('error',e.message)});this.process.on('exit',()=>{this.dead=true;this.vjoy=this.xbox=false;this.emit('error','Компонент введення завершив роботу. Перезапусти Host.')});let buf='';this.process.stdout.on('data',d=>{buf+=d;let ix;while((ix=buf.indexOf('\n'))>=0){const line=buf.slice(0,ix).trim();buf=buf.slice(ix+1);if(line.startsWith('VJOY|')||line.startsWith('XBOX|')){this[line.startsWith('VJOY')?'vjoy':'xbox']=line.endsWith('|1');this.emit('status')}else if(line.startsWith('ERROR|'))this.emit('error',line.slice(6));}});this.process.stderr.on('data',d=>this.emit('error',d.toString().trim()));this.process.stdin.on('error',()=>{this.dead=true})}
 state(s){if(this.dead)return;const x=s.xbox||{lx:s.steer,ly:0,rx:0,ry:0,lt:s.brake,rt:s.throttle};const line=['STATE',s.mode,s.steer.toFixed(5),(['xbox','drone'].includes(s.mode)?x.rt:s.throttle).toFixed(5),(['xbox','drone'].includes(s.mode)?x.lt:s.brake).toFixed(5),s.keys.join(','),s.joy.join(','),...[x.lx,x.ly,x.rx,x.ry].map(n=>n.toFixed(5)),s.mouse?.dx||0,s.mouse?.dy||0].join('|');this.process.stdin.write(line+'\n');this.last=line}
 release(){if(this.dead||this.last==='RELEASE')return;this.process.stdin.write('RELEASE\n');this.last='RELEASE'}
 close(){this.release();if(!this.dead)this.process.stdin.end('QUIT\n')}
}
module.exports={NativeInput};
