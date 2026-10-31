import app.vector.controller.SessionCrypto;
public final class SessionCryptoTest {
 static String repeat(char c){char[] s=new char[32];java.util.Arrays.fill(s,c);return new String(s);}
 public static void main(String[] args)throws Exception{byte[] token=new byte[32];java.util.Arrays.fill(token,(byte)1);byte[] key=SessionCrypto.derive(token,repeat('a'),repeat('b'),repeat('c'),repeat('d'),repeat('e'));StringBuilder hex=new StringBuilder();for(byte b:key)hex.append(String.format("%02x",b&255));if(!hex.toString().equals(args[0]))throw new AssertionError("PC / Android session keys differ");boolean rejected=false;try{SessionCrypto.derive(token,"bad",repeat('b'),repeat('c'),repeat('d'),repeat('e'));}catch(IllegalArgumentException e){rejected=true;}if(!rejected)throw new AssertionError("Invalid IDs accepted");System.out.println("PASS PC / Android trusted session derivation and ID validation");}
}
