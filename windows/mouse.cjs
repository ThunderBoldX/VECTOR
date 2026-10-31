'use strict';
// Relative mouse counts: fixed displacement by default, optional tilt-as-speed.
class MouseMotion {
 constructor(){this.reset()}
 reset(){this.x=this.y=this.fx=this.fy=0;this.at=null;}
 sample(p,c,now){const zone=c.mouseDead||0,shape=a=>Math.abs(a)<=zone?0:Math.sign(a)*(Math.abs(a)-zone);let x=shape(p.angle||0)*(c.mouseInvertX?-1:1),y=-shape(p.pitch||0)*(c.mouseInvertY?-1:1),dx,dy;
  if(c.mouseMode==='velocity'){const dt=this.at==null?0:Math.min(.05,Math.max(0,(now-this.at)/1000));dx=Math.max(-1,Math.min(1,x/25))*c.mouseSpeed*dt;dy=Math.max(-1,Math.min(1,y/25))*c.mouseSpeed*dt;}
  else{x=Math.max(-75,Math.min(75,x))*c.mouseGain;y=Math.max(-75,Math.min(75,y))*c.mouseGain;dx=x-this.x;dy=y-this.y;this.x=x;this.y=y;}
  this.at=now;this.fx+=dx*(c.mouseSenseX??100)/100;this.fy+=dy*(c.mouseSenseY??100)/100;const out={dx:Math.max(-384000,Math.min(384000,Math.trunc(this.fx))),dy:Math.max(-384000,Math.min(384000,Math.trunc(this.fy)))};this.fx-=out.dx;this.fy-=out.dy;return out;
 }
}
module.exports={MouseMotion};
