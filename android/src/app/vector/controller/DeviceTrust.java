package app.vector.controller;

import android.content.*;import android.security.keystore.*;import android.util.Base64;
import org.json.*;import java.nio.charset.StandardCharsets;import java.security.KeyStore;import java.util.*;
import javax.crypto.*;import javax.crypto.spec.GCMParameterSpec;

/** Metadata may be displayed in the WebView; long-lived tokens never leave this class as UI data. */
public final class DeviceTrust {
 private final SharedPreferences prefs;private static final String ALIAS="vector-trusted-pcs-v1";
 public DeviceTrust(Context c){prefs=c.getSharedPreferences("vector-trusted-pcs",0);if(!prefs.contains("deviceId"))prefs.edit().putString("deviceId",UUID.randomUUID().toString().replace("-","")).commit();}
 public String deviceId(){return prefs.getString("deviceId","");}
 private javax.crypto.SecretKey key()throws Exception{KeyStore ks=KeyStore.getInstance("AndroidKeyStore");ks.load(null);if(!ks.containsAlias(ALIAS)){KeyGenerator gen=KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES,"AndroidKeyStore");gen.init(new KeyGenParameterSpec.Builder(ALIAS,KeyProperties.PURPOSE_ENCRYPT|KeyProperties.PURPOSE_DECRYPT).setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE).setKeySize(256).build());gen.generateKey();}return (javax.crypto.SecretKey)ks.getKey(ALIAS,null);}
 private JSONObject rows(){try{return new JSONObject(prefs.getString("hosts","{}"));}catch(Exception e){return new JSONObject();}}
 public synchronized JSONObject get(String id){try{JSONObject row=rows().optJSONObject(id);if(row==null)return null;Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.DECRYPT_MODE,key(),new GCMParameterSpec(128,Base64.decode(row.getString("iv"),Base64.NO_WRAP)));String token=new String(cipher.doFinal(Base64.decode(row.getString("encrypted"),Base64.NO_WRAP)),StandardCharsets.US_ASCII);if(!token.matches("[a-f0-9]{64}"))return null;return new JSONObject(row.toString()).put("token",token);}catch(Exception e){return null;}}
 public synchronized void save(String hostId,String name,String address,int port,String token)throws Exception{if(!hostId.matches("[a-f0-9]{32}")||!token.matches("[a-f0-9]{64}"))throw new Exception("Неправильний ключ ПК");JSONObject all=rows();if(!all.has(hostId)&&all.length()>=32)throw new Exception("Збережено 32 ПК. Видали зайвий.");Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.ENCRYPT_MODE,key());JSONObject row=new JSONObject().put("hostId",hostId).put("name",name).put("address",address).put("port",port).put("iv",Base64.encodeToString(cipher.getIV(),Base64.NO_WRAP)).put("encrypted",Base64.encodeToString(cipher.doFinal(token.getBytes(StandardCharsets.US_ASCII)),Base64.NO_WRAP));all.put(hostId,row);if(!prefs.edit().putString("hosts",all.toString()).putString("lastHost",hostId).commit())throw new Exception("Не вдалося зберегти ПК");}
 public synchronized JSONArray list(){JSONArray out=new JSONArray();JSONObject all=rows();for(Iterator<String> it=all.keys();it.hasNext();){String id=it.next();JSONObject r=all.optJSONObject(id);if(r!=null)try{out.put(new JSONObject().put("hostId",id).put("name",r.optString("name","VECTOR Host")).put("address",r.optString("address")).put("port",r.optInt("port",26760)));}catch(Exception ignored){}}return out;}
 public synchronized void forget(String id){JSONObject all=rows();all.remove(id);prefs.edit().putString("hosts",all.toString()).apply();if(id.equals(lastHost()))prefs.edit().remove("lastHost").apply();}
 public String lastHost(){return prefs.getString("lastHost","");}
}
