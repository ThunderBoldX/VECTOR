using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

// Isolated Win32 input process. Releases held input after 250 ms without a STATE.
internal static class VectorInput {
 [StructLayout(LayoutKind.Sequential)] struct INPUT {public uint type;public UNION data;}
 [StructLayout(LayoutKind.Explicit)] struct UNION {[FieldOffset(0)]public KEYBDINPUT key;[FieldOffset(0)]public MOUSEINPUT mouse;[FieldOffset(0)]public HARDWAREINPUT hardware;}
 [StructLayout(LayoutKind.Sequential)] struct KEYBDINPUT {public ushort vk,scan;public uint flags,time;public UIntPtr extra;}
 [StructLayout(LayoutKind.Sequential)] struct MOUSEINPUT {public int x,y;public uint data,flags,time;public UIntPtr extra;}
 [StructLayout(LayoutKind.Sequential)] struct HARDWAREINPUT {public uint message;public ushort low,high;}
 [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint n,INPUT[] input,int size);
 [DllImport("user32.dll")] static extern uint MapVirtualKey(uint code,uint type);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr LoadLibraryEx(string path,IntPtr reserved,uint flags);
 [DllImport("kernel32.dll")] static extern IntPtr GetProcAddress(IntPtr module,string name);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool Enabled();
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool Acquire(uint id);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate void Relinquish(uint id);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool Axis(int value,uint id,uint usage);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool Button(bool pressed,uint id,byte button);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool Limit(uint id,uint usage,ref int value);
 static readonly object gate=new object();static HashSet<int> down=new HashSet<int>();static HashSet<int> joyDown=new HashSet<int>();static readonly Stopwatch time=Stopwatch.StartNew();static long last=-1000;static bool vjoy=false,usingJoy=false;static Axis setAxis;static Button setButton;static Relinquish relinquish;static readonly int[] min=new int[3],max=new int[3];
 static T Function<T>(IntPtr module,string name)where T:class{IntPtr p=GetProcAddress(module,name);if(p==IntPtr.Zero)throw new Exception("vJoy function missing: "+name);return Marshal.GetDelegateForFunctionPointer(p,typeof(T))as T;}
 static void LoadJoy(){try{string[] folders={Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)};foreach(string folder in folders){string file=Path.Combine(folder,"vJoy","x64","vJoyInterface.dll");if(!File.Exists(file))continue;IntPtr module=LoadLibraryEx(file,IntPtr.Zero,0x00000100|0x00001000);if(module==IntPtr.Zero)continue;Enabled enabled=Function<Enabled>(module,"vJoyEnabled");Acquire acquire=Function<Acquire>(module,"AcquireVJD");if(!enabled()||!acquire(1))continue;setAxis=Function<Axis>(module,"SetAxis");setButton=Function<Button>(module,"SetBtn");relinquish=Function<Relinquish>(module,"RelinquishVJD");Limit low=Function<Limit>(module,"GetVJDAxisMin"),high=Function<Limit>(module,"GetVJDAxisMax");bool axes=true;for(int i=0;i<3;i++)axes &= low(1,(uint)(0x30+i),ref min[i])&&high(1,(uint)(0x30+i),ref max[i])&&max[i]>min[i];vjoy=axes;if(!axes)relinquish(1);break;}}catch(Exception e){Console.WriteLine("ERROR|vJoy: "+e.Message);}Console.WriteLine("VJOY|"+(vjoy?"1":"0"));}
 static bool Extended(int key){return (key>=33&&key<=40)||key==45||key==46||key==163||key==165||key==111||key==91||key==92||key==93;}
 static bool Key(int vk,bool pressed){uint scan=MapVirtualKey((uint)vk,0);INPUT i=new INPUT();i.type=1;i.data.key.scan=(ushort)scan;i.data.key.flags=8u|(pressed?0u:2u)|(Extended(vk)?1u:0u);if(scan==0){i.data.key.vk=(ushort)vk;i.data.key.flags=pressed?0u:2u;}bool ok=SendInput(1,new INPUT[]{i},Marshal.SizeOf(typeof(INPUT)))==1;if(!ok)Console.WriteLine("ERROR|Windows не прийняла клавішу. Перевір рівень прав Host і гри.");return ok;}
 static void Keys(HashSet<int> next){foreach(int key in new List<int>(down))if(!next.Contains(key)&&Key(key,false))down.Remove(key);foreach(int key in next)if(!down.Contains(key)&&Key(key,true))down.Add(key);}
 static int AxisValue(double v,int i){return (int)Math.Round(min[i]+(max[i]-min[i])*v);}
 static void Joy(double steer,double gas,double brake,HashSet<int> next){if(!vjoy)return;bool ok=setAxis(AxisValue((steer+1)/2,0),1,0x30)&setAxis(AxisValue(gas,1),1,0x31)&setAxis(AxisValue(brake,2),1,0x32);foreach(int button in new List<int>(joyDown))if(!next.Contains(button)){ok &= setButton(false,1,(byte)button);joyDown.Remove(button);}foreach(int button in next)if(!joyDown.Contains(button)){ok &= setButton(true,1,(byte)button);joyDown.Add(button);}usingJoy=true;if(!ok)Console.WriteLine("ERROR|vJoy не прийняв звіт. Перевір осі X/Y/Z і кількість кнопок Device 1.");}
 static XboxInput xbox;static bool usingXbox;
 static INPUT MouseReport(int dx,int dy){INPUT i=new INPUT();i.type=0;i.data.mouse.x=dx;i.data.mouse.y=dy;i.data.mouse.flags=0x0001|0x2000;return i;}
 static void Mouse(int dx,int dy){if(dx==0&&dy==0)return;if(SendInput(1,new INPUT[]{MouseReport(dx,dy)},Marshal.SizeOf(typeof(INPUT)))!=1)Console.WriteLine("ERROR|Windows не прийняла рух миші. Перевір рівень прав Host і гри.");}
 static void Release(){Keys(new HashSet<int>());if(vjoy&&usingJoy){Joy(0,0,0,new HashSet<int>());usingJoy=false;}if(usingXbox){xbox.Neutral();usingXbox=false;}}
 static HashSet<int> Numbers(string value,int lo,int hi){HashSet<int> set=new HashSet<int>();if(value.Length==0)return set;string[] parts=value.Split(',');if(parts.Length>32)throw new Exception("Too many buttons");foreach(string part in parts){int k=int.Parse(part,CultureInfo.InvariantCulture);if(k<lo||k>hi)throw new Exception("Invalid button");set.Add(k);}return set;}
 static double Number(string text,double lo,double hi){double d=double.Parse(text,CultureInfo.InvariantCulture);if(double.IsNaN(d)||double.IsInfinity(d)||d<lo||d>hi)throw new Exception("Invalid axis");return d;}
 static int Main(string[] args){if(args.Length>0&&args[0]=="--self-test"){
  if(Marshal.SizeOf(typeof(INPUT))!=40||Marshal.SizeOf(typeof(XboxInput.Report))!=12)throw new Exception("Wrong ABI size");
  if(!Extended(37)||Extended(65)||Numbers("65,65,32",8,254).Count!=2)throw new Exception("Keyboard policy");
  bool rejected=false;try{Number("NaN",-1,1);}catch{rejected=true;}if(!rejected)throw new Exception("Non-finite value accepted");
  XboxInput.Report r=XboxInput.Make(-1,.5,0,0,.25,1,new HashSet<int>{1,5,14});if(r.LX!=-32768||r.RT!=255||r.LT!=64||r.Buttons!=0x1108)throw new Exception("Xbox mapping");
  r=XboxInput.Make(0,0,0,0,0,0,new HashSet<int>{15,16});if(r.LT!=255||r.RT!=255)throw new Exception("Trigger buttons");
  if(Number("384000",-384000,384000)!=384000)throw new Exception("High-sensitivity mouse range");
  INPUT mouse=MouseReport(-384000,384000);if(mouse.type!=0||mouse.data.mouse.x!=-384000||mouse.data.mouse.flags!=0x2001)throw new Exception("Relative mouse ABI");
  XboxInput.CheckLibrary();Console.WriteLine("PASS native ABI, SDK DLL loading, Xbox sticks, RT/LT, relative mouse, keyboard validation");return 0;}
 xbox=new XboxInput();if(Environment.GetEnvironmentVariable("VECTOR_SMOKE")!="1")LoadJoy();else Console.WriteLine("VJOY|0");
 using(Timer watchdog=new Timer(_=>{lock(gate){if(time.ElapsedMilliseconds-last>250)Release();}},null,50,50)){try{string line;while((line=Console.ReadLine())!=null){lock(gate){if(line=="QUIT")break;if(line=="RELEASE"){Release();continue;}try{
  string[] a=line.Split('|');if((a.Length!=7&&a.Length!=11&&a.Length!=13)||a[0]!="STATE")continue;double steer=Number(a[2],-1,1),gas=Number(a[3],0,1),brake=Number(a[4],0,1);HashSet<int> keys=Numbers(a[5],8,254),buttons=Numbers(a[6],1,32);
  double lx=a.Length>=11?Number(a[7],-1,1):steer,ly=a.Length>=11?Number(a[8],-1,1):0,rx=a.Length>=11?Number(a[9],-1,1):0,ry=a.Length>=11?Number(a[10],-1,1):0;
  int dx=a.Length==13?(int)Number(a[11],-384000,384000):0,dy=a.Length==13?(int)Number(a[12],-384000,384000):0;
  last=time.ElapsedMilliseconds;if(a[1]=="xbox"){if(usingJoy||down.Count>0)Release();xbox.Send(XboxInput.Make(lx,ly,rx,ry,brake,gas,buttons));usingXbox=true;}
  else if(a[1]=="joystick"){if(usingXbox)Release();Keys(new HashSet<int>());Joy(steer,gas,brake,buttons);}
  else if(a[1]=="drone"){if(usingJoy)Release();Keys(keys);xbox.Send(XboxInput.Make(0,0,0,0,brake,gas,buttons));usingXbox=true;Mouse(dx,dy);}
  else if(a[1]=="keyboard"){if(usingJoy||usingXbox)Release();Keys(keys);}else Release();
 }catch(Exception e){Release();Console.WriteLine("ERROR|"+e.Message);}}}}finally{lock(gate){Release();if(vjoy)relinquish(1);xbox.Dispose();}}}return 0;
 }
}
