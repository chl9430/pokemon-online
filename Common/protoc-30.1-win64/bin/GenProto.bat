protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
IF ERRORLEVEL 1 PAUSE

REM START ../../../Server/PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto
REM XCOPY /Y Protocol.cs "../../../Client/Assets/Scripts/Packet"
REM XCOPY /Y Protocol.cs "../../../Server/Server/Packet"
REM XCOPY /Y ClientPacketManager.cs "../../../Client/Assets/Scripts/Packet"
REM XCOPY /Y ServerPacketManager.cs "../../../Server/Server/Packet"