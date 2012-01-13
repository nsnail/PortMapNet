使用说明:
编辑PortMapNet.exe.config, 修改appSettings节下的portmaps.
格式为:
侦听本机IP:端口|映射至主机IP或HOST:端口;第二组;第三组...
如:
*:2012|10.10.1.18:3389;*:2013|www.beta-1.cn:80;

附: "*" 表示绑定本机所有IP.