# BiubiuClick  

����һ��������Windowsƽ̨�Ա�������Ƶʱ����ͬ�����ŵĹ��ߣ�����������һ������������Ƶ��

![BiuBiuClick](BiuBiuClick.png)
# ����  
* Ĭ��֧�� [PotPlayer](https://potplayer.daum.net/) ������
* һ����������������ͬʱ���š���ͣ��ֹͣ
* һ������Ļƽ��չʾ��������
* һ�������������ƶ���������Ļ�ϣ�����ȫ��

# ����  

* Windows 10
* ��Ҫ��װ .Net Framework 4.7.2 ([΢��ٷ�����](https://support.microsoft.com/en-us/topic/microsoft-net-framework-4-7-2-offline-installer-for-windows-05a72734-2127-a15d-50cf-daf56d5faec2))

# ԭ��  

ͨ�����Ϳ�ݼ�������ʵ�ֿ��Ʋ�������

# ��չ��  

* �����Ͽ�֧������֧�ֿ�ݼ��Ĳ����� 
* ������ö�̬��������ʵ�ֿ���չ�ԣ��û�����ͨ����������ʵ�ֶ�������������֧��
 
# ��Ӳ�����֧�� 

ͨ�����������ļ��ķ�ʽ�������������֧�֣�  

1. �������װĿ¼
2. app/config/ ���½��ļ��У����ò�������Ӣ���������������� demoplayer
3. �����½���Ŀ¼
4. ��ӿ��ư�ťͼƬ��ͼƬ��֧��png��ʽ��һ��ͼƬ��ʾһ����ť��ͼƬ��������Ϊ������������-��ť���ơ�������ȫ��ʹ��Ӣ�ģ�����potplayer-play.png
5. �½������ļ� config.ini��Ҳ���Ը��� app/config/potplayer/config.ini 
6. �༭config.ini�ļ����ݣ���������

``` ini
[common]
; className Ҫ����Ĵ����࣬ʹ��AutoIt Window Info�����ص�ַ��https://www.autoitscript.com/site/autoit/downloads/�� ��ȡ�����࣬��Ҫ�뵱Ȼ����
className=PotPlayer
    ; macthTitleFromRight �Ƿ���Ҳ�ƥ�䴰�ڱ��⣬0:�����ƥ�� 1:���Ҳ�ƥ��
macthClassNameFromRight=1
; processName ��������
processName=PotPlayerMini
; keys ��ݼ�����
; ��Ӧ�������Ŀ�ݼ������������Ŀ�ݼ�ת��Ϊ�������������
; ע���ݼ��벥������ݼ�����һ�£��޸Ĳ�������ݼ�����Ҫ�޸Ĵ˴����ã�����������ƹ��ܻ�ʧЧ
; ��ֵʹ��˫����������
; �����ΰ��Ŀ�ݼ�ʹ��Ӣ�Ķ��ŷָ����м䲻Ҫ�ӿո�
; ��ϼ���SHIFT	��Ӧ"+"��CTRL��Ӧ"^"��ALT��Ӧ"%"��CTRL+F1�ļ�ֵΪ"^{F1}"
; Ҫָ���ڰ��¼���������ʱӦ��ס SHIFT��CTRL �� ALT ��������ϣ��뽫��Щ���Ĵ������������С�
; ���磬Ҫָ���ڰ��� E �� C ʱ��ס SHIFT����ʹ�á�+(EC)����Ҫָ���ڰ��� E ʱ��ס SHIFT��Ȼ���� C ����ʹ�� SHIFT����ʹ�á�+EC����
; �ո����һ���ո�������ֵ�Ķ���� https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?view=windowsdesktop-6.0
[keys]
; ��ͣ �ո�
pause= 
; ���� �ո�
play= 
; ֹͣ potplayer��ֹͣ��ݼ���ʹ����ͣ�Ͷ�λ��0֡ģ��ֹͣ���������õĺ����ǣ��Ȱ��ո��ٰ��˸��
stop= ,{BACKSPACE}
; ���� ALT+F1
property=^{F1}
```   

7. app��������Ŀ¼�ṹΪ   

``` 
+---app
|   \---config
|       \---potplayer
|               config.ini
|               potplayer-pause.png
|               potplayer-play.png
|               potplayer-property.png
|               potplayer-stop.png
```

# ˵��  

* Ŀǰ����Windows 10�²��Թ���������Windows�汾������δ֪��

# ��֪����  

* [ ] ������ʾ����ʾ������˳����أ�����Ѿ�����������ʾ�����������޷���������������***ϵͳ����/��ʾ����*** �� **����������ʾ��˳��**���ԡ�
* [ ] ������Ƶͬ�����Ŵ��ڼ��ٺ��������ӳ١� 
 
# TODO

* [ ] ���ڡ�����������ʾ�Ż�  
* [ ] �Զ�����
 


