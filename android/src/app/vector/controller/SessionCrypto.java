package app.vector.controller;
import java.nio.charset.StandardCharsets;import javax.crypto.Mac;import javax.crypto.spec.SecretKeySpec;
/** Same domain-separated derivation as the PC, independent of Android APIs. */
public final class SessionCrypto {
 public static byte[] derive(byte[] token,String host,String device,String request,String nonce,String session)throws Exception{for(String id:new String[]{host,device,request,nonce,session})if(!id.matches("[a-f0-9]{32}"))throw new IllegalArgumentException("Invalid reconnect identifier");Mac mac=Mac.getInstance("HmacSHA256");mac.init(new SecretKeySpec(token,"HmacSHA256"));return mac.doFinal(("VECTOR-SESSION-v1|"+host+"|"+device+"|"+request+"|"+nonce+"|"+session).getBytes(StandardCharsets.UTF_8));}
}
