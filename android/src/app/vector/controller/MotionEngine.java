package app.vector.controller;

/** Quaternion-based relative motion. No Android dependency: executable JVM tests. */
public final class MotionEngine {
 private double[] origin={1,0,0,0}; private boolean centered=false; private int pitchAxis=1; private int pitchSign=1;
 public double sensitivity=72,dead=0,maxThrottle=25,maxBrake=25,pedalDead=3,smoothing=.18;
 public boolean invertSteer=false,invertPitch=false; public String curve="balanced";
 private double filteredSteer,filteredPitch;
 public static double clamp(double v,double a,double b){return Math.max(a,Math.min(b,v));}
 public static double[] multiply(double[] a,double[] b){return new double[]{a[0]*b[0]-a[1]*b[1]-a[2]*b[2]-a[3]*b[3],a[0]*b[1]+a[1]*b[0]+a[2]*b[3]-a[3]*b[2],a[0]*b[2]-a[1]*b[3]+a[2]*b[0]+a[3]*b[1],a[0]*b[3]+a[1]*b[2]-a[2]*b[1]+a[3]*b[0]};}
 public static double[] inverse(double[] q){return new double[]{q[0],-q[1],-q[2],-q[3]};}
 public static double[] normalize(double[] q){double n=0;for(double v:q)n+=v*v;n=Math.sqrt(n);if(n<1e-9)return new double[]{1,0,0,0};return new double[]{q[0]/n,q[1]/n,q[2]/n,q[3]/n};}
 public static double distance(double[] a,double[] b){double dot=0;for(int i=0;i<4;i++)dot+=a[i]*b[i];return Math.toDegrees(2*Math.acos(clamp(Math.abs(dot),0,1)));}
 public synchronized void center(double[] q,int rotation){origin=normalize(q);centered=true;pitchAxis=(rotation==1||rotation==3)?2:1;pitchSign=rotation==1?-1:1;filteredSteer=0;filteredPitch=0;}
 public synchronized boolean centered(){return centered;}
 public static double axis(double degrees,double zone,double maximum,String curve){double x=clamp((Math.abs(degrees)-zone)/Math.max(.1,maximum-zone),0,1);if("balanced".equals(curve))x=Math.pow(x,1.5);else if("dynamic".equals(curve))x=Math.pow(x,2.2);return Math.copySign(x,degrees);}
 public synchronized double[] sample(double[] input,double dt){
  if(!centered)return new double[]{0,0,0,0,0,0,0};
  double[] q=multiply(inverse(origin),normalize(input));if(q[0]<0)for(int i=0;i<4;i++)q[i]=-q[i];
  double twist=Math.sqrt(q[0]*q[0]+q[3]*q[3]);double[] z=twist<1e-8?new double[]{1,0,0,0}:new double[]{q[0]/twist,0,0,q[3]/twist};
  double angle=-Math.toDegrees(2*Math.atan2(z[3],z[0]));double[] swing=multiply(q,inverse(z));
  double pitch=pitchSign*Math.toDegrees(2*Math.atan2(swing[pitchAxis],swing[0]));
  if(invertSteer)angle=-angle;if(invertPitch)pitch=-pitch;
  // Time-based smoothing: independent of actual sensor frequency.
  double tau=clamp(smoothing,0,.8)*.08;double alpha=tau==0?1:1-Math.exp(-clamp(dt,.001,.1)/tau);
  filteredSteer+=alpha*(angle-filteredSteer);filteredPitch+=alpha*(pitch-filteredPitch);
  double maximum=15+(100-clamp(sensitivity,1,100))*.75;
  double steer=axis(filteredSteer,dead,Math.max(dead+1,maximum),curve);
  double gas=axis(Math.max(0,filteredPitch),pedalDead,maxThrottle,"linear"),brake=axis(Math.max(0,-filteredPitch),pedalDead,maxBrake,"linear");
  return new double[]{steer,gas,brake,filteredSteer,filteredPitch,angle,pitch};
 }
}
