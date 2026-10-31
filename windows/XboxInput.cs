using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

// ViGEmClient C ABI: XUSB_REPORT is the 12-byte XInput gamepad report.
internal sealed class XboxInput : IDisposable {
 [StructLayout(LayoutKind.Sequential)] internal struct Report {public ushort Buttons;public byte LT,RT;public short LX,LY,RX,RY;}
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr LoadLibraryEx(string file,IntPtr reserved,uint flags);
 [DllImport("kernel32.dll")] static extern IntPtr GetProcAddress(IntPtr module,string name);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate IntPtr Allocate();
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate uint Connect(IntPtr client);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate uint Target(IntPtr client,IntPtr target);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate uint Update(IntPtr client,IntPtr target,Report report);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate void Free(IntPtr ptr);
 const uint Success=0x20000000;
 IntPtr client,target;Target remove;Free freeTarget,disconnect,freeClient;Update update;bool attached;
 public bool Ready {get;private set;}
 static T Function<T>(IntPtr dll,string name)where T:class{IntPtr p=GetProcAddress(dll,name);if(p==IntPtr.Zero)throw new Exception(name+" missing");return Marshal.GetDelegateForFunctionPointer(p,typeof(T))as T;}
 internal static void CheckLibrary(){string file=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ViGEmClient.dll");IntPtr dll=LoadLibraryEx(file,IntPtr.Zero,0x100|0x1000);if(dll==IntPtr.Zero)throw new Exception("ViGEmClient load failed: "+Marshal.GetLastWin32Error());Function<Update>(dll,"vigem_target_x360_update");IntPtr c=Function<Allocate>(dll,"vigem_alloc")();if(c==IntPtr.Zero)throw new Exception("ViGEm allocation failed");Function<Free>(dll,"vigem_free")(c);}
 public XboxInput(){
  if(Environment.GetEnvironmentVariable("VECTOR_SMOKE")=="1"){Console.WriteLine("XBOX|0");return;}
  try{string file=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ViGEmClient.dll");IntPtr dll=LoadLibraryEx(file,IntPtr.Zero,0x100|0x1000);if(dll==IntPtr.Zero)throw new Exception("ViGEmClient.dll unavailable");
   freeClient=Function<Free>(dll,"vigem_free");disconnect=Function<Free>(dll,"vigem_disconnect");freeTarget=Function<Free>(dll,"vigem_target_free");remove=Function<Target>(dll,"vigem_target_remove");update=Function<Update>(dll,"vigem_target_x360_update");
   client=Function<Allocate>(dll,"vigem_alloc")();uint result=Function<Connect>(dll,"vigem_connect")(client);if(result!=Success){Dispose();Console.WriteLine("XBOX|0");return;}
   target=Function<Allocate>(dll,"vigem_target_x360_alloc")();result=Function<Target>(dll,"vigem_target_add")(client,target);if(result!=Success){Dispose();Console.WriteLine("XBOX|0");return;}attached=true;Ready=true;Send(new Report());
  }catch(Exception e){Dispose();Console.WriteLine("ERROR|Xbox: "+e.Message);}
  Console.WriteLine("XBOX|"+(Ready?"1":"0"));
 }
 internal static short Stick(double value){return (short)Math.Round(Math.Max(-1,Math.Min(1,value))*(value<0?32768:32767));}
 internal static Report Make(double lx,double ly,double rx,double ry,double lt,double rt,HashSet<int> buttons){
  Report r=new Report();r.LX=Stick(lx);r.LY=Stick(ly);r.RX=Stick(rx);r.RY=Stick(ry);r.LT=(byte)Math.Round(Math.Max(0,Math.Min(1,lt))*255);r.RT=(byte)Math.Round(Math.Max(0,Math.Min(1,rt))*255);
  ushort[] masks={0x1000,0x2000,0x4000,0x8000,0x0100,0x0200,0x0020,0x0010,0x0040,0x0080,0x0001,0x0002,0x0004,0x0008};foreach(int b in buttons){if(b>=1&&b<=14)r.Buttons|=masks[b-1];if(b==15)r.LT=255;if(b==16)r.RT=255;}return r;
 }
 public void Send(Report report){if(!Ready)return;uint result=update(client,target,report);if(result!=Success){Ready=false;Console.WriteLine("XBOX|0");Console.WriteLine("ERROR|Xbox controller disconnected. Restart Host.");}}
 public void Neutral(){Send(new Report());}
 public void Dispose(){if(client!=IntPtr.Zero&&target!=IntPtr.Zero){if(Ready)Neutral();if(attached){remove(client,target);attached=false;}freeTarget(target);target=IntPtr.Zero;}if(client!=IntPtr.Zero){disconnect(client);freeClient(client);client=IntPtr.Zero;}Ready=false;}
}
