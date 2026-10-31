import app.vector.controller.MotionEngine;
public final class MotionEngineTest {
 static void check(boolean b,String m){if(!b)throw new AssertionError(m);}
 static double[] rotation(int axis,double angle){double a=Math.toRadians(angle)/2;double[] q={Math.cos(a),0,0,0};q[axis]=Math.sin(a);return q;}
 public static void main(String[] args){MotionEngine e=new MotionEngine();e.smoothing=0;e.curve="linear";e.center(new double[]{1,0,0,0},1);
 double[] s=e.sample(new double[]{1,0,0,0},.01);check(s[0]==0&&s[1]==0&&s[2]==0,"neutral");
 s=e.sample(rotation(3,-36),.01);check(Math.abs(s[0]-1)<.001,"full steering");check(s[1]==0&&s[2]==0,"steering must not press pedals");
 s=e.sample(rotation(2,-25),.01);check(s[1]>.999&&s[2]==0,"forward throttle");
 s=e.sample(rotation(2,25),.01);check(s[2]>.999&&s[1]==0,"backward brake");
 double[] mixed=MotionEngine.multiply(rotation(2,-25),rotation(3,-36));s=e.sample(mixed,.01);check(s[0]>.999&&s[1]>.999,"steering/pedals independence");
 e.invertSteer=true;s=e.sample(rotation(3,-36),.01);check(s[0]<-.999,"invert");
 check(MotionEngine.axis(2,3,36,"linear")==0,"dead zone");check(MotionEngine.axis(500,3,36,"linear")==1,"saturation");
 double[] arbitrary=MotionEngine.multiply(rotation(1,35),rotation(2,-25));e.center(arbitrary,1);s=e.sample(arbitrary,.01);check(Math.abs(s[0])<1e-8&&s[1]==0&&s[2]==0,"arbitrary neutral");
 e.center(new double[]{1,0,0,0},1);s=e.sample(new double[]{-1,0,0,0},.01);check(s[0]==0,"quaternion sign invariance");
 e.dead=0;e.invertSteer=false;e.center(new double[]{1,0,0,0},1);for(double a:new double[]{-1,1}){s=e.sample(rotation(3,-a),.01);check(Math.signum(s[0])==Math.signum(a)&&Math.abs(s[0])>0,"one degree steering");}
 System.out.println("PASS 12 quaternion, calibration, steering/pedals, dead-zone checks");}
}
