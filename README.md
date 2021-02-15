# RTSCamera
调用CameraRig中的PanCamera(Vector3 panDelta)方法即可移动相机

示例代码就写在RSTCamera.cs里了(用wasd移动)

使用时 CameraRig 和RSTCamera 挂载在相机上

相机下需要挂载一个空物体,位置角度都为0

![在这里插入图片描述](https://img-blog.csdnimg.cn/20210215190321270.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3Nkd3N5bnM=,size_16,color_FFFFFF,t_70#pic_center)

CameraRigEditor记得放在Editor文件内

边框限制:点相机,然后就可以在Scene窗口直接调矩形大小

原理不需要懂,用就完事了()
